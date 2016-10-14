using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class ShoppingCartDetail : IEntity
    {
        public int ID { get; set; }
        public int ShoppingCartID { get; set; }
        public int ProductID { get; set; }
        public int ProductPriceID { get; set; }
        public decimal Price { get; set; }
        public string PromoCode { get; set; }
        public string Comment { get; set; }
        public bool isNewOrder { get; set; }
        public bool IsCompetitiveUpgrade { get; set; }
        public int NumberOfServers { get; set; }
        public int AdditionalDomains { get; set; }
        public decimal PromoDiscount { get; set; }
        public string ExField1 { get; set; }
        public string ExField2 { get; set; }
        public string ExField3 { get; set; }
        public string ExField4 { get; set; }
        public string ExField5 { get; set; }

        //Navigation
        [ForeignKey("ShoppingCartID")]
        public ShoppingCart ShoppingCart { get; set; }
        [ForeignKey("ProductID")]
        public Product Product { get; set; }
        [ForeignKey("ProductPriceID")]
        public ProductPricing ProductPricing { get; set; }
      
    }
}