#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
            o.ToString();                        // Compliant
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
            o.ToString();                        // Compliant We learn from the invocation that o is not null. Any conversions are transparent to this.
        }

        public void Expression(bool condition)
        {
            (condition ? new object() : null)!.ToString();  // Compliant
        }

        public void Parenthesis(bool condition)
        {
            object? o = null;
            (((condition ? o : null))!).ToString();  // Compliant
            o.ToString();                            // Noncompliant The ! only suppresses the warning on the expression, but is not applied to any inner variables.
        }
    }

    public class NullableAnnotations
    {
        public void DebugAssert()
        {
            object? o = null;
            Debug.Assert(o != null);
            o.ToString();  // Compliant. Debug.Assert is annotated in .NetStandard with Debug.Assert([DoesNotReturnIf(false)]bool condition)
        }

        public void NotNullWhenFalse()
        {
            string? s = null;
            if (!CustomNotNullAssertion(s))
            {
                s.ToString();  // Compliant
            }
        }

        private static bool CustomNotNullAssertion([NotNullWhen(false)] string? s)
            => string.IsNullOrEmpty(s);
    }
}
