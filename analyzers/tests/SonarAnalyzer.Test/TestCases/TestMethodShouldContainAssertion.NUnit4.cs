using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestNUnit4
{
    [TestFixture]
    class TestAttributeTests
    {
        [Test]
        public void Test1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^
        {
            var x = 42;
        }

        [Test]
        public void Test2()
        {
            var x = 42;
            ClassicAssert.AreEqual(x, 42);
        }

        [Test]
        public void Test3()
        {
            var x = 42;
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(x, 42);
        }

        [Test]
        public void Test4()
        {
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void Test5()
        {
            ClassicAssert.IsFalse(false);
        }
    }
}
