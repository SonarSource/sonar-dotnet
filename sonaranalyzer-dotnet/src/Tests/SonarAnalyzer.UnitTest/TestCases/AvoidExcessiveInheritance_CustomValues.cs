using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class FirstSubClass : Exception { }
    class SecondSubClass : FirstSubClass { } // Noncompliant {{This class has 3 parents which is greater than 2 authorized.}}
}