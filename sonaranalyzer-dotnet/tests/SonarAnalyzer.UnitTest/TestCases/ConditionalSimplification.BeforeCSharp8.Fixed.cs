
namespace Tests.TestCases
{
    class ConditionalSimplification
    {
        void NullCoalesceAssignment(object a, object b)
        {
            //??= can be used from C# 8 only
            a = a ?? b;                 // Compliant, this time
            a = a ?? b;    // Fixed
            a = a ?? b;    // Fixed

            a = a ?? b; // Fixed

            bool? value = null;
            value = value ?? false;  // Fixed

        }
    }
}
