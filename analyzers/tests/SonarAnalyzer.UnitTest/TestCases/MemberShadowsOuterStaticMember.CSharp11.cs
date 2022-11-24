public interface OuterInterfaceWithStaticMember
{
    public static int StaticFoo1 => 42;
    public static int StaticFoo2 => 42;
    public static int StaticFoo3 => 42;
    public static int StaticFoo4 => 42;
    public static int StaticFoo5 => 42;

    public static int StaticFoo6 => 42;
    public static int StaticFoo7 => 42;

    public abstract class InnerClass
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant
    }

    public interface InnerInterface
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant

        public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public class OuterClassWithStaticMember
{
    public static int StaticFoo1 => 42;
    public static int StaticFoo2 => 42;
    public static int StaticFoo3 => 42;
    public static int StaticFoo4 => 42;
    public static int StaticFoo5 => 42;

    public static int StaticFoo6 => 42;
    public static int StaticFoo7 => 42;

    public interface InnerInterface
    {
        public int StaticFoo1 => 42; // Noncompliant
        public const int StaticFoo2 = 42; // Noncompliant
        public virtual int StaticFoo3 => 42; // Noncompliant
        public abstract int StaticFoo4 { get; } // Noncompliant
        public static int StaticFoo5 => 42; // Noncompliant

        public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public interface OuterInterfaceWithStaticVirtualMember
{
    public static virtual int StaticVirtualFoo1 => 42;
    public static virtual int StaticVirtualFoo2 => 42;
    public static virtual int StaticVirtualFoo3 => 42;
    public static virtual int StaticVirtualFoo4 => 42;
    public static virtual int StaticVirtualFoo5 => 42;

    public static virtual int StaticVirtualFoo6 => 42;
    public static virtual int StaticVirtualFoo7 => 42;

    public abstract class InnerClass
    {
        public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public const int StaticVirtualFoo2 = 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public virtual int StaticVirtualFoo3 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public abstract int StaticVirtualFoo4 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static int StaticVirtualFoo5 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }

    public interface InnerInterface
    {
        public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public const int StaticVirtualFoo2 = 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public virtual int StaticVirtualFoo3 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public abstract int StaticVirtualFoo4 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static int StaticVirtualFoo5 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type

        public static virtual int StaticVirtualFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int StaticVirtualFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public interface OuterInterfaceWithStaticAbstractMember
{
    public static abstract int StaticAbstractFoo1 { get; }
    public static abstract int StaticAbstractFoo2 { get; }
    public static abstract int StaticAbstractFoo3 { get; }
    public static abstract int StaticAbstractFoo4 { get; }
    public static abstract int StaticAbstractFoo5 { get; }

    public static abstract int StaticAbstractFoo6 { get; }
    public static abstract int StaticAbstractFoo7 { get; }

    public abstract class InnerClass
    {
        public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public const int StaticAbstractFoo2 = 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public virtual int StaticAbstractFoo3 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public abstract int StaticAbstractFoo4 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static int StaticAbstractFoo5 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }

    public interface InnerInterface
    {
        public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public const int StaticAbstractFoo2 = 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public virtual int StaticAbstractFoo3 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public abstract int StaticAbstractFoo4 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static int StaticAbstractFoo5 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type

        public static virtual int StaticAbstractFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int StaticAbstractFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public interface OuterInterfaceWithConstMember
{
    public const int ConstFoo1 = 42;
    public const int ConstFoo2 = 42;
    public const int ConstFoo3 = 42;
    public const int ConstFoo4 = 42;
    public const int ConstFoo5 = 42;

    public const int ConstFoo6 = 42;
    public const int ConstFoo7 = 42;

    public abstract class InnerClass
    {
        public int ConstFoo1 => 42; // Noncompliant
        public const int ConstFoo2 = 42; // Noncompliant
        public virtual int ConstFoo3 => 42; // Noncompliant 
        public abstract int ConstFoo4 { get; } // Noncompliant
        public static int ConstFoo5 => 42; // Noncompliant
    }

    public interface InnerInterface
    {
        public int ConstFoo1 => 42; // Noncompliant
        public const int ConstFoo2 = 42; // Noncompliant
        public virtual int ConstFoo3 => 42; // Noncompliant
        public abstract int ConstFoo4 { get; } // Noncompliant
        public static int ConstFoo5 => 42; // Noncompliant

        public static virtual int ConstFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int ConstFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public class OuterClassWithConstMember
{
    public const int ConstFoo1 = 42;
    public const int ConstFoo2 = 42;
    public const int ConstFoo3 = 42;
    public const int ConstFoo4 = 42;
    public const int ConstFoo5 = 42;

    public const int ConstFoo6 = 42;
    public const int ConstFoo7 = 42;

    public interface InnerInterface
    {
        public int ConstFoo1 => 42; // Noncompliant
        public const int ConstFoo2 = 42; // Noncompliant
        public virtual int ConstFoo3 => 42; // Noncompliant
        public abstract int ConstFoo4 { get; } // Noncompliant
        public static int ConstFoo5 => 42; // Noncompliant

        public static virtual int ConstFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
        public static abstract int ConstFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, needs implementing type
    }
}

public interface OuterTypes
{
    public class SomeClass { }
    public interface SomeInterface { }

    public class SomeWeirdClass { }
    public interface SomeWeirdInterface { }

    public interface InnerTypes
    {
        public class SomeClass { } // Noncompliant
        public interface SomeInterface { } // Noncompliant

        public interface SomeWeirdClass { } // Noncompliant
        public class SomeWeirdInterface { } // Noncompliant
    }
}

