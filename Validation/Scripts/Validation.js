jQuery.validator.addMethod('compareoperatortypecheck', function (value, element, params) {
    return compareOperator_IsOfType(params.dataType, value);
}, '');

function compareOperator_IsOfType(dataType, value) {
	if (compareOperator_isUnknown(value)) {
		return false;
	}
	if (compareOperator_isEmpty(value)) {
		return true;
	}
    switch (dataType) {
        case "Integer": {
            return compareOperator_isValidInteger(value);
        }

        case "Double": {
            return compareOperator_isValidDouble(value);
        }

        case "Currency": {
            return compareOperator_isValidCurrency(value);
        }

        case "Date": {
            return compareOperator_isValidDate(value);
        }  

    	case "String": {
            return typeof (value) == "string";
        }
    }
}

function compareOperator_isValidCurrency(value) {
	if (typeof value == "date") {
		return false;
	}
	floatValue = +value;
	return !isNaN(floatValue) 
		&& floatValue >= -79228162514264337593543950335 
		&& floatValue <= 79228162514264337593543950335;
}

function compareOperator_isValidInteger(value) {
	if (typeof value == "date") {
		return false;
	}
	//http://stackoverflow.com/questions/12694455/javascript-parsefloat-in-different-cultures?lq=1
	var stringValue = new String(value);
	if (stringValue.indexOf(".") != -1) {
		return false;
	}
	floatValue = +value;
	return !isNaN(floatValue) && floatValue % 1 == 0
}

function compareOperator_isValidDouble(value) {
	if (typeof value == "date") {
		return false;
	}
	floatValue = +value;
	return !isNaN(floatValue) && floatValue > -1.7e308 && floatValue < 1.7e308;
}

function compareOperator_isValidDate(value) {
	if (typeof value != "date" && typeof value != "string") {
		return false;
	}
	var dateValue = new Date(value);
	return !isNaN(dateValue);
}

function compareOperator_isEmpty(value) {
	if (value == null) {
		return true;
	}
	if (typeof value != "string") {
		return false;
	}
	if (value.trim() == "") {
		return true;
	}
}

function compareOperator_isUnknown(value) {
	return typeof value == "undefined" || typeof value == "unknown"; 
}

jQuery.validator.addMethod('compareoperator', function (value, element, params) {
	value = value.trim();
	var otherValue = $(params.element).val().trim();
	return compareOperator_performComparison(value, otherValue, params.dataType, params.compareOperator);
}, '');

function compareOperator_performComparison(value, otherValue, dataType, compareOperator) {
	if (compareOperator_isUnknown(value) || compareOperator_isUnknown(otherValue)) {
		return true; 
	}
	if (compareOperator_isEmpty(value)) {
		return true;
	}
	//TODO: validate delegate map here
	switch (dataType) {
		case "Integer": {
			if (!compareOperator_isValidInteger(value) || !compareOperator_isValidInteger(otherValue)) {
				return true;
			}
			x = +value;
			y = +otherValue;
			break;
		}

		case "Double": {
			if (!compareOperator_isValidDouble(value) || !compareOperator_isValidDouble(otherValue)) {
				return true;
			}
			x = +value;
			y = +otherValue;
			break;
		}

		case "Currency": {
			if (!compareOperator_isValidCurrency(value) || !compareOperator_isValidCurrency(otherValue)) {
				return true;
			}
			x = +value;
			y = +otherValue;
			break;
		}

		case "Date": {
			if (!compareOperator_isValidDate(value) || !compareOperator_isValidDate(otherValue)) {
				return true;
			}
			x = new Date(value).getTime();
			y = new Date(otherValue).getTime();
			break;
		}

		case "String": {
			x = new String(value).toString();
			y = new String(otherValue).toString();
			break;
		}
	}

	switch (compareOperator) {
		case "LessThan":
			return x < y;

		case "LessThanEqual":
			return x <= y;

		case "GreaterThan":
			return x > y;

		case "GreaterThanEqual":
			return x >= y;

		case "Equal":
			return x == y;

		case "NotEqual":
			return x != y;
	}
}

jQuery.validator.unobtrusive.adapters.add('compareoperator', ['other', 'datatype', 'compareoperator'],
		function (options) {
			var prefix = options.element.name.substr(0, options.element.name.lastIndexOf('.') + 1);

			var otherElement;
			if (options.params.other != undefined) {
				fullOtherName = appendModelPrefix(options.params.other, prefix);
				otherElement = $(options.form).find(':input[name=' + fullOtherName + ']')[0];
			}

			options.rules['compareoperator'] = {
				dataType: options.params.datatype,
				compareOperator: options.params.compareoperator,
				element: otherElement
			};

			if (options.message) {
				options.messages['compareoperator'] = options.message;
			}
		});


jQuery.validator.unobtrusive.adapters.add('compareoperatortypecheck', ['datatype'],
		function (options) {
			options.rules['compareoperatortypecheck'] = {
				dataType: options.params.datatype
			};

			if (options.message) {
				options.messages['compareoperatortypecheck'] = options.message;
			}
		});


jQuery.validator.addMethod('coupled', function (value, element, params) {
	$(params.element).valid();
	$(element).removeClass("input-validation-error");
	return true;
}, '');


jQuery.validator.unobtrusive.adapters.add('coupled', ['other'], function (options) {
	var prefix = options.element.name.substr(0, options.element.name.lastIndexOf('.') + 1),
    other = options.params.other,
    fullOtherName = appendModelPrefix(other, prefix),
    otherElement = $(options.form).find(':input[name=' + fullOtherName + ']')[0];

	options.rules['coupled'] = { element: otherElement };
	if (options.message) {
		options.messages['coupled'] = options.message;
	}
});


function appendModelPrefix(value, prefix) {
	if (value.indexOf('*.') === 0) {
		value = value.replace('*.', prefix);
	}
	return value;
}


$.extend($.validator.methods, {
	//Modifies standard jQuery number validator so that it ignores preceeding and trailing spaces
	number: function (value, element) {
		return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:,\d{3})+)(?:\.\d+)?$/.test(value.trim());
	},

	//Skips range check if value is not number (in case it's called before number validator)
	range: function (value, element, param) {
		return this.optional(element)
			|| !$.validator.methods.number.call(this, value, element)
			|| (value >= param[0] && value <= param[1]);
	}
});


