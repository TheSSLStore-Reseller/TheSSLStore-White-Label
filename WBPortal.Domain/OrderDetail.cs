using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class OrderDetail : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int OrderID { get; set; }
        [Required]
        public int ProductID { get; set; }
        public int? StoreAPIInteractionID { get; set; }
        [Display(Name = "Price_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public decimal Price { get; set; }
        [Required]
        public int CertificateRequestID { get; set; }
         [Display(Name = "CertificateExpiresOn_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? CertificateExpiresOn { get; set; }
        [Required]
        [Display(Name = "Product_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ProductName { get; set; }
        [Required]
        [Display(Name = "NumberOfMonths_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int NumberOfMonths { get; set; }
        public int PromoCodeID { get; set; }
        [Display(Name = "ExternalOrderID_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ExternalOrderID { get; set; }
        [Display(Name = "PromoDiscount_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public decimal PromoDiscount { get; set; }
        public int AuditID { get; set; }
        [Display(Name = "ActivatedDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? ActivatedDate { get; set; }

        public int OrderStatusID { get; set; }

        [DataType(DataType.Currency)]
        public decimal VATAmount { get; set; }


        [DataType(DataType.Currency)]
        public decimal GrossAmount { get; set; }
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }

        [NotMapped]
        [Display(Name = "OrderStatus_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public OrderStatus OrderStatus
        {
            get { return (OrderStatus)OrderStatusID; }
            set { OrderStatusID = (int)value; }
        }

      

        //Navigation
        [ForeignKey("OrderID")]
        public Order Order { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; }
        [ForeignKey("CertificateRequestID")]
        public CertificateRequest CertificateRequest { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
        [ForeignKey("StoreAPIInteractionID")]
        public GatewayInteraction StoreAPIInteraction { get; set; }

   
       //  Use this property to store Invoice Number with Invoice Prefix
        [NotMapped]
        [Display(Name = "InvoiceNo_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string InvoiceNumber { get; set; }
    }
}