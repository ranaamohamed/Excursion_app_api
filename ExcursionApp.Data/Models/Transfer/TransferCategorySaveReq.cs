using ExcursionApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcursionApp.Data.Models.Transfer
{
    public class TransferCategorySaveReq : transfer_category
    {
        public bool? delete {  get; set; }
    }
}
