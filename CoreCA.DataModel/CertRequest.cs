using System.ComponentModel.DataAnnotations;

namespace CoreCA.DataModel
{
    public class CertRequest
    {
        [Required, StringLength(64, MinimumLength = 1)]
        public string Uid { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string CertPassphrase { get; set; }

        [Required]
        public CipherSuite RequestedCipherSuite { get; set; }
    }
}
