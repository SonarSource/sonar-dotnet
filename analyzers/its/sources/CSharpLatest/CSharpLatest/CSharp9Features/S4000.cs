using System;

namespace CSharpLatest.CSharp9Features;

public unsafe class S4000
{
    protected int* p1; // Noncompliant
    public IntPtr p2; // Noncompliant
    public delegate* unmanaged[Cdecl]<int, int> f1; // Noncompliant
}
