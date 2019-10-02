using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Account
{
	public class LoginDetails
	{
		[Required, EmailAddress]
		public string Email { get; set; }

		[Required, DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
