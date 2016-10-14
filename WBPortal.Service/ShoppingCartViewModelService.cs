using System.Linq;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Data.Repository;
namespace WBSSLStore.Service
{
    public interface IShoppingCartViewModelService
    {
        ShoppingCart GetShoppingCart(int CartID, int UserID, string AnonymousToken, int SiteID);
        IQueryable<ShoppingCartDetail> GetShoppingCartDetails(int CartID);
        bool AddToCart(ShoppingCartViewModel cart);
        ShoppingCartJSonMessageModel DeleteFromCart(int id);
        bool EmptyCart(int CartID, int UserID, string AnonymousToken, int SiteID);
        ProductPricing GetProductPricing(int ppid, int SiteID);
        IQueryable<ProductPricing> GetProductPricingByProduct(int ProductID, int ContractID, int SiteID);
        PromoCode GetPromoRow(string code, int SiteID);
    }
    public class ShoppingCartViewModelService : IShoppingCartViewModelService
    {
        private readonly IRepository<PromoCode> _PromoCode;
        private readonly IRepository<ShoppingCartDetail> _shoppingCartDetails;
        private readonly IRepository<ShoppingCart> _shoppingCart;
        private readonly IRepository<ProductPricing> _ProductPricing;
        private readonly IRepository<OrderDetail> _OrderDetail;
        private readonly IUnitOfWork _unitOfWork;
     

        public ShoppingCartViewModelService(IRepository<OrderDetail> OrderDetail, IRepository<PromoCode> PromoCode, IRepository<ShoppingCartDetail> shoppingCartDetails, IRepository<ShoppingCart> shoppingCart, IRepository<ProductPricing> ProductPricing, IUnitOfWork UnitOfWork)
        {
            _OrderDetail = OrderDetail;
            _PromoCode = PromoCode;
            _ProductPricing = ProductPricing;
            _shoppingCart = shoppingCart;
            _shoppingCartDetails = shoppingCartDetails;
            _unitOfWork = UnitOfWork;
          

        }

        public ProductPricing GetProductPricing(int ppid, int SiteID)
        {
            return _ProductPricing.Find(c => c.ID == ppid && c.SiteID == SiteID && c.RecordStatusID == (int)RecordStatus.ACTIVE).EagerLoad(pp => pp.Product).FirstOrDefault();

        }

        public IQueryable<ProductPricing> GetProductPricingByProduct(int ProductID, int ContractID, int SiteID)
        {
            return _ProductPricing.Find(p => p.Product.ID == ProductID && p.ContractID == ContractID && p.SiteID == SiteID && p.RecordStatusID == (int)RecordStatus.ACTIVE);
        }
        public ShoppingCart GetShoppingCart(int CartID, int UserID, string AnonymousToken, int SiteID)
        {
            if (CartID > 0)
                return _shoppingCart.Find(sc => sc.ID == CartID && sc.SiteID == SiteID).FirstOrDefault();
            else if (UserID > 0)
                return _shoppingCart.Find(sc => sc.UserID == UserID && sc.SiteID == SiteID).FirstOrDefault();
            else if (!string.IsNullOrEmpty(AnonymousToken))
                return _shoppingCart.Find(sc => sc.UserAnonymousToken == AnonymousToken && sc.SiteID == SiteID).FirstOrDefault();


            return null;
        }
        public IQueryable<ShoppingCartDetail> GetShoppingCartDetails(int CartID)
        {
            if (CartID > 0)
                return _shoppingCartDetails.Find(sc => sc.ShoppingCart.ID == CartID).EagerLoad(c => c.ShoppingCart, c => c.Product, c => c.ProductPricing);

            return null;
        }


        public bool AddToCart(ShoppingCartViewModel cart)
        {
            if (cart.Cart.ID == 0)
                _shoppingCart.Add(cart.Cart);
            else
                _shoppingCart.Update(cart.Cart);

            foreach (ShoppingCartDetail s in cart.CartDetails)
            {
                if (s.ProductID > 0 && s.ProductPriceID > 0)
                {
                    if (s.ID == 0)
                        _shoppingCartDetails.Add(s);
                    else
                        _shoppingCartDetails.Update(s);
                }
            }
            _unitOfWork.Commit();
            return true;
        }

        public ShoppingCartJSonMessageModel DeleteFromCart(int id)
        {
            ShoppingCartDetail sd = _shoppingCartDetails.FindByID(id);
            ShoppingCart spCrt = _shoppingCart.FindByID(sd.ShoppingCartID);
            if (sd != null)
            {
              
                int CartID = sd.ShoppingCartID;
                int SiteID = spCrt.SiteID;
                 
                _shoppingCartDetails.Delete(sd);

                _unitOfWork.Commit();


                var cartItems = _shoppingCartDetails.Find(s => s.ShoppingCartID == CartID);
              
                

                var result = new ShoppingCartJSonMessageModel();

                result.Message = "Product has been removed from your shopping cart.";
                result.ItemCount = cartItems.Count().ToString();

                if (System.Convert.ToInt32(result.ItemCount) > 0)
                    result.CartTotal = string.Format("{0:C}", cartItems.Sum(p => p.Price) - cartItems.Sum(p => p.PromoDiscount));

                if (cartItems.Count() > 0)
                    result.Id = id.ToString();
                else
                {
                   result.Id = System.Convert.ToString(SiteID); // ((CurrentSiteSettings.IsSiteRunWithHTTPS) ? "https://" : "http://") + ((CurrentSiteSettings.IsRunWithWWW) ? "www." : "") + CurrentSiteSettings.CurrentSite.Alias;
                }

                cartItems = null; 

                return result;
            }
            else
            {
                return null;

            }

        }

        public bool EmptyCart(int CartID, int UserID, string AnonymousToken, int SiteID)
        {

            return false;
        }

        public PromoCode GetPromoRow(string code, int SiteID)
        {
            PromoCode promorow = _PromoCode.Find(promo => promo.Code == code && promo.EndDate >= System.DateTimeWithZone.Now && promo.StartDate <= System.DateTimeWithZone.Now && promo.SiteID == SiteID).FirstOrDefault();

            if (promorow != null && promorow.ID > 0)
            {
                int MaxOrder = _OrderDetail.Find(o => o.PromoCodeID == promorow.ID).Count();

                if (MaxOrder >= promorow.MaxOrders)
                    return null;
                else
                    return promorow;

            }

            return null;
        }
    }

}
