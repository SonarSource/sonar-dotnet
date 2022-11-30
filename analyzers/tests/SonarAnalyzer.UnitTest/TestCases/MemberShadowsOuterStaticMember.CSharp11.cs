public interface OuterInterface
{
    public static int StaticFoo1 => 42;
    public static int StaticFoo2 => 42;
    public static int StaticFoo3 => 42;
    public static int StaticFoo4 => 42;
    public static int StaticFoo5 => 42;

    public static int StaticFoo6 => 42;
    public static int StaticFoo7 => 42;

    public static virtual int StaticVirtualFoo1 => 42;
    public static abstract int StaticAbstractFoo1 { get; }
    public const int ConstFoo1 = 42;

    public abstract class InnerClass
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant

        public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for virtual members, only on type parameters
        public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract members, only on type parameters

        public int ConstFoo1 => 42; // Noncompliant
    }

    public interface InnerInterface
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant

        public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters 
        public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters

        public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for virtual members, only on type parameters
        public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract members, only on type parameters

        public int ConstFoo1 => 42; // Noncompliant
    }
}

public class OuterClass
{
    public static int StaticFoo1 => 42;
    public static int StaticFoo2 => 42;
    public static int StaticFoo3 => 42;
    public static int StaticFoo4 => 42;
    public static int StaticFoo5 => 42;

    public static int StaticFoo6 => 42;
    public static int StaticFoo7 => 42;

    public const int ConstFoo1 = 42;

    public interface InnerInterface
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant

        public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters 
        public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters

        public int ConstFoo1 => 42; // Noncompliant
    }
}
