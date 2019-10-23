using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertServer.Models
{
    public class PrivateKey
    {
        [Key]
        [Column("uid")]
        public string Uid { get; set; }
		
		[Column("privkey")]
        public string KeyPkcs12 { get; set; }
    }
}