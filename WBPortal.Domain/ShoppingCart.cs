using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class ShoppingCart : IEntity
    {
        public int ID { get; set; }
        public int AuditID { get; set; }
        public int? UserID { get; set; }
        public string UserAnonymousToken { get; set; }
        public int SiteID { get; set; }
        public string Email { get; set; }
    
        //Navigation
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}