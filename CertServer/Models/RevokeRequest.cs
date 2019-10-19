using System.ComponentModel.DataAnnotations;

namespace CertServer.Models
{
	public class RevokeRequest
	{
		[Required]
		public string Uid { get; set; }

		[Required]
		public string Password { get; set; }

		[Required]
		public int SerialNumber { get; set; }
	}
}
