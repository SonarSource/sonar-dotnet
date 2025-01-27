public interface IAbstractFirst
{
    public static abstract string StaticAbstractMethod(string value);
}

public interface IAbstractSecond
{
    public static abstract string StaticAbstractMethod(string value);
//                                ^^^^^^^^^^^^^^^^^^^^ Secondary {{This member collides with 'IAbstractFirst.StaticAbstractMethod(string)'}}
}

public interface IAbstractCommon : IAbstractFirst, IAbstractSecond { } // Noncompliant

public interface IVirtualFirst
{
    public static virtual string StaticVirtualMethod(string value) => value;
}

public interface IVirtualSecond
{
    public static virtual string StaticVirtualMethod(string value) => value;
//                               ^^^^^^^^^^^^^^^^^^^ Secondary {{This member collides with 'IVirtualFirst.StaticVirtualMethod(string)'}}
}

public interface IVirtualCommon : IVirtualFirst, IVirtualSecond { }  // Noncompliant
