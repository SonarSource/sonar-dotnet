using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestOnlyProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(10, 5 + 5);
        }
    }
}
