using System;

Console.WriteLine("Hello World!");

abstract class Base
{
    public Base() { } // Noncompliant {{Change the visibility of this constructor to 'private', 'private protected' or 'protected'.}}

    private Base(int i) { } // Compliant
}

abstract record AbstractRecordOne
{
    public string X { get; }

    public AbstractRecordOne(string x) => (X) = (x); // Noncompliant
}

record RecordOne : AbstractRecordOne
{
    public RecordOne(string x) : base(x) { } // Compliant
}
