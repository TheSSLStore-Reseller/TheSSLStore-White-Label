using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Domain;
using WBSSLStore.Web.Helpers.PagedList;
using System.Web.Security;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class PromoCodeController : WBController<PromoCode, IRepository<PromoCode>, IProductService>
    {
        public User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
                    if (loginuser != null && loginuser.Details != null)
                        return loginuser.Details;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser(User.Identity.Name);
                    if (loginuser != null && loginuser.Details != null)
                        return loginuser.Details;
                }
                return null;
            }
        }
        //
        // GET: /Admin/PromoCode/

        public ActionResult Index(FormCollection collection, int? page)
        {
            ViewBag.PromoCode = collection["txtPromoCode"] ?? Request.QueryString["promo"];
            ViewBag.Status = collection["ddlStatus"] ?? Request.QueryString["status"];
            ViewBag.ProductID = collection["ddlProducts"] ?? Request.QueryString["productid"];

            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();
            int ProductID = !string.IsNullOrEmpty(ViewBag.ProductID) ? Convert.ToInt16(ViewBag.ProductID) : 0;
            int Status = !string.IsNullOrEmpty(ViewBag.Status) ? Convert.ToInt16(ViewBag.Status) : 0;
            string PromoCode = Convert.ToString(ViewBag.PromoCode);
            IPagedList<PromoCode> lstPromo = null;

            if (Status == 2)
            {
                lstPromo = _repository.Find(pc => pc.SiteID == Site.ID && pc.Code.Contains((string.IsNullOrEmpty(PromoCode) ? pc.Code : PromoCode))
                                               && pc.ProductID == (ProductID > 0 ? ProductID : pc.ProductID)
                                               && System.Data.Entity.DbFunctions.DiffDays(DateTimeWithZone.Now, pc.EndDate) < 0
                                               ).OrderByDescending(pc => pc.AuditDetails.DateCreated).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));
            }
            else
            {
                lstPromo = _repository.Find(pc => pc.SiteID == Site.ID && pc.Code.Contains((string.IsNullOrEmpty(PromoCode) ? pc.Code : PromoCode))
                                                && pc.ProductID == (ProductID > 0 ? ProductID : pc.ProductID)
                                                && System.Data.Entity.DbFunctions.DiffDays((Status == 1 ? DateTimeWithZone.Now : pc.EndDate), pc.EndDate) >= 0
                                                ).OrderByDescending(pc => pc.AuditDetails.DateCreated).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));
            }

            ViewBag.Products = from pro in _service.GetAllProductAvailablity(Site.ID).ToList()
                               select pro.Product;
            ViewBag.PromoCode = PromoCode;
            ViewBag.Status = Status == 0 ? "" : Status.ToString();
            ViewBag.ProductID = ProductID == 0 ? "" : ProductID.ToString();
            if (!PagingInputValidator.IsPagingInputValid(ref page, lstPromo))
                return View();
            return View(lstPromo);
        }


        //
        // GET: /Admin/PromoCode/Create

        public ActionResult Create()
        {
            Contract ClientContract = _service.GetClientContract(Site.ID);
            var pricing = _service.GetProductPricing(Site.ID, 0, ClientContract.ID).ToList();
            string SelectedVal = string.Empty;
            if (_viewModel.ID > 0)
                SelectedVal = _viewModel.ProductID + "|" + _viewModel.NoOfMonths;
            IEnumerable<SelectListItem> lst = from price in pricing select new SelectListItem { Selected = (SelectedVal == price.ProductID + "|" + price.NumberOfMonths), Text = price.Product.ProductName + ":" + Convert.ToInt16(price.NumberOfMonths / 12) + " Years -- " + Convert.ToDecimal(price.SalesPrice / Convert.ToInt32((price.NumberOfMonths >= 12 ? price.NumberOfMonths : 12) / 12)).ToString("C") + " per year", Value = price.ProductID + "|" + price.NumberOfMonths };
            ViewBag.ProductList = lst;
            return View(_viewModel);
        }

        //
        // POST: /Admin/PromoCode/Create

        [HttpPost]
        public ActionResult Create(PromoCode model)
        {
            try
            {
                Contract ClientContract = _service.GetClientContract(Site.ID);
                var pricing = _service.GetProductPricing(Site.ID, 0, ClientContract.ID).ToList();

                string SelectedVal = string.Empty;
                if (model.ID > 0)
                    SelectedVal = model.ProductID + "|" + model.NoOfMonths;
                IEnumerable<SelectListItem> lst = from price in pricing select new SelectListItem { Selected = (SelectedVal == price.ProductID + "|" + price.NumberOfMonths), Text = price.Product.ProductName + ":" + Convert.ToInt16(price.NumberOfMonths / 12) + " Years -- " + Convert.ToDecimal(price.SalesPrice / Convert.ToInt16((price.NumberOfMonths >= 12 ? price.NumberOfMonths : 12) / 12)).ToString("C") + " per year", Value = price.ProductID + "|" + price.NumberOfMonths };
                ViewBag.ProductList = lst;


                if (ModelState.IsValid)
                {
                    if (_repository.Find(pr => pr.Code.Equals(model.Code, StringComparison.OrdinalIgnoreCase) && pr.SiteID == Site.ID && pr.ID != model.ID).FirstOrDefault() == null)
                    {
                        if (model.ID > 0)
                        {
                            model.AuditDetails.DateModified = DateTimeWithZone.Now;
                            model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                            model.AuditDetails.IP = Request.UserHostAddress;
                            _repository.Update(model);
                        }
                        else
                        {
                            model.AuditDetails = DependencyResolver.Current.GetService<Audit>();
                            model.AuditDetails.DateModified = DateTimeWithZone.Now;
                            model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                            model.AuditDetails.IP = Request.UserHostAddress;
                            model.AuditDetails.DateCreated = DateTimeWithZone.Now;
                            model.AuditDetails.ByUserID = CurrentUser.ID;
                            model.SiteID = Site.ID;
                            _repository.Add(model);
                        }
                        _unitOfWork.Commit();
                        return RedirectToAction("Index");
                    }
                    else
                    {


                        ViewBag.Message = "<div class='errormsg'>" + WBSSLStore.Resources.ErrorMessage.Message.PromocodeAlreadyExist + "</div>";
                        return View(model);
                    }
                }
                else
                    return View(model);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {

                ViewBag.Message = "<div class='errormsg'>" + ex.Message + "</div>";
                return View(model);
            }
        }

        //
        // GET: /Admin/PromoCode/Edit/5

        public ActionResult Edit(int id)
        {
            Contract ClientContract = _service.GetClientContract(Site.ID);
            var pricing = _service.GetProductPricing(Site.ID, 0, ClientContract.ID).ToList();
            _viewModel = _repository.Find(pc => pc.ID == id && pc.SiteID == Site.ID).EagerLoad(pg => pg.AuditDetails).SingleOrDefault();
            string SelectedVal = string.Empty;
            if (_viewModel.ID > 0)
                SelectedVal = _viewModel.ProductID + "|" + _viewModel.NoOfMonths;
            IEnumerable<SelectListItem> lst = from price in pricing select new SelectListItem { Selected = (SelectedVal == price.ProductID + "|" + price.NumberOfMonths), Text = price.Product.ProductName + ":" + Convert.ToInt16(price.NumberOfMonths / 12) + " Years -- " + Convert.ToDecimal(price.SalesPrice / Convert.ToInt16((price.NumberOfMonths >= 12 ? price.NumberOfMonths : 12) / 12)).ToString("C") + " per year", Value = price.ProductID + "|" + price.NumberOfMonths };
            ViewBag.ProductList = lst;
            return View("create", _viewModel);
        }

        //
        // POST: /Admin/PromoCode/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, PromoCode model)
        {
            try
            {
                // TODO: Add update logic here
                Create(model);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        //
        // POST: /Admin/PromoCode/Delete/5

        public ActionResult Delete(int id)
        {
            var Promo = _repository.Find(pr => pr.SiteID == Site.ID && pr.ID == id).FirstOrDefault();
            if (Promo != null && Promo.ID > 0)
            {
                var _OrderDetail = DependencyResolver.Current.GetService<IRepository<OrderDetail>>();
                var lstOrderDetail = _OrderDetail.Find(od => od.PromoCodeID == id && od.Order.SiteID == Site.ID);
                if (lstOrderDetail != null && lstOrderDetail.Count() > 0)
                {
                    return Json(false);
                }
                else
                {
                    _repository.Delete(Promo);
                    _unitOfWork.Commit();
                    return Json(true);
                }
            }
            else
                return Json(false);
        }
    }
}
