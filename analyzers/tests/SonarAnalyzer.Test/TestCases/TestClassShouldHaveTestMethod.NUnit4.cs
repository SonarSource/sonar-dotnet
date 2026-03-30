using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests.Diagnostics
{
    [TestFixture]
    class ClassTest1 // Noncompliant
//        ^^^^^^^^^^
    {
    }

    [TestFixture]
    public class ClassTest11
    {
        [TestCaseSource("DivideCases")]
        public void DivideTest(int n, int d, int q)
        {
            ClassicAssert.AreEqual(q, n / d);
        }

        static object[] DivideCases =
        {
            new object[] { 12, 3, 4 }
        };
    }
}
