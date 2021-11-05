using System.IO;

var topLevel = 0;
var person = new { FirstName = "Scott", LastName = "Hunter", Age = 25 };
var otherPerson = person with { LastName = "Hanselman", Age = topLevel = 42 }; // Noncompliant

void WithRecordStructClone()
{
    var zero = new RecordStruct() { Value = 0 };
    var answer = zero with { Value = 42 };
    var badAnswer = zero with { Value = topLevel = 42 };  // Noncompliant

    var one = new PositionalRecordStruct(1) { Value = 1 };
    var clone = one with { Value = 2 };
    var badClone = one with { Value = topLevel = 2 };     // Noncompliant
}

record struct RecordStruct
{
    public int Value { get; init; }
}

record struct PositionalRecordStruct(int Input)
{
    public int Value { get; init; } = Input;

    private void Method(int arg = 42)
    {
        var i = 0;
        Method(i = 42); // Noncompliant
    }

    private void Method2()
    {
        int y;
        (y, var x) = (16, 23);
    }
}
