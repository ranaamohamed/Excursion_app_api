using ExcursionApp.Data.Entities;
using ExcursionApp.Data.Models.trips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.Bookings
{
    public class BookingWithTripDetailsAll 
    {
        public long? trip_id { get; set; }
        public int? trip_type { get; set; }
        public string? booking_code { get; set; }
        public int? child_num { get; set; }

        public string? client_email { get; set; }
        public string? client_name { get; set; }

        public string? client_id { get; set; }

        public string? client_nationality { get; set; }

        public string? client_phone { get; set; }

        public string? gift_code { get; set; }

        public int? infant_num { get; set; }

        public string? pickup_address { get; set; }

        public string? pickup_time { get; set; }

        public int? total_pax { get; set; }

        public decimal? total_price { get; set; }

        public string? trip_code { get; set; }

        public decimal? review_rate { get; set; }

        public string? lang_code { get; set; }

        public string? trip_name { get; set; }

        public string? currency_code { get; set; }

        public long? booking_id { get; set; }

        public string? trip_datestr { get; set; }

        public string? booking_datestr { get; set; }
        public List<TripsPickupResponse> pickups { get; set; }
    }
}
