using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Security.Cryptography;

using CoreCA.DataModel;

namespace CertServer.DataModifiers
{
	public class UserDBAuthenticator
	{
		private readonly IMoviesUserContext _dbContext;

        public UserDBAuthenticator(IMoviesUserContext dbContext)
        {
            _dbContext = dbContext;
        }

		public IDbContextTransaction GetScope()
		{
			return _dbContext.Database.BeginTransaction();
		}

		private string ComputePasswordHash(string password)
		{
			SHA1 sha1Managed = new SHA1Managed();
			return BitConverter.ToString(
				sha1Managed.ComputeHash(
					System.Text.Encoding.ASCII.GetBytes(password)
				)
			).Replace("-","").ToLower();
		}

		private bool MeetsPasswordPolicy(string password)
		{
			// XXX: Check that the new password meets the password policy
			return true;
		}

		public bool ChangePassword(User user, string newPassword)
		{
			if (MeetsPasswordPolicy(newPassword))
			{
				user.PasswordHash = ComputePasswordHash(newPassword);
				_dbContext.SaveChanges();
				return true;
			}

			return false;
		}

		public User AuthenticateAndGetUser(string uid, string password)
		{
			User user = _dbContext.Users.Find(uid);
			if (user != null && user.PasswordHash.Equals(ComputePasswordHash(password)))
			{
				return user;
			}
			else
			{
				return null;
			}
		}

		public User GetUser(string uid)
		{
			return _dbContext.Users.Find(uid);
		}
	}
}
