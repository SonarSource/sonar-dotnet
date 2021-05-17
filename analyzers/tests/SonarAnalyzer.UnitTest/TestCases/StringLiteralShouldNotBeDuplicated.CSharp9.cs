using System.Diagnostics;

string compliant = "compliant";
string noncompliant = "foobar";

var x = "foobar"; // Compliant - FN

record Record
{
    private string name = "foobar"; // Noncompliant

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
}
