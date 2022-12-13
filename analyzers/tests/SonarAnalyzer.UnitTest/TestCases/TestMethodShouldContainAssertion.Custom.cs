using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomTests
{
    using TestFramework;
    using TestFramework.Attributes;

    [TestClass]
    public class BaseTest
    {
        [AssertionMethod]
        protected virtual void CustomAssertionMethod<T>(T t) { }

        [TestMethod]
        public void TestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
        //          ^^^^^^^^^^^
        {
            var x = 42;
        }

        [TestMethod]
        public void TestMethod2() // Compliant
        {
            Validator.StaticWithAttribute();
        }

        [TestMethod]
        public void TestMethod3() // Noncompliant
        {
            Validator.StaticWrongAttribute();
        }

        [TestMethod]
        public void TestMethod4() // Noncompliant
        {
            Validator.StaticNoAttribute();
        }

        [TestMethod]
        public void TestMethod5() // Compliant
        {
            var validator = new Validator();
            validator.InstanceWithAttribute();
        }

        [TestMethod]
        public void TestMethod6() => // Compliant
            new Validator().InstanceWithAttributeAndArg(null);

        [TestMethod]
        public void TestMethod7() => // Noncompliant, attribute must be on the method itself
            AttributedType.AttributeOnType();

        [TestMethod]
        [DerivedExpectedException]
        public void TestMethod8() // Compliant
        {
            var x = 42;
        }
    }

    [TestClass]
    public class DerivedTest : BaseTest
    {
        [TestMethod]
        public void Derived() // Noncompliant FP: The overridden method needs to be annotated because Roslyn does not respect AttributeUsage.Inherited in ISymbol.GetAttributes
        {
            CustomAssertionMethod(new object());
        }

        protected override void CustomAssertionMethod<T>(T t)
        {
        }
    }
}

namespace TestFramework
{
    using TestFramework.Attributes;

    public class Validator
    {
        [AssertionMethodAttribute]
        public static void StaticWithAttribute() { }

        [NotAssertionMethodAttribute]
        public static void StaticWrongAttribute() { }

        public static void StaticNoAttribute() { }

        [AssertionMethod]
        public bool InstanceWithAttribute() => true;

        [AssertionMethod]
        public bool InstanceWithAttributeAndArg(object arg) => true;
    }

    [AssertionMethod] // Missused attribute
    public static class AttributedType
    {
        public static void AttributeOnType() { } // Not an assertion method
    }
}

namespace TestFramework.Attributes
{
    public sealed class AssertionMethodAttribute : Attribute { }
    public class NotAssertionMethodAttribute : Attribute { } // AssertionMethodAttribute doesn't count as an assertion method attribute
    public class DerivedExpectedExceptionAttribute : ExpectedExceptionBaseAttribute { protected override void Verify(Exception exception) { } }
}
