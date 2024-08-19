using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            int a;
            int b = 10;
            (a, int c) = (1, 2 + b++); // Noncompliant
        }
    }
}
