using System.Diagnostics;

string compliant = "compliant";
string noncompliant = "foobar";

var x = "foobar"; // Compliant - FN

record Record
{
    private string name = "foobar"; // Compliant - FN

    public static readonly string NameReadonly = "foobar"; // Compliant - FN

    string Name { get; } = "foobar"; // Compliant - FN

    void Method()
    {
        var x = "foobar"; // Compliant - FN

        void NestedMethod()
        {
            var y = "foobar"; // Compliant - FN
        }
    }

    [DebuggerDisplay("foobar", Name = "foobar", TargetTypeName = "foobar")] // Compliant - in attribute -> ignored
    record InnerRecord
    {
        private string name = "foobar"; // Compliant - FN

        public static readonly string NameReadonly = "foobar"; // Compliant - FN

        string Name { get; } = "foobar"; // Compliant - FN

        void Method()
        {
            var x = "foobar"; // Compliant - FN

            [Conditional("DEBUG")] // Compliant - in attribute -> ignored
            static void NestedMethod()
            {
                var y = "foobar"; // Compliant - FN
            }
        }
    }
}
