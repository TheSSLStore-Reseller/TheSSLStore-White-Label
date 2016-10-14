using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WhiteBrandSite.ActionFilter
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SeoActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            { 
                
            }
            base.OnActionExecuting(filterContext);
        }
    }
}