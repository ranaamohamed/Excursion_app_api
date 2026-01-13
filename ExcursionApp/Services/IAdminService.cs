using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models;
using ExcursionApp.Data.Models.Bookings.Admin;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.Transfer;
using ExcursionApp.Data.Models.trips;

namespace ExcursionApp.Services
{
    public interface IAdminService
    {
        #region "Main_setting"
        public Task<List<tbl_currency>> Get_Currencies();
        public Task<List<tbl_language>> Get_Languages();
        public  Task<List<DashBoardStats>> GetDashBoardStats();
        #endregion

        public Task<List<destination_translation>> GetDestinationTranslation(int? destination_id);
        public ResponseCls SaveMainDestination(destination_main row);
        public ResponseCls SaveDestinationTranslations(destination_translation row);
        public ResponseCls saveDestinationImage(List<destination_img> row);
        public DestinationWithTranslationsPag GetDestinationWithTranslations(DestinationWithTranslationsReq req);
        public Task<List<destination_img>> GetImgsByDestination(int? destination_id);
        public Task<List<destination_main>> GetDestination_Mains(bool? leaf, bool? isAll);
        public ResponseCls UpdateDestinationImage(DestinationImgUpdateReq cls);
        public Task<List<transfer_category>> GetTransfer_Categories();
        public ResponseCls SaveTransferCategory(TransferCategorySaveReq row);


        #region trips
        public  Task<List<trip_pickups_translation>> GetTrip_Pickups_Translations(long? trip_pickup_id);
        public Task<List<facility_translation>> GetFacilityTranslation(long? faclility_id);
        public TripMainCastPag GetTrip_MainsWithPag(TripMainReq req);
        public Task<List<child_policy_setting>> GetTrip_ChildPolicy(long? trip_id);
        public ResponseCls SaveTripChildPolicy(ChildPolicyPricesReq row);
        public Task<List<trip_category>> GetTripCategories();
        public Task<List<trip_price>> GetTrip_Prices(long? trip_id);
        public Task<List<TripTranslationGrp>> GetTripTranslationGrp(long? trip_id);
        public ResponseCls SaveMainTrip(trip_main row);
        public ResponseCls SaveTripTranslation(TripTranslationReq row);
        public ResponseCls SaveTripPrices(TripPricesReq row);
        public ResponseCls saveTripImage(List<trip_img> lst);
        public ResponseCls SaveMainFacility(facility_main row);
        public ResponseCls SaveFacilityTranslation(FacilityTranslationReq row);
        public ResponseCls AssignFacilityToTrip(TripFacilityAssignReq row);

        public ResponseCls SaveMainTripPickups(TripsPickupSaveReq row);
        public ResponseCls SaveTripPickupsTranslations(TripsPickupTranslationSaveReq row);

        public Task<List<TripMainCast>> GetTrip_Mains(int destination_id, int trip_type);

        public List<TripsPickupResponseGrp> GetPickupsAllForTrip(PickupsReq req);
        public Task<List<trip_img>> GetImgsByTrip(decimal? trip_id);
        public ResponseCls UpdateTripImage(TripImgUpdateReq trip);

        public List<FacilityWithTranslationGrp> GetFacilityWithTranslation();

        public List<FacilityAllWithSelect> GetFacilityAllWithSelect(long? trip_id);
        #endregion

        #region "booking"
        public Task<BookingAll> GetAllBooking(BookingAllReq req);
        #endregion
    }
}
