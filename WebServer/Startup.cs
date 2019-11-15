using CoreCA.Client;
using CoreCA.DataModel;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using WebServer.Authentication;
using WebServer.HealthChecks;

namespace WebServer
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var webServerOptionsSection = Configuration.GetSection("WebServer");

            services.AddOptions<WebServerOptions>()
                .Bind(webServerOptionsSection)
                .ValidateDataAnnotations();

            var webServerOptions = webServerOptionsSection.Get<WebServerOptions>();

            services.AddHealthChecks()
                .AddDbContextCheck<IMoviesUserContext>()
                .AddDbContextCheck<IMoviesCertContext>()
                .AddCheck<CertServerHealthCheck>(nameof(CertServerHealthCheck));

            services.AddControllersWithViews(opt =>
            {
                opt.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                opt.OutputFormatters.Add(new CrlDerOutputFormatter());
            });

            services.AddAuthentication()
                .AddCookie(opt =>
                {
                    opt.EventsType = typeof(CookieAuthenticationDBValidator);
                })
                .AddCertificate(opt =>
                {
                    opt.AllowedCertificateTypes = CertificateTypes.Chained;
                    opt.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
                    opt.RevocationMode = X509RevocationMode.Online;
                    opt.ValidateCertificateUse = true;
                    opt.ValidateValidityPeriod = true;

                    opt.EventsType = typeof(CertificateAuthenticationDBValidator);
                });

            // Configure the default authorization policy to accept both certificate and cookie authentication
            // This removes the need to specify both auth schemes explicitly with each [Authorize] attribute.
            services.AddAuthorization(opt =>
            {
                var policyBuilder = new AuthorizationPolicyBuilder(
                    CertificateAuthenticationDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                policyBuilder.RequireAuthenticatedUser();

                opt.DefaultPolicy = policyBuilder.Build();
            });

            services.AddDbContext<IMoviesUserContext>(opt => opt.UseMySql(Configuration.GetConnectionString("IMoviesUserDB")));
            services.AddDbContext<IMoviesCertContext>(opt => opt.UseMySql(Configuration.GetConnectionString("IMoviesCertDB")));

            services.AddScoped<CertificateAuthenticationDBValidator>();
            services.AddScoped<CookieAuthenticationDBValidator>();

            services.AddHttpClient<CoreCAClient>(client =>
            {
                client.BaseAddress = new Uri(webServerOptions.CoreCAURL);
            }).ConfigurePrimaryHttpMessageHandler(() => {
                var handler = new HttpClientHandler();
                
                if (!string.IsNullOrEmpty(webServerOptions.ClientCertPath))
                {
                    var cert = new X509Certificate2(webServerOptions.ClientCertPath);
                    handler.ClientCertificates.Add(cert);
                }
                return handler;
            });

            if (Environment.IsProduction())
            {
                services.AddCertificateForwarding(options =>
                {
                    options.CertificateHeader = "X-SSL-CERT";
                    options.HeaderConverter = (headerValue) =>
                    {
                        // For some reason, nginx URL-encodes the cert.
                        return new X509Certificate2(HttpUtility.UrlDecodeToBytes(headerValue)); ;
                    };
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseRouting();

            if (Environment.IsProduction())
                app.UseCertificateForwarding();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Index}/{id?}");

                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
