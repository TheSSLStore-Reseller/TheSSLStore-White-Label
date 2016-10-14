using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [Serializable]
    public class Country : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "CountryNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Country_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CountryName { get; set; }
        [Display(Name = "ISOName_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ISOName { get; set; }
        public int RecordStatusID { get; set; }

        [NotMapped]
        [Display(Name = "RecordStatus_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus) RecordStatusID; }
            set { RecordStatusID = (int) value; }
        }
    }
}