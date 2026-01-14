using ExcursionApp.Data.Data;
using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models;
using ExcursionApp.Data.Models.Bookings;
using ExcursionApp.Data.Models.Bookings.Client;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.profile;
using ExcursionApp.Data.Models.trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExcursionApp.Data
{
    public class ClientDAO
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly excursion_client_dbContext _db;

        public ClientDAO(excursion_client_dbContext db, IStringLocalizer<Messages> localizer)
        {
            _db = db;
            _localizer = localizer;
        }
        #region destination

        public List<DestinationResponse> getDestinations(DestinationReq req)
        {
            try
            {

                var result = from trans in _db.destination_translations.Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() && wr.active == true)
                             join dest in _db.destination_mains.Where(wr => wr.active == true && wr.country_code.ToLower() == (System.String.IsNullOrEmpty(req.country_code) ? wr.country_code.ToLower() : req.country_code.ToLower()) && wr.leaf == req.leaf && wr.parent_id==0) on trans.destination_id equals dest.id         // INNER JOIN
                             join img in _db.destination_imgs on trans.id equals img.destination_id into DestAll
                             from combined in DestAll.DefaultIfEmpty()               // LEFT JOIN
                             select new DestinationResponse
                             {
                                 destination_id = trans.destination_id,
                                 id = trans.id,
                                 country_code = dest.country_code,
                                 active = dest.active,
                                 dest_code = dest.dest_code,
                                 dest_description = trans.dest_description,
                                 dest_name = trans.dest_name,
                                 img_path = combined != null ? "http://api.raccoon24.com/" + combined.img_path : null,
                                 lang_code = trans.lang_code,
                                 dest_default_name = dest.dest_default_name,
                                 route = dest.route,
                                 parent_id= dest.parent_id
                             };

                return result.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DestinationTree> GetDestination_Tree(DestinationReq req)
        {

            try
            {
                var main = from trans in _db.destination_translations.Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() && wr.active == true)
                           join dest in _db.destination_mains.Where(wr => wr.active == true && wr.country_code.ToLower() == (System.String.IsNullOrEmpty(req.country_code) ? wr.country_code.ToLower() : req.country_code.ToLower())) on trans.destination_id equals dest.id         // INNER JOIN
                           join img in _db.destination_imgs on trans.id equals img.destination_id into DestAll
                           from combined in DestAll.DefaultIfEmpty()               // LEFT JOIN
                           select new DestinationResponse
                           {
                               destination_id = trans.destination_id,
                               id = trans.id,
                               country_code = dest.country_code,
                               active = dest.active,
                               dest_code = dest.dest_code,
                               dest_description = trans.dest_description,
                               dest_name = trans.dest_name,
                               img_path = combined != null ? "http://api.raccoon24.com/" + combined.img_path : null,
                               lang_code = trans.lang_code,
                               dest_default_name = dest.dest_default_name,
                               route = dest.route,
                               leaf = dest.leaf,
                               parent_id = dest.parent_id
                           };


                var result = GetDestination_TreeMain(main.ToList(), 0).ToList();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DestinationTree> GetDestination_TreeMain(List<DestinationResponse> lst, int? parentId)
        {

            return lst
                   .Where(x => x.parent_id == parentId)
                   .ToList()
                  .Select(s => new DestinationTree
                  {
                      leaf = s.leaf,
                      lang_code = s.lang_code,
                      parent_id = s.parent_id,
                      active = s.active,
                      country_code = s.country_code,
                      destination_id = s.destination_id,
                      dest_code = s.dest_code,
                      dest_default_name = s.dest_default_name,
                      dest_description = s.dest_description,
                      dest_name = s.dest_name,
                      id = s.id,
                      img_path = s.img_path,
                      route = s.route,
                      children = GetDestination_TreeMain(lst, s.destination_id).ToList(),

                  })
                .ToList();
        }

        #endregion

        #region trips
        public async Task<List<trip_category>> GetTripCategories()
        {
            return await _db.trip_categories.ToListAsync();
        }
        //get facilities for specific trip
        public List<TripFacility> getFacilityForTrip(long? trip_id, string lang_code, bool? isExtra)
        {
            try
            {

                var result =
                   from TFAC in _db.trip_facilities.Where(wr => wr.trip_id == trip_id)
                   join TRANS in _db.facility_translations.Where(wr => wr.lang_code.ToLower() == lang_code.ToLower())
                   on TFAC.facility_id equals TRANS.facility_id
                   into TRIPFAC

                   from combinedFACT in TRIPFAC.DefaultIfEmpty() // LEFT JOIN Customers
                   join FACM in _db.facility_mains.Where(wr => wr.active == true && wr.is_extra == isExtra)

                      on TFAC.facility_id equals FACM.id into FacAll
                   from combinedFACM in FacAll.DefaultIfEmpty() // LEFT JOIN Payments
                   select new TripFacility
                   {
                       facility_desc = combinedFACT != null ? combinedFACT.facility_desc : "",
                       facility_name = combinedFACT != null ? combinedFACT.facility_name : "",
                       extra_price = combinedFACM != null ? combinedFACM.extra_price : 0,
                       currency_code = combinedFACM != null ? combinedFACM.currency_code : "",
                       is_extra = combinedFACM != null ? combinedFACM.is_extra : false,
                       facility_id = TFAC.facility_id


                   };

                //var result = from TFAC in _db.trip_facilities.Where(wr => wr.trip_id == trip_id)
                //             join TRANS in _db.facility_translations.Where(wr => wr.lang_code.ToLower() == lang_code.ToLower()) on TFAC.facility_id equals TRANS.facility_id into TRIPFAC
                //             from m in TRIPFAC.DefaultIfEmpty()
                //             select new TripFacility
                //             {
                //                 facility_desc = m.facility_desc,
                //                 facility_name = m.facility_name,


                //             };
                return result.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //get images list for specific trip
        public async Task<List<trip_img>> GetImgsByTrip(decimal? trip_id)
        {
            try
            {
                return await _db.trip_imgs.Where(wr => wr.trip_id == trip_id).Select(s => new trip_img
                {
                    id = s.id,
                    img_height = s.img_height,
                    img_name = s.img_name,
                    img_path = "http://api.raccoon24.com/" + s.img_path,
                    img_resize_path = "http://api.raccoon24.com/" + s.img_resize_path,
                    img_width = s.img_width,
                    is_default = s.is_default,
                    trip_id = s.trip_id,
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //get trips and top trips with its details 

        public trips_wishlist CheckIfTripInWishList(long? trip_id, string client_id, int? trip_type)
        {
            try
            {
                var result = _db.trips_wishlists.Where(wr => wr.trip_id == trip_id && wr.client_id == client_id && wr.trip_type == trip_type).SingleOrDefault();
                if (result == null)
                {
                    return new trips_wishlist();
                }
                else
                {
                    return result;

                }
            }
            catch (Exception ex)
            {
                return new trips_wishlist();
            }
        }
        //public async Task<List<TripsAll>> GetTripsAll(TripsReq req)
        //{
        //    try
        //    {
        //        var trips = new List<tripwithdetail>();
        //        if (req.trip_types.Count == 0)
        //        {
        //             trips = await _db.tripwithdetails
        //                                    .Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() &&
        //                                                 wr.show_in_top == (req.show_in_top == false ? wr.show_in_top : req.show_in_top) &&
        //                                                 wr.destination_id == (req.destination_id == 0 ? wr.destination_id : req.destination_id) &&
        //                                                 wr.show_in_slider == (req.show_in_slider == false ? wr.show_in_slider : req.show_in_slider))
        //                                    .ToListAsync();
        //        }
        //        else
        //        {
        //             trips = await _db.tripwithdetails
        //                .Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() &&
        //                             //wr.trip_type == (req.trip_type == 0 ? wr.trip_type : req.trip_type) &&

        //                             wr.show_in_top == (req.show_in_top == false ? wr.show_in_top : req.show_in_top) &&
        //                             wr.destination_id == (req.destination_id == 0 ? wr.destination_id : req.destination_id) &&
        //                             //req.trip_types.Contains(wr.trip_type) &&
        //                            req.trip_types.Contains(wr.trip_type) &&
        //                             wr.show_in_slider == (req.show_in_slider == false ? wr.show_in_slider : req.show_in_slider))
        //                .ToListAsync();
        //        }


        //        return trips.Select(s => MapToTripsAll(s, req.currency_code, req.client_id))
        //             .Where(wr => wr.trip_min_price >= req.min_price && wr.trip_min_price <= req.max_price)
        //            .ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<TripsAll>();
        //    }
        //}
        public async Task<List<TripsAll>> GetTripsAll(TripsReq req)
        {
            try
            {
                var query = _db.tripwithdetails.AsQueryable();

                // normalize lang_code once
                string langCode = req.lang_code.ToLower();

                query = query.Where(wr =>
                    wr.lang_code.ToLower() == langCode &&
                    //(req.show_in_top == false || wr.show_in_top == req.show_in_top) &&
                     //(req.destination_id == 0 || wr.destination_id == req.destination_id) &&
                    wr.show_in_top == (req.show_in_top == false ? wr.show_in_top : req.show_in_top) &&
                    wr.show_in_slider == (req.show_in_slider == false ? wr.show_in_slider : req.show_in_slider) &&
                    (req.trip_types.Count == 0 || req.trip_types.Contains(wr.trip_type)) &&
                    (req.destination_lst.Count == 0 || req.destination_lst.Contains(wr.destination_id))
                );

                var trips = await query.ToListAsync();

                return trips
                    .Select(s => MapToTripsAll(s, req.currency_code, req.client_id))
                    .Where(wr => wr.trip_min_price >= req.min_price && wr.trip_min_price <= req.max_price)
                    .ToList();
            }
            catch
            {
                return new List<TripsAll>();
            }
        }
        public async Task<TripsAll> GetTripDetails(TripDetailsReq req)
        {
            try
            {
                var trips = await _db.tripwithdetails
                    .Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() &&
                                 wr.trip_id == req.trip_id &&
                                 wr.trip_type == (req.trip_type == 0 ? wr.trip_type : req.trip_type)
                                 //&& wr.currency_code.ToLower() == req.currency_code.ToLower()
                                 //(string.IsNullOrEmpty(wr.currency_code) || wr.currency_code.ToLower() == req.currency_code.ToLower())
                                 //&& (string.IsNullOrEmpty(wr.transfer_currency) || wr.transfer_currency.ToLower() == req.currency_code.ToLower())

                                 )
                    .ToListAsync();
                var result = trips.Select(s => MapToTripsAll(s, req.currency_code, req.client_id)).ToList();
                //var result = trips.Select(s => new TripsAll
                //{
                //    destination_id = s.destination_id,
                //    lang_code = s.lang_code,
                //    country_code = s.country_code,
                //    currency_code = req.currency_code,
                //    default_img = "http://api.raccoon24.com/" + s.default_img,
                //    dest_code = s.dest_code,
                //    dest_default_name = s.dest_default_name,
                //    pickup = s.pickup,
                //    show_in_slider = s.show_in_slider,
                //    show_in_top = s.show_in_top,
                //    trip_code = s.trip_code,
                //    trip_default_name = s.trip_default_name,
                //    trip_description = s.trip_description,
                //    trip_duration = s.trip_duration,
                //    trip_highlight = s.trip_highlight,
                //    trip_id = s.trip_id,
                //    trip_includes = s.trip_includes,
                //    trip_name = s.trip_name,
                //    //trip_origin_price = s.trip_origin_price,
                //    //trip_sale_price = s.trip_sale_price,
                //    trip_trans_id = s.trip_trans_id,
                //    isfavourite = string.IsNullOrEmpty(req.client_id) ? false : (CheckIfTripInWishList(s.trip_id, req.client_id, s.trip_type).id == 0 ? false : true),
                //    trip_type = s.trip_type,
                //    wish_id = string.IsNullOrEmpty(req.client_id) ? 0 : CheckIfTripInWishList(s.trip_id, req.client_id, s.trip_type).id,
                //    wsh_created_at = string.IsNullOrEmpty(req.client_id) ? null : (CheckIfTripInWishList(s.trip_id, req.client_id, s.trip_type).created_at == null ? null : CheckIfTripInWishList(s.trip_id, req.client_id, s.trip_type).created_at.Value.ToString("dd-MM-yyyy")),
                //    //wsh_created_at = null,
                //    dest_route = s.dest_route,
                //    route = s.route,
                //    client_id = req.client_id,
                //    facilities = getFacilityForTrip(s.trip_id, s.lang_code, false).ToList(),
                //    imgs = GetImgsByTrip(s.trip_id).Result,
                //    important_info = s.important_info,
                //    trip_details = s.trip_details,
                //    trip_not_includes = s.trip_not_includes,
                //    //trip_price_notes = s.trip_price_notes,
                //    //trip_child_price = s.trip_child_price,
                //    //transfer_child_price = s.transfer_child_price,
                //    //transfer_category_notes = s.transfer_category_notes,
                //    //transfer_currency = s.transfer_currency,
                //    trip_category_name = s.trip_category_name,
                //    trip_category_code = s.trip_category_code,
                //    //max_capacity = s.max_capacity,
                //    //max_price = s.max_price,
                //    //min_capacity = s.min_capacity,
                //    //min_price = s.min_price,
                //    trip_code_auto = s.trip_code_auto,
                //    //transfer_category_name = s.transfer_category_name,
                //    //transfer_category__code = s.transfer_category__code,
                //    cancelation_policy = s.cancelation_policy,
                //    release_days=s.release_days,
                //    trip_max_price = _db.trip_prices.Where(wr => wr.trip_id == s.trip_id && wr.currency_code == req.currency_code).Max(m => m.trip_sale_price),
                //    trip_min_price= _db.trip_prices.Where(wr => wr.trip_id == s.trip_id && wr.currency_code == req.currency_code).Min(m => m.trip_sale_price),
                //    trip_max_capacity = _db.trip_prices.Where(wr => wr.trip_id == s.trip_id && wr.currency_code == req.currency_code).Max(m => m.pax_to),
                //    total_reviews = _db.tbl_reviews.Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type).Count(),
                //    review_rate = _db.tbl_reviews.Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type).Max(m => m.review_rate)
                //}).ToList();

                if (result != null)
                {
                    return result.SingleOrDefault();
                }
                return new TripsAll();
            }
            catch (Exception ex)
            {
                return new TripsAll();
            }
        }

        //get wish list for Specific client
        public async Task<List<TripsAll>> GetClientWishList(ClientWishListReq req)
        {
            try
            {
                var trips = await _db.tripwithdetails
                     .Where(wr => wr.lang_code == req.lang_code &&
                                   // wr.currency_code.ToLower() == req.currency_code.ToLower() && 
                                   //(string.IsNullOrEmpty(wr.currency_code) || wr.currency_code.ToLower() == req.currency_code.ToLower()) &&
                                   //&& (string.IsNullOrEmpty(wr.transfer_currency) || wr.transfer_currency.ToLower() == req.currency_code.ToLower()) &&
                                   wr.trip_type == (req.trip_type == 0 ? wr.trip_type : req.trip_type))
                     .Join(_db.trips_wishlists.Where(wr => wr.client_id == req.client_id),
                                    TRIP => new { TRIP.trip_id },
                                    WSH => new { WSH.trip_id },
                                    (TRIP, WSH) => new TripsAll
                                    {
                                        destination_id = TRIP.destination_id,
                                        lang_code = TRIP.lang_code,
                                        country_code = TRIP.country_code,
                                        default_img = "http://api.raccoon24.com/" + TRIP.default_img,
                                        dest_code = TRIP.dest_code,
                                        dest_default_name = TRIP.dest_default_name,
                                        pickup = TRIP.pickup,
                                        show_in_slider = TRIP.show_in_slider,
                                        show_in_top = TRIP.show_in_top,
                                        trip_code = TRIP.trip_code,
                                        trip_default_name = TRIP.trip_default_name,
                                        trip_description = TRIP.trip_description,
                                        trip_duration = TRIP.trip_duration,
                                        trip_highlight = TRIP.trip_highlight,
                                        trip_id = TRIP.trip_id,
                                        trip_includes = TRIP.trip_includes,
                                        trip_name = TRIP.trip_name,
                                        trip_trans_id = TRIP.trip_trans_id,
                                        wish_id = WSH.id,
                                        client_id = WSH.client_id,
                                        wsh_created_at = (WSH != null && WSH.created_at != null) ? WSH.created_at.Value.ToString("dd-MM-yyyy") : null,
                                        trip_type = TRIP.trip_type,
                                        isfavourite = (WSH != null && WSH.id != 0) ? true : false,
                                        dest_route = TRIP.dest_route,
                                        important_info = TRIP.important_info,
                                        route = TRIP.route,
                                        trip_not_includes = TRIP.trip_not_includes,
                                        trip_details = TRIP.trip_details,
                                        trip_category_code = TRIP.trip_category_code,
                                        trip_category_name = TRIP.trip_category_name,
                                        cancelation_policy = TRIP.cancelation_policy,
                                        currency_code = req.currency_code,
                                        trip_code_auto = TRIP.trip_code_auto,
                                        release_days = TRIP.release_days,
                                        dest_name= TRIP.dest_name
                                    }).ToListAsync();

                return trips.Select(s => new TripsAll
                {
                    destination_id = s.destination_id,
                    lang_code = s.lang_code,
                    country_code = s.country_code,
                    currency_code = s.currency_code,
                    default_img = s.default_img,
                    dest_code = s.dest_code,
                    dest_default_name = s.dest_default_name,
                    pickup = s.pickup,
                    show_in_slider = s.show_in_slider,
                    show_in_top = s.show_in_top,
                    trip_code = s.trip_code,
                    trip_default_name = s.trip_default_name,
                    trip_description = s.trip_description,
                    trip_duration = s.trip_duration,
                    trip_highlight = s.trip_highlight,
                    trip_id = s.trip_id,
                    trip_includes = s.trip_includes,
                    trip_name = s.trip_name,
                    trip_trans_id = s.trip_trans_id,
                    wish_id = s.wish_id,
                    client_id = s.client_id,
                    wsh_created_at = s.wsh_created_at,
                    trip_type = s.trip_type,
                    isfavourite = s.isfavourite,
                    dest_route = s.dest_route,
                    important_info = s.important_info,
                    route = s.route,
                    trip_not_includes = s.trip_not_includes,
                    trip_details = s.trip_details,
                    total_reviews = _db.tbl_reviews.Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type).Count(),
                    review_rate = _db.tbl_reviews.Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type).Max(m => m.review_rate),
                    facilities = getFacilityForTrip(s.trip_id, s.lang_code, false).ToList(),
                    imgs = GetImgsByTrip(s.trip_id).Result,
                    trip_category_code = s.trip_category_code,
                    trip_category_name = s.trip_category_name,
                    trip_code_auto = s.trip_code_auto,
                    cancelation_policy = s.cancelation_policy,
                    release_days = s.release_days,
                    trip_max_price= GetTripPrice(s.trip_id, req.currency_code,"max"),
                    trip_min_price = GetTripPrice(s.trip_id, req.currency_code, "min"),
                    dest_name = s.dest_name,
                    //trip_max_price = _db.trip_prices.Where(wr => wr.trip_id == s.trip_id && wr.currency_code == req.currency_code).Max(m => m.trip_sale_price),
                    //trip_min_price = _db.trip_prices.Where(wr => wr.trip_id == s.trip_id && wr.currency_code == req.currency_code).Min(m => m.trip_sale_price)
                }).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public decimal? GetTripPrice(long? tripId, string? currencyCode, string? mode)
        {
            mode = mode?.ToLower();

            // 1. Prices for requested currency
            var queryRequested = _db.trip_prices
                .Where(wr => wr.trip_id == tripId &&
                             wr.currency_code!.ToLower() == currencyCode!.ToLower());

            // 2. Pick max or min for requested currency
            decimal? requestedResult = null;

            if (mode == "max")
                requestedResult = queryRequested.Max(wr => (decimal?)wr.trip_sale_price);
            else if (mode == "min")
                requestedResult = queryRequested.Min(wr => (decimal?)wr.trip_sale_price);


            // If found → return
            if (requestedResult != null)
                return requestedResult;

            // 3. Fallback to any currency (max or min)
            var queryAll = _db.trip_prices.Where(wr => wr.trip_id == tripId);

            if (mode == "max")
                return queryAll.Max(wr => (decimal?)wr.trip_sale_price);

            if (mode == "min")
                return queryAll.Min(wr => (decimal?)wr.trip_sale_price);

            // Invalid mode → return null
            return null;
        }

        private TripsAll MapToTripsAll(tripwithdetail s, string? currency_code, string? client_id)
        {

            var wishlist = (string.IsNullOrEmpty(client_id)
            ? null
            : CheckIfTripInWishList(s.trip_id, client_id, s.trip_type));
            //decimal? trip_max_price = _db.trip_prices
            //        .Where(wr => wr.trip_id == s.trip_id && wr.currency_code.ToLower() == currency_code.ToLower())
            //        .Max(m => m.trip_sale_price);
            decimal? trip_max_price = GetTripPrice(s.trip_id, currency_code!.ToLower(), "max");
            decimal? trip_min_price = GetTripPrice(s.trip_id, currency_code.ToLower(), "min");
            return new TripsAll
            {
                destination_id = s.destination_id,
                lang_code = s.lang_code,
                country_code = s.country_code,
                currency_code = currency_code,
                default_img = "http://api.raccoon24.com/" + s.default_img,
                dest_code = s.dest_code,
                dest_default_name = s.dest_default_name,
                pickup = s.pickup,
                show_in_slider = s.show_in_slider,
                show_in_top = s.show_in_top,
                trip_code = s.trip_code,
                trip_default_name = s.trip_default_name,
                trip_description = s.trip_description,
                trip_duration = s.trip_duration,
                trip_highlight = s.trip_highlight,
                trip_id = s.trip_id,
                trip_includes = s.trip_includes,
                trip_name = s.trip_name,
                trip_trans_id = s.trip_trans_id,
                isfavourite = (wishlist != null && wishlist.id != 0),
                trip_type = s.trip_type,
                wish_id = wishlist?.id ?? 0,
                wsh_created_at = wishlist?.created_at?.ToString("dd-MM-yyyy"),
                dest_route = s.dest_route,
                route = s.route,
                client_id = client_id,
                facilities = getFacilityForTrip(s.trip_id, s.lang_code, false).ToList(),
                imgs = GetImgsByTrip(s.trip_id).Result,
                important_info = s.important_info,
                trip_details = s.trip_details,
                trip_not_includes = s.trip_not_includes,
                trip_category_name = s.trip_category_name,
                trip_category_code = s.trip_category_code,
                trip_code_auto = s.trip_code_auto,
                cancelation_policy = s.cancelation_policy,
                release_days = s.release_days,
                trip_max_price = trip_max_price,
                trip_min_price = trip_min_price,
                trip_max_capacity = _db.trip_prices
                    .Where(wr => wr.trip_id == s.trip_id && wr.currency_code.ToLower() == currency_code.ToLower())
                    .Max(m => m.pax_to),
                total_reviews = _db.tbl_reviews
                    .Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type)
                    .Count(),
                review_rate = _db.tbl_reviews
                    .Where(wr => wr.trip_id == s.trip_id && wr.trip_type == s.trip_type)
                    .Max(m => m.review_rate),
                max_child_age = _db.child_policy_settings
                    .Where(wr => wr.trip_id == s.trip_id && wr.currency_code.ToLower() == currency_code.ToLower())
                    .Max(m => m.age_to),
                trip_order = s.trip_order,
                is_comm_soon = s.is_comm_soon,
                dest_name=s.dest_name,
                child_lst = GetChild_Prices(currency_code ,s.trip_id, trip_max_price)
            };
        }

        public List<Child_Prices> GetChild_Prices(string? currency_code, long? trip_id, decimal? adult_price)
        {
            /// <summary>
            /// 1 =Free
            /// 2=% of Adult Price
            /// 3=Fixed Amount
            /// </summary>
            try
            {
                var recorded = _db.child_policy_settings
                    .Where(wr => wr.trip_id == trip_id && wr.currency_code!.ToLower() == currency_code!.ToLower() && wr.pricing_type != 1).ToList();
                return recorded.Select(s => new Child_Prices
                {
                    age_from = s.age_from,
                    age_to = s.age_to,
                    child_price = s.pricing_type == 3 ? s.child_price : (adult_price * s.child_price)
                }).ToList();
            }
            catch (Exception ex)
            {
                return new List<Child_Prices>();
            }
        }

        //get trips which shown in home page slider
        public async Task<List<tripwithdetail>> GetTripsForSlider(TripsReq req)
        {
            try
            {
                var trips = await _db.tripwithdetails
                    .Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower() &&
                                wr.show_in_slider == true &&
                                wr.trip_type == (req.trip_type == 0 ? wr.trip_type : req.trip_type) &&
                                wr.destination_id == (req.destination_id == 0 ? wr.destination_id : req.destination_id)
                                //wr.currency_code.ToLower() == req.currency_code.ToLower()
                                //(string.IsNullOrEmpty(wr.currency_code) || wr.currency_code.ToLower() == req.currency_code.ToLower())
                                //&& (string.IsNullOrEmpty(wr.transfer_currency) || wr.transfer_currency.ToLower() == req.currency_code.ToLower())

                                )
                    .ToListAsync();


                return trips;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //get Pickups for spicifics trips
        public async Task<List<TripsPickupResponse>> GetPickupsForTrip(PickupsReq req)
        {
            try
            {
                var result = await _db.trip_pickups_mains.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type)
                                   .Join(_db.trip_pickups_translations.Where(wr => wr.lang_code.ToLower() == req.lang_code.ToLower()),
                                        MAIN => new { trip_pickup_id = MAIN.id },
                                        TRANS => new { trip_pickup_id = (long) TRANS.trip_pickup_id },
                                        (MAIN, TRANS) => new TripsPickupResponse
                                        {
                                            trip_pickup_id = MAIN.id,
                                            lang_code = TRANS.lang_code,
                                            order = MAIN.order,
                                            pickup_code = MAIN.pickup_code,
                                            pickup_default_name = MAIN.pickup_default_name,
                                            pickup_description = TRANS.pickup_description,
                                            pickup_name = TRANS.pickup_name,
                                            trip_id = MAIN.trip_id,
                                            trip_type = MAIN.trip_type,
                                            duration = MAIN.duration,
                                            pickup_lat= MAIN.pickup_lat,
                                            pickup_long= MAIN.pickup_long
                                        }
                                       ).OrderBy(x => x.order).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //get clients reviews for specific trip 
        //used for exercusion trip && transfer trip
        //trip_type = 1 mean exercusion 
        //trip_type = 2 mean transfer
        // pageNumber = 1; // Current page number (1-based)
        // pageSize = 10;  // Number of items per page
        public async Task<ClientsReviewsResponse> GetClientsReviews(ClientsReviewsReq req)
        {

            try
            {
                var totalRecords = await _db.tbl_reviews.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type).CountAsync();
                var average_review_rate = await _db.tbl_reviews.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type).MaxAsync(m => m.review_rate);
                var reviews = await _db.tbl_reviews.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type)
                                            .Select(s => new ClientsReviews
                                            {
                                                trip_id = s.trip_id,
                                                client_id = s.client_id,
                                                entry_date = s.entry_date,
                                                //entry_dateStr = s.entry_date.ToString(),
                                                id = s.id,
                                                review_description = s.review_description,
                                                review_rate = s.review_rate,
                                                review_title = s.review_title,
                                                trip_type = s.trip_type,
                                                client_name=s.client_name
                                            })
                                             .Skip((req.pageNumber - 1) * req.pageSize)
                                             .Take(req.pageSize)
                                            .ToListAsync();
                foreach (var r in reviews)
                {
                    r.entry_dateStr = r.entry_date?.ToString("yyyy-MM-dd");
                }
                return new ClientsReviewsResponse
                {
                    reviews = reviews,
                    average_review_rate = average_review_rate,
                    totalPages = totalRecords
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //save client reviews for trip
        public ResponseCls SaveReviewForTrip(tbl_review row)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {
                row.entry_date = DateTime.Now;
                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.tbl_reviews.Where(wr => wr.client_id == row.client_id && wr.trip_id == row.trip_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["AddReviewDuplicate"] };
                    }
                    if (_db.tbl_reviews.Count() > 0)
                    {
                        maxId = _db.tbl_reviews.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.tbl_reviews.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    _db.tbl_reviews.Update(row);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = row.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //save client wishList

        public ResponseCls AddTripToWishList(TripsWishlistReq cls)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {
                trips_wishlist row = new trips_wishlist
                {
                    client_id = cls.client_id,
                    created_at = DateTime.Now,
                    id = cls.id,
                    trip_id = cls.trip_id,
                    trip_type = cls.trip_type
                };
                if (cls.delete == true)
                {
                    _db.Remove(row);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trips_wishlists.Where(wr => wr.client_id == row.client_id && wr.trip_id == row.trip_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trips_wishlists.Count() > 0)
                    {
                        maxId = _db.trips_wishlists.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.trips_wishlists.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    _db.trips_wishlists.Update(row);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = row.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //get count of wishlist for specific client

        public async Task<int> GetWishListCount(string client_id)
        {
            try
            {
                return await _db.trips_wishlists.Where(wr => wr.client_id == client_id).CountAsync();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion

        #region "booking"
        public async Task<ResponseCls> CancelBooking(long? booking_id, string? client_id)
        {
            ResponseCls response = new();
            try
            {
                var booking = await _db.trips_bookings.Where(wr => wr.id == booking_id && wr.client_id == client_id).SingleOrDefaultAsync();
                //var booking = await _db.trips_bookings.FirstOrDefaultAsync(wr => wr.id == booking_id && wr.client_id == client_id);
                if (booking != null)
                {
                    booking.booking_status = 3;
                    _db.trips_bookings.Update(booking);
                    await _db.SaveChangesAsync();
                    response = new ResponseCls { errors = null, msg = "", success = true };
                }
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = null, msg = "", success = true };
            }
            return response;
        }
        public async Task<ResponseCls> DeleteBooking(long? booking_id, string? client_id)
        {
            ResponseCls response = new();
            try
            {
                var booking = await _db.trips_bookings.Where(wr => wr.id == booking_id && wr.client_id == client_id).SingleOrDefaultAsync();
                //var booking = await _db.trips_bookings.FirstOrDefaultAsync(wr => wr.id == booking_id && wr.client_id == client_id);
                if (booking != null)
                {
                    _db.Remove(booking);
                    await _db.SaveChangesAsync();
                    response = new ResponseCls { errors = null, msg = "", success = true };
                }
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = null, msg = "", success = true };
            }
            return response;
        }
        public async Task<List<BookingSummary>> GetBookingWithDetails(BookingReq req)
        {
            try
            {
                var result = await _db.bookingwithdetails.Where(
                                                        wr => wr.lang_code == req.lang_code &&
                                                              //wr.booking_id == req.booking_id &&
                                                              wr.booking_status_id == req.booking_status_id &&
                                                              wr.client_id == req.client_id
                                                         )
                    .ToListAsync();
                if (result != null)
                {
                    return result.Select(s => new BookingSummary
                    {
                        client_name = s.client_name,
                        trip_type = s.trip_type,
                        trip_id = s.trip_id,
                        booking_code = s.booking_code,
                        booking_code_auto = s.booking_code_auto,
                        booking_date = s.booking_date,
                        booking_datestr = s.booking_datestr,
                        booking_id = s.booking_id,
                        booking_notes = s.booking_notes,
                        booking_status = s.booking_status,
                        booking_status_id = s.booking_status_id,
                        cancelation_policy = s.cancelation_policy,
                        child_num = s.child_num,
                        client_email = s.client_email,
                        client_id = s.client_id,
                        client_nationality = s.client_nationality,
                        client_phone = s.client_phone,
                        currency_code = s.currency_code,
                        default_img = s.default_img,
                        gift_code = s.gift_code,
                        infant_num = s.infant_num,
                        lang_code = s.lang_code,
                        pickup_address = s.pickup_address,
                        pickup_time = s.pickup_time,
                        review_rate = s.review_rate,
                        total_pax = s.total_pax,
                        total_price = s.total_price,
                        trip_code = s.trip_code,
                        trip_date = s.trip_date,
                        trip_datestr = s.trip_datestr,
                        trip_description = s.trip_description,
                        trip_name = s.trip_name,
                        release_days = s.release_days,
                        trip_code_auto = s.trip_code_auto,
                        extras = GetExtraAssignedToBooking(s.booking_id, req.lang_code, false).ToList(),
                        extras_obligatory = GetExtraAssignedToBooking(s.booking_id, req.lang_code, true).ToList()
                    }).ToList();
                }
                return new List<BookingSummary>();
            }
            catch (Exception ex)
            {
                return new List<BookingSummary>();
            }
        }

        public async Task<BookingDetailsList> ConfirmBookingLst(ConfirmBookingListReq req)
        {

            try
            {
                var items = await _db.trips_bookings
                             .Where(e => req.booking_ids.Contains(e.id))
                             .ToListAsync();
               
                if (items != null)
                {
                    foreach (var booking in items)
                    {
                        booking.booking_status = 2;
                        booking.booking_notes = req.booking_notes;
                        booking.client_email = req.client_email;
                        booking.client_name = req.client_name;
                        booking.client_nationality = req.client_nationality;
                        booking.client_phone = req.client_phone;

                    }
                    await _db.SaveChangesAsync();

                    var result = await _db.bookingwithdetails.Where(
                                                       wr => wr.lang_code == req.lang_code &&
                                                             req.booking_ids.Contains(wr.booking_id) &&
                                                             wr.client_id == req.client_id
                                                        ).ToListAsync();

                    var records = result.Select(s => new BookingWithTripDetailsAll
                    {
                        booking_code = s.booking_code,
                        booking_datestr = s.booking_datestr,
                        booking_id = s.booking_id,
                        child_num = s.child_num,
                        client_email = s.client_email,
                        client_id = s.client_id,
                        client_nationality = s.client_nationality,
                        client_phone = s.client_phone,
                        currency_code = s.currency_code,
                        gift_code = s.gift_code,
                        infant_num = s.infant_num,
                        lang_code = s.lang_code,
                        pickup_address = s.pickup_address,
                        pickup_time = s.pickup_time,
                        review_rate = s.review_rate,
                        total_pax = s.total_pax,
                        total_price = s.total_price,
                        trip_code = s.trip_code,
                        trip_datestr = s.trip_datestr,
                        trip_name = s.trip_name,
                        client_name = s.client_name,
                        trip_id = s.trip_id,
                        trip_type = s.trip_type,
                        pickups = GetPickupsForTrip(new PickupsReq { lang_code = req.lang_code, trip_id = s.trip_id, trip_type = s.trip_type }).Result
                    }).ToList();
                    return new BookingDetailsList
                    {
                        
                        bookings = records

                    };
                    //send mail to client && horizon reservation team

                }
                return new BookingDetailsList();
            }
            catch (Exception ex)
            {
                return new BookingDetailsList();
            }

        }
        public BookingWithTripDetailsAll ConfirmBooking(ConfirmBookingReq req)
        {

            try
            {
                trips_booking booking = _db.trips_bookings.Where(wr => wr.id == req.booking_id).SingleOrDefault();
                if (booking != null)
                {
                    booking.booking_status = 2;
                    _db.trips_bookings.Update(booking);
                    _db.SaveChanges();
                    var result = _db.bookingwithdetails.Where(
                                                       wr => wr.lang_code == req.lang_code &&
                                                             wr.booking_id == req.booking_id &&
                                                             wr.client_id == req.client_id
                                                        ).SingleOrDefault();
                    return new BookingWithTripDetailsAll
                    {
                        booking_code = result.booking_code,
                        booking_datestr = result.booking_datestr,
                        booking_id = result.booking_id,
                        child_num = result.child_num,
                        client_email = result.client_email,
                        client_id = result.client_id,
                        client_nationality = result.client_nationality,
                        client_phone = result.client_phone,
                        currency_code = result.currency_code,
                        gift_code = result.gift_code,
                        infant_num = result.infant_num,
                        lang_code = result.lang_code,
                        pickup_address = result.pickup_address,
                        pickup_time = result.pickup_time,
                        review_rate = result.review_rate,
                        total_pax = result.total_pax,
                        total_price = result.total_price,
                        trip_code = result.trip_code,
                        trip_datestr = result.trip_datestr,
                        trip_name = result.trip_name,
                        client_name = result.client_name,
                        trip_id = result.trip_id,
                        trip_type = result.trip_type,
                        pickups = GetPickupsForTrip(new PickupsReq { lang_code = req.lang_code, trip_id = result.trip_id, trip_type = result.trip_type }).Result


                    };
                    //send mail to client && horizon reservation team

                }
                return new BookingWithTripDetailsAll();
            }
            catch (Exception ex)
            {
                return new BookingWithTripDetailsAll();
            }

        }
        //public ResponseCls CalculateBookingPrice(long? booking_id , long? trip_id , int? adult_num, int? child_num,string currency,decimal extras_price)
        public BookingPrice CalculateBookingPrice(CalculateBookingPriceReq req)
        {
            BookingPrice response = new BookingPrice();
            decimal? total_price = 0;
            decimal? total_adult_price = 0;
            decimal? total_child_price = 0;
            decimal? final_price = 0;
            decimal? extras_price = 0;
            if (req.extra_lst != null && req.extra_lst.Count > 0)
            {
                foreach (var item in req.extra_lst)
                {
                    extras_price = extras_price + (item.extra_price * item.extra_count);
                }
            }

            try
            {
                //get trip details
                var trip = _db.trip_mains.Where(wr => wr.id == req.trip_id).SingleOrDefault();
                if (trip != null)
                {
                    var capacity = req.adult_num + req.child_num;
                    ////mean it trip is transfer type, get price data from tbl transfer_categories 
                    //if (trip.trip_type == 2)
                    //{
                    //    var transfer =  _db.transfer_categories.Where(wr => wr.id == trip.transfer_category_id && wr.min_capacity <= capacity && wr.max_capacity >= capacity && wr.currency_code.ToLower() == req.currency_code.ToLower()).SingleOrDefault();
                    //    total_price = transfer?.max_price ;
                    //}
                    //else
                    //{
                    //mean trip is diving or excursion or transfer get price data from tbl trip_prices depend on pax capacity
                    //if trip price doesnot contain pax range so skip check againt capacity
                    var price = _db.trip_prices.Where(wr => wr.trip_id == trip.id && wr.currency_code.ToLower() == req.currency_code.ToLower() && wr.pax_from <= capacity && (wr.pax_to >= capacity || wr.pax_to == 0)).SingleOrDefault();
                    total_adult_price = (price?.trip_sale_price * req.adult_num);

                    //calculte child price depend on policy assigned to trip
                    foreach (var age in req.childAges)
                    {
                        var policy = _db.child_policy_settings.FirstOrDefault(p => age >= p.age_from && age <= p.age_to && p.trip_id == req.trip_id && p.currency_code.ToLower() == req.currency_code.ToLower());
                        if (policy == null) continue;
                        decimal? child_price = 0;
                        switch (policy.pricing_type)
                        {
                            case 1:
                                child_price = 0;
                                break;
                            case 2:
                                child_price = price?.trip_sale_price * (policy.child_price / 100);
                                break;
                            case 3:
                                child_price = policy.child_price;
                                break;

                        }

                        total_child_price += child_price;
                    }
                    //}
                }
                final_price = total_adult_price + total_child_price + extras_price;
                if (req.booking_id > 0)
                {
                    //update booking
                    var booking = _db.trips_bookings.Where(wr => wr.id == req.booking_id).SingleOrDefault();

                    if (booking != null)
                    {
                        booking.total_price = final_price;
                        _db.trips_bookings.Update(booking);
                        _db.SaveChanges();
                        response = new BookingPrice { success = true, message = _localizer["UpdateBookingPrice"], final_price = final_price, total_adult_price = total_adult_price, total_child_price = total_child_price };
                    }

                }
                response = new BookingPrice { success = true, final_price = final_price, total_adult_price = total_adult_price, total_child_price = total_child_price };
            }
            catch (Exception ex)
            {
                //final_price = 0;
                response = new BookingPrice { message = _localizer["CheckAdmin"], success = false };
            }
            return response;
        }
        public ResponseCls CalculateBookingPriceOld(CalculateBookingPriceReq req)
        {
            decimal? total_price = 0;
            decimal? final_price = 0;
            decimal? extras_price = 0;
            if (req.extra_lst != null && req.extra_lst.Count > 0)
            {
                foreach (var item in req.extra_lst)
                {
                    extras_price = extras_price + (item.extra_price * item.extra_count);
                }
            }
            ResponseCls response = new ResponseCls();
            try
            {
                //get trip details
                var trip = _db.trip_mains.Where(wr => wr.id == req.trip_id).SingleOrDefault();
                if (trip != null)
                {
                    var capacity = req.adult_num + req.child_num;
                    ////mean it trip is transfer type, get price data from tbl transfer_categories 
                    //if (trip.trip_type == 2)
                    //{
                    //    var transfer =  _db.transfer_categories.Where(wr => wr.id == trip.transfer_category_id && wr.min_capacity <= capacity && wr.max_capacity >= capacity && wr.currency_code.ToLower() == req.currency_code.ToLower()).SingleOrDefault();
                    //    total_price = transfer?.max_price ;
                    //}
                    //else
                    //{
                    //mean trip is diving or excursion or transfer get price data from tbl trip_prices depend on pax capacity
                    var price = _db.trip_prices.Where(wr => wr.trip_id == trip.id && wr.currency_code.ToLower() == req.currency_code.ToLower() && wr.pax_from <= capacity && wr.pax_to >= capacity).SingleOrDefault();
                    total_price = (price?.child_price * req.child_num) + (price?.trip_sale_price * req.adult_num);
                    //}
                }
                final_price = total_price + extras_price;
                //update booking
                var booking = _db.trips_bookings.Where(wr => wr.id == req.booking_id).SingleOrDefault();

                if (booking != null)
                {
                    booking.total_price = final_price;
                    _db.trips_bookings.Update(booking);
                    _db.SaveChanges();
                    response = new ResponseCls { errors = null, success = true, idOut = booking.id, msg = _localizer["UpdateBookingPrice"] };
                }
            }
            catch (Exception ex)
            {
                //final_price = 0;
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
        public ResponseCls SaveClientBooking(BookingCls row)
        {
            long maxId = 0;
            ResponseCls response = null;
            try
            {
                string bookCode = "BK" + "-" + row.trip_code + "-" + DateTime.Now.ToString("yyyyMMdd");
                trips_booking booking = new trips_booking
                {
                    booking_code = bookCode,
                    booking_date = DateTime.Today,
                    booking_notes = row.booking_notes,
                    booking_status = row.booking_status,
                    child_num = row.child_num,
                    client_email = row.client_email,
                    client_id = row.client_id,
                    id = row.id,
                    pickup_time = row.pickup_time,
                    total_pax = row.total_pax,
                    total_price = row.total_price,
                    trip_code = row.trip_code,
                    trip_id = row.trip_id,
                    trip_date = DateTime.Parse(row.trip_dateStr),
                    booking_code_auto=row.booking_code_auto,
                    client_nationality = row.client_nationality,
                    client_phone = row.client_phone,
                    gift_code = row.gift_code,
                    infant_num = row.infant_num,
                    pickup_address = row.pickup_address,
                    currency_code = row.currency_code,
                    trip_type = row.trip_type,
                    client_name = row.client_name,
                    pickup_long=row.pickup_long,
                    pickup_lat=row.pickup_lat
                };
                // booking.total_price = CalculateBookingPrice(booking.trip_id, booking.total_pax, booking.child_num, row.currency_code);
                if (row.id == 0)
                {
                    //check duplicate validation
                    //var result = _db.trips_bookings.Where(wr => wr.trip_id == booking.trip_id && wr.booking_status == booking.booking_status && wr.client_id == booking.client_id && wr.booking_date == booking.booking_date).SingleOrDefault();
                    //if (result != null)
                    //{
                    //    return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    //}
                    if (_db.trips_bookings.Count() > 0)
                    {
                        maxId = _db.trips_bookings.Max(d => d.id);

                    }
                    booking.id = maxId + 1;
                    booking.booking_code_auto = "BK_" + booking.id.ToString();
                    _db.trips_bookings.Add(booking);
                    _db.SaveChanges();
                }
                else
                {
                    _db.trips_bookings.Update(booking);
                    _db.SaveChanges();
                }
                response = new ResponseCls { errors = null, success = true, idOut = booking.id, msg = _localizer["BookingMsg"] };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
      
        public ResponseCls AssignExtraToBooking(List<booking_extra> lst)
        {
            ResponseCls response;
            int maxId = 0;
            int count = 0;
            try
            {
                foreach (var row in lst)
                {
                    row.created_at = DateTime.Now;
                    if (row.id == 0)
                    {
                        //check duplicate validation
                        var result = _db.booking_extras.Where(wr => wr.extra_id == row.extra_id && wr.booking_id == row.booking_id).SingleOrDefault();
                        if (result != null)
                        {
                            return new ResponseCls { success = false, errors = _localizer["AddExtraDuplicate"] };
                        }
                        if (_db.booking_extras.Count() > 0)
                        {
                            maxId = _db.booking_extras.Max(d => d.id);

                        }
                        row.id = maxId + 1;
                        _db.booking_extras.Add(row);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.booking_extras.Update(row);
                        _db.SaveChanges();
                    }
                    count++;
                }
                if (count == lst.Count)
                {
                    response = new ResponseCls { errors = null, success = true };
                }
                else
                {
                    response = new ResponseCls { errors = _localizer["BookingExtraSaveError"], success = false, idOut = 0 };
                }
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["BookingExtraSaveError"], success = false, idOut = 0 };
            }
            return response;
        }

        public List<BookingExtraCast> GetExtraAssignedToBooking(long? booking_id, string lang_code, bool is_obligatory)
        {
            try
            {

                var result = from BOOK in _db.booking_extras.Where(wr => wr.booking_id == booking_id)
                             join TRANS in _db.facility_translations.Where(wr => wr.lang_code.ToLower() == lang_code.ToLower()) on BOOK.extra_id equals TRANS.facility_id
                             join FAC in _db.facility_mains.Where(wr => wr.is_obligatory == is_obligatory) on TRANS.facility_id equals FAC.id
                             select new BookingExtraCast
                             {
                                 extra_id = BOOK.extra_id,
                                 booking_id = BOOK.booking_id,
                                 extra_count = BOOK.extra_count,
                                 extra_name = TRANS.facility_name,
                                 extra_price = FAC.extra_price,
                                 id = BOOK.id,
                                 is_obligatory = FAC.is_obligatory,
                                 isExtra = FAC.is_extra,
                                 pricing_type = FAC.pricing_type,
                                 //total_extra_price = FAC!.pricing_type == 1 ? (FAC!.extra_price  * BOOK.extra_count)  : FAC!.extra_price  
                             };

                return result != null ? result.ToList() : new List<BookingExtraCast>();
            }
            catch (Exception ex)
            {
                return new List<BookingExtraCast>();
            }
        }

        public async Task<List<BookingSummary>> GetMyBooking(string? booking_code ,LangReq req, string client_id)
        {
            try
            {
                //get all booking except cancel which status =3
                var records = await _db.bookingwithdetails.Where(wr => wr.client_id == client_id && wr.lang_code.ToLower() == req.lang_code.ToLower() && wr.booking_status_id != 3 && wr.booking_code == (System.String.IsNullOrEmpty(booking_code) ? wr.booking_code : booking_code)).ToListAsync();

                return records.Select(result => new BookingSummary
                {
                    client_name = result.client_name,
                    trip_type = result.trip_type,
                    trip_id = result.trip_id,
                    booking_code = result.booking_code,
                    booking_code_auto = result.booking_code_auto,
                    booking_date = result.booking_date,
                    booking_datestr = result.booking_datestr,
                    booking_id = result.booking_id,
                    booking_notes = result.booking_notes,
                    booking_status = result.booking_status,
                    booking_status_id = result.booking_status_id,
                    cancelation_policy = result.cancelation_policy,
                    child_num = result.child_num,
                    client_email = result.client_email,
                    client_id = result.client_id,
                    client_nationality = result.client_nationality,
                    client_phone = result.client_phone,
                    currency_code = result.currency_code,
                    default_img = result.default_img,
                    gift_code = result.gift_code,
                    infant_num = result.infant_num,
                    lang_code = result.lang_code,
                    pickup_address = result.pickup_address,
                    pickup_time = result.pickup_time,
                    review_rate = result.review_rate,
                    total_pax = result.total_pax,
                    total_price = result.total_price,
                    trip_code = result.trip_code,
                    trip_date = result.trip_date,
                    trip_datestr = result.trip_datestr,
                    trip_description = result.trip_description,
                    trip_name = result.trip_name,
                    release_days = result.release_days,
                    trip_code_auto = result.trip_code_auto,
                    is_two_way = result.is_two_way,
                    trip_return_date = result.trip_return_date,
                    trip_return_datestr = result.trip_return_datestr,
                    pricing_type = result.pricing_type,
                    child_ages = result.child_ages,
                    extras = GetExtraAssignedToBooking(result.booking_id, req.lang_code, false).ToList(),
                    extras_obligatory = GetExtraAssignedToBooking(result.booking_id, req.lang_code, true).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                return new List<BookingSummary>();
            }
        }
        public async Task<int> GetMyBookingCount(string client_id)
        {
            try
            {
                return await _db.trips_bookings.Where(wr => wr.client_id == client_id && wr.booking_status != 3).CountAsync();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        #region "Profile"
        //get profile for the client
        public async Task<List<ClientProfileCast>> GetClientProfiles(string clientId)
        {
            try
            {
                return await _db.client_Profiles.Where(wr => wr.client_id == clientId).Select(slc => new ClientProfileCast
                {
                    client_birthday = slc.client_birthday,
                    client_birthdayStr = slc.client_birthday != null ? DateTime.Parse(slc.client_birthday.ToString()).ToString("yyyy-MM-dd") : "",
                    client_email = slc.client_email,
                    client_id = slc.client_id,
                    client_name = slc.client_name,
                    gender = slc.gender,
                    lang = slc.lang,
                    nation = slc.nation,
                    pay_code = slc.pay_code,
                    phone_number = slc.phone_number,
                    profile_id = slc.profile_id,
                    address = slc.address,
                    client_first_name = slc.client_first_name,
                    client_last_name = slc.client_last_name,

                }).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ResponseCls SaveMainProfile(ClientProfileCast profile)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {

                if (profile.client_birthdayStr != null)
                {
                    profile.client_birthday = DateOnly.Parse(profile.client_birthdayStr, CultureInfo.InvariantCulture);
                }

                if (profile.profile_id == 0)
                {
                    profile.created_at = DateTime.Now;
                    if (_db.client_Profiles.Count() > 0)
                    {
                        //check validate
                        if (_db.client_Profiles.Where(wr => wr.client_id == profile.client_id).Count() == 0)
                        {
                            maxId = _db.client_Profiles.Max(d => d.profile_id);
                        }
                        else
                        {
                            //do no thing duplicate data
                            return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                        }
                    }
                    profile.profile_id = maxId + 1;
                    _db.client_Profiles.Add(profile);
                }
                else
                {
                    profile.updated_at = DateTime.Now;
                    profile.updated_at = DateTime.Now;
                    _db.Update(profile);
                }
                _db.SaveChanges();
                response = new ResponseCls { success = true, errors = null, idOut = profile.profile_id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { success = false, errors = _localizer["CheckAdmin"], idOut = 0 };
            }
            return response;
        }
        public async Task<ResponseCls> SaveProfileImage(client_image image)
        {
            ResponseCls response;
            decimal maxId = 0;
            try
            {
                //check if client saved profile image before or not (save or update)
                var result = _db.client_images.Where(wr => wr.client_id == image.client_id && wr.type == image.type).SingleOrDefault();
                if (result != null)
                {
                    result.img_path = image.img_path;
                    result.img_name = image.img_name;
                    _db.Update(result);
                }
                else
                {
                    if (_db.client_images.Count() > 0)
                    {
                        maxId = await _db.client_images.DefaultIfEmpty().MaxAsync(d => d.id);
                    }
                    image.id = maxId + 1;
                    _db.client_images.Add(image);
                }


                _db.SaveChanges();
                response = new ResponseCls { success = true, errors = null, idOut = image.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { success = false, errors = _localizer["CheckAdmin"], idOut = 0 };
            }
            return response;
        }

        //get profile image for specific client
        public async Task<List<client_image>> GetProfileImage(string clientId)
        {
            try
            {
                return await _db.client_images.Where(wr => wr.client_id == clientId && wr.type == 1).Select(s => new client_image
                {
                    id = s.id,
                    client_id = s.client_id,
                    img_name = s.img_name,
                    type = s.type,
                    img_path = "http://api.raccoon24.de/" + s.img_path
                }).ToListAsync();

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
