using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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

        // Send private key, encrypted with the public key of the backup server, to the backup
        private void BackupPrivateKey(string privKeyExport, string fileName)
        {
            X509Certificate2 backupServerCert = new X509Certificate2(CAConfig.BackupServerCertPath);

            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.Build(backupServerCert);

                // OaepSHA1 instead of OaepSHA512 padding is used because OpenSSL only supports the former,
                // which would make key recovery more difficult for system administrators.
                byte[] privKeyEncrypted = ((RSA) backupServerCert.PublicKey.Key).Encrypt(
                    Encoding.ASCII.GetBytes(privKeyExport),
                    RSAEncryptionPadding.OaepSHA1
                );

                System.IO.File.WriteAllBytes(
                    CAConfig.BackupFolder + fileName,
                    privKeyEncrypted
                );
            }
            Process.Start(
                "/usr/bin/bash",
                string.Format("-c \"sudo {0}\"", CAConfig.BackupScript)
            );
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
            if (certRequest.CertPassphrase.Length < Constants.MinPassphraseLength)
            {
                _logger.LogWarning(
                    string.Format(
                        "User {} tried to issue new certificate with too short passphrase.",
                        certRequest.Uid
                    )
                );
                return BadRequest("Too short passphrase");
            }

            CipherSuite cipherSuite = certRequest.RequestedCipherSuite;
            if (!cipherSuite.IsValidCipherSuite())
            {
                _logger.LogWarning("Invalid cipher suite:\n" + cipherSuite);
                return BadRequest("Invalid cipher suite.");
            }

            User user = _userDBAuthenticator.AuthenticateAndGetUser(certRequest.Uid, certRequest.Password);
            if (user != null)
            {
                // Load the certificate
                X509Certificate2 coreCACert = new X509Certificate2(CAConfig.CoreCACertPath);

                HashAlgorithmName hashAlg = new HashAlgorithmName(cipherSuite.HashAlg);
                AsymmetricAlgorithm privKey = null;
                string privKeyExport = null;

                CertificateRequest req = null;

                if (cipherSuite.Alg.Equals(EncryptionAlgorithms.RSA))
                {
                    privKey = RSA.Create(cipherSuite.KeySize);
                    privKeyExport = ((RSA) privKey).ToPem();

                    req = new CertificateRequest(
                        "CN=" + user.Id,
                        (RSA) privKey,
                        hashAlg,
                        RSASignaturePadding.Pss
                    );
                }
                else if (cipherSuite.Alg.Equals(EncryptionAlgorithms.ECDSA))
                {
                    privKey = ECDsa.Create();
                    privKeyExport = ((ECDsa) privKey).ToPem();

                    req = new CertificateRequest(
                        "CN=" + user.Id,
                        (ECDsa) privKey,
                        hashAlg
                    );
                }
                else
                {
                    _logger.LogError(
                        string.Format(
                            "Unknown encryption algorithm '{0}'.",
                            cipherSuite.Alg
                        )
                    );
                    throw new CryptographicUnexpectedOperationException();
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
                                                CAConfig.CrlDistributionPoint
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

                X509Certificate2 coreCaPublicCert = new X509Certificate2(coreCACert.Export(X509ContentType.Cert));
                if (coreCaPublicCert.HasPrivateKey)
                {
                    _logger.LogError("Core CA public certificate exported with private key!");
                    throw new CryptographicUnexpectedOperationException();
                }

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

                    _caDBModifier.RevokeAllCertificatesOfUser(user);

                    // Add certificate to DB
                    _caDBModifier.AddCertificate(
                        new PublicCertificate {
                            SerialNr = serialNr.SerialNr,
                            Uid = user.Id,
                            Certificate = Convert.ToBase64String(
                                userCert.Export(X509ContentType.Cert)
                            ),
                            IsRevoked = false
                        }
                    );

                    scope.Commit();
                }

                var collection = new X509Certificate2Collection();

                X509Certificate2 userCertWithPrivKey;
                if (privKey is RSA rsa)
                    userCertWithPrivKey = userCert.CopyWithPrivateKey(rsa);
                else if (privKey is ECDsa dsa)
                    userCertWithPrivKey = userCert.CopyWithPrivateKey(dsa);
                else
                    throw new CryptographicUnexpectedOperationException();

                collection.Add(userCertWithPrivKey);
                collection.Add(coreCaPublicCert);

                BackupPrivateKey(privKeyExport, user.Id + '_' + userCert.Thumbprint + ".pem.enc");

                byte[] certBytes = collection.Export(X509ContentType.Pkcs12, certRequest.CertPassphrase);
                string pkcs12ArchiveB64 = Convert.ToBase64String(certBytes);

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
    }
}