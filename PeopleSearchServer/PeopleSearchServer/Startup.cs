using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using PeopleSearchServer.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;

namespace PeopleSearchServer
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString;
            string allowedClients;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            if ( Extensions.RunningOnAzure )
            {
                string user = GetSecret ( "dbUser" );
                string password = GetSecret ( "dbPassword" );

                connectionString = Configuration.GetConnectionString ( "AzurePeopleSearch" );
                connectionString = connectionString.Replace ( "{user}", user );
                connectionString = connectionString.Replace ( "{password}", password );

                allowedClients = (string) Configuration.GetValue<string> ( "AllowedClients" );
            }
            else
            {
                connectionString = Configuration.GetConnectionString ( "PeopleSearchContext" );
                allowedClients = (string) Configuration.GetValue<string> ( "AllowedLocalClients" );
            }

            PeopleSearchContext.ConnectionString = connectionString;
            ImageContext.ConnectionString = connectionString; ;

            services.AddDbContext<PeopleSearchContext>(options =>
                    options.UseSqlServer(PeopleSearchContext.ConnectionString));
            services.AddDbContext<ImageContext> ( options =>
                     options.UseSqlServer ( ImageContext.ConnectionString ) );

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(allowedClients)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();  // Added this last for Upload 6/20
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=People}/{action=Get}/{id?}");
            });
        }

        private string GetSecret (string whichsecret )
        {
            SecretBundle secret = null;

            AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider ();
            KeyVaultClient keyVaultClient = new KeyVaultClient ( new KeyVaultClient.AuthenticationCallback (tokenProvider.KeyVaultTokenCallback));
            string url = $"https://healthcatalystkeyvault.vault.azure.net/secrets/{whichsecret}";
            Task.Run ( async () => secret = await keyVaultClient.GetSecretAsync ( url ) ).Wait ();

            return secret.Value;
        }
    }
}
