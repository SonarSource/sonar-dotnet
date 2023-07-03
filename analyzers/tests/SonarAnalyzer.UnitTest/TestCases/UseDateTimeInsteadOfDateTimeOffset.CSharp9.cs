using System;
using System.Globalization;

DateTime myDate = new(1); // Noncompliant {{Prefer using "DateTimeOffset" struct instead of "DateTime"}}
//                ^^^^^^
_ = DateTime.MaxValue;    // Noncompliant
_ = DateTime.Now;         // Noncompliant

_ = myDate.AddDays(1);
DateTime.Parse("06/01/1993"); // Noncompliant
