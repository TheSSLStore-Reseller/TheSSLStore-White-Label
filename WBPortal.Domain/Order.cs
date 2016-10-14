using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class Order : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "OrderDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
    [DataType( System.ComponentModel.DataAnnotations.DataType .DateTime)] 
        public DateTime OrderDate { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public int SiteID { get; set; }
        [Required]
        [Display(Name = "TotalPrice_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public decimal TotalPrice { get; set; }
        public int BillingAddressID { get; set; }
        public int AuditID { get; set; }
        public int OrderStatusID { get; set; }

        [Display(ResourceType = typeof(Resources.GeneralMessage.Message),Name="VAT")]
        [StringLength(64)]  
        public string VATNumber{get; set;}



        [NotMapped]
        public PymentProcessStatus OrderStatus
        {
            get { return (PymentProcessStatus)OrderStatusID; }
            set { OrderStatusID = (int) value;}
        }
    
        //Navigation
        [ForeignKey("UserID")]
        public User User { get; set; }
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
        [ForeignKey("BillingAddressID")]
        public Address BillingAddress { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
    }
}