record struct RecordStruct
{
    private int privateUnused = 0; // Compliant - unused
    private int privateUsed = 0; // FN
    internal int @internal = 0; // Compliant
    public int @public = 0; // Compliant - Public
    private int usedInWith = 0; // FN

    void Foo()
    {
        ((privateUsed)) = 42;
        @internal = 42;
        @public = 42;
        Bar(privateUsed);
        var zero = new RecordStruct();
        usedInWith = 4;
        var answer = zero with { @public = usedInWith };
    }

    void Bar(int i) { }

    private int fieldInPropertyCompliant = 0;
    public int P1 { get { return fieldInPropertyCompliant; } }

    private int fieldInPropertyNoncompliant = 0; // FN
    public int P2 { get { fieldInPropertyNoncompliant = 42; return fieldInPropertyNoncompliant; } }

    private int fieldInPropertyWithInitCompliant = 0;
    public int P3
    {
        get { return fieldInPropertyWithInitCompliant; }
        init { fieldInPropertyWithInitCompliant = value; }
    }

    public record struct PositionalRecordStruct(string Input)
    {
        private string inputField = Input; // FN
        public string Output { get { inputField = "2021"; return inputField + " year"; } }
    }
}
