using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class Contract : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ContractNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Contract_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        
        public string ContractName { get; set; }
        [Display(Name = "ContractLevel_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Range(1, 99999999, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ContractLevelRequired")]        
        public decimal? ContractLevel { get; set; }
        public int SiteID { get; set; }
        [Display(Name = "isAutoCalculation_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isAutoCalculation { get; set; }
        [Display(Name = "isForReseller_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isForReseller { get; set; }

        public int RecordStatusID { get; set; }
        [NotMapped]
        [Display(Name = "RecordStatus_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus)RecordStatusID; }
            set { RecordStatusID = (int)value; }
        }


        //Navigational Properties
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }


    }
}