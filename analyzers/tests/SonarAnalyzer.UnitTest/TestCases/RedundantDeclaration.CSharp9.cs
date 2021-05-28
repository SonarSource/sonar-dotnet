using System;

Record r1 = new Record(); // Compliant - FN
Record r2 = null;
r2 = new Record(); // Compliant - FN

LocalStatic(new Record()); // Compliant - FN
Action<int, int> a = static (parameter1, parameter2) => { }; // Compliant - FN, variables are not used and they should be replaced by discard (_, _)

Record Local() => new Record(); // Compliant - FN
static Record LocalStatic(Record r) => r;

record Record
{
    object field;

    object Property
    {
        get { return new object(); } // Compliant - FN
        init { field = new object(); } // Compliant - FN
    }
}

public class RedundantDeclaration
{
    public RedundantDeclaration()
    {
        MyEvent += new((a, b) => { }); // Noncompliant {{Remove the explicit delegate creation; it is redundant.}}
//                 ^^^^^^^^^^^^^^^^^^

        object o = new() { }; // Noncompliant {{Remove the initializer; it is redundant.}}
//                       ^^^
    }

    private event EventHandler MyEvent;
}
