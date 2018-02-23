using System;
using System.ComponentModel.DataAnnotations;

namespace FlightChecker.Domain.Models
{
    public class FlightDetail 
    {
      
        [Key]
        public int Id { get; set; }

        public string FlightNumber { get; set; }

        public string FromAirport { get; set; }
        public string ToAirport { get; set; }

        public DateTime DepartureTime { get; set; }
        public string FlightCost { get; set; }

        public int? Rating { get; set; }

    }
}
