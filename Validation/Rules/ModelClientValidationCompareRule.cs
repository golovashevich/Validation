using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Validation.Attributes {
	public class ModelClientValidationCompareRule : ModelClientValidationRule {
		public ModelClientValidationCompareRule(string errorMessage, string otherProperty, string dataType,
				CompareOperator compareOperator)
        {
            ErrorMessage = errorMessage;
			//TODO: Check that this does not interfere with MVC's CompareAttribute
            ValidationType = "compareoperator";
            ValidationParameters["other"] = otherProperty;
			ValidationParameters["datatype"] = dataType;
            ValidationParameters["compareoperator"] = compareOperator;
        }
	}
}