using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertServer.Models
{
    public class PublicCertificate
    {
		[Key]
		[Column("serialNr")]
        public ulong SerialNr { get; set; }

        [Column("uid")]
        public string Uid { get; set; }

		[Column("cert")]
        public string Certificate { get; set; }

		[Column("isrevoked")]
        public bool IsRevoked { get; set; }
    }
}