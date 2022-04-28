using System.Diagnostics;

record struct RecordStruct
{
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
        private string name = "foobar"; // Noncompliant
                                        // Secondary@-1

        public static readonly string NameReadonly = "foobar"; // Secondary
                                                               // Secondary@-1

        string Name { get; } = "foobar"; // Secondary
                                         // Secondary@-1

        void Method()
        {
            var x = "foobar"; // Secondary
                              // Secondary@-1

            [Conditional("foobar")] // Compliant - in attribute -> ignored
            static void NestedMethod()
            {
                var y = "foobar"; // Secondary
                                  // Secondary@-1
            }
        }
    }

    record struct PositionalRecordStruct(string Name)
    {
        private string name = "foobar";
        //                    ^^^^^^^^ Secondary
    }
}

namespace Tests.Diagnostics
{
    record struct RecordStruct
    {
        public const string NameConst01 = "barfoo"; // Noncompliant {{Define a constant instead of using this literal 'barfoo' 5 times.}}
        //                                ^^^^^^^^ Secondary
        public const string NameConst02 = "barfoo";
        //                                ^^^^^^^^ Secondary
        public const string NameConst03 = "barfoo";
        //                                ^^^^^^^^ Secondary
        public const string NameConst04 = "barfoo";
        //                                ^^^^^^^^ Secondary
        public const string NameConst05 = "barfoo";
        //                                ^^^^^^^^ Secondary
    }
}
