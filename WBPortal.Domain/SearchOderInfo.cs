using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [NotMapped]
    public class SearchOderInfo
    {
        [NotMapped]
        public int OrderDetailID
        { get; set; }
        [NotMapped]
        public int OrderID
        { get; set; }
        [NotMapped]
        public DateTime OrderDate
        { get; set; }
        [NotMapped]
        public string OrderNumber
        { get; set; }
        [NotMapped]
        public string ProductName
        { get; set; }
        [NotMapped]
        public string DomainName
        { get; set; }
        [NotMapped]
        public int OrderStatusID
        { get; set; }
        [NotMapped]
        public decimal Price
        { get; set; }
    }
}
