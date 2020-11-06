record Record
{
    private int F0 = 0; // Compliant - unused
    private int F1 = 0; // Compliant - FN
    public int F2 = 0; // Compliant - Public

    void Foo()
    {
        ((F1)) = 42;
        F2 = 42;
        Bar(F1);
    }

    void Bar(int i) { }

    private int fieldInPropertyCompliant = 0;
    public int P1 { get { return fieldInPropertyCompliant; } }

    private int fieldInPropertyNoncompliant = 0; // Compliant - FN
    public int P2 { get { fieldInPropertyNoncompliant = 42; return fieldInPropertyNoncompliant; } }

    private int fieldInPropertyWithInitCompliant = 0;
    public int P3
    {
        get { return fieldInPropertyWithInitCompliant; }
        init { fieldInPropertyWithInitCompliant = value; }
    }
}
