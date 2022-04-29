using System.Diagnostics;

string compliant = "compliant";
// equivalent to args argument of the top level file
string compliant1 = "args";
string compliant2 = "args";
string compliant3 = "args";

string noncompliant = "foobar"; // Noncompliant

var x = "foobar";
//      ^^^^^^^^ Secondary

record Record
{
    private string name = "foobar";
//                        ^^^^^^^^ Secondary

    public static readonly string NameReadonly = "foobar";
//                                               ^^^^^^^^ Secondary

    string Name { get; } = "foobar";
//                         ^^^^^^^^ Secondary

    void Method()
    {
        var x = "foobar";
//              ^^^^^^^^ Secondary

        void NestedMethod()
        {
            var y = "foobar";
//                  ^^^^^^^^ Secondary
        }
    }

    [DebuggerDisplay("foobar", Name = "foobar", TargetTypeName = "foobar")] // Compliant - in attribute -> ignored
    record InnerRecord
    {
        private string name = "foobar";
//                            ^^^^^^^^ Secondary

        public static readonly string NameReadonly = "foobar";
//                                                   ^^^^^^^^ Secondary

        string Name { get; } = "foobar";
//                             ^^^^^^^^ Secondary

        void Method()
        {
            var x = "foobar";
//                  ^^^^^^^^ Secondary

            [Conditional("DEBUG")] // Compliant - in attribute -> ignored
            static void NestedMethod()
            {
                var y = "foobar";
//                      ^^^^^^^^ Secondary
            }
        }
    }

    record PositionalRecord(string Name)
    {
        private string name = "foobar";
//                            ^^^^^^^^ Secondary
    }
}
