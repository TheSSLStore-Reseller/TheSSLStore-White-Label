using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;

namespace WBSSLStore.Service.ViewModels
{
    public class AccountStatement
    {
        public int OrderDetailID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionDetail { get; set; }
        public int InvoiceID { get; set; }
        public TransactionMode TransactionMode { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
        public int UserTransactionID { get; set; }

    }

    public class AccountStatementViewModel
    {
        public List<AccountStatement> AccountStatement { get; set; }
        public decimal AccountBalance { get; set; }

    }
}
