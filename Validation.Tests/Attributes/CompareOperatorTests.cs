using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {
	[TestClass]
	public class CompareOperatorTests {
		private const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;
		private const double DOUBLE_SMALL_VALUE = Double.MinValue / 2;

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FormatPropertyForClientValidation() {
			BaseValidationAttribute.FormatPropertyForClientValidation(null);
		}


		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorWithNull() {
			new CompareOperatorAttribute(null, CompareOperator.Equal);
		}


		[TestMethod]
		public void DefaultOperatorIsEqual() {
			Assert.AreEqual(CompareOperator.Equal,
					new CompareOperatorAttribute("otherProperty").Operator,
					"Default CompareOperator is Equal");
		}

		[TestMethod]
		public void DefaultTypeIsString() {
			Assert.AreEqual(ValidationDataType.String,
					new CompareOperatorAttribute("otherProperty").Type,
					"Default Type is String");
		}


		[TestMethod]
		public void IsValid_ArgumentChecks() {
			var compareOperator = new CompareOperatorAttribute("otherProperty",
					dataType: ValidationDataType.Integer);
			var compareOperator_noProperty = new CompareOperatorAttribute("noProperty");
			var context = new ValidationContext(new PropertyHolder() { otherProperty = 1 });

			Assert.IsNotNull(compareOperator_noProperty.GetValidationResult(1, context), "Null other property");

			var nullContext = new ValidationContext(new PropertyHolder());
			Assert.IsNull(compareOperator.GetValidationResult(null, nullContext),
					"Both other and value are nulls");
			Assert.IsNotNull(compareOperator.GetValidationResult(1, nullContext), "Null other value");
			Assert.IsNull(compareOperator.GetValidationResult(null, context), "Null value");
			Assert.IsNotNull(compareOperator.GetValidationResult(DateTime.Now, context), "Different types");
		}


		[TestMethod]
		public void InvalidDataTypes() {
			var holder = new PropertyHolder();
			var context = new ValidationContext(holder);
			var validValuesMap = new Dictionary<ValidationDataType, object>() {
				{ ValidationDataType.String, "test" },
				{ ValidationDataType.Currency, 1m },
				{ ValidationDataType.Date, DateTime.Now },
				{ ValidationDataType.Double, 1d },
				{ ValidationDataType.Integer, 1 }};

			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				object validValue = validValuesMap[dataType];

				object invalidValue = holder;
				CheckForInvalidDataType(holder, context, dataType, validValue, invalidValue);

				invalidValue = 1f;
				CheckForInvalidDataType(holder, context, dataType, validValue, invalidValue);

				invalidValue = new object();
				CheckForInvalidDataType(holder, context, dataType, validValue, invalidValue);
			}
		}


		private static void CheckForInvalidDataType(PropertyHolder holder, ValidationContext context,
				ValidationDataType dataType, object validValue, object invalidValue) {
			var compareOperator = new CompareOperatorAttribute("otherProperty", dataType: dataType);
			string message = String.Format("Check for {0}: Value is not of one of ValidationDataType types",
					dataType);

			compareOperator.Operator = CompareOperator.Equal;
			holder.otherProperty = validValue;
			Assert.IsNotNull(compareOperator.GetValidationResult(holder, context), message);

			holder.otherProperty = invalidValue;
			Assert.IsNotNull(compareOperator.GetValidationResult(validValue, context), message);
			Assert.IsNotNull(compareOperator.GetValidationResult(invalidValue, context), message);

			var typeCheck = new TypeCheckAttribute(dataType);
			Assert.IsNotNull(compareOperator.GetValidationResult(invalidValue, context), message);
		}


		[TestMethod]
		public void IncompatibleDataTypeAndArguments() {
			//data type, invalid value, valid value
			var checks = new Checks<object, object, object>() {
				{ ValidationDataType.Currency, DateTime.Now, 1m }, 
				{ ValidationDataType.Integer, DateTime.Now, 1 }, 
				{ ValidationDataType.Double, DateTime.Now, 1d }, 
				{ ValidationDataType.Date, Decimal.MinValue, DateTime.Now }, 
				{ ValidationDataType.Date, DOUBLE_SMALL_VALUE, DateTime.Now }, 
				{ ValidationDataType.Date, Int32.MinValue, DateTime.Now }};

			foreach (var check in checks) {
				IncompatibleCheck(check.Item1, check.Item2, check.Item3);
				IncompatibleCheck(check.Item1, check.Item3, check.Item2);
				IncompatibleCheck(check.Item1, check.Item2, check.Item2);
			}
		}


		private static void IncompatibleCheck(object dataType, object value1, object value2) {
			var compareOperator = new CompareOperatorAttribute("otherProperty",
					dataType: (ValidationDataType)dataType);
			var holder = new PropertyHolder();
			var context = new ValidationContext(holder);

			holder.otherProperty = value1;
			Assert.IsNotNull(compareOperator.GetValidationResult(value2, context),
					String.Format("Check fails for type {0} ({1}, {2})", dataType, value1, value2));
		}


		[TestMethod]
		public void ComparisonChecks() {
			var checks = CreateComparisonChecksTable();

			var holder = new PropertyHolder();
			var context = new ValidationContext(holder);
			var mapInfo = typeof(BaseTypeCheckAttribute).GetField("TypeDataTypeMap",
					BindingFlags.NonPublic | BindingFlags.Static);
			var typeDataTypeMap = (Dictionary<Type, ValidationDataType>)mapInfo.GetValue(null);

			foreach (var operatorCheck in checks) {
				foreach (var check in operatorCheck.Item2) {
					var compareOperator = new CompareOperatorAttribute("otherProperty",
							operatorCheck.Item1, typeDataTypeMap[check.Item1.GetType()]);

					var trueMessage = String.Format(
							"Comparison operator {0} with {1} and {2} should be true",
							compareOperator.Operator, check.Item1, check.Item2);
					var falseMessage = String.Format(
							"Comparison operator {0} with {1} and {2} should be false",
								compareOperator.Operator, check.Item1, check.Item3);

					//pure value check
					holder.otherProperty = check.Item1;
					Assert.IsNull(compareOperator.GetValidationResult(check.Item2, context), trueMessage);
					Assert.IsNotNull(compareOperator.GetValidationResult(check.Item3, context), falseMessage);

					//check is always true if value null or empty
					var emptyMessage = String.Format("Comparison operator {0} with empty value should be true",
							compareOperator.Operator);
					Assert.IsNull(compareOperator.GetValidationResult(null, context), emptyMessage);
					Assert.IsNull(compareOperator.GetValidationResult("", context), emptyMessage);
					Assert.IsNull(compareOperator.GetValidationResult(" ", context), emptyMessage);

					//string version check
					holder.otherProperty = Convert.ToString(check.Item1);
					Assert.IsNull(
							compareOperator.GetValidationResult(Convert.ToString(check.Item2), context),
							trueMessage);
					Assert.IsNotNull(
							compareOperator.GetValidationResult(Convert.ToString(check.Item3), context),
							falseMessage);
				}
			}
		}

		private static Checks<CompareOperator, Checks<object, object, object>>
				CreateComparisonChecksTable() {
			// value2, value 1 -> true, value3, value 1 -> false
			// Why not Double.MaxValue: http://stackoverflow.com/questions/4441782/why-does-double-tryparse-return-false-for-a-string-containing-double-maxvalue
			var checks = new Checks<CompareOperator, Checks<object, object, object>>() {
					{ CompareOperator.Equal, new Checks<object, object, object> { 
						{ Int32.MinValue, Int32.MinValue, Int32.MaxValue },
						{ Decimal.MinValue, Decimal.MinValue, Decimal.MaxValue },
						{ DOUBLE_SMALL_VALUE, DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE },
						{ DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue },
						{ "test", "test", "not test" }
					}},
					{ CompareOperator.NotEqual, new Checks<object, object, object> { 
						{ Int32.MinValue, Int32.MaxValue, Int32.MinValue },
						{ Decimal.MinValue, Decimal.MaxValue, Decimal.MinValue },
						{ DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE },
						{ DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue},
						{ "test", "not test", "test" }
					}},
					{ CompareOperator.GreaterThan, new Checks<object, object, object> { 
						{ Int32.MinValue, Int32.MaxValue, Int32.MinValue },
						{ Decimal.MinValue, Decimal.MaxValue, Decimal.MinValue },
						{ DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE },
						{ DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue },
						{ "test", "west", "test" }
					}},
					{ CompareOperator.GreaterThanEqual, new Checks<object, object, object> { 
						{ Int32.MinValue + 1, Int32.MaxValue, Int32.MinValue }, //greater 
						{ Int32.MaxValue, Int32.MaxValue, Int32.MinValue }, //equal
						{ 1.0m, Decimal.MaxValue, Decimal.MinValue },
						{ 1.0m, 1.0m, Decimal.MinValue },
						{ 1.0d, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE },
						{ 1.0d, 1.0d,  DOUBLE_SMALL_VALUE },
						{ DateTime.Now, DateTime.MaxValue, DateTime.MinValue },
						{ DateTime.Now, DateTime.Now, DateTime.MinValue },
						{ "test", "west", "less than test" },
						{ "test", "test", "less than test" }
					}},
					{ CompareOperator.LessThan, new Checks<object, object, object> { 
						{ Int32.MaxValue, Int32.MinValue, Int32.MaxValue },
						{ Decimal.MaxValue, Decimal.MinValue, Decimal.MaxValue },
						{ DOUBLE_BIG_VALUE, DOUBLE_SMALL_VALUE,  DOUBLE_BIG_VALUE },
						{ DateTime.MaxValue, DateTime.MinValue, DateTime.MaxValue },
						{ "west", "test", "west" }
					}},
					{ CompareOperator.LessThanEqual, new Checks<object, object, object> { 
						{ Int32.MaxValue - 1, Int32.MinValue, Int32.MaxValue }, //less
						{ Int32.MinValue, Int32.MinValue, Int32.MaxValue }, //equal
						{ 1.0m, Decimal.MinValue, Decimal.MaxValue },
						{ 1.0m, 1.0m, Decimal.MaxValue },
						{ 1.0d, DOUBLE_SMALL_VALUE,  DOUBLE_BIG_VALUE },
						{ 1.0d, 1.0d,  DOUBLE_BIG_VALUE },
						{ DateTime.Now, DateTime.MinValue, DateTime.MaxValue },
						{ DateTime.Now, DateTime.Now, DateTime.MaxValue },
						{ "west", "test", "xyz" },
						{ "test", "test", "xyz" }
				}}	 
			};
			return checks;
		}
	}
}
