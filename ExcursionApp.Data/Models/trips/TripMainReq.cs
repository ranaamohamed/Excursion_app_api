using ExcursionApp.Data.Models.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class TripMainReq : PaginationReq
    {
        public int destination_id { get; set; }
        public int trip_type { get; set; }
    }
}
