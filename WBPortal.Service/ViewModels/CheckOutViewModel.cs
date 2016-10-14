using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using System.ComponentModel.DataAnnotations;
using WBSSLStore.Resources;

namespace WBSSLStore.Service.ViewModels
{
    [Serializable] 
    public class CheckOutViewModel
    {
        public string Errormsg { get; set; }
        public User user { get; set; }

        public int SiteID { get; set; }
        public int ShoppingCartID { get; set; }
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
        public decimal AvailableCredit { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal PromoDiscount { get; set; }
        public decimal PayableAmount { get; set; }

        public string BillingCountry { get; set; }
        
        //Vat Related Deatils
        public bool ISVatApplicable { get; set; }
        public decimal Tax { get; set; }
        public string VATNumber { get; set; }
        public int VatCountry { get; set; }
        public int VatPercent { get; set; }

        public bool IsNewuser { get; set; }
        public int OrderID { get; set; }
        public bool IsPaymentSuccess { get; set; }
        /// <summary>
        /// All products information added in ShoppinfCart.
        /// This property is used to send mail after order placed.
        /// </summary>
        public string AllProductInfo { get; set; }
        /// <summary>
        /// Invoice Number with Prefix
        /// This property is used to send mail after order placed.
        /// </summary>
        public string InvoiceNumber { get; set; }
       
    }
    [Serializable]
    public class AddFundResponse
    {
        public string ErrorMessage { get; set; }
        public string RedirectUrl { get; set; }
        public bool isSuccess { get; set; }
    }

   
}
