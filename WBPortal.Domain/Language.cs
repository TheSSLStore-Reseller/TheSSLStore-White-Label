using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [Serializable]
    public class Language : IEntity
    {
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "LangNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "LanguageName_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string LangName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "LangCodeRequired", AllowEmptyStrings = false)]
        [Display(Name = "LanguageCode_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string LangCode { get; set; }
        public int RecordStatusID { get; set; }

        [NotMapped]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus) RecordStatusID; }
            set { RecordStatusID = (int)value; }
        }

        //Navigational Properties
        public virtual ICollection<Site> Sites { get; set; }

        public object[] CultureCode { get; set; }
    }
}