using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class SupportRequest : IEntity
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int SiteID { get; set; }
        public string ExternalIncidentID { get; set; }
        public string WebServerType { get; set; }
        public bool isOpen { get; set; }
        public int AuditID { get; set; }
        public decimal AmountCharged { get; set; }
        public string Subject { get; set; }
        [MaxLength]
        [Column(TypeName = "ntext")]
        public string Comment { get; set; }

        public int? OrderDetailID { get; set; }
        public decimal? RefundAmount { get; set; }

        public int RefundStatusID { get; set; }
        [NotMapped]
        public RefundStatus RefundStatus
        {
            get { return (RefundStatus)RefundStatusID; }
            set { RefundStatusID = (int)value; }
        }
        public int SupportTypeID { get; set; }
        [NotMapped]
        public SupportType SupportType
        {
            get { return (SupportType) SupportTypeID; }
            set { SupportTypeID = (int) value; }
        }

        [StringLength(512)] 
        public string Reason { get; set; }
        //Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
        [ForeignKey("OrderDetailID")]
        public OrderDetail OrderDetail { get; set; }
    }
}
