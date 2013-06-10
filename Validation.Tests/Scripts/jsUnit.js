/// jQuery->Script Control adapter

var jQuery = {
	extend: function (object, options) {
	},

	validator: {
		addMethod: function (methodName, method) {
		},

		unobtrusive: {
			adapters: {
				add: function (adapterName, params, fn) {
				}
			}
		}
	}
};

var $ = jQuery;

String.prototype.trim = function () {
	return this.replace(/^\s*/, "").replace(/\s*$/, "");
}


/// QUnit->Script Control adapter
var js_methods = {};
function test(name, method) {
	js_methods[name] = method;
}

function ok(value, message) {
	if (!value) {
		throw new Error(message);
	}
}

function execute(name) {
	js_methods[name]();
}
