
using ExcursionApp.Data.Data;
using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models;
using ExcursionApp.Data.Models.Bookings.Admin;
using ExcursionApp.Data.Models.destination;
using ExcursionApp.Data.Models.global;
using ExcursionApp.Data.Models.Transfer;
using ExcursionApp.Data.Models.trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExcursionApp.Data
{
    public class AdminDAO
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly excursion_client_dbContext _db;

        public AdminDAO(excursion_client_dbContext db, IStringLocalizer<Messages> localizer)
        {
            _db = db;
            _localizer = localizer;
        }

        #region "main_Setting"

        public async Task<List<tbl_currency>> Get_Currencies()
        {
            return await _db.tbl_currencies.ToListAsync();
        }
        public async Task<List<tbl_language>> Get_Languages()
        {
            return await _db.tbl_languages.ToListAsync();
        }
        public async Task<List<DashBoardStats>> GetDashBoardStats()
        {
            //var bookingsCountTask = _db.trips_bookings.CountAsync();
            //var tripsCountTask = _db.trip_mains.CountAsync();
            //var destinationsCountTask = _db.destination_mains.CountAsync();

            //await Task.WhenAll(
            //    bookingsCountTask,
            //    tripsCountTask,
            //    destinationsCountTask
            //);

            return new List<DashBoardStats>
    {
        new DashBoardStats
        {
            name = "Bookings",
            value = await _db.trips_bookings.CountAsync()
        },
        new DashBoardStats
        {
            name = "Trips",
            value = await _db.trip_mains.CountAsync()
        },
        new DashBoardStats
        {
            name = "Destinations",
            value = await _db.destination_mains.CountAsync()
        }
    };
        }

        #endregion
        #region destination

        //save main destination data by admin
        public ResponseCls SaveMainDestination(destination_main row)
        {
            ResponseCls response;
            int maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                if (row.id == 0)
                {
                    //check duplicate order (in add new row)
                    //if (_db.destination_mains.Where(wr => wr.order == row.order && wr.active == row.active).SingleOrDefault() != null)
                    //{
                    //    return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                    //}
                    //check duplicate validation
                    var result = _db.destination_mains.Where(wr => wr.dest_code == row.dest_code && wr.active == row.active).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.destination_mains.Count() > 0)
                    {
                        maxId = _db.destination_mains.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    row.order = _db.destination_mains.Max(d => d.order) + 1;
                    _db.destination_mains.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    //check duplicate order (in update row)
                    //if (_db.destination_mains.Where(wr => wr.order == row.order && wr.active == row.active && wr.id != row.id && wr.parent_id == row.parent_id).SingleOrDefault() != null)
                    //{
                    //    return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                    //}
                    row.updated_at = DateTime.Now;
                    _db.destination_mains.Update(row);
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
        //save destination's translations
        public ResponseCls SaveDestinationTranslations(destination_translation row)
        {
            ResponseCls response;
            int maxId = 0;
            try
            {
                if (row.active == false)
                {
                    _db.Remove(row);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }

                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.destination_translations.Where(wr => wr.destination_id == row.destination_id && wr.active == row.active && wr.lang_code == row.lang_code).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.destination_translations.Count() > 0)
                    {
                        maxId = _db.destination_translations.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.destination_translations.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.destination_translations.Update(row);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true };
            }

            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }

            return response;
        }
        //save destination images
        public ResponseCls saveDestinationImage(List<destination_img> lst)
        {
            ResponseCls response;
            int maxId = 0;
            int count = 0;
            try
            {
                foreach (var row in lst)
                {
                    if (row.id == 0)
                    {
                        //check duplicate validation
                        var result = _db.destination_imgs.Where(wr => wr.destination_id == row.destination_id && wr.is_default == (row.is_default == true ? row.is_default : null)).SingleOrDefault();
                        if (result != null)
                        {
                            return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                        }
                        if (_db.destination_imgs.Count() > 0)
                        {
                            maxId = _db.destination_imgs.Max(d => d.id);

                        }
                        row.id = maxId + 1;
                        _db.destination_imgs.Add(row);
                        _db.SaveChanges();
                    }
                    else
                    {
                        row.updated_at = DateTime.Now;
                        _db.destination_imgs.Update(row);
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
                    response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
                }

            }

            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //public DestinationWithTranslationsPag GetDestinationWithTranslations(DestinationWithTranslationsReq req)
        //{
        //    try
        //    {
        //        // -------- Base query (joins only once) ----------
        //        var query =
        //            from dest in _db.destination_mains
        //            join trans in _db.destination_translations.Where(t => (bool)t.active)
        //                on dest.id equals trans.destination_id into trans_join
        //            from t in trans_join.DefaultIfEmpty()
        //            join parent in _db.destination_mains.Where(p => (bool)p.active && p.parent_id == 0)
        //                on dest.parent_id equals parent.id into parent_join
        //            from p in parent_join.DefaultIfEmpty()
        //            where dest.active == true &&
        //                  (string.IsNullOrEmpty(req.country_code) ||
        //                   dest.country_code.ToLower() == req.country_code.ToLower())
        //            select new DestinationResponse
        //            {
        //                destination_id = dest.id,
        //                id = t != null ? t.id : 0,
        //                country_code = dest.country_code,
        //                active = dest.active,
        //                dest_code = dest.dest_code,
        //                dest_description = t != null ? t.dest_description : null,
        //                dest_name = t != null ? t.dest_name : null,
        //                //img_path = IMGDEST != null ? "http://api.raccoon24.de/" + IMGDEST.img_path : null,
        //                lang_code = t != null ? t.lang_code : null,
        //                dest_default_name = dest.dest_default_name,
        //                route = dest.route,
        //                leaf = dest.leaf,
        //                parent_id = dest.parent_id,
        //                parent_name = p != null ? p.dest_default_name : null,
        //                order = dest.order,
        //                // parent_order = PARDEST != null ? PARDEST.order : 0,
        //            };

        //        // -------- Grouping (SQL-level grouping) ----------
        //        var groupedQuery =
        //            from grp in query
        //            group grp by new
        //            {
        //                grp.dest_code,
        //                grp.img_path,
        //                grp.destination_id,
        //                grp.country_code,
        //                grp.dest_default_name,
        //                grp.route,
        //                grp.active,
        //                grp.parent_name,
        //                grp.parent_id,
        //                grp.leaf,
        //                grp.order
        //            }
        //            into g
        //            select new DestinationWithTranslations
        //            {
        //                destination_id = g.Key.destination_id,
        //                dest_code = g.Key.dest_code,
        //                country_code = g.Key.country_code,
        //                dest_default_name = g.Key.dest_default_name,
        //                route = g.Key.route,
        //                active = g.Key.active,
        //                parent_id = g.Key.parent_id,
        //                parent_name = g.Key.parent_name,
        //                leaf = g.Key.leaf,
        //                order = g.Key.order,
        //                translations = g.Where(wr => wr.dest_code == g.Key.dest_code).ToList()
        //                //translations = g
        //                //    .Where(x => x.Translation != null)
        //                //    .Select(x => new DestinationResponse
        //                //    {
        //                //        destination_id = x.destination_id,
        //                //        id = x.Translation.id,
        //                //        dest_code = x.dest_code,
        //                //        country_code = x.country_code,
        //                //        dest_name = x.Translation.dest_name,
        //                //        dest_description = x.Translation.dest_description,
        //                //        lang_code = x.Translation.lang_code
        //                //    }).ToList()
        //            };

        //        // -------- Get total count BEFORE pagination --------
        //        int totalRecords = groupedQuery.Count();
        //        //int totalPages = (int)Math.Ceiling(totalRecords / (double)req.pageSize);

        //        // -------- Apply pagination --------
        //        var result = groupedQuery
        //            .OrderBy(o => o.order)
        //            .Skip((req.pageNumber - 1) * req.pageSize)
        //            .Take(req.pageSize)
        //            .ToList();

        //        return new DestinationWithTranslationsPag
        //        {
        //            result = result,
        //            totalPages = totalRecords
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new DestinationWithTranslationsPag();
        //    }
        //}

        public DestinationWithTranslationsPag GetDestinationWithTranslations(DestinationWithTranslationsReq req)
        {
            try
            {
                var records =
                    from dest in _db.destination_mains.Where(wr => wr.active == true && wr.country_code.ToLower() == (String.IsNullOrEmpty(req.country_code) ? wr.country_code.ToLower() : req.country_code.ToLower()))
                    join trans in _db.destination_translations.Where(wr => wr.active == true)
                        on dest.id equals trans.destination_id
                             into dest_trans

                    from combinedDEST in dest_trans.DefaultIfEmpty() // LEFT JOIN 
                    join PAR in _db.destination_mains.Where(wr => wr.parent_id == 0 && wr.active == true)

                       on dest.parent_id equals PAR.id into DestAll
                    from PARDEST in DestAll.DefaultIfEmpty() // LEFT JOIN Payments
                    select new DestinationResponse
                    {
                        destination_id = dest.id,
                        id = combinedDEST != null ? combinedDEST.id : 0,
                        country_code = dest.country_code,
                        active = dest.active,
                        dest_code = dest.dest_code,
                        dest_description = combinedDEST != null ? combinedDEST.dest_description : null,
                        dest_name = combinedDEST != null ? combinedDEST.dest_name : null,
                        //img_path = IMGDEST != null ? "http://api.raccoon24.de/" + IMGDEST.img_path : null,
                        lang_code = combinedDEST != null ? combinedDEST.lang_code : null,
                        dest_default_name = dest.dest_default_name,
                        route = dest.route,
                        leaf = dest.leaf,
                        parent_id = dest.parent_id,
                        parent_name = PARDEST != null ? PARDEST.dest_default_name : null,
                        order = dest.order,
                        // parent_order = PARDEST != null ? PARDEST.order : 0,
                    };
                var flatList = records.AsEnumerable();
                var grouped = flatList.GroupBy(grp => new
                {
                    grp.dest_code,
                    grp.img_path,
                    grp.destination_id,
                    grp.country_code,
                    grp.dest_default_name,
                    grp.route,
                    grp.active,
                    grp.parent_name,
                    grp.parent_id,
                    grp.leaf,
                    grp.order

                }).Select(s => new DestinationWithTranslations
                {
                    country_code = s.Key.country_code,
                    dest_code = s.Key.dest_code,
                    img_path = s.Key.img_path,
                    destination_id = s.Key.destination_id,
                    dest_default_name = s.Key.dest_default_name,
                    route = s.Key.route,
                    active = s.Key.active,
                    parent_id = s.Key.parent_id,
                    parent_name = s.Key.parent_name,
                    leaf = s.Key.leaf,
                    order = s.Key.order,
                    translations = records.Where(wr => wr.dest_code == s.Key.dest_code).ToList()

                }).ToList();
                // --- Step 4: pagination ---
                int totalRecords = grouped.Count;
                //int totalPages = (int)Math.Ceiling(totalRecords / (double)req.pageSize);

                var paginated = grouped
                    .Skip((req.pageNumber - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .ToList();

                return new DestinationWithTranslationsPag
                {
                    result = paginated,
                    totalPages = totalRecords
                };
                //return new DestinationWithTranslationsPag { result = result, totalPages = result.Count };
            }
            catch (Exception ex)
            {
                return new DestinationWithTranslationsPag();
            }
        }
        //get images list for specific trip
        public async Task<List<destination_img>> GetImgsByDestination(int? destination_id)
        {
            try
            {
                return await _db.destination_imgs.Where(wr => wr.destination_id == destination_id)
                    .OrderBy(o => o.id).Select(s => new destination_img
                    {
                        id = s.id,
                        img_height = s.img_height,
                        img_name = s.img_name,
                        img_path = "http://api.raccoon24.de/" + s.img_path,
                        img_resize_path = "http://api.raccoon24.de/" + s.img_resize_path,
                        img_width = s.img_width,
                        is_default = s.is_default,
                        destination_id = s.destination_id,
                    }).ToListAsync();
                //return await _db.destination_imgs.Where(wr => wr.destination_id == destination_id)
                //    .OrderBy(o => o.id)
                //    .ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<destination_main>> GetDestination_Mains(bool? leaf, bool? isAll)
        {
            if(isAll == true)
            return await _db.destination_mains.Where(wr => wr.active == true).ToListAsync();
            return await _db.destination_mains.Where(wr => wr.active == true && wr.leaf == leaf).ToListAsync();
        }

        //use to set destination image is default , and first should check if there is another image with same destination id is default or not
        public ResponseCls UpdateDestinationImage(DestinationImgUpdateReq cls)
        {
            ResponseCls response;
            try
            {
                destination_img dest = new destination_img
                {
                    id = cls.id,
                    destination_id = cls.destination_id,
                    is_default = cls.is_default,
                    img_name = cls.img_name,
                    img_path = cls.img_path,
                    created_by = cls.created_by
                };
                if (cls.delete == true)
                {
                    _db.Remove(dest);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                if (dest.is_default == true)
                {
                    var result = _db.destination_imgs.Where(wr => wr.destination_id == dest.destination_id && wr.is_default == true).SingleOrDefault();
                    if (result != null)
                    {
                        //if already an image is default , so make it false and set is default to new one
                        result.updated_at = DateTime.Now;
                        result.is_default = false;
                        _db.destination_imgs.Update(result);
                        _db.SaveChanges();
                    }

                }
                var row = _db.destination_imgs.Where(wr => wr.id == dest.id).SingleOrDefault();
                if (row != null)
                {
                    row.updated_at = DateTime.Now;
                    row.is_default = dest.is_default;
                    _db.destination_imgs.Update(row);
                    _db.SaveChanges();
                }
                response = new ResponseCls { errors = null, success = true };



            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        public async Task<List<destination_translation>> GetDestinationTranslation(int? destination_id)
        {
            return await _db.destination_translations.Where(wr => wr.active == true && wr.destination_id == destination_id).ToListAsync();
        }
        #endregion


        #region trips


        public async Task<List<trip_category>> GetTripCategories()
        {
            return await _db.trip_categories.ToListAsync();
        }

        //save main trip data by admin
        public ResponseCls SaveMainTrip(trip_main row)
        {
            ResponseCls response;
            long maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                if (row.id == 0)
                {
                    //check duplicate order (in add new row)
                    //if (_db.trip_mains.Where(wr => wr.trip_order == row.trip_order && wr.active == row.active).SingleOrDefault() != null)
                    //{
                    //    return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                    //}
                    //check duplicate validation
                    var result = _db.trip_mains.Where(wr => wr.trip_code == row.trip_code && wr.active == row.active && wr.destination_id == row.destination_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_mains.Count() > 0)
                    {
                        maxId = _db.trip_mains.Max(d => d.id);

                    }

                    row.id = maxId + 1;
                    row.trip_order = _db.trip_mains.Max(d => d.trip_order) + 1;
                    row.trip_code_auto = "TRIP_" + row.id.ToString();
                    _db.trip_mains.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    //check duplicate order (in update row)
                    //if (_db.trip_mains.Where(wr => wr.trip_order == row.trip_order && wr.active == row.active && wr.id != row.id).SingleOrDefault() != null)
                    //{
                    //    return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                    //}
                    row.updated_at = DateTime.Now;
                    _db.trip_mains.Update(row);
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

        //save trip translation data by admin
        public ResponseCls SaveTripTranslation(TripTranslationReq row)
        {
            ResponseCls response;
            long maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                trip_translation trip = new trip_translation
                {
                    id = row.id,
                    lang_code = row.lang_code,
                    trip_description = row.trip_description,
                    trip_highlight = row.trip_highlight,
                    trip_id = row.trip_id,
                    trip_includes = row.trip_includes,
                    trip_name = row.trip_name,
                    important_info = row.important_info,
                    trip_details = row.trip_details,
                    trip_not_includes = row.trip_not_includes,
                    created_by = row.created_by,
                    cancelation_policy = row.cancelation_policy,
                };
                if (row.delete == true)
                {
                    _db.Remove(trip);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }

                if (trip.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trip_translations.Where(wr => wr.lang_code == trip.lang_code && wr.trip_id == trip.trip_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_translations.Count() > 0)
                    {
                        maxId = _db.trip_translations.Max(d => d.id);

                    }
                    trip.id = maxId + 1;
                    _db.trip_translations.Add(trip);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.trip_translations.Update(trip);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = trip.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //assign prices with different currency to trips
        public ResponseCls SaveTripPrices(TripPricesReq row)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {
                trip_price price = new trip_price
                {
                    id = row.id,
                    trip_id = row.trip_id,
                    currency_code = row.currency_code,
                    trip_origin_price = row.trip_origin_price,
                    trip_sale_price = row.trip_sale_price,
                    created_by = row.created_by,
                    notes = row.notes,
                    child_price = row.child_price,
                    pax_from = row.pax_from,
                    pax_to = row.pax_to,
                    pricing_type = row.pricing_type
                };
                if (row.delete == true)
                {
                    _db.Remove(price);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                if (price.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trip_prices.Where(wr => wr.trip_id == price.trip_id && wr.currency_code == price.currency_code && wr.pax_from == price.pax_from && wr.pax_to == price.pax_to).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_prices.Count() > 0)
                    {
                        maxId = _db.trip_prices.Max(d => d.id);

                    }
                    price.id = maxId + 1;
                    _db.trip_prices.Add(price);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.trip_prices.Update(price);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = price.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //use to set trip image is default , and first should check if there is another image with same trip id is default or not
        public ResponseCls UpdateTripImage(TripImgUpdateReq cls)
        {
            ResponseCls response;
            try
            {
                trip_img trip = new trip_img
                {
                    id = cls.id,
                    trip_id = cls.trip_id,
                    is_default = cls.is_default,
                    img_name = cls.img_name,
                    img_path = cls.img_path,
                    created_by = cls.created_by,
                    trip_type = cls.trip_type,
                    img_order = cls.img_order
                };
                if (cls.delete == true)
                {
                    _db.Remove(trip);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                //check duplicate order (in add new row)
                if (_db.trip_imgs.Where(wr => wr.img_order == trip.img_order && wr.id != trip.id).SingleOrDefault() != null)
                {
                    return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                }
                if (trip.is_default == true)
                {
                    var result = _db.trip_imgs.Where(wr => wr.trip_id == trip.trip_id && wr.is_default == true).SingleOrDefault();
                    if (result != null)
                    {
                        //if already an image is default , so make it false and set is default to new one
                        result.updated_at = DateTime.Now;
                        result.is_default = false;
                        _db.trip_imgs.Update(result);
                        _db.SaveChanges();
                    }

                }
                var row = _db.trip_imgs.Where(wr => wr.id == trip.id).SingleOrDefault();
                if (row != null)
                {
                    row.updated_at = DateTime.Now;
                    row.is_default = trip.is_default;
                    _db.trip_imgs.Update(row);
                    _db.SaveChanges();
                }
                response = new ResponseCls { errors = null, success = true };



            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
        public ResponseCls saveTripImage(List<trip_img> lst)
        {
            ResponseCls response;
            long maxId = 0;
            int count = 0;
            try
            {
                foreach (var trip in lst)
                {
                    if (trip.id == 0)
                    {
                        //check duplicate order (in add new row)
                        if (_db.trip_imgs.Where(wr => wr.img_order == trip.img_order).SingleOrDefault() != null)
                        {
                            return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                        }
                        //check duplicate validation
                        var result = _db.trip_imgs.Where(wr => wr.trip_id == trip.trip_id && wr.is_default == (trip.is_default == true ? trip.is_default : null)).SingleOrDefault();
                        if (result != null)
                        {
                            return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                        }
                        if (_db.trip_imgs.Count() > 0)
                        {
                            maxId = _db.trip_imgs.Max(d => d.id);

                        }
                        trip.id = maxId + 1;
                        _db.trip_imgs.Add(trip);
                        _db.SaveChanges();
                    }
                    else
                    {
                        //check duplicate order (in update row)
                        if (_db.trip_imgs.Where(wr => wr.img_order == trip.img_order && wr.id != trip.id).SingleOrDefault() != null)
                        {
                            return new ResponseCls { success = false, errors = _localizer["DuplicateOrder"] };
                        }
                        trip.updated_at = DateTime.Now;
                        _db.trip_imgs.Update(trip);
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
                    response = new ResponseCls { errors = _localizer["CheckAdmin"], success = true };
                }



            }

            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
        //public ResponseCls saveTripImage(TripImgReq row)
        //{
        //    ResponseCls response;
        //    long maxId = 0;
        //    try
        //    {
        //        trip_img trip = new trip_img
        //        {
        //            trip_id = row.trip_id,
        //            id = row.id,
        //            img_name = row.img_name,
        //            img_path = row.img_path,
        //            is_default = row.is_default,
        //            created_by = row.created_by,
        //            img_height = row.img_height,
        //            img_resize_path = row.img_resize_path,
        //            img_width = row.img_width,


        //        };

        //        if (row.id == 0)
        //        {
        //            //check duplicate validation
        //            var result = _db.trip_imgs.Where(wr => wr.trip_id == trip.trip_id && wr.is_default == (trip.is_default == true ? trip.is_default : null)).SingleOrDefault();
        //            if (result != null)
        //            {
        //                return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
        //            }
        //            if (_db.trip_imgs.Count() > 0)
        //            {
        //                maxId = _db.trip_imgs.Max(d => d.id);

        //            }
        //            trip.id = maxId + 1;
        //            _db.trip_imgs.Add(trip);
        //            _db.SaveChanges();
        //        }
        //        else
        //        {
        //            row.updated_at = DateTime.Now;
        //            _db.trip_imgs.Update(trip);
        //            _db.SaveChanges();
        //        }

        //        response = new ResponseCls { errors = null, success = true, idOut = trip.id };


        //    }

        //    catch (Exception ex)
        //    {
        //        response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
        //    }
        //    return response;
        //}
        //save main facility data by admin
        public ResponseCls SaveMainFacility(facility_main row)
        {
            ResponseCls response;
            long maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.facility_mains.Where(wr => wr.facility_code == row.facility_code).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.facility_mains.Count() > 0)
                    {
                        maxId = _db.facility_mains.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.facility_mains.Add(row);
                    _db.SaveChanges();
                }
                else
                {
                    //check if is_etxra updated from true to false , should remove pricing info
                    row.updated_at = DateTime.Now;
                    _db.facility_mains.Update(row);
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

        //save facility translation data by admin
        public ResponseCls SaveFacilityTranslation(FacilityTranslationReq row)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {
                facility_translation facility = new facility_translation
                {
                    facility_desc = row.facility_desc,
                    facility_id = row.facility_id,
                    facility_name = row.facility_name,
                    id = row.id,
                    lang_code = row.lang_code,
                    created_by = row.created_by
                };
                if (row.delete == true)
                {
                    _db.Remove(facility);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }

                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.facility_translations.Where(wr => wr.lang_code == facility.lang_code && wr.facility_id == facility.facility_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.facility_translations.Count() > 0)
                    {
                        maxId = _db.facility_translations.Max(d => d.id);

                    }
                    facility.id = maxId + 1;
                    _db.facility_translations.Add(facility);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.facility_translations.Update(facility);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = facility.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }

        //assign facility to trip  by admin
        public ResponseCls AssignFacilityToTrip(TripFacilityAssignReq row)
        {
            ResponseCls response;
            long maxId = 0;
            try
            {
                trip_facility trip = new trip_facility
                {
                    id = row.id,
                    facility_id = row.facility_id,
                    active = row.active,
                    trip_id = row.trip_id,
                    created_by = row.created_by,
                    trip_type = row.trip_type
                };
                if (row.selected == false && row.id > 0)
                {
                    _db.Remove(trip);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }

                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trip_facilities.Where(wr => wr.trip_id == trip.trip_id && wr.facility_id == trip.facility_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_facilities.Count() > 0)
                    {
                        maxId = _db.trip_facilities.Max(d => d.id);

                    }
                    trip.id = maxId + 1;
                    _db.trip_facilities.Add(trip);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.trip_facilities.Update(trip);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = trip.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }


        //save  trip pickups main data by admin
        public ResponseCls SaveMainTripPickups(TripsPickupSaveReq row)
        {
            ResponseCls response;
            long maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                trip_pickups_main pickup = new trip_pickups_main
                {
                    id = row.id,
                    order = row.order,
                    pickup_code = row.pickup_code,
                    pickup_default_name = row.pickup_default_name,
                    trip_id = row.trip_id,
                    trip_type = row.trip_type,
                    created_by = row.created_by,
                    duration = row.duration
                };
                if (row.delete == true)
                {
                    _db.Remove(pickup);
                    _db.SaveChanges();
                    //check if there are translations for this row and delete all also
                    var transList = _db.trip_pickups_translations.Where(wr => wr.trip_pickup_id == pickup.id).ToList();
                    if (transList.Count > 0)
                    {
                        _db.RemoveRange(transList);
                        _db.SaveChanges();
                    }
                    return new ResponseCls { errors = null, success = true };
                }
                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trip_pickups_mains.Where(wr => wr.pickup_code == pickup.pickup_code && wr.trip_id == pickup.trip_id && wr.order == pickup.order).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_pickups_mains.Count() > 0)
                    {
                        maxId = _db.trip_pickups_mains.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.trip_pickups_mains.Add(pickup);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.trip_pickups_mains.Update(pickup);
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


        public  async Task<List<facility_translation>> GetFacilityTranslation(long? faclility_id)
        {
            try
            {
                return await _db.facility_translations.Where(wr => wr.facility_id == faclility_id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<FacilityWithTranslationGrp> GetFacilityWithTranslation()
        {
            try
            {
                var result =
                   from FM in _db.facility_mains
                   join FT in _db.facility_translations
                       on FM.id equals FT.facility_id
                            into FM_FT

                   from combined in FM_FT.DefaultIfEmpty()
                   select new FacilityWithTranslation
                   {
                       facility_id = FM.id,
                       id = combined != null ? combined.id : 0,
                       facility_code = FM.facility_code,
                       facility_default_name = FM.facility_default_name,
                       facility_desc = combined != null ? combined.facility_desc : null,
                       facility_name = combined != null ? combined.facility_name : null,
                       lang_code = combined != null ? combined.lang_code : null,
                       active = FM.active,
                       currency_code = FM.currency_code,
                       extra_price = FM.extra_price,
                       is_obligatory = FM.is_obligatory,
                       pricing_type = FM.pricing_type,
                       is_extra = FM.is_extra

                   };
                return result.ToList().GroupBy(grp => new
                {
                    grp.facility_id,
                    grp.facility_default_name,
                    grp.facility_code,
                    grp.active,
                    grp.is_extra,
                    grp.extra_price,
                    grp.currency_code,
                    grp.pricing_type,
                    grp.is_obligatory
                }).Select(s => new FacilityWithTranslationGrp
                {
                    facility_code = s.Key.facility_code,
                    facility_default_name = s.Key.facility_default_name,
                    facility_id = s.Key.facility_id,
                    active = s.Key.active,
                    currency_code = s.Key.currency_code,
                    extra_price = s.Key.extra_price,
                    is_extra = s.Key.is_extra,
                    is_obligatory = s.Key.is_obligatory,
                    pricing_type = s.Key.pricing_type,
                    pricing_type_name = s.Key.pricing_type == 1 ? "Per Pax" : (s.Key.pricing_type == 1 ? "Per Unit" : "") ,
                    translations = result.ToList().Where(wr => wr.facility_id == s.Key.facility_id && wr.id != 0).ToList()

                }).OrderBy(x => x.facility_id).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public List<FacilityAllWithSelect> GetFacilityAllWithSelect(long? trip_id)
        {
            try
            {
                var result =
                   from FM in _db.facility_mains.Where(wr => wr.active == true)
                   join FT in _db.trip_facilities.Where(wr => wr.trip_id == trip_id)
                       on FM.id equals FT.facility_id
                            into FM_FT

                   from combined in FM_FT.DefaultIfEmpty()
                   select new FacilityAllWithSelect
                   {
                       active = FM.active,
                       facility_code = FM.facility_code,
                       facility_default_name = FM.facility_default_name,
                       facility_id = FM.id,
                       fac_trip_id = combined != null ? combined.id : 0,
                       pricing_type = FM.pricing_type,
                       extra_price = FM.extra_price,
                       is_obligatory = FM.is_obligatory,
                       pricing_type_name = FM.pricing_type == 1 ? "Per Pax" : (FM.pricing_type == 1 ? "Per Unit" : ""),
                       selected = combined != null && combined.id > 0 ? true : false,
                       currency_code = FM.currency_code
                   };
                return result.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //save  trip pickups main data by admin
        public ResponseCls SaveTripPickupsTranslations(TripsPickupTranslationSaveReq row)
        {
            ResponseCls response;
            long maxId = 0;
            var msg = _localizer["DuplicateData"];
            try
            {
                trip_pickups_translation pickup = new trip_pickups_translation
                {
                    id = row.id,
                    lang_code = row.lang_code,
                    pickup_description = row.pickup_description,
                    pickup_name = row.pickup_name,
                    trip_pickup_id = row.trip_pickup_id,
                    created_by = row.created_by,

                };
                if (row.delete == true)
                {
                    _db.Remove(pickup);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.trip_pickups_translations.Where(wr => wr.lang_code == pickup.lang_code && wr.trip_pickup_id == pickup.trip_pickup_id).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.trip_pickups_translations.Count() > 0)
                    {
                        maxId = _db.trip_pickups_translations.Max(d => d.id);

                    }
                    row.id = maxId + 1;
                    _db.trip_pickups_translations.Add(pickup);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.trip_pickups_translations.Update(pickup);
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

        public TripMainCastPag GetTrip_MainsWithPag(TripMainReq req)
        {
            try
            {
                var records =
                  _db.trip_mains.Where(wr => wr.destination_id == (req.destination_id == 0 ? wr.destination_id : req.destination_id) && wr.trip_type == (req.trip_type == 0 ? wr.trip_type : req.trip_type))
                      .Join(_db.destination_mains,
                              TRIP => new { TRIP.destination_id },
                              DEST => new { destination_id = (int?)DEST.id },
                              (TRIP, DEST) => new TripMainCast
                              {
                                  destination_id = TRIP.destination_id,
                                  active = TRIP.active,
                                  id = TRIP.id,
                                  pickup = TRIP.pickup,
                                  route = TRIP.route,
                                  show_in_slider = TRIP.show_in_slider,
                                  show_in_top = TRIP.show_in_top,
                                  trip_code = TRIP.trip_code,
                                  trip_default_name = TRIP.trip_default_name,
                                  trip_duration = TRIP.trip_duration,
                                  country_code = DEST.country_code,
                                  dest_code = DEST.dest_code,
                                  dest_default_name = DEST.dest_default_name,
                                  trip_type = TRIP.trip_type,
                                  transfer_category_id = TRIP.transfer_category_id,
                                  trip_code_auto = TRIP.trip_code_auto,
                                  release_days = TRIP.release_days,
                                  trip_order = TRIP.trip_order,
                                  is_comm_soon = TRIP.is_comm_soon
                              }).OrderBy(d => d.id).AsEnumerable();
                int totalRecords = records.Count();
                //int totalPages = (int)Math.Ceiling(totalRecords / (double)req.pageSize);

                var paginated = records
                    .Skip((req.pageNumber - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .ToList();
                return new TripMainCastPag { trips = paginated, totalPages = totalRecords };
            }   // return await _db.trip_mains.Where(wr => wr.destination_id == (destination_id == 0 ? wr.destination_id : destination_id)).ToListAsync();
            catch (Exception ex)
            {
                return new TripMainCastPag();
            }
        }
        public async Task<List<TripMainCast>> GetTrip_Mains(int destination_id, int trip_type)
        {
            try
            {
                return await _db.trip_mains.Where(wr => wr.destination_id == (destination_id == 0 ? wr.destination_id : destination_id) && wr.trip_type == (trip_type == 0 ? wr.trip_type : trip_type))
                      .Join(_db.destination_mains,
                              TRIP => new { TRIP.destination_id },
                              DEST => new { destination_id = (int?)DEST.id },
                              (TRIP, DEST) => new TripMainCast
                              {
                                  destination_id = TRIP.destination_id,
                                  active = TRIP.active,
                                  id = TRIP.id,
                                  pickup = TRIP.pickup,
                                  route = TRIP.route,
                                  show_in_slider = TRIP.show_in_slider,
                                  show_in_top = TRIP.show_in_top,
                                  trip_code = TRIP.trip_code,
                                  trip_default_name = TRIP.trip_default_name,
                                  trip_duration = TRIP.trip_duration,
                                  country_code = DEST.country_code,
                                  dest_code = DEST.dest_code,
                                  dest_default_name = DEST.dest_default_name,
                                  trip_type = TRIP.trip_type,
                                  transfer_category_id = TRIP.transfer_category_id,
                                  trip_code_auto = TRIP.trip_code_auto,
                                  release_days = TRIP.release_days,
                                  trip_order = TRIP.trip_order,
                                  is_comm_soon = TRIP.is_comm_soon
                              }).OrderBy(d => d.id).ToListAsync();


            }   // return await _db.trip_mains.Where(wr => wr.destination_id == (destination_id == 0 ? wr.destination_id : destination_id)).ToListAsync();
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<TripTranslationGrp>> GetTripTranslationGrp(long? trip_id)
        {
            try
            {
                var result = await _db.trip_translations.Where(wr => wr.trip_id == trip_id).ToListAsync();
                List<tbl_language> langs = await _db.tbl_languages.ToListAsync();

                List<TripTranslationGrp> translations = new List<TripTranslationGrp>();
                foreach (var item in langs)
                {
                    TripTranslationGrp row = new TripTranslationGrp { lang_code = item.lang_code.ToLower(), translation = result.ToList().Where(wr => wr.lang_code.ToLower() == item.lang_code.ToLower()).SingleOrDefault() };
                    translations.Add(row);

                }
                ;
                //List<TripTranslationGrp> translations = new List<TripTranslationGrp>
                //{
                //    new TripTranslationGrp { lang_code = "en", translation = result.ToList().Where(wr => wr.lang_code == "en").SingleOrDefault() },
                //    new TripTranslationGrp { lang_code = "de", translation = result.ToList().Where(wr => wr.lang_code == "de").SingleOrDefault() },

                //};

                return translations;
                //if (result.Count > 0)
                //{
                //    return result.GroupBy(g => new
                //    {
                //        g.lang_code
                //    }).Select(s => new TripTranslationGrp
                //    {
                //        lang_code = s.Key.lang_code,
                //        translation = result.ToList().Where(wr => wr.lang_code == s.Key.lang_code).SingleOrDefault()
                //    }).ToList();
                //}
                //else
                //{
                //    List<TripTranslationGrp> translations = new List<TripTranslationGrp>
                //        {
                //            new TripTranslationGrp { lang_code="en",translation=null },
                //            new TripTranslationGrp { lang_code="de",translation=null },

                //        };
                //    return translations;
                //}

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<trip_price>> GetTrip_Prices(long? trip_id)
        {
            try
            {
                return await _db.trip_prices.Where(wr => wr.trip_id == trip_id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<List<trip_pickups_main>> GetTrip_Pickups_Mains(long? trip_id)
        {
            try
            {
                return await _db.trip_pickups_mains.Where(wr => wr.trip_id == trip_id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //get pickup translations for specific pickup main

        public async Task<List<trip_pickups_translation>> GetTrip_Pickups_Translations(long? trip_pickup_id)
        {
            try
            {
                return await _db.trip_pickups_translations.Where(wr => wr.trip_pickup_id == trip_pickup_id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<TripsPickupResponseGrp> GetPickupsAllForTrip(PickupsReq req)
        {
            try
            {

                var result =
                    from PM in _db.trip_pickups_mains.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type)
                    join trans in _db.trip_pickups_translations
                        on PM.id equals trans.trip_pickup_id
                             into pm_trans

                    from combined in pm_trans.DefaultIfEmpty()
                    select new TripsPickupResponse
                    {
                        trip_pickup_id = PM.id,
                        lang_code = combined != null ? combined.lang_code : null,
                        order = PM.order,
                        pickup_code = PM.pickup_code,
                        pickup_default_name = PM.pickup_default_name,
                        pickup_description = combined != null ? combined.pickup_description : null,
                        pickup_name = combined != null ? combined.pickup_name : null,
                        trip_id = PM.trip_id,
                        trip_type = PM.trip_type,
                        duration = PM.duration,
                        id = combined != null ? combined.id : 0
                    };
                //var result = await _db.trip_pickups_mains.Where(wr => wr.trip_id == req.trip_id && wr.trip_type == req.trip_type)
                //                   .Join(_db.trip_pickups_translations,
                //                        MAIN => new { trip_pickup_id = MAIN.id },
                //                        TRANS => new { TRANS.trip_pickup_id },
                //                        (MAIN, TRANS) => new TripsPickupResponse
                //                        {
                //                            trip_pickup_id = MAIN.id,
                //                            lang_code = TRANS.lang_code,
                //                            order = MAIN.order,
                //                            pickup_code = MAIN.pickup_code,
                //                            pickup_default_name = MAIN.pickup_default_name,
                //                            pickup_description = TRANS.pickup_description,
                //                            pickup_name = TRANS.pickup_name,
                //                            trip_id = MAIN.trip_id,
                //                            trip_type = MAIN.trip_type,
                //                            duration = MAIN.duration
                //                        }
                //                       ).OrderBy(x => x.order).ToListAsync();

                return result.OrderBy(x => x.order).ToList().GroupBy(grp => new
                {
                    grp.pickup_code,
                    grp.pickup_default_name,
                    grp.duration,
                    grp.order,
                    grp.trip_id,
                    grp.trip_pickup_id,
                    grp.trip_type
                }).Select(s => new TripsPickupResponseGrp
                {
                    trip_pickup_id = s.Key.trip_pickup_id,
                    trip_id = s.Key.trip_id,
                    order = s.Key.order,
                    pickup_default_name = s.Key.pickup_default_name,
                    pickup_code = s.Key.pickup_code,
                    trip_type = s.Key.trip_type,
                    duration = s.Key.duration,
                    translations = result.ToList().Where(wr => wr.trip_pickup_id == s.Key.trip_pickup_id && wr.lang_code != null).ToList()

                }).ToList();

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
                return await _db.trip_imgs.Where(wr => wr.trip_id == trip_id).OrderBy(o => o.id).Select(s => new trip_img
                {
                    id = s.id,
                    img_height = s.img_height,
                    img_name = s.img_name,
                    img_path = "http://api.raccoon24.de/" + s.img_path,
                    img_resize_path = "http://api.raccoon24.de/" + s.img_resize_path,
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
        //get child policy plan for trip
        public async Task<List<child_policy_setting>> GetTrip_ChildPolicy(long? trip_id)
        {
            try
            {
                return await _db.child_policy_settings.Where(wr => wr.trip_id == trip_id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //assign prices with different currency to trips
        public ResponseCls SaveTripChildPolicy(ChildPolicyPricesReq row)
        {
            ResponseCls response;
            int maxId = 0;
            try
            {
                child_policy_setting price = new child_policy_setting
                {

                    created_by = row.created_by,
                    notes = row.notes,
                    child_price = row.child_price,
                    age_from = row.age_from,
                    age_to = row.age_to,
                    policy_id = row.policy_id,
                    pricing_type = row.pricing_type,
                    trip_id = row.trip_id,
                    code_auto = row.code_auto,
                    currency_code = row.currency_code,

                };
                if (row.delete == true)
                {
                    _db.Remove(price);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }
                if (price.policy_id == 0)
                {
                    //check duplicate validation
                    var result = _db.child_policy_settings.Where(wr => wr.trip_id == price.trip_id && wr.currency_code == price.currency_code && wr.age_from == price.age_from && wr.age_to == price.age_to).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.child_policy_settings.Count() > 0)
                    {
                        maxId = _db.child_policy_settings.Max(d => d.policy_id);

                    }
                    price.created_at = DateTime.Now;
                    price.policy_id = maxId + 1;
                    price.code_auto = "CHDPO_" + price.policy_id;
                    _db.child_policy_settings.Add(price);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.child_policy_settings.Update(price);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = price.policy_id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
        #endregion

        #region "transfer"
        //get transfer category list
        public async Task<List<transfer_category>> GetTransfer_Categories()
        {
            try
            {
                return await _db.transfer_categories.ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<transfer_category>();
            }
        }
        //save transfer category
        public ResponseCls SaveTransferCategory(TransferCategorySaveReq row)
        {
            ResponseCls response;
            int maxId = 0;
            try
            {
                transfer_category transfer = new transfer_category
                {
                    id = row.id,
                    category_code = row.category_code,
                    category_name = row.category_name,
                    currency_code = row.currency_code,
                    created_by = row.created_by,
                    max_capacity = row.max_capacity,
                    max_price = row.max_price,
                    min_capacity = row.min_capacity,
                    min_price = row.min_price,
                    child_price = row.child_price,
                    notes = row.notes

                };
                if (row.delete == true)
                {
                    _db.Remove(transfer);
                    _db.SaveChanges();
                    return new ResponseCls { errors = null, success = true };
                }

                if (row.id == 0)
                {
                    //check duplicate validation
                    var result = _db.transfer_categories.Where(wr => wr.min_capacity == transfer.min_capacity && wr.max_capacity == transfer.max_capacity && wr.currency_code == transfer.currency_code).SingleOrDefault();
                    if (result != null)
                    {
                        return new ResponseCls { success = false, errors = _localizer["DuplicateData"] };
                    }
                    if (_db.transfer_categories.Count() > 0)
                    {
                        maxId = _db.transfer_categories.Max(d => d.id);

                    }
                    transfer.id = maxId + 1;
                    _db.transfer_categories.Add(transfer);
                    _db.SaveChanges();
                }
                else
                {
                    row.updated_at = DateTime.Now;
                    _db.transfer_categories.Update(transfer);
                    _db.SaveChanges();
                }

                response = new ResponseCls { errors = null, success = true, idOut = transfer.id };
            }
            catch (Exception ex)
            {
                response = new ResponseCls { errors = _localizer["CheckAdmin"], success = false, idOut = 0 };
            }
            return response;
        }
        #endregion

        #region "booking"
        public async Task<BookingAll> GetAllBooking(BookingAllReq req)
        {
            DateTime dateFrom = DateTime.ParseExact(req.date_from, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime dateTo = DateTime.ParseExact(req.date_to, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            int count = await _db.bookingwithdetails.Where(
                                                           wr => wr.lang_code == "en" &&
                                                           wr.trip_id == (req.trip_id == 0 ? wr.trip_id : req.trip_id) &&
                                                           wr.client_email == (string.IsNullOrEmpty(req.client_email) ? wr.client_email : req.client_email) &&
                                                           wr.booking_code == (string.IsNullOrEmpty(req.booking_code) ? wr.booking_code : req.booking_code) &&
                                                            (wr.trip_date >= dateFrom && wr.trip_date <= dateTo)
                                                          ).CountAsync();
            var result = await _db.bookingwithdetails.Where(
                                                           wr => wr.lang_code == "en" &&
                                                           wr.trip_id == (req.trip_id == 0 ? wr.trip_id : req.trip_id) &&
                                                           wr.client_email == (string.IsNullOrEmpty(req.client_email) ? wr.client_email : req.client_email) &&
                                                           wr.booking_code == (string.IsNullOrEmpty(req.booking_code) ? wr.booking_code : req.booking_code) &&
                                                            (wr.trip_date >= dateFrom && wr.trip_date <= dateTo)
                                                          )
                                                     .Skip((req.pageNumber - 1) * req.pageSize)
                                             .Take(req.pageSize).ToListAsync();
            return new BookingAll { totalPages = count, bookings = result };
        }
        #endregion
    
    
    }
}
