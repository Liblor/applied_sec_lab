using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using WebServer.Authentication;

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
            services.AddControllersWithViews(opt =>
            {
                opt.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddAuthentication()
                .AddCookie()
                .AddCertificate(opt =>
                {
                    opt.AllowedCertificateTypes = CertificateTypes.Chained;
                    opt.RevocationFlag = X509RevocationFlag.EntireChain;
                    opt.RevocationMode = X509RevocationMode.NoCheck; // TODO: switch to online
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

            if (Environment.IsDevelopment())
            {
                services.AddDbContext<IMoviesUserContext>(opt => opt.UseInMemoryDatabase("DummyDB"));
            } else
            {
                // TODO: Hook up to the real database
            }

            services.AddScoped<CertificateAuthenticationDBValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Keep HTTP->HTTPS redirection to test client authentication over SSL during development only; in production,
                // HTTPS terminates at the nginx reverse proxy and Kestrel only receives plain HTTP, which would cause infinite redirect loops.
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Index}/{id?}");
            });
        }
    }

}
