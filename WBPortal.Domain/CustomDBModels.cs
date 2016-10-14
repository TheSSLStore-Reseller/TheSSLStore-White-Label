using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Data.Objects;

namespace WBSSLStore.Domain
{
    public class CustomDBModels
    {
    }



    //public class PasswordOptional : ValidationAttribute,IClientValidatable
    //{
        
    //    private string propertyname { get; set; }
    //    public PasswordOptional(string validatationprop)
    //    {
    //        propertyname = validatationprop;
    //    }
        
    //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    //    {
    //        var basePropertyInfo = validationContext.ObjectType.GetProperty(propertyname);
    //        bool valOther = (bool)basePropertyInfo.GetValue(validationContext.ObjectInstance, null);
    //        if (!valOther)
    //            return ValidationResult.Success;
    //        else if (valOther && !string.IsNullOrEmpty(Convert.ToString(value)))
    //            return ValidationResult.Success;
    //        else
    //            return new ValidationResult(base.ErrorMessage);

    //        //return valOther is bool && (bool)valOther;
    //    }
    //    // Implement IClientValidatable for client side Validation 
    //    //public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    //    //{
    //    //    return new ModelClientValidationRule[] { new ModelClientValidationRule { ValidationType = "required", ErrorMessage = this.ErrorMessage } };
    //    //}

    //}

    public class Uservalidate : IValidatableObject
    {
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "EmailRequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        public string Email { get; set; }

        
        //[PasswordOptional("isRegisteredUser", ErrorMessage = "Enter valid password")]
        public string Password { get; set; }
        public bool isRegisteredUser { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (isRegisteredUser && string.IsNullOrEmpty(Password))
                yield return new ValidationResult("Enter valid password");
            else
                yield return ValidationResult.Success;

        }
    }



}
