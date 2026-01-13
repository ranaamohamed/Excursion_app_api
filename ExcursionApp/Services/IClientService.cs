using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models;
using ExcursionApp.Data.Models.Bookings;
using ExcursionApp.Data.Models.Bookings.Client;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.profile;
using ExcursionApp.Data.Models.trips;

namespace ExcursionApp.Services
{
    public interface IClientService
    {
        #region destination
        public List<DestinationResponse> getDestinations(DestinationReq req);
        public List<DestinationTree> GetDestination_Tree(DestinationReq req);
        #endregion

        #region trips
        public Task<List<trip_category>> GetTripCategories();
        public Task<TripsAll> GetTripDetails(TripDetailsReq req);
        public Task<List<TripsAll>> GetTripsAll(TripsReq req);
        public Task<List<tripwithdetail>> GetTripsForSlider(TripsReq req);
        public Task<List<TripsPickupResponse>> GetPickupsForTrip(PickupsReq req);
        public Task<ClientsReviewsResponse> GetClientsReviews(ClientsReviewsReq req);

        public ResponseCls SaveReviewForTrip(tbl_review row);
        public Task<List<TripsAll>> GetClientWishList(ClientWishListReq req);
        public ResponseCls AddTripToWishList(TripsWishlistReq row);
        public Task<int> GetWishListCount(string client_id);
        #endregion

        #region "booking"
        public Task<ResponseCls> CancelBooking(long? booking_id, string? client_id);
        public Task<ResponseCls> DeleteBooking(long? booking_id, string? client_id);
        public ResponseCls SaveClientBooking(BookingCls row);
        public List<TripFacility> getFacilityForTrip(long? trip_id, string lang_code, bool? isExtra);
        public ResponseCls AssignExtraToBooking(List<booking_extra> lst);
        public BookingPrice CalculateBookingPrice(CalculateBookingPriceReq req);
        public Task<List<BookingSummary>> GetBookingWithDetails(BookingReq req);
        public BookingWithTripDetailsAll ConfirmBooking(ConfirmBookingReq req);

        public Task<BookingDetailsList> ConfirmBookingLst(ConfirmBookingListReq req);
        public  Task<List<BookingSummary>> GetMyBooking(string booking_code,LangReq req, string client_id);
        public Task<int> GetMyBookingCount(string client_id);
        #endregion

        #region "Profile"
        public Task<List<ClientProfileCast>> GetClientProfiles(string clientId);
        public ResponseCls SaveMainProfile(ClientProfileCast profile);
        public Task<ResponseCls> SaveProfileImage(client_image image);
        public Task<List<client_image>> GetProfileImage(string clientId);
      
        #endregion
    }
}
