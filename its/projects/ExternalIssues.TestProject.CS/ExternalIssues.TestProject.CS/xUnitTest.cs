using Xunit;

namespace ExternalIssues.TestProject.CS
{
    public class xUnitTest
    {
        [Fact]
        public void Parametrized(int thisHasNoInputValue)   // Raises xUnit1001
        {
            Assert.Equal(42, thisHasNoInputValue);
        }
    }
}
