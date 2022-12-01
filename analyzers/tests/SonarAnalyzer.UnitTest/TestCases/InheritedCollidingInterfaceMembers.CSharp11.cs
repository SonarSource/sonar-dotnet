public interface IAbstractFirst
{
    public static abstract string StaticAbstractMethod(string value);
}

public interface IAbstractSecond
{
    public static abstract string StaticAbstractMethod(string value);
//                                ^^^^^^^^^^^^^^^^^^^^ Secondary
}

public interface IAbstractCommon : IAbstractFirst, IAbstractSecond { } // Noncompliant

public class AbstractCommon : IAbstractCommon
{
    static string IAbstractFirst.StaticAbstractMethod(string value) => $"First: {value}";
    static string IAbstractSecond.StaticAbstractMethod(string value) => $"Second: {value}";
}

public interface IVirtualFirst
{
    public static virtual string StaticVirtualMethod(string value) => value;
}

public interface IVirtualSecond
{
    public static virtual string StaticVirtualMethod(string value) => value;
//                               ^^^^^^^^^^^^^^^^^^^ Secondary
}

public interface IVirtualCommon : IVirtualFirst, IVirtualSecond { }  // Noncompliant

public class Foo
{
    public void ValidAbstractCall<TFirst, TSecond>()
        where TFirst : IAbstractFirst
        where TSecond : IAbstractSecond
    {
        TFirst.StaticAbstractMethod("First is called here");
        TSecond.StaticAbstractMethod("Second is called here");
    }

    public void ValidVirtualCall<TFirst, TSecond>()
        where TFirst : IVirtualFirst
        where TSecond : IVirtualSecond
    {
        TFirst.StaticVirtualMethod("First is called here");
        TSecond.StaticVirtualMethod("Second is called here");
    }

    public void AmbiguousAbstractCall<TCommon>()
        where TCommon : IAbstractCommon
    {
        TCommon.StaticAbstractMethod("Which method am i supposed to resolve here?"); // Error [CS0121] The call is ambiguous between the following methods or properties: 'IAbstractFirst.StaticAbstractMethod(string)' and 'IAbstractSecond.StaticAbstractMethod(string)' 
    }


    public void AmbiguousVirtualCall<TCommon>()
        where TCommon : IVirtualCommon
    {
        TCommon.StaticVirtualMethod("Which method am i supposed to resolve here?"); // Error [CS0121] The call is ambiguous between the following methods or properties: 'IVirtualFirst.StaticVirtualMethod(string)' and 'IVirtualSecond.StaticVirtualMethod(string)' 
    }

    public void CallMethods()
    {
        ValidAbstractCall<AbstractCommon, AbstractCommon>();
        AmbiguousAbstractCall<AbstractCommon>();

        ValidVirtualCall<IVirtualFirst, IVirtualSecond>();
        AmbiguousVirtualCall<IVirtualCommon>();
    }
}
