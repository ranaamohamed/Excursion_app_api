using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class TripMainCastPag
    {
        public int totalPages { get; set; }
        public List<TripMainCast> trips { get; set; }
    }
}
