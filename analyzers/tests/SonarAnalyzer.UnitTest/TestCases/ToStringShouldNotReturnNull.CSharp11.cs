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
