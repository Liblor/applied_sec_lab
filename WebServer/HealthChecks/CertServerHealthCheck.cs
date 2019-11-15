using CoreCA.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer.HealthChecks
{
    public class CertServerHealthCheck : IHealthCheck
    {
        private readonly CoreCAClient _coreCAClient;

        public CertServerHealthCheck(CoreCAClient coreCAClient)
        {
            _coreCAClient = coreCAClient;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return _coreCAClient.CheckHealth();
        }
    }
}
