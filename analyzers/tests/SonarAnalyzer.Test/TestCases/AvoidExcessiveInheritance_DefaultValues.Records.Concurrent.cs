using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace AppendedNamespaceForConcurrencyTest.Tests.Diagnostics
{
    public record BaseRecord { }
}

namespace OtherRecordNamespace
{
    public record Record_0 : AppendedNamespaceForConcurrencyTest.Tests.Diagnostics.BaseRecord { }
    public record Record_1 : Record_0 { }
    public record Record_2 : Record_1 { }
    public record Record_3 : Record_2 { }
    public record Record_4 : Record_3 { }
    public record Record_5 : Record_4 { }
    public record Record_6 : Record_5 { } // Noncompliant  {{This record has 6 parents which is greater than 5 authorized.}}
    public record Record_7 : Record_6 { } // Noncompliant  {{This record has 7 parents which is greater than 5 authorized.}}
}
