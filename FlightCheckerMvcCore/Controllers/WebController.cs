using FlightChecker.Domain.Models;
using FlightCheckerMvcCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FlightCheckerMvcCore.Controllers
{
    [Produces("application/json")]
    [Route("api/Web")]
    public class WebController : Controller
    {
        protected FlightCheckerContext Context;

        public WebController(FlightCheckerContext context)
        {
            Context = context;
        }

        [HttpGet]
        [Route("getsavedflightoffers")]
        public async Task<IEnumerable<PricedItinerary>> GetSavedFlightOffers()
        {
            var response = Context.PricedItineraries.OrderByDescending(x => x.TravelMonth).OrderByDescending(x => x.DateAdded).ToListAsync();
            return await response;
        }

        [HttpPost]
        [Route("postsavedflightoffers")]
        public async Task<PagedPricedItineraries> GetSavedFlightOffers(PagingInfo pagingInfo)
        {
            var itineraries = Context.PricedItineraries.OrderByDescending(x => x.TravelMonth).OrderByDescending(x => x.DateAdded)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToListAsync();

           
            pagingInfo.TotalItems = await Context.PricedItineraries.CountAsync();

            var response = new PagedPricedItineraries
            {
                PricedItineraries = await itineraries,
                PagingInfo = pagingInfo
            };

            return response;
        }

        
        [HttpGet]
        [Route("getsavedflightoffer/{id}")]
        public async Task<PricedItinerary> GetSavedFlightOffer(int id)
        {
            var response = Context.PricedItineraries.Where(x => x.Id == id).FirstOrDefaultAsync();
            return await response;
        }

        [HttpGet]
        [Route("getapikey/{apiName}")]
        public string GetApiKey(string apiName)
        {
            var response = Context.ApiKeys.Where(x => x.ApiName == apiName).Select(x => x.ApiKeyValue).FirstOrDefault();
            return response;
        }

       


        [Route("searchlowestfareflightoffers/{arrivalCity}/{cabin}/{journeyType}/{range}")]
        public async Task<JsonResult> SearchLowestFareFlightOffers(string arrivalCity,string cabin, string journeyType, string range)
        {
            var statusCode = System.Net.HttpStatusCode.OK;
            HttpResponseMessage apiResponse;

            try
            {
                //validate the input
                var p = new KeyValuePair<string, string>("Arrival City", arrivalCity);
                var errors = ValidateFilters(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string,string>("Arrival City", arrivalCity),
                    new KeyValuePair<string,string>("Cabin", cabin),
                    new KeyValuePair<string,string>("Journey Type", journeyType),
                    new KeyValuePair<string,string>("Range", range),
                });
                if (!string.IsNullOrEmpty(errors))
                {
                    statusCode = HttpStatusCode.BadRequest;
                    throw new ApplicationException(errors);
                }

                //get the key for connection to the BA LowCostOffer API
                var apiKey = GetApiKey("BA_LowestMonthlyPricesAPI");
                var client = new HttpClient();

                client.DefaultRequestHeaders.Add("client-key", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = "https://api.ba.com/rest-v1/v1/flightOfferBasic";

                apiResponse = await client.GetAsync(url + ";departureCity=LON;arrivalCity=" + arrivalCity + ";cabin=" + cabin + ";journeyType=" + journeyType + ";range=" + range);
                statusCode = apiResponse.StatusCode;  
                if(statusCode != System.Net.HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
                {
                    throw new ApplicationException("An unexpected server error type " + ((int)statusCode).ToString() + " occurred: " + apiResponse.ReasonPhrase);
                }

                var content = await apiResponse.Content.ReadAsStringAsync();
                var itineraries = new List<PricedItinerary>();
                if (!string.IsNullOrEmpty(content))
                {
                    //convert the awkward BA response to a nice flat model for our view to work with     
                    MapBaPlannedItinerariesToContract(ref itineraries, JObject.Parse(content));
                }

                //get distinct resullts based on price as BA Api for monthly Low seems to return duplicates
                var distinctItineraries = itineraries.GroupBy(x => x.Amount)
                     .Select(x => x.First());

                return Json(distinctItineraries);
            }
            catch(Exception e)
            {
                Response.StatusCode = (int)statusCode;
                var response = new HttpResponseMessage(statusCode);
                response.Content = new StringContent(e.Message);
                return Json(response);

            }           
        }

        [HttpPost]
        [Route("addlowestfareflightoffer")]
        public JsonResult AddLowestFareFlightOffer(PricedItinerary item)
        {
            try
            {
                item.DateAdded = DateTime.UtcNow;
                Context.PricedItineraries.Add(item);
                Context.SaveChanges();
                Response.StatusCode = 200;
                return Json(item);
            }
            catch(Exception e)
            {
                Response.StatusCode = 400;
                return Json(e.Message);
            }
        }

        [HttpPost]
        [Route("deletelowestfareflightoffer")]
        public JsonResult DeleteLowestFareFlightOffer(int id)
        {
            try
            {
                //find the flight and remove it
                var flight = Context.PricedItineraries.Where(x => x.Id == id).FirstOrDefault();

                if (flight == null)
                {
                    throw new ApplicationException("Itinerary entry not found for id: " + id.ToString());
                }
                Context.PricedItineraries.Remove(flight);
                Context.SaveChanges();
               
            }
            catch (Exception e)
            {
                Response.StatusCode = 400;
                return Json(e.Message);
            }

            Response.StatusCode = 200;
            return Json("success");

        }
        /// <summary>
        /// Map the Ba Lowest Cost Response to a flattened contract
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="apiResponse"></param>
        private void MapBaPlannedItinerariesToContract(ref List<PricedItinerary> contract, JObject apiResponse)
        {      
            var responseItems = apiResponse["PricedItinerariesResponse"]["PricedItinerary"];
            if(responseItems is JObject)
            {
                var item = responseItems;
                var contractItem = MapSingleBaPricedItineraryToContract(item);
                contract.Add(contractItem);
            }
            if (responseItems is JArray)
            {
                foreach (var item in responseItems)
                {
                    var contractItem = MapSingleBaPricedItineraryToContract(item);
                    contract.Add(contractItem);
                }
            }
        }//end method

        private PricedItinerary MapSingleBaPricedItineraryToContract(JToken item)
        {
            var contractItem = new PricedItinerary
            {
                DepartureCity = item["DepartureCity"].ToString(),
                DepartureCityCode = item["DepartureCityCode"].ToString(),
                ArrivalCity = item["ArrivalCity"].ToString(),
                ArrivalCityCode = item["ArrivalCityCode"].ToString(),
                Cabin = item["Cabin"].ToString(),
                JourneyType = item["JourneyType"].ToString(),
                TravelMonth = item["TravelMonth"].ToString(),
                Amount = Convert.ToDecimal(item["Price"]["Amount"]["Amount"].ToString()),
                CurrencyCode = item["Price"]["Amount"]["CurrencyCode"].ToString(),
                IsTaxIncluded = Convert.ToBoolean(item["Price"]["IsTaxIncluded"].ToString())
            };
            return contractItem;
        }

        
        private string ValidateFilters(List<KeyValuePair<string,string>> parameters)
        {
            string errorString = "";
            foreach (var item in parameters)
            {
                if (string.IsNullOrEmpty(item.Value) || item.Value.Contains("choose"))
                {
                    errorString = item.Key + " must be supplied";
                }
            }
            return errorString;
        }

    }
}

