using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class GatewayInteraction : IEntity
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public int AuditID { get; set; }
        [Column(TypeName="nvarchar(MAX)")]
        [MaxLength(4000)] 
        public string GatewayRequest { get; set; }
        [Column(TypeName = "nvarchar(MAX)")] 
        [MaxLength(4000)] 
        public string GatewayResponse { get; set; }
        public bool isSuccess { get; set; }
        public bool isPaymentTransaction { get; set; }
        public string GatewayAuthCode { get; set; }
        public string GatewayErrorCode { get; set; }
     
        

        //Navigation
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("AuditID")]
        public virtual Audit AuditDetails { get; set; }

    }
}