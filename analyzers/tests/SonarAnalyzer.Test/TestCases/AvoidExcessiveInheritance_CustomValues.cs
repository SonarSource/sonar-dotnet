using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Class_0 : Exception { }
    class Class_1 : Class_0 { }
    class Class_2 : Class_1 { }
    class Class_3 : Class_2 { } // Noncompliant {{This class has 3 parents which is greater than 2 authorized.}}
    class SecondSubClass : Class_3 { }  // Noncompliant {{This class has 4 parents which is greater than 2 authorized.}}
    class ThirdSubClass : SecondSubClass { }
    class FourthSubClass : ThirdSubClass { }
    class FifthSubClass : FourthSubClass { }
}