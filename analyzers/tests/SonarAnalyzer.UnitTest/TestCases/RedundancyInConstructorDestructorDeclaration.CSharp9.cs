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
    public Foo() { } // Noncompliant

    ~Foo() { } // Noncompliant {{Remove this redundant destructor.}}
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

    ~FooWithParams() { } // Noncompliant {{Remove this redundant destructor.}}
}
