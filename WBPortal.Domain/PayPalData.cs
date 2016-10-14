using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class PayPalData : IEntity
    {
        public int ID { get; set; }
        [MaxLength]
        [Column(TypeName = "ntext")]
        public string PayPalDatas { get; set; }
        public string SessionID { get; set; }
        public string OrderType { get; set; }
        public int SiteID { get; set; }

        //Navigation
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
    }
}