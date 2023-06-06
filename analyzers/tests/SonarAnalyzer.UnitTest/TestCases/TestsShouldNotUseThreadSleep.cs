using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Threading;
using Xunit;
using Alias = System.Threading.Thread;
using static System.Threading.Thread;

class Compliant
{
    [Test]
    public void TestWithoutThreadSleep()
    {
        Xunit.Assert.Equal("42", "The answer to life, the universe, and everything");
    }

    [Test]
    public void CallToOtherSleepMethod()
    {
        Sleep(42);
    }

    void ThreadSleepNotInTest()
    {
        Thread.Sleep(42);
    }

    public int Property
    {
        get
        {
            Thread.Sleep(1000); // Lovely, but compliant
            return 42;
        }
    }

    private void Sleep(int durartion) { }
}

class NonCompliant
{
    [Test]
    public void ThreadSleepInNUnitTest()
    {
        Thread.Sleep(42); // {{Do not use 'Thread.Sleep()' in a test.}}
//      ^^^^^^^^^^^^^^^^
    }

    [Fact]
    public void ThreadSleepInXUnitTest()
    {
        Thread.Sleep(42); // Noncompliant
    }

    [TestMethod]
    public void ThreadSleepInMSTest()
    {
        Thread.Sleep(42); // Noncompliant
    }

    [Test]
    public void ThreadSleepViaAlias()
    {
        Alias.Sleep(42); // Noncompliant
    }

    [Test]
    public void ThreadSleepViaStaticImport()
    {
        Sleep(42); // Noncompliant
    }

    [Test]
    public void ThreadSleepInLocalFunction()
    {
        Wait();

        void Wait()
        {
            Thread.Sleep(42); // Noncompliant
        }
    }

    [Test]
    public void ThreadSleepInLambdaFunction()
    {
        Action sleep = () => Thread.Sleep(42); // Noncompliant
    }
}
