using System.ComponentModel.DataAnnotations;

namespace WebServer.Models
{
    public class UserDetails
    {
        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "First name")]
        [Required]
        public string FirstName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "Last name")]
        [Required]
        public string LastName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "Email Address")]
        [Required, EmailAddress]
        public string Email { get; set; }

    }
}