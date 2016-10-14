using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Data.Repository;
using WBSSLStore.Logger;
using WhiteBrandShrink;
using System.Reflection;

namespace WBSSLStore.Web.Helpers.Caching
{
    public static class SiteCacher
    {
        private const string SiteKey = "SITE";
        private const string SiteAdmin = "SITEADMIN";
        private const string SITESTMP = "SITESMTP";
        private const string DefaultSiteKey = "DEFAULTSITE";
        private const string MinorResourceVersion = "_randomnum";

        public static bool isSiteNotCreate()
        {
            return (HttpContext.Current.Cache[SiteKey + DnsHost] == null);
        
        }

        private static string DnsHost
        {
            get
            {
                return HttpContext.Current.Request.Url.DnsSafeHost.Replace("www.", string.Empty);
            }
        }
        public static Site CurrentSite
        {
            get
            {                
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["CurrentSiteID"] != null && Convert.ToInt32(HttpContext.Current.Session["CurrentSiteID"]) > 0)
                {
                    return GetSite(Convert.ToInt32(HttpContext.Current.Session["CurrentSiteID"]));
                }
                return GetCached();
            }
        }
        public static void ClearCache(int id)
        {
            HttpContext.Current.Cache.Remove(SiteKey + DnsHost);
            

            if (id > 0)
            {
                HttpContext.Current.Cache.Remove(SiteKey + id);            
                HttpContext.Current.Cache.Remove(SITESTMP + id);
                HttpContext.Current.Cache.Remove(SiteAdmin + id);
            }

            HttpContext.Current.Cache.Remove(SiteAdmin + DnsHost);
            HttpContext.Current.Cache.Remove(SITESTMP + DnsHost);



            HttpContext.Current.Cache.Remove("Countries");


        }

        public static Site GetCached()
        {

            if (Convert.ToInt32(System.Web.Security.Membership.ApplicationName) > 0)
            {

                return GetSite(Convert.ToInt32(System.Web.Security.Membership.ApplicationName));

            }

            Site site = HttpContext.Current.Cache[SiteKey + DnsHost] as Site;

            if (site == null || (site != null && ((!site.Alias.Equals(DnsHost) && !site.CName.Equals(DnsHost)))))
            {
                var siteservice = DependencyResolver.Current.GetService<ISiteService>();

                site = siteservice.GetSite(DnsHost);

                if (site == null)
                    throw new HttpException("No default site either");

                SetCache(site, 0);
                siteservice = null;

            }

            System.Web.Security.Membership.ApplicationName = site.ID.ToString();
            SetSettings(site);
            return site;
        }
        public static User SiteAdminDetail(int SiteID)
        {
            var siteAdmin = HttpContext.Current.Cache[SiteAdmin + SiteID];
            if (siteAdmin == null)
            {
                var repo = DependencyResolver.Current.GetService<ISiteRepository>();

                siteAdmin = repo.GetSiteAdmin(SiteID);

                if (siteAdmin == null)
                    throw new HttpException("No default site either");
                else
                {
                    HttpContext.Current.Cache.Remove(SiteAdmin + SiteID);
                    HttpContext.Current.Cache.Insert(SiteAdmin + SiteID, siteAdmin, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                }

                return siteAdmin as User;
            }


            return siteAdmin as User;
        }

        public static SiteSMTP SiteSMTPDetail()
        {
            var sitesmtp = HttpContext.Current.Cache[SITESTMP + CurrentSite.ID];
            if (sitesmtp == null)
            {
                using (CurrentSiteSettings CurrentSiteSettings = new Domain.CurrentSiteSettings(CurrentSite))
                {
                    var repo = DependencyResolver.Current.GetService<ISiteRepository>();

                    sitesmtp = repo.GetSMTPDetails(CurrentSiteSettings.SiteID);

                    if (sitesmtp == null)
                    {
                        throw new HttpException("Mail Configuration settings not set. Please contact site administrator.");
                    }
                    else
                    {
                        HttpContext.Current.Cache.Remove(SITESTMP + CurrentSiteSettings.SiteID);
                        HttpContext.Current.Cache.Insert(SITESTMP + CurrentSiteSettings.SiteID, sitesmtp, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                    }
                }
            }



            return sitesmtp as SiteSMTP;
        }
        public static List<Country> GetCountryCachedList()
        {
            List<Country> country = HttpContext.Current.Cache["Countries"] as List<Country>;

            if (country == null || country.Count() == 0)
            {
                var repository = DependencyResolver.Current.GetService<ISiteRepository>();

                country = repository.GetCountryList();
                HttpContext.Current.Cache.Insert("Countries", country, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
            }
            return country;
        }


        private static void SetSettings(Site site)
        {
            System.Web.Security.Membership.ApplicationName = site.ID.ToString();
            //WBSSLStore.Web.Util.WBSiteSettings.SetSettings(site);
        }
        private static void SetCache(Site site, int id)
        {
            string cachekey = string.Empty;

            if (id > 0)
                cachekey = SiteKey + id;
            else if (id.Equals(0))
                cachekey = SiteKey + DnsHost;

            HttpContext.Current.Cache.Remove(cachekey);
            HttpContext.Current.Cache.Insert(cachekey, site, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), CacheItemPriority.High, null);

            System.Web.Security.Membership.ApplicationName = site.ID.ToString();

            SetSettings(site);

        }

        public static Site GetSite(int id)
        {
            Site site = HttpContext.Current.Cache[SiteKey + id] as Site;
            if (site == null || (site != null && !site.ID.Equals(id)))
            {
                var siteservice = DependencyResolver.Current.GetService<ISiteService>();

                site = siteservice.GetSite(id);//: (siteservice.GetSite(Convert.ToInt32(!string.IsNullOrEmpty(System.Web.Security.Membership.ApplicationName) ? System.Web.Security.Membership.ApplicationName : "0")));


                if (site == null)
                    throw new HttpException("No default site either");

                SetCache(site as Site, id);
                siteservice = null;
            }

            System.Web.Security.Membership.ApplicationName = site.ID.ToString();
            SetSettings(site);
            return site;

        }

        public static string ResourceVersion()
        {
            return CacheGetValue(MinorResourceVersion.ToString(), () => Math.Abs(new Random().Next(5000))).ToString();

            //return CacheGetValue(MinorResourceVersion.ToString(), () => Math.Abs(Assembly.GetExecutingAssembly().GetName().Version.MinorRevision)).ToString();
        }

        public static object CacheGetValue(string cacheKey, Func<object> codeToExecute)
        {
            object result;
            if (HttpContext.Current.Cache[cacheKey] == null ||  Convert.ToBoolean (HttpContext.Current.Request.QueryString["Init"]))
            {
                result = codeToExecute();
                if (result != null) HttpContext.Current.Cache[cacheKey] = result;
            }
            else
            {
                result = HttpContext.Current.Cache[cacheKey];
            }
            return result;
        }

        //public static void GetInstallationConfigCache(SetupConfig objSetting)
        //{
        //    HttpContext.Current.Cache.Remove("InstallationConfig");
        //    HttpContext.Current.Cache.Insert("InstallationConfig", objSetting, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 300, 0), CacheItemPriority.High, null);                        
        //}




    }
}