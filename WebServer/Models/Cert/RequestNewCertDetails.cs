using System.ComponentModel.DataAnnotations;

using CoreCA.DataModel;

namespace WebServer.Models.Cert
{
    public class RequestNewCertDetails
    {
        // TODO: re-think arbitrary min-length
        [Required, DataType(DataType.Password)]
        [Display(Name = "Certificate encryption passphrase"), StringLength(256, MinimumLength = Constants.MinPassphraseLength)]
        public string Passphrase { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Account password")]
        public string Password { get; set; }
    }
}
