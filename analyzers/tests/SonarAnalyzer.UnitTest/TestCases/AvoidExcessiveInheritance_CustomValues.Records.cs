using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    record Record_0 { }
    record Record_1 : Record_0 { }
    record Record_2 : Record_1 { }
    record Record_3 : Record_2 { } // Noncompliant {{This record has 3 parents which is greater than 2 authorized.}}
    record SecondSubRecord : Record_3 { }  // Noncompliant {{This record has 4 parents which is greater than 2 authorized.}}
    record ThirdSubRecord : SecondSubRecord { }
    record FourthSubRecord : ThirdSubRecord { }
    record FifthSubRecord : FourthSubRecord { }
}
