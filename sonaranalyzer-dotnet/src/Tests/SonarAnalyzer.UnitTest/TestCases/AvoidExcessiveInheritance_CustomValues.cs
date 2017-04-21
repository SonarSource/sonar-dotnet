using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class FirstClass : Exception { }
    class SecondSubClass : FirstClass { } // Noncompliant {{This class has 3 parents which is greater than 2 authorized.}}
    class ThirdSubClass : SecondSubClass { }
    class FourthSubClass : ThirdSubClass { }
}