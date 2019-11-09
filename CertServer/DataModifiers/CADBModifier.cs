using Microsoft.EntityFrameworkCore.Storage;
using System;
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

        public CADBModifier(IMoviesCAContext dbContext)
        {
            _dbContext = dbContext;
        }

		/*
		 * Public certificates functionality
		 */
		public void AddCertificate(PublicCertificate publicCert) 
		{
			_dbContext.PublicCertificates.Add(publicCert);
			_dbContext.SaveChanges();
		}

		public void RevokeAllCertificatesOfUser(User user)
		{
			var unrevokedCertificates = _dbContext.PublicCertificates.Where(
				p => p.Uid.Equals(user.Id) && !p.IsRevoked
			);

			foreach (PublicCertificate pubCert in unrevokedCertificates)
			{
                if (DateTime.Now > pubCert.Parse().NotAfter)
                    continue;
				pubCert.IsRevoked = true;
			}

			_dbContext.SaveChanges();
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

			// XXX: Log error, notify system admin to generate new certificates and make a fresh start?
			// if (maxSerialNr > threshold * ulong.MaxValue) {}
			
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
            crlGen.SetNextUpdate(now.AddMinutes(10));

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
		}

		public PrivateKey GetPrivateKey(User user)
		{
			return _dbContext.PrivateKeys.Find(user.Id);
		}
    }
}
