using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace WBSSLStore.Domain
{
    public class Disposable : IDisposable
    {
        private bool isDisposed;

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
                Disposer();
            isDisposed = true;
        }

        protected virtual void Disposer()
        {
            //To be overriden in the child class
        }
    }
    /// <summary>
    /// Name all the Settings for the Site Here
    /// </summary>
    public static class SettingConstants
    {
        #region QueryStrings
        public const string QS_PRODUCTID = "pid";
        public const string QS_PRODUCT_PRICING_ID = "ppid";
        public const string QS_EXTRA_SAN = "NoofSan";
        public const string QS_QTY = "qty";
        public const string QS_USERID = "uid";
        public const string QS_TOKEN = "token";
        public const string QS_ORDERID = "oid";
        public const string QS_ORDER_DETAIL_ID = "odid";
        public const string QS_INVOICEID = "iid";
        public const string QS_PROMOCODE = "code";
        public const string QS_EMAIL = "email";
        public const string QS_PAGEID = "pid";
        public const string QS_USER_TYPE = "utype";
        public const string QS_MESSAGE = "msg";

        public const string QS_ISRECOMMENDED = "isRecommended";
        #endregion

        #region NumberOfMonths
        public enum NumberOfMonths
        {
            Month12 = 12,
            Month24 = 24,
            Month36 = 36,
            Month48 = 48,
            Month60 = 60

        }
        #endregion

        #region Payment
        /// <summary>
        /// Paypal Receipient ID
        /// </summary>
        public const string PAYPAL_RECEIPIENT_KEY = "PAYPAL_RECEIPIENT_KEY";
        public const string PAYPAL_SESSION_KEY = "SessionID";
        public const string PAYPAL_PAYMENTTYPE = "paymenttype";


        #endregion

        #region Display
        //
        /// <summary>
        /// Page Size for a given site
        /// </summary>
        public const string PAGESIZE_KEY = "PAGESIZE_KEY";
        #endregion

        #region Others
        /// <summary>
        /// TimeZone Difference in Minutes for the Site Owner.
        /// </summary>
        public const string TIMEZONE_DIFF_KEY = "TIMEZONE_DIFF_KEY";
        public static DateTime defaultDate = Convert.ToDateTime("1900-1-1");
        public const string Seprate = "|||";
        #endregion

        #region SiteSettings

        public const string CURRENT_LAYOUT_KEY = "layout";
        public const string CURRENT_COLORCODE_KEY = "colorcode";
        public const string CURRENT_SITEPNONE_KEY = "sitephone";
        public const string CURRENT_NEEDBANNER_KEY = "needbanner";
        public const string CURRENT_BANNERFILE_KEY = "bannerfile";
        public const string CURRENT_ALLOWEDBRAND_KEY = "allowedbrand";
        public const string CURRENT_VAT_APPLICABLE_KEY = "vatapplicable";
        public const string CURRENT_VAT_NUMBER_KEY = "vatnumber";
        public const string CURRENT_VAT_COUNTRY_KEY = "vatcountry";
        public const string CURRENT_USESSL_KEY = "usessl";
        public const string CURRENT_SITELOGO_KEY = "logo";
        public const string CURRENT_SITEMYCART_KEY = "mycart";
        public const string CURRENT_REFUND30DAYS_KEY = "refund30dayslogo";
        public const string CURRENT_ShowPayMentAcceptSection = "ShowPayMentAcceptSection";
        public const string CURRENT_ShowFreeToolsSection = "ShowFreeToolsSection";

        public const string CURRENT_SITELIVECHATE_KEY = "livechate";
        public const string CURRENT_SITEFACEBOOK_KEY = "facebook";
        public const string CURRENT_SITEGOOGLEPLUS_KEY = "googlepluse";
        public const string CURRENT_SITELINKEDIN_KEY = "linkedin";
        public const string CURRENT_SITETWITTER_KEY = "twitter";

        public const string CURRENT_SITENEWSLETTER_KEY = "newsletter";
        public const string CURRENT_SITELANGUAGE_KEY = "language";
        public const string CURRENT_SITECURRANCY_KEY = "currancy";
        public const string CURRENT_SITECULTURE_KEY = "culturekey";
        public const string CURRENT_SITETIMEZONE_KEY = "timezone";
        public const string CURRENT_SITEINVOICEPREFIX_KEY = "invoiceprefix";
        public const string CURRENT_SITEAPPROVERESELLER_KEY = "approvereseller";
        public const string CURRENT_SITEADMIN_EMAIL_KEY = "adminemail";
        public const string CURRENT_SITESUPPORT_EMAI_KEY = "supportemail";
        public const string CURRENT_SITEBILLING_EMAIL_KEY = "billingemail";
        public const string CURRENT_SITEUNDERMAINTANACE = "undermaintanance";
        public const string CURRENT_SITEPAGESIZE_KEY = "pagesize";
        public const string CURRENT_PUNCHLINE_KEY = "punchline";
        public const string CURRENT_SITEBLOG_URL = "blogurl";
        public const string CURRENT_ANIMATED_BANNER_KEY = "showanimatedbanner";
        public const string CURRENT_BANNER_HTML_KEY = "bannerhtml";

        public const string CURRENT_ROBOTS_FILE_VALUE = "robotsfilevalue";
        public const string GOOGLE_ANALYTIC_VALUE = "googleanalyticvalue";
        public const string CURRENT_IS_SITE_RUN_WITH_WWW = "issiterunwithwww";
        public const string CURRENT_TESTIMONIAL_APPLICABLE_KEY = "testimonials";

        public const string CURRNENT_CUSTOMELAYOUT_KEY = "customlayout";
        public const string CURRENT_SITE_SUBJECTFEIELD = "subjectfield";
        public const string CURRENT_SITE_TOEMAIL = "toemail";
        public const string CURRENT_SITE_RUNWITH_HTTPS = "issiterunwithhttps";
        public const string CURRENT_SITE_THANKYOUPAGE = "thankyoupage";
        public const string CURRENT_SITE_SIGNATURE = "signature";
        public const string CURRENT_SITE_PAYMENTNOTE = "paymentnote";

        public const string CURRENT_SITE_JQUERYUITHEME = "JqueryUITheme";

        public const string CUSTOM_EMAILID_RESELLER_CUSTOMERS_RENEWALS_NOTIFICATION = "customemail_reseller_customer_renewal";
        public const string CURRENT_SITE_SHOWBANNER_HOME_PAGE_ONLY = "showbannerhomepageonly";
        public const string CURRENT_SITE_CONFIGURATIONSTATUS = "SiteConfigurationStage";

        #endregion

        #region DefaultValues
        public const string DEFAULT_LANGUAGE_CODE = "en";
        public const string DEFAULT_CULTURE_KEY = "en-US";
        public const string DEFAULT_CURRANCY_CODE = "USD";
        public const int DEFAULT_PAGE_SIZE = 25;
        #endregion
    }



    /// <summary>
    /// Get All CurrentSiteSettings
    /// </summary>
    public class CurrentSiteSettings : Disposable
    {

        private string _currentcolor = "color1";
        private string _currentlayout = "layout1";
        private ICollection<WBSSLStore.Domain.SiteSettings> CurrentSettings;


        private Site _Site;
        public CurrentSiteSettings()
        {
        }
        public CurrentSiteSettings(Site _CurrentSite)
        {

            _Site = _CurrentSite;
            CurrentSettings = _Site.Settings;
        }
        protected override void Disposer()
        {
            if (_Site != null)
                _Site = null;

            if (CurrentSettings != null)
                CurrentSettings = null;


        }
        public bool CheckSite(int id)
        {

            return SiteID.Equals(id);
        }
        public void SetSettings(Site _CurrentSite)
        {


            _Site = _CurrentSite;
            CurrentSettings = _Site.Settings;
        }
        public Site CurrentSite
        {
            get
            {
                return _Site;
            }
        }
        public int SiteID
        {
            get
            {
                return _Site.ID;
            }
        }

        public bool IsSiteRunWithHTTPS
        {
            get
            {


                if (CurrentSettings != null)
                {
                    try
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_RUNWITH_HTTPS && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                    catch { }
                }
                return false;
            }

        }
        public bool IsRunWithWWW
        {
            get
            {


                if (CurrentSettings != null)
                {
                    try
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                    catch { }
                }
                return true;
            }

        }


       
        public static int CurrentLangID(Site site)
        {

            if (site != null && site.Settings != null && site.SupportedLanguages != null)
            {
                string CurrentLangCode = WBSSLStore.Domain.SettingConstants.DEFAULT_LANGUAGE_CODE;
                SiteSettings s = site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELANGUAGE_KEY && o.SiteID == site.ID).FirstOrDefault();
                if (s != null)
                {
                    CurrentLangCode = s.Value;
                }
                var lang = site.SupportedLanguages.Where(o => o.LangCode == CurrentLangCode && o.RecordStatus == RecordStatus.ACTIVE).SingleOrDefault();
                if (lang != null)
                {
                    return lang.ID;
                }
            }

            return 0;

        }
        public string CurrentLangCode
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELANGUAGE_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return WBSSLStore.Domain.SettingConstants.DEFAULT_LANGUAGE_CODE;
            }

        }
        public string CurrentCultureKey
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITECULTURE_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return WBSSLStore.Domain.SettingConstants.DEFAULT_CULTURE_KEY;
            }

        }
        public string CurrentCurrencyCode
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITECURRANCY_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return WBSSLStore.Domain.SettingConstants.DEFAULT_CURRANCY_CODE;
            }

        }

        public int CurrentTimeZone
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        DateTimeWithZone.CurrentTimeZone = Convert.ToInt32(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITETIMEZONE_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                        return DateTimeWithZone.CurrentTimeZone;
                    }
                }
                catch { }
                return 0;
            }

        }

        
        public string SitePhone
        {
            get
            {
                if (CurrentSettings != null)
                {
                    Domain.SiteSettings S = CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEPNONE_KEY && o.SiteID == SiteID).FirstOrDefault();
                    if (S != null)
                    {
                        return S.Value;
                    }
                    else
                        return string.Empty;
                }
                return string.Empty;
            }

        }
        public string JQueryTheme
        {
            get
            {
                if (CurrentSettings != null)
                {
                    Domain.SiteSettings S = CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_JQUERYUITHEME && o.SiteID == SiteID).FirstOrDefault();
                    if (S != null)
                    {
                        return S.Value;
                    }
                    else
                        return "base";
                }
                return "base";
            }

        }
        public string InvoicePrefix
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEINVOICEPREFIX_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public string SiteBillingEmail
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEBILLING_EMAIL_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public string SiteAdminEmail
        {
            get
            {
                if (CurrentSettings != null)
                {
                    return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                }
                return string.Empty;
            }

        }
        public string SiteSupportEmail
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public string FaceBookUrl
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEFACEBOOK_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }

        public string LinkedINUrl
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELINKEDIN_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }


        public string PaymentNote
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_PAYMENTNOTE && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;

            }
        }
        public string TwitterUrl
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITETWITTER_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }



        public string NewsLetterLink
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITENEWSLETTER_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public string GooglePlusUrl
        {
            get
            {
                try
                {
                    return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEGOOGLEPLUS_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                }
                catch
                { }
                return string.Empty;
            }
        }


        public string SiteSignature
        {
            get
            {
                try
                {
                    return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_SIGNATURE && o.SiteID == SiteID).FirstOrDefault().Value;
                }
                catch
                { }
                return string.Empty;
            }
        }

        public string LiveChatScript
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELIVECHATE_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public string BlogUrl
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEBLOG_URL && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }

        }
        public bool NeedBanner
        {
            get
            {
                if (CurrentSettings != null)
                {
                    try
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_NEEDBANNER_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                    catch { }
                }
                return false;
            }
        }
        public bool ShowAnimatedBanner
        {
            get
            {
                if (CurrentSettings != null)
                {
                    try
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_ANIMATED_BANNER_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                    catch { }
                }
                return false;
            }
        }
        public string AnimatedBannerHTML
        {
            get
            {
                if (CurrentSettings != null)
                {
                    try
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_BANNER_HTML_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                    catch { }
                }
                return string.Empty;
            }
        }
        public bool NeedApproveReseller
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEAPPROVERESELLER_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }

                return false;
            }

        }

        public bool UnderMaintanance
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(u => u.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEUNDERMAINTANACE && u.SiteID == SiteID).FirstOrDefault().Value);
                    }

                }
                catch { }
                return false;
            }
        }

        public string CurrentSiteLogoPath
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return "~/upload/sitelogo/" + CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELOGO_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return "~/upload/sitelogo/logo.png";
            }

        }
        public string CurrentSiteBannerPath
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return "~/upload/bannerimages/"+ CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_BANNERFILE_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch
                {
                }
                return "";
            }

        }
        public string CurrentMyCartLogo
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return "/upload/cartlogo/" + CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEMYCART_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch
                {
                }
                return string.Empty;
            }

        }
        public bool USESSL
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_USESSL_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return false;
            }

        }
        public bool IsVatApplicable
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_VAT_APPLICABLE_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return false;
            }

        }
        public int VatCountry
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToInt32(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_VAT_COUNTRY_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return 0;
            }

        }
        public int VATTax
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToInt32(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_VAT_NUMBER_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return 0;
            }
        }
        public string GoogleAnalytics
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.GOOGLE_ANALYTIC_VALUE && o.SiteID == SiteID).FirstOrDefault().Value;
                    }
                }
                catch { }
                return string.Empty;
            }
        }
        public static List<string> AllowedBrand(Site site)
        {

            try
            {
                if (site.Settings != null)
                {
                    return (site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_ALLOWEDBRAND_KEY && o.SiteID == site.ID).FirstOrDefault().Value.Split(',').ToList());
                }
            }
            catch { }
            return "0,1,2,3,4,99".Split(',').ToList();


        }
        public List<string> CurrentAllowedBrand
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return (CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_ALLOWEDBRAND_KEY && o.SiteID == SiteID).FirstOrDefault().Value.Split(',').ToList());
                    }
                }
                catch { }
                return "0,1,2,3,4,99".Split(',').ToList();
            }

        }
        public bool ShowRefund30DaysLogo
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_REFUND30DAYS_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return false;
            }

        }
        public bool ShowPayMentAcceptSection
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_ShowPayMentAcceptSection && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return true;
            }

        }
        public bool ShowFreeToolsSection
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_ShowFreeToolsSection && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return true;
            }

        }
            
        public string CurrentSiteImagePath
        {
            get
            {
                return "~/Content/images";
            }

        }
      

        public static string CurrentSiteAdminImagePath
        {
            get
            {
                return "~/content/admin/images";
            }
        }

        public static string GetNumberOfServers(Product p, int NumberOfServer = 1)
        {
            string strNoOfServer = "-";
            switch ((ProductBrands)p.BrandID)
            {
                case ProductBrands.RapidSSL:
                case ProductBrands.GeoTrust:
                    strNoOfServer = "Unlimited";
                    break;
                case ProductBrands.Symantec:
                case ProductBrands.Thawte:
                    strNoOfServer = NumberOfServer.ToString();
                    break;
                case ProductBrands.TrustWave:
                    strNoOfServer = "1";
                    break;
                case ProductBrands.Comodo:
                    if (p.isWildcard)
                        strNoOfServer = "100";
                    break;
            }
            return strNoOfServer;
        }

        public int PageSize
        {
            get
            {
                if (CurrentSettings != null && CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEPAGESIZE_KEY && o.SiteID == SiteID).FirstOrDefault() != null)
                {
                    return Convert.ToInt32(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITEPAGESIZE_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                }
                return WBSSLStore.Domain.SettingConstants.DEFAULT_PAGE_SIZE;
            }
        }

        public string PunchLine
        {
            get
            {
                if (CurrentSettings != null && CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_PUNCHLINE_KEY && o.SiteID == SiteID).FirstOrDefault() != null)
                {
                    return CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_PUNCHLINE_KEY && o.SiteID == SiteID).FirstOrDefault().Value;
                }
                return string.Empty;
            }
        }
        public bool IsTestimonials
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_TESTIMONIAL_APPLICABLE_KEY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return false;
            }


        }

        public bool ShowBannerHomePageOnly
        {
            get
            {
                try
                {
                    if (CurrentSettings != null)
                    {
                        return Convert.ToBoolean(CurrentSettings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_SHOWBANNER_HOME_PAGE_ONLY && o.SiteID == SiteID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return false;
            }

        }


    }


    /// <summary>
    /// Consolidates commonly used Regular Expressions
    /// </summary>
    public static class RegularExpressionConstants
    {
        public const string EMAIL = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,8}$";//@"^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*\\.([a-z]{2,4})$";// @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}$";
        public const string URL =
            @"\b(?:(?:https?|ftp|file)://|www\.|ftp\.)[-A-Z0-9+&@#/%=~_|$?!:,.]*[A-Z0-9+&@#/%=~_|$]";

        public const string CCWITHOUTSPACES =
            @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";

        public const string CCWITHSPACEANDDASH =
            @"^[ -]*(?:4[ -]*(?:\d[ -]*){11}(?:(?:\d[ -]*){3})?\d|5[ -]*[1-5](?:[ -]*[0-9]){14}|6[ -]*(?:0[ -]*1[ -]*1|5[ -]*\d[ -]*\d)(?:[ -]*[0-9]){12}|3[ -]*[47](?:[ -]*[0-9]){13}|3[ -]*(?:0[ -]*[0-5]|[68][ -]*[0-9])(?:[ -]*[0-9]){11}|(?:2[ -]*1[ -]*3[ -]*1|1[ -]*8[ -]*0[ -]*0|3[ -]*5(?:[ -]*[0-9]){3})(?:[ -]*[0-9]){11})[ -]*$";

        public const string DOMAINNAME =
            @"\b([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}\b";

        public const string INTEGER = @"(?<!\S)\d++(?!\S)";

        public const string SPECIALCHARS = @"^(a-z|A-Z|0-9)*[^#$%^&*'.?=!@%^]*$";

        public const string PASSWORD = @"^[A-Za-z0-9!@#$%^&*()_]{8}$";
    }


}
