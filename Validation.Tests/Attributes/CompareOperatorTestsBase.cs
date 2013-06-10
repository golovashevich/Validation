using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {

	public abstract class CompareOperatorTestsBase {
		protected const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;
		protected const double DOUBLE_SMALL_VALUE = Double.MinValue / 2;

		protected PropertyHolder _holder;
		protected ValidationContext _context;
		protected string _holderPropertyName;

		[TestInitialize]
		public virtual void TestInit() {
			_holder = new PropertyHolder();
			_holderPropertyName = "otherProperty"; 
			_context = new ValidationContext(_holder);
		}

		[TestMethod]
		public void InvalidDataTypes() {
			var checks = new Checks<ValidationDataType, object, object>() {
				{ ValidationDataType.Currency, 1m, this },
				{ ValidationDataType.Date, DateTime.Now, 1.1 },
				{ ValidationDataType.Double, 1d, this },
				{ ValidationDataType.Integer, 1, 1.1 }};

			foreach(var check in checks) {
				CheckForInvalidDataType(check.Item1, check.Item2, check.Item3);
			}
		}

		protected void CheckForInvalidDataType(ValidationDataType dataType,
				object validValue, object invalidValue) {
			CurrentDataType = dataType;
			string message = "Check for {0} ({1}): Value is not of one of ValidationDataType types";

			CurrentOperator = CompareOperator.Equal;

			_holder.otherProperty = validValue;
			Assert.IsNull(PerformCheck(_holder), message, dataType, "invalid vs valid"); 

			_holder.otherProperty = invalidValue;
			Assert.IsNull(PerformCheck(validValue), message, dataType, "valid vs invalid");  
			Assert.IsNull(PerformCheck(invalidValue), message, dataType, "invalid vs invalid");

			Assert.IsNotNull(PerformTypeCheck(dataType, invalidValue), message, dataType, "data type check");
		}

		[TestMethod]
		public virtual void IsValid_ArgumentChecks() {
			CurrentDataType = ValidationDataType.Integer;
			_holder.otherProperty = 1;

			var nullContext = new ValidationContext(new PropertyHolder());
			Assert.IsNull(PerformCheck(null, nullContext), "Both other and value are nulls");
			Assert.IsNull(PerformCheck(1, nullContext), "Null other value");
			Assert.IsNull(PerformCheck(null), "Null value");
			Assert.IsNull(PerformCheck(DateTime.Now), "Different types (date and int)");
			CurrentDataType = ValidationDataType.Currency;
			Assert.IsNull(PerformCheck(DateTime.Now), "Different types (date and currency)");
			CurrentDataType = ValidationDataType.Double;
			Assert.IsNull(PerformCheck(DateTime.Now), "Different types (date and double)");
		}

		[TestMethod]
		public void IncompatibleDataTypeAndArguments() {
			//data type, invalid value, valid value
			var checks = new Checks<ValidationDataType, object, object>() {
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

		private void IncompatibleCheck(ValidationDataType dataType, object value1, object value2) {
			CurrentDataType = dataType;
			_holder.otherProperty = value1;
			Assert.IsNull(PerformCheck(value2),
					String.Format(
						"Check for type {0} ({1}, {2}) should be valid (incompatible types can not be judged", 
					dataType, value1, value2));
		}

		[TestMethod]
		public void ComparisonChecks() {
			var checks = CreateComparisonChecksTable();

			var mapInfo = typeof(BaseTypeCheckAttribute).GetField("TypeDataTypeMap",
					BindingFlags.NonPublic | BindingFlags.Static);
			var typeDataTypeMap = (Dictionary<Type, ValidationDataType>)mapInfo.GetValue(null);

			foreach (var operatorCheck in checks) {
				foreach (var check in operatorCheck.Item2) {
					CurrentOperator = operatorCheck.Item1;
					CurrentDataType = typeDataTypeMap[check.Item1.GetType()];

					var trueMessage = String.Format(
							"Comparison operator {0} with {1} and {2} should be true",
							CurrentOperator, check.Item1, check.Item2);
					var falseMessage = String.Format(
							"Comparison operator {0} with {1} and {2} should be false",
								CurrentOperator, check.Item1, check.Item3);

					//pure value check
					_holder.otherProperty = check.Item1;
					Assert.IsNull(PerformCheck(check.Item2), trueMessage);
					Assert.IsNotNull(PerformCheck(check.Item3), falseMessage);

					//check is always true if value null or empty
					var emptyMessage = String.Format("Comparison operator {0} with empty value should be true",
							CurrentOperator);
					Assert.IsNull(PerformCheck(null), emptyMessage);
					Assert.IsNull(PerformCheck(""), emptyMessage);
					Assert.IsNull(PerformCheck(" "), emptyMessage);

					//string version check
					_holder.otherProperty = Convert.ToString(check.Item1);
					Assert.IsNull(PerformCheck(Convert.ToString(check.Item2)), trueMessage);
					Assert.IsNotNull(PerformCheck(Convert.ToString(check.Item3)), falseMessage);
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
						{ "test", "test", "not test" },
						{ Int32.MaxValue.ToString(), Int32.MaxValue, "not test" },
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

		[DebuggerStepThrough]
		protected object PerformCheck(object value) {
			return PerformCheck(value, _context);
		}

		protected abstract object PerformCheck(object value, ValidationContext context);
		protected abstract object PerformTypeCheck(ValidationDataType dataType, object value); 

		protected abstract ValidationDataType CurrentDataType { get; set; }
		protected abstract CompareOperator CurrentOperator { get; set; } 
	}
}
