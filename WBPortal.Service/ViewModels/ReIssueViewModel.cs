using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using System.ComponentModel.DataAnnotations;

namespace WBSSLStore.Service.ViewModels
{
    [Serializable] 
    public class ReIssueViewModel
    {
        public string Errormsg { get; set; }
        public User user { get; set; }

        public int SiteID { get; set; }
        
        public PaymentMode paymentmode { get; set; }
        public bool ISCC { get; set; }
        public bool IsPayPal { get; set; }
        public string PayPalID { get; set; }
        public bool IsMoneybookers { get; set; }
        public string MoneybookersID { get; set; }
        public string CCName { get; set; }

        [StringLength(16)]
        public string CCNumber { get; set; }
        public string CVV { get; set; }
        //public string CardType { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string BillingCountry { get; set; }

        public decimal AvailableCredit { get; set; }
        public decimal SANAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public int NewSANAdded { get; set; }
        public string ReIssueUrl { get; set; }
        //Vat Related Details
        public int OrderDetailID { get; set; }
        public bool IsPaymentSuccess { get; set; }
        /// <summary>
        /// All products information added in ShoppinfCart.
        /// This property is used to send mail after order placed.
        /// </summary>
    }
}
