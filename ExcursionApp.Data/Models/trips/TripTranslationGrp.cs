using ExcursionApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class TripTranslationGrp
    {
        public string? lang_code {  get; set; }
        public trip_translation translation { get; set; }
    }
}
