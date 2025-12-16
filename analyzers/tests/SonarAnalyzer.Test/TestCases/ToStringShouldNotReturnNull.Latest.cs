using System;

public static class Condition
{
    public static bool When()
    {
        return true;
    }
}

class LocalFunctionReturnsNull
{
    public override string ToString()
    {
        return string.Empty;

        static string Local()
        {
            return null; // Compliant
        }
    }
}

class LambdaReturnsNull
{
    public override string ToString()
    {
        Func<string> expression = () => { return null; }; // Compliant
        Func<string> statment = () => null; // Compliant
        var simple = Simple(s => null); // Compliant
        return string.Empty;
    }

    string Simple(Func<string, string> exp) => exp(null);
}

record RecordReturnsStringEmpty
{
    public override string ToString()
    {
        if (Condition.When())
        { return string.Empty; }
        return string.Empty;
    }
}

record RecordReturnsNull
{
    public override string ToString()
    {
        if (Condition.When())
        { return null; } // Noncompliant
        return null; // Noncompliant
    }
}

public interface SomeInterface
{
    static virtual string ToString()
    {
        return null; // Compliant
    }
}

public interface SomeOtherInterface
{
    static abstract string ToString();
}

public class SomeClass : SomeOtherInterface
{
    public static string ToString()
    {
        return null; // Compliant
    }
}

static class Extensions
{
    extension(SomeClass s)
    {
        string ToString() { return null; }  // Noncompliant FP NET-2877
    }
}

static class StaticExtensions
{
    extension(SomeClass)
    {
        static string ToString() { return null; }  // Compliant: static
    }
}
