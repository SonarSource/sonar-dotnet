using System;
using System.IO;
using NUnit.Framework;
using NUnit.TestData.TestContextData;
using NUnit.TestUtilities;

namespace NUnit.Core.Tests
{
    [TestFixture]
    public class TestContextTests
    {
        [Test]
        public void TestCanAccessItsOwnName()
        {
            Assert.That(TestContext.CurrentContext.Test.Name, Is.EqualTo("TestCanAccessItsOwnName"));
        }

        [Test]
        public void TestCanAccessItsOwnFullName()
        {
            Assert.That(TestContext.CurrentContext.Test.FullName,
                Is.EqualTo("NUnit.Core.Tests.TestContextTests.TestCanAccessItsOwnFullName"));
        }

        [Test]
        [Property("Answer", 42)]
        public void TestCanAccessItsOwnProperties()
        {
            Assert.That(TestContext.CurrentContext.Test.Properties["Answer"], Is.EqualTo(42));
        }

        [Test]
        public void TestCanAccessTestDirectory()
        {
            string testDirectory = TestContext.CurrentContext.TestDirectory;
            Assert.NotNull(testDirectory);
            Assert.That(Directory.Exists(testDirectory));
            Assert.That(File.Exists(Path.Combine(testDirectory, "nunit.core.tests.dll")));
        }

        [Test]
        public void TestCanAccessTestState_PassingTest()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>Inconclusive=>Success"));
            Assert.That(fixture.statusList, Is.EqualTo("Inconclusive=>Inconclusive=>Passed"));
        }

        [Test]
        public void TestCanAccessTestState_FailureInSetUp()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.setUpFailure = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>=>Failure"));
            Assert.That(fixture.statusList, Is.EqualTo("Inconclusive=>=>Failed"));
        }

        [Test]
        public void TestCanAccessTestState_FailingTest()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.testFailure = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>Inconclusive=>Failure"));
            Assert.That(fixture.statusList, Is.EqualTo("Inconclusive=>Inconclusive=>Failed"));
        }

        [Test]
        public void TestCanAccessTestState_IgnoredInSetUp()
        {
            TestStateRecordingFixture fixture = new TestStateRecordingFixture();
            fixture.setUpIgnore = true;
            TestBuilder.RunTestFixture(fixture);
            Assert.That(fixture.stateList, Is.EqualTo("Inconclusive=>=>Ignored"));
            Assert.That(fixture.statusList, Is.EqualTo("Inconclusive=>=>Skipped"));
        }	

	        [Test, RequiresThread]
        public void CanAccessTestContextOnSeparateThread()
        {
            Assert.That(TestContext.CurrentContext.Test.Name, Is.EqualTo("CanAccessTestContextOnSeparateThread"));
        }
	}
}
