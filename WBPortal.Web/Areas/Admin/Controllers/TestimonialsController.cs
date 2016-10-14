using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.PagedList;
using WBSSLStore.Web.Util;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    public class TestimonialsController : WBController<Testimonials, IRepository<Testimonials>, IEmailQueueService>
    {
        public ViewResult Index(int? page)
        {
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();
            var Testimaonial = _repository.Find(t => t.SiteID == Site.ID && t.RecordStatusID != (int)TestimonialStatus.DELETED).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));
            Testimaonial.ActionName = "index";
            Testimaonial.ControllerName = "testimonials";

            if (!PagingInputValidator.IsPagingInputValid(ref page, Testimaonial))
                return View();
            return View(Testimaonial);
        }

        public ActionResult Edit(int id)
        {
            var testimonial = _repository.Find(t => t.ID == id && t.SiteID == Site.ID).FirstOrDefault();
            var select = Enum.GetValues(typeof(TestimonialStatus)).Cast<TestimonialStatus>().Select(x => new SelectListItem { Text = ((int)x == (int)TestimonialStatus.ACTIVE || (int)x == (int)TestimonialStatus.SHOWINPAGE ? WBSSLStore.Resources.GeneralMessage.Message.ddlStatus_Active : WBSSLStore.Resources.GeneralMessage.Message.ddlStatus_InActive), Value = ((int)x).ToString(), Selected = true }).Where(x => Convert.ToInt32(x.Value) != (int)TestimonialStatus.DELETED && Convert.ToInt32(x.Value) != (int)TestimonialStatus.SHOWINPAGE);
            if (testimonial.RecordStatusID == (int)TestimonialStatus.SHOWINPAGE)
                ViewBag.IsTestimonials = true;
            else
                ViewBag.IsTestimonials = false;
            //testimonial.RecordStatusID == (int)TestimonialStatus.ACTIVE
            //var selectlist = new SelectList(select,   );

            return View(testimonial);
        }

        [HttpPost]
        public ActionResult Edit(Testimonials testimonials)
        {
            if (ModelState.IsValid)
            {
                var ask = Request.Form["chkTestimonials"];
                if (Convert.ToBoolean(Request.Form["chkTestimonials"]))
                {
                    testimonials.RecordStatusID = (int)TestimonialStatus.SHOWINPAGE;
                }
                else
                {
                    testimonials.RecordStatusID = testimonials.RecordStatusID;
                }

                _repository.Update(testimonials);
                _unitOfWork.Commit();
                return RedirectToAction("index", "testimonials");
            }
            return View(testimonials);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            Testimonials objTestm = _repository.Find(t => t.ID == id && t.SiteID == Site.ID).FirstOrDefault();
            if (objTestm != null)
            {
                objTestm.RecordStatus = (TestimonialStatus)Convert.ToInt16(Request.QueryString["s"]);
                //objTestm.RecordStatus = (objTestm.RecordStatus == TestimonialStatus.ACTIVE ? TestimonialStatus.INACTIVE : (objTestm.RecordStatus == TestimonialStatus.SHOWINPAGE ? TestimonialStatus.ACTIVE :  ));
                _repository.Update(objTestm);
                _unitOfWork.Commit();
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult changestatuspage(int id) 
        {
            
            Testimonials objTestm = _repository.Find(t => t.ID == id && t.SiteID == Site.ID).FirstOrDefault();
            if (objTestm != null)
            {
                objTestm.RecordStatus = (Convert.ToBoolean(Request.QueryString["s"]) ? TestimonialStatus.SHOWINPAGE : TestimonialStatus.ACTIVE);
                _repository.Update(objTestm);
                _unitOfWork.Commit();
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            Testimonials objTestm = _repository.Find(t => t.ID == id && t.SiteID == Site.ID).FirstOrDefault();
            if (objTestm != null)
            {
                objTestm.RecordStatus = TestimonialStatus.DELETED;
                _repository.Update(objTestm);
                _unitOfWork.Commit();
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }


        public ActionResult Create()
        {

            _viewModel.Site = Site;

            return View("Edit", _viewModel);
        }

        //// POST: /Admin/Reseller/Create
        [HttpPost]
        public ActionResult Create(Testimonials testimonial)
        {
            if (ModelState.IsValid)
            {
                testimonial.SiteID = Site.ID;

                if (AddEdit(testimonial))
                    return RedirectToAction("index");
            }
            return View("Edit", testimonial);
        }

        public bool AddEdit(Testimonials testimonial)
        {
            try
            {

                var isexist = _repository.Find(t => t.SiteID == Site.ID && t.Signature == testimonial.Signature).FirstOrDefault();
                if (isexist != null)
                {
                    ViewBag.IsUserExist = true;
                    //return false;
                }
                else
                    ViewBag.IsUserExist = false;

                testimonial.Signature = testimonial.Signature;
                testimonial.SiteID = Site.ID;
                testimonial.Description = testimonial.Description;

                if (Convert.ToBoolean(Request.Form["chkTestimonials"]))
                {
                    testimonial.RecordStatusID = (int)TestimonialStatus.SHOWINPAGE;
                }
                else
                {
                    testimonial.RecordStatusID = testimonial.RecordStatusID;
                }


                _repository.Add(testimonial);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception exc)
            {
                ViewBag.ErrMsg = exc.Message.ToString();
                return false;
            }
        }
    }
}
