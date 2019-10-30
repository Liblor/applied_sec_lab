using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Cert
{
    public class DownloadCertDetails
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Account password")]
        public string Password { get; set; }
    }
}
