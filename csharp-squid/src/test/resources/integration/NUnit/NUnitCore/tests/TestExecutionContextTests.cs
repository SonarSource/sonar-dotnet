// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;
using System.Security.Principal;
using System.Threading;
using System.Globalization;
using NUnit.Framework;

namespace NUnit.Core.Tests
{
	/// <summary>
    /// Summary description for TestExecutionContextTests.
	/// </summary>
	[TestFixture]
    public class TestExecutionContextTests
	{
		string currentDirectory;
		CultureInfo currentCulture;
        CultureInfo currentUICulture;
        IPrincipal currentPrincipal;

		/// <summary>
		/// Since we are testing the mechanism that saves and
		/// restores contexts, we save manually here
		/// </summary>
		[SetUp]
		public void SaveContext()
		{
			currentDirectory = Environment.CurrentDirectory;
			currentCulture = CultureInfo.CurrentCulture;
            currentUICulture = CultureInfo.CurrentUICulture;
            currentPrincipal = Thread.CurrentPrincipal;
		}

		[TearDown]
		public void RestoreContext()
		{
			Environment.CurrentDirectory = currentDirectory;
			Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
            Thread.CurrentPrincipal = currentPrincipal;
		}

		[Test]
		public void SetAndRestoreCurrentDirectory()
		{
            Assert.AreEqual(currentDirectory, TestExecutionContext.CurrentContext.CurrentDirectory, "Directory not in initial context");

            TestExecutionContext.Save();

            try
            {
                string otherDirectory = System.IO.Path.GetTempPath();
                if (otherDirectory[otherDirectory.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                    otherDirectory = otherDirectory.Substring(0, otherDirectory.Length - 1);
                TestExecutionContext.CurrentContext.CurrentDirectory = otherDirectory;
                Assert.AreEqual(otherDirectory, Environment.CurrentDirectory, "Directory was not set");
                Assert.AreEqual(otherDirectory, TestExecutionContext.CurrentContext.CurrentDirectory, "Directory not in new context");
            }
            finally
            {
                TestExecutionContext.Restore();
            }

			Assert.AreEqual( currentDirectory, Environment.CurrentDirectory, "Directory was not restored" );
            Assert.AreEqual(currentDirectory, TestExecutionContext.CurrentContext.CurrentDirectory, "Directory not in final context");
		}
		
		[Test]
		public void SetAndRestoreCurrentCulture()
		{
            Assert.AreEqual(currentCulture, TestExecutionContext.CurrentContext.CurrentCulture, "Culture not in initial context");

            TestExecutionContext.Save();

            try
            {
                CultureInfo otherCulture =
                    new CultureInfo(currentCulture.Name == "fr-FR" ? "en-GB" : "fr-FR");
                TestExecutionContext.CurrentContext.CurrentCulture = otherCulture;
                Assert.AreEqual(otherCulture, CultureInfo.CurrentCulture, "Culture was not set");
                Assert.AreEqual(otherCulture, TestExecutionContext.CurrentContext.CurrentCulture, "Culture not in new context");
            }
            finally
            {
                TestExecutionContext.Restore();
            }

			Assert.AreEqual( currentCulture, CultureInfo.CurrentCulture, "Culture was not restored" );
            Assert.AreEqual(currentCulture, TestExecutionContext.CurrentContext.CurrentCulture, "Culture not in final context");
		}

        [Test]
        public void SetAndRestoreCurrentUICulture()
        {
            Assert.AreEqual(currentUICulture, TestExecutionContext.CurrentContext.CurrentUICulture, "UICulture not in initial context");

            TestExecutionContext.Save();

            try
            {
                CultureInfo otherCulture =
                    new CultureInfo(currentUICulture.Name == "fr-FR" ? "en-GB" : "fr-FR");
                TestExecutionContext.CurrentContext.CurrentUICulture = otherCulture;
                Assert.AreEqual(otherCulture, CultureInfo.CurrentUICulture, "UICulture was not set");
                Assert.AreEqual(otherCulture, TestExecutionContext.CurrentContext.CurrentUICulture, "UICulture not in new context");
            }
            finally
            {
                TestExecutionContext.Restore();
            }

            Assert.AreEqual(currentUICulture, CultureInfo.CurrentUICulture, "UICulture was not restored");
            Assert.AreEqual(currentUICulture, TestExecutionContext.CurrentContext.CurrentUICulture, "UICulture not in final context");
        }

        [Test]
        public void SetAndRestoreCurrentPrincipal()
        {
            Assert.AreEqual(currentPrincipal, TestExecutionContext.CurrentContext.CurrentPrincipal, "Principal not in initial context");

            TestExecutionContext.Save();

            try
            {
                GenericIdentity identity = new GenericIdentity("foo");
                TestExecutionContext.CurrentContext.CurrentPrincipal = new GenericPrincipal(identity, new string[0]);
                Assert.AreEqual("foo", Thread.CurrentPrincipal.Identity.Name, "Principal was not set");
                Assert.AreEqual("foo", TestExecutionContext.CurrentContext.CurrentPrincipal.Identity.Name, "Principal not in new context");
            }
            finally
            {
                TestExecutionContext.Restore();
            }

            Assert.AreEqual(currentPrincipal, Thread.CurrentPrincipal, "Principal was not restored");
            Assert.AreEqual(currentPrincipal, TestExecutionContext.CurrentContext.CurrentPrincipal, "Principal not in final context");
        }
    }
}
