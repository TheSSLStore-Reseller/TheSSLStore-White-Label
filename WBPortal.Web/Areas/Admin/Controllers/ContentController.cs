using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using System.Xml.Linq;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ContentController : WBController<CMSPage, IRepository<CMSPage>, IRepository<CMSPageContent>>
    {

        public ActionResult Index(FormCollection collection)
        {
            if (collection["txtSearchPage"] != null)
                return View(Site.Pages.Where(pg => WBSSLStore.Web.Util.WBSiteSettings.AllowedBrand(Site).Contains(pg.BrandID.ToString()) && pg.Caption.ToLower().Contains(collection["txtSearchPage"].ToLower()))
                                      .OrderBy(pg => pg.ParentID).ThenBy(pg => pg.DisplayOrder).ToList());
            else
                return View(Site.Pages.Where(pg => WBSSLStore.Web.Util.WBSiteSettings.AllowedBrand(Site).Contains(pg.BrandID.ToString()))
                                      .OrderBy(pg => pg.DisplayOrder).ToList());
        }

        public ActionResult AddPage()
        {
            BindPages();
            return View("Pages");
        }
        [HttpPost]
        public ActionResult AddPage(Pages model)
        {
            if (ModelState.IsValid)
            {


                try
                {
                    AddEditPage(model);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                return RedirectToAction("index");
            }
            BindPages();
            return View("pages", model);
        }

        public ActionResult RemovePage(int ID)
        {
            var pagerepo = DependencyResolver.Current.GetService<IRepository<Pages>>();

            Pages p = pagerepo.FindByID(ID);
            if (p != null && p.SiteID.Equals(Site.ID))
            {
                CMSPage cpage = _repository.Find(cp => cp.PageID == p.ID).FirstOrDefault();
                if (cpage != null)
                {
                    _service.Delete(cpc => cpc.CMSPageID == cpage.ID);
                    _repository.Delete(cpage);
                }
                pagerepo.Delete(p);
                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);

                return RedirectToAction("index");
            }
            return View("Error");
        }

        public ActionResult EditPage(int ID)
        {
            var pagerepo = DependencyResolver.Current.GetService<IRepository<Pages>>();
            Pages p = pagerepo.Find(pg => pg.ID == ID && pg.SiteID == Site.ID).FirstOrDefault();
            if (p != null && p.SiteID.Equals(Site.ID))
            {
                BindPages(p.ParentID);
                return View("Pages", p);
            }
            return View("Error");
        }

        [HttpPost]
        public ActionResult EditPage(Pages model)
        {
            if (ModelState.IsValid)
            {
                AddEditPage(model);
                return RedirectToAction("index");
            }
            BindPages();
            return View("pages", model);
        }
        //
        // GET: /Admin/Content/Details/5

        public ActionResult EditMeta()
        {

            int PageID = Convert.ToInt16(Request.QueryString[SettingConstants.QS_PAGEID]);
            string CurrentLangCode = string.Empty;

            if (PageID > 0)
            {
                using (CurrentSiteSettings CurrentSiteSettings = new CurrentSiteSettings(Site))
                {

                    CurrentLangCode = CurrentSiteSettings.CurrentLangCode;
                }

                if (!string.IsNullOrEmpty(CurrentLangCode))
                {

                    var _Page = DependencyResolver.Current.GetService<IRepository<Pages>>();

                    _viewModel = _repository.Find(cp => cp.PageID == PageID && cp.Language.LangCode == CurrentLangCode && cp.Pages.SiteID == Site.ID).FirstOrDefault();
                    if (_viewModel == null)
                    {
                        int LID = Site.SupportedLanguages.Where(lg => lg.LangCode == CurrentLangCode).FirstOrDefault().ID;
                        _viewModel = new CMSPage() { ID = 0, PageID = PageID, LangID = LID };
                    }
                    return View(_viewModel);

                }

            }
            return View();
        }

        // POST: /Admin/Content/EditMeta

        [HttpPost]
        public ActionResult EditMeta(CMSPage model)
        {
            try
            {
                // TODO: Add insert logic here                
                if (model.ID > 0)
                {
                    _repository.Update(model);
                }
                else
                {
                    _repository.Add(model);
                }
                _unitOfWork.Commit();

                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult PageContent()
        {
            string CurrentLangCode = string.Empty;
            int PageID = Convert.ToInt16(Request.QueryString[SettingConstants.QS_PAGEID]);
            using (CurrentSiteSettings CurrentSiteSettings = new CurrentSiteSettings(Site))
            {

                CurrentLangCode = CurrentSiteSettings.CurrentLangCode;
            }
            CMSPage objCMSPage = _repository.Find(pg => pg.Language.LangCode.Equals(CurrentLangCode, StringComparison.OrdinalIgnoreCase) && pg.PageID == PageID && pg.Pages.SiteID == Site.ID).FirstOrDefault();
            if (objCMSPage == null)
            {
                objCMSPage = new CMSPage() { Description = string.Empty, FooterSection = string.Empty, HeaderSection = string.Empty, ID = 0, Keywords = string.Empty, LangID = Site.SupportedLanguages.Where(lg => lg.LangCode.Equals(CurrentLangCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().ID, PageID = PageID, Title = string.Empty };
                _repository.Add(objCMSPage);
                _unitOfWork.Commit();

                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);

                objCMSPage = _repository.Find(cms => cms.ID == objCMSPage.ID).EagerLoad(cms => cms.Pages).FirstOrDefault();
            }

            ViewBag.PageName = objCMSPage.Pages.Caption;
            ViewBag.Language = Site.SupportedLanguages.Where(lg => lg.LangCode.Equals(CurrentLangCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().LangName;
            BindMailMerge();

            CMSPageContent objContent = _service.Find(cpc => cpc.CMSPageID == objCMSPage.ID).FirstOrDefault();
            if (objContent == null)
            {
                objContent = new CMSPageContent() { ID = 0, CMSPageID = objCMSPage.ID, PageContentKey = "1" };

            }
            return View(objContent);
        }

        private void BindMailMerge()
        {
            ViewBag.MailMergeNames = "";
            ViewBag.MailMergeValues = "";
            var _Product = DependencyResolver.Current.GetService<IRepository<ProductAvailablity>>();
            var _Brand = DependencyResolver.Current.GetService<IRepository<Brand>>();
            List<ProductAvailablity> lstProduct = _Product.Find(pa => pa.SiteID == Site.ID && pa.Product.RecordStatusID != (int)RecordStatus.DELETED && pa.isActive == true).OrderBy(pa => pa.Product.ProductName).ToList();
            string Names = string.Empty, Values = string.Empty;
            if (lstProduct != null && lstProduct.Count > 0)
            {
                foreach (ProductAvailablity pa in lstProduct)
                {
                    Names += pa.Product.ProductName + ";";
                    Values += "[--PRICECONTROL=" + pa.Product.InternalProductCode + "--]" + ";";
                }
            }
            List<Brand> lstBrand = _Brand.Find(br => br.isActive == true).ToList();
            if (lstBrand != null && lstBrand.Count > 0)
            {
                foreach (Brand br in lstBrand)
                {
                    Names += "All-" + br.BrandName + ";";
                    Values += "[--BRANDCONTROL=" + br.ID + "--]" + ";";
                }
            }

            List<CertificateType> lstCert = Enum.GetValues(typeof(CertificateType)).Cast<CertificateType>().ToList();
            foreach (CertificateType item in lstCert)
            {

                Names += item.GetEnumDescription() + ";";
                Values += "[--CERTTYPECONTROL=" + Convert.ToString(Convert.ToInt32(item)) + "--]" + ";";

            }

            Names += "ContactUs Form" + ";";
            Values += "[--CONTACTUS--]" + ";";

            if (Names.EndsWith(";"))
                Names = Names.Substring(0, Names.Length - 1);
            if (Values.EndsWith(";"))
                Values = Values.Substring(0, Values.Length - 1);
            ViewBag.MailMergeNames = Names;
            ViewBag.MailMergeValues = Values;
        }


        [HttpPost]
        public ActionResult PageContent(CMSPageContent model)
        {
            if (ModelState.IsValid)
            {
                if (model.ID > 0)
                {
                    _service.Update(model);
                }
                else
                {
                    _service.Add(model);
                }
                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult ChangeVisibility(int id)
        {
            var _Pages = DependencyResolver.Current.GetService<IRepository<Pages>>();
            Pages objPage = _Pages.Find(pg => pg.ID == id && pg.SiteID == Site.ID).FirstOrDefault();
            if (objPage != null)
            {
                int iStatus = string.IsNullOrEmpty(Request.QueryString["status"]) ? 0 : Convert.ToInt16(Request.QueryString["status"]);
                objPage.PageStatusID = iStatus;
                //objPage.isVisible = !objPage.isVisible;
                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                //Site.Pages = _Pages.Find(pg => pg.SiteID == Site.ID).ToList();
                return Json(true);
            }
            return Json(false);
        }

        private void AddEditPage(Pages model)
        {
            if (model != null)
            {
                model.ParentID = WBHelper.ToInt((string.IsNullOrEmpty(Request.Form["ddlParentID"]) ? "0" : Request.Form["ddlParentID"].Split('|')[0]));
                model.PageStatusID = Convert.ToInt16(Request.Form["ddlPageStatus"]);
                model.SiteID = Site.ID;


                var pagerepo = DependencyResolver.Current.GetService<IRepository<Pages>>();
                if (model.ID == 0)
                {
                    model.DisplayOrder = model.ID;
                    model.BrandID = 99;
                    pagerepo.Add(model);
                    _unitOfWork.Commit();

                    //update display order for newly added page
                    model.DisplayOrder = model.ID;
                    pagerepo.Update(model);
                    _unitOfWork.Commit();
                }
                else
                {
                    var objPage = pagerepo.Find(pg => pg.ID == model.ID && pg.SiteID == Site.ID).FirstOrDefault();
                    if (objPage != null)
                    {
                        objPage.ParentID = model.ParentID;
                        objPage.PageStatusID = model.PageStatusID;
                        objPage.Caption = model.Caption;
                        objPage.slug = model.slug;
                        objPage.URLTargetID = model.URLTargetID;
                        objPage.StartDate = model.StartDate;
                        objPage.EndDate = model.EndDate;

                        pagerepo.Update(objPage);
                        _unitOfWork.Commit();
                    }
                }

                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
            }
        }

        private void BindPages(int id = 0)
        {
            List<Pages> lstPages = Site.Pages.Where(pg => WBSSLStore.Web.Util.WBSiteSettings.AllowedBrand(Site).Contains(pg.BrandID.ToString())).OrderBy(pg => pg.DisplayOrder).ToList();
            List<SelectListItem> lstSelectList = new List<SelectListItem>();
            foreach (Pages page in lstPages)
                lstSelectList.Add(new SelectListItem { Text = page.Caption, Value = page.ID + "|" + page.slug, Selected = page.ID == id });

            ViewBag.ParentID = lstSelectList;
        }

        [HttpPost]
        public int moveup(int ID)
        {
            var pagerepo = DependencyResolver.Current.GetService<IRepository<Pages>>();
            Pages pCurrentPage = pagerepo.Find(pc => pc.ID == ID && pc.SiteID == Site.ID).FirstOrDefault();
            if (pCurrentPage != null)
            {
                Pages pNewPage = pagerepo.Find(pn => pn.DisplayOrder < pCurrentPage.DisplayOrder && pn.SiteID == Site.ID && pn.ParentID == pCurrentPage.ParentID).OrderByDescending(pn => pn.DisplayOrder).FirstOrDefault();

                decimal tmpOrder = pNewPage.DisplayOrder;
                pNewPage.DisplayOrder = pCurrentPage.DisplayOrder;
                pCurrentPage.DisplayOrder = tmpOrder;

                pagerepo.Update(pCurrentPage);
                pagerepo.Update(pNewPage);

                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                return pNewPage.ID;
            }
            return 0;
        }

        [HttpPost]
        public int movedown(int ID)
        {
            var pagerepo = DependencyResolver.Current.GetService<IRepository<Pages>>();
            Pages pCurrentPage = pagerepo.Find(pc => pc.ID == ID && pc.SiteID == Site.ID).FirstOrDefault();
            if (pCurrentPage != null)
            {
                Pages pNewPage = pagerepo.Find(pn => pn.DisplayOrder > pCurrentPage.DisplayOrder && pn.SiteID == Site.ID && pn.ParentID == pCurrentPage.ParentID).OrderBy(pn => pn.DisplayOrder).FirstOrDefault();

                decimal tmpOrder = pNewPage.DisplayOrder;
                pNewPage.DisplayOrder = pCurrentPage.DisplayOrder;
                pCurrentPage.DisplayOrder = tmpOrder;

                pagerepo.Update(pCurrentPage);
                pagerepo.Update(pNewPage);

                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                return pNewPage.ID;
            }
            return 0;
        }
    }
}
