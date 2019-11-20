using System.ComponentModel.DataAnnotations;

using CoreCA.DataModel;

namespace WebServer.Models.Cert
{
    public class RequestNewCertDetails
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Certificate encryption passphrase"), StringLength(256, MinimumLength = Constants.MinPassphraseLength)]
        public string Passphrase { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Account password")]
        public string Password { get; set; }
    }
}
