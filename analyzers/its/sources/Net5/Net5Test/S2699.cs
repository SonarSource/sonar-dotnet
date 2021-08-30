using Xunit;

namespace Net5Test
{
    public class S2699
    {
        public void OuterMethod()
        {
            [Fact]
            void WithAssert() => Assert.True(true);

            [Fact]
            void NoAssert() { /* no assert */ }

            [Fact]
            static void StaticNoAssert() { /* no assert */ }
        }
    }
}
