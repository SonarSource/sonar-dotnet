using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string ConstText = "this cannot change";

        [Localizable(true)]
        public string Property1 { get; set; }
        [Localizable(false)]
        public string Property2 { get; set; }
        public string Message { get; set; }
        public string Text { get; set; }
        public string Caption { get; set; }
        public string OtherText { get; set; }
        public string TextualRepresenatation { get; set; }

        public void Foo()
        {
            Bar("param1", // Noncompliant {{Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.}}
//              ^^^^^^^^
                "param2", // Noncompliant
//              ^^^^^^^^
                "param3", // Noncompliant
//              ^^^^^^^^
                "param4"); // Noncompliant
//              ^^^^^^^^

            const string localConst = "unchangeable";

            Baz("param1",
                "param2", // Noncompliant
                localConst, // Noncompliant
                ConstText, // Noncompliant
                ConstText); // Compliant, parameter does not match exactly

            Console.WriteLine("some text"); // Noncompliant
            Console.Write("some more text"); // Noncompliant
            Console.WriteLine("the format", "a"); // Noncompliant
            Console.Write("the format", "a"); // Noncompliant

            Property1 = "some text"; // Noncompliant
            Property2 = "moar text";
            Message = "message text"; // Noncompliant
            Text = "textual text"; // Noncompliant
            Caption = "caption text"; // Noncompliant
            OtherText = "other text"; // Noncompliant
            TextualRepresenatation = "different things here"; // Compliant, property does not match exactly
        }

        public void Bar([Localizable(true)]string param1, string message, string text, string caption)
        {
        }

        public void Baz([Localizable(false)]string param1, string myMessage, string otherText, string captionString, string captionlessTitle)
        {
        }
    }

    class InvalidProgram
    {
        public void Foo()
        {
            Bar("some string", "other string", "third string"); // Compliant, this cannot be compiled
            Bar("some string"); // Compliant, this cannot be compiled
            Bar(); // Compliant
            Console.Write(); // Compliant
            Console.WriteLine(); // Compliant
        }

        public void Bar(string text, string message)
        {
        }
    }

    class DebugCode
    {
        public void LogStuff()
        {
            // Regression tests for https://github.com/SonarSource/sonar-csharp/issues/1464
            // S4055 should not raise issues for string literal used in the 'message' of Debug.XXX
            Debug.Assert(true, "Assertion message");                    // compliant - method on Debug
            Debug.WriteLine("Stuff happened");                          // compliant - method on Debug
            Debug.WriteLineIf(true, "Stuff happened conditionally");    // compliant - method on Debug
        }
    }
}
