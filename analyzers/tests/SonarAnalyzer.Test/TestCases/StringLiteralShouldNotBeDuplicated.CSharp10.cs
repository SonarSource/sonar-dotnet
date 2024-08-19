using System.Diagnostics;

record struct RecordStruct
{
    public RecordStruct() { }

    private string name = "foobar"; // Noncompliant

    public static readonly string NameReadonly = "foobar";
    //                                           ^^^^^^^^ Secondary

    string Name { get; } = "foobar";
    //                     ^^^^^^^^ Secondary

    void Method()
    {
        var x = "foobar";
        //      ^^^^^^^^ Secondary
        void NestedMethod()
        {
            var y = "foobar";
            //      ^^^^^^^^ Secondary
        }
    }

    [DebuggerDisplay("foobar", Name = "foobar", TargetTypeName = "foobar")] // Compliant - in attribute -> ignored
    record struct InnerRecordStruct
    {
        public InnerRecordStruct() { }

        private string name = "foobar";
        //                    ^^^^^^^^ Secondary

        public static readonly string NameReadonly = "foobar"; // Secondary

        string Name { get; } = "foobar"; // Secondary

        void Method()
        {
            var x = "foobar"; // Secondary

            [Conditional("foobar")] // Compliant - in attribute -> ignored
            static void NestedMethod()
            {
                var y = "foobar"; // Secondary
            }
        }
    }

    record struct PositionalRecordStruct(string Name)
    {
        private string name = "foobar";
        //                    ^^^^^^^^ Secondary
    }
}
