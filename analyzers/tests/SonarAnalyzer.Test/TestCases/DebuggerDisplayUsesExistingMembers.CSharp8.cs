using System;
using System.Diagnostics;

public class AccessModifiers
{
    public class BaseClass
    {
        private protected int PrivateProtectedProperty => 1;

        [DebuggerDisplay("{PrivateProtectedProperty}")] // Compliant
        public int SomeProperty => 1;

        [DebuggerDisplay("{Nonexistent}")]              // Noncompliant
        public int OtherProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{PrivateProtectedProperty}")] // Compliant
        public int OtherProperty => 1;
    }
}
