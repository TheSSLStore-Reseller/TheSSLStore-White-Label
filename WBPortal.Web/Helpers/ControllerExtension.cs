using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Web.UI;

namespace WBSSLStore.Web.Helpers
{/// <summary>
    /// Controller extension class that adds controller methods
    /// to render a partial view and return the result as string.
    /// 
   
    /// </summary>
    public static class ControllerExtension
    {

        /// <summary>
        /// Renders a (partial) view to string.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">(Partial) view to render</param>
        /// <returns>Rendered (partial) view as string</returns>
        public static MvcHtmlString RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.RenderPartialViewToString(viewName, null);
        }

        /// <summary>
        /// Renders a (partial) view to string.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">(Partial) view to render</param>
        /// <param name="model">Model</param>
        /// <returns>Rendered (partial) view as string</returns>
        public static MvcHtmlString RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return   MvcHtmlString.Create(sw.GetStringBuilder().ToString());
            }
        }

        public static string RenderPartialToString(string controlName, object viewData)
        {
            ViewPage viewPage = new ViewPage() { ViewContext = new ViewContext() };

            viewPage.ViewData = new ViewDataDictionary(viewData);
            viewPage.Controls.Add(viewPage.LoadControl(controlName));

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (HtmlTextWriter tw = new HtmlTextWriter(sw))
                {
                    viewPage.RenderControl(tw);
                }
            }

            return sb.ToString();
        } 

        

    
    }
   
}