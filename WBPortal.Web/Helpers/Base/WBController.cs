using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Ninject.Parameters;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Caching;
using System.Xml;
using System.IO;
using System.Configuration;

namespace WBSSLStore.Web.Helpers.Base
{
    public class WBController<VM, R, S> : Controller
    {
        protected IUnitOfWork _unitOfWork;
        public VM _viewModel;
        protected R _repository;
        protected S _service;
        protected WBSSLStore.Logger.ILogger _logger;
        private Site _Site = null;

        public WBController()
            : base()
        {
            _unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>();
            _viewModel = DependencyResolver.Current.GetService<VM>();
            _repository = DependencyResolver.Current.GetService<R>();
            _service = DependencyResolver.Current.GetService<S>();
            _logger = DependencyResolver.Current.GetService<WBSSLStore.Logger.ILogger>();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            using (CurrentSiteSettings CurrentSiteSettings = new CurrentSiteSettings(SiteCacher.CurrentSite))
            {                 
                WBSSLStore.Web.Helpers.Localization.CultureSwitch.SwitchCulture(CurrentSiteSettings.CurrentSite, CurrentSiteSettings.CurrentLangCode, CurrentSiteSettings.CurrentCultureKey);
            }
        }
      
        public Site Site
        {
            get
            {
                if(_Site == null )
                    _Site = SiteCacher.CurrentSite;

               return _Site;
            }
        }

        public Site GetSite(int id)
        {

            if (_Site != null &&  !_Site.ID.Equals(id) && id > 0)
                _Site = SiteCacher.GetSite(id);
            else if (_Site == null && id > 0)
                _Site = SiteCacher.GetSite(id);
           

            if (_Site == null)
                throw new Exception("Site Setting is null. No Site Found."); 

            return _Site;

        }

        public List<Country> CountryList
        {
            get { return SiteCacher.GetCountryCachedList(); }


        }

        public MvcHtmlString WBRenderPartialViewToString(string viewName, object model)
        {
            return this.RenderPartialViewToString(viewName, model);
        }
    }

    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool bAuthenticated = true;

            if (System.Web.Security.Roles.IsUserInRole("admin"))
                bAuthenticated = true;
            else if (System.Web.Security.Roles.IsUserInRole("finance"))
            {
                List<string> lst = GetPagesForRole("finance");
                string strExist = lst.Find(l => httpContext.Request.RawUrl.ToLower().StartsWith(l));
                bAuthenticated = !string.IsNullOrEmpty(strExist);
            }
            else if (System.Web.Security.Roles.IsUserInRole("support"))
            {
                List<string> lst = GetPagesForRole("support");
                string strExist = lst.Find(l => httpContext.Request.RawUrl.ToLower().StartsWith(l));
                bAuthenticated = !string.IsNullOrEmpty(strExist);
            }
            if (!bAuthenticated)
                httpContext.Response.Redirect("~/admin/accessdenied", true);
            return bAuthenticated;
        }

        public static List<string> GetPagesForRole(string strRole)
        {
            List<string> lstPages = new List<string>();
            XmlDocument xmldoc = new XmlDocument();
            StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~") + @"\Roles.xml");
            XmlReader xReader = XmlReader.Create(reader);
            xmldoc.Load(xReader);
            try
            {
                XmlNodeList nodeList = xmldoc.SelectNodes("Roles/" + strRole);
                foreach (XmlNode node in nodeList[0].ChildNodes)
                    lstPages.Add(node.InnerText);
                return lstPages;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }
    }
}