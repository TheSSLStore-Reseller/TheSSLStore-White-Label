using System.Web.Mvc;
using System.Web.Security;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers.Base;

namespace WhiteBrandSite.Controllers
{
    public class ProductsController : WBController<StaticPageViewModel, IRepository<Contract>, IStaticPageViewModelService>
    {
        #region Properties
        int ContractID = 0;
        private SSLStoreUser _loginuser = null;
        public SSLStoreUser loginuser
        {
            get
            {
                if (_loginuser == null)
                {
                    _loginuser = ((SSLStoreUser)Membership.GetUser());
                }
                return _loginuser;
            }
        }
        public ProductsController()
        {
            ViewBag.UserName = loginuser != null && loginuser.Details != null ? loginuser.Details.FirstName + " " + loginuser.Details.LastName : string.Empty;
        }



        #endregion

        [Route("symantac/securesiteprowithev", Name = "SecureSiteProwithEV_stc")]
        public ActionResult SecureSiteProwithEV()
        {
            SetPricing("/symantac/"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantac/"); return View(_viewModel);
        }

        #region Custom Methods
        private void SetPricing(string slug, string code = "")
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

            _viewModel.Items = _service.GetProductPricingByDetailSlug(Site.ID, ContractID, slug);
        }
        #endregion

        #region RapidSSl Products

        [Route("rapidssl/rapidssl-certificate", Name = "product_rapidssl")]
        public ActionResult RapidSSL()
        {
            SetPricing("/rapidssl/rapidssl-certificate");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/rapidssl/rapidssl-certificate");            
            return View(_viewModel);
        }

        [Route("rapidssl/rapidssl-wildcard-certificate", Name = "product_rapidsslwildcard")]
        public ActionResult RapidSSLWildcard()
        {
            SetPricing("/rapidssl/rapidssl-wildcard-certificate");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/rapidssl/rapidssl-wildcard-certificate");
            return View(_viewModel);
        }

        [Route("rapidssl/free-ssl-certificates", Name = "product_freessl")]
        public ActionResult FreeSSL()
        {
            SetPricing("/rapidssl/free-ssl-certificates");
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/rapidssl/free-ssl-certificates");
            return View(_viewModel);
        }

        #endregion

        #region Symantec Products

        [Route("symantec/wildcard-ssl", Name = "product_symantecwildcard")]
        public ActionResult SymantecSecureSiteWildCard()
        {
            SetPricing("/symantec/wildcard-ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/wildcard-ssl"); return View(_viewModel);
        }

        [Route("symantec/secure-site-pro-with-ev", Name = "product_securesiteproev")]
        public ActionResult SymantecSecureSiteProwithEV()
        {
            SetPricing("/symantec/secure-site-pro-with-ev"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-pro-with-ev"); return View(_viewModel);
        }

        [Route("symantec/secure-site-with-ev", Name = "product_securesiteev")]
        public ActionResult SymantecSecureSitewithEV()
        {
            SetPricing("/symantec/secure-site-with-ev"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-with-ev"); return View(_viewModel);
        }

        [Route("symantec/secure-site-pro-ssl", Name = "product_securesitepro")]
        public ActionResult SymantecSecureSitePro()
        {
            SetPricing("/symantec/secure-site-pro-ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-pro-ssl"); return View(_viewModel);
        }

        [Route("symantec/secure-site-ssl", Name = "product_securesite")]
        public ActionResult SymantecSecureSite()
        {
            SetPricing("/symantec/secure-site-ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-ssl"); return View(_viewModel);
        }

        [Route("symantec/code-signing", Name = "product_verisigncsc")]
        public ActionResult SymantecCodeSign()
        {
            SetPricing("/symantec/code-signing"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/code-signing"); return View(_viewModel);
        }

        [Route("symantec/code-signing-individual", Name = "product_verisigncscind")]
        public ActionResult SymantecCodeSignIndividual()
        {
            SetPricing("/symantec/code-signing-individual"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/code-signing-individual"); return View(_viewModel);
        }

        [Route("symantec/safe-site", Name = "product_trustsealorg")]
        public ActionResult SymantecSafeSite()
        {
            SetPricing("/symantec/safe-site"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/safe-site"); return View(_viewModel);
        }

        [Route("symantec/secure-site-pro-wildcard", Name = "product_securesiteprowildcard")]
        public ActionResult SymantecSecureSiteProWildcard()
        {
            //secure-site-pro-multi-domain-wildcard
            SetPricing("/symantec/secure-site-pro-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-pro-wildcard"); return View(_viewModel);
        }

        [Route("symantec/secure-site-pro-multi-domain-wildcard", Name = "product_securesitepromdwc")]
        public ActionResult securesitepromdwc()
        {
            //secure-site-pro-multi-domain-wildcard
            SetPricing("/symantec/secure-site-pro-multi-domain-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-pro-multi-domain-wildcard"); return View(_viewModel);
        }

        [Route("symantec/secure-site-pro-sha-1-private", Name = "product_securesiteprosha1")]
        public ActionResult securesitepro_sha1()
        {
            //secure-site-pro-multi-domain-wildcard
            SetPricing("/symantec/secure-site-pro-sha-1-private"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-pro-sha-1-private"); return View(_viewModel);
        }

         [Route("symantec/secure-site-multi-domain-wildcard", Name = "product_securemdwc")]
        public ActionResult securesitemdwc()
        {
            SetPricing("/symantec/secure-site-multi-domain-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/symantec/secure-site-multi-domain-wildcard"); return View(_viewModel);
        }
        
        #endregion

        #region GeoTrust Products

        [Route("geotrust/quickssl-premium", Name = "product_quicksslpremium")]
        public ActionResult GeotrustQuickSSLPremium()
        {
            SetPricing("/geotrust/quickssl-premium"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/quickssl-premium"); return View(_viewModel);
        }

        [Route("geotrust/quickssl-premium-san", Name = "product_quicksslpremiumsan")]
        public ActionResult GeotrustQuickSSLPremiumSan()
        {
            SetPricing("/geotrust/quickssl-premium-san"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/quickssl-premium-san"); return View(_viewModel);
        }

        [Route("geotrust/true-businessid", Name = "product_truebizid")]
        public ActionResult GeotrustTrueBusinessID()
        {
            SetPricing("/geotrust/true-businessid"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid"); return View(_viewModel);
        }

        [Route("geotrust/true-businessid-wildcard", Name = "product_truebusinessidwildcard")]
        public ActionResult GeoTrustTrueBusinessIDWildcard()
        {
            SetPricing("/geotrust/true-businessid-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid-wildcard"); return View(_viewModel);
        }

        [Route("geotrust/true-businessid-ev", Name = "product_truebizidev")]
        public ActionResult GeoTrustTrueBusinessIDEV()
        {
            SetPricing("/geotrust/true-businessid-ev"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid-ev"); return View(_viewModel);
        }


        [Route("geotrust/true-businessid-multi-domain", Name = "product_truebizidmd")]
        public ActionResult GeoTrustTrueBusinessIDMultiDomain()
        {
            SetPricing("/geotrust/true-businessid-multi-domain"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid-multi-domain"); return View(_viewModel);
        }

        [Route("geotrust/true-businessid-ev-multi-domain", Name = "product_truebusinessidevmd")]
        public ActionResult GeoTrustTrueBusinessIDEVMultidomain()
        {
            SetPricing("/geotrust/true-businessid-ev-multi-domain"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid-ev-multi-domain"); return View(_viewModel);
        }

        [Route("geotrust/web-site-anti-malware-scan", Name = "product_malwarescan")]
        public ActionResult GeoTrustAntiMalwareScan()
        {
            SetPricing("/geotrust/web-site-anti-malware-scan"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/web-site-anti-malware-scan"); return View(_viewModel);
        }

        [Route("geotrust/basic-web-site-anti-malware-scan", Name = "malwarebasic")]
        public ActionResult GeoTrustBasicAntiMalwareScan()
        {
            SetPricing("/geotrust/basic-web-site-anti-malware-scan"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/basic-web-site-anti-malware-scan"); return View(_viewModel);
        }

        [Route("geotrust/quickssl-premium-wildcard", Name = "product_quicksslpremiumwildcard")]
        public ActionResult GeoTrustQuickSSLPremiumWildcard()
        {
            SetPricing("/geotrust/quickssl-premium-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/quickssl-premium-wildcard"); return View(_viewModel);
        }

        [Route("geotrust/true-businessid-with-multi-domain-wildcard", Name = "product_truebizmdwc")]
        public ActionResult GeoTrustQuickTrueBizMDWC() 
        {
            SetPricing("/geotrust/true-businessid-with-multi-domain-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/geotrust/true-businessid-with-multi-domain-wildcard"); return View(_viewModel);
        }
        #endregion

        #region Thawte Products

        [Route("thawte/ssl123", Name = "product_ssl123")]
        public ActionResult ThawteSSL123()
        {
            SetPricing("/thawte/ssl123"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/ssl123"); return View(_viewModel);
        }


        [Route("thawte/web-server", Name = "product_sslwebserver")]
        public ActionResult ThawteSSLWebServer()
        {
            SetPricing("/thawte/web-server"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/web-server"); return View(_viewModel);
        }

        [Route("thawte/web-server-ev", Name = "product_sslwebserverev")]
        public ActionResult ThawteSSLWebServerEV()
        {
            SetPricing("/thawte/web-server-ev"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/web-server-ev"); return View(_viewModel);
        }

        [Route("thawte/wildcard-ssl", Name = "product_sslwebserverwildcard")]
        public ActionResult ThawteSSLWebServerWildcard()
        {
            SetPricing("/thawte/wildcard-ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/wildcard-ssl"); return View(_viewModel);
        }

        [Route("thawte/code-signing", Name = "product_thawtecsc")]
        public ActionResult ThawteCodeSign()
        {
            SetPricing("/thawte/code-signing"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/code-signing"); return View(_viewModel);
        }

        [Route("thawte/code-signing-individual", Name = "product_thawtecscind")]
        public ActionResult ThawteCodeSignIndividual()
        {
            SetPricing("/thawte/code-signing-individual"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/code-signing-individual"); return View(_viewModel);
        }
        [Route("thawte/ssl123-wildcard", Name = "product_ssl123wildcard")]
        public ActionResult ThawteSSL123Wildcard()
        {
            SetPricing("/thawte/ssl123-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/ssl123-wildcard"); return View(_viewModel);
        }

        [Route("thawte/ssl-webserver-multi-domain-wildcard", Name = "product_sslwbmdwc")]
        public ActionResult ThawteWebserver_multi_domain_wildcard()
        {
            SetPricing("/thawte/ssl-webserver-multi-domain-wildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/thawte/ssl-webserver-multi-domain-wildcard"); return View(_viewModel);
        }

        #endregion

        #region Comodo Products
        [Route("comodo/codesigning", Name = "product_comodocodesigning")]
        public ActionResult ComodoCodeSigning() { SetPricing("/comodo/codesigning"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/codesigning"); return View(_viewModel); }

        [Route("comodo/domainvalidateduccssl", Name = "product_domainvalidateduccssl")]
        public ActionResult DomainValidatedUCCSSL() { SetPricing("/comodo/domainvalidateduccssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/domainvalidateduccssl"); return View(_viewModel); }


        [Route("comodo/elitessl", Name = "product_comodoelitessl")]
        public ActionResult ComodoEliteSSL() { SetPricing("/comodo/elitessl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/elitessl"); return View(_viewModel); }

        [Route("comodo/essentialssl", Name = "product_comodoessentialssl")]
        public ActionResult ComodoEssentialSSL() { SetPricing("/comodo/essentialssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/essentialssl"); return View(_viewModel); }

        [Route("comodo/essentialsslwildcard", Name = "product_comodoessentialsslwildcard")]
        public ActionResult ComodoEssentialSSLWildcard() { SetPricing("/comodo/essentialsslwildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/essentialsslwildcard"); return View(_viewModel); }

        [Route("comodo/evmultidomainssl", Name = "product_comodoevmultidomainssl")]
        public ActionResult ComodoEVMultiDomainSSL() { SetPricing("/comodo/evmultidomainssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/evmultidomainssl"); return View(_viewModel); }

        [Route("comodo/evssl", Name = "product_comodoevssl")]
        public ActionResult ComodoEVSSL() { SetPricing("/comodo/evssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/evssl"); return View(_viewModel); }

        [Route("comodo/hackerguardian-pci-scan", Name = "product_comodopciscan")]
        public ActionResult ComodoHackerGuardianPCIScan() { SetPricing("/comodo/hackerguardian-pci-scan"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/hackerguardian-pci-scan"); return View(_viewModel); }

        [Route("comodo/hackerproof-trustmark", Name = "product_hackerproofdailyvulnerabilityscan")]
        public ActionResult ComodoHackerProofDailyVulnerabilityScan() { SetPricing("/comodo/hackerproof-trustmark"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/hackerproof-trustmark"); return View(_viewModel); }

        [Route("comodo/instantssl", Name = "product_comodoinstantssl")]
        public ActionResult ComodoInstantSSL() { SetPricing("/comodo/instantssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/instantssl"); return View(_viewModel); }

        [Route("comodo/premiumssl", Name = "product_comodopremiumssl")]
        public ActionResult ComodoPremiumSSL() { SetPricing("/comodo/premiumssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/premiumssl"); return View(_viewModel); }

        [Route("comodo/instantsslpro", Name = "product_comodoinstantsslpro")]
        public ActionResult ComodoInstantSSLPro() { SetPricing("/comodo/instantsslpro"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/instantsslpro"); return View(_viewModel); }

        [Route("comodo/multidomainssl", Name = "product_comodomultidomainssl")]
        public ActionResult ComodoMultiDomainSSL() { SetPricing("/comodo/multidomainssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/multidomainssl"); return View(_viewModel); }

        [Route("comodo/multidomainwildcardssl", Name = "product_comodomultidomainwildcardssl")]
        public ActionResult ComodoMultiDomainWildcardSSL() { SetPricing("/comodo/multidomainwildcardssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/multidomainwildcardssl"); return View(_viewModel); }

        [Route("comodo/pciscanning-enterprise-edition", Name = "product_pciscanenterprise")]
        public ActionResult ComodoPCIScanningEnterpriseEdition() { SetPricing("/comodo/pciscanning-enterprise-edition"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/pciscanning-enterprise-edition"); return View(_viewModel); }

        [Route("comodo/positivesslmultidomain", Name = "product_positivesslmultidomain")]
        public ActionResult PositiveSSLMultiDomain() { SetPricing("/comodo/positivesslmultidomain"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/positivesslmultidomain"); return View(_viewModel); }

        [Route("comodo/positivesslmultidomainwildcard", Name = "product_positivesslmultiwc")]
        public ActionResult PositiveSSLMultiDomainWildcard() { SetPricing("/comodo/positivesslmultidomainwildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/positivesslmultidomainwildcard"); return View(_viewModel); }

        [Route("comodo/positivessl", Name = "product_comodopositivessl")]
        public ActionResult ComodoPositiveSSL() { SetPricing("/comodo/positivessl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/positivessl"); return View(_viewModel); }

        [Route("comodo/positivesslwildcard", Name = "product_comodopositivesslwildcard")]
        public ActionResult ComodoPositiveSSLWildcard() { SetPricing("/comodo/positivesslwildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/positivesslwildcard"); return View(_viewModel); }

        [Route("comodo/premiumsslwildcard", Name = "product_comodopremiumsslwildcard")]
        public ActionResult ComodoPremiumSSLWildcard() { SetPricing("/comodo/premiumsslwildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/premiumsslwildcard"); return View(_viewModel); }

        [Route("comodo/ssl", Name = "product_comodossl")]
        public ActionResult ComodoSSL() { SetPricing("/comodo/ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/ssl"); return View(_viewModel); }

        [Route("comodo/ucwildcard", Name = "product_ucwildcard")]
        public ActionResult ComodoUnifiedCommunicationsWildcard() { SetPricing("/comodo/ucwildcard"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/ucwildcard"); return View(_viewModel); }

        [Route("comodo/unifiedcommunications", Name = "product_unifiedcommunications")]
        public ActionResult ComodoUnifiedCommunications() { SetPricing("/comodo/unifiedcommunications"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/unifiedcommunications"); return View(_viewModel); }

        [Route("comodo/wildcardssl", Name = "product_wildcardssl")]
        public ActionResult ComodoWildcardSSL() { SetPricing("/comodo/wildcardssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/wildcardssl"); return View(_viewModel); }

        [Route("comodo/web-inspector-starter", Name = "product_webinspectorstarter")]
        public ActionResult webinspectorstarter() { SetPricing("/comodo/web-inspector-starter"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/web-inspector-starter"); return View(_viewModel); }


        [Route("comodo/web-inspector-plus", Name = "product_webinspectorplus")]
        public ActionResult webinspectorplus() { SetPricing("/comodo/web-inspector-plus"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/web-inspector-plus"); return View(_viewModel); }


        [Route("comodo/web-inspector-premium", Name = "product_webinspectorpremium")]
        public ActionResult webinspectorpremium() { SetPricing("/comodo/web-inspector-premium"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/web-inspector-premium"); return View(_viewModel); }

        [Route("comodo/web-inspector-enterprise", Name = "product_webinspectorenterprise")]
        public ActionResult webinspectorenterprise() { SetPricing("/comodo/web-inspector-enterprise"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/web-inspector-enterprise"); return View(_viewModel); }

        [Route("comodo/personal-authentication-basic-certificate", Name = "product_pacbasic")]
        public ActionResult pacbasic() { SetPricing("/comodo/personal-authentication-basic-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/personal-authentication-basic-certificate"); return View(_viewModel); }


        [Route("comodo/personal-authentication-pro-certificate", Name = "product_pacpro")]
        public ActionResult pacpro() { SetPricing("/comodo/personal-authentication-pro-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/personal-authentication-pro-certificate"); return View(_viewModel); }


        [Route("comodo/personal-authentication-enterprise-certificate", Name = "product_pacenterprise")]
        public ActionResult pacenterprise() { SetPricing("/comodo/personal-authentication-enterprise-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/comodo/personal-authentication-enterprise-certificate"); return View(_viewModel); }

       
        #endregion

        #region "Certum"
        [Route("certum/basic-id-certificate", Name = "product_CertumBasicID")]
        public ActionResult CertumBasicID() { SetPricing("/certum/basic-id-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/basic-id-certificate"); return View(_viewModel); }

        [Route("certum/commercial-ssl", Name = "product_CertumCommercialSSL")]
        public ActionResult CertumCommercialSSL() { SetPricing("/certum/commercial-ssl"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/commercial-ssl"); return View(_viewModel); }

        [Route("certum/commercial-ssl-wildcard-certificate", Name = "product_CertumCommercialSSLWildCard")]
        public ActionResult CertumCommercialSSLWildCard() { SetPricing("/certum/commercial-ssl-wildcard-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/commercial-ssl-wildcard-certificate"); return View(_viewModel); }

        [Route("certum/enterprise-id-certificate", Name = "product_CertumEnterpriseID")]
        public ActionResult CertumEnterpriseID() { SetPricing("/certum/enterprise-id-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/enterprise-id-certificate"); return View(_viewModel); }

        [Route("certum/professional-id-certificate", Name = "product_CertumProfessionalID")]
        public ActionResult CertumProfessionalID() { SetPricing("/certum/enterprise-id-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/enterprise-id-certificate"); return View(_viewModel); }

        [Route("certum/trusted-ssl-certificate", Name = "product_CertumTrustedSSL")]
        public ActionResult CertumTrustedSSL() { SetPricing("/certum/trusted-ssl-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/trusted-ssl-certificate"); return View(_viewModel); }

        [Route("certum/trusted-ssl-wildcard-certificate", Name = "product_CertumTrustedSSLWildcard")]
        public ActionResult CertumTrustedSSLWildcard() { SetPricing("/certum/trusted-ssl-wildcard-certificate"); _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/certum/trusted-ssl-wildcard-certificate"); return View(_viewModel); }

        #endregion
    }
}