using System;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Validation.Tests.Attributes {
	public abstract class TypeCheckTestsBase {
		private const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;

		[TestMethod]
		public void NullIsConsideredAsValidValue() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				Assert.IsNull(PerformTypeCheck(dataType, null),
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
				string errorMessage = String.Format("Data type check fails for type {0}, value {1} ({2})",
						check.Item2, check.Item3, check.Item3.GetType());
				Assert.AreEqual(check.Item1,
						null == PerformTypeCheck(check.Item2, check.Item3),
						errorMessage);
				if (check.Item2 != ValidationDataType.String) {
					Assert.AreEqual(check.Item1,
							null == PerformTypeCheck(check.Item2, check.Item3.ToString()),
							errorMessage);
				}
			}
		}

		[TestMethod]
		public void EmptyStringIsAlwaysValid() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				Assert.IsNull(PerformTypeCheck(dataType, ""),
						"({0}) Empty String should be considered as valid for every type", dataType);
			}
		}

		protected abstract object PerformTypeCheck(ValidationDataType dataType, object value);
	}
}
