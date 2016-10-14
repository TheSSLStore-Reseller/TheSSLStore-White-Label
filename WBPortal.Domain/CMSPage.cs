using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class CMSPage : IEntity
    {
        public int ID { get; set; }
        public int PageID { get; set; }
        public int LangID { get; set; }
        [StringLength(256)]
        [Display(Name= "Title_Caption" , ResourceType= typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "TitleRequired", AllowEmptyStrings = true)]
        public string Title { get; set; }
        [StringLength(1024)]
        [Display(Name = "Keywords_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "KeywordsRequired", AllowEmptyStrings = true)]
        public string Keywords { get; set; }
        [StringLength(1024)]
        [Display(Name = "Description_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Description { get; set; }
        [StringLength(1024)]
        [Display(Name = "HeaderSection_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [System.Web.Mvc.AllowHtml]
        public string HeaderSection { get; set; }
        [StringLength(1024)]
        [Display(Name = "FooterSection_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [System.Web.Mvc.AllowHtml]
        public string FooterSection { get; set; }
               
        //Navigational Properties
        [ForeignKey("PageID")]
        public virtual Pages Pages { get; set; }
        [ForeignKey("LangID")]
        public virtual Language Language { get; set; }
    
    }
}
