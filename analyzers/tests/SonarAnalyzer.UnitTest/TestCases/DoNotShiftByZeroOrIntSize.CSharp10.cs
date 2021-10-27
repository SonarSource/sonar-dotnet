using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void RightShift()
        {
            int i = 0;
            (int a, i) = (0, i >> 0); // Noncompliant {{Remove this useless shift by 0.}}
        }
    }
}
