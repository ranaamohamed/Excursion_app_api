using ExcursionApp.Data.Models.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.destination
{
    public class DestinationWithTranslationsReq : PaginationReq
    {
        public string? country_code { get; set; }
        public bool? leaf { get; set; }
        public int? parent_id { get; set; }
        public int trip_type { get; set; } = 1;
    }
}
