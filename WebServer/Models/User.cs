using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebServer.Models
{
    // TODO: Configure model validation
    // TODO: Configure serialization
    public class User
    {
        public string Id { get; set; }

        [StringLength(256, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(256, MinimumLength = 1)]
        [Required]
        public string LastName { get; set; }

        [StringLength(256, MinimumLength = 1)]
        [Required, EmailAddress]
        public string Email { get; set; }

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
