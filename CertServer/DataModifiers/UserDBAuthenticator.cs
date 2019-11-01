using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

using CertServer.Data;
using CertServer.Models;

namespace CertServer.DataModifiers
{
	public class UserDBAuthenticator
	{
		private readonly IMoviesUserContext _dbContext;
		private readonly ILogger _logger;

		public UserDBAuthenticator(
			IMoviesUserContext dbContext,
			ILogger<UserDBAuthenticator> logger
		)
		{
			_dbContext = dbContext;
			_logger = logger;
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
			// Check password list
			// Enforce min length 8
			return true;
		}

		public bool ChangePassword(User user, string newPassword)
		{
			if (MeetsPasswordPolicy(newPassword))
			{
				user.PasswordHash = ComputePasswordHash(newPassword);
				_dbContext.SaveChanges();

				_logger.LogInformation("Changed password of user " + user.Uid);
				return true;
			}

			_logger.LogInformation(
				string.Format(
					"Refused to change password of user {0} because the new password "
					+ "did not meet the password policy",
					user.Uid
				)
			);

			return false;
		}

		public User AuthenticateAndGetUser(string uid, string password)
		{
			User user = _dbContext.Users.Find(uid);
			if (user != null && user.PasswordHash.Equals(ComputePasswordHash(password)))
			{
				_logger.LogInformation("Successful authentication of user " + uid);
				return user;
			}
			else
			{
				string error_msg = string.Format("Failed authentication attempt of user {0}; ", uid);

				if (user == null)
				{
					error_msg += "no user with this UID found";
				}
				else
				{
					error_msg += "wrong password";
				}

				_logger.LogWarning(error_msg);
				return null;
			}
		}

		public User GetUser(string uid)
		{
			return _dbContext.Users.Find(uid);
		}
	}
}
