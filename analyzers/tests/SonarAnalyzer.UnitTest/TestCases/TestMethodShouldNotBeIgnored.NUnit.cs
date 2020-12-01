using System;
using NUnit.Framework;
using MSTest = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    [TestFixture]
    public class NUnitClass
    {
        // False positive: using an MSTest Ignore against an NUnit test does nothing,
        // but we still raise an issue.
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Foo1()
        {
        }

        static object[] DivideCases =
        {
            new object[] { 12, 3, 4 }
        };

        [Datapoint]
        public int Data = 42;

        [Theory]
        [Ignore("a reason")]
        public void Theory3(int arg)
        {
        }
    }

    [Ignore("Ignored because reasons"), TestFixture]
    public class IgnoredClass4
    {
    }
}
