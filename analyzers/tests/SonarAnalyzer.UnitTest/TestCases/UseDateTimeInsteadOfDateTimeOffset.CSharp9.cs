using System;
using System.Globalization;

DateTime myDate = new(1); // Noncompliant {{Prefer using "DateTimeOffset" instead of "DateTime"}}
//                ^^^^^^
_ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar());                   // Noncompliant
_ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);                          // Noncompliant
_ = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Noncompliant

_ = DateTime.UnixEpoch; // Noncompliant
Span<char> span = new();
myDate.TryFormat(span, out int myInt);
myDate.AddMicroseconds(0);
