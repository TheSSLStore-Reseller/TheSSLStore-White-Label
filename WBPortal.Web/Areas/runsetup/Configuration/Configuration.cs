namespace WhiteBrandShrink.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Xml.Linq;
    using WBSSLStore.Data;
    using WBSSLStore.Domain;
    internal sealed class Configuration : DbMigrationsConfiguration<WBSSLStoreDb>
    {
        public Configuration(System.Data.Entity.Infrastructure.DbConnectionInfo pTargetDatabase)
        {
            AutomaticMigrationsEnabled = true;
            TargetDatabase = pTargetDatabase;
        }

        protected override void Seed(WBSSLStoreDb context)
        {
            using (WBSSLStoreDb db = new WBSSLStoreDb())
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
                    WebServerType objWbServer = new WebServerType();
                    objWbServer.BrandID = 1;
                    objWbServer.isActive = true;
                    objWbServer.WebServerName = "MicroSoft IIS";
                    db.WebServerTypes.Add(objWbServer);
                    db.SaveChanges();

                    //WBSSLStore.Web.Helpers.Caching.SiteCacher.GetSite(obj.ID);

                    //xdoc = System.Xml.Linq.XDocument.Load(AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\ProductsDetail.xml");
                    //if (!db.ProductDetail.Any(x => x.ID > 0))
                    //{
                    //    var _productdetails = (from lv1 in xdoc.Descendants("Product")
                    //                           select new ProductDetail
                    //                           {
                    //                               productcode = lv1.Descendants("productcode").FirstOrDefault().Value.Replace(" ", ""),
                    //                               CertName = lv1.Descendants("CertName").FirstOrDefault().Value.Replace(" ", ""),
                    //                               URL = lv1.Descendants("URL").FirstOrDefault().Value.Replace(" ", ""),
                    //                               MobileFriendly = Convert.ToBoolean(lv1.Descendants("MobileFriendly").FirstOrDefault().Value.Replace(" ", "")),
                    //                               InstantDelivery = Convert.ToBoolean(lv1.Descendants("InstantDelivery").FirstOrDefault().Value.Replace(" ", "")),
                    //                               DocSigning = Convert.ToBoolean(lv1.Descendants("DocSigning").FirstOrDefault().Value.Replace(" ", "")),
                    //                               ScanProduct = Convert.ToBoolean(lv1.Descendants("ScanProduct").FirstOrDefault().Value.Replace(" ", "")),
                    //                               BusinessValid = Convert.ToBoolean(lv1.Descendants("BusinessValid").FirstOrDefault().Value.Replace(" ", "")),
                    //                               SanSupport = Convert.ToBoolean(lv1.Descendants("SanSupport").FirstOrDefault().Value.Replace(" ", "")),
                    //                               WildcardSupport = Convert.ToBoolean(lv1.Descendants("WildcardSupport").FirstOrDefault().Value.Replace(" ", "")),
                    //                               GreenBar = Convert.ToBoolean(lv1.Descendants("GreenBar").FirstOrDefault().Value.Replace(" ", "")),
                    //                               IssuanceTime = lv1.Descendants("IssuanceTime").FirstOrDefault().Value.Replace(" ", ""),
                    //                               Warranty = lv1.Descendants("Warranty").FirstOrDefault().Value.Replace(" ", ""),
                    //                               SiteSeal = lv1.Descendants("SiteSeal").FirstOrDefault().Value.Replace(" ", ""),
                    //                               StarRating = lv1.Descendants("StarRating").FirstOrDefault().Value.Replace(" ", ""),
                    //                               SealInSearch = Convert.ToBoolean(lv1.Descendants("SealInSearch").FirstOrDefault().Value.Replace(" ", "")),
                    //                               VulnerabilityAssessment = Convert.ToBoolean(lv1.Descendants("VulnerabilityAssessment").FirstOrDefault().Value.Replace(" ", "")),
                    //                               ValidationType = lv1.Descendants("ValidationType").FirstOrDefault().Value.Replace(" ", ""),
                    //                               ServerLicense = lv1.Descendants("ServerLicense").FirstOrDefault().Value.Replace(" ", ""),
                    //                               ShortDesc = lv1.Descendants("ShortDesc").FirstOrDefault().Value.Replace(" ", ""),
                    //                               LongDesc = lv1.Descendants("LongDesc").FirstOrDefault().Value.Replace(" ", ""),
                    //                               ProductDatasheetUrl = lv1.Descendants("ProductDatasheetUrl").FirstOrDefault().Value.Replace(" ", ""),
                    //                               VideoUrl = lv1.Descendants("VideoUrl").FirstOrDefault().Value.Replace(" ", ""),
                    //                               SimilarProducts = lv1.Descendants("SimilarProducts").FirstOrDefault().Value.Replace(" ", ""),
                    //                               SealType = lv1.Descendants("SealType").FirstOrDefault().Value.Replace(" ", "")

                    //                           }).ToArray();

                    //    db.ProductDetail.AddOrUpdate(_productdetails);
                    //    db.SaveChanges();
                    //}
                }

            }
        }

    }
}
