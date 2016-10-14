using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class Payment : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int GatewayInteractionID { get; set; }
        public int BillingAddressID { get; set; }
        public decimal TransactionAmount { get; set; }
        public int AuditID { get; set; }
        public string CCNumber { get; set; }

        public int PaymentModeID { get; set; }
        [NotMapped]
        public PaymentMode PaymentMode
        {
            get { return (PaymentMode) PaymentModeID; }
            set { PaymentModeID = (int) value; }
        }

        //Navigation
        [ForeignKey("GatewayInteractionID")]
        public GatewayInteraction GatewayInteraction { get; set; }
        [ForeignKey("BillingAddressID")]
        public Address BillingAddress { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
    }
}