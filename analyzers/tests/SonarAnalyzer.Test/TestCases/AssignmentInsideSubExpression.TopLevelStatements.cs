using System.IO;

var topLevel = 0;
Method(topLevel = 42); // Noncompliant
var person = new { FirstName = "Scott", LastName = "Hunter", Age = 25 };
var otherPerson = person with { LastName = "Hanselman", Age = topLevel = 42 }; // Noncompliant

void Method(int arg)
{
    var i = 0;
    Method(i = 42); // Noncompliant
}

void WithRecordClone()
{
    var zero = new Record() { Value = 0 };
    var answer = zero with { Value = 42 };
    var badAnswer = zero with { Value = topLevel = 42 };  // Noncompliant

    var one = new PositionalRecord(1) { Value = 1 };
    var clone = one with { Value = 2 };
    var badClone = one with { Value = topLevel = 2 };     // Noncompliant
}

void TargetTypedNew()
{
    Record r;
    Method1(r = new());                // Noncompliant
    PositionalRecord p;
    Method2(p = new(42));              // Noncompliant

    void Method1(Record arg) { }
    void Method2(PositionalRecord arg) { }
}

void WithPositionalRecord()
{
    var x = 42;
    new PositionalRecord(x);
    new PositionalRecord(x = 42); // Noncompliant
}

async void IsNotNull(StreamReader reader)
{
    string line;
    // See: https://github.com/SonarSource/sonar-dotnet/issues/4264
    while ((line = await reader.ReadLineAsync()) is not null)
    {
    }

    while ((line = await reader.ReadLineAsync()) is null)
    {
    }
}

void WithRecordStructClone()
{
    var zero = new RecordStruct() { Value = 0 };
    var answer = zero with { Value = 42 };
    var badAnswer = zero with { Value = topLevel = 42 };  // Noncompliant

    var one = new PositionalRecordStruct(1) { Value = 1 };
    var clone = one with { Value = 2 };
    var badClone = one with { Value = topLevel = 2 };     // Noncompliant
}

record Record
{
    public int Value { get; init; }
}

record PositionalRecord(int Input)
{
    public int Value { get; init; } = Input;

    private void Method(int arg = 42)
    {
        var i = 0;
        Method(i = 42); // Noncompliant
    }

    private void Method2()
    {
        int y, z;
        var x = (y, z) = (16, 23);
    }
}

// See https://github.com/SonarSource/sonar-dotnet/issues/4446
interface ICustomMsgQueue
{
    string? Pop();
}

class MessageQueueUseCase
{
    void Process(ICustomMsgQueue queue)
    {
        string msg;
        while ((msg = queue.Pop()) is not null)
        {
        }

        do
        {
        } while ((msg = queue.Pop()) is null);
    }
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
