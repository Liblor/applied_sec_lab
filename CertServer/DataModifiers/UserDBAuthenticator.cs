using CertServer.Models;
using System;
using System.Security.Cryptography;

namespace CertServer.DataModifiers
{
	public class UserDBAuthenticator
	{
		private readonly IMoviesUserContext _dbContext;

        public UserDBAuthenticator(IMoviesUserContext dbContext)
        {
            _dbContext = dbContext;
        }

		public User AuthenticateAndGetUser(string uid, string pw)
		{
			SHA1 sha1Managed = new SHA1Managed();
			string pwHash = BitConverter.ToString(
				sha1Managed.ComputeHash(
					System.Text.Encoding.ASCII.GetBytes(pw)
				)
			).Replace("-","").ToLower();

			User user = _dbContext.Users.Find(uid);
			if (user.Uid.Equals(uid) && pwHash.Equals(user.PasswordHash))
			{
				return user;
			}
			else
			{
				return null;
			}
		}
	}
}
