using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.Bookings.Client
{
    public class BookingPrice
    {
        public decimal? total_adult_price { get; set; }
        public decimal? total_child_price { get; set; }
        public decimal? final_price { get; set; }
        public string? message { get; set; }
        public bool? success { get; set; }
    }
}
