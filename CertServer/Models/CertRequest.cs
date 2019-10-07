using System.ComponentModel.DataAnnotations;

namespace CertServer.Models
{
    public class CertRequest
    {
        [Required]
        public string Uid { get; set; } 

        [Required]
        public string Password { get; set; }

        [Required]
        public string CipherSuite { get; set; }
    }
}