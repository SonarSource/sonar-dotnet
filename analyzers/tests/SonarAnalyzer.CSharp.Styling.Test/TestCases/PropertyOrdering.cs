public class AllValid
{
    public static int S1 { get; }
    public static int S2 => 42;
    public static int S3
    {
        get => 42;
    }
    public int I1 { get; }
    public int I2 => 42;
    public int I3
    {
        get => 42;
    }

    protected static int ProtectedS { get; }    // Compliant
    protected int ProtectedI { get; }

    private static int PrivateS { get; }        // Compliant
    private int PrivateI { get; }
}

public class SomeValid
{
    public static int S1 => 42;
    public int I1 => 42;
    public int I2 => 42;
    public static int S2 => 42;  // Noncompliant {{Move this static property above the public instance ones.}}
    //                ^^
    public int I3 => 42;

    private static int PrivateS { get; }    // Compliant, there's no other private instance one
}

public class ProtectedInternal
{
    protected internal int ProtectedI { get; }
    protected static int ProtectedS { get; }    // Noncompliant {{Move this static property above the protected instance ones.}}
}

public class ProtectedPrivate
{
    protected private int ProtectedI { get; }
    protected static int ProtectedS { get; }    // Noncompliant {{Move this static property above the protected instance ones.}}
}

public class AllWrong
{
    public int I1 { get; }
    public int I2 => 42;
    public int I3
    {
        get => 42;
    }
    protected int ProtectedI { get; }
    private int PrivateI { get; }

    public static int S1 { get; }   // Noncompliant {{Move this static property above the public instance ones.}}
    public static int S2 => 42;     // Noncompliant
    public static int S3            // Noncompliant
    {
        get => 42;
    }

    private static int PrivateS { get; }        // Noncompliant {{Move this static property above the private instance ones.}}
    protected static int ProtectedS { get; }    // Noncompliant {{Move this static property above the protected instance ones.}}
}

public record R
{
    public int I => 42;
    public static int S => 42;   // Noncompliant

}

public record struct RS
{
    public int I => 42;
    public static int S => 42;   // Noncompliant
}

public struct Str
{
    public int I => 42;
    public static int S => 42;   // Noncompliant
}
