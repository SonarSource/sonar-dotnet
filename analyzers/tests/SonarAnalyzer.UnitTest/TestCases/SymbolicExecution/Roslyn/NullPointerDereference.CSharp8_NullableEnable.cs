#nullable enable

using System;

namespace Tests.Diagnostics.CSharp8
{
    public class NullForgivingOperator
    {
        object? field;
        static object? staticField;
        event EventHandler<object>? handler;

        public void Local()
        {
            object? o = null;
            o.ToString();                        // Noncompliant
            o = null;
            o!.ToString();                       // Compliant
        }

        public void Parameter(object? p)
        {
            if (p == null)
            {
                p!.ToString();                   // Compliant
            }
        }

        public void Field()
        {
            if (field == null)
            {
                field!.ToString();               // Compliant
            }
        }

        public void StaticField()
        {
            if (staticField == null)
            {
                staticField!.ToString();         // Compliant
            }
        }

        public void EventHandler()
        {
            if (handler == null)
            {
                handler!.ToString();             // Compliant
            }
        }

        public void Array()
        {
            object[]? arr = null;
            arr!.ToString();                     // Compliant
        }

        public void ArrayElement()
        {
            var arr = new object?[] { null };
            arr[0]!.ToString();                  // Compliant
        }

        public void Conversion()
        {
            object? o = null;
            ((int?)o)!.ToString();               // Compliant
        }

        public void Expression(bool condition)
        {
            (condition ? new object() : null)!.ToString();  // Compliant
        }

        public void Parenthesis(bool condition)
        {
            (((condition ? new object() : null))!).ToString();  // Compliant
        }
    }
}
