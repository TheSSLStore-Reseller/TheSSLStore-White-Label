using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class ResellerContract : IEntity
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int SiteID { get; set; }
        [Required(ErrorMessageResourceName = "ContractIDRequired", ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message))]
        public int ContractID { get; set; }
        public int AuditID { get; set; }

        //Navigational Properties
        [ForeignKey("UserID")]
        public virtual User Reseller { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("ContractID")]        
        public virtual Contract Contract { get; set; }
        [ForeignKey("AuditID")]
        public virtual Audit AuditDetails { get; set; }

        

    }

    public class CusUserUserOption
    {
        public ResellerContract objResContract { get; set; }
        public UserOptions objUserOption { get; set; }
    }
    
}