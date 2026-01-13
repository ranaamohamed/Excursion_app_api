using ExcursionApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class FacilityTranslationReq : facility_translation
    {
        public bool delete { get; set; }
    }
}
