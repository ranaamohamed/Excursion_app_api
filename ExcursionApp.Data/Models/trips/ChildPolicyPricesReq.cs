using ExcursionApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.trips
{
    public class ChildPolicyPricesReq : child_policy_setting
    {
        public bool delete { get; set; }
    }
}
