using System;

Record r1 = new Record(); // Compliant - FN
Record r2 = null;
r2 = new Record(); // Compliant - FN

LocalStatic(new Record()); // Compliant - FN
Action<int, int> a = static (parameter1, parameter2) => { };
//                           ^^^^^^^^^^ {{'parameter1' is not used. Use discard parameter instead.}}
//                                       ^^^^^^^^^^@-1 {{'parameter2' is not used. Use discard parameter instead.}}

Action<int, int, int> b = static (p1, p2, p3) => { Console.WriteLine(p2); };
//                                ^^ {{'p1' is not used. Use discard parameter instead.}}
//                                        ^^@-1 {{'p3' is not used. Use discard parameter instead.}}

Action<int> c = static (parameter1) => { Console.WriteLine(parameter1); }; // FN - Can be replaced with method group `Action<int> b = Console.WriteLine;`

Action<int> d = static parameter1 => { Console.WriteLine(parameter1); }; // FN - Can be replaced with method group `Action<int> c = Console.WriteLine;`

Record Local() => new Record(); // Compliant - FN
static Record LocalStatic(Record r) => r;

int? xx = ((new int?(5))); // Noncompliant {{Remove the explicit nullable type creation; it is redundant.}}

EventHandler myEvent = new ((a, b) => { });
//                     ^^^^^^^^^^^^^^^^^^^ {{Remove the explicit delegate creation; it is redundant.}}
//                           ^@-1 {{'a' is not used. Use discard parameter instead.}}
//                              ^@-2 {{'b' is not used. Use discard parameter instead.}}

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
        MyEvent += new((a, b) => { });
//                 ^^^^^^^^^^^^^^^^^^ {{Remove the explicit delegate creation; it is redundant.}}
//                      ^@-1 {{'a' is not used. Use discard parameter instead.}}
//                         ^@-2 {{'b' is not used. Use discard parameter instead.}}

        object o = new() { }; // Noncompliant {{Remove the initializer; it is redundant.}}
//                       ^^^

        _ = new Point[] // Noncompliant - FP
            {
                new(1, 2)
            };
    }

    private event EventHandler MyEvent;

    public record Point(int x, int y);
}
