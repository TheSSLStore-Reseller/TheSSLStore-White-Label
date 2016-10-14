using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{
    public class SupportDetail : IEntity
    {   
        public int ID { get; set; }
        public int SupportRequestID { get; set; }
        public int AuditID { get; set; }
        [MaxLength]
        [Column(TypeName="ntext")]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CommentsRequired", AllowEmptyStrings = false)]
        public string Comments { get; set; }
        [MaxLength]
        [Column(TypeName = "ntext")]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "StaffNoteRequired", AllowEmptyStrings = true)]
        public string StaffNote { get; set; }
        //Navigation
        [ForeignKey("SupportRequestID")]
        public virtual SupportRequest SupportRequest { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
    }
}