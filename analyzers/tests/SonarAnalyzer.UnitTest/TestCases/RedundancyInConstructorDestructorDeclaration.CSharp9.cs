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

// https://github.com/SonarSource/sonar-dotnet/issues/8436
public class Repro_FP_8436
{
    public abstract record Base(string Value);

    public record RecordStyle() : Base("SomeValue"); // Noncompliant FP, Foo is calling Base constructor with record idiomatic syntax, it's not redundant

    public record DefaultStyle : Base
    {
        public DefaultStyle() : base("SomeValue") // Compliant, "default" way of calling Base constructor
        { }
    }
}
