using Microsoft.EntityFrameworkCore.Storage;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

using CertServer.Data;
using CertServer.Models;

namespace CertServer.DataModifiers
{
    public class PublicCertificatesDBModifier
    {
        private readonly IMoviesPublicCertificatesContext _dbContext;

        public PublicCertificatesDBModifier(IMoviesPublicCertificatesContext dbContext)
        {
            _dbContext = dbContext;
        }

		public void AddCertificate(PublicCertificate publicCert) 
		{
			_dbContext.PublicCertificates.Add(publicCert);
			_dbContext.SaveChanges();
		}

		public bool RevokeCertificate(User user, ulong serialNr) 
		{
			PublicCertificate publicCert = _dbContext.PublicCertificates.Find(serialNr);

			if (publicCert == null)
			{
				// XXX: Implement logging
				// logger.LogWarning("Tried to revoke inexistant certificate of user with UID " + user.Uid);
				return false;
			}

			if (user.Uid.Equals(publicCert.Uid)) {
				publicCert.IsRevoked = true;
				_dbContext.SaveChanges();
				return true;
			}
			else {
				// XXX: Implement logging
				// logger.LogWarning(
				// 	String.Format(
				// 		"Tried to revoke the certificate of user {0} but authenticating for user {1}",
				// 		publicCert.Uid, 
				// 		user.Uid
				// 	)
				// );
				return false;
			}

		}

		public byte[] GetMaxSerialNr()
		{
			ulong maxSerialNr;

			if (_dbContext.PublicCertificates.Any())
			{
				maxSerialNr = _dbContext.PublicCertificates.Max(c => c.SerialNr) + 1;
			}
			else 
			{
				maxSerialNr = 0;
			}

			// XXX: Log error, notify system admin to generate new certificates and make a fresh start?
			// if (maxSerialNr > threshold * ulong.MaxValue) {}

			// Return big endian representation of this integer
			byte[] serialNrBytes = BitConverter.GetBytes(maxSerialNr);
			Array.Reverse(serialNrBytes);
			return serialNrBytes;
		}

		public IDbContextTransaction GetScope() 
		{
			return _dbContext.Database.BeginTransaction();
		}

		public void GenerateCRL(X509Certificate2 coreCACert)
		{
			X509V2CrlGenerator crlGen = new X509V2CrlGenerator();

			DateTime now = DateTime.UtcNow;
            crlGen.SetIssuerDN(new X509Name(coreCACert.SubjectName));
            crlGen.SetThisUpdate(now);
            crlGen.SetNextUpdate(now.AddMinutes(10));
            crlGen.SetSignatureAlgorithm("SHA512WithRSAEncryption");

			foreach (PublicCertificate pubCert in _dbContext.PublicCertificates)
			{
				if (pubCert.IsRevoked)
				{
					//XXX: add crlreason to pubCert DB
					crlGen.AddCrlEntry(new BigInteger(pubCert.SerialNr), now, CrlReason.KeyCompromise);
				}
			}

			FileStream coreCaCertFileStream = new FileStream(CAConfig.CoreCACertPath, FileMode.Open);
			X509Certificate coreCaCert = new X509CertificateParser().ReadCertificate(coreCaCertFileStream);
			coreCaCertFileStream.Close();

			var authorityKeyIdentifierExtension = new AuthorityKeyIdentifier(
					SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(coreCaCert.GetPublicKey()),
					new GeneralNames(new GeneralName(coreCaCert.IssuerDN)),
					(ulong) coreCaCert.SerialNumber
			);

			crlGen.AddExtension(
				X509Extensions.AuthorityKeyIdentifier, 
				false,
                authorityKeyIdentifierExtension
			);

			X509Crl newCrl = crlGen.Generate(coreCaCert.Private);
		}
    }
}