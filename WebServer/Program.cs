using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using WebServer.Models;

namespace WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    SeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(kestrelOpts =>
                    {
                        kestrelOpts.ConfigureHttpsDefaults(httpsOpts =>
                        {
                            // Allow client cert auth but do not require it - we still want to support username/password auth
                            httpsOpts.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                        });
                    });
                    // Must call UseKestrel *again* (it's implicitly called earlier in CreateDefaultBuilder),
                    // after configuring ClientCertificateMode for client cert auth to work.
                    // Refer to https://github.com/aspnet/AspNetCore.Docs/issues/14767
                    webBuilder.UseKestrel();
                }).ConfigureWebHost(host => host.UseKestrel());
    }
}
