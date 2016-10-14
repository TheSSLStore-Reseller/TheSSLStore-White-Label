using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace WBSSLStore.Gateway.AuthorizeNET
{
    public class AuthorizeNETHandler : PGGatewayHandler
    {
        AuthoriseNetTransaction objAuthorisedNet = null;
        PaymentGatewayInteraction objTranResult = null;


        public AuthorizeNETHandler()
        {

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            objAuthorisedNet = new AuthoriseNetTransaction();
            objTranResult = new PaymentGatewayInteraction();
        }
        ~AuthorizeNETHandler()
        {
            objAuthorisedNet = null;
            objTranResult = null;
        }

        public override PaymentGatewayInteraction ProcessAuth(GatewayInParameter transaction)
        {
            objTranResult.InParams = transaction;
            InitializeAuthNetParamsForCreditCard(transaction);
            GatewayReponse(TransactionType.AUTH, transaction);
            return objTranResult;
        }

        public override PaymentGatewayInteraction ProcessAuthAndCapture(GatewayInParameter transaction)
        {
            objTranResult.InParams = transaction;
            InitializeAuthNetParamsForCreditCard(transaction);
            GatewayReponse(TransactionType.AUTH_CAPTURE, transaction);
            return objTranResult;
        }

        public override PaymentGatewayInteraction ProcessVoidTransaction(GatewayInParameter transaction)
        {
            objTranResult.InParams = transaction;
            InitializeAuthNetParamsForCreditCard(transaction);
            objAuthorisedNet.TransactionReference = transaction.RefTransactionId;
            GatewayReponse(TransactionType.VOID, transaction);
            return objTranResult;
        }

        public override PaymentGatewayInteraction ProcessRecurring(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessECheck(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessRefund(GatewayInParameter transaction)
        {
            objTranResult.InParams = transaction;
            InitializeAuthNetParamsForCreditCard(transaction);
            objAuthorisedNet.TransactionReference = transaction.RefTransactionId;
            GatewayReponse(TransactionType.CREDIT, transaction);
            return objTranResult;
        }
        private void InitializeAuthNetParamsForCreditCard(GatewayInParameter transaction)
        {

            objAuthorisedNet.AuthNetLoginID =  transaction.APILoginID;
            objAuthorisedNet.AuthNetTransKey = transaction.APITransactionKey;
            objAuthorisedNet.Amount = transaction.TransactionAmount;
            objAuthorisedNet.CreditCardNumber = transaction.CCNumber;
            objAuthorisedNet.ExpireMonthYear = transaction.CCExpMonth.ToString() + "/" + transaction.CCExpYear.ToString();
            objAuthorisedNet.CCV = transaction.CVV;
            objAuthorisedNet.sFirstName = transaction.CardFirstName;
            objAuthorisedNet.sLastName = transaction.CardLastName;
            objAuthorisedNet.InvoiceNumber = transaction.InvoiceNumber;

            objAuthorisedNet.sBillAddress = transaction.BillingAddress1 + transaction.BillingAddress2;
            objAuthorisedNet.sBillCity = transaction.City;
            objAuthorisedNet.sBillState = transaction.State;
            objAuthorisedNet.sBillZIP = transaction.ZipCode;
            objAuthorisedNet.CustomerIP = HttpContext.Current.Request.UserHostAddress;
            objAuthorisedNet.sBillEmail = transaction.BillingEmail;
            objAuthorisedNet.sBillPhone = transaction.BillingPhone;
            objAuthorisedNet.sBillCompany = transaction.BillingCompanyName;
            if (!transaction.CountryName.Equals(string.Empty))
                objAuthorisedNet.sBillCountry = transaction.CountryName; //"US";
            else
                objAuthorisedNet.sBillCountry = "US";

            if (transaction.IsTestMode )
                objAuthorisedNet.AuthNetURL = string.IsNullOrEmpty(transaction.APITestUrl) ? "https://test.authorize.net/gateway/transact.dll" : transaction.APITestUrl;
            else
                objAuthorisedNet.AuthNetURL = string.IsNullOrEmpty(transaction.APILiveUrl) ? "https://secure.authorize.net/gateway/transact.dll" : transaction.APILiveUrl;

        }
        private void GatewayReponse(TransactionType Trnas, GatewayInParameter transaction)
        {
            string strRequest = string.Empty;
            string strResponse = string.Empty;

            if (objAuthorisedNet.Process(Trnas))
            {
                objTranResult.TransactionWrapper.isSuccess = true;

                objTranResult.StatusCode = objAuthorisedNet.StatusCode;

                objTranResult.TransactionWrapper.GatewayAuthCode = objAuthorisedNet.RefTransactionId;

                if (objAuthorisedNet.objCCInf != null)
                {
                    foreach (string strKey in objAuthorisedNet.objCCInf.AllKeys)
                    {
                        if (strKey == "x_card_num")
                        {
                            strRequest += strKey + "=" + objAuthorisedNet.objCCInf[strKey].Substring(objAuthorisedNet.objCCInf[strKey].Length - 4, 4) + "|";
                        }
                        else
                            strRequest += strKey + "=" + objAuthorisedNet.objCCInf[strKey] + "|";
                    }

                    strRequest = strRequest.Remove((strRequest.Length - 1), 1);
                    objTranResult.TransactionWrapper.GatewayRequest = strRequest;
                }

                if (objAuthorisedNet.objCCRetVals != null)
                {
                    foreach (string strRetVal in objAuthorisedNet.objCCRetVals)
                    {
                        strResponse += strRetVal + "|";
                    }

                    strResponse = strResponse.Remove((strResponse.Length - 1), 1);
                    objTranResult.TransactionWrapper.GatewayResponse = strResponse;
                }


            }
            else
            {
                objTranResult.StatusCode = objAuthorisedNet.StatusCode;
                if (objAuthorisedNet.StatusCode == 4)
                {
                    objTranResult.TransactionWrapper.GatewayAuthCode  = objAuthorisedNet.RefTransactionId;
                }
                objTranResult.TransactionWrapper.GatewayErrorCode  = objAuthorisedNet.ErrorMessage;
                objTranResult.TransactionWrapper.isSuccess = false;

                if (objAuthorisedNet.objCCInf != null)
                {
                    foreach (string strKey in objAuthorisedNet.objCCInf.AllKeys)
                    {
                      
                        if (strKey == "x_card_num")
                            strRequest += strKey + "=" + objAuthorisedNet.objCCInf[strKey].Substring(objAuthorisedNet.objCCInf[strKey].Length - 4, 4) + "|";
                        else
                            strRequest += strKey + "=" + objAuthorisedNet.objCCInf[strKey] + "|";
                    }

                    strRequest = strRequest.Remove((strRequest.Length - 1), 1);
                    objTranResult.TransactionWrapper.GatewayRequest = strRequest;
                }

                if (objAuthorisedNet.objCCRetVals != null)
                {
                    foreach (string strRetVal in objAuthorisedNet.objCCRetVals)
                    {
                        strResponse += strRetVal + "|";
                    }

                    strResponse = strResponse.Remove((strResponse.Length - 1), 1);
                    objTranResult.TransactionWrapper.GatewayResponse = strResponse;
                }
            }

            objTranResult.TransactionWrapper.PaymentModeID = (int)PaymentMode.CC ;
            objTranResult.TransactionWrapper.TransactionAmount = transaction.TransactionAmount;
            objTranResult.TransactionWrapper.isPaymentTransaction = true;
          
        }

    }
}
