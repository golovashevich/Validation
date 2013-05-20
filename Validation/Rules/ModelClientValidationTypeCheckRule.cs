using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Validation.Attributes {
	public class ModelClientValidationTypeCheckRule : ModelClientValidationRule {
		public ModelClientValidationTypeCheckRule(string errorMessage, string dataType)
        {
            ErrorMessage = errorMessage;
            ValidationType = "compareoperatortypecheck";
			ValidationParameters["datatype"] = dataType;
        }
	}
}