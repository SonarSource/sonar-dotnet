using System;

record RecordBase
{
    public RecordBase() { }
    public RecordBase(int i) { }
}

record Record : RecordBase
{
    public Record()
        : base() { } // Noncompliant

    public Record(string s)
        : base() // Noncompliant
    {
    }

    public Record(int i)
        : base(i)
    {
    }
}

record StaticCtor
{
    static StaticCtor() { } // Noncompliant
}

record Foo
{
    public Foo() { } // Compliant - FN

    ~Foo() { } // Noncompliant {{Remove this redundant destructor.}}
}

record Bar
{
    public Bar()
    {
        Console.WriteLine("Hi!");
    }
}
