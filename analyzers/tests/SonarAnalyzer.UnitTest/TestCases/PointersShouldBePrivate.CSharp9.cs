using System;

unsafe public record Record
{
    private IntPtr p1;
    protected int* p2; // Noncompliant
    protected readonly IntPtr p3;
    public IntPtr p4; // Noncompliant

    private UIntPtr p5;
    protected char* p6; // Noncompliant
    protected readonly UIntPtr p7;
    public UIntPtr p8; // Noncompliant

    public delegate* managed<int, int> f1;
    protected delegate* managed<int, int> f2;
    protected readonly delegate* managed<int, int> f3;
    private readonly delegate* managed<int, int> f4;
    public delegate* unmanaged[Cdecl]<int, int> f5; // Noncompliant
    protected delegate* unmanaged<int, int> f6; // Noncompliant
    protected readonly delegate* unmanaged<int, int> f7;
    private readonly delegate* unmanaged<int, int> f8;
    protected delegate* unmanaged[Fastcall]<int, int> f9; // Noncompliant
    public delegate* unmanaged<int, int> f10; // Noncompliant
}
