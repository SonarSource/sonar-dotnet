using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net5Test
{
    [TestClass]
    public class S3433_Mstest
    {
        public void Method()
        {
            [TestMethod]
            void NestedTest()
            {
                Assert.IsTrue(true);
            }
        }
    }
}
