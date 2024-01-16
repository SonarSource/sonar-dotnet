using System;

DateTime dateTime = new(1970, 1, 1); // Noncompliant
DateTimeOffset dateTimeOffset = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero); // Noncompliant
