using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Validation.Tests.Attributes {
	[TestClass]
	public class TypeCheckTestsClient : TypeCheckTestsBase {
		private static TestContext _testContext;
		private JavaScriptTestHelper _jsHelper;

		[ClassInitialize]
		public static void ClassInit(TestContext context) {
			_testContext = context;
		}

		[TestInitialize]
		public void TestInit() {
			_jsHelper = new JavaScriptTestHelper(_testContext);
			_jsHelper.LoadFile(@".\Scripts\jsUnit.js");
			_jsHelper.LoadFile(@".\Scripts\Validation.js");
		}

		protected override object PerformTypeCheck(ValidationDataType dataType, object value) {
			bool result = (bool)_jsHelper.ExecuteMethod("compareOperator_IsOfType",
					dataType.ToString(), value);
			return result ? null : new object();
		}
	}
}
