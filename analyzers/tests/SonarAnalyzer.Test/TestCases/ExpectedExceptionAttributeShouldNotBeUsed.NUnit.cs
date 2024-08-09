using System;
using NUnit.Framework;

namespace Tests.Diagnostics
{
    [TestFixture]
    class Program
    {
        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Noncompliant
        public void TestFoo2()
        {
            var x = true;
            x.ToString();
        }

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public void TestFoo4()
        {
            new object().ToString();
        }

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]  // Compliant - one line
        public string TestFoo6() => new object().ToString();

        [Test]
        public void TestFoo8()
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

        [Test]
        [NUnit.Framework.ExpectedException(typeof(ArgumentNullException))]
        public void NoBody() // Error [CS0501,CS1002]
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8300
    class Repro_8300
    {
        [Test]
        [NUnit.Framework.ExpectedException(typeof(InvalidOperationException))] // Compliant - using ExpectedException makes the test more readable
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
    }
}
