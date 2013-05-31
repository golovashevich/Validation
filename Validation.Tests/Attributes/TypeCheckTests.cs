using System;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {

	[TestClass]
	public class TypeCheckTests {
		private const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;

		[TestMethod]
		public void DataTypeCheckWorksWithDataType() {
			new TypeCheckAttribute(ValidationDataType.Date);
		}

		[TestMethod]
		public void NullIsConsideredAsValidValue() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				var compareOperator = new TypeCheckAttribute(dataType);
				var context = new ValidationContext(new PropertyHolder() { otherProperty = null });
				Assert.IsNull(compareOperator.GetValidationResult(null, context),
						"({0}) Null should be considered as valid for every type", dataType);
			}
		}

		[TestMethod]
		public void DataTypeChecksForType() {
			var checks = new Checks<bool, ValidationDataType, object>() {
						{ true, ValidationDataType.Date, (object)DateTime.Now} , 
						{ false, ValidationDataType.Date, (object)Decimal.MaxValue }, 
						{ true, ValidationDataType.Currency, (object)Decimal.MaxValue }, 
						{ false, ValidationDataType.Currency, (object)DateTime.Now }, 
						{ true, ValidationDataType.Double, (object)(DOUBLE_BIG_VALUE) }, 
						{ false, ValidationDataType.Double, (object)DateTime.Now }, 
						{ true, ValidationDataType.Integer, (object)Int32.MaxValue }, 
						{ false, ValidationDataType.Integer, (object)DateTime.Now }, 
						{ false, ValidationDataType.String, (object)Double.MaxValue },
						{ false, ValidationDataType.String, (object)DateTime.MaxValue },
						{ false, ValidationDataType.String, (object)Decimal.MaxValue },
						{ false, ValidationDataType.String, (object)Int32.MaxValue }};

			foreach (var check in checks) {
				var holder = new PropertyHolder();
				var context = new ValidationContext(holder);

				var operator1 = new TypeCheckAttribute(check.Item2);
				var operator2 = new TypeCheckAttribute(check.Item2);

				string errorMessage = String.Format("Data type check fails for type {0}, value {1} ({2})",
						check.Item2, check.Item3, check.Item3.GetType());
				Assert.AreEqual(check.Item1, null == operator1.GetValidationResult(check.Item3, context),
						errorMessage);
				Assert.AreEqual(check.Item1, null == operator2.GetValidationResult(check.Item3, context),
						errorMessage);
				if (check.Item2 != ValidationDataType.String) {
					Assert.AreEqual(check.Item1, null == operator1.GetValidationResult(check.Item3.ToString(),
							context), errorMessage);
					Assert.AreEqual(check.Item1, null == operator2.GetValidationResult(check.Item3.ToString(),
							context), errorMessage);
				}
			}
		}

		[TestMethod]
		public void EmptyStringIsAlwaysValid() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				var compareOperator = new TypeCheckAttribute(dataType);
				var context = new ValidationContext(new PropertyHolder() { otherProperty = "" });
				Assert.IsNull(compareOperator.GetValidationResult("", context),
						"({0}) Empty String should be considered as valid for every type", dataType);
			}
		}
	}
}
