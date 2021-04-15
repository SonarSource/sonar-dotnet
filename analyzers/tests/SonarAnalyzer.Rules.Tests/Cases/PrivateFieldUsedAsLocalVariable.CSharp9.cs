record Record
{
    private int privateUnused = 0; // Compliant - unused
    private int privateUsed = 0; // Compliant - FN
    private protected int privateProtected = 0; // Compliant
    internal int @internal = 0; // Compliant
    protected internal int protectedInternal = 0; // Compliant
    protected int @protected = 0; // Compliant
    public int @public = 0; // Compliant - Public

    void Foo()
    {
        ((privateUsed)) = 42;
        privateProtected = 42;
        @internal = 42;
        protectedInternal = 42;
        @protected = 42;
        @public = 42;
        Bar(privateUsed);
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
