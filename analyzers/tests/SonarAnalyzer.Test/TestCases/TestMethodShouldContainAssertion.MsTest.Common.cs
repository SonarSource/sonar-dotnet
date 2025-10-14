namespace TestMsTest
{
    using System;
    using System.Text;
    using FluentAssertions;
    using NFluent;
    using NSubstitute;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
        public void TestMethod2()
        {
            var x = 42;
            Assert.AreEqual(x, 42);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var x = 42;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(x, 42);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [TestMethod]
        public void TestMethod8() => AssertSomething();

        [TestMethod]
        public void TestMethod9()
        {
            ShouldSomething();
        }

        [TestMethod]
        public void TestMethod10()
        {
            ExpectSomething();
        }

        [TestMethod]
        public void TestMethod11()
        {
            MustSomething();
        }

        [TestMethod]
        public void TestMethod12()
        {
            VerifySomething();
        }

        [TestMethod]
        public void TestMethod13()
        {
            ValidateSomething();
        }

        [TestMethod]
        public void TestMethod14()
        {
            dynamic d = 10;
            Assert.AreEqual(d, 10.0);
        }

        [TestMethod]
        public void TestMethod15()
        {
            var x = 42;
            AreEqual(x, 42);
        }

        [TestMethod]
        public void TestMethod16()
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException("You failed me!");
        }

        [TestMethod]
        public void TestMethod17()
        {
            var exception = new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException("You failed me!");
            throw exception;
        }

        [TestMethod]
        public void TestMethod18() // Noncompliant
        {
            throw new Exception("You failed me!");
        }

        [TestMethod]
        public void TestMethod18_UndefinedException() // Noncompliant
        {
            throw new UndefinedTypeException("You failed me!"); // Error [CS0246]
        }

        [TestMethod]
        [Ignore]
        public void TestMethod19() // Don't raise on skipped test methods
        {
        }

        [TestMethod]
        public void TestMethod20() // Noncompliant
        {
            var x = 42;
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestMethod21()
        {
            Check.ThatCode(() => 42).WhichResult().IsStrictlyPositive();
        }

        [TestMethod]
        public void TestMethod22()
        {
            throw new NFluent.Kernel.FluentCheckException("You failed me!");
        }

        [TestMethod]
        public void ArrowMethod1() => DoNothing(); // Noncompliant
//                  ^^^^^^^^^^^^

        [TestMethod]
        public void ArrowMethod2() => AssertSomething();

        public void DoNothing() { }

        public void AssertSomething()
        {
            Assert.IsTrue(true);
        }

        public void ShouldSomething()
        {
            Assert.IsTrue(true);
        }

        public void ExpectSomething()
        {
            Assert.IsTrue(true);
        }

        public void MustSomething()
        {
            Assert.IsTrue(true);
        }

        public void VerifySomething()
        {
            Assert.IsTrue(true);
        }

        public void ValidateSomething()
        {
            Assert.IsTrue(true);
        }

        [DerivedTestMethod]
        public void DerivedTestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^^^^^^^^^^^^
        {
            var x = 42;
        }

        [DerivedTestMethod]
        public void DerivedTestMethod2()
        {
            var x = 42;
            Assert.AreEqual(x, 42);
        }
    }

    [TestClass]
    public class Program_DataTestMethod
    {
        [DataTestMethod]
        public void TestMethod1() // Noncompliant {{Add at least one assertion to this test case.}}
//                  ^^^^^^^^^^^
        {
            var x = 42;
        }

        [DataTestMethod]
        public void TestMethod2()
        {
            var x = 42;
            Assert.AreEqual(x, 42);
        }

        [DataTestMethod]
        public void TestMethod3()
        {
            var x = 42;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(x, 42);
        }

        [DataTestMethod]
        public void TestMethod5()
        {
            var x = 42;
            x.Should().Be(42);
        }

        [DataTestMethod]
        public void TestMethod8()
        {
            AssertSomething();
        }

        [DataTestMethod]
        public void TestMethod9()
        {
            ShouldSomething();
        }

        [DataTestMethod]
        public void TestMethod10()
        {
            ExpectSomething();
        }

        [DataTestMethod]
        public void TestMethod11()
        {
            MustSomething();
        }

        [DataTestMethod]
        public void TestMethod12()
        {
            VerifySomething();
        }

        [DataTestMethod]
        public void TestMethod13()
        {
            ValidateSomething();
        }

        [DataTestMethod]
        public void TestMethod14()
        {
            dynamic d = 10;
            Assert.AreEqual(d, 10.0);
        }

        [DataTestMethod]
        [Ignore]
        public void TestMethod15() // Don't raise on skipped test methods
        {
        }

        public void AssertSomething()
        {
            Assert.IsTrue(true);
        }

        public void ShouldSomething()
        {
            Assert.IsTrue(true);
        }

        public void ExpectSomething()
        {
            Assert.IsTrue(true);
        }

        public void MustSomething()
        {
            Assert.IsTrue(true);
        }

        public void VerifySomething()
        {
            Assert.IsTrue(true);
        }

        public void ValidateSomething()
        {
            Assert.IsTrue(true);
        }
    }

    public class Class1
    {
        public int Add(int a, int b) => a + b;
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1() // Noncompliant
        {
            var result = new Class1().Add(1, 2);
        }
    }

    /// <summary>
    /// The NSubstitute and Shoudly assertions are extensively verified in the NUnit test files.
    /// Here we just do a simple test to confirm that the errors are not raised in conjunction with MsTest.
    /// </summary>
    [TestClass]
    public class NSubstituteAndShouldlyTests
    {
        [TestMethod]
        public void Received() => Substitute.For<IDisposable>().Received().Dispose();

        [TestMethod]
        public void Shouldly()
        {
            int[] array = { 1, 2, 3 };
            array.ShouldAllBe(x => x > 0);
        }
    }

    [TestClass]
    public class WithHelperMethods
    {
        [TestMethod]
        public void TestMethod1()
        {
            DoTheWork("42", 42);
        }

        [TestMethod]
        public void TestMethod2() =>
            DoTheWork("42", 42);

        [TestMethod]
        public void Complex()
        {
            int cnt = 42;
            var sb = new System.Text.StringBuilder();
            sb.Append("This invocation doesn't have syntax tree for Append");
            DoNothing(sb);
            for(int i = 0; i < cnt; i++)
            {
                DoNothing(sb);
                if (i % 2 == 0)
                {
                    sb.Append(i.ToString());
                    sb.Clear();
                    try
                    {
                        sb.Append("42");
                        DoTheWork(sb.ToString(), 42);
                    }
                    finally
                    {
                        DoNothing(null);
                    }

                }
                DoNothing(sb);
            }
        }

        private void DoNothing(StringBuilder sb)
        {
            // Empty path
        }

        [TestMethod]
        public void Recursion() =>      // Noncompliant
            Recursion(100);

        private void Recursion(int cnt)
        {
            if (cnt > 0)
                Recursion(cnt - 1);
        }

        [TestMethod]
        public void NestedTwoTimes() =>
            NestedTwo();

        [TestMethod]
        public void NestedThreeTimes() => // Noncompliant, it's too deep
            NestedThree();

        private void NestedThree() =>
            NestedTwo();

        private void NestedTwo() =>
            DoTheWork("42", 42);

        private void DoTheWork(string s, int i) =>
            Assert.AreEqual(i.ToString(), s);

        [TestMethod]
        public void TestMethod3() =>
            DoTheWorkByInvocationName("42", 42);

        private void DoTheWorkByInvocationName(string s, int i) =>
            i.ToString().Should().Be(s);

        [TestMethod]
        public void TestMethod4() =>
            DoTheWorkWithThrow();

        private void DoTheWorkWithThrow()
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException("You failed me!");
        }

        [TestMethod]
        public void MixedSyntaxTrees() =>
            HelperFromAnotherSyntaxTree.Is42(42);

        [TestMethod]
        public void MixedSyntaxTreesNested() =>
            HelperFromAnotherSyntaxTree.Is42Nested(42);

        [TestMethod]
        public void MixedSyntaxTreesMissingAssertion() =>   // Noncompliant
            HelperFromAnotherSyntaxTree.DoNothing();

        [TestMethod]
        public void AssertionIsInConstructor()   // Noncompliant FP, not supported
        {
            var x = new BadDesign("42", 42);
        }

        [TestMethod]
        public void AssertionIsInSetter()       // Noncompliant FP, not supported
        {
            var x = new BadDesign();
            x.Property = 42;
        }

        private class BadDesign
        {
            public BadDesign() { }

            public BadDesign(string s, int i) =>
                Assert.AreEqual(i.ToString(), s);

            public int Property
            {
                set => Assert.AreEqual(value, 42);
            }
        }
    }

    public class DerivedTestMethodAttribute : TestMethodAttribute
    {

    }

}
