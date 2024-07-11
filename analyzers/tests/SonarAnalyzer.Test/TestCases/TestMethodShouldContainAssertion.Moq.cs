namespace TestMoq
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    public delegate void CalculatorEvent(int i, bool b);
    internal interface ICalculator
    {
        int Property { get; set; }
        event CalculatorEvent Event;
        int Add(int a, int b);
    }

    [TestClass]
    public class MoqVerifyTests
    {
        [TestMethod]
        public void MoqVerify()
        {
            var mock = new Mock<ICalculator>();

            mock.Verify(x => x.Add(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void MoqVerifyAdd()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifyAdd(x => x.Event += It.IsAny<CalculatorEvent>());
        }

        [TestMethod]
        public void MoqVerifyRemove()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifyRemove(x => x.Event -= It.IsAny<CalculatorEvent>(), Times.AtMostOnce());
        }

        [TestMethod]
        public void MoqVerifySet()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifySet(x => x.Property = It.IsAny<int>());
        }

        [TestMethod]
        public void MoqVerifyGet()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifyGet(x => x.Property);
        }

        [TestMethod]
        public void MoqVerifyNoOtherCalls()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void MoqVerifyAll()
        {
            var mock = new Mock<ICalculator>();

            mock.VerifyAll();
        }

        [TestMethod]
        public void MoqNoVerify() // Noncompliant
        {
            var mock = new Mock<ICalculator>();
        }
    }
}
