using System.Diagnostics;

namespace Validation.Tests.Attributes {
	public class PropertyHolder {
		public object otherProperty { get; set; }
		public PropertyHolder NonComparable { get; set; }

		[DebuggerStepThrough]
		public PropertyHolder() {
			NonComparable = this;
		}
	}
}
