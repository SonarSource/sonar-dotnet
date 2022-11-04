using System;

namespace Tests.Diagnostics
{
    public interface IAbstractMethodFirst
    {
        public static abstract string StaticAbstractMethod(string value);
    }

    public interface IAbstractMethodSecond
    {
        public static abstract string StaticAbstractMethod(string value);
    }

    public interface IAbstractMethodCommon : IAbstractMethodFirst, IAbstractMethodSecond { } // FN

    public interface IVirtualMethodFirst
    {
        public static virtual string StaticVirtualMethod(string value) => value;
    }

    public interface IVirtualMethodSecond
    {
        public static virtual string StaticVirtualMethod(string value) => value;
    }

    public interface IVirtualMethodCommon : IVirtualMethodFirst, IVirtualMethodSecond { } // FN
}
