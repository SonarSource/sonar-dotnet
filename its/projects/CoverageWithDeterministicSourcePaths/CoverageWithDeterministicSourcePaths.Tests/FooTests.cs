using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Repro3362.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1() => new Foo().Covered(true);
    }
}
