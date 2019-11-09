using System.ComponentModel.DataAnnotations;

namespace CoreCA.DataModel
{
	public class RevokeRequest
	{
		[Required]
		public string Uid { get; set; }
	}
}
