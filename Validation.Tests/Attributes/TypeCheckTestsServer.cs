using System;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {

	[TestClass]
	public class TypeCheckTestsServer : TypeCheckTestsBase {
		private const double DOUBLE_BIG_VALUE = Double.MaxValue / 2;

		[TestMethod]
		public void DataTypeCheckWorksWithDataType() {
			new TypeCheckAttribute(ValidationDataType.Date);
		}

		#region Implementation of abstracts
		protected override object PerformTypeCheck(ValidationDataType dataType, object value) {
			var typeCheck = new TypeCheckAttribute(dataType);
			return typeCheck.GetValidationResult(value, new ValidationContext(new object()));
		}
		#endregion
	}
}
