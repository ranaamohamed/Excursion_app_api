using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.Bookings
{
    public class TripExtraReq
    {
        public long? trip_id {  get; set; }

        public string? lang_code { get; set; }
        public bool? isExtra { get; set; }
    }
}
