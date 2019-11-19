using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using CertServer.Data;
using CertServer.DataModifiers;
using CertServer.Models;
using CoreCA.DataModel;

namespace CertServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder();

            // XXX: Implement better solution for DB password
            builder.AddUserSecrets<Startup>();
            builder.AddConfiguration(configuration);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<IMoviesUserContext>()
                .AddDbContextCheck<IMoviesCAContext>();

            services.AddControllers(opt => opt.OutputFormatters.Add(new CrlDerOutputFormatter()));

            // Use lowercase routing although controller names are uppercase
            services.AddRouting(options => options.LowercaseUrls = true);

            // Register the Swagger generator, defining one Swagger document
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = CAConfig.APIName, Version = CAConfig.APIVersion });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddDbContext<IMoviesUserContext>(
                opt => opt.UseMySql(
                    Configuration.GetConnectionString("IMoviesUserDB")
                )
            );

            services.AddDbContext<IMoviesCAContext>(
                opt => opt.UseMySql(
                    Configuration.GetConnectionString("IMoviesCertDB")
                )
            );

            services.AddScoped<UserDBAuthenticator>();
            services.AddScoped<CADBModifier>();

            services.AddSingleton<PasswordPolicyValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // XXX: Only run https server
            // app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/swagger/{documentname}/swagger.json";
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    string.Format("/{0}/swagger/{1}/swagger.json", CAConfig.APIBasePath, CAConfig.APIVersion),
                    string.Format("{0} {1}", CAConfig.APIName, CAConfig.APIVersion)
                );
                c.RoutePrefix = CAConfig.APIBasePath + "/swagger";
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
