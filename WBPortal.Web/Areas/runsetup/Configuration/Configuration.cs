using System;
using System.Data.Entity.Migrations;
using System.Linq;
using WBSSLStore.Data;
using WBSSLStore.Domain;

namespace WhiteBrandShrink.Migrations
{

    public class DefaultDataSeed 
    {
      
        public void MySeed(WBSSLStoreDb db)
        {
            // using (WBSSLStoreDb db = new WBSSLStoreDb())
            {
                System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load(AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\coutrywb.xml");
                if (!db.Countries.Any(x => x.ID > 0))
                {
                    var lv1s = (from lv1 in xdoc.Descendants("Row")
                                select new Country
                                {
                                    CountryName = lv1.Descendants("Data").FirstOrDefault().Value.Replace(" ", ""),
                                    ISOName = lv1.Descendants("IsoCode").FirstOrDefault().Value.Replace(" ", ""),
                                    RecordStatus = RecordStatus.ACTIVE,
                                    RecordStatusID = (int)RecordStatus.ACTIVE
                                }).ToArray();

                    db.Countries.AddOrUpdate(lv1s);

                    db.SaveChanges();
                }

                if (!db.Brands.Any(x => x.ID > 0 && x.isActive == true))
                {
                    var lv1s = (from lv1 in xdoc.Descendants("Brand")
                                select new Brand
                                {
                                    BrandName = lv1.Descendants("BrandName").FirstOrDefault().Value.Replace(" ", ""),
                                    ID = Convert.ToInt32(lv1.Descendants("ID").FirstOrDefault().Value),
                                    isActive = true
                                }).ToArray();

                    db.Brands.AddOrUpdate(lv1s);
                    db.SaveChanges();
                }

                if (!db.Languages.Any(x => x.ID > 0))
                {
                    var lv1s = (from lv1 in xdoc.Descendants("Languages")
                                select new Language
                                {
                                    LangName = lv1.Descendants("LangName").FirstOrDefault().Value.Replace(" ", "").Replace("\n", ""),
                                    LangCode = lv1.Descendants("LangCode").FirstOrDefault().Value.Replace(" ", "").Replace("\n", ""),
                                    RecordStatusID = (int)RecordStatus.ACTIVE
                                }).ToArray();

                    db.Languages.AddOrUpdate(lv1s);
                    db.SaveChanges();
                }



                using (db)
                {
                    Site obj = db.Sites.Where(x => x.isActive).ToList().FirstOrDefault();
                    if (obj == null)
                    {
                        obj = new Site();
                        obj.Alias = "";
                        obj.CName = System.Web.HttpContext.Current.Request.Url.Host;
                        obj.DateCreated = DateTime.Now;
                        obj.DateModified = DateTime.Now;
                        obj.isActive = true;
                        obj.APIisInTest = true;
                        obj.APIPartnerCode = "xxx";
                        obj.APIPassword = "xxx";
                        obj.APIUsername = "xxx";
                        obj.APIAuthToken = "xxx";
                        db.Sites.Add(obj);
                        db.SaveChanges();
                    }

                    Audit objAudit = new Audit();
                    objAudit.ByUserID = 0;
                    objAudit.DateCreated = DateTime.Now;
                    objAudit.DateModified = DateTime.Now;
                    objAudit.HttpHeaderDump = "SYSTEM";
                    objAudit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                    db.Audits.Add(objAudit);
                    db.SaveChanges();

                    User objUser = db.Users.Where(x => x.RecordStatusID.Equals((int)RecordStatus.ACTIVE) && x.UserTypeID.Equals((int)UserType.ADMIN)).FirstOrDefault();
                    if (objUser == null)
                    {
                        Address add = new Address();
                        add.City = "St pete";
                        add.CompanyName = objUser != null ? objUser.CompanyName : "system";
                        add.Email = objUser != null ? objUser.Email : "admin@admin.com";
                        add.CountryID = 5;
                        add.Fax = "";
                        add.Mobile = "";
                        add.Phone = "134654632";
                        add.State = "Florida";
                        add.Street = "Stpet";
                        add.Zip = "13245";
                        db.Addresses.Add(add);
                        db.SaveChanges();

                        User u = new User();

                        u.SiteID = obj.ID;
                        u.AddressID = add.ID;
                        u.FirstName = "System";
                        u.LastName = "Admin";
                        u.Email = "admin@admin.com";
                        u.CompanyName = "System";
                        u.AlternativeEmail = "admin@admin.com";
                        u.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                        u.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash("system20167", u.PasswordSalt);
                        u.ConfirmPassword = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash("system20167", u.PasswordSalt);
                        u.RecordStatus = RecordStatus.ACTIVE;
                        u.RecordStatusID = (int)RecordStatus.ACTIVE;
                        u.UserTypeID = (int)UserType.ADMIN;
                        u.UserType = UserType.ADMIN;
                        u.AuditDetails = new Audit();
                        u.AuditID = u.AuditDetails.ID;
                        u.AuditDetails.ByUserID = 0;
                        u.AuditDetails.DateCreated = DateTime.Now;
                        u.AuditDetails.DateModified = DateTime.Now;
                        u.AuditDetails.HttpHeaderDump = "SYSTEM";
                        u.AuditDetails.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                        db.Users.Add(u);
                        db.SaveChanges();
                    }

                   
                   
                    if (!db.EmailTemplateses.Any(x => x.ID > 0))
                    {

                        System.Xml.Linq.XDocument xdocEmail = System.Xml.Linq.XDocument.Load(AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\Emailconfiguration.xml");
                        var lstEmailTemp = (from lv1 in xdocEmail.Descendants("EmailTemplate")
                                    select new EmailTemplates
                                    {
                                        BCC = "",
                                        CC = "",
                                        EmailContent = (lv1.Descendants("EmailContent").FirstOrDefault() != null ? lv1.Descendants("EmailContent").FirstOrDefault().Value.Trim() : ""),
                                        EmailSubject = (lv1.Descendants("EmailSubject").FirstOrDefault() != null ? lv1.Descendants("EmailSubject").FirstOrDefault().Value.Trim() : ""),
                                        EmailTypeId = Convert.ToInt32((lv1.Descendants("EmailContent").Select(x => x.Attribute("EmailTypeId").Value.Replace("\n", "").Replace("\r", "").Trim()).FirstOrDefault())),
                                        From = "support@domain.com",
                                        isActive = true,
                                        LangID = Convert.ToInt32((lv1.Descendants("EmailContent").Select(x => x.Attribute("LangID").Value.Replace("\n", "").Replace("\r", "").Trim()).FirstOrDefault())),
                                        MailMerge = (lv1.Descendants("EmailContent").Select(x => x.Attribute("MailMerge").Value.Replace("\n", "").Replace("\r", "").Trim()).FirstOrDefault()),
                                        ReplyTo = "",
                                        SiteID = obj.ID
                                    }).ToArray();

                        db.EmailTemplateses.AddOrUpdate(lstEmailTemp); 
                        db.SaveChanges();
                    }
                    WebServerType objWbServer = new WebServerType();
                    objWbServer.BrandID = 1;
                    objWbServer.isActive = true;
                    objWbServer.WebServerName = "MicroSoft IIS";
                    db.WebServerTypes.Add(objWbServer);
                    db.SaveChanges();

                   
                }

            }
        }

    }
}
