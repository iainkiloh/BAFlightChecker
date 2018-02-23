using FlightChecker.Domain.Models;
using System.Collections.Generic;

namespace FlightCheckerMvcCore.ViewModels
{
    public class PagedPricedItineraries
    {
        public IEnumerable<PricedItinerary> PricedItineraries { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
