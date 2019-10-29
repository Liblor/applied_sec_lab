using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace WebServer.Models
{
    public class User
    {
        [Key]
        [Column("uid")]
        public string Id { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "First name")]
        [Required]
        [Column("firstname")]
        public string FirstName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "Last name")]
        [Required]
        [Column("lastname")]
        public string LastName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Display(Name = "Email Address")]
        [Required, EmailAddress]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required]
        [Column("pwd")]
        public string PasswordHash { get; set; }

        public string Name => $"{FirstName} {LastName}";

        public ClaimsIdentity ToClaimsIdentity(string authScheme)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Id),
                new Claim(ClaimTypes.GivenName, FirstName),
                new Claim(ClaimTypes.Surname, LastName),
                new Claim(ClaimTypes.Name, Name),
                new Claim(ClaimTypes.Email, Email),
            };

            return new ClaimsIdentity(claims, authScheme);
        }
    }
}
