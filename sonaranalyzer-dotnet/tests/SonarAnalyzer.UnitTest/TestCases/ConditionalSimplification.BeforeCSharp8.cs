
namespace Tests.TestCases
{
    class ConditionalSimplification
    {
        void NullCoalesceAssignment(object a, object b)
        {
            //??= can be used from C# 8 only
            a = a ?? b;                 // Compliant, this time
            a = a != null ? (a) : b;    // Noncompliant {{Use the '??' operator here.}}
            a = null == a ? b : (a);    // Noncompliant {{Use the '??' operator here.}}

            if (a == null) // Noncompliant {{Use the '??' operator here.}}
            {
                a = b;
            }

            bool? value = null;
            if (value == null)  // Noncompliant {{Use the '??' operator here.}}
                value = false;

        }
    }
}
