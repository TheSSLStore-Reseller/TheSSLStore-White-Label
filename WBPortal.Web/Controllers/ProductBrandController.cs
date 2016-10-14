using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Caching;

namespace WBSSLStore.Web.Controllers
{
    public class ProductBrandController : WBController<StaticPageViewModel, IRepository<Contract>, IStaticPageViewModelService>
    {
        #region Properties
        private SSLStoreUser _loginuser = null;
        public SSLStoreUser loginuser
        {
            get
            {
                if (_loginuser == null)
                    _loginuser = ((SSLStoreUser)Membership.GetUser());
                return _loginuser;
            }

        }
        int ContractID = 0;

        #endregion

        #region CustomMethod
        private void SetPricing(int BrandID, string code = "")
        {
            int UserID = 0;
            if (User.Identity.IsAuthenticated)
            {
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    if (loginuser != null && loginuser.Details != null)
                    {
                        UserID = loginuser.Details.ID;
                    }
                }
            }

            if (ContractID.Equals(0))
                ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            _viewModel.Items = _service.GetProductPricing(Site.ID, 0, ContractID, BrandID, code);
        }
        #endregion

        #region All Brands
        [Route("ssl/brands", Name = "AllBrand_us")]
        public ActionResult Index()
        {
            var currentsite = SiteCacher.CurrentSite;
            using (CurrentSiteSettings settings = new CurrentSiteSettings(currentsite))
            {
                _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            }

            int UserID = 0;

            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    if (loginuser != null && loginuser.Details != null)
                    {

                        UserID = loginuser.Details.ID;
                    }
                }
            }

            if (ContractID.Equals(0))
                ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            _viewModel.Items = _service.GetAllProductPricing(currentsite.ID, ContractID);
            return View(_viewModel);
        }
        #endregion

        #region Brands
        [Route("brands/symantec", Name = "symantecBrand_us")]
        public ActionResult Symantec()
        {
            
            SetPricing((int)ProductBrands.Symantec, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }
        [Route("brands/thawte", Name = "thawteBrand_us")]
        public ActionResult Thawte()
        {
            SetPricing((int)ProductBrands.Thawte, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }
        [Route("brands/Comodo", Name = "ComodoBrand_us")]
        public ActionResult Comodo()
        {
            SetPricing((int)ProductBrands.Comodo, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/RapidSSL", Name = "RapidSSLBrand_us")]
        public ActionResult RapidSSL()
        {
            SetPricing((int)ProductBrands.RapidSSL, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/GeoTrust", Name = "GeoTrustBrand_us")]
        public ActionResult GeoTrust()
        {
            SetPricing((int)ProductBrands.GeoTrust, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/Certum", Name = "CertumBrand_us")]
        public ActionResult Certum()
        {
            SetPricing((int)ProductBrands.Certum, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }
        #endregion

        #region ProductTypes
        [Route("ssl/domain-validated", Name = "dvssl_us")]
        public ActionResult dvSSL()
        {
            SetPricing(0, "rapidssl,freessl,ssl123,quicksslpremium,quicksslpremiummd,287,301");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/organisation-validated", Name = "ovssl_us")]
        public ActionResult ovSSL()
        {
            SetPricing(0, "securesite,securesitepro,sslwebserver,truebizid,24,34,7");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/extended-validated", Name = "evssl_us")]
        public ActionResult evSSL()
        {
            SetPricing(0, "truebizidmd,truebusinessidevmd,truebusinessidev,sslwebserverev,337,335,410,securesiteproev,securesiteev");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }


        [Route("ssl/wildcard", Name = "wildcardssl_us")]
        public ActionResult WildcardSSL()
        {
            SetPricing(0, "truebusinessidwildcard,sslwebserverwildcard,rapidsslwildcard,343,35,289");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/ucc-san", Name = "uccsanSsl_us")]
        public ActionResult UccsanSSL()
        {
            SetPricing(0, "361");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/code-signing", Name = "csssl_us")]
        public ActionResult csSSL()
        {
            SetPricing(0, "verisigncsc,thawtecsc,8");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }
        [Route("ssl/anti-malware", Name = "antimalware_us")]
        public ActionResult antimalwareSSL()
        {
            SetPricing(0, "malwarescan");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        #endregion

       
    }
}
