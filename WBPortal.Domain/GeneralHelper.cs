using System;
using System.Collections.Generic;
using System.Linq;

namespace WBSSLStore.Domain
{
    public class GeneralHelper
    {
        public static List<PageUrl> GetProductDetailSlugs()
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\pageconfiguration.xml";

            if (System.IO.File.Exists(path))
            {

                System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load(path);
                return (from lv1 in xdoc.Descendants("ProductPage")
                        select new PageUrl
                        {
                            Description = lv1.Descendants("Description").FirstOrDefault() != null ? lv1.Descendants("Description").FirstOrDefault().Value.Replace("\n", "").Replace("\r", "").Trim() : "",
                            Keywords = lv1.Descendants("Keywords").FirstOrDefault() != null ? lv1.Descendants("Keywords").FirstOrDefault().Value.Replace("\n", "").Replace("\r", "").Trim() : "",
                            SlugUrl = lv1.Descendants("InternalProductCode").Select(x => x.Attribute("data-url").Value.Replace("\n", "").Replace("\r", "").Trim()).FirstOrDefault(),
                            ProductCode = lv1.Descendants("InternalProductCode").FirstOrDefault() != null ? lv1.Descendants("InternalProductCode").FirstOrDefault().Value.Replace("\n", "").Replace("\r", "").Trim() : "",
                            Title = lv1.Descendants("Title").FirstOrDefault() != null ? lv1.Descendants("Title").FirstOrDefault().Value.Replace("\n", "").Replace("\r", "").Trim() : ""
                        }).ToList();
            }
            else
                return null;

        }

        public static List<ProductDetail> GetProductDetailsData()
        {

            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\ProductsDetail.xml";

            if (System.IO.File.Exists(path))
            {

                System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load(path);
                return (from lv1 in xdoc.Descendants("Product")
                        select new ProductDetail()
                        {
                            productcode = lv1.Descendants("ProductCode").FirstOrDefault().Value.Replace(" ", ""),
                            CertName = lv1.Descendants("ProductName").FirstOrDefault().Value.Replace(" ", ""),
                            URL = lv1.Descendants("URL").FirstOrDefault() != null ? lv1.Descendants("URL").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            MobileFriendly = Convert.ToBoolean(lv1.Descendants("MobileFriendly").FirstOrDefault().Value.Replace(" ", "")),
                            InstantDelivery = Convert.ToBoolean(lv1.Descendants("InstantDelivery").FirstOrDefault().Value.Replace(" ", "")),
                            DocSigning = Convert.ToBoolean(lv1.Descendants("DocSigning").FirstOrDefault().Value.Replace(" ", "")),
                            ScanProduct = Convert.ToBoolean(lv1.Descendants("ScanProduct").FirstOrDefault().Value.Replace(" ", "")),
                            BusinessValid = Convert.ToBoolean(lv1.Descendants("BusinessValid").FirstOrDefault().Value.Replace(" ", "")),
                            SanSupport = Convert.ToBoolean(lv1.Descendants("SanSupport").FirstOrDefault().Value.Replace(" ", "")),
                            WildcardSupport = Convert.ToBoolean(lv1.Descendants("WildcardSupport").FirstOrDefault().Value.Replace(" ", "")),
                            GreenBar = Convert.ToBoolean(lv1.Descendants("GreenBar").FirstOrDefault().Value.Replace(" ", "")),
                            IssuanceTime = lv1.Descendants("IssuanceTime").FirstOrDefault().Value.Replace(" ", ""),
                            Warranty = lv1.Descendants("Warranty").FirstOrDefault().Value.Replace(" ", ""),
                            SiteSeal = lv1.Descendants("SiteSeal").FirstOrDefault().Value.Replace(" ", ""),
                            StarRating = lv1.Descendants("StarRating").FirstOrDefault().Value.Replace(" ", ""),
                            SealInSearch = Convert.ToBoolean(lv1.Descendants("SealInSearch").FirstOrDefault().Value.Replace(" ", "")),
                            VulnerabilityAssessment = Convert.ToBoolean(lv1.Descendants("VulnerabilityAssessment").FirstOrDefault().Value.Replace(" ", "")),
                            ValidationType = lv1.Descendants("ValidationType").FirstOrDefault().Value.Replace(" ", ""),
                            ServerLicense = lv1.Descendants("ServerLicense").FirstOrDefault().Value.Replace(" ", ""),
                            ShortDesc = lv1.Descendants("ShortDesc").FirstOrDefault() != null ? lv1.Descendants("ShortDesc").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            LongDesc = lv1.Descendants("LongDesc").FirstOrDefault() != null ? lv1.Descendants("LongDesc").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            ProductDatasheetUrl = lv1.Descendants("ProductDatasheetUrl").FirstOrDefault() != null ? lv1.Descendants("ProductDatasheetUrl").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            VideoUrl = lv1.Descendants("VideoUrl").FirstOrDefault() != null ? lv1.Descendants("VideoUrl").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            SimilarProducts = lv1.Descendants("SimilarProducts").FirstOrDefault() != null ? lv1.Descendants("SimilarProducts").FirstOrDefault().Value.Replace(" ", "") : string.Empty,
                            SealType = lv1.Descendants("SealType").FirstOrDefault() != null ? lv1.Descendants("SealType").FirstOrDefault().Value.Replace(" ", "") : string.Empty

                        }).ToList();


            }
            else
                return null;
        }

    }
    public class PageUrl
    {
        public string SlugUrl { get; set; }
        public string ProductCode { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
    }
}
