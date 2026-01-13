using ExcursionApp.Data;
using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models.Bookings;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.profile;
using ExcursionApp.Data.Models.trips;
using ExcursionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;

namespace ExcursionApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TravelClientController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TravelClientController(IHttpContextAccessor httpContextAccessor, IClientService clientService)
        {
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
        }
        

        #region destination
        //get destination tree
        [HttpPost("GetDestination_Tree")]
        public IActionResult GetDestination_Tree(DestinationReq req)
        {
            return Ok(_clientService.GetDestination_Tree(req));
        }
        //get destinations
        [HttpPost("getDestinations")]
        public IActionResult getDestinations(DestinationReq req)
        {

            return Ok(_clientService.getDestinations(req));
        }
        #endregion
        #region trips
        [HttpPost("GetTripCategories")]
        public async Task<IActionResult> GetTripCategories()
        {
            return Ok(await _clientService.GetTripCategories());
        }
        //get specific trip details
        [HttpPost("GetTripDetails")]
        public async Task<IActionResult> GetTripDetails(TripDetailsReq req)
        {

            return Ok(await _clientService.GetTripDetails(req));
        }
        //get trips and top trips with its details 
        [HttpPost("GetTripsAll")]
        public async Task<IActionResult> GetTripsAll(TripsReq req)
        {

            return Ok(await _clientService.GetTripsAll(req));
        }
        //get trips which shown in home page slider
        [HttpPost("GetTripsForSlider")]
        public async Task<IActionResult> GetTripsForSlider(TripsReq req)
        {

            return Ok(await _clientService.GetTripsForSlider(req));
        }

        [HttpPost("GetPickupsForTrip")]
        public async Task<IActionResult> GetPickupsForTrip(PickupsReq req)
        {

            return Ok(await _clientService.GetPickupsForTrip(req));
        }
        [HttpPost("GetClientsReviews")]
        public async Task<IActionResult> GetClientsReviews(ClientsReviewsReq req)
        {
            return Ok(await _clientService.GetClientsReviews(req));
        }
        [HttpPost("GetWishListCount")]
        public async Task<IActionResult> GetWishListCount([FromQuery] string? clientId)
        {
            
            return Ok(await _clientService.GetWishListCount(clientId));
        }
        [HttpPost("CalculateBookingPrice")]
        public IActionResult CalculateBookingPrice(CalculateBookingPriceReq req)
        {
            return Ok(_clientService.CalculateBookingPrice(req));
        }
        [HttpPost("GetMyBookingCount")]
        public async Task<IActionResult> GetMyBookingCount([FromQuery] string? clientId)
        {

            return Ok(await _clientService.GetMyBookingCount(clientId));
        }
        #endregion


    }
}
