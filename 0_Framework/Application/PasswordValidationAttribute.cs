using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace _0_Framework.Application
{
    public class PasswordValidationAttribute : ValidationAttribute, IClientModelValidator
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            string password = value.ToString();

            if (password.Length < 6 || password.Length > 20)
                return new ValidationResult(ErrorMessage);

            if (!Regex.IsMatch(password, "[A-Z]"))
                return new ValidationResult(ErrorMessage);

            if (!Regex.IsMatch(password, "[a-z]"))
                return new ValidationResult(ErrorMessage);

            if (!Regex.IsMatch(password, "[0-9]"))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-passwordvalidation", ErrorMessage);
        }
    }

}
