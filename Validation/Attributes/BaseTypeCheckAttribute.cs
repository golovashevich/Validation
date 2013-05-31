using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Validation.Attributes {
	public class BaseTypeCheckAttribute : BaseValidationAttribute {
		public ValidationDataType Type { get; set; }

		protected static Dictionary<Type, ValidationDataType> TypeDataTypeMap =
			new Dictionary<Type, ValidationDataType>() {
						{ typeof(DateTime), ValidationDataType.Date },
						{ typeof(String), ValidationDataType.String },
						{ typeof(Int32), ValidationDataType.Integer },
						{ typeof(Double), ValidationDataType.Double },
						{ typeof(Decimal), ValidationDataType.Currency }
					};

		protected static Dictionary<ValidationDataType, Type> DataTypeTypeMap =
				new Dictionary<ValidationDataType, Type>() {
						{ ValidationDataType.Date, typeof(DateTime) },
						{ ValidationDataType.String, typeof(String) },
						{ ValidationDataType.Integer, typeof(Int32) },
						{ ValidationDataType.Double, typeof(Double) },
						{ ValidationDataType.Currency, typeof(Decimal) }
					};
	}
}
