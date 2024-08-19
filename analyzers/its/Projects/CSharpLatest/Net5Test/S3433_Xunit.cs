using Xunit;

namespace Net5Test
{
    public class S3433_Xunit
    {
        public void Method()
        {
            [Fact]
            void NestedFact()
            {
                Assert.True(true);
            }
        }
    }
}
