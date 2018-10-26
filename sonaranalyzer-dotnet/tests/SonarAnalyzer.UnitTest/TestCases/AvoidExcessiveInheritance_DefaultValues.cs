using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class BaseClass { }

    public class DerivedClass_1 : BaseClass { }

    public class DerivedClass_2 : DerivedClass_1 { }

    public class DerivedClass_3 : DerivedClass_2 { }

    public class DerivedClass_4 : DerivedClass_3 { }

    public class DerivedClass_5 : DerivedClass_4 { }

    public class DerivedClass_6 : DerivedClass_5 { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^^^^^
    public class DerivedClass_7 : DerivedClass_6 { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^^^^^

    public class SubClass_0 : Exception { }
    public class SubClass_1 : SubClass_0 { }
    public class SubClass_2 : SubClass_1 { }
    public class SubClass_3 : SubClass_2 { }
    public class SubClass_4 : SubClass_3 { }
    public class SubClass_5 : SubClass_4 { }
    public class SubClass_6 : SubClass_5 { } // Noncompliant {{This class has 6 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^
    public class SubClass_7 : SubClass_6 { } // Noncompliant {{This class has 7 parents which is greater than 5 authorized.}}
//               ^^^^^^^^^^
}

namespace Tests
{
    public class Tests_5 : Tests.Diagnostics.DerivedClass_4 { }

    public class Tests_6 : Tests.Diagnostics.DerivedClass_5 { } // Noncompliant

    public class Tests_7 : Tests.Diagnostics.SubClass_5 { } // Noncompliant
}

namespace Tests.Foo.Bar.Baz
{
    public class Tests_5 : Tests.Diagnostics.DerivedClass_4 { }

    public class Tests_6 : Tests.Diagnostics.DerivedClass_5 { } // Noncompliant

    public class Tests_7 : Tests.Diagnostics.SubClass_5 { } // Noncompliant
}

namespace OtherNamespace
{
    public class Other_0 : Tests.Diagnostics.SubClass_6 { }
    public class Other_1 : Other_0 { }
    public class Other_2 : Other_1 { }
    public class Other_3 : Other_2 { }
    public class Other_4 : Other_3 { }
    public class Other_5 : Other_4 { }
    public class Other_6 : Other_5 { } // Noncompliant  {{This class has 6 parents which is greater than 5 authorized.}}
    public class Other_7 : Other_6 { } // Noncompliant  {{This class has 7 parents which is greater than 5 authorized.}}
}

public class Other_0 : Exception { }
public class Other_1 : Other_0 { }
public class Other_2 : Other_1 { }
public class Other_3 : Other_2 { }
public class Other_4 : Other_3 { }
public class Other_5 : Other_4 { }
public class Other_6 : Other_5 { } // Noncompliant  {{This class has 6 parents which is greater than 5 authorized.}}
public class Other_7 : Other_6 { } // Noncompliant  {{This class has 7 parents which is greater than 5 authorized.}}
