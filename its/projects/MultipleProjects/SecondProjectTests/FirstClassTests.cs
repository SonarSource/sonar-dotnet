using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecondProject;

namespace SecondProjectTests
{
    [TestClass]
    public class FirstClassTests
    {
        [TestMethod]
        public void Ten()
        {
            var underTest = new FirstClass();
            Assert.AreEqual("ten", underTest.SwitchExpression(10));
        }

        [TestMethod]
        public void Twenty()
        {
            var underTest = new FirstClass();
            Assert.AreEqual("twenty", underTest.SwitchExpression(20));
        }

        [TestMethod]
        public void IfTrue()
        {
            var underTest = new FirstClass();
            Assert.AreEqual("ten", underTest.IfElse(true));
        }
    }
}
