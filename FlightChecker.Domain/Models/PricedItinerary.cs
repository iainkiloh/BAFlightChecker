using System;
using System.ComponentModel.DataAnnotations;

namespace FlightChecker.Domain.Models
{
    public class PricedItinerary
    {
        [Key]
        public int Id { get; set; }

        public string DepartureCity { get; set;}
        public string DepartureCityCode { get; set; }
        public string ArrivalCity { get; set; }
        public string ArrivalCityCode { get; set; }
        public string Cabin { get; set; }
        public string TravelMonth { get; set; }
        public string JourneyType { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsTaxIncluded { get; set; }

        public DateTime DateAdded { get; set; }
       
    }
}
