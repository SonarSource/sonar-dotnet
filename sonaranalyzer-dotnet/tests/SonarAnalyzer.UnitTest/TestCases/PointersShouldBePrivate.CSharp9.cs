using System;

unsafe public record Record
{
    private IntPtr p1;
    protected IntPtr p2; // Noncompliant
    protected readonly IntPtr p3;
    public IntPtr p4; // Noncompliant

    private UIntPtr p5;
    protected UIntPtr p6; // Noncompliant
    protected readonly UIntPtr p7;
    public UIntPtr p8; // Noncompliant

    /*
     * This rule is searching only for IntPtr and UIntPtr types.
     * For function pointers we should investigate if the same reasoning applies
     * and if it makes sense to update the rule or to create another one.
     */
    public delegate* managed<int, int> f1;
    protected delegate* managed<int, int> f2;
    protected readonly delegate* managed<int, int> f3;
    private readonly delegate* managed<int, int> f4;

    public delegate* unmanaged<int, int> f5;
    protected delegate* unmanaged<int, int> f6;
    protected readonly delegate* unmanaged<int, int> f7;
    private readonly delegate* unmanaged<int, int> f8;
}
