using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Repository;
using WBSSLStore.Data.Infrastructure;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class HomeController : WBController<AdminDashboard, IWBRepository, ISiteService>
    {


        // GET: /Admin/Home/

        // [Authorize(Roles="Admin")]  
        public ActionResult Index()
        {

            _viewModel = _repository.GetAdminDashBoard(Site.ID);
            return View(_viewModel);
        }


    }
}
