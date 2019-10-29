using System.ComponentModel.DataAnnotations;

namespace CertServer.Models
{
	public class DownloadPrivateKeyRequest
	{
		[Required]
		public string Uid { get; set; } 

		[Required]
		public string Password { get; set; }
	}
}
