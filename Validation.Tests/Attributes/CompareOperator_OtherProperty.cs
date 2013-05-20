using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Validation.Attributes;
using Validation.Tests.Attributes;

namespace Web.Tests.Attributes
{
    [TestClass]
    public class CompareOperator_OtherProperty
    {
        private class TestProperties
        {
            [DisplayName("WithDisplayName")]
            public virtual int WithDisplayName { get; set; }

            [Display(Name="WithName")]
            public virtual int WithName { get; set; }

            [Display(Name = "WithNameAndResourceType", ResourceType=typeof(Resources))]
            public virtual int WithNameAndResourceType { get; set; }

            [Display(Name = "WithNonExistentNameAndResourceType", ResourceType = typeof(Resources))]
            public virtual int WithNonExistentNameAndResourceType { get; set; }

            [Display(Name = "WithNameAndNullResourceType", ResourceType=null)]
            public virtual int WithNameAndNullResourceType { get; set; }
        }


        [TestMethod]
        public void OtherPropertyTitle()
        {
            var methodInfo = typeof(CompareOperatorAttribute).GetMethod("TryToExtractOtherPropertyTitle", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var getOtherPropertyTitle = typeof(CompareOperatorAttribute).GetProperty("OtherPropertyTitle", 
                    BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);

            var checks = new Tuple<string, string>[] { 
                    Tuple.Create("WithNonExistentNameAndResourceType", "WithNonExistentNameAndResourceType"),
                    Tuple.Create("WithDisplayName", "WithDisplayName"), 
                    Tuple.Create("WithName", "WithName"), 
                    Tuple.Create("WithNameAndResourceType", "TestFieldValue"),
                    Tuple.Create("WithNonExistentNameAndResourceType", "WithNonExistentNameAndResourceType"),
                    Tuple.Create("WithNameAndNullResourceType", "WithNameAndNullResourceType")};

            foreach (var check in checks)
            {
                var attribute = new CompareOperatorAttribute(check.Item1);
                var propertyInfo = typeof(TestProperties).GetProperty(check.Item1);
                methodInfo.Invoke(attribute, new[] { propertyInfo });
                Assert.AreEqual(check.Item2, getOtherPropertyTitle.Invoke(attribute, null), 
                        "Invalid title for property " + check.Item1);
            }
        }
    }
}
