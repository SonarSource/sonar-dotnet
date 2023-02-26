using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class OuterClass
{
    static void UsedInNameOfExpressionInNestedClass() { }           // Noncompliant 

    class NestedClass
    {
        void Bar()
        {
            string methodName = nameof(UsedInNameOfExpressionInNestedClass);
        }
    }
}

