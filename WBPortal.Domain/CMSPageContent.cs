using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class CMSPageContent : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int CMSPageID { get; set; }
        [MaxLength]
        [System.Web.Mvc.AllowHtml]
        [Column(TypeName="ntext")]
        public string PageContent { get; set; }
        [StringLength(64)]
        public string PageContentKey { get; set; }

        //Navigational Properties
        [ForeignKey("CMSPageID")]
        public virtual CMSPage CmsPage { get; set; }
    }
}