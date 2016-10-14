using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class WebServerType : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int BrandID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "WebServerNameRequired", AllowEmptyStrings = false)]
        public string WebServerName { get; set; }
        public bool isActive { get; set; }

        //Navigation
        [ForeignKey("BrandID")]
        public Brand Brand { get; set; }
    }
}
