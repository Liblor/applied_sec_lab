using CoreCA.DataModel;
using System.Collections.Generic;
using System.Security.Claims;

namespace WebServer.Models
{
    public static class UserExtensions
    {
        public static string GetName(this User user) => $"{user.FirstName} {user.LastName}";

        public static ClaimsIdentity ToClaimsIdentity(this User user, string authScheme)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Name, user.GetName()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            return new ClaimsIdentity(claims, authScheme);
        }
    }
}
