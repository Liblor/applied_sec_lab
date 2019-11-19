using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Account
{
    public class LoginDetails
    {
        [Required, StringLength(64, MinimumLength = 1)]
        [Display(Name = "User ID")]
        public string Id { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
