using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Account
{
    public class LoginDetails
    {
        [Display(Name = "Email Address")]
        [Required, EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
