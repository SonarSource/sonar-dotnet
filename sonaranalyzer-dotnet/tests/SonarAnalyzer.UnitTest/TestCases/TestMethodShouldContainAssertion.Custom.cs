using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomTests
{
    using TestFramework;

    [TestClass]
    public class Program
    {
        [TestMethod]
        public void TestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^^^^^
        {
            var x = 42;
        }

        [TestMethod]
        public void TestMethod2() // Noncompliant FP
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
        public void TestMethod5() // Noncompliant FP
        {
            var validator = new Validator();
            validator.InstanceWithAttribute();
        }

        [TestMethod]
        public void TestMethod6() => // Noncompliant FP
            new Validator().InstanceWithAttributeAndArg(null);
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
    public class AssertionMethodAttribute : Attribute { }
    public class NotAssertionMethodAttribute : Attribute { } // AssertionMethodAttribute doesn't count as an assertion method attribute
}
