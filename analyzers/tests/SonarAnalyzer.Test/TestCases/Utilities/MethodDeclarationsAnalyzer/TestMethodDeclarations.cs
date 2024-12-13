namespace Samples.CSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

public class Address
{
    [TestMethod]
    public string GetZipCode() => "12345";
}

public struct Person
{
    [TestMethod]
    public string GetFullName() => "John Doe";
}

public record class Company
{
    [TestMethod]
    public string GetCompanyName() => "ACME";
}

public record struct Employee
{
    [TestMethod]
    public string GetEmployeeName() => "Jane Doe";
}

public enum EnumDeclaration
{
    Value1,
    Value2
}

public class Visibility
{
    [TestMethod]
    public void PublicMethod() { }

    [TestMethod]
    protected internal void ProtectedInternalMethod() { }

    [TestMethod]
    protected void ProtectedMethod() { }

    [TestMethod]
    internal void InternalMethod() { }

    [TestMethod]
    private protected void PrivateProtectedMethod() { }

    [TestMethod]
    private void PrivateMethod() { }

    [TestMethod]
    void NoAccessModifierMethod() { }

    internal class InternalClass
    {
        [TestMethod]
        public void Method() { }
    }

    private class PrivateClass
    {
        [TestMethod]
        public void Method() { }
    }
}

file class FileClass
{
    [TestMethod]
    public void Method() { }
}

class NoModifiers
{
    [TestMethod]
    public void Method() { }
}

public class MultipleMethods
{
    [TestMethod]
    public void Method1() { }

    [TestMethod]
    public void Method2() { }
}

public class Overloads
{
    [TestMethod]
    public void Method() { }

    [TestMethod]
    public void Method(int i) { }

    [TestMethod]
    public void Method(string s) { }

    [TestMethod]
    public void Method(int i, string s) { }
}

public class GenericClass<T>
{
    [TestMethod]
    public void Method() { }

    [TestMethod]
    public void Method<U>() { }
}

public class WithGenericMethod
{
    [TestMethod]
    public void Method<T>() { }
}

public partial class PartialClass : BaseClass
{
    [TestMethod]
    public void InFirstFile() { }

    public partial void PartialMethod(); // Test method attribute is defined in the other file.
}

public class PropertiesAndIndexers
{
    private int[] values;

    public string Property { get; set; }

    public int this[int i]
    {
        get => values[i];
        set => values[i] = value;
    }
}

public class LocalFunctions
{
    [TestMethod]
    public void Main()
    {
        [TestMethod]
        void LocalFunction() { }
    }
}

public abstract class BaseClass
{
    [TestMethod]
    public void BaseClassMethod() { }

    [TestMethod]
    public virtual void Method() { }
}

public class DerivedClass : BaseClass
{
    [TestMethod]
    public override void Method() { }
}

public class MultipleLevelInheritance : DerivedClass
{
    [TestMethod]
    public void MultipleLevelInheritanceMethod() { }
}

public interface IInterfaceWithTestDeclarations
{
    [TestMethod]
    public string GetZipCode();
}
