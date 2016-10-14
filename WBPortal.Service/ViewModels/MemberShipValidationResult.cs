using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;

namespace WBSSLStore.Service.ViewModels
{
    public class MemberShipValidationResult
    {
        public bool IsSuccess { get; set; }
        public string errormsg { get; set; }
        public bool IsSetAuthCookie { get; set; }
        public string UserName { get; set; }
        public string ReturnUrl { get; set; }

    }
}
