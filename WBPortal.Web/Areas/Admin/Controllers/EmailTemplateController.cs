using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class EmailTemplateController : WBController<EmailTemplates, IRepository<EmailTemplates>, ISiteService>
    {
        //
        // GET: /Admin/EmailTemplate/

        public ActionResult Index()
        {
            return View();
        }
        //
        // GET: /Admin/EmailTemplate/Edit/5

        public ActionResult Edit(int emailtypeid)
        {
            string CurrentLangCode = string.Empty;

            SiteSettings s = Site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITELANGUAGE_KEY && o.SiteID == Site.ID).FirstOrDefault();
            if (s != null)
            {
                CurrentLangCode = s.Value;
            }
            s = null;

            if (emailtypeid.Equals((int)EmailType.RESELLER_CUSTOMER_RENEWAL_30) || emailtypeid.Equals((int)EmailType.RESELLER_CUSTOMER_RENEWAL_15) || emailtypeid.Equals((int)EmailType.RESELLER_CUSTOMER_RENEWAL_7) || emailtypeid.Equals((int)EmailType.RESELLER_CUSTOMER_RENEWAL_3))
            {
                s = Site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CUSTOM_EMAILID_RESELLER_CUSTOMERS_RENEWALS_NOTIFICATION && o.SiteID == Site.ID).FirstOrDefault();
                ViewBag.CustomRenewalEmailNoti = s != null && s.Value.ToLower().Equals("true");
            }

            Language objLang = Site.SupportedLanguages.Where(ln => ln.LangCode.Equals(CurrentLangCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            EmailTemplates objTemplate = _repository.Find(et => et.EmailTypeId == emailtypeid && et.SiteID == Site.ID && et.LangID == objLang.ID).FirstOrDefault();
            if (objTemplate == null)
                objTemplate = new EmailTemplates() { SiteID = Site.ID, isActive = true, LangID = objLang.ID, EmailTypeId = emailtypeid };
            if (!objLang.LangCode.ToLower().Equals(WBSSLStore.Domain.SettingConstants.DEFAULT_LANGUAGE_CODE))
            {
                Language objDefultLang = Site.SupportedLanguages.Where(dln => dln.LangCode.ToLower().Equals(WBSSLStore.Domain.SettingConstants.DEFAULT_LANGUAGE_CODE)).FirstOrDefault();
                var objDefultLangTamplate = _repository.Find(et => et.EmailTypeId == emailtypeid && et.SiteID == Site.ID && et.LangID == objDefultLang.ID).FirstOrDefault();
                ViewBag.MailMergeNames = objDefultLangTamplate.MailMerge;
                ViewBag.MailMergeValues = objDefultLangTamplate.MailMerge;
            }
            else
            {
                ViewBag.MailMergeNames = objTemplate.MailMerge;
                ViewBag.MailMergeValues = objTemplate.MailMerge;
            }
            return View(objTemplate);
        }

        //
        // POST: /Admin/EmailTemplate/Edit/5

        [HttpPost]
        public ActionResult Edit(EmailTemplates collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (collection.ID > 0)
                        _repository.Update(collection);
                    else
                        _repository.Add(collection);
                    _unitOfWork.Commit();
                    return RedirectToAction("Index");
                }
                else
                    return View(collection);
            }
            catch
            {
                return View(collection);
            }
        }


    }
}
