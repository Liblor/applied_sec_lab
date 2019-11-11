using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using CoreCA.DataModel;

namespace CertServer.Models
{
    public class PrivateKey
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("uid")]
        [StringLength(64, MinimumLength = Constants.MinPasswordLength)]
        public string Uid { get; set; }

        [Column("private_cert", TypeName = "ntext")]
        [Required]
        public string KeyPkcs12 { get; set; }
    }
}
