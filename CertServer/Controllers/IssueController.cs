using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using CertServer.Models;

// XXX: For testing, generate a self signed root certificate with:
// openssl ecparam -genkey -name prime256v1 -out core_ca_key.pem
// openssl req -new -sha256 -x509 -config openssl.cnf -key core_ca_key.pem -out core_ca_cert.pem

// Combine certificate and privat key in one file
// openssl pkcs12 -export -out core_ca_pub_priv_keys.pfx -inkey core_ca_key.pem -in core_ca_cert.pem -passout pass:TEST

// Convert key to pkcs8 format, which is one of the few that C# should actually be able to import.
// openssl pkcs8 -topk8 -nocrypt -in core_ca_key.pem -out core_ca_key_pkcs8.pem

namespace CertServer.Controllers
{
	[ApiController, Route("api")]

    public class IssueController : ControllerBase
    {
		private User GetUser(string user, string password)
		{
			// XXX: Implement user authentication against DB
			if (true) 
			{
				return new User {
					Uid = "testuser",
					FirstName = "Test",
					LastName = "User",
					Email = ""
				};
			}
			else
			{
				return null;
			}
		}

		private bool IsVaildCipherSuite(CipherSuite cipherSuite)
		{
			return Array.Exists(CAConfig.CipherSuites, elem => elem.Equals(cipherSuite));
		}

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
		/// <returns>Private key as well as the certificate for the public key</returns>
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

			if (IsVaildCipherSuite(cipherSuite))
			{
				User user = GetUser(certRequest.Uid, certRequest.Password);

				if (user != null)
				{
					// Load the certificate
					// XXX: @Loris, do you see an advantage in storing the certificate password protected,
					// and hard coding the password here?
					// XXX: [added later] This version requires a password, the empty password does not work for reasons
					X509Certificate2 coreCACert = new X509Certificate2(CAConfig.CoreCACertPath, CAConfig.CoreCACertPW);

					// XXX: The following code should import a private key from pkcs8 format.
					// It looks ugly, but this is because c# doesn't support this well.
					// However, the last assignement is not available on macOS. Thus I search for
					// an alternative.
					
					// var ecdsaAlg = ECDsa.Create();
					//
					// // Read private key in pkcs8 format, strip prefix and suffix and remove newlines
					// var coreCAPrivKeyP8 = String.Join("", 
					// 	System.IO.File.ReadLines(CAConfig.CoreCAPrivKeyPath).Where(
					// 		l => !l.Contains("---")
					// 	)
					// ).Replace("\n", String.Empty);
					//
					// byte[] coreCAPrivKeyBytes = System.Convert.FromBase64String(coreCAPrivKeyP8);
					// ecdsaAlg.ImportPkcs8PrivateKey(coreCAPrivKeyBytes, out int _);
					// coreCACert.PrivateKey = ecdsaAlg;

					HashAlgorithmName hashAlg = new HashAlgorithmName(cipherSuite.HashAlg);
					CertificateRequest req = null; 
					if (cipherSuite.Alg == "RSA")
					{
						// XXX: @Loris, use DB userid as CN name (because that is unique), is that ok?
						req = new CertificateRequest(
							"CN=" + user.Uid,
							RSA.Create(cipherSuite.KeySize),
							hashAlg,
							RSASignaturePadding.Pss
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

					// XXX: @Loris, how long should the certificate be valid? (3rd parameter, 
					// currently randomly chosen 90 days)
					// XXX: This throws a "The certificate key algorithm is not supported." error
					// maybe we have to try with a different algorithm for our core CA.
					X509Certificate2 userCert = req.Create(
						coreCACert.IssuerName,
						X509SignatureGenerator.CreateForECDsa( (ECDsa) coreCACert.PrivateKey),
						DateTimeOffset.UtcNow.AddDays(-1),
            			DateTimeOffset.UtcNow.AddDays(90),
						GetNextSerialNumber()
					);

					// XXX: The version below complains that the algorithms in the CA cert and the 
					// CSR are different.

					// X509Certificate2 userCert = req.Create(
					// 	coreCACert,
					// 	DateTimeOffset.UtcNow.AddDays(-1),
            		// 	DateTimeOffset.UtcNow.AddDays(90),
					// 	GetNextSerialNumber()
					// );

					Console.WriteLine(
						userCert.Export(X509ContentType.Pfx, "")
					);
					return Ok(CAConfig.CipherSuites);
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