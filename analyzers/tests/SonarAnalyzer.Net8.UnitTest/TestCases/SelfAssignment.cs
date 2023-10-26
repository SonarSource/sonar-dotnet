namespace SonarAnalyzer.Net8.UnitTest.TestCases;

class WithInlineArrays
{
    void Test(Buffer b)
    {
        b = b; // Noncompliant: local to local assignment
               // Secondary@-1
    }

    [System.Runtime.CompilerServices.InlineArray(10)]
    struct Buffer
    {
        int arrayItem;
    }
}
