using ExcursionApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class FacilityAllWithSelect : facility_main
    {
        public bool selected { get; set; }
        public long fac_trip_id { get; set; }
        public long? facility_id { get; set; }
        public string? pricing_type_name { get; set; }
    }
}
