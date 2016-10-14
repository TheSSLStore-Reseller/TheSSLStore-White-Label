using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Logger;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;

namespace WBSSLStore.Web.Controllers
{
    public class StreamOneController : WBController<User, IRepository<User>, ISiteService>
    {
        //
        // GET: /StreamOne/
        [HttpGet]
        public ActionResult getInfo(int? id)
        {
            return Content("Invalid URL", "text/html");
        }
        [HttpPost]
        public ActionResult getInfo()
        {
            StringBuilder sbResponse = new StringBuilder("<status>[Status]</status><url>[LendingURL]</url><external_reseller_id>[CustomeID]</external_reseller_id>");
            try
            {
                string ProvisionData = Request.Form["provision_data"];

                if (!string.IsNullOrEmpty(ProvisionData))
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    SOData data = serializer.Deserialize<SOData>(ProvisionData);
                    if (data != null)
                    {

                    }
                }
                else
                {
                    sbResponse.Replace("[Status]", "failed");
                    sbResponse.Replace("[LendingURL]", string.Empty);
                    sbResponse.Replace("[CustomeID]", string.Empty);

                    _logger.Log("Empty data posted from StreamOne", LogType.INFO);
                }
            }
            catch (Exception ex)
            {
                sbResponse.Replace("[Status]", "failed");
                sbResponse.Replace("[LendingURL]", string.Empty);
                sbResponse.Replace("[CustomeID]", string.Empty);

                _logger.Log(ex.Message, LogType.ERROR);
            }
            return Content(sbResponse.ToString(), "text/xml");
        }
    }

    public class ResellerInfo
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string reseller_id { get; set; }
        public string external_reseller_id { get; set; }
    }

    public class Item
    {
        public string item_id { get; set; }
        public string sku { get; set; }
        public string quantity { get; set; }
        public string item_description { get; set; }
        public string unit_price { get; set; }
        public string order_date { get; set; }
        public string currency { get; set; }
        public string promo_code { get; set; }
        public string external_id { get; set; }
        public string upgraded_item_id { get; set; }
    }

    public class SOData
    {
        public string store_name { get; set; }
        public string marketplace_country { get; set; }
        public string listing_id { get; set; }
        public string customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string company_website { get; set; }
        public string billing_cycle_day { get; set; }
        public string order_id { get; set; }
        public List<ResellerInfo> reseller_info { get; set; }
        public List<Item> items { get; set; }
    }
}