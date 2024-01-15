using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

    public struct Struct
    {
        public ushort value; // Noncompliant
    }

    [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Ansi)]
    public struct InteropStruct
    {
        [FieldOffset(0)] public ushort value; // Compliant - for interop code
    }

    [StructLayout(LayoutKind.Sequential)]
    public class InteropClass
    {
        public string value; // Compliant - for interop code
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8504
    [Serializable]
    public class Repro_8504
    {
        public string type;     // Compliant
        public string key;      // Compliant
        [NonSerialized]
        public string value;    // Noncompliant
    }
}
