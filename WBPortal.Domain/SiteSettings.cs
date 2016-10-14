using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [Serializable]
    public class SiteSettings : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int SiteID { get; set; }
        [StringLength(64)]
        [Required]
        public string Key { get; set; }
        [MaxLength]
        [System.Web.Mvc.AllowHtml]
        [Required(AllowEmptyStrings = true)]
        [Column(TypeName = "ntext")]
        public string Value { get; set; }

        //Navigational Properties
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
    }
}