using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace CoreCA.DataModel
{
    public class PublicCertificate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("serial_number")]
        public ulong SerialNr { get; set; }

        [Column("uid")]
        [Required]
        [StringLength(64, MinimumLength = 1)]
        public string Uid { get; set; }

        [Column("public_cert", TypeName = "ntext")]
        [Required]
        public string Certificate { get; set; }

        [Column("revoked", TypeName = "boolean")]
        [Required]
        public bool IsRevoked { get; set; }

        public SerialNumber GetSerialNumber()
        {
            return new SerialNumber {
                SerialNr = SerialNr
            };
        }

        public X509Certificate2 Parse() => new X509Certificate2(Convert.FromBase64String(Certificate));
    }
}