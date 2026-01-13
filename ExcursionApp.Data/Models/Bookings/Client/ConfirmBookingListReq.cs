using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.Bookings.Client
{
    public class ConfirmBookingListReq
    {
        public List<long?>? booking_ids { get; set; }
        public string? client_id { get; set; }
        public string? lang_code { get; set; }
        public string? client_email { get; set; }
        public string? client_nationality { get; set; }
        public string? client_name { get; set; }
        public string? client_phone { get; set; }
        public string? booking_notes { get; set; }
        
    }
}
