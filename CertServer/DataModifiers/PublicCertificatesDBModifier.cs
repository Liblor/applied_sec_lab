using CertServer.Data;
using CertServer.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;

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

		public bool RevokeCertificate(User user, long serialNr) 
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
			long maxSerialNr;

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
			return BitConverter.GetBytes(
				System.Net.IPAddress.NetworkToHostOrder(maxSerialNr)
			);
		}

		public IDbContextTransaction GetScope() 
		{
			return _dbContext.Database.BeginTransaction();
		}

		public void GenerateCRL()
		{
			// XXX: Not possible with System.Security.Cryptography, move to bouncycastle?
		}
    }
}