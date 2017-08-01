using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class BaseClass { }

    class FirstDerivedClass : BaseClass { }

    class SecondDerivedClass : FirstDerivedClass { }

    class ThirdDerivedClass : SecondDerivedClass { }

    class FourthDerivedClass : ThirdDerivedClass { }

    class FifthDerivedClass : FourthDerivedClass { }

    class SixthDerivedClass : FifthDerivedClass { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
//        ^^^^^^^^^^^^^^^^^
    class SeventhDerivedClass : SixthDerivedClass { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//        ^^^^^^^^^^^^^^^^^^^

    public class FirstSubClass : Exception { }
    public class SecondSubClass : FirstSubClass { }
    public class ThirdSubClass : SecondSubClass { }
    public class FourthSubClass : ThirdSubClass { }
    public class FifthSubClass : FourthSubClass { }
    public class SixthSubClass : FifthSubClass { }
    public class SeventhSubClass : SixthSubClass { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^^^^^^
    public class EighthSubClass : SeventhSubClass { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^^^^^
}

namespace Tests
{
    public class Tests_5 : Tests.Diagnostics.FourthDerivedClass { }

    public class Tests_6 : Tests.Diagnostics.FifthDerivedClass { } // Noncompliant

    public class Tests_7 : Tests.Diagnostics.SixthSubClass { } // Noncompliant
}

namespace Tests.Foo.Bar.Baz
{
    public class Tests_5 : Tests.Diagnostics.FourthDerivedClass { }

    public class Tests_6 : Tests.Diagnostics.FifthDerivedClass { } // Noncompliant

    public class Tests_7 : Tests.Diagnostics.SixthSubClass { } // Noncompliant
}

namespace OtherNamespace
{
    public class Other_1 : Tests.Diagnostics.SeventhSubClass { }
    public class Other_2 : Other_1 { }
    public class Other_3 : Other_2 { }
    public class Other_4 : Other_3 { }
    public class Other_5 : Other_4 { }
    public class Other_6 : Other_5 { }
    public class Other_7 : Other_6 { } // Noncompliant  {{This class has 6 parents which is greater than 5 authorized.}}
    public class Other_8 : Other_7 { } // Noncompliant  {{This class has 7 parents which is greater than 5 authorized.}}
}