using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [NotMapped]
    public class AdminDashboard : User
    {
        [NotMapped]
        public int RenewalAlerts { get; set; }
        [NotMapped]
        public int 	TotalOrders { get; set; }
        [NotMapped]
        public int ActiveUsers { get; set; }
        [NotMapped]
        public int RefundRequest { get; set; }
        [NotMapped]
        public int ActiveContracts { get; set; }
        [NotMapped]
        public int ActiveResellers { get; set; }
        [NotMapped]
        public int InactiveResellers { get; set; }
        
    }
}
