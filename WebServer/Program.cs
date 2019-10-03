using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                });
    }
}
