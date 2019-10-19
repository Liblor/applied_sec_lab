using CertServer.Models;

namespace CertServer.Authentication
{
	public static class UserDBAuthenticator
	{
		public static User GetUser(string uid, string password)
		{
			// XXX: Implement user authentication against DB, use functionality from webserver to prevent code duplication?
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
	}
}
