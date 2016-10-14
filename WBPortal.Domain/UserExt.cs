using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [NotMapped]
    public class UserExt : User
    {
        [NotMapped]
        public string ContractName { get; set; }
        [NotMapped]
        public DateTime RegisterDate { get; set; }
        [NotMapped]
        public decimal? TotalPurchase { get; set; }
        [NotMapped]
        public decimal? CuurentBalance { get; set; }
        [NotMapped]
        public int? TotalOrders { get; set; }
        [NotMapped]
        public int? TotalInCompleteOrders { get; set; }
        [NotMapped]
        public int? TotalSupportIncident { get; set; }
    }
}
