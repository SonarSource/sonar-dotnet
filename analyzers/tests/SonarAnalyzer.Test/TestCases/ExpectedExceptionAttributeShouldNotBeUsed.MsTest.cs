using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Diagnostics
{
    class Program
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo1()
        {
            var x = true;
            x.ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))] // Compliant - one line
        public void TestFoo3()
        {
            new object().ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))] // Compliant - FN
        public void TestMultineFalseNegative()
        {
            {
                new object().ToString();
                new object().ToString();
                new object().ToString();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public string TestFoo5() => new object().ToString();

        [TestMethod]
        public void TestFoo7()
        {
            bool callFailed = false;
            try
            {
                //...
            }
            catch (ArgumentNullException)
            {
                callFailed = true;
            }
        }

        [TestMethod]
        public void TestWithThrowsAssertation()
        {
            object o = new object();
            Assert.ThrowsException<ArgumentNullException>(() => o.ToString());
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8300
    class Repro_8300
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Compliant
        public void AssertInFinally()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            finally
            {
                Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Noncompliant
        public void NoAssertInFinally()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            finally
            {
                Console.WriteLine("No Assert");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Noncompliant
        public void NoAssertInCatch()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException e)
            {
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Compliant
        public void AssertInCatch()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Compliant
        public void AssertInAllCatch()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            catch
            {
                Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Compliant
        public void AssertInFinallyWithCatch()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(Console.ForegroundColor);
            }
            finally
            {
                Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))] // Noncompliant - FP
        public void AssertInCatchWithFinally()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ConsoleColor.Black, Console.ForegroundColor);
            }
            finally
            {
                Console.WriteLine(Console.ForegroundColor);
            }
        }
    }
}
