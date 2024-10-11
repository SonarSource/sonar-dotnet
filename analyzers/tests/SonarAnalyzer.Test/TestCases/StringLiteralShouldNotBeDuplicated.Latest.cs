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

namespace CSharp9
{
    record Record
    {
        private string name = "csharp9"; // Noncompliant {{Define a constant instead of using this literal 'csharp9' 11 times.}}
        //                    ^^^^^^^^^

        public static readonly string NameReadonly = "csharp9";
        //                                           ^^^^^^^^^ Secondary

        string Name { get; } = "csharp9";
        //                     ^^^^^^^^^ Secondary

        void Method()
        {
            var x = "csharp9";
            //      ^^^^^^^^^ Secondary

            void NestedMethod()
            {
                var y = "csharp9";
                //      ^^^^^^^^^ Secondary
            }
        }

        [DebuggerDisplay("csharp9", Name = "csharp9", TargetTypeName = "csharp9")] // Compliant - in attribute -> ignored
        record InnerRecord
        {
            private string name = "csharp9";
            //                    ^^^^^^^^^ Secondary

            public static readonly string NameReadonly = "csharp9";
            //                                           ^^^^^^^^^ Secondary

            string Name { get; } = "csharp9";
            //                     ^^^^^^^^^ Secondary

            void Method()
            {
                var x = "csharp9";
                //      ^^^^^^^^^ Secondary

                [Conditional("DEBUG")] // Compliant - in attribute -> ignored
                static void NestedMethod()
                {
                    var y = "csharp9";
                    //      ^^^^^^^^^ Secondary
                }
            }
        }

        record PositionalRecord(string Name)
        {
            private string name = "csharp9";
            //                    ^^^^^^^^^ Secondary
        }
    }
}

namespace CSharp10
{
    record struct RecordStruct
    {
        public RecordStruct() { }

        private string name = "csharp10"; // Noncompliant

        public static readonly string NameReadonly = "csharp10";
        //                                           ^^^^^^^^^^ Secondary

        string Name { get; } = "csharp10";
        //                     ^^^^^^^^^^ Secondary

        void Method()
        {
            var x = "csharp10";
            //      ^^^^^^^^^^ Secondary
            void NestedMethod()
            {
                var y = "csharp10";
                //      ^^^^^^^^^^ Secondary
            }
        }

        [DebuggerDisplay("csharp10", Name = "csharp10", TargetTypeName = "csharp10")] // Compliant - in attribute -> ignored
        record struct InnerRecordStruct
        {
            public InnerRecordStruct() { }

            private string name = "csharp10";
            //                    ^^^^^^^^^^ Secondary

            public static readonly string NameReadonly = "csharp10"; // Secondary

            string Name { get; } = "csharp10"; // Secondary

            void Method()
            {
                var x = "csharp10"; // Secondary

                [Conditional("foobar")] // Compliant - in attribute -> ignored
                static void NestedMethod()
                {
                    var y = "csharp10"; // Secondary
                }
            }
        }

        record struct PositionalRecordStruct(string Name)
        {
            private string name = "csharp10";
            //                    ^^^^^^^^^^ Secondary
        }
    }
}

namespace CSharp11
{
    public class FooNonCompliant
    {
        private string NameOne = """csharp11"""; // Noncompliant {{Define a constant instead of using this literal '""csharp11""' 4 times.}}

        private string NameTwo = """csharp11"""; // Secondary

        public const string NameConst = """csharp11"""; // Secondary

        public static readonly string NameReadonly = """csharp11"""; // Secondary

    }

    public class FooLessThanFiveCharacters
    {
        private string NameOne = """fo"""; // Compliant (less than 5 characters)

        private string NameTwo = """fo""";

        public const string NameConst = """fo""";

        public static readonly string NameReadonly = """fo""";
    }

    public class FooNonCompliantStringInterpolation
    {
        public string NameOne = $"{"BarBar" // Noncompliant {{Define a constant instead of using this literal 'BarBar' 4 times.}}
            }";

        public string NameTwo = $"{"BarBar" // Secondary
            }";

        public static string NameThree = "BarBar"; // Secondary

        public static readonly string NameReadonly = $"{"BarBar"}"; // Secondary

    }
}

namespace CSharp13
{
    class EscapeSequence
    {
        private string backslash = "Filename\u001B" // Noncompliant
                + "Filename\e"                      // Secondary
                + "Filename\u001b"                  // Secondary
                + "Filename\e";                     // Secondary
    }

    partial class PartialClass
    {
        private string some = "csharp13"; // Noncompliant [0]
        public partial string Hello => "csharp13"; // Secondary [0] 
        public partial string World { get; }
    }

    partial class PartialClass
    {
        private const string name = "csharp13"; // Secondary [0] 
        public partial string Hello { get; }
        public partial string World => "csharp13"; // Secondary [0] 
    }
}
