using System.ComponentModel.DataAnnotations;

namespace WebServer.Models.Account
{
    public class LoginDetails
    {
        [Display(Name = "User ID")]
        [StringLength(64, MinimumLength = 1)]
        [Required]
        public string Id { get; set; }

        [Display(Name = "Password")]
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
