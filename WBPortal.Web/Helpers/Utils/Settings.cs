using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;

namespace WBSSLStore.Web.Util
{
    public static class WBSiteTools
    {
        public static string MakeId(string s, string prefix = null)
        {
            if (s == null) return null;
            return (prefix ?? string.Empty) + s.Replace('.', '_').Replace('[', '_').Replace(']', '_');
        }

        public static string JsEncode(string s)
        {
            return s.Replace("\'", "\\'").Replace("\"", "\\\"");
        }

        public static string MakeIdJsArray(IEnumerable<string> arr)
        {
            return string.Join(",", arr.Select(o => "'" + MakeId(o) + "'").ToArray());
        }

        public static string MakeJsArray(IEnumerable<string> arr)
        {
            return string.Join(",", arr.Select(o => "'" + o + "'"));
        }

        public static string MakeJsArrayObj(IEnumerable<object> arr)
        {
            return string.Join(",", arr.Select(o => "'" + o + "'").ToArray());
        }
    }

    public class WBSiteSettings : WBSSLStore.Domain.CurrentSiteSettings
    {

        public WBSiteSettings(Site _CurrentSite)
        {
            base.SetSettings(_CurrentSite);

        }
        public string LogoImagePath
        {

            get
            {
                return VirtualPathUtility.ToAbsolute(CurrentSiteLogoPath);
            }


        }
        public string BannerImagePath
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(CurrentSiteBannerPath);
            }

        }


      
        public string ImagePath
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(CurrentSiteImagePath);
            }

        }
        


        public static string AppPath
        {
            get
            {
                return HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.DnsSafeHost;
            }
        }

        public static string AdminImagePath
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(CurrentSiteAdminImagePath);
            }
        }

        public static bool IsSMTPConfigure
        {
            get
            {
                try
                {
                    WBSSLStore.Domain.SiteSMTP obj = WBSSLStore.Web.Helpers.Caching.SiteCacher.SiteSMTPDetail();
                    if (obj != null && obj.ID > 0)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool IsPaymentSettingConfigure
        {
            get
            {
                try
                {
                    int SiteID = 0;
                    Site site = WBSSLStore.Web.Helpers.Caching.SiteCacher.CurrentSite;
                    SiteID = site.ID;
                    site = null;

                    var repo = DependencyResolver.Current.GetService<IRepository<PaymentGateways>>();
                    var paymentsett = repo.Find(pg => pg.SiteID == SiteID);
                    if (paymentsett.Count() > 0)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

}