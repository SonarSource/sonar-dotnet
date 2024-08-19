using NUnit.Framework;

namespace ExternalIssues.TestProject.CS
{
    public class NUnitTest
    {
        [TestCase("This should be integer value")]  // Raises NUnit1001
        public void SampleTest(int thisHasWrongValueType)
        {
            Assert.That(thisHasWrongValueType, Is.EqualTo(42));
        }
    }
}
