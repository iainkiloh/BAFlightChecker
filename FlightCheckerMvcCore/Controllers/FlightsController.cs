
using Microsoft.AspNetCore.Mvc;

namespace FlightCheckerMvcCore.Controllers
{
    public class FlightsController : Controller
    {
        public IActionResult Flights()
        {
            return View();
        }
    }
}