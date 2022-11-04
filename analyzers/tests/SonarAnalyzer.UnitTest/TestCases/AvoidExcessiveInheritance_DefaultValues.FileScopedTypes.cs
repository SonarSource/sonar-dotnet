using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file class Class_0 { }
    file class Class_1 : Class_0 { }
    file class Class_2 : Class_1 { }
    file class Class_3 : Class_2 { }
    file class Class_4 : Class_3 { }
    file class Class_5 : Class_4 { }
    file class Class_6 : Class_5 { } // Noncompliant  {{This class has 6 parents which is greater than 5 authorized.}}
    file class Class_7 : Class_6 { } // Noncompliant  {{This class has 7 parents which is greater than 5 authorized.}}

    file record Record_0 { }
    file record Record_1 : Record_0 { }
    file record Record_2 : Record_1 { }
    file record Record_3 : Record_2 { }
    file record Record_4 : Record_3 { }
    file record Record_5 : Record_4 { }
    file record Record_6 : Record_5 { } // Noncompliant  {{This record has 6 parents which is greater than 5 authorized.}}
    file record Record_7 : Record_6 { } // Noncompliant  {{This record has 7 parents which is greater than 5 authorized.}}
}
