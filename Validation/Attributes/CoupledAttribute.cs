using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Validation.Attributes
{
    /// <summary>
    /// Indicates that validating field marked with this attribute also should validate coupled field
    /// </summary>
    public class CoupledAttribute : ValidationAttribute, IClientValidatable
    {
        public string OtherProperty { get; private set; }


        public CoupledAttribute(string otherProperty)
        {
            if (otherProperty == null) { throw new ArgumentNullException("otherProperty"); }

            this.OtherProperty = otherProperty;            
        }


        public override bool IsValid(object value)
        {
            return true;
        }


        public static string FormatPropertyForClientValidation(string property)
        {
            if (property == null)
            {
                throw new ArgumentException("Value cannot be null or empty.", "property");
            }
            return "*." + property;
        }       


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, 
				ControllerContext context)
        {
            yield return new ModelClientValidationCoupledRule(ErrorMessageString, 
                    FormatPropertyForClientValidation(OtherProperty));
        }    }
}
