using System.ComponentModel.DataAnnotations;

namespace CertServer.Models
{
	public class RevokeRequest
	{
		[Required]
		public string Uid { get; set; }
	}
}
