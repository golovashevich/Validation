using System.Web.Mvc;

namespace Validation.Attributes
{
    public class ModelClientValidationCoupledRule : ModelClientValidationRule
    {
        public ModelClientValidationCoupledRule(string errorMessage, string other)
        {
            ErrorMessage = errorMessage;
            ValidationType = "coupled";
            ValidationParameters["other"] = other;
        }
    }
}