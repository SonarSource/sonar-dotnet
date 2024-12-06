namespace Calculator.NUnit3;

[TestFixture]
public class Tests
{
    [Test]
    public void TestMethod1()
    {
        var calculator = new Calculator();
        var result = calculator.Add(1, 2, x => x > 0);
        Assert.That(result, Is.EqualTo(3));
    }
}

[TestFixture]
public class BaseClass<T> where T : class
{
    [Test]
    public void TestMethodInBaseClass() =>
        Assert.AreEqual(1, 1);

    [Test]
    public virtual void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestFixture]
public sealed class Derived : BaseClass<string>
{
    [Test]
    public override void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestFixture]
public sealed class GenericCalculatorTests<T> : BaseClass<T> where T : class
{
    [Test]
    public void Method() =>
        Assert.AreEqual(1, 1);

    [Test]
    public void GenericMethod<T>() =>
        Assert.AreEqual(1, 1);

    [Test]
    public override void VirtualMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}

[TestFixture]
public class GenericTests
{
    [TestCase(42)]
    [TestCase("string")]
    [TestCase(double.Epsilon)]
    public void GenericTest<T>(T instance)
    {
        Console.WriteLine(instance);
    }
}
