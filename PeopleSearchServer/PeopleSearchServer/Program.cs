using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeopleSearchServer.Models;
using System;

namespace PeopleSearchServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // Migrate the Databases

                    var peopleContext = services.GetRequiredService<PeopleSearchContext>();
                    peopleContext.Database.Migrate();

                    var imageContext = services.GetRequiredService<ImageContext>();
                    imageContext.Database.Migrate ();

                    // Seed the databases

                    SeedData.Initialize( 
                        services.GetRequiredService<DbContextOptions<ImageContext>>(),
                        services.GetRequiredService<DbContextOptions<PeopleSearchContext>>() );
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
