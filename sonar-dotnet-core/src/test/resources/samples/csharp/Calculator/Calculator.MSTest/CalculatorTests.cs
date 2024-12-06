namespace Calculator.Tests;

[TestClass]
public sealed class CalculatorTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var calculator = new Calculator();
        var result = calculator.Add(1, 2, x => x > 0);
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void GenericMethod<T>() where T : class =>
        Assert.AreEqual(1, 1);
}

[TestClass]
public abstract class BaseClass<T> where T : class
{
    [TestMethod]
    public void TestMethodInBaseClass() =>
        Assert.AreEqual(1, 1);

    [TestMethod]
    public virtual void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestClass]
public sealed class Derived : BaseClass<string>
{
    [TestMethod]
    public override void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestClass]
public sealed class GenericCalculatorTests<T> : BaseClass<T> where T : class
{
    [TestMethod]
    public void Method() =>
        Assert.AreEqual(1, 1);

    [TestMethod]
    public void GenericMethod<T>() =>
        Assert.AreEqual(1, 1);

    [TestMethod]
    public override void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestClass]
public class GenericTests
{
    [DataTestMethod]
    [DataRow(typeof(int))]
    [DataRow(typeof(string))]
    public void GenericTestMethod<T>(Type type)
    {
    }
}
