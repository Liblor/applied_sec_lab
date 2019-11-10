using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Cert
{
    public class RequestNewCertDetails
    {
        // TODO: validate passphrase requirements in the Core CA too, re-think arbitrary min-length
        [Required, DataType(DataType.Password)]
        [Display(Name = "Certificate encryption passphrase"), StringLength(256, MinimumLength = 12)]
        public string Passphrase { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Account password")]
        public string Password { get; set; }
    }
}
