using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Gateways.RestAPIModels.Response;
using WBSSLStore.Gateways.RestAPIModels.Request;
using WBSSLStore.Gateways.RestAPIModels.Helpers;
using System.Web.Script.Serialization;
using System.Collections;


namespace WBSSLStore.Gateways.RestAPIModels.Services
{
    public class OrderService
    {
        public static OrderResponse RefundRequest(OrderRequest orderdetails)
        {
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/refundrequest/");

            orderResponse = new JavaScriptSerializer().Deserialize<OrderResponse>(strResponse);

            return orderResponse;
        }

        public static OrderResponse RefundRequestStatus(OrderRequest orderdetails)
        {
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/refundstatus/");

            orderResponse = new JavaScriptSerializer().Deserialize<OrderResponse>(strResponse);

            return orderResponse;
        }

        public static OrderResponse OrderStatus(OrderRequest orderdetails)
        {
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[CUSTORDID]", orderdetails.CustomOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/status/");

            orderResponse = new JavaScriptSerializer().Deserialize<OrderResponse>(strResponse);

            return orderResponse;
        }
        public static List<OrderResponse> GetOrderDeatilsByDateRange(OrderQueryRequest orderdetails)
        {
            List<OrderResponse> orderResponse = new List<OrderResponse>();
            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.QueryOrderRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
             TimeSpan tspan = orderdetails.EndDate.Value - (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            sbRequest.Replace("[STARTDATE]",  (UnixTicks(orderdetails.StartDate.Value)).ToString().Remove(UnixTicks(orderdetails.StartDate.Value).ToString().IndexOf('.')).ToString());
            sbRequest.Replace("[ENDDATE]", (UnixTicks(orderdetails.EndDate.Value)).ToString().Remove(UnixTicks(orderdetails.EndDate.Value).ToString().IndexOf('.')).ToString());
            sbRequest.Replace("[SUBUSERID]", orderdetails.SubUserID);
            sbRequest.Replace("[PRDCODE]", ApiHelper.GetApiProductCode (orderdetails.ProductCode));

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/query/");

            orderResponse = new JavaScriptSerializer().Deserialize<List<OrderResponse>>(strResponse);

            return orderResponse;
        }

        private static long ToUnixTimespan(DateTime date)
        {
            TimeSpan tspan = date.ToUniversalTime().Subtract(
               new DateTime(1970, 1, 1, 0, 0, 0));

            return (long)Math.Truncate(tspan.TotalSeconds);
        }
        public static double UnixTicks(DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static OrderResponse InviteOrderRequest(TinyOrderRequest orderdetails)
        {
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.InviteOrderRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[PREFERLINK]", orderdetails.PreferVendorLink ? "true" : "false");
            sbRequest.Replace("[CUSTORDID]", orderdetails.CustomOrderID);
            sbRequest.Replace("[PRODCODE]", ApiHelper.GetApiProductCode ( orderdetails.ProductCode));
            sbRequest.Replace("[EXTRAPRODCODE]", orderdetails.ExtraProductCode);
            sbRequest.Replace("[VALIDPERIOD]",orderdetails.ValidityPeriod.ToString());
            sbRequest.Replace("[SERVERCOUNT]",orderdetails.ServerCount.Equals(0) ? "1" : orderdetails.ServerCount.ToString());
            sbRequest.Replace("[REQUESTOREMAIL]", orderdetails.RequestorEmail);
            sbRequest.Replace("[EXTRASAN]", orderdetails.ExtraSAN.ToString());
            sbRequest.Replace("[ADDINSTALLATIONSUPPORT]", orderdetails.AddInstallationSupport ? "true" : "false");
            sbRequest.Replace("[LANGUAGE]", orderdetails.EmailLanguageCode);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/inviteorder/");

            orderResponse = new JavaScriptSerializer().Deserialize<OrderResponse>(strResponse);

            return orderResponse;
        }
       public static CSRResponse CheckCSRDetail(CSRRequest csrdetails)
        {
            CSRResponse Response = new CSRResponse(); 

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.CheckCSRRequst());

            sbRequest.Replace("[PARTNERCODE]", csrdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", csrdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", csrdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", csrdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[PRODUCTCODE]", !string.IsNullOrEmpty(csrdetails.ProductCode) ? csrdetails.ProductCode : "rapidssl");
            sbRequest.Replace("[CSRCONTENT]",  System.Web.HttpUtility.UrlEncode(csrdetails.CSR));

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "csr");

            Response = new JavaScriptSerializer().Deserialize<CSRResponse>(strResponse);

            return Response;
        }

        public static DownloadCertificateResponse DownloadCertificateRequest(OrderRequest orderdetails)
        {
            DownloadCertificateResponse orderResponse = new DownloadCertificateResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[CUSTORDID]", orderdetails.CustomOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/download/");
            orderResponse = new JavaScriptSerializer().Deserialize<DownloadCertificateResponse>(strResponse);
            
            return orderResponse;
        }

        public static DownloadCertificateZipResponse DownloadCertificateZipResponse(OrderRequest orderdetails) 
        {
            DownloadCertificateZipResponse orderResponse = new DownloadCertificateZipResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[CUSTORDID]", orderdetails.CustomOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/downloadaszip/"); //downloadaszip
            
            Dictionary<string, object> values = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(strResponse);
            orderResponse.Zip = values["Zip"].ToString();
            //orderResponse = new JavaScriptSerializer().Deserialize<DownloadCertificateZipResponse>(strResponse);
            return orderResponse;
        }
      
        public static GetAllPricingResponse GetAllProductPricing(string PartnerCode, string APIAuthToken)
        {
            GetAllPricingResponse objResponse = new GetAllPricingResponse();
            try
            {
                List<ProductResponse> allPrice = new List<ProductResponse>();
                List<ProductPricing> Allpricing = new List<ProductPricing>();
                StringBuilder sbRequest = new StringBuilder();
                sbRequest.Append(ApiHelper.ProductRequst());

                sbRequest = sbRequest.Replace("[PARTNERCODE]", PartnerCode);
                sbRequest = sbRequest.Replace("[AUTHCODE]", APIAuthToken);
                sbRequest = sbRequest.Replace("[REPLAYTOKEN]", System.Web.HttpContext.Current.Request.Url.DnsSafeHost);
                sbRequest = sbRequest.Replace("[USERAGENT]", "SSL Store WhiteBrand Sites");
                sbRequest = sbRequest.Replace("[PRDTYPE]", "0");
                sbRequest = sbRequest.Replace("[PRDCODE]", string.Empty);
                string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "product/query/");

                allPrice = new JavaScriptSerializer().Deserialize<List<ProductResponse>>(strResponse).ToList();


                List<ProductPricing> lsttempprice = new List<ProductPricing>();
                if (allPrice != null && allPrice.Count > 0)
                {
                    for (int i = 0; i < allPrice.Count; i++)
                    {
                        try
                        {
                            var apiData =  allPrice[i] ;
                            ALLProduct product = new ALLProduct();

                            product.ProductCode = ApiHelper.GetInternalProductCode(apiData.ProductCode);
                            product.ProductName = apiData.ProductName;

                            product.Brand = apiData.VendorName;
                            product.CanbeReissued = apiData.CanbeReissued;
                            product.IsCompetitiveUpgradeSupported = apiData.IsCompetitiveUpgradeSupported;
                            product.isNoOfServerFree = apiData.isNoOfServerFree;
                            product.IsSanEnable = apiData.IsSanEnable;
                            product.IsWildCardProduct = apiData.isWlidcard;
                            product.ProductType = (int)apiData.ProductType;
                            product.RefundDays = 30;
                            product.ReissueDays = apiData.ReissueDays;
                            product.SanMax = 24;
                            product.SanMin = 4;
                            lsttempprice = apiData.ProductCode.Equals("freessl",StringComparison.OrdinalIgnoreCase) ? apiData.PricingInfo : apiData.PricingInfo.Where(x => x.NumberOfMonths > 11).ToList();
                            for (int j = 0; j < lsttempprice.Count; j++)
                            {
                                var apiPrice = lsttempprice[j];
                               APIPricing price = new APIPricing();
                                price.NumberOfMonth = apiPrice.NumberOfMonths;
                                price.Price = apiPrice.Price;
                                price.SRP = apiPrice.SRP;
                                price.AdditionalSanPrice = apiPrice.PricePerAdditionalSAN;
                                product.Pricings.Add(price);
                            }
                            product.Pricings.OrderBy(p => p.NumberOfMonth);
                            objResponse.Product.Add(product);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    objResponse.Product.OrderBy(p => p.ProductName);
                }
                else
                {
                    objResponse.ErrorCode = "-99999";
                    objResponse.ErrorMessage = "No Product Available.";
                }



            }
            catch (Exception ex)
            {
                objResponse.ErrorCode = "-99999";
                objResponse.ErrorMessage = ex.Message;
                //serviceclient.Dispose();

            }

            return objResponse;
        }

        public static string ResendEmail(OrderRequest orderdetails)
        {
            AuthResponse auth = new AuthResponse();
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.GetRequestRefundRequst());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[SSLSTOREOID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[CUSTORDID]", orderdetails.CustomOrderID);
            sbRequest.Replace("[REFUNDREASON]", orderdetails.RefundReason);
            sbRequest.Replace("[REFUNDREQUESTID]", orderdetails.RefundRequestID);
            sbRequest.Replace("[REEMAILTYPE]", "ApproverEmail");
            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/resend/");
            auth = new JavaScriptSerializer().Deserialize<AuthResponse>(strResponse);
            if (auth.isError)
                return auth.Message[0];
            else
                return "Approval Email Resend Successfully.";


        }

        public static OrderResponse ReissueCerts(ReissueOrderRequest orderdetails)
        {
          
            OrderResponse orderResponse = new OrderResponse();

            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(ApiHelper.ReissuedOrder());

            sbRequest.Replace("[PARTNERCODE]", orderdetails.AuthRequest.PartnerCode);
            sbRequest.Replace("[AUTHCODE]", orderdetails.AuthRequest.AuthToken);
            sbRequest.Replace("[REPLAYTOKEN]", orderdetails.AuthRequest.ReplayToken);
            sbRequest.Replace("[USERAGENT]", orderdetails.AuthRequest.UserAgent);
            sbRequest.Replace("[PARTNEROID]", orderdetails.TheSSLStoreOrderID);
            sbRequest.Replace("[CSR]", orderdetails.CSR);
            sbRequest.Replace("[WEBSERVER]", orderdetails.WebServerType);
            int i = 1;
            if (orderdetails.DNSNames != null && orderdetails.DNSNames.Length > 0)
            {
                foreach (string str in orderdetails.DNSNames)
                {
                    sbRequest.Replace("[DNS" + i + "]", !string.IsNullOrEmpty(str) ? str : string.Empty);
                    i++;
                }
            }
            else
            {
                sbRequest.Replace("[DNS1]",  string.Empty);
            }
            sbRequest.Replace("[ISRENEWALORDER]", orderdetails.isRenewalOrder ? "true" : "false");
            sbRequest.Replace("[SPECINST]", orderdetails.SpecialInstructions);

            #region EditSAN
            if (orderdetails.EditSAN != null && orderdetails.EditSAN.Length > 0)
            {
                int i1 = 1;
                StringBuilder sedit = new StringBuilder();
                foreach (Pair values in orderdetails.EditSAN)
                {
                    if (values != null)
                    {
                        sedit.Append("{\"OldValue\":" + "\"" + (!string.IsNullOrEmpty(values.OldValue) ? values.OldValue : string.Empty) + "\",");
                        sedit.Append("\"NewValue\":" + "\"" + (!string.IsNullOrEmpty(values.NewValue) ? values.NewValue : string.Empty) + "\"},");
                        i1++;
                    }
                    else
                        break;
                }
                sbRequest.Replace("[EDIT]", sedit != null && !string.IsNullOrEmpty(sedit.ToString()) ? sedit.ToString().Remove(sedit.ToString().Length - 1, 1) : string.Empty);
            }
            #endregion

            #region DeleteSAN
            if (orderdetails.DeleteSAN != null && orderdetails.DeleteSAN.Length > 0)
            {
                int i2 = 1;
                StringBuilder sdelete = new StringBuilder();
                foreach (Pair values in orderdetails.DeleteSAN)
                {
                    if (values != null)
                    {
                        sdelete.Append("{\"OldValue\":" + "\"" + (!string.IsNullOrEmpty(values.OldValue) ? values.OldValue : string.Empty) + "\",");
                        sdelete.Append("\"NewValue\":" + "\"" + string.Empty + "\"},");
                        i2++;
                    }
                    else
                        break;
                }
                sbRequest.Replace("[DELETE]", !string.IsNullOrEmpty(sdelete.ToString()) ? sdelete.ToString().Remove(sdelete.ToString().Length - 1, 1) : string.Empty);
            }
            #endregion

            #region ADDSAN
            if (orderdetails.AddSAN != null && orderdetails.AddSAN.Length > 0)
            {
                int i3 = 1;
                StringBuilder sadd = new StringBuilder();
                foreach (Pair values in orderdetails.AddSAN)
                {
                    if (values != null)
                    {
                        sadd.Append("{\"OldValue\":" + "\"" +  string.Empty + "\",");
                        sadd.Append("\"NewValue\":" + "\"" + (!string.IsNullOrEmpty(values.NewValue) ? values.NewValue : string.Empty) + "\"},");
                        i3++;
                    }
                    else
                        break;
                }
                sbRequest.Replace("[ADD]", !string.IsNullOrEmpty(sadd.ToString()) ? sadd.ToString().Remove(sadd.ToString().Length - 1, 1) : string.Empty);
            }
            #endregion

            sbRequest.Replace("[ISWILDCARD]", orderdetails.isWildCard ? "true" : "false");
            sbRequest.Replace("[REISSUEEMAIL]", orderdetails.ReissueEmail);

            string strResponse = ApiHelper.GetResponseFromAPI(sbRequest.ToString(), "order/reissue/");
            orderResponse = new JavaScriptSerializer().Deserialize<OrderResponse>(strResponse);
            //if (orderResponse.AuthResponse.isError)
            //    return false;
            //else
            //    return true;
            return orderResponse;
        }
    }
}
