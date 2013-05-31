using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Validation.Attributes {
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class TypeCheckAttribute : BaseTypeCheckAttribute, IClientValidatable {

		public TypeCheckAttribute(ValidationDataType dataType) {
			Type = dataType;
		}
	
		protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
			return IsValidDataType(value, validationContext);
		}

		protected ValidationResult IsValidDataType(object value, ValidationContext context) {
			if (null == value) {
				return null;
			}

			ValidationDataType valueType;
			if (!TypeDataTypeMap.TryGetValue(value.GetType(), out valueType)) {
				return new ValidationResult(FormatErrorMessage(context.DisplayName));
			}

			if (valueType == Type) {
				return null;
			}

			if (Type == ValidationDataType.String) {
				return new ValidationResult(FormatErrorMessage(context.DisplayName));
			}

			if (valueType != ValidationDataType.String) {
				return new ValidationResult(FormatErrorMessage(context.DisplayName));
			}

			if (String.IsNullOrEmpty((string)value)) {
				return null;
			}
			try {
				Convert.ChangeType(value, DataTypeTypeMap[Type]);
				return null;
			}
			catch {
				return new ValidationResult(FormatErrorMessage(context.DisplayName));
			}
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
				ControllerContext context) {
			var displayName = metadata.DisplayName ?? metadata.PropertyName;
			var errorMessage = FormatErrorMessage(displayName);
			return new[] { new ModelClientValidationTypeCheckRule(errorMessage, Type.ToString()) };
		}
	}
}
