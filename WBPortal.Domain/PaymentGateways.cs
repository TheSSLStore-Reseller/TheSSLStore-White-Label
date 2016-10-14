using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class PaymentGateways : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required] 
        public int InstancesID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "PaymentGatewayNameRequired", AllowEmptyStrings = false)]
        public string Name { get; set; }
        public string LoginID { get; set; }
        public string TransactionKey { get; set; }
        public string Password { get; set; }
        public string MerchantID { get; set; }
        public string KeyFilePath { get; set; }
        public string PartnerID { get; set; }
        public bool IsTestMode { get; set; }
        [StringLength(500)]
        public string LiveURL { get; set; }
        [StringLength(500)]
        public string TestURL { get; set; }
        public string AcceptCards { get; set; }
        public int SiteID { get; set; }
        public int AuditID { get; set; }

        [NotMapped]
        public PGInstances PGInstances
        {
            get { return (PGInstances)InstancesID; }
            set { InstancesID = (int)value; }
        }

        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }

        [ForeignKey("SiteID")]
        public Site Site { get; set; }


    }
}
