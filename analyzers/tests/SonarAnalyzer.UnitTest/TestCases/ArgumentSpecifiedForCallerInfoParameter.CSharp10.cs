using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    class ArgumentSpecifiedForCallerInfoParameter
    {
        void TraceMessage(bool condition, [CallerArgumentExpression("condition")] string expression = null)
        {
        }

        void MyMethod()
        {
            TraceMessage(true, "condition");        // Noncompliant
            TraceMessage(true);                     // Compliant
            TraceMessage(true, expression: "aaaa"); // Noncompliant
        }
    }
}
