using System.ComponentModel.DataAnnotations;

namespace CertServer.Models
{
	public class PasswordChangeRequest
	{
		[Required]
		public string Uid { get; set; } 

		[Required]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(64, MinimumLength = 1)]
		public string NewPassword { get; set; }
	}
}
