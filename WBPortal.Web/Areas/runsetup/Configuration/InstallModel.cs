using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WhiteBrandShrink.Models
{
    #region ENUM
    public enum SQLServerInfo
    {
        SQLCONNECTIONVALUES = 1,
        SQLCONNECTIONSTRING = 2
    }
    public enum SQLAuthentication
    {
        SQL_SERVER_AUTHENTICATION = 1,
        INTEGRATED_WINDOWS_AUTHENTICATION = 2
    }
    #endregion

    public class InstallModel
    {
        [DBValidation(ErrorMessage = "Enter your database <strong>Connection string</strong>.")]
        public string DatabaseConnectionString { get; set; }
        public string DataProvider { get; set; }
        public bool DisableSqlCompact { get; set; }
        //SQL Server properties
        public string SqlConnectionInfo { get; set; }
        [AllowHtml]
        [DBValidation(ErrorMessage = "Enter your <strong>Servername</strong>.")]
        public string SqlServerName { get; set; }

        [DBValidation(ErrorMessage = "Enter your <strong>database name</strong>.")]
        public string SqlDatabaseName { get; set; }

        [DBValidation(ErrorMessage = "Enter your <strong>Username</strong>.")]
        public string SqlServerUsername { get; set; }

        [DBValidation(ErrorMessage = "Enter your <strong>Password</strong>.")]
        public string SqlServerPassword { get; set; }
        public string SqlAuthenticationType { get; set; }
        public bool SqlServerCreateDatabase { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Enter digits only.")]
        [DBValidation(ErrorMessage = "Enter <strong>TCP PORT</strong>.")]
        public string TCPPORT { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Enter digits only.")]
        [DBValidation(ErrorMessage = "Enter <strong>Connection Timeout value</strong>.")]
        public string ConnectionTimeout { get; set; }

        [AllowHtml]
        public string Collation { get; set; }
        public bool UseSSL { get; set; }
        public bool UseTCPPort { get; set; }
        public bool UseConnectionTimeout { get; set; }
        public bool IsCustomRawConnection { get; set; }
        public bool IsWindowsAuthentication { get; set; }
        public bool IsExists { get; set; }
        public string Errormessage { get; set; }


    }

    public class DBValidation : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _data = (InstallModel)validationContext.ObjectInstance;
            if (_data != null)
            {
                if (_data.SqlConnectionInfo.Equals(SQLServerInfo.SQLCONNECTIONSTRING.ToString()) && string.IsNullOrEmpty(_data.DatabaseConnectionString))
                    return new ValidationResult("Enter your database <strong>Connection string</strong>.", new[] { "SqlConnectionInfo" });
                else if ((_data.SqlConnectionInfo.Equals(SQLServerInfo.SQLCONNECTIONVALUES.ToString())))
                {
                    return (string.IsNullOrEmpty(_data.SqlServerName) || string.IsNullOrEmpty(_data.SqlDatabaseName)) ? new ValidationResult("Enter your <strong>" + (string.IsNullOrEmpty(_data.SqlServerName) ? "server name" : (string.IsNullOrEmpty(_data.SqlDatabaseName) ? "database name" : "")) + "</strong>.", new[] { string.IsNullOrEmpty(_data.SqlServerName) ? "SqlServerName" : "SqlDatabaseName" }) : ValidationResult.Success;
                }
                else if (!_data.SqlAuthenticationType.Equals("-99") && _data.SqlAuthenticationType.Equals(SQLAuthentication.SQL_SERVER_AUTHENTICATION.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return (string.IsNullOrEmpty(_data.SqlServerUsername) || string.IsNullOrEmpty(_data.SqlServerPassword)) ? new ValidationResult("Enter your <strong>" + (string.IsNullOrEmpty(_data.SqlServerUsername) ? "user name" : (string.IsNullOrEmpty(_data.SqlServerPassword) ? " password " : "")) + "</strong>.", new[] { string.IsNullOrEmpty(_data.SqlServerUsername) ? "SqlServerUsername" : "SqlServerPassword" }) : ValidationResult.Success;
                }
                else if (_data.UseTCPPort)
                {
                    return (string.IsNullOrEmpty(_data.TCPPORT) ? new ValidationResult("Enter <strong>TCP Port</strong>", new[] { "TCPPORT" }) : ValidationResult.Success);
                }
                else if (_data.UseConnectionTimeout)
                {
                    return (string.IsNullOrEmpty(_data.ConnectionTimeout) ? new ValidationResult("Enter <strong>Connection Timeout value</strong>.", new[] { "ConnectionTimeout" }) : ValidationResult.Success);
                }
                else
                    return ValidationResult.Success;
            }

            return base.IsValid(value, validationContext);
        }

        IEnumerable<ModelClientValidationRule> IClientValidatable.GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRule { ValidationType = "required", ErrorMessage = this.ErrorMessage } };
        }
    }

    public class Paymentviewmodel
    {
        public PaymentModelSetup paymentmodel { get; set; }
        public SMTPModelsetup SMTPmodel { get; set; }

        public string Errormessage { get; set; }
    }

    public class PaymentModelSetup
    {
        public bool EnableTestMode { get; set; }

        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,6})$", ErrorMessage = "Please provide <b>valid EmailID</b>")]
        [Required(ErrorMessage = "Enter <b>PayPal ID</b>")]
        [MaxLength(150, ErrorMessage = "PayPal id does not exid 150 characters")]
        public string PaypalID { get; set; }

        [Required(ErrorMessage = "Enter <b>AuthorizeNet Login ID</b>")]
        [MaxLength(150, ErrorMessage = "AuthorizeNet id does not exid 150 characters")]
        public string AuthorizeNetloginID { get; set; }

        [Required(ErrorMessage = "Enter <b>AuthorizeNet Transaction Key</b>")]
        public string AuthorizeNetTranKey { get; set; }

    }

    public class SMTPModelsetup
    {
        public bool UserSSL { get; set; }
        public bool RequireCredintial { get; set; }

        [Required(ErrorMessage = "Enter <b>SMTP Host.</b>")]
        public string SMTPHOST { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Enter digits only.")]
        [Required(ErrorMessage = "Enter <b>SMTP Port.</b>")]
        [MaxLength(6, ErrorMessage = "Enter valid <b>SMTP Port.</b>")]
        public string SMTPPORT { get; set; }

        [SMTPValidation(ErrorMessage = "Enter <b>SMTP Username.</b>")]
        public string SMTPUser { get; set; }

        [SMTPValidation(ErrorMessage = "Enter <b>SMTP Password.</b>")]
        public string SMTPPassword { get; set; }
    }

    public class SMTPValidation : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _data = (SMTPModelsetup)validationContext.ObjectInstance;
            if (_data != null)
            {
                if (_data.RequireCredintial && (string.IsNullOrEmpty(_data.SMTPUser) || string.IsNullOrEmpty(_data.SMTPPassword)))
                    return new ValidationResult(string.IsNullOrEmpty(_data.SMTPUser) ? "Enter <b>SMTP Username.</b>" : (string.IsNullOrEmpty(_data.SMTPPassword) ? "Enter <b>SMTP Password.</b>" : ""), new[] { string.IsNullOrEmpty(_data.SMTPUser) ? "SMTPUser" : "SMTPPassword" });
                else
                    return ValidationResult.Success;
            }
            return base.IsValid(value, validationContext);
        }
        IEnumerable<ModelClientValidationRule> IClientValidatable.GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRule { ValidationType = "required", ErrorMessage = this.ErrorMessage } };
        }
    }

    public class AdminUserModel
    {
        [Required(ErrorMessage="Enter <b>Fullname</b>")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage="Enter <b>Company name</b>")]
        public string CompanyName { get; set; }
        
        [Required(ErrorMessage="Enter <b>Password</b>")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage="Enter <b>Confirm password</b>")]
        [System.ComponentModel.DataAnnotations.Compare("Password",ErrorMessage="Confirm password does <b>not match with password</b>.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage="Enter <b>Email address</b>")]        
        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,6})$", ErrorMessage = "Please provide <b>valid Email</b>")]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        [Required(ErrorMessage="Enter <b>Live partnercode</b>")]
        
        [MaxLength(25, ErrorMessage = "Enter valid <b>partner code</b>")]
        public string LivePartnerCode { get; set; }
        [Required(ErrorMessage="Enter <b>Live Auth code</b>")]
        [MaxLength(125)]
        public string LivePartnerAuthCode { get; set; }
        //[Required(ErrorMessage = "Enter <b>Test Partner Code</b>")]
        //[MaxLength(25, ErrorMessage = "Enter valid <b>test partner code</b>")]
        public string TestPartnerCode { get; set; }
        //[Required]
        //[MaxLength(125)]
        public string TestPartnerAuthCode { get; set; }
    }

    public class GeneralSettings
    {
        [Required]
        [RegularExpression(@"^(?:[-A-Za-z0-9]+\.)+[A-Za-z]{2,6}$", ErrorMessage = "Please provide <b>valid domain name</b>")]        
        public string DomainName { get; set; }
        public string BillingCurrency { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,6})$", ErrorMessage = "Please provide <b>valid Email</b>")]
        public string AdminEmail { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,6})$", ErrorMessage = "Please provide <b>valid Email</b>")]
        public string BillingEmail { get; set; }
        [RegularExpression(@"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,6})$", ErrorMessage = "Please provide <b>valid Email</b>")]
        public string SupportEmail { get; set; }


        public string FBUrl { get; set; }
        public string TwiterURL { get; set; }
        public string GPURL{ get; set; }
        public string SitePhone { get; set; }
        public string SiteLanguage { get; set; }
        public string TimeZone { get; set; }
    }


}

