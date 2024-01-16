using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    struct Dummy
    { }

    class MemberInitializedToDefault<T>
    {
        public const int myConst = 0; //Compliant
        public double fieldD1 = 0; // Noncompliant
//                            ^^^
        public double fieldD2 = +0.0; // Noncompliant
        public double fieldD2b = -+-+-0.0; // Noncompliant
        public double fieldD3 = .0; // Noncompliant
        public decimal fieldD4 = .0m; // Noncompliant
        public decimal fieldD5 = .2m;
        public byte b = 0; // Noncompliant
        public char c = '\u0000'; // Noncompliant
        public char c2 = new char(); // Noncompliant
        public bool bo = false; // Noncompliant
        public sbyte sb = +0; // Noncompliant
        public ushort us = -0; // Noncompliant
        public uint ui = +-+-+0; // Noncompliant
        public ulong ul = 0UL; // Noncompliant
        public long l = new long(); // Noncompliant
        public static object o = default(object); // Noncompliant {{Remove this initialization to 'o', the compiler will do that for you.}}
        public object MyProperty { get; set; } = null; // Noncompliant
        public object Property3 => null;
        public object Property4
        {
            get => null;
            set => throw new Exception();
        }

        public event EventHandler MyEvent = null;  // Noncompliant
        public event EventHandler MyEvent2 = (s, e) => { };
    }
}
