using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Certificate;
using WebServer.Models;

namespace WebServer.Authentication
{
    public class CertificateAuthenticationDBValidator : CertificateAuthenticationEvents
    {
        private readonly IMoviesUserContext _dbContext;

        public CertificateAuthenticationDBValidator(IMoviesUserContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task CertificateValidated(CertificateValidatedContext context)
        {
            X509Certificate2 cert = context.ClientCertificate;

            using (var chain = new X509Chain())
            {
                // We build the chain to merely obtain the full issuer's certificate from the machine store, hence the Revocation.NoCheck flag.
                // Revocation and validity should have already been checked earlier.
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(cert);

                // 0th element is our leaf cert
                // 1st element must contain the issuer's cert
                var caCert = chain.ChainElements[1].Certificate;

                // TODO: properly check the certificate identity against config/Core CA server/etc.
                if (!caCert.Subject.Contains("iMovies", System.StringComparison.OrdinalIgnoreCase))
                    // Calling NoResult() fails certificate authentication only, allowing the user to fall back to username/password
                    // Calling Fail() would short-circuit the entire auth pipeline without giving a chance for the cookie handler to run (we don't want that).
                    context.NoResult();

                // For now, assume the DB is keyed on whatever gets entered into the cert subject field, whether thats an UID, email address, or something else entirely.
                // TODO: consider edge cases/alternatives (e.g. explicitly using a different field, fallback mechanism, etc)
                string key = cert.GetNameInfo(X509NameType.SimpleName, false);
                User user = _dbContext.Users.Find(key);
                if (user == null)
                    context.NoResult();

                context.Principal.AddIdentity(user.ToClaimsIdentity(context.Scheme.Name));

                return Task.CompletedTask;
            }
        }
    }
}
