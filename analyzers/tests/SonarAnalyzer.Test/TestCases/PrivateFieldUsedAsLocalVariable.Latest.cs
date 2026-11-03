record Record
{
    private int privateUnused = 0; // Compliant - unused
    private int privateUsed = 0; // Noncompliant
    private protected int privateProtected = 0; // Compliant
    internal int @internal = 0; // Compliant
    protected internal int protectedInternal = 0; // Compliant
    protected int @protected = 0; // Compliant
    public int @public = 0; // Compliant - Public
    private int usedInWith = 0; // Noncompliant

    void Foo()
    {
        ((privateUsed)) = 42;
        privateProtected = 42;
        @internal = 42;
        protectedInternal = 42;
        @protected = 42;
        @public = 42;
        Bar(privateUsed);
        var zero = new Record();
        usedInWith = 4;
        var answer = zero with { @public = usedInWith };
    }

    void Bar(int i) { }

    private int fieldInPropertyCompliant = 0;
    public int P1 { get { return fieldInPropertyCompliant; } }

    private int fieldInPropertyNoncompliant = 0; // Noncompliant
    public int P2 { get { fieldInPropertyNoncompliant = 42; return fieldInPropertyNoncompliant; } }

    private int fieldInPropertyWithInitCompliant = 0;
    public int P3
    {
        get { return fieldInPropertyWithInitCompliant; }
        init { fieldInPropertyWithInitCompliant = value; }
    }

    public record PositionalRecord(string Input)
    {
        private string inputField = Input; // Noncompliant
        public string Output { get { inputField = "2021"; return inputField + " year"; } }
    }
}

record struct RecordStruct
{
    public RecordStruct() { }

    private int privateUnused = 0; // Compliant - unused
    private int privateUsed = 0;   // Noncompliant {{Remove the field 'privateUsed' and declare it as a local variable in the relevant methods.}}
    //          ^^^^^^^^^^^^^^^
    internal int @internal = 0;    // Compliant
    public int @public = 0;        // Compliant - Public
    private int usedInWith = 0;    // Noncompliant

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

    private int fieldInPropertyNoncompliant = 0; // Noncompliant
    public int P2 { get { fieldInPropertyNoncompliant = 42; return fieldInPropertyNoncompliant; } }

    private int fieldInPropertyWithInitCompliant = 0;
    public int P3
    {
        get { return fieldInPropertyWithInitCompliant; }
        init { fieldInPropertyWithInitCompliant = value; }
    }

    public record struct PositionalRecordStruct(string Input)
    {
        private string inputField = Input; // Noncompliant
        public string Output { get { inputField = "2021"; return inputField + " year"; } }
    }
}

namespace CSharp15
{
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union
    public union UnionRepro(UnionCaseA, UnionCaseB)
    {
        private static int privateStaticUsed = 0; // FN NET-4956

        void Foo()
        {
            ((privateStaticUsed)) = 42;
            Bar(privateStaticUsed);
        }

        void Bar(int i) { }

        private static int genuineField; // Compliant, used as real state across methods

        void Write() => genuineField = 1;
        void Read() => Bar(genuineField);
    }

    public record class UnionCaseA(string Name);
    public record class UnionCaseB(string Name);
}

public class NullConditionalAssignment
{
    public class Sample
    {
        public int Value { get; set; }
    }
    private Sample compliantConditionalAssignment;
    private Sample compliant;

    void Test()
    {
        compliantConditionalAssignment?.Value = 42;
        if(compliant is null)
        {
            compliant = new Sample();
        }
        compliant.Value = 42;

        useSample(compliant);
        useSample(compliantConditionalAssignment);
    }
    void useSample(Sample sample) { }
}
