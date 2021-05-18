using System;

record RecordBase
{
    public RecordBase() { }
    public RecordBase(int i) { }
}

record Record : RecordBase
{
    public Record()
{ } // Fixed

    public Record(string s)
    {
    }

    public Record(int i)
        : base(i)
    {
    }
}

record StaticCtor
{
    static StaticCtor() { } // Fixed
}

record Foo
{
    public Foo() { } // Fixed

    ~Foo() { } // Fixed
}

record Bar
{
    public Bar()
    {
        Console.WriteLine("Hi!");
    }
}

record FooWithParams(string name)
{
    public FooWithParams() : this("a") { } // Compliant - has initializer with arguments

    ~FooWithParams() { } // Fixed
}
