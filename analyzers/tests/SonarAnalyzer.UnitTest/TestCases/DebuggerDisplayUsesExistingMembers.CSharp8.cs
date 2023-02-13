using System;
using System.Diagnostics;

public class AccessModifiers
{
    public class BaseClass
    {
        private protected int PrivateProtectedProperty => 1;

        [DebuggerDisplay("{PrivateProtectedProperty}")]
        public int SomeProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{PrivateProtectedProperty}")]
        public int OtherProperty => 1;
    }
}
