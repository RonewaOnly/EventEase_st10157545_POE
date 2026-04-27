using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
namespace EventEase_st10157545_POE.Data
{

    public class EventEaseDbContextFactory : IDesignTimeDbContextFactory<EventEaseDbContext>
    {
        public EventEaseDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EventEaseDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "EventEase_st10157545_POE"));

            // Load configuration including user secrets
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();


            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var rawconnectionString = configuration["ConnectionStrings:DefaultConnection"];

            Console.WriteLine("BASE PATH: " + basePath);
            Console.WriteLine("FILE EXISTS: " + File.Exists(Path.Combine(basePath, "appsettings.Development.json")));
            Console.WriteLine("CONN: " + connectionString);
            Console.WriteLine("RAW CONN: " + rawconnectionString);
            builder.UseSqlServer(connectionString);

            return new EventEaseDbContext(builder.Options);
        }
    }
}
