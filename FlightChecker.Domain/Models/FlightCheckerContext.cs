using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FlightChecker.Domain.Models
{
    public class FlightCheckerContext : DbContext
    {

        public FlightCheckerContext(DbContextOptions<FlightCheckerContext> options) : base(options)
        {
        }

        //public DbSet<FlightDetail> FlightFavourites { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<PricedItinerary> PricedItineraries { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FlightChecker;Trusted_Connection=True;ConnectRetryCount=0");

        //}

    }



}
