using System.ComponentModel.DataAnnotations;

namespace WebServer.Models
{
    public class UserDetails
    {
        [Required, StringLength(64, MinimumLength = 1)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required, EmailAddress, StringLength(64, MinimumLength = 1)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

    }
}