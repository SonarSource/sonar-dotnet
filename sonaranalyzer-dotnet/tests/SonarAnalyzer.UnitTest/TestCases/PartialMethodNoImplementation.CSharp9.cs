public partial record Record
{
    partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}

    private partial void M2(); // Noncompliant - FP (there is a compiler error already)
    internal partial void M3(); // Noncompliant - FP (there is a compiler error already)
    protected partial void M4(); // Noncompliant - FP (there is a compiler error already)
    protected internal partial void M5(); // Noncompliant - FP (there is a compiler error already)
    public partial void M6(); // Noncompliant - FP (there is a compiler error already)

    public partial void M7(); // Compliant
}

public partial record Record
{
    public partial void M7()
    {

    }
}
