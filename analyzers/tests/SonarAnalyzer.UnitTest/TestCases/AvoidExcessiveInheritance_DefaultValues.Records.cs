using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace Tests.Diagnostics
{
    public record BaseRecord { }
}

namespace RecordNamespace
{
    public record Record_0 : Tests.Diagnostics.BaseRecord { }
    public record Record_1 : Record_0 { }
    public record Record_2 : Record_1 { }
    public record Record_3 : Record_2 { }
    public record Record_4 : Record_3 { }
    public record Record_5 : Record_4 { }
    public record Record_6 : Record_5 { } // Noncompliant  {{This record has 6 parents which is greater than 5 authorized.}}
    public record Record_7 : Record_6 { } // Noncompliant  {{This record has 7 parents which is greater than 5 authorized.}}
}

public record Record_0(string s);
public record Record_1(string s) : Record_0(s);
public record Record_2(string s) : Record_1(s);
public record Record_3(string s) : Record_2(s);
public record Record_4(string s) : Record_3(s);
public record Record_5(string s) : Record_4(s);
public record Record_6(string s) : Record_5(s); // Noncompliant  {{This record has 6 parents which is greater than 5 authorized.}}
public record Record_7(string s) : Record_6(s); // Noncompliant  {{This record has 7 parents which is greater than 5 authorized.}}
