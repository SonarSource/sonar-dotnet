using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpConsoleApp.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(10, Program.Passthrough(10));
        }
    }
}
