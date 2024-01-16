using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    struct Dummy
    { }

    class MemberInitializedToDefault<T>
    {
        public const int myConst = 0; //Compliant
        public double fieldD1; // Fixed
        public double fieldD2; // Fixed
        public double fieldD2b; // Fixed
        public double fieldD3; // Fixed
        public decimal fieldD4; // Fixed
        public decimal fieldD5 = .2m;
        public byte b; // Fixed
        public char c; // Fixed
        public char c2; // Fixed
        public bool bo; // Fixed
        public sbyte sb; // Fixed
        public ushort us; // Fixed
        public uint ui; // Fixed
        public ulong ul; // Fixed
        public long l; // Fixed
        public static object o; // Fixed
        public object MyProperty { get; set; } // Fixed
        public object Property3 => null;
        public object Property4
        {
            get => null;
            set => throw new Exception();
        }

        public event EventHandler MyEvent;  // Fixed
        public event EventHandler MyEvent2 = (s, e) => { };
    }
}
