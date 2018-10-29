using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        ~Program() // Noncompliant {{Remove this empty finalizer.}}
        {

        }
    }

    class Program1
    {
        ~Program1() // Noncompliant {{Remove this empty finalizer.}}
        {
            // Some comment
        }
    }

    class Program2
    {
        ~Program2() // Compliant
        {
            Console.WriteLine("foo");
        }
    }

    class Program3
    {
        ~Program3() => Console.WriteLine();
    }
}
