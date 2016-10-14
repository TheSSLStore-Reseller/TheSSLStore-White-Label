using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Domain;
using WBSSLStore.Data.Repository;
using WBSSLStore.Web.Helpers.PagedList;
using System.Web.Security;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Controllers
{
    [HandleError]
    public class ShoppingCartController : WBController<ShoppingCartViewModel, IRepository<ShoppingCartDetail>, IShoppingCartViewModelService>
    {
        //
        // GET: /Store/
        private string promo
        {
            get
            {
                if (!string.IsNullOrEmpty(Request.QueryString[SettingConstants.QS_PROMOCODE]))
                {
                    return Request.QueryString[SettingConstants.QS_PROMOCODE];
                }

                return string.Empty;
            }
        }
        private string AnonymousID
        {
            get
            {
                return Request.AnonymousID;
            }
        }

        private User _loginuser = null;
        private User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (_loginuser == null)
                    {
                        SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
                        if (loginuser != null && loginuser.Details != null)
                            return loginuser.Details;
                    }
                }
                return _loginuser;
            }


        }



        public ActionResult addtocart(int ppid, int qty, int? san, int? server)
        {
            ShoppingCartDetail _Detail = new ShoppingCartDetail();
            _Detail.ProductPriceID = ppid;
            _Detail.AdditionalDomains = (san != null ? Convert.ToInt32(san) : 0);
            _Detail.Comment = string.Empty;
            _Detail.IsCompetitiveUpgrade = false;
            _Detail.isNewOrder = true;

            _Detail.NumberOfServers = server.HasValue ? server.Value : 1;
            _Detail.PromoCode = string.Empty;

            AddDataInCart(_Detail, qty);

            ViewBag.CartTotal = _viewModel.CartDetails.Sum(p => p.Price);
            ViewBag.TotalDiscount = _viewModel.CartDetails.Sum(p => p.PromoDiscount);
            //return View("Index", _viewModel);
            string url = "/shoppingcart";
            return RedirectToRoute("shoppingcart_us");
            //return Redirect(WBHelper.ApllicationFullPath + url);

        }

        [Route("shoppingcart/cart", Name = "shoppingcart_us")]
        public ActionResult Index()
        {
            GetShoppingCart();

            if (_viewModel.CartDetails != null && _viewModel.CartDetails.Count > 0)
            {

                foreach (ShoppingCartDetail sd in _viewModel.CartDetails)
                {
                    if (!string.IsNullOrEmpty(sd.PromoCode))
                    {
                        AssignPromoInCart(_service.GetPromoRow(sd.PromoCode, Site.ID), sd);
                    }
                }
                ViewBag.Username = _loginuser != null ? _loginuser.FirstName + " " + _loginuser.LastName : string.Empty;
                ViewBag.CartTotal = _viewModel.CartDetails.Sum(p => p.Price);
                ViewBag.TotalDiscount = _viewModel.CartDetails.Sum(p => p.PromoDiscount);
                return View(_viewModel);
            }
            else
                return Redirect("/");

        }

        [HttpPost]
        public ActionResult Edit(ShoppingCartDetail model, FormCollection collection)
        {


            if (model != null && model.ShoppingCartID > 0 && model.ProductPriceID > 0 && model.ID > 0)
            {
                ProductPricing pp = _service.GetProductPricing(model.ProductPriceID, model.ShoppingCart.SiteID > 0 ? model.ShoppingCart.SiteID : Site.ID);

                if (pp != null)
                {

                    model.Price = CalCulatePrice(pp, model.NumberOfServers, model.AdditionalDomains);

                    model.ProductPricing = pp;
                    model.Product = pp.Product;

                    AssignPromoInCart(_service.GetPromoRow(model.PromoCode, model.ShoppingCart.SiteID > 0 ? model.ShoppingCart.SiteID : Site.ID), model);

                }

                model.ShoppingCart = null;
                _repository.Update(model);
                _unitOfWork.Commit();

                return Json("ok");
            }
            else if (model != null && model.ProductPriceID > 0 && model.ProductID > 0)
            {
                AddDataInCart(model, Convert.ToInt32(collection["drpQty"]));
                return Json("ok");
            }
            else
                return Json("flase");
        }

        [Route("shoppingcart/checkoutresult/{sid?}/{errorcode?}", Name = "CheckOutResult_err")]
        public ActionResult CheckOutResult(int sid, int errorcode)
        {

            string Error = "";

            if (errorcode.Equals(-1))
                Error = WBSSLStore.Resources.ErrorMessage.Message.UnmandPassWrong;
            else if (errorcode.Equals(-2))
                Error = WBSSLStore.Resources.ErrorMessage.Message.ValidUsernameandPass;
            else
            {
                Error = WBSSLStore.Resources.ErrorMessage.Message.Autherr;
            }

            ViewBag.SiteID = Site.ID;
            ViewBag.Error = Error;
            ViewBag.ShoppingCartID = sid;
            return View("checkout");
        }

        [HttpGet]
        [Route("shoppingcart/checkout/{id?}", Name = "shoppingcart_chkout")]
        public ActionResult Checkout(int id)
        {
            string url = "/checkout/payment/index/" + id;
            string AuthToken = string.Empty;
            if (CurrentUser != null && CurrentUser.ID > 0)
            {
                ViewBag.Username = _loginuser != null ? _loginuser.FirstName + " " + _loginuser.LastName : string.Empty;
                AuthToken = HttpUtility.UrlEncode(WBSSLStore.CryptorEngine.Encrypt(CurrentUser.ID + SettingConstants.Seprate + CurrentUser.Email + SettingConstants.Seprate + url, true));
                return Redirect(WBHelper.ApllicationSecurePath + "/" + url);
            }
            else
            {
                ViewBag.SiteID = Site.ID;
                ViewBag.ShoppingCartID = id;
                return View();
            }


        }
        [HttpGet]
        public ActionResult Edit(int id, int? ppid)
        {
            ShoppingCartDetail sd = null;
            ShoppingCart sp = null;
            if (id > 0)
                sd = _repository.Find(x => x.ID == id).EagerLoad(x => x.Product, x => x.ShoppingCart, x => x.ProductPricing).FirstOrDefault();
            else
            {
                sd = new ShoppingCartDetail();

                sd.ProductPriceID = Convert.ToInt32(ppid);
                sd.ProductPricing = _service.GetProductPricing(sd.ProductPriceID, Site.ID);
                sd.Product = sd.ProductPricing.Product;
                sd.ProductID = sd.ProductPricing.Product.ID;
                sd.isNewOrder = true;
            }

            if (sd != null)
            {
                int SiteID = 0;
                if (sd.ShoppingCart != null && sd.ShoppingCart.SiteID > 0)
                    SiteID = sd.ShoppingCart.SiteID;
                else if (Site != null && Site.ID > 0)
                    SiteID = Site.ID;
                //GetSite(sd.ShoppingCart.SiteID);

                GetSite(SiteID);


                int ContractID = 0;
                if (CurrentUser != null)
                {
                    ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(CurrentUser.ID, Site.ID > 0 ? Site.ID : sd.ShoppingCart.SiteID);
                }
                else
                    ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(0, Site.ID > 0 ? Site.ID : sd.ShoppingCart.SiteID);

                List<ProductPricing> price = _service.GetProductPricingByProduct(sd.ProductID, ContractID, Site.ID > 0 ? Site.ID : sd.ShoppingCart.SiteID).ToList();

                List<SelectListItem> s1 = new List<SelectListItem>();
                foreach (ProductPricing p in price)
                {
                    s1.Add(new SelectListItem
                    {
                        Text = p.NumberOfMonths >= 12 ? (p.NumberOfMonths / 12).ToString() + " " + @WBSSLStore.Resources.GeneralMessage.Message.Year_Caption + "--" + (p.SalesPrice / (p.NumberOfMonths / 12)).ToString("c") + @WBSSLStore.Resources.GeneralMessage.Message.per_year_Caption :
                        p.NumberOfMonths.ToString() + " " + @WBSSLStore.Resources.GeneralMessage.Message.Months_Caption,
                        Value = p.ID.ToString(),
                        Selected = p.ID == id
                    });
                }


                ViewBag.Price = s1;


                List<SelectListItem> s2 = new List<SelectListItem>();
                if (sd.Product.isSANEnabled)
                {
                    int SanMin = Convert.ToInt32(sd.Product.SanMin);
                    int SanMax = Convert.ToInt32(sd.Product.SanMax);
                    for (int icnt = SanMin; icnt <= SanMax; icnt++)
                    {
                        s2.Add(new SelectListItem { Text = icnt.ToString(), Value = icnt.ToString(), Selected = icnt == sd.AdditionalDomains });
                    }
                }
                else
                {
                    s2.Add(new SelectListItem { Text = "0", Value = "0" });
                }

                ViewBag.Domains = s2;


            }

            return View(sd);


        }
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            var result = _service.DeleteFromCart(id);

            if (Convert.ToInt32(result.ItemCount) < 0 || Convert.ToInt32(result.ItemCount) == 0)
            {
                CurrentSiteSettings curret = new CurrentSiteSettings(GetSite(Convert.ToInt32(result.Id)));
                ViewBag.Site = curret.CurrentSite.Alias;
                result.Id = ((curret.IsSiteRunWithHTTPS) ? "https://" : "http://") + ((curret.IsRunWithWWW) ? "www." : "") + curret.CurrentSite.Alias;

            }
            //else
            //    result.Id = "/";

            return Json(result);
        }

        [HttpPost]
        public ActionResult ApplyPromo(string code)
        {
            var result = new ShoppingCartJSonMessageModel();
            result.ItemCount = "0";
            result.Message = WBSSLStore.Resources.ErrorMessage.Message.ValidPromocode;
            PromoCode PRow = _service.GetPromoRow(code, Site.ID);
            if (PRow != null && PRow.ID > 0)
            {
                GetShoppingCart();

                foreach (ShoppingCartDetail sd in _viewModel.CartDetails)
                {
                    AssignPromoInCart(PRow, sd);
                    if (!string.IsNullOrEmpty(sd.PromoCode) && sd.PromoDiscount > 0)
                    {
                        result.ItemCount = "1";
                        result.Message = WBSSLStore.Resources.ErrorMessage.Message.PromoAppliedSuccess;
                        result.promodiscount += string.Format("{0:C}", sd.PromoDiscount) + "|-----|";
                        result.Id += sd.ID + "|-----|";
                    }
                }

                decimal? amount = _viewModel.CartDetails.Sum(p => (decimal?)p.Price) - _viewModel.CartDetails.Sum(p => (decimal?)p.PromoDiscount);
                result.CartTotal = string.Format("{0:C}", amount ?? 0);
                _service.AddToCart(_viewModel);

            }
            return Json(result);
        }

        private decimal CalCulatePrice(ProductPricing pp, int NoOfServer, int NoOfDomains)
        {
            decimal price = 0;
            decimal AdditionDomainPrice = 0;
            if (pp != null)
            {
                if (pp.Product.isSANEnabled)
                {
                    int SanMin = Convert.ToInt32(pp.Product.SanMin);
                    if (NoOfDomains > SanMin)
                    {
                        AdditionDomainPrice = pp.AdditionalSanPrice * (NoOfDomains - SanMin);
                    }


                }

                if (!pp.Product.isNoOfServerFree)
                {
                    price = pp.SalesPrice * NoOfServer;
                }
                else
                    price = pp.SalesPrice;


                price += AdditionDomainPrice;
            }

            return price;
        }


        private void GetShoppingCart()
        {
            ShoppingCart sc = null;
            if (CurrentUser != null && CurrentUser.ID > 0)
            {
                sc = _service.GetShoppingCart(0, CurrentUser.ID, AnonymousID, Site.ID);
            }
            else
                sc = _service.GetShoppingCart(0, 0, AnonymousID, Site.ID);

            if (sc != null)
            {
                var cart = _service.GetShoppingCartDetails(sc.ID).ToList();
                _viewModel.Cart = sc;
                if (cart != null && cart.Count > 0)
                {
                    _viewModel.CartDetails = cart;
                }
                else
                {
                    if (_viewModel.CartDetails == null || (_viewModel.CartDetails != null && _viewModel.CartDetails.Count == 0))
                        _viewModel.CartDetails = new List<ShoppingCartDetail>();

                }

            }
            else
            {
                _viewModel.Cart = new ShoppingCart();

                if (_viewModel.CartDetails == null)
                    _viewModel.CartDetails = new List<ShoppingCartDetail>();
            }



        }

        private void AssignPromoInCart(PromoCode prow, ShoppingCartDetail cartDetails)
        {
            decimal promodiscount = 0;

            if (prow == null)
            {
                cartDetails.PromoCode = string.Empty;
                cartDetails.PromoDiscount = 0;
                return;
            }

            if (prow != null && cartDetails != null && cartDetails.ShoppingCartID > 0)
            {
                if (cartDetails.ProductPricing == null && cartDetails.ProductPriceID > 0)
                {
                    cartDetails.ProductPricing = _service.GetProductPricing(cartDetails.ProductPriceID, Site.ID);
                }

                if (cartDetails.ProductID == prow.ProductID && cartDetails.ProductPricing.NumberOfMonths == prow.NoOfMonths)
                {

                    if (prow.Discount > 0)
                    {
                        if (prow.DiscountModeID == (int)DiscountMode.FLAT)
                        {
                            promodiscount = prow.Discount;
                            if (!cartDetails.Product.isNoOfServerFree)
                                promodiscount = (prow.Discount * (cartDetails.NumberOfServers > 0 ? cartDetails.NumberOfServers : 1));
                        }
                        else if (prow.DiscountModeID == (int)DiscountMode.PERCENTAGE)
                        {
                            promodiscount = (cartDetails.ProductPricing.SalesPrice * prow.Discount) / 100;
                            if (!cartDetails.Product.isNoOfServerFree)
                                promodiscount = (promodiscount * (cartDetails.NumberOfServers > 0 ? cartDetails.NumberOfServers : 1));

                        }
                    }

                    cartDetails.PromoCode = prow.Code;
                    cartDetails.PromoDiscount = promodiscount;

                }

            }
        }


        private void AddDataInCart(ShoppingCartDetail _Detail, int Quantity)
        {
            GetShoppingCart();
            if (Quantity <= 0)
                Quantity = 1;
            if (_viewModel.Cart.ID == 0)
            {
                _viewModel.Cart.AuditID = 1;
                _viewModel.Cart.SiteID = Site.ID;
                if (CurrentUser != null && CurrentUser.ID > 0)
                {
                    _viewModel.Cart.UserID = CurrentUser.ID;
                    _viewModel.Cart.UserAnonymousToken = string.Empty;
                }
                else
                    _viewModel.Cart.UserAnonymousToken = AnonymousID;

                _viewModel.Cart.Email = "";
            }


            ShoppingCartDetail cartdetail;

            ProductPricing pp = _service.GetProductPricing(_Detail.ProductPriceID, Site.ID);
            PromoCode prow = null;
            if (promo != "")
            {
                prow = _service.GetPromoRow(promo, Site.ID);

            }

            for (int iCnt = 1; iCnt <= Quantity; iCnt++)
            {

                cartdetail = new ShoppingCartDetail();

                cartdetail.AdditionalDomains = _Detail.AdditionalDomains;
                cartdetail.IsCompetitiveUpgrade = _Detail.IsCompetitiveUpgrade;
                cartdetail.NumberOfServers = _Detail.NumberOfServers;
                cartdetail.isNewOrder = _Detail.isNewOrder;
                cartdetail.PromoCode = _Detail.PromoCode;
                cartdetail.ProductID = pp.ProductID;
                cartdetail.ProductPriceID = _Detail.ProductPriceID;
                cartdetail.Comment = _Detail.Comment;
                cartdetail.Price = CalCulatePrice(pp, _Detail.NumberOfServers, _Detail.AdditionalDomains); ;
                cartdetail.ShoppingCart = _viewModel.Cart;

                AssignPromoInCart(prow, cartdetail);

                _viewModel.CartDetails.Add(cartdetail);

            }

            _service.AddToCart(_viewModel);
        }
    }
}
