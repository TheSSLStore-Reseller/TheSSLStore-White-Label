using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Service.ViewModels
{
    public class AccountSummary
    {
        public decimal TotalTransaction{get;set;}
        public decimal Balance { get; set; }
        public int TotalOrders { get; set; }
        public int TotalInCompleteOrders { get; set; }
        public int TotalSupportIncident { get; set; }
    }
    [Serializable]
    public class FileUploadResponse
    {
        public string FilePath { get; set; }
        public string PhysicalPath { get; set; }
        public bool NeedToCrop { get; set; }
        public string Type { get; set; }
    }
}
