
public class Sample
{
    public string GetName() => "Lorem ipsum";   // Noncompliant {{Do not use 'Get' prefix. Just don't.}}
    //            ^^^^^^^
    public int GetValue() => 42;                // Noncompliant
    public int Get2items() => 2;                // Noncompliant, and wrong

    public int Getvalue() => 42;                // Wrong, but compliant
    public int getValue() => 42;                // Wrong, but compliant
    public int Get() => 42;                     // Wrong, but compliant

    public object FindGetGetterGet() => null;   // Compliant
    public object Getter() => null;             // Compliant
    public object Getaway() => null;            // Compliant
    public object Getup() => null;              // Compliant

    public void GetLier() { }                   // Wrong due to void, but compliant

    public bool GetIsTrue(bool value)           // Noncompliant
    {
        return value == true;
    }

    public int Parameter(int GetSometing) => 42; // Wrong, but compliant

    public string LocalFunction()
    {
        return GetName(GetValue(GetLost()));    // Ugly, but compliant. We raise on declarations

        int GetValue(bool arg) => 42;   // Noncompliant
        string GetName(int count)       // Noncompliant
        {
            return count.ToString();
        }
        static bool GetLost() => true;  // Noncompliant
    }

    public int GetProperty => 42;       // Wrong, but compliant
    public int GetField = 42;           // Wrong, but compliant

    private class Nested
    {
        public int GetValue() => 42;    // Noncompliant
    }
}

public abstract class Base
{
    public abstract int GetValue();             // Noncompliant
    public abstract int GetProperty { get; }    // Wrong, but compliant

    public virtual string GetName() => null;    // Noncompliant
}

public class Derived : Base
{
    public override int GetValue() => 42;           // Compliant, unfortunately
    public override int GetProperty => 42;          // Compliant, unfortunately
    public override string GetName() => "Lorem";    // Compliant, unfortunately
}

public interface IGet
{
    int GetProperty { get; }    // Wrong, but compliant
    int GetValue();             // Noncompliant
}

public class ImplicitInterface : IGet
{
    public int GetValue() => 42;    // Compliant, unfortunately
    public int GetProperty => 42;   // Wrong, but compliant
}

public class ExplicitInterface : IGet
{
    int IGet.GetValue() => 42;      // Compliant, unfortunately
    int IGet.GetProperty => 42;     // Wrong, unfortunately
}

public class GetGettingGetter
{
    public GetGettingGetter() { }   // Wrong, but compliant
}

public partial class Partial
{
    public partial int GetValue();          // Noncompliant, both are wrong
}

public partial class Partial
{
    public partial int GetValue() => 42;    // Noncompliant, both are wrong
}
