using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class BaseClass { }

    class FirstDerivedClass : BaseClass { }

    class SecondDerivedClass : FirstDerivedClass { }

    class ThirdDerivedClass : SecondDerivedClass { }

    class FourthDerivedClass : ThirdDerivedClass { }

    class FifthDerivedClass : FourthDerivedClass { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}

    class SixthDerivedClass : FifthDerivedClass { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//        ^^^^^^^^^^^^^^^^^


    class FirstSubClass : Exception { }
    class SecondSubClass : FirstSubClass { }
    class ThirdSubClass : SecondSubClass { }
    class FourthSubClass : ThirdSubClass { }
    class FifthSubClass : FourthSubClass { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
    class SixthSubClass : FifthSubClass { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//        ^^^^^^^^^^^^^
}