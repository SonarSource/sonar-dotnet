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
        [Localizable(false)]
        public string SomeMessage { get; set; }
        public string Message { get; set; }
        public string Text { get; set; }
        public string Caption { get; set; }
        public string OtherText { get; set; }
        public string TextualRepresenatation { get; set; }

        private string message;
        [Localizable(true)]
        public int IntProperty { get; set; }

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
                "some parameter", // Compliant as Localizable is set to false
                "param2", // Noncompliant
                localConst, // Noncompliant
                ConstText, // Noncompliant
                ConstText); // Compliant, parameter does not match exactly

            Console.WriteLine("some text"); // Noncompliant
            Console.Write("some more text"); // Noncompliant
            Console.WriteLine("the format", "a"); // Noncompliant
            Console.Write("the format", "a"); // Noncompliant

            Property1 = "some text"; // Noncompliant {{Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.}}
            //          ^^^^^^^^^^^
            Property2 = "moar text";
            SomeMessage = "some text"; // Compliant as Localizable is set to false
            Message = "message text"; // Noncompliant
            Text = "textual text"; // Noncompliant
            Caption = "caption text"; // Noncompliant
            OtherText = "other text"; // Noncompliant
            TextualRepresenatation = "different things here"; // Compliant, property does not match exactly

            message = "message text"; // Compliant as it is a field
            IntProperty = 42; // Compliant as it is not a string property.

            InvalidAttribute1("some parameter");
            InvalidAttribute2("some parameter");
        }

        public void Bar([Localizable(true)]string param1, string message, string text, string caption)
        {
        }

        public void Baz([Localizable(false)]string param1, [Localizable(false)] string someMessage, string myMessage, string otherText, string captionString, string captionlessTitle)
        {
        }

        public void InvalidAttribute1([Localizable] string param1) // Error [CS7036]
        {
        }

        public void InvalidAttribute2([Localizable(42)] string param1) // Error [CS1503]
        {
        }
    }

    class InvalidProgram
    {
        public void Foo()
        {
            Bar("some string", "other string", "third string"); // Compliant, this cannot be compiled // Error [CS1501]
            Bar("some string"); // Error [CS7036] Compliant, this cannot be compiled
            Bar();              // Error [CS7036] Compliant
            Console.Write(true); // Compliant
            Console.WriteLine(); // Compliant
        }

        public void Bar(string text, string message)
        {
        }
    }

    class DebugCode
    {
        public string Message { get; set; }

        public void LogStuff()
        {
            // Regression tests for https://github.com/SonarSource/sonar-dotnet/issues/1464
            // S4055 should not raise issues for string literal used in the 'message' of Debug.XXX
            Debug.Assert(true, "Assertion message");                    // compliant - method on Debug
            Debug.WriteLine("Stuff happened");                          // compliant - method on Debug
            Debug.WriteLineIf(true, "Stuff happened conditionally");    // compliant - method on Debug
        }

        [Conditional("DEBUG")]
        public void DebugOnlyMethod(string text)
        {
            Console.WriteLine("hello world");   // compliant - in a debug only method
            Message = "hello world";            // compliant - in a debug only method
        }

        [Conditional("NONDEBUG")]
        public void NonDebugOnlyMethod(string text)
        {
            Console.WriteLine("hello world");    // Noncompliant - not DEBUG conditional
            Message = "hello world";             // Noncompliant - not DEBUG conditional
        }

        public void Caller()
        {
            DebugOnlyMethod("a message");    // compliant - calling a debug-only method
            NonDebugOnlyMethod("a message"); // Noncompliant
        }
    }
}
