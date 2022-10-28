public interface SomeInterface
{
    static virtual string ToString()
    {
        return null; // Noncompliant FP
    }
}

public interface SomeOtherInterface
{
    static abstract string ToString();
}

public class SomeClass: SomeOtherInterface
{
    public static string ToString()
    {
        return null; // Noncompliant FP
    }
}
