using FirstProject;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirstProjectTests
{
    [TestClass]
    public class FirstProjectTests
    {
        [TestMethod]
        public void CallMethods()
        {
            FirstClass firstClass = new FirstClass();
            Assert.AreEqual(0, firstClass.BodyMethod());
            Assert.AreEqual(0, firstClass.ArrowMethod(true));
        }

        [TestMethod]
        public void CallFirstClassViaSecondClass()
        {
            SecondClass secondClass = new SecondClass();
            secondClass.CallFirstClass(new FirstClass());
        }
    }
}
