using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Gateway;
using WBSSLStore.Data.Repository;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Gateway.PayPal;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Util;
using WBSSLStore.Web.Helpers.Caching;
using System.Web.Security;
using System.Security.Principal;


namespace WBSSLStore.Web.Areas.Checkout.Controllers
{

    [HandleError]

    public class PaymentController : WBController<CheckOutViewModel, IRepository<ShoppingCartDetail>, ICheckoutService>
    {
        private User _user;
        private new Site Site;
        private WBSSLStore.Domain.CurrentSiteSettings currentsitesettings
        {
            get;
            set;
        }
        private User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated && _user == null)
                {
                    SSLStoreUser user1 = ((SSLStoreUser)System.Web.Security.Membership.GetUser());

                    if (user1 != null && user1.Details != null)
                        _user = user1.Details;
                    else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                    {
                        user1 = ((SSLStoreUser)System.Web.Security.Membership.GetUser(User.Identity.Name));

                        if (user1 != null && user1.Details != null)
                            _user = user1.Details;
                    }

                }

                return _user;
            }

        }

        private string Host = string.Empty;

        private string SiteAlias
        {
            get
            {
                if (string.IsNullOrEmpty(Host))
                {

                    if (!currentsitesettings.CheckSite(Site.ID))
                    {
                        Site = GetSite(Site.ID);
                    }

                    Host = (string.IsNullOrEmpty(currentsitesettings.CurrentSite.Alias) ? currentsitesettings.CurrentSite.CName : currentsitesettings.CurrentSite.Alias);

                    if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
                    {
                        if (currentsitesettings.IsRunWithWWW && !Host.Contains("www."))
                        {
                            Host = Host.Replace(Host, "www." + Host);
                        }
                        else if (!currentsitesettings.IsRunWithWWW && Host.Contains("www."))
                            Host = Host.Replace("www.", "");
                    }
                }

                return (currentsitesettings.IsSiteRunWithHTTPS ? "https://" : "http://") + Host;
            }
        }
        private void SetSiteIDInSession()
        {
            Session["CurrentSiteID"] = Site != null ? Site.ID : 0;
        }
        private void SetDefaultData(int id)
        {
            var cartdeatil = _repository.Find(x => x.ShoppingCartID == id).EagerLoad(c => c.ShoppingCart, c => c.Product, c => c.ProductPricing).ToList();

            if (cartdeatil != null && cartdeatil.Count > 0)
            {
                _viewModel.SiteID = cartdeatil[0].ShoppingCart.SiteID;
                Site = GetSite(_viewModel.SiteID);

                currentsitesettings = new CurrentSiteSettings(Site);
                SetSiteIDInSession();
            }

            ViewBag.CartDetails = cartdeatil;
            ViewBag.Country = CountryList;


            ViewBag.VATCountryCode = currentsitesettings.IsVatApplicable ? CountryList.Find(c => c.ID == currentsitesettings.VatCountry).ISOName : string.Empty;

            _viewModel.OrderAmount = cartdeatil.Sum(p => p.Price);
            _viewModel.PromoDiscount = cartdeatil.Sum(p => p.PromoDiscount);
        }
        
        public ActionResult Index(int id)
        {

            SetDefaultData(id);


            _viewModel.ShoppingCartID = id;
            _viewModel.VatCountry = currentsitesettings.VatCountry;
            _viewModel.VATNumber = String.Empty;
            _viewModel.VatPercent = currentsitesettings.VATTax;
            _viewModel.ISVatApplicable = currentsitesettings.IsVatApplicable;
            _viewModel.Tax = 0;

            if (!User.Identity.IsAuthenticated)
            {
                _viewModel.user = new Domain.User();
                _viewModel.user.Address = new Address();
                _viewModel.IsNewuser = true;
                _viewModel.AvailableCredit = 0;
                _viewModel.user.Email = Request.QueryString["eid"];
            }
            else
            {
                _viewModel.user = CurrentUser;
                _viewModel.IsNewuser = false;
                _viewModel.AvailableCredit = _service.GetCreditAmount(_viewModel.user.ID, _viewModel.user.SiteID);
            }

            _viewModel.PayableAmount = _viewModel.OrderAmount - _viewModel.PromoDiscount - _viewModel.AvailableCredit;

            _viewModel.PayableAmount = _viewModel.PayableAmount <= 0 ? 0 : _viewModel.PayableAmount;


            //Get Setting For Payment
            var PGInstance = _service.GetPGInstances(Site.ID);

            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
            if (PG != null)
            {
                _viewModel.ISCC = true;
                _viewModel.paymentmode = Domain.PaymentMode.CC;
            }
            else
                _viewModel.ISCC = false;

            PG = null;
            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
            if (PG != null)
            {
                _viewModel.paymentmode = Domain.PaymentMode.PAYPAL;
                _viewModel.IsPayPal = true;
            }
            else
                _viewModel.IsPayPal = false;

            PG = null;
            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
            if (PG != null)
            {
                _viewModel.paymentmode = Domain.PaymentMode.MONEYBOOKERS;
                _viewModel.IsMoneybookers = true;
            }
            else
                _viewModel.IsMoneybookers = false;

            if (_viewModel.ISCC && _viewModel.IsPayPal && _viewModel.IsMoneybookers)
                _viewModel.paymentmode = Domain.PaymentMode.CC;
            PG = null;

            if (currentsitesettings != null)
                currentsitesettings.Dispose();

            return View("index", _viewModel);
        }

        [HttpPost]
        public ActionResult pay(CheckOutViewModel collection)
        {
            bool result = true;
            try
            {
                // TODO: Add insert logic here

                if (collection != null)
                {
                    _viewModel = collection;
                    Site = GetSite(_viewModel.SiteID);

                    currentsitesettings = new CurrentSiteSettings(Site);
                    //SetSiteIDInSession();
                    if (!User.Identity.IsAuthenticated)
                    {
                        _viewModel.user.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                        _viewModel.user.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash(_viewModel.user.FirstName + _viewModel.SiteID, _viewModel.user.PasswordSalt);
                        _viewModel.user.ConfirmPassword = _viewModel.user.PasswordHash;

                        User user = _viewModel.user;

                        int resultid = _service.AddUserandUpdateCart(user, _viewModel.ShoppingCartID, Site.ID, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail);

                        if (resultid.Equals(1))
                        {
                            //Set Auhtentic ticket in Member Ship.
                            System.Web.Security.Membership.ApplicationName = Site.ID.ToString();

                            if (System.Web.Security.Membership.ValidateUser(user.Email, user.FirstName + Site.ID))
                                System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);

                        }
                        else if (resultid.Equals(2))
                        {
                            _viewModel.Errormsg = WBSSLStore.Resources.ErrorMessage.Message.UserExist;
                            result = false;
                        }
                        else if (resultid.Equals(-1))
                        {
                            _viewModel.Errormsg = WBSSLStore.Resources.ErrorMessage.Message.ProcessError;
                            result = false;
                        }


                    }
                    else if (User.Identity.IsAuthenticated && _viewModel.user != null && _viewModel.user.ID.Equals(0))
                    {
                        _viewModel.user = CurrentUser;
                    }

                    try
                    {

                        // validate Amount
                        var cartdeatil = _repository.Find(x => x.ShoppingCartID == _viewModel.ShoppingCartID).EagerLoad(c => c.ShoppingCart, c => c.Product, c => c.ProductPricing).ToList();

                        _viewModel.OrderAmount = cartdeatil.Sum(p => p.Price);
                        _viewModel.PromoDiscount = cartdeatil.Sum(p => p.PromoDiscount);
                        _viewModel.AvailableCredit = _service.GetCreditAmount(_viewModel.user.ID, _viewModel.user.SiteID);
                        //Calculate VAT
                        if (currentsitesettings.IsVatApplicable)
                        {
                            if (!string.IsNullOrEmpty(_viewModel.VATNumber) || _viewModel.Tax > 0)
                            {
                                int vatpercent = currentsitesettings.VATTax;
                                if (vatpercent > 0)
                                {
                                    _viewModel.Tax = (((_viewModel.OrderAmount - _viewModel.PromoDiscount) * vatpercent) / 100);
                                }

                            }
                            else
                            {
                                _viewModel.Tax = 0;
                            }
                        }
                        else
                        {
                            _viewModel.Tax = 0;
                        }
                        //

                        _viewModel.PayableAmount = ((_viewModel.OrderAmount - _viewModel.PromoDiscount) + _viewModel.Tax) - _viewModel.AvailableCredit;
                        _viewModel.PayableAmount = _viewModel.PayableAmount <= 0 ? 0 : _viewModel.PayableAmount;
                        _viewModel.PayableAmount = Convert.ToDecimal(_viewModel.PayableAmount.ToString("0.00"));

                        // Set Country Name
                        if (_viewModel.user.Address.CountryID > 0)
                        {
                            Country c = CountryList.Find(x => x.ID == _viewModel.user.Address.CountryID & x.RecordStatusID == (int)RecordStatus.ACTIVE);
                            if (c != null)
                                _viewModel.BillingCountry = c.CountryName;
                            else
                                _viewModel.BillingCountry = "US";

                            c = null;
                        }
                        else
                            _viewModel.BillingCountry = "US";
                        //End
                        // Make Payment
                        if (string.IsNullOrEmpty(_viewModel.Errormsg) && result)
                        {
                            result = _service.PlaceOrder(_viewModel, Site, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail, currentsitesettings.InvoicePrefix);
                            ViewBag.Errormsg = _viewModel.Errormsg;
                        }
                        else
                            ViewBag.Errormsg = _viewModel.Errormsg;
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Errormsg = ex.Message;
                        _logger.LogException(ex);
                        result = false;
                    }


                }

                if (_viewModel.OrderID > 0 && string.IsNullOrEmpty(_viewModel.Errormsg) && result)
                {
                    //if (!currentsitesettings.USESSL)
                    //    System.Web.Security.FormsAuthentication.SignOut();

                    Host = (currentsitesettings.USESSL && currentsitesettings.IsSiteRunWithHTTPS ? "https://" : "http://") + (string.IsNullOrEmpty(currentsitesettings.CurrentSite.Alias) ? currentsitesettings.CurrentSite.CName : currentsitesettings.CurrentSite.Alias);

                    if (currentsitesettings.IsRunWithWWW && !Host.Contains("www."))
                    {
                        Host = Host.Replace(Host, "www." + Host);
                    }
                    else if (!currentsitesettings.IsRunWithWWW && Host.Contains("www."))
                        Host = Host.Replace("www.", "");

                    string url = Host + "/client/thankyou?token=" + HttpUtility.UrlEncode(WBSSLStore.CryptorEngine.Encrypt(_viewModel.OrderID + SettingConstants.Seprate + _viewModel.user.ID, true));
                    return Redirect(url);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Errormsg = ex.Message;
                _logger.LogException(ex);
                result = false;
                if (currentsitesettings != null)
                    currentsitesettings.Dispose();

            }

            SetDefaultData(_viewModel.ShoppingCartID);

            if (currentsitesettings != null)
                currentsitesettings.Dispose();
            return View("index", _viewModel);
        }

        [HttpPost]
        public ActionResult calTax(string vatnumber, int CountryID, decimal amount, int VatCountry, int VATTax)
        {

            if (CountryID.Equals(VatCountry))
            {
                int vatpercent = VATTax;
                if (vatpercent > 0)
                {
                    return Json(((amount * vatpercent) / 100));
                }

            }

            return Json(0);
        }



        public ActionResult paypalipn()
        {
            try
            {
                User U = null;
                if (U == null)
                {
                    int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                    var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                    User user = repo.FindByID(UserID);

                    if (user != null && user.ID > 0)
                    {
                        System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();

                        System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                        U = user;

                    }
                }

                Site = GetSite(U.SiteID);
                currentsitesettings = new CurrentSiteSettings(Site);
                SetSiteIDInSession();

                _logger.Log("Start PayPal IPN Call Success. Url :" + Request.Url.AbsoluteUri.ToString(), Logger.LogType.INFO);
                if (System.Web.HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString().ToLower().Equals("reissueaddfund"))
                {
                    _service.ProcessPayPalIPNRequestReIssue(Site, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail, currentsitesettings.InvoicePrefix);
                }
                else
                {
                    _service.ProcessPayPalIPNRequest(Site, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail, currentsitesettings.InvoicePrefix);
                }
                _logger.Log("end Paypal IPN Call Success. Url :" + Request.Url.AbsoluteUri.ToString(), Logger.LogType.INFO);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
            finally
            {
                currentsitesettings = null;

            }
            return null;
        }


        public ActionResult paypalwait()
        {
            User U = CurrentUser;


            if (U == null)
            {
                int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User user = repo.FindByID(UserID);

                if (user != null && user.ID > 0)
                {
                    System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();

                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                    U = user;

                }
            }

            Site = GetSite(U.SiteID);
            currentsitesettings = new CurrentSiteSettings(Site);
            _viewModel.SiteID = U.SiteID;
            SetSiteIDInSession();

            int OrderID = _service.ProcessPayPalWaitRequest(Site);
            _logger.Log("ProcessPayPalWaitRequest success OrderID=" + OrderID, Logger.LogType.INFO);

            if (OrderID.Equals(-99))
            {
                return View();
            }


            if (OrderID.Equals(0))
            {
                return RedirectToAction(WBSSLStore.Web.Util.WBSiteSettings.AppPath + "Error");
            }
            else
            {
                if (OrderID.Equals(-1))
                    return Redirect(SiteAlias + "/client/orders");
                else
                    return Redirect(SiteAlias + "/client/thankyou?token=" + HttpUtility.UrlEncode(WBSSLStore.CryptorEngine.Encrypt(OrderID + SettingConstants.Seprate + U.ID, true)));//RedirectToAction("index", "Thankyou", new { area = "client", id = OrderID });
            }

        }

        public ActionResult moneybookersipn()
        {
            try
            {
                User U = null;
                if (U == null)
                {
                    int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                    var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                    User user = repo.FindByID(UserID);

                    if (user != null && user.ID > 0)
                    {
                        System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();
                        System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                        U = user;
                    }
                }
                Site = GetSite(U.SiteID);
                currentsitesettings = new CurrentSiteSettings(Site);
                SetSiteIDInSession();

                _logger.Log("Start Moneybookers IPN Call Success. Url :" + Request.Url.AbsoluteUri.ToString(), Logger.LogType.INFO);
                if (System.Web.HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString().ToLower().Equals("reissueaddfund"))
                    _service.ProcessMBIPNRequestReIssue(Site, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail, currentsitesettings.InvoicePrefix);
                else
                    _service.ProcessMBIPNRequest(Site, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, currentsitesettings.SiteAdminEmail, currentsitesettings.InvoicePrefix);
                _logger.Log("End Moneybookers IPN Call Success. Url :" + Request.Url.AbsoluteUri.ToString(), Logger.LogType.INFO);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
            finally
            {
                currentsitesettings = null;

            }
            return null;
        }

        public ActionResult mbwait()
        {
            User U = CurrentUser;
            if (U == null)
            {
                int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User user = repo.FindByID(UserID);
                if (user != null && user.ID > 0)
                {
                    System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();
                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                    U = user;
                }
            }

            Site = GetSite(U.SiteID);
            currentsitesettings = new CurrentSiteSettings(Site);
            _viewModel.SiteID = U.SiteID;
            SetSiteIDInSession();

            int OrderID = _service.ProcessMBWaitRequest(Site);
            _logger.Log("ProcessMBWaitRequest success OrderID=" + OrderID, Logger.LogType.INFO);
            if (OrderID.Equals(-99))
                return View();

            if (OrderID.Equals(0))
                return RedirectToAction(SiteAlias + "/error");
            else
            {
                if (OrderID.Equals(-1))
                    return Redirect(SiteAlias + "/client/orders");//RedirectToAction("index", "Orders", new { area = "client" });
                else
                    return Redirect(SiteAlias + "/client/thankyou?token=" + HttpUtility.UrlEncode(WBSSLStore.CryptorEngine.Encrypt(OrderID + SettingConstants.Seprate + U.ID, true)));//RedirectToAction("index", "Thankyou", new { area = "client", id = OrderID });
            }
        }

        public ActionResult mbcancel()
        {
            User U = CurrentUser;
            if (U == null)
            {
                int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User user = repo.FindByID(UserID);
                if (user != null && user.ID > 0)
                {
                    System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();

                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                    U = user;
                }
            }

            Site = GetSite(U.SiteID);
            currentsitesettings = new CurrentSiteSettings(Site);
            _viewModel.SiteID = U.SiteID;
            return Redirect(SiteAlias + "/client/orders");
        }

        public ActionResult paypalcancel()
        {
            User U = CurrentUser;


            if (U == null)
            {
                int UserID = Convert.ToInt32(Request.QueryString[SettingConstants.QS_USERID]);
                var repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User user = repo.FindByID(UserID);

                if (user != null && user.ID > 0)
                {
                    System.Web.Security.Membership.ApplicationName = user.SiteID.ToString();

                    System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, false);
                    U = user;

                }
            }

            Site = GetSite(U.SiteID);
            currentsitesettings = new CurrentSiteSettings(Site);
            _viewModel.SiteID = U.SiteID;
            return Redirect(SiteAlias + "/client/orders");
        }
        [HttpGet]
        [Authorize]
        public ActionResult addfund()
        {
            //Get Setting For Payment
            SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
            if (loginuser != null && loginuser.Details != null)
            {
                User CurrentUser = loginuser.Details;
                Site = GetSite(CurrentUser.SiteID);
                SetSiteIDInSession();
                var PGInstance = _service.GetPGInstances(Site.ID);
                ViewBag.SiteID = Site.ID;
                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
                if (PG != null)
                    ViewBag.ISCC = true;
                else
                    ViewBag.ISCC = false;

                PG = null;
                PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
                if (PG != null)
                    ViewBag.IsPayPal = true;
                else
                    ViewBag.IsPayPal = false;
                PG = null;
                PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
                if (PG != null)
                    ViewBag.IsMoneybookers = true;
                else
                    ViewBag.IsMoneybookers = false;
            }
            return View();
        }
        [HttpPost]
        public ActionResult addfund(FormCollection collection)
        {

            string Message = string.Empty;
            int SiteID = Convert.ToInt32(collection["hndSiteID"]);
            int UserID = Convert.ToInt32(collection["hndUserID"]);
            Site = GetSite(SiteID);
            currentsitesettings = new Domain.CurrentSiteSettings(Site);
            SetSiteIDInSession();
            var _UserRepo = DependencyResolver.Current.GetService<IRepository<User>>();
            User _objUser = _UserRepo.FindByID(UserID);

            _viewModel.ISCC = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("cc") ? true : false;
            _viewModel.IsPayPal = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("paypal") ? true : false;
            _viewModel.IsMoneybookers = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("moneybookers") ? true : false;
            _viewModel.user = _objUser;
            _viewModel.PayableAmount = Convert.ToDecimal(collection["txtAmount"]);

            // Set Country Name
            if (_viewModel.user.Address.CountryID > 0 && _viewModel.user.Address.Country == null)
            {
                Country c = CountryList.Find(x => x.ID == _viewModel.user.Address.CountryID & x.RecordStatusID == (int)RecordStatus.ACTIVE);
                if (c != null)
                    _viewModel.BillingCountry = c.CountryName;
                else
                    _viewModel.BillingCountry = "US";
                c = null;
            }
            else if (_viewModel.user.Address.CountryID > 0 && _viewModel.user.Address.Country != null)
            {
                _viewModel.BillingCountry = _viewModel.user.Address.Country.CountryName;
            }
            else
                _viewModel.BillingCountry = "US";
            //End

            if (_viewModel.IsPayPal || _viewModel.ISCC || _viewModel.IsMoneybookers)
            {
                //if (_viewModel.IsPayPal && !string.IsNullOrEmpty(collection["txtPaypalID"]))-- 
                if (_viewModel.IsPayPal)
                {
                    _viewModel.PayPalID = !string.IsNullOrEmpty(Convert.ToString(collection["txtPaypalID"])) ? Convert.ToString(collection["txtPaypalID"]) : string.Empty;
                    _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.PAYPAL;
                }
                else if (_viewModel.IsMoneybookers)
                {
                    _viewModel.MoneybookersID = !string.IsNullOrEmpty(Convert.ToString(collection["txtMoneybookers"])) ? Convert.ToString(collection["txtMoneybookers"]) : string.Empty;
                    _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.MONEYBOOKERS;
                }
                else if (_viewModel.IsPayPal)
                {
                    ViewBag.ErrorMessage = WBSSLStore.Resources.ErrorMessage.Message.PaypalIdRequired;
                }
                else if (_viewModel.ISCC)
                {
                    _viewModel.CCName = Convert.ToString(collection["txtNameOnCard"]);
                    _viewModel.CCNumber = Convert.ToString(collection["txtCCNumber"]);
                    _viewModel.CVV = Convert.ToString(collection["txtCCV"]);
                    _viewModel.Month = Convert.ToInt32(collection["drpMonth"]);
                    _viewModel.Year = Convert.ToInt32(collection["drpYear"]);
                    _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.CC;
                }

                Message = _service.AddFund(_viewModel, Site);

                AddFundResponse objResponse = new AddFundResponse();
                if (string.IsNullOrEmpty(Message))
                {
                    Host = currentsitesettings.USESSL ? "https://" : "http://" + (string.IsNullOrEmpty(currentsitesettings.CurrentSite.Alias) ? currentsitesettings.CurrentSite.CName : currentsitesettings.CurrentSite.Alias);
                    if (currentsitesettings.IsRunWithWWW && !Host.Contains("www."))
                    {
                        Host = currentsitesettings.USESSL ? "https://" : "http://" + Host.Replace(Host, "www." + Host);
                    }
                    else if (!currentsitesettings.IsRunWithWWW && Host.Contains("www."))
                        Host = currentsitesettings.USESSL ? "https://" : "http://" + Host.Replace("www.", "");

                    return Redirect(Host + "/client/orders");
                }
                else
                {
                    ViewBag.ErrorMessage = Message;
                }
            }
            else
                ViewBag.ErrorMessage = WBSSLStore.Resources.ErrorMessage.Message.NoGatewaySelected;


            var PGInstance = _service.GetPGInstances(Site.ID);
            ViewBag.SiteID = Site.ID;
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
            if (PG != null)
                ViewBag.ISCC = true;
            else
                ViewBag.ISCC = false;

            PG = null;
            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
            if (PG != null)
                ViewBag.IsPayPal = true;
            else
                ViewBag.IsPayPal = false;
            PG = null;
            return View();
        }
    }
}
