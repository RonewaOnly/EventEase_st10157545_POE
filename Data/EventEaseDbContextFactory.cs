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

            // Load configuration including user secrets
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>() // <-- ensures EF CLI reads secrets
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new EventEaseDbContext(builder.Options);
        }
    }
}
