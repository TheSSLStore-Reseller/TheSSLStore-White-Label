using System.ComponentModel.DataAnnotations;

namespace WBSSLStore.Domain
{
    public class Brand : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "BrandNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Brand_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string BrandName { get; set; }
        [Display(Name = "isActive_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isActive { get; set; }
    }
}