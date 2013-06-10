using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;

namespace Validation.Tests.Attributes {
	[TestClass]
	public class CompareOperatorTestsClient : CompareOperatorTestsBase {
		private static TestContext _testContext;
		private JavaScriptTestHelper _jsHelper;

		[ClassInitialize]
		public static void ClassInit(TestContext context) {
			_testContext = context;
		}

		[TestInitialize]
		public override void TestInit() {
			base.TestInit();
			_jsHelper = new JavaScriptTestHelper(_testContext);
			_jsHelper.LoadFile(@".\Scripts\jsUnit.js");
			_jsHelper.LoadFile(@".\Scripts\Validation.js");
		}

		private object GetOtherValue(ValidationContext context, string otherPropertyName) {
			PropertyInfo otherPropertyInfo = context.ObjectType.GetProperty(otherPropertyName);
			Assert.IsNotNull(otherPropertyInfo, "Other property is not found");
			object otherPropertyValue = otherPropertyInfo.GetValue(context.ObjectInstance, null);
			return otherPropertyValue;
		}

		protected override object PerformCheck(object value, ValidationContext context) {
			var otherValue = GetOtherValue(context, _holderPropertyName);
			bool result = (bool)_jsHelper.ExecuteMethod("compareOperator_performComparison", value,
					otherValue, 
					CurrentDataType.ToString(), CurrentOperator.ToString());
			return result ? null :new object();
		}

		protected override object PerformTypeCheck(ValidationDataType dataType, object value) {
			bool result = (bool)_jsHelper.ExecuteMethod("compareOperator_IsOfType", 
					dataType.ToString(), value);
			return result ? null : new object();
		}

		private ValidationDataType _dataType;
		protected override ValidationDataType CurrentDataType {
			get { return _dataType; }
			set { _dataType = value; }
		}

		private CompareOperator _currentOperator;
		protected override CompareOperator CurrentOperator {
			get { return _currentOperator; }
			set { _currentOperator = value; }
		}
	}
}
