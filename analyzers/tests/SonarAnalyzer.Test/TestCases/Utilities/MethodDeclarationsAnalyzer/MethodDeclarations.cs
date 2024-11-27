namespace Samples;

public class Address
{
    public string GetZipCode() => "12345";
}

public struct Person
{
    public string GetFullName() => "John Doe";
}

public record class Company
{
    public string GetCompanyName() => "ACME";
}

public record struct Employee
{
    public string GetEmployeeName() => "Jane Doe";
}

public enum EnumDeclaration
{
    Value1,
    Value2
}

public class Visibility
{
    public void PublicMethod() { }

    protected internal void ProtectedInternalMethod() { }

    protected void ProtectedMethod() { }

    internal void InternalMethod() { }

    private protected void PrivateProtectedMethod() { }

    private void PrivateMethod() { }

    void NoAccessModifierMethod() { }

    internal class InternalClass
    {
        public void Method() { }
    }

    private class PrivateClass
    {
        public void Method() { }
    }
}

file class FileClass
{
    public void Method() { }
}

class NoModifiers
{
    public void Method() { }
}

public class MultipleMethods
{
    public void Method1() { }

    public void Method2() { }
}

public class Overloads
{
    public void Method() { }

    public void Method(int i) { }

    public void Method(string s) { }

    public void Method(int i, string s) { }
}

public class GenericClass<T>
{
    public void Method() { }

    public void Method<U>() { }
}

public class WithGenericMethod
{
    public void Method<T>() { }
}

public partial class PartialClass
{
    public void InFirstFile() { }

    public partial void PartialMethod();
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
    public void Main()
    {
        void LocalFunction() { }
    }
}
