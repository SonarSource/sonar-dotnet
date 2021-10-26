using System.Diagnostics;

string compliant = "compliant";
record struct RecordStruct
{
    private string name = "foobar"; // FN

    public static readonly string NameReadonly = "foobar";

    string Name { get; } = "foobar";

    void Method()
    {
        var x = "foobar";

        void NestedMethod()
        {
            var y = "foobar";
        }
    }

    [DebuggerDisplay("foobar", Name = "foobar", TargetTypeName = "foobar")] // Compliant - in attribute -> ignored
    record struct InnerRecordStruct
    {
        private string name = "foobar";

        public static readonly string NameReadonly = "foobar";

        string Name { get; } = "foobar";

        void Method()
        {
            var x = "foobar";

            [Conditional("DEBUG")] // Compliant - in attribute -> ignored
            static void NestedMethod()
            {
                var y = "foobar";
            }
        }
    }

    record struct PositionalRecordStruct(string Name)
    {
        private string name = "foobar";
    }
}
