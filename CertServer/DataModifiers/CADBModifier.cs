using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using CertServer.Data;
using CertServer.Models;
using CoreCA.DataModel;

namespace CertServer.DataModifiers
{
    public class CADBModifier
    {
        private readonly IMoviesCAContext _dbContext;
		private readonly ILogger _logger;

        public CADBModifier(IMoviesCAContext dbContext, ILogger<CADBModifier> logger)
        {
            _dbContext = dbContext;
			_logger = logger;
        }

		/*
		 * Public certificates functionality
		 */
		public void AddCertificate(PublicCertificate publicCert) 
		{
			_dbContext.PublicCertificates.Add(publicCert);

			_logger.LogInformation(
				string.Format(
					"Added new certificate of user {0} with serial number {1} to DB",
					publicCert.Uid,
					publicCert.SerialNr
				)
			);
			_dbContext.SaveChanges();
		}

		public void RevokeAllCertificatesOfUser(User user)
		{
			var unrevokedCertificates = _dbContext.PublicCertificates.Where(
				p => p.Uid.Equals(user.Id) && !p.IsRevoked
			);

			 List<ulong> logRevokedCertificates = new List<ulong>();
			foreach (PublicCertificate pubCert in unrevokedCertificates)
			{
                if (DateTime.Now > pubCert.Parse().NotAfter)
                    continue;
				pubCert.IsRevoked = true;
				logRevokedCertificates.Add(pubCert.SerialNr);
			}

			_dbContext.SaveChanges();

			_logger.LogInformation(
				string.Format(
					"Revoked all certificates (serial number(s) {0}) of user {1}",
					string.Join(", ", logRevokedCertificates),
					user.Id
				)
			);
		}

		public SerialNumber GetMaxSerialNr()
		{
			ulong newSerialNr;

			if (_dbContext.PublicCertificates.Any())
			{
				var serialNrs = _dbContext.PublicCertificates.Select(p => p.SerialNr);
				// Max() fails if only one serial number exists, although it shouldn't
				newSerialNr = (serialNrs.Count() == 1) ? serialNrs.First() + 1 : serialNrs.Max() + 1;
			}
			else
			{
				newSerialNr = 0;
			}

			if (newSerialNr > CAConfig.SerialNumberWarningThreshold * ulong.MaxValue) {
				_logger.LogWarning(
					string.Format(
						"More than {0}% of all available serial numbers are used. "
						+ "This CA service should be restarted from scratch before the numbers run out.",
						CAConfig.SerialNumberWarningThreshold * 100
					)
				);
			}
			
			return new SerialNumber {
				SerialNr = newSerialNr
			};
		}

		public IDbContextTransaction GetScope() 
		{
			return _dbContext.Database.BeginTransaction();
		}

		// XXX: Test if CRL signature is correct
		public string GenerateCRL()
		{
			var coreCApkcs = new Org.BouncyCastle.Pkcs.Pkcs12Store(
				new FileStream(CAConfig.CoreCACertPath, FileMode.Open, FileAccess.Read),
				"".ToCharArray()
			);

			string alias = System.Environment.MachineName;

			Org.BouncyCastle.X509.X509Certificate coreCaCert = coreCApkcs.GetCertificate(alias).Certificate;
			Org.BouncyCastle.X509.X509V2CrlGenerator crlGen = new Org.BouncyCastle.X509.X509V2CrlGenerator();

			DateTime now = DateTime.UtcNow;
            crlGen.SetIssuerDN(coreCaCert.SubjectDN);
            crlGen.SetThisUpdate(now);
            crlGen.SetNextUpdate(now.AddMinutes(CAConfig.CRLNextUpdatedIntervalMinutes));

			var revokedPubCerts = _dbContext.PublicCertificates.Where(p => p.IsRevoked);

			foreach (PublicCertificate pubCert in revokedPubCerts)
			{
				//XXX: add crlreason to pubCert DB
				crlGen.AddCrlEntry(
					new Org.BouncyCastle.Math.BigInteger(pubCert.GetSerialNumber().SerialNrBytes),
					now,
					Org.BouncyCastle.Asn1.X509.CrlReason.KeyCompromise
				);
			}

			crlGen.AddExtension(
				Org.BouncyCastle.Asn1.X509.X509Extensions.AuthorityKeyIdentifier,
				false,
                new Org.BouncyCastle.X509.Extension.AuthorityKeyIdentifierStructure(
					coreCaCert.GetPublicKey()
				)
			);

			Org.BouncyCastle.X509.X509Crl crl = crlGen.Generate(
				new Org.BouncyCastle.Crypto.Operators.Asn1SignatureFactory(
					"SHA512WITHRSA", 
					coreCApkcs.GetKey(alias).Key
				)
			);
			
			StringWriter crlPEM = new StringWriter();
			Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(crlPEM);
			pemWriter.WriteObject(crl);
			pemWriter.Writer.Close();

			_logger.LogInformation(
				string.Format(
					"Generated CRL at {0} valid for {1} minutes",
					now,
					CAConfig.CRLNextUpdatedIntervalMinutes
				)
			);

			return crlPEM.ToString();
		}


		/*
		 * Private certificates functionality
		 */
		public void AddPrivateKey(PrivateKey privKey)
		{
			// Transaction is necessary to prevent read before write race condition
			// when private keys for the same uids would be added simultaneously.
			using (
				IDbContextTransaction scope = GetScope()
			)
			{
				if (_dbContext.PrivateKeys.Any(p => p.Uid.Equals(privKey.Uid)))
				{
					_dbContext.PrivateKeys.Update(privKey);
				}
				else
				{
					_dbContext.PrivateKeys.Add(privKey);
				}
				_dbContext.SaveChanges();
				scope.Commit();
			}

			_logger.LogInformation(
				string.Format(
					"Added encrypted private key of user {0} to DB",
					privKey.Uid
				)
			);
		}

		public PrivateKey GetPrivateKey(User user)
		{
			_logger.LogInformation(
				"Retrieve encrypted private key of user " + user.Id
			);

			return _dbContext.PrivateKeys.Find(user.Id);
		}
    }
}
