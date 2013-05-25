using System;
using System.ComponentModel.DataAnnotations;

namespace Validation.Attributes {
	public class BaseValidationAttribute : ValidationAttribute {
		public static string FormatPropertyForClientValidation(string property) {
			if (property == null) {
				throw new ArgumentException("Value cannot be null or empty.", "property");
			}
			return "*." + property;
		}
	}
}
