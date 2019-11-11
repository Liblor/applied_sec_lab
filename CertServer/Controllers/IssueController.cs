using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using CertServer.Models;
using CertServer.DataModifiers;
using CoreCA.DataModel;
using Org.BouncyCastle.Asn1.X509;

namespace CertServer.Controllers
{
    [ApiController, Route("api")]

    public class IssueController : ControllerBase
    {
        private readonly CADBModifier _caDBModifier;
        private readonly UserDBAuthenticator _userDBAuthenticator;
        private readonly ILogger _logger;

        public IssueController(
            CADBModifier caDBModifier,
            UserDBAuthenticator userDBAuthenticator,
            ILogger<IssueController> logger
        )
        {
            _caDBModifier = caDBModifier;
            _userDBAuthenticator = userDBAuthenticator;
            _logger = logger;
        }

        /// <summary>
        /// Request to issue a new certificate.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/issue
        ///     {
        ///            "uid": "ab",
        ///            "password": "plain",
        ///            "certPassphrase": "enterSomeUniquePassphraseHere",
        ///            "requestedCipherSuite": {
        ///                "Alg": "RSA",
        ///                "HashAlg": "SHA512",
        ///                "KeySize": 4096
        ///            }
        ///     }
        ///
        /// </remarks>
        /// <param name="certRequest"></param>
        /// <returns>
        ///        Returns a pkcs 12 archive which includes the issued certificate as well as
        ///        the users private key.
        /// </returns>
        /// <response code="200">Certificate generation was successful</response>
        /// <response code="400">Invalid cipher suite</response>
        /// <response code="401">Unauthorized request</response>
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [HttpPost("[controller]")]
        public IActionResult IssueCertificate(CertRequest certRequest)
        {
            CipherSuite cipherSuite = certRequest.RequestedCipherSuite;

            if (cipherSuite.IsValidCipherSuite())
            {
                User user = _userDBAuthenticator.AuthenticateAndGetUser(certRequest.Uid, certRequest.Password);

                if (user != null)
                {
                    // Load the certificate
                    X509Certificate2 coreCACert = new X509Certificate2(CAConfig.CoreCACertPath);

                    HashAlgorithmName hashAlg = new HashAlgorithmName(cipherSuite.HashAlg);
                    AsymmetricAlgorithm privKey = null;

                    CertificateRequest req = null;

                    PbeParameters pbeParameters = new PbeParameters(
                        PbeEncryptionAlgorithm.Aes256Cbc,
                        HashAlgorithmName.SHA512,
                        10000
                    );

                    if (cipherSuite.Alg.Equals("RSA"))
                    {
                        privKey = RSA.Create(cipherSuite.KeySize);

                        req = new CertificateRequest(
                            "CN=" + user.Id,
                            (RSA) privKey,
                            hashAlg,
                            RSASignaturePadding.Pss
                        );
                    }
                    else if (cipherSuite.Alg.Equals("ECDSA"))
                    {
                        privKey = ECDsa.Create();

                        req = new CertificateRequest(
                            "CN=" + user.Id,
                            (ECDsa) privKey,
                            hashAlg
                        );
                    }

                    // Add email as SAN
                    SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
                    sanBuilder.AddEmailAddress(user.Email);
                    req.CertificateExtensions.Add(sanBuilder.Build());

                    // Arguments: Is no CA, no restricted nr of path levels, (nr of path levels), is not critical
                    req.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false)
                    );

                    req.CertificateExtensions.Add(
                        new X509SubjectKeyIdentifierExtension(req.PublicKey, false)
                    );

                    req.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.KeyEncipherment
                            | X509KeyUsageFlags.DigitalSignature
                            | X509KeyUsageFlags.NonRepudiation,
                            false
                        )
                    );

                    OidCollection oidCollection = new OidCollection();
                    // Set Client Authentication Oid
                    oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.2"));
                    // Set Secure Email / Email protection Oid
                    oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.4"));

                    req.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                            oidCollection,
                            false
                        )
                    );

                    // Add CRL Distribution Point (CDP)
                    req.CertificateExtensions.Add(
                        new System.Security.Cryptography.X509Certificates.X509Extension(
                            new Oid(X509Extensions.CrlDistributionPoints.Id),
                            new CrlDistPoint(
                                new[] { 
                                    new DistributionPoint(
                                        new DistributionPointName(
                                            new GeneralNames(
                                                new GeneralName(
                                                    GeneralName.UniformResourceIdentifier,
                                                    // Trim the 'aslcert' part of the hostname
                                                    string.Format(CAConfig.CrlDistributionPointFormatString, Environment.MachineName.Substring(7))
                                                    )
                                                )
                                            ), 
                                        null, 
                                        null
                                    )
                                }
                            ).GetDerEncoded(),
                            false
                        )
                    );

                    // Use transaction to prevent race conditions on the serial number
                    X509Certificate2 userCert;

                    using (
                        IDbContextTransaction scope = _caDBModifier.GetScope()
                    )
                    {
                        SerialNumber serialNr = _caDBModifier.GetMaxSerialNr();

                        // It is necessary to use this constructor to be able to sign keys
                        // that use different algorithms than the one for the core CA's key
                        userCert = req.Create(
                            coreCACert.SubjectName,
                            X509SignatureGenerator.CreateForRSA(
                                (RSA) coreCACert.PrivateKey,
                                RSASignaturePadding.Pss
                            ),
                            DateTimeOffset.UtcNow,
                            DateTimeOffset.UtcNow.AddDays(CAConfig.UserCertValidityPeriod),
                            serialNr.SerialNrBytes
                        );

                        // Revoke all other certificates of this user
                        _caDBModifier.RevokeAllCertificatesOfUser(user);

                        // Add certificate to DB
                        _caDBModifier.AddCertificate(
                            new PublicCertificate {
                                SerialNr = serialNr.SerialNr,
                                Uid = user.Id,
                                Certificate = Convert.ToBase64String(
                                    userCert.Export(X509ContentType.Pkcs12)
                                ),
                                IsRevoked = false
                            }
                        );

                        scope.Commit();
                    }

                    // XXX: Send privKeyExport to backup server

                    // Create pkcs12 file including the user certificate and private key
                    Pkcs12Builder pkcs12Builder = new Pkcs12Builder();

                    Pkcs12SafeContents pkcs12Cert = new Pkcs12SafeContents();
                    pkcs12Cert.AddCertificate(userCert);
                    pkcs12Cert.AddCertificate(coreCACert);
                    pkcs12Builder.AddSafeContentsUnencrypted(pkcs12Cert);

                    Pkcs12SafeContents pkcs12PrivKey = new Pkcs12SafeContents();
                    pkcs12PrivKey.AddShroudedKey(
                        privKey,
                        certRequest.CertPassphrase,
                        pbeParameters
                    );

                    pkcs12Builder.AddSafeContentsUnencrypted(pkcs12PrivKey);

                    pkcs12Builder.SealWithMac(
                        certRequest.CertPassphrase,
                        HashAlgorithmName.SHA512,
                        10000
                    );

                    // Since the size of the pkcs12 encoding is unknown,
                    // we might need to retry
                    Span<Byte> pkcs12ArchiveRaw;
                    int pkcs12ArchiveSize = 1024;
                    int bytesWritten = 0;

                    do {
                        pkcs12ArchiveSize *= 2;
                        pkcs12ArchiveRaw = new byte[pkcs12ArchiveSize];

                    } while(!pkcs12Builder.TryEncode(pkcs12ArchiveRaw, out bytesWritten));

                    string pkcs12ArchiveB64 = Convert.ToBase64String(
                        pkcs12ArchiveRaw.ToArray().Take(bytesWritten).ToArray()
                    );

                    // Add encrypted private key to DB
                    _caDBModifier.AddPrivateKey(
                        new PrivateKey {
                            Uid = user.Id,
                            KeyPkcs12 = pkcs12ArchiveB64
                        }
                    );

                    _logger.LogInformation("Successfully issued new certificate for user " + user.Id);

                    return Ok(
                        new UserCertificate {
                            Pkcs12Archive = pkcs12ArchiveB64
                        }
                    );
                }
                else {
                    _logger.LogWarning(
                        "Unauthorized attempt to issue certificate for user "
                        + certRequest.Uid
                    );
                    return Unauthorized();
                }
            }
            else
            {
                _logger.LogWarning("Invalid cipher suite:\n" + cipherSuite);
                return BadRequest("Invalid cipher suite.");
            }
        }
    }
}
