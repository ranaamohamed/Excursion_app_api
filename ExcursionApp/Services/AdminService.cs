using ExcursionApp.Data;
using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models;
using ExcursionApp.Data.Models.Bookings.Admin;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.Transfer;
using ExcursionApp.Data.Models.trips;

namespace ExcursionApp.Services
{
    public class AdminService : IAdminService
    {
        private AdminDAO _adminDAO;

        public AdminService(AdminDAO adminDAO)
        {
            _adminDAO = adminDAO;

        }

        public ResponseCls AssignFacilityToTrip(TripFacilityAssignReq row)
        {
            return _adminDAO.AssignFacilityToTrip(row);
        }

        public Task<BookingAll> GetAllBooking(BookingAllReq req)
        {
            return _adminDAO.GetAllBooking(req);
        }

        public Task<List<DashBoardStats>> GetDashBoardStats()
        {
            return _adminDAO.GetDashBoardStats();
        }

        public Task<List<destination_translation>> GetDestinationTranslation(int? destination_id)
        {
            return _adminDAO.GetDestinationTranslation(destination_id);
        }

        public DestinationWithTranslationsPag GetDestinationWithTranslations(DestinationWithTranslationsReq req)
        {
            return _adminDAO.GetDestinationWithTranslations(req);
        }

        public Task<List<destination_main>> GetDestination_Mains(bool? leaf ,bool? isAll)
        {
            return _adminDAO.GetDestination_Mains(leaf,isAll);
        }

        public List<FacilityAllWithSelect> GetFacilityAllWithSelect(long? trip_id)
        {
            return _adminDAO.GetFacilityAllWithSelect(trip_id);
        }

        public Task<List<facility_translation>> GetFacilityTranslation(long? faclility_id)
        {
            return _adminDAO.GetFacilityTranslation(faclility_id);
        }

        public List<FacilityWithTranslationGrp> GetFacilityWithTranslation()
        {
            return _adminDAO.GetFacilityWithTranslation();
        }

        public Task<List<destination_img>> GetImgsByDestination(int? destination_id)
        {
            return _adminDAO.GetImgsByDestination(destination_id);
        }

        public Task<List<trip_img>> GetImgsByTrip(decimal? trip_id)
        {
            return _adminDAO.GetImgsByTrip(trip_id);
        }

        public List<TripsPickupResponseGrp> GetPickupsAllForTrip(PickupsReq req)
        {
            return _adminDAO.GetPickupsAllForTrip(req);
        }

        public Task<List<transfer_category>> GetTransfer_Categories()
        {
            return _adminDAO.GetTransfer_Categories();
        }

        public Task<List<trip_category>> GetTripCategories()
        {
            return _adminDAO.GetTripCategories();
        }

        public Task<List<TripTranslationGrp>> GetTripTranslationGrp(long? trip_id)
        {
            return _adminDAO.GetTripTranslationGrp(trip_id);
        }

        public Task<List<child_policy_setting>> GetTrip_ChildPolicy(long? trip_id)
        {
            return _adminDAO.GetTrip_ChildPolicy(trip_id);
        }

        public Task<List<TripMainCast>> GetTrip_Mains(int destination_id, int trip_type)
        {
            return _adminDAO.GetTrip_Mains(destination_id, trip_type);
        }

        public TripMainCastPag GetTrip_MainsWithPag(TripMainReq req)
        {
            return _adminDAO.GetTrip_MainsWithPag(req);
        }

        public Task<List<trip_pickups_translation>> GetTrip_Pickups_Translations(long? trip_pickup_id)
        {
            return _adminDAO.GetTrip_Pickups_Translations(trip_pickup_id);
        }

        public Task<List<trip_price>> GetTrip_Prices(long? trip_id)
        {
            return _adminDAO.GetTrip_Prices(trip_id);
        }

        public Task<List<tbl_currency>> Get_Currencies()
        {
            return _adminDAO.Get_Currencies();
        }

        public Task<List<tbl_language>> Get_Languages()
        {
            return _adminDAO.Get_Languages();
        }

        public ResponseCls saveDestinationImage(List<destination_img> row)
        {
            return _adminDAO.saveDestinationImage(row);
        }

        public ResponseCls SaveDestinationTranslations(destination_translation row)
        {
            return _adminDAO.SaveDestinationTranslations(row);
        }

        public ResponseCls SaveFacilityTranslation(FacilityTranslationReq row)
        {
            return _adminDAO.SaveFacilityTranslation(row);
        }

        public ResponseCls SaveMainDestination(destination_main row)
        {
            return _adminDAO.SaveMainDestination(row);
        }

        public ResponseCls SaveMainFacility(facility_main row)
        {
            return _adminDAO.SaveMainFacility(row);
        }

        public ResponseCls SaveMainTrip(trip_main row)
        {
            return _adminDAO.SaveMainTrip(row);
        }

        public ResponseCls SaveMainTripPickups(TripsPickupSaveReq row)
        {
            return _adminDAO.SaveMainTripPickups(row);
        }

        public ResponseCls SaveTransferCategory(TransferCategorySaveReq row)
        {
            return _adminDAO.SaveTransferCategory(row);
        }

        public ResponseCls SaveTripChildPolicy(ChildPolicyPricesReq row)
        {
            return _adminDAO.SaveTripChildPolicy(row);
        }

        public ResponseCls saveTripImage(List<trip_img> lst)
        {
            return _adminDAO.saveTripImage(lst);
        }

        public ResponseCls SaveTripPickupsTranslations(TripsPickupTranslationSaveReq row)
        {
            return _adminDAO.SaveTripPickupsTranslations(row);
        }

        public ResponseCls SaveTripPrices(TripPricesReq row)
        {
            return _adminDAO.SaveTripPrices(row);
        }

        public ResponseCls SaveTripTranslation(TripTranslationReq row)
        {
            return _adminDAO.SaveTripTranslation(row);
        }

        public ResponseCls UpdateDestinationImage(DestinationImgUpdateReq cls)
        {
            return _adminDAO.UpdateDestinationImage(cls);
        }

        public ResponseCls UpdateTripImage(TripImgUpdateReq trip)
        {
            return _adminDAO.UpdateTripImage(trip);
        }
    }
}
