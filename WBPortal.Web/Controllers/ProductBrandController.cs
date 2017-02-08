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
                ContractID = WBHelper.GetCurrentContractID(UserID, Site.ID);

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
        [Route("brands/comodo", Name = "ComodoBrand_us")]
        public ActionResult Comodo()
        {
            SetPricing((int)ProductBrands.Comodo, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/rapidSSL", Name = "RapidSSLBrand_us")]
        public ActionResult RapidSSL()
        {
            SetPricing((int)ProductBrands.RapidSSL, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/geotrust", Name = "GeoTrustBrand_us")]
        public ActionResult GeoTrust()
        {
            SetPricing((int)ProductBrands.GeoTrust, "");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("brands/certum", Name = "CertumBrand_us")]
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
            SetPricing(0, "quicksslpremium,ssl123,rapidssl,rapidsslwildcard,freessl,essentialssl,essentialwildcard,comodossl,positivessl,positivesslwildcard,comodowildcard,ucommercialssl,ucommercialwildcard,quicksslpremiumwildcard,ssl123wildcard");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/organisation-validated", Name = "ovssl_us")]
        public ActionResult ovSSL()
        {
            SetPricing(0, "securesitepro,securesite,truebizid,sslwebserver,instantssl,instantsslpro,comodopremiumssl,elitessl,utrustedssl,securesitemdwc,securesitepromdwc,truebizidmdwc,sslwebservermdwc,securesitepro_SHA1");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/extended-validated", Name = "evssl_us")]
        public ActionResult evSSL()
        {

            SetPricing(0, "securesiteproev,securesiteev,truebusinessidev,truebusinessidevmd,sslwebserverev,comodoevssl,comodoevmdc");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }


        [Route("ssl/wildcard", Name = "wildcardssl_us")]
        public ActionResult WildcardSSL()
        {
            SetPricing(0, "truebusinessidwildcard,sslwebserverwildcard,rapidsslwildcard,essentialwildcard,positivesslwildcard,comodowildcard,comodopremiumwildcard,securesitewildcard,ucommercialwildcard,utrustedwildcard,positivemdcwildcard,comodomdcwildcard,comodouccwildcard,quicksslpremiumwildcard,ssl123wildcard,securesiteprowildcard,basicDVWildcard,securesitemdwc,securesitepromdwc,truebizidmdwc,sslwebservermdwc");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/ucc-san", Name = "uccsanSsl_us")]
        public ActionResult UccsanSSL()
        {
            SetPricing(0, "securesiteproev,securesiteev,securesitepro,securesite,truebizidmd,truebusinessidevmd,sslwebserverev,sslwebserver,comodomdc,comodoevmdc,comodoucc,comododvucc,positivemdcssl,quicksslpremiummd,positivemdcwildcard,comodomdcwildcard,comodouccwildcard,securesitemdwc,securesitepromdwc,truebizidmdwc,sslwebservermdwc,securesitepro_SHA1");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        [Route("ssl/code-signing", Name = "csssl_us")]
        public ActionResult csSSL()
        {
            SetPricing(0, "verisigncsc,thawtecsc,comodocsc");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }
        [Route("ssl/anti-malware", Name = "antimalware_us")]
        public ActionResult antimalwareSSL()
        {
            SetPricing(0, "trustsealorg,malwarescan,hgpcicontrolscan,hackerprooftm,comodopciscan,webinsbasic,webinsplus,webinspremium,webinsnterprise,ubasicid,uenterpriseid,uprofessionalid,pacbasic,pacpro,pacenterprise,malwarebasic");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            return View(_viewModel);
        }

        #endregion

       
    }
}
