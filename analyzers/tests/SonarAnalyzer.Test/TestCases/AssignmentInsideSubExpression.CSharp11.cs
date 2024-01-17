using System;

namespace Tests.Diagnostics
{
    public class AssignmentInsideSubExpression
    {
        void foo(int a)
        {
        }

        void Foo()
        {
            int i = 0;

            foo(i >>>= 1); // Noncompliant
        }
    }
}
