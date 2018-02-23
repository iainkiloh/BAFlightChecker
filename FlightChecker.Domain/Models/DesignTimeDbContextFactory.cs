using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

/// <summary>
/// NOTE: this class used only for ensuring migrations in EF core work
/// </summary>
namespace FlightChecker.Domain.Models
{

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FlightCheckerContext>
    {
        public FlightCheckerContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<FlightCheckerContext>();

            //this is unable get the connection string
            //var connectionString = configuration.GetConnectionString("DefaultConnection");

            //use hardcoded string for migrations use only
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=FlightChecker;Trusted_Connection=True;ConnectRetryCount=0";

            builder.UseSqlServer(connectionString);

            return new FlightCheckerContext(builder.Options);
        }
    }

}
