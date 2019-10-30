using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using WebServer.Models;

namespace WebServer.Authentication
{
    public class CookieAuthenticationDBValidator : CookieAuthenticationEvents
    {
        private readonly IMoviesUserContext _dbContext;
        private readonly ILogger _logger;

        public CookieAuthenticationDBValidator(IMoviesUserContext dbContext, ILogger<CertificateAuthenticationDBValidator> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var user = _dbContext.Users.Find(userPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            ClaimsIdentity identity = user.ToClaimsIdentity(context.Scheme.Name);
            foreach (var claim in identity.Claims)
            {
                if (userPrincipal.FindFirstValue(claim.Type) != claim.Value)
                {
                    context.ReplacePrincipal(new ClaimsPrincipal(identity));
                    context.ShouldRenew = true;
                    return;
                }
            }

        }
    }
}
