using System;

public unsafe class NativeInts
{
    protected IntPtr _intPtr; // Noncompliant
    public UIntPtr _uIntPtr; // Noncompliant

    protected nint _nint; // Compliant, FN, nint should be treated as a pointer type
    public nuint _nuint; // Compliant, FN, nuint should be treated as a pointer type
}
