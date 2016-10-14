using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class ProductAvailablity : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        public int SiteID { get; set; }
        public bool isActive { get; set; }

        //Navigational Properies
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
    }


}