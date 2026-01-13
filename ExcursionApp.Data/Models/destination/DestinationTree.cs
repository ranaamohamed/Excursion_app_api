using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.destination
{
    public class DestinationTree : DestinationResponse
    {
        public List<DestinationTree> children { get; set; }
    }
}
