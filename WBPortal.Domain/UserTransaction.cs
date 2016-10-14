using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class UserTransaction : IEntity  
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public int UserID { get; set; }
        public decimal TransactionAmount { get; set; }
        public string ReceipientInstrumentDetails { get; set; }
        public int AuditID { get; set; }
        public int? RefundRequestID { get; set; }
        public int? OrderDetailID { get; set; }
        public int? PaymentID { get; set; }
        public string Comment { get; set; }

        public int TransactionModeID { get; set; }
        [NotMapped]
        public TransactionMode TransactionMode
        {
            get { return (TransactionMode) TransactionModeID; }
            set { TransactionModeID = (int) value; }
        }


        //Navigation
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("AuditID")]
        public virtual Audit AuditDetails { get; set; }
        [ForeignKey("RefundRequestID")]
        public virtual SupportRequest RefundRequest { get; set; }
        [ForeignKey("OrderDetailID")]
        public virtual OrderDetail OrderDetail { get; set; }
        [ForeignKey("PaymentID")]
        public virtual Payment Payment { get; set; }
    }
}
