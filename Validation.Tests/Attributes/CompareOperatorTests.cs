using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Web.Tests.Attributes {
	[TestClass]
	public class CompareOperatorTests {
		private const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;
		private const double DOUBLE_SMALL_VALUE = Double.MinValue / 2;

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FormatPropertyForClientValidation() {
			CompareOperatorAttribute.FormatPropertyForClientValidation(null);	
		}


		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorWithNull() {			
			new CompareOperatorAttribute(null, ValidationCompareOperator.Equal);
		}


		private class PropertyHolder {
			public object otherProperty { get; set; }
			public PropertyHolder NonComparable { get; set; }

			[DebuggerStepThrough]
			public PropertyHolder() {
				NonComparable = this;
			}
		}


		[TestMethod]
		public void DefaultOperatorIsEqual() {
			Assert.AreEqual(ValidationCompareOperator.Equal,
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
				{ ValidationDataType.Integer, 1 }
			};

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

			compareOperator.Operator = ValidationCompareOperator.Equal;
			holder.otherProperty = validValue;
			Assert.IsNotNull(compareOperator.GetValidationResult(holder, context), message);

			holder.otherProperty = invalidValue;
			Assert.IsNotNull(compareOperator.GetValidationResult(validValue, context), message);
			Assert.IsNotNull(compareOperator.GetValidationResult(invalidValue, context), message);

			compareOperator.Operator = ValidationCompareOperator.DataTypeCheck;
			Assert.IsNotNull(compareOperator.GetValidationResult(invalidValue, context), message);
		}


		[TestMethod]
		public void IncompatibleDataTypeAndArguments() {
			//data type, invalid value, valid value
			var checks = new Tuple<object, object, object>[] {
				CreateCheckTriple(ValidationDataType.Currency, DateTime.Now, 1m), 
				CreateCheckTriple(ValidationDataType.Integer, DateTime.Now, 1), 
				CreateCheckTriple(ValidationDataType.Double, DateTime.Now, 1d), 
				CreateCheckTriple(ValidationDataType.Date, Decimal.MinValue, DateTime.Now), 
				CreateCheckTriple(ValidationDataType.Date, DOUBLE_SMALL_VALUE, DateTime.Now), 
				CreateCheckTriple(ValidationDataType.Date, Int32.MinValue, DateTime.Now)
			};

			foreach (var check in checks) {
				IncompabileCheck(check.Item1, check.Item2, check.Item3); 
				IncompabileCheck(check.Item1, check.Item3, check.Item2);
				IncompabileCheck(check.Item1, check.Item2, check.Item2);
			}
		}


		private static void IncompabileCheck(object dataType, object value1, object value2) {
			var compareOperator = new CompareOperatorAttribute("otherProperty", 
					dataType: (ValidationDataType) dataType);
			var holder = new PropertyHolder();
			var context = new ValidationContext(holder);

			holder.otherProperty = value1;
			Assert.IsNotNull(compareOperator.GetValidationResult(value2, context),
					String.Format("Check fails for type {0} ({1}, {2})", dataType, value1, value2));
		}


		[TestMethod]
		public void DataTypeCheckWorksWithDataType() {
			new CompareOperatorAttribute("test", ValidationCompareOperator.DataTypeCheck, 
					ValidationDataType.Date);
		}


		[TestMethod]
		public void DataTypeCheckDoesNotRequireOther() {
			new CompareOperatorAttribute(null, ValidationCompareOperator.DataTypeCheck, ValidationDataType.Integer);
			new CompareOperatorAttribute(ValidationDataType.Double);
		}


		[TestMethod]
		public void NullIsConsideredAsValidValue() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType)))	{
				var compareOperator = new CompareOperatorAttribute(dataType);
				var context = new ValidationContext(new PropertyHolder() { otherProperty = null });
				Assert.IsNull(compareOperator.GetValidationResult(null, context),
						"({0}) Null should be considered as valid for every type", dataType);
			}
		}


		[TestMethod]
		public void EmptyStringIsAlwaysValid() {
			foreach (ValidationDataType dataType in Enum.GetValues(typeof(ValidationDataType))) {
				var compareOperator = new CompareOperatorAttribute(dataType);
				var context = new ValidationContext(new PropertyHolder() { otherProperty = "" });
				Assert.IsNull(compareOperator.GetValidationResult("", context),
						"({0}) Empty String should be considered as valid for every type", dataType);
			}
		}


		[TestMethod]
		public void DataTypeChecksForType() {
			var checks = new Tuple<bool, ValidationDataType, object>[] {
					Tuple.Create(true, ValidationDataType.Date, (object)DateTime.Now), 
					Tuple.Create(false, ValidationDataType.Date, (object)Decimal.MaxValue), 
					Tuple.Create(true, ValidationDataType.Currency, (object)Decimal.MaxValue), 
					Tuple.Create(false, ValidationDataType.Currency, (object)DateTime.Now), 
					Tuple.Create(true, ValidationDataType.Double, (object)(DOUBLE_BIG_VALUE)), 
					Tuple.Create(false, ValidationDataType.Double, (object)DateTime.Now), 
					Tuple.Create(true, ValidationDataType.Integer, (object)Int32.MaxValue), 
					Tuple.Create(false, ValidationDataType.Integer, (object)DateTime.Now), 
					Tuple.Create(false, ValidationDataType.String, (object)Double.MaxValue),
					Tuple.Create(false, ValidationDataType.String, (object)DateTime.MaxValue),
					Tuple.Create(false, ValidationDataType.String, (object)Decimal.MaxValue),
					Tuple.Create(false, ValidationDataType.String, (object)Int32.MaxValue)
			};

			foreach (var check in checks) {
				var holder = new PropertyHolder();
				var context = new ValidationContext(holder);

				var compareOperator1 = new CompareOperatorAttribute("otherProperty",
						ValidationCompareOperator.DataTypeCheck, check.Item2);
				var compareOperator2 = new CompareOperatorAttribute(check.Item2);

				string errorMessage = String.Format("Data type check fails for type {0}, value {1} ({2})", 
						check.Item2, check.Item3, check.Item3.GetType());
				Assert.AreEqual(check.Item1, null == compareOperator1.GetValidationResult(check.Item3, context), 
						errorMessage);
				Assert.AreEqual(check.Item1, null == compareOperator2.GetValidationResult(check.Item3, context),
						errorMessage);
				if (check.Item2 != ValidationDataType.String) {
					Assert.AreEqual(check.Item1, null == compareOperator1.GetValidationResult(check.Item3.ToString(),
							context), errorMessage);
					Assert.AreEqual(check.Item1, null == compareOperator2.GetValidationResult(check.Item3.ToString(),
							context), errorMessage);
				}
			}
		}


		private static Tuple<object, object, object> CreateCheckTriple(object value1, object value2, object value3) {
			return Tuple.Create(value1, value2, value3);
		}


		private static Tuple<object, object> CreateCheckDouble(object value1, object value2) {
			return Tuple.Create(value1, value2);
		}

	
		[TestMethod]
		public void ComparisonChecks() {
			// value2, value 1 -> true, value3, value 1 -> false
			var checks = new Tuple<ValidationCompareOperator, Tuple<object, object, object>[]>[] {
				Tuple.Create(ValidationCompareOperator.Equal, new [] { 
					CreateCheckTriple(Int32.MinValue, Int32.MinValue, Int32.MaxValue),
					CreateCheckTriple(Decimal.MinValue, Decimal.MinValue, Decimal.MaxValue),
					CreateCheckTriple(DOUBLE_SMALL_VALUE, DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE),
					CreateCheckTriple(DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue),
					CreateCheckTriple("test", "test", "not test")
				}),				
				Tuple.Create(ValidationCompareOperator.NotEqual, new [] { 
					CreateCheckTriple(Int32.MinValue, Int32.MaxValue, Int32.MinValue),
					CreateCheckTriple(Decimal.MinValue, Decimal.MaxValue, Decimal.MinValue),
					CreateCheckTriple(DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE),
					CreateCheckTriple(DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue),
					CreateCheckTriple("test", "not test", "test")
				}),
				Tuple.Create(ValidationCompareOperator.GreaterThan, new [] { 
					CreateCheckTriple(Int32.MinValue, Int32.MaxValue, Int32.MinValue),
					CreateCheckTriple(Decimal.MinValue, Decimal.MaxValue, Decimal.MinValue),
					CreateCheckTriple(DOUBLE_SMALL_VALUE, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE),
					CreateCheckTriple(DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue),
					CreateCheckTriple("test", "west", "test")
				}),
				Tuple.Create(ValidationCompareOperator.GreaterThanEqual, new [] { 
					CreateCheckTriple(Int32.MinValue + 1, Int32.MaxValue, Int32.MinValue), //greater 
					CreateCheckTriple(Int32.MaxValue, Int32.MaxValue, Int32.MinValue), //equal
					CreateCheckTriple(1.0m, Decimal.MaxValue, Decimal.MinValue),
					CreateCheckTriple(1.0m, 1.0m, Decimal.MinValue),
					CreateCheckTriple(1.0d, DOUBLE_BIG_VALUE,  DOUBLE_SMALL_VALUE),
					CreateCheckTriple(1.0d, 1.0d,  DOUBLE_SMALL_VALUE),
					CreateCheckTriple(DateTime.Now, DateTime.MaxValue, DateTime.MinValue),
					CreateCheckTriple(DateTime.Now, DateTime.Now, DateTime.MinValue),
					CreateCheckTriple("test", "west", "less than test"),
					CreateCheckTriple("test", "test", "less than test")
				}),
				Tuple.Create(ValidationCompareOperator.LessThan, new [] { 
					CreateCheckTriple(Int32.MaxValue, Int32.MinValue, Int32.MaxValue),
					CreateCheckTriple(Decimal.MaxValue, Decimal.MinValue, Decimal.MaxValue),
					CreateCheckTriple(DOUBLE_BIG_VALUE, DOUBLE_SMALL_VALUE,  DOUBLE_BIG_VALUE),
					CreateCheckTriple(DateTime.MaxValue, DateTime.MinValue, DateTime.MaxValue),
					CreateCheckTriple("west", "test", "west")
				}),
				Tuple.Create(ValidationCompareOperator.LessThanEqual, new [] { 
					CreateCheckTriple(Int32.MaxValue - 1, Int32.MinValue, Int32.MaxValue), //less
					CreateCheckTriple(Int32.MinValue, Int32.MinValue, Int32.MaxValue), //equal
					CreateCheckTriple(1.0m, Decimal.MinValue, Decimal.MaxValue),
					CreateCheckTriple(1.0m, 1.0m, Decimal.MaxValue),
					CreateCheckTriple(1.0d, DOUBLE_SMALL_VALUE,  DOUBLE_BIG_VALUE),
					CreateCheckTriple(1.0d, 1.0d,  DOUBLE_BIG_VALUE),
					CreateCheckTriple(DateTime.Now, DateTime.MinValue, DateTime.MaxValue),
					CreateCheckTriple(DateTime.Now, DateTime.Now, DateTime.MaxValue),
					CreateCheckTriple("west", "test", "xyz"),
					CreateCheckTriple("test", "test", "xyz")
				})
			};

			var holder = new PropertyHolder();
			var context = new ValidationContext(holder);
			var mapInfo = typeof(CompareOperatorAttribute).GetField("TypeDataTypeMap", 
					BindingFlags.NonPublic | BindingFlags.Static);
			var typeDataTypeMap = (Dictionary<Type, ValidationDataType>) mapInfo.GetValue(null);

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
	}		
}
