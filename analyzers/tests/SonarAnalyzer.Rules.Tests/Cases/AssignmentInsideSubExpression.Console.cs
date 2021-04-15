// version: CSharp9

var topLevel = 0;
Method(topLevel = 42); // Noncompliant

void Method(int arg)
{
    var i = 0;
    Method(i = 42); // Noncompliant
}

void WithRecordClone()
{
    var zero = new Record() { Value = 0 };
    var answer = zero with { Value = 42 };  // Noncompliant FP
}

void TargetTypedNew()
{
    Record r;
    Method(r = new());              // Noncompliant

    void Method(Record arg) { }
}

record Record
{
    public int Value { get; init; }
}
