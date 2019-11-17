using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CoreCA.DataModel;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Logging;
using WebServer.Models;

namespace WebServer.Authentication
{
    public class CertificateAuthenticationDBValidator : CertificateAuthenticationEvents
    {
        private readonly IMoviesUserContext _dbContext;
        private readonly ILogger _logger;

        public CertificateAuthenticationDBValidator(IMoviesUserContext dbContext, ILogger<CertificateAuthenticationDBValidator> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override Task CertificateValidated(CertificateValidatedContext context)
        {
            _logger.LogTrace($"Initial cert validation successful, entering {nameof(CertificateValidated)}()");
            X509Certificate2 cert = context.ClientCertificate;

            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
                chain.Build(cert);

                // 0th element is our leaf cert
                // 1st element must contain the issuer's cert
                var caCert = chain.ChainElements[1].Certificate;

                // TODO: properly check the certificate identity against config/Core CA server/etc.

                // For now, assume the DB is keyed on whatever gets entered into the cert subject field, whether thats an UID, email address, or something else entirely.
                // TODO: consider edge cases/alternatives (e.g. explicitly using a different field, fallback mechanism, etc)
                string key = cert.GetNameInfo(X509NameType.SimpleName, false);
                User user = _dbContext.Users.Find(key);

                if (user == null) {
                    _logger.LogError($"Cert auth failed: could not find a user with UID '{key}'");
                    context.NoResult();
                    return Task.CompletedTask;
                }

                var userClaims = user.ToClaimsIdentity(context.Scheme.Name);

                // Attach a flag to the user identity to easily reveal the "Admin" navigation tab in the view
                if (_dbContext.Admins.Find(key) != null)
                    userClaims.AddClaim(new Claim(Constants.AdminClaim, true.ToString()));

                context.Principal.AddIdentity(userClaims);

                _logger.LogDebug($"Cert auth success (serial no: {cert.SerialNumber})");

                context.Success();

                return Task.CompletedTask;
            }
        }
    }
}
