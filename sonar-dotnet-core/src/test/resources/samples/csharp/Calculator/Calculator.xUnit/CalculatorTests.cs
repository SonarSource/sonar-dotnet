namespace Calculator.xUnit;

public class Tests
{
    [Fact]
    public void TestMethod1()
    {
        var calculator = new Calculator();
        var result = calculator.Add(1, 2, x => x > 0);
        Assert.Equal(3, result);
    }
}

public abstract class BaseClass<T> where T : class
{
    [Fact]
    public void TestMethodInBaseClass() =>
        Assert.Equal(3, 3);

    [Fact]
    public virtual void VirtualMethodInBaseClass() =>
        Assert.Equal(1, 1);
}

public sealed class Derived : BaseClass<string>
{
    [Fact]
    public override void VirtualMethodInBaseClass() =>
        Assert.Equal(1, 1);
}

public class GenericTests
{
    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    public void GenericTestMethod<T>(Type type) => Console.WriteLine(type);
}

public sealed class GenericDerivedFromGenericClass<T> : BaseClass<T> where T : class
{
    [Fact]
    public void GenericDerivedFromGenericClass_PassMethod() =>
        Assert.Equal(1, 1);
}
