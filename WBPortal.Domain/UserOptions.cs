
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{

    public class UserOptions : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int SiteID { get; set; }
        public bool? StopResellerEmail
        {
            get;
            set;

        }
        public bool? StopResellerCustomerEmail { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        public DateTime DateAdded { get; set; }


        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }

        //[NotMapped]
        //public bool StopResellerEmailC
        //{
        //    get { return StopResellerEmail ?? false; }
        //    set { StopResellerEmail = value; }

        //}

    }
}
