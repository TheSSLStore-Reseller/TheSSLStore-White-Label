using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Service;
using WBSSLStore.Domain;
using System.Security.Cryptography;
using System.Web.Security;
using WBSSLStore.Data.Infrastructure;
using System.Text;
using System.IO;
using WBSSLStore.Web.Helpers.Caching;
using System.Reflection;
using System.ComponentModel;
using WBSSLStore.Web.Util;
using System.Resources;

namespace WBSSLStore.Web.Helpers
{
    public static class WBHelper
    {
        private static ResourceManager rm = null;
        public static int GetCurrentContractID(int UserID, int SiteID)
        {
            var siteservice = DependencyResolver.Current.GetService<ISiteService>();


            return siteservice.GetCurrentContractID(UserID, SiteID);
        }

        public static List<RecentlyViewProdctList> GetRecentlyViewProdctList(string Codes, int SiteID, int ContractID)
        {
            ISiteService siteservice = DependencyResolver.Current.GetService<ISiteService>();
            return siteservice.GetRecentlyViewProdctList(Codes, SiteID, ContractID);



        }
        public static IQueryable<ProductDetail> GetProductDetailRows()
        {
            var siteservice = DependencyResolver.Current.GetService<IRepository<ProductDetail>>();
            return siteservice.FindAll().AsQueryable();



        }
        public static int CurrentLangID()
        {


            Site site = SiteCacher.CurrentSite;
            if (site != null && site.Settings != null && site.SupportedLanguages != null)
            {

                var lang = site.SupportedLanguages.Where(o => o.LangCode == CurrentLangCode && o.RecordStatus == RecordStatus.ACTIVE).SingleOrDefault();
                if (lang != null)
                {
                    return lang.ID;
                }
            }


            return 0;

        }
       
        public static string CurrentColourCSS
        {
            get
            {
                string _cureentLayout = "layout1";
                string _cureentCOLOR = "color1";
                Site site = SiteCacher.CurrentSite;
                SiteSettings S = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_LAYOUT_KEY && o.SiteID == site.ID).FirstOrDefault();
                if (S != null)
                {
                    _cureentLayout = S.Value;
                }
                S = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_COLORCODE_KEY && o.SiteID == site.ID).FirstOrDefault();
                if (S != null)
                {
                    _cureentCOLOR = S.Value;
                }
                site = null;
                return VirtualPathUtility.ToAbsolute("/Content/" + _cureentLayout + "/" + _cureentCOLOR + "/css/" + _cureentCOLOR + ".min.css");
            }

        }
        public static string SiteSupportEmail
        {
            get
            {

                string _SiteSupportEmail = "";

                Site site = SiteCacher.CurrentSite;
                try
                {
                    if (site.Settings != null)
                    {
                        _SiteSupportEmail = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY && o.SiteID == site.ID).FirstOrDefault().Value;
                    }
                }
                catch { }

                site = null;
                return _SiteSupportEmail;
            }

        }
        public static string CurrentLangCode
        {
            get
            {
                string _CurrentLangCode = SettingConstants.DEFAULT_LANGUAGE_CODE;

                Site site = SiteCacher.CurrentSite;
                try
                {
                    if (site.Settings != null)
                    {
                        _CurrentLangCode = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITELANGUAGE_KEY && o.SiteID == site.ID).FirstOrDefault().Value;
                    }
                }
                catch { }

                site = null;
                return _CurrentLangCode;
            }

        }
        public static CMSPage GetMetaData()
        {
            string slug = HttpContext.Current.Request.Url.AbsolutePath;
            var service = DependencyResolver.Current.GetService<IRepository<CMSPage>>();
            int langID = WBHelper.CurrentLangID();
            int SiteID = SiteCacher.CurrentSite.ID;
            return service.Find(c => c.Pages.SiteID == SiteID && c.LangID.Equals(langID) && c.Pages.slug == slug).FirstOrDefault(); //GetPageMetadata(SiteID, WBHelper.CurrentLangID(), "/" + slug);
        }
        public static string CurrentSiteImagePath
        {
            get
            {
                string _cureentLayout = "layout1";
                string _cureentCOLOR = "color1";
                Site site = SiteCacher.CurrentSite;
                Domain.SiteSettings S = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_LAYOUT_KEY && o.SiteID == site.ID).FirstOrDefault();
                if (S != null)
                {
                    _cureentLayout = S.Value;
                }
                S = site.Settings.Where(o => o.Key == SettingConstants.CURRENT_COLORCODE_KEY && o.SiteID == site.ID).FirstOrDefault();
                if (S != null)
                {
                    _cureentCOLOR = S.Value;
                }
                site = null;
                return "~/Content/" + _cureentLayout + "/" + _cureentCOLOR + "/images";
            }

        }
        public static string CurrentSiteLogoPath
        {
            get
            {
                try
                {
                    Site site = SiteCacher.CurrentSite;
                    if (site.Settings != null)
                    {
                        return VirtualPathUtility.ToAbsolute("~/upload/" + SiteCacher.CurrentSite.ID + "/" + site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITELOGO_KEY && o.SiteID == site.ID).FirstOrDefault().Value);
                    }
                }
                catch { }
                return VirtualPathUtility.ToAbsolute(CurrentSiteImagePath + "/logo.png");
            }

        }
        public static string CreateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[32];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }
        public static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = string.Concat(pwd, salt);

            string hashedPwd =  FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");

            return hashedPwd;
        }
        public static string GetTemplateText(string strFilename)
        {
            string strPath = strFilename;
            StringBuilder stbHTML = new StringBuilder();
            if (File.Exists(strPath))
            {
                StreamReader objStreamReader = new StreamReader(strPath);
                stbHTML.Append(objStreamReader.ReadToEnd());
                objStreamReader.Dispose();
            }
            return stbHTML.ToString();
        }
        public static EmailQueue AddinMailQueue(string Toemail, string AltEmail, int SiteID, int SMTPID, EmailTemplates emailTemplate)
        {
            //Send Email to Client
            EmailQueue EQ = new EmailQueue();
            EQ.QueuedOn = System.DateTimeWithZone.Now;
            EQ.SiteID = SiteID;
            EQ.NumberOfTries = 0;
            EQ.LastAttempt = null;
            EQ.SiteSMTPID = SMTPID;
            EQ.BCC = emailTemplate.BCC;

            if (!string.IsNullOrEmpty(AltEmail))
                EQ.CC = AltEmail;

            EQ.TO = Toemail;

            EQ.Subject = emailTemplate.EmailSubject;
            EQ.EmailContent = emailTemplate.EmailContent;

            return EQ;
        }
        public static EmailTemplates GetEmailTemplate(EmailType enmTemplate, int SiteID, int LangID)
        {
            var repo = DependencyResolver.Current.GetService<IRepository<EmailTemplates>>();

            if (repo == null)
                repo = new EFRepository<EmailTemplates>(new DatabaseFactory());

            return repo.Find(x => x.EmailTypeId == (int)enmTemplate && x.SiteID == SiteID && x.LangID == LangID).FirstOrDefault();


        }
        public static decimal CalCulateItemPrice(ProductPricing pp, int NoOfServer, int NoOfDomains)
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
        public static void AssignPromoInCart(PromoCode prow, ShoppingCartDetail cartDetails, ProductPricing pp)
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
                            promodiscount = (pp.SalesPrice * prow.Discount) / 100;
                            if (!cartDetails.Product.isNoOfServerFree)
                                promodiscount = (promodiscount * (cartDetails.NumberOfServers > 0 ? cartDetails.NumberOfServers : 1));

                        }
                    }

                    cartDetails.PromoCode = prow.Code;
                    cartDetails.PromoDiscount = promodiscount;

                }
                else
                {
                    cartDetails.PromoCode = string.Empty;
                    cartDetails.PromoDiscount = 0;
                }

            }
        }

        public static string apllicationfullpath(Site site)
        {
            string Host = string.Empty;

            using (CurrentSiteSettings CurrentSiteSettings = new CurrentSiteSettings(site))
            {

                Host = (string.IsNullOrEmpty(CurrentSiteSettings.CurrentSite.Alias) ? CurrentSiteSettings.CurrentSite.CName : CurrentSiteSettings.CurrentSite.Alias);
                if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
                {
                    if (CurrentSiteSettings.IsRunWithWWW && !Host.Contains("www."))
                    {
                        Host = Host.Replace(Host, "www." + Host);
                    }
                    else if (!CurrentSiteSettings.IsRunWithWWW && Host.Contains("www."))
                        Host = Host.Replace("www.", "");
                }

                Host = ((CurrentSiteSettings.USESSL && CurrentSiteSettings.IsSiteRunWithHTTPS) ? "https" : "http") + "://" + Host;


            }

            return Host;
        }

        public static string ApllicationFullPath
        {
            get
            {
                string Host = string.Empty;
                Site site = SiteCacher.GetCached();
                Host = apllicationfullpath(site);
                site = null;
                return Host;
            }
        }
        public static string ApllicationSecurePath
        {
            get
            {
                string Path = string.Empty;
                Site site = SiteCacher.GetCached();
                using (CurrentSiteSettings CurrentSiteSettings = new CurrentSiteSettings(site))
                {
                    Path = CurrentSiteSettings.USESSL ? apllicationfullpath(site).Replace("http:", "https:") : ApllicationFullPath;
                }
                site = null;

                return Path;
            }
        }
        public static string GenerateSiteMapXML()
        {
            Site site = SiteCacher.GetCached();

            StringBuilder strMenu = new StringBuilder();
            List<Pages> lstPage = site.Pages.ToList();
            if (lstPage != null && lstPage.Count > 0)
            {
                string str = "xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"  xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\"";
                strMenu.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + string.Empty + "<urlset  " + str + ">" + string.Empty);

                strMenu.AppendFormat("{3}<url>{1}<loc>{0}</loc> <changefreq>daily</changefreq><priority>{2}</priority></url>", ApllicationFullPath, string.Empty, "1.00", string.Empty);

                foreach (Pages objPage in lstPage)
                {
                    strMenu.AppendFormat("{3}<url>{1}<loc>{0}</loc> <changefreq>daily</changefreq><priority>{2}</priority></url>", ApllicationFullPath + objPage.slug, string.Empty, objPage.ParentID.Equals(0) ? "0.90" : "0.80", string.Empty);
                }
            }
            strMenu.Append("</urlset>");
            site = null;
            return strMenu.ToString();
        }

        private static string GenerateMenuStringChildvd(List<Product> obProductRow)
        {
            if (obProductRow != null && obProductRow.Count > 0)
            {
                StringBuilder inner = new StringBuilder("<ul>");
                foreach (Product pgr in obProductRow)
                {
                    inner.Append("<li><a href='" + ApllicationFullPath + (!string.IsNullOrEmpty(pgr.DetailPageslug) ? pgr.DetailPageslug : "#") + "' target=\"_self\">" + GetValueFromResourceFile(pgr.ProductName) + "</a>");
                    inner.Append("</li>");
                   
                }
                inner.Append("</ul>");
                return inner.ToString();
            }
            else
                return string.Empty;

        }


        public static bool FooterPage(string fPage)
        {
            Site site = SiteCacher.GetCached();
            List<Pages> lstpage = site.Pages.Where(pg => pg.PageStatusID == (int)PageStatus.Show && pg.Caption.ToString().Trim().ToLower() == fPage.ToString().Trim().ToLower()).ToList();
            if (lstpage.Count > 0)
                return true;
            else
                return false;
        }
        public static string GetValueFromResourceFile(string key1)
        {
            string key = key1.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
            string resourceValue = string.Empty;
            try
            {
                if (rm == null)
                {
                    string resourceFile = "Message";
                    string filePath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\bin\\Navigation";
                    rm = ResourceManager.CreateFileBasedResourceManager(resourceFile, filePath, null);
                }
                // retrieve the value of the specified key
                resourceValue = rm.GetString(key.ToLower());
                if (string.IsNullOrEmpty(resourceValue))
                    resourceValue = key1;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                resourceValue = key1;
            }
            return resourceValue;
        }
        public static string GenerateMenuString()
        {
            Site site = SiteCacher.GetCached();

            bool IsLoginVisible = false;

            StringBuilder strMenu = new StringBuilder();

            List<Pages> lstPage = site.Pages.ToList();
            if (lstPage != null && lstPage.Count > 0)
            {
                strMenu.Append("<div><ul class=\"contentlist\">");
                List<Pages> objParent = lstPage.Where(pg => pg.ParentID == 0).ToList();
                int Count = 0;
                foreach (Pages objPage in objParent)
                {
                    if (objPage.Caption.ToLower().Equals("login") || objPage.Caption.ToLower().Equals("logout"))
                    {
                        IsLoginVisible = (objPage.PageStatusID == (int)PageStatus.Show);
                    }

                    if (Count == 0)
                    {
                        strMenu.Append("<li><a href=\"" + (objPage.slug.ToLower().StartsWith("http") ? objPage.slug : ApllicationFullPath + objPage.slug) + "\" target=\"" + objPage.URLTarget.ToString() + "\" >" + GetValueFromResourceFile(objPage.Caption) + "</a>" + GenerateMenuStringChild(lstPage.Where(pg => pg.ParentID == objPage.ID).ToList(), lstPage) + "</li>");
                    }

                    else
                    {
                        strMenu.Append("<li><a href=\"" + (objPage.slug.ToLower().StartsWith("http") ? objPage.slug : ApllicationFullPath + objPage.slug) + "\" target=\"" + objPage.URLTarget.ToString() + "\">" + GetValueFromResourceFile(objPage.Caption) + "</a>" + GenerateMenuStringChild(lstPage.Where(pg => pg.ParentID == objPage.ID).ToList(), lstPage) + "</li>");
                    }

                    Count++;
                }


                bool IsAuth = (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated);

                strMenu.Append("<li><a href=\"" + (IsAuth ? "/staticpage/Logout" : "/logon") + "\">" + GetValueFromResourceFile((IsAuth ? "Logout" : "Login")) + "</a></li>");

                strMenu.Append("</ul></div>");
            }
            site = null;
            return strMenu.ToString();
        }
        private static string GenerateMenuStringChild(List<Pages> obPageRow, List<Pages> lstAllPages)
        {

            if (obPageRow != null && obPageRow.Count > 0)
            {
                StringBuilder inner = new StringBuilder("<ul>");
                foreach (Pages pgr in obPageRow)
                {
                    inner.Append("<li><a href='" + (pgr.slug.ToLower().StartsWith("http") ? pgr.slug : ApllicationFullPath + pgr.slug) + "' target=\"" + pgr.URLTarget.ToString() + "\">" + GetValueFromResourceFile(pgr.Caption) + "</a>");
                    inner.Append(GenerateMenuStringChild(lstAllPages.Where(pg => pg.ParentID == pgr.ID).ToList(), lstAllPages));
                    inner.Append("</li>");
                }
                inner.Append("</ul>");

                return inner.ToString();
            }
            else
                return string.Empty;
        }




        public static string GenerateCerblobMenu(string slug, int PageID)
        {
            Site site = SiteCacher.GetCached();
            List<Pages> lstPage = site.Pages.Where(pg => (pg.StartDate == null ? DateTime.Now.Date : pg.StartDate.Value) <= DateTime.Now.Date && (pg.EndDate == null ? DateTime.Now.Date : pg.EndDate.Value) >= DateTime.Now.Date && pg.PageStatusID == (int)PageStatus.Show && WBSSLStore.Web.Util.WBSiteSettings.AllowedBrand(site).Contains(pg.BrandID.ToString())).OrderBy(pg => pg.ParentID).ThenBy(pg => pg.DisplayOrder).ThenBy(pg => pg.BrandID).ThenBy(pg => pg.ID).ToList();
            site = null;
            if (lstPage != null && lstPage.Count > 0)
                return GenerateCertblobSubMenu(lstPage.Where(pg => (pg.ParentID == PageID || pg.slug.ToLower().Equals(slug)) && pg.ParentID != 0).ToList(), lstPage);
            return string.Empty;
        }

        public static string GenerateCertblobSubMenu(List<Pages> obPageRow, List<Pages> lstAllPages)
        {
            if (obPageRow != null && obPageRow.Count > 0)
            {
                StringBuilder inner = new StringBuilder("<ul>");
                foreach (Pages pgr in obPageRow)
                {
                    inner.Append("<li><a href='" + ApllicationFullPath + pgr.slug + "' target=\"" + pgr.URLTarget.ToString() + "\">" + GetValueFromResourceFile(pgr.Caption) + "</a>");
                    inner.Append(GenerateCertblobSubMenu(lstAllPages.Where(pg => pg.ParentID == pgr.ID).ToList(), lstAllPages));
                    inner.Append("</li>");
                }
                inner.Append("</ul>");

                return inner.ToString();
            }
            else
                return string.Empty;
        }

        public static void GetCSVFile<T>(List<T> lstData, List<SelectListItem> headers, string FileName)
        {
            HttpContext context = HttpContext.Current;
            context.Response.Clear();

            foreach (SelectListItem sl in headers)
                context.Response.Write(sl.Text + ",");
            context.Response.Write(Environment.NewLine);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            object[] values = new object[props.Count];

            foreach (T item in lstData)
            {
                foreach (SelectListItem sl in headers)
                {
                    if (sl.Value.Contains("."))
                    {
                        string[] str = sl.Value.Split('.');
                        if (str != null && str.Length > 0)
                        {
                            PropertyInfo[] propsChild = props[str[0]].PropertyType.GetProperties();
                            foreach (PropertyInfo pi in propsChild)
                            {
                                if (pi.Name.ToLower() == str[1].ToLower())
                                {
                                    object val = pi.GetValue(item, null);
                                    if (val != null)
                                        context.Response.Write(val.ToString().Replace(",", string.Empty) + ",");
                                    else
                                        context.Response.Write(",");
                                }
                            }
                        }
                    }
                    else
                    {

                        if (props[sl.Value] != null)
                        {
                            object val = props[sl.Value].GetValue(item);
                            if (val != null)
                                context.Response.Write(val.ToString().Replace(",", string.Empty) + ",");
                            else
                                context.Response.Write(",");
                        }
                    }
                }
                context.Response.Write(Environment.NewLine);
            }
            context.Response.ContentType = "text/csv";
            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileName + ".csv");
            context.Response.AddHeader("Pragma", "public");
            context.Response.End();
        }

        public static void GetCSVFile(List<string> lstData, string FileName)
        {
            string CSVData = String.Join(Environment.NewLine, lstData.Select(x => x.ToString()).ToArray());
            HttpContext context = HttpContext.Current;
            context.Response.Clear();
            context.Response.Write(CSVData);
            context.Response.ContentType = "text/csv";
            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileName + ".csv");
            context.Response.AddHeader("Pragma", "public");
            context.Response.End();
        }

        public static OrderStatus GetOrderDetailStatusValue(string Status)
        {
            if (!string.IsNullOrEmpty(Status))
            {
                switch (Status.ToLower())
                {
                    case "pending":
                        return OrderStatus.PENDING;
                    case "active":
                    case "issued":
                    case "valid":
                    case "reissued":
                        return OrderStatus.ACTIVE;
                    case "cancelled":
                    case "revoked":
                    case "rejected":
                        return OrderStatus.REJECTED;
                    case "expired":
                        return OrderStatus.ACTIVE;
                    case "complete":
                        return OrderStatus.ACTIVE;
                    default:
                        return OrderStatus.PENDING;
                }
            }
            else
                return OrderStatus.PENDING;
        }

        public static string GetProductPricing(List<ProductPricing> lstPricing)
        {
            
            string RetValue = string.Empty;
            ProductPricing pricing = lstPricing.Where(price => price.NumberOfMonths == lstPricing.Max(pp => pp.NumberOfMonths)).FirstOrDefault();
            decimal YearlyPrice = pricing.SalesPrice > 0 ? (pricing.SalesPrice / Convert.ToInt32(pricing.NumberOfMonths / 12)) : pricing.SalesPrice;
            return string.Format("{0:C}", YearlyPrice);
        }
        

        public static string GetProductPricingSave(List<ProductPricing> lstPricing)
        {
            string RetValue = string.Empty;
            ProductPricing pricing = lstPricing.Where(price => price.NumberOfMonths == lstPricing.Max(pp => pp.NumberOfMonths)).FirstOrDefault();
            decimal YearlyPrice = pricing.SalesPrice > 0 ? (pricing.SalesPrice / Convert.ToInt32(pricing.NumberOfMonths / 12)) : pricing.SalesPrice;
            decimal SRP = pricing.RetailPrice > 0 ? (pricing.RetailPrice / Convert.ToInt32(pricing.NumberOfMonths / 12)) : pricing.RetailPrice;
            return string.Format("{0:C}", (SRP - YearlyPrice));
        }
        public static string GetMAXProductPricingID(List<ProductPricing> lstPricing)
        {
            string RetValue = string.Empty;
            ProductPricing pricing = lstPricing.Where(price => price.NumberOfMonths == lstPricing.Max(pp => pp.NumberOfMonths)).FirstOrDefault();
            return pricing.ID.ToString();
        }
        public static string CalculateSavePercentage(decimal IssuePrice, decimal RetailPrice, int Year)
        {

            Year = Year > 0 ? Year : 1;
            decimal RP = RetailPrice / Year;
            decimal IP = IssuePrice / Year;
            decimal Percentage, PerYear;
            PerYear = RP - IP;
            Percentage = (PerYear * 100) / (RP > 0 ? RP : 1);
            return Convert.ToInt16(Percentage).ToString() + "%.";
        }

        public static int ToInt(object odata)
        {
            return ToInt(odata, 0);
        }

        public static int ToInt(object odata, int defaultvalue)
        {
            int result = defaultvalue;
            if (odata != null)
            {
                try
                {
                    int.TryParse(odata.ToString(), out result);
                }
                catch { }
            }
            return (result == 0 ? defaultvalue : result);
        }

        public static bool CheckBrandShow(List<ProductPricing> _ProductPricing, ProductBrands brandEnum)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            if (_ProductPricing != null && WBSiteSettings.AllowedBrand(site).Contains(((int)brandEnum).ToString()) && _ProductPricing.Where(pp => pp.Product.BrandID == (int)brandEnum).Count() > 0)
                Result = true;
            else
                Result = false;

            site = null;
            return Result;

        }
        public static bool ShowStandardSSL(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.StandardSSL.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowHighAssurance(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.HighAssurance.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowWildCardSSL(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.WildCardSSL.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowSAN(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.SAN.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowSGC(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.SGC.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowEV(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.EV.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowCodeSigning(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.CodeSigning.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowTrustSeal(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.TrustSeal.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowOther(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.Other.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool ShowMalware(List<ProductPricing> _ProductPricing)
        {
            bool Result = false;
            Site site = SiteCacher.GetCached();
            foreach (string str in SSLCategories.Malware.Split(','))
            {
                if (_ProductPricing.Where(pp => pp.Product.InternalProductCode.ToLower().Equals(str.ToLower()) && WBSiteSettings.AllowedBrand(site).Contains(pp.Product.BrandID.ToString())).Count() > 0)
                {
                    Result = true;
                    break;
                }
            }
            site = null;
            return Result;
        }
        public static bool IsRunWithWWW(Site site)
        {


            if (site.Settings != null)
            {
                try
                {
                    return Convert.ToBoolean(site.Settings.Where(o => o.Key == SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW && o.SiteID == site.ID).FirstOrDefault().Value);
                }
                catch { }
            }
            return true;


        }
        public static string InvoicePrefix(Site Site)
        {

            try
            {
                if (Site.Settings != null)
                {
                    return Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITEINVOICEPREFIX_KEY && o.SiteID == Site.ID).FirstOrDefault().Value;
                }
            }
            catch { }
            return string.Empty;

        }
        public static int PageSize(Site Site)
        {


            if (Site.Settings != null && Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITEPAGESIZE_KEY && o.SiteID == Site.ID).FirstOrDefault() != null)
            {
                return Convert.ToInt32(Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITEPAGESIZE_KEY && o.SiteID == Site.ID).FirstOrDefault().Value);
            }

            return SettingConstants.DEFAULT_PAGE_SIZE;

        }
        public static bool USESSL(Site Site)
        {
            try
            {
                if (Site.Settings != null)
                {
                    return Convert.ToBoolean(Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_USESSL_KEY && o.SiteID == Site.ID).FirstOrDefault().Value);
                }
            }
            catch { }
            return false;


        }
        public static string SiteAdminEmail(Site Site)
        {

            if (Site.Settings != null)
            {
                return Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY && o.SiteID == Site.ID).FirstOrDefault().Value;
            }
            return string.Empty;


        }
        public static string ThankyouPage(Site site)
        {

            try
            {
                return site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITE_THANKYOUPAGE && o.SiteID == site.ID).FirstOrDefault().Value;
            }
            catch
            { }
            return string.Empty;

        }
   

        //public static string GetProductDetailValue(string pProductCode, string SettingValue)
        //{
        //    string path = HttpContext.Current.Server.MapPath("/content/Templates/ProductsDetail.xml");
        //    System.Xml.Linq.XDocument obj = System.Xml.Linq.XDocument.Load(path);
        //    var _data = from p in obj.Descendants("Product").Where(x => x.Element("ProductCode").Value.Equals(pProductCode,StringComparison.OrdinalIgnoreCase)) select new { strVal = p.Element(SettingValue).Value };
        //    if (_data != null && _data.FirstOrDefault() != null && _data.Count() > 0)
        //    {
        //        return Convert.ToString(_data.FirstOrDefault().strVal);
        //    }
        //    return string.Empty;
        //}


        public static ConfigurationStage GetSiteConfiguration(string Connection )
        {
            try
            {
                using (Data.WBSSLStoreDb db = !string.IsNullOrEmpty(Connection) ? new Data.WBSSLStoreDb(Connection) : new Data.WBSSLStoreDb())
                {
                    if(string.IsNullOrEmpty( db.Database.Connection.ConnectionString))
                        return ConfigurationStage.NoCreated;

                    var pSites = db.Sites.Where(x => x.isActive).ToList().FirstOrDefault();
                    if (pSites != null)
                    {
                        SiteSettings item = db.Settings.Where(ss => ss.Key.ToLower().Equals(SettingConstants.CURRENT_SITE_CONFIGURATIONSTATUS, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (item != null && !string.IsNullOrEmpty(item.Value))
                        {
                            return (ConfigurationStage)Convert.ToInt32(item.Value);
                        }
                    }
                }
            }
            catch (Exception e) {  }
            return ConfigurationStage.NoCreated;
        }


       
    }

   

}