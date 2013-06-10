using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSScriptControl;

namespace Validation.Tests {

	/// <summary>
	/// Idea came from 
	/// <see cref="http://stephenwalther.com/archive/2010/12/20/integrating-javascript-unit-tests-with-visual-studio.aspx"/>
	/// Specific processing/class is required as VS 2012 Express Web does not allow 3rd party extensions
	/// like Chutzpah (which can be used as JavaScript UT framework adapter)
	/// </summary>
	public class JavaScriptTestHelper : IDisposable {

		private ScriptControl _sc;
		private TestContext _context;

		/// <summary>
		/// You need to use this helper with Unit Tests and not
		/// Basic Unit Tests because you need a Test Context
		/// </summary>
		/// <param name="testContext">Unit Test Test Context</param>
		public JavaScriptTestHelper(TestContext testContext) {
			if (testContext == null) {
				throw new ArgumentNullException("TestContext");
			}
			_context = testContext;

			_sc = new ScriptControl();
			_sc.Language = "javascript";
			_sc.AllowUI = false;
		}

		/// <summary>
		/// Load the contents of a JavaScript file into the
		/// Script Engine.
		/// </summary>
		/// <param name="path">Path to JavaScript file</param>
		public void LoadFile(string path) {
			if (!File.Exists(path)) {
				var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				path = Path.Combine(directory, path);
			}
			var fileContents = File.ReadAllText(path);
			try {
				_sc.AddCode(fileContents);
			}
			catch {
				throw CreateJSException();
			}
		}

		/// <summary>
		/// Pass the path of the test that you want to execute.
		/// </summary>
		/// <param name="testMethodName">JavaScript function name</param>
		public void ExecuteTest(string testMethodName) {
			dynamic result = null;
			try {
				result = _sc.Run("execute", new object[] { testMethodName });
			}
			catch {
				throw CreateJSException();
			}
		}


		public object ExecuteMethod(string methodName, params object[] args) {
			dynamic result = null;
			try {
				result = _sc.Run(methodName, args);
				return result;
			}
			catch {
				throw CreateJSException();
			}
		}

		private AssertFailedException CreateJSException() {
			var error = ((IScriptControl)_sc).Error;
			if (error != null) {
				var description = error.Description;
				var text = error.Text;
				var source = error.Source;
				var details = String.Format("{0}\r\n{1}\r\nLine: {2} Column: {3}",
						description, source, error.Line, error.Column);
				if (_context != null) {
					_context.WriteLine(details);
				}
				return new AssertFailedException(details);
			}
			return new AssertFailedException("Unknown error returned from JScript control");
		}

		public void Dispose() {
			_sc = null;
		}
	}
}