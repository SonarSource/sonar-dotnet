using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class PublicClass
    {
        public int myValue = 42; // Noncompliant {{Make this field 'private' and encapsulate it in a 'public' property.}}

        public readonly int MagicNumber = 42;
        public const int AnotherMagicNumber = 998001;

        private class PrivateClass
        {
            public int myValue = 42;

            public readonly int MagicNumber = 42;
            public const int A = 998001;
        }

        protected class ProtectedClass
        {
            public int myValue = 42; // Noncompliant

            public readonly int MagicNumber = 42;
            public const int B = 998001;
        }
    }

    internal class InternalClass
    {
        public int myValue = 42;

        public readonly int MagicNumber = 42;
        public const int AnotherMagicNumber = 998001;

        public class InnerPublicClass
        {
            public int mySubValue = 42;
        }

        private class PrivateClass
        {
            public int myValue = 42;

            public readonly int MagicNumber = 42;
            public const int A = 998001;
        }

        protected class ProtectedClass
        {
            public int myValue = 42;

            public readonly int MagicNumber = 42;
            public const int B = 998001;
        }
    }

    public partial class PartialClass
    {

    }

    partial class PartialClass
    {
        public int myValue = 0; // Noncompliant
    }
}