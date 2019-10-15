using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using CertServer.Models;
using CertServer.Authentication;

// XXX: For testing, generate a self signed root certificate with:

// ECC:
// openssl ecparam -genkey -name prime256v1 -out core_ca_key.pem
// openssl req -new -sha256 -x509 -days 365 -config openssl.cnf -key core_ca_key.pem -out core_ca_cert.pem

// Combine certificate and privat key in one file
// openssl pkcs12 -export -out core_ca_pub_priv_keys.pfx -inkey core_ca_key.pem -in core_ca_cert.pem -passout pass:TEST

// RSA:
// openssl genrsa -out core_ca_key.pem 4096
namespace CertServer.Controllers
{
	[ApiController, Route("api")]

    public class IssueController : ControllerBase
    {
		// XXX: Must be implemented. Using local DB? Or fetch largest serial number 
		// from pub key DB? (trusting it but not having to worry about backups)
		private byte[] GetNextSerialNumber()
		{
			return new byte[] { 1, 3, 3, 7 };
		}

		/// <summary>
		/// Request to issue a new certificate.
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     POST /api/issue
		///     {
		///        	"uid": "ab",
		///			"password": "plain",
		///			"requestedCipherSuite": {
		///				"Alg": "RSA",
		///				"HashAlg": "SHA512",
		///				"KeySize": 4096
		///			}
		///     }
		///
		/// </remarks>
		/// <param name="certRequest"></param>
		/// <returns>
		///		Private key as well as the certificate for the public key, 
		///		both encoded in base 64
		/// </returns>
		/// <response code="200">Certificate generation was successful</response>
		/// <response code="400">Invalid cipher suite.</response>
		/// <response code="401">Unauthorized request</response>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[HttpPost("[controller]")]
		public IActionResult IssueCertificate(CertRequest certRequest)
		{
			CipherSuite cipherSuite = certRequest.RequestedCipherSuite;

			if (CipherSuiteHelper.IsVaildCipherSuite(cipherSuite))
			{
				User user = UserDBAuthenticator.GetUser(certRequest.Uid, certRequest.Password);

				if (user != null)
				{
					// Load the certificate
					// XXX: @Loris, do you see an advantage in storing the certificate password protected,
					// and hard coding the password here?
					// XXX: [added later] This version requires a password, the empty password does not work for reasons
					X509Certificate2 coreCACert = new X509Certificate2(CAConfig.CoreCACertPath);

					HashAlgorithmName hashAlg = new HashAlgorithmName(cipherSuite.HashAlg);
					byte[] privKeyExport = null;
					CertificateRequest req = null; 
					if (cipherSuite.Alg == "RSA")
					{
						RSA privKey = RSA.Create(cipherSuite.KeySize);
						privKeyExport = privKey.ExportRSAPrivateKey();
						// XXX: @Loris, use DB userid as CN name (because that is unique), is that ok?
						req = new CertificateRequest(
							"CN=" + user.Uid,
							privKey,
							hashAlg,
							RSASignaturePadding.Pss
						);
					}
					else if (cipherSuite.Alg == "ECDSA")
					{
						ECDsa privKey = ECDsa.Create();
						privKeyExport = privKey.ExportECPrivateKey();
						req = new CertificateRequest(
							"CN=" + user.Uid,
							privKey,
							hashAlg
						);
					}

					req.CertificateExtensions.Add(
						// Arguments: Is no CA, no restricted nr of path levels, (nr of path levels), is not critical
            			new X509BasicConstraintsExtension(false, false, 0, false)
					);
					
					req.CertificateExtensions.Add(
            			new X509SubjectKeyIdentifierExtension(req.PublicKey, false)
					);

					// XXX: @Loris, agree with flags?
					req.CertificateExtensions.Add(
						new X509KeyUsageExtension(
							X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
							false
						)
					);

					// It is necessary to use this constructor to be able to sign keys
					// that use different algorithms than the one for the core CA's key
					X509Certificate2 userCert = req.Create(
						coreCACert.IssuerName,
						X509SignatureGenerator.CreateForRSA( 
							(RSA) coreCACert.PrivateKey,
							RSASignaturePadding.Pkcs1
						),
						DateTimeOffset.UtcNow,
            			DateTimeOffset.UtcNow.AddDays(CAConfig.UserCertValidityPeriod),
						GetNextSerialNumber()
					);

					// XXX: Send private key to backup server

					// XXX: Register public key in DB

					return Ok(
						new UserCertificate {
							PrivateKey = Convert.ToBase64String(privKeyExport),
							Certificate = Convert.ToBase64String(
								userCert.Export(X509ContentType.Pkcs12)
							),
						}
					);
				}
				else {
					return Unauthorized();
				}
			}
			else
			{
				return BadRequest("Invalid cipher suite.");
			}
		}
    }
}