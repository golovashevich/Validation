using System;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {
	[TestClass]
	public class CompareOperatorTestsServer : CompareOperatorTestsBase {
		private CompareOperatorAttribute _operator;

		[TestInitialize]
		public override void TestInit() {
			base.TestInit();
			_operator = new CompareOperatorAttribute(_holderPropertyName);
		}

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
		[ExpectedException(typeof(ArgumentNullException))]
		public void CannotSetNullOtherProperty() {
			var compareOperator = new CompareOperatorAttribute("otherProperty");
			compareOperator.OtherProperty = null;
		}

		[TestMethod]
		public override void IsValid_ArgumentChecks() {
			base.IsValid_ArgumentChecks();
			var compareOperator_noProperty = new CompareOperatorAttribute("noProperty");
			Assert.IsNotNull(compareOperator_noProperty.GetValidationResult(1, _context), "Null other property");
		}


		#region Implementation of abstracts
		protected override ValidationDataType CurrentDataType {
			get { return _operator.Type; }
			set { _operator.Type = value; }
		}

		protected override CompareOperator CurrentOperator {
			get { return _operator.Operator; }
			set { _operator.Operator = value; }
		}

		protected override object PerformCheck(object value, ValidationContext context) {
			return _operator.GetValidationResult(value, context);
		}

		protected override object PerformTypeCheck(ValidationDataType dataType, object value) {
			var typeCheck = new TypeCheckAttribute(dataType);
			return typeCheck.GetValidationResult(value, _context);
		}
		#endregion
	}
}
