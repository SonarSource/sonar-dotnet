using System;
using System.Globalization;

public class Program
{
    public void DateTimeTest(int ticks, int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, Calendar calendar, DateTimeKind kind)
    {
        // default date
        var ctor0_0 = new DateTime(); // Compliant

        // ticks
        var ctor1_0 = new DateTime(1); // Compliant
        var ctor1_1 = new DateTime(ticks); // Compliant
        var ctor1_2 = new DateTime(ticks: ticks);

        // year, month, and day
        var ctor2_0 = new DateTime(1, 1, 1); // Noncompliant {{Use "DateOnly" instead of just setting the date for a "DateTime" struct}}
        //            ^^^^^^^^^^^^^^^^^^^^^
        var ctor2_1 = new DateTime(1, 3, 1); // Noncompliant
        var ctor2_2 = new DateTime(year, month, day); // Compliant
        var ctor2_3 = new DateTime(month: month, day: day, year: year); // Compliant
        var ctor2_4 = new DateTime(month: 1, day: 3, year: 1); // Noncompliant

        // year, month, day, and calendar
        var ctor3_0 = new DateTime(1, 1, 1, new GregorianCalendar()); // Compliant
        var ctor3_1 = new DateTime(1, 3, 1, new GregorianCalendar()); // Compliant
        var ctor3_2 = new DateTime(year, month, day, calendar); // Compliant
        var ctor3_3 = new DateTime(month: month, day: day, calendar: calendar, year: year); // Compliant

        // year, month, day, hour, minute, and second
        var ctor4_0 = new DateTime(1, 1, 1, 1, 1, 1); // Noncompliant {{Use "TimeOnly" instead of just setting the time for a "DateTime" struct}}
        //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        var ctor4_1 = new DateTime(1, 3, 1, 1, 1, 1); // Compliant
        var ctor4_2 = new DateTime(1, 1, 1, 1, 3, 1); // Noncompliant
        var ctor4_3 = new DateTime(year, month, day, hour, minute, second); // Compliant
        var ctor4_4 = new DateTime(year: year, minute: 1, month: 1, day: 1, hour: 1, second: second); // Compliant
        var ctor4_5 = new DateTime(year: 1, minute: 3, month: 1, day: 1, hour: 1, second: 4); // Noncompliant
        var ctor4_6 = new DateTime(year: 1, minute: minute, month: 1, day: 1, hour: hour, second: 4); // Noncompliant

        // year, month, day, hour, minute, second, and calendar
        var ctor5_0 = new DateTime(1, 1, 1, 1, 1, 1, new GregorianCalendar()); // Compliant
        var ctor5_1 = new DateTime(1, 3, 1, 1, 1, 1, new GregorianCalendar()); // Compliant

        // year, month, day, hour, minute, second, and DateTimeKind value
        var ctor6_0 = new DateTime(1, 1, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant
        var ctor6_1 = new DateTime(1, 3, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant

        // year, month, day, hour, minute, second, and millisecond
        var ctor7_0 = new DateTime(1, 1, 1, 1, 1, 1, 1); // Noncompliant {{Use "TimeOnly" instead of just setting the time for a "DateTime" struct}}
        var ctor7_1 = new DateTime(1, 1, 1, 1, 1, 1, 3); // Noncompliant
        var ctor7_2 = new DateTime(1, 3, 1, 1, 1, 1, 1); // Compliant
        var ctor7_3 = new DateTime(year, month, day, hour, minute, second, millisecond); // Compliant
        var ctor7_4 = new DateTime(year: year, minute: 1, month: 1, day: 1, hour: 1, millisecond: 1, second: second); // Compliant
        var ctor7_5 = new DateTime(year: 1, minute: 3, month: 1, day: 1, hour: 1, millisecond: 1, second: 4); // Noncompliant
        var ctor7_6 = new DateTime(year: 1, minute: minute, month: 1, day: 1, hour: hour, millisecond: millisecond, second: 4); // Noncompliant

        // year, month, day, hour, minute, second, millisecond, and calendar
        var ctor8_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar()); // Compliant
        var ctor8_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, new GregorianCalendar()); // Compliant

        // year, month, day, hour, minute, second, millisecond, and DateTimeKind value
        var ctor9_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant
        var ctor9_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant

        // year, month, day, hour, minute, second, millisecond, calendar and DateTimeKind value
        var ctor10_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Compliant
        var ctor10_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Compliant

        // year, month, day, hour, minute, second, millisecond and microsecond
        var ctor11_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, 1); // Noncompliant {{Use "TimeOnly" instead of just setting the time for a "DateTime" struct}}
        var ctor11_1 = new DateTime(1, 1, 1, 1, 1, 1, 1, 3); // Noncompliant
        var ctor11_2 = new DateTime(1, 3, 1, 1, 1, 1, 1, 1); // Compliant
        var ctor11_3 = new DateTime(year, month, day, hour, minute, second, millisecond, microsecond); // Compliant
        var ctor11_4 = new DateTime(year: year, minute: 1, month: 1, day: 1, hour: 1, millisecond: 1, second: second, microsecond: microsecond); // Compliant
        var ctor11_5 = new DateTime(year: 1, minute: 3, month: 1, day: 1, hour: 1, millisecond: 1, second: 4, microsecond: 2); // Noncompliant
        var ctor11_6 = new DateTime(year: 1, microsecond: 2, minute: minute, month: 1, hour: hour, day: 1, millisecond: millisecond, second: 4); // Noncompliant

        // year, month, day, hour, minute, second, millisecond, microsecond and calendar
        var ctor12_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar()); // Compliant
        var ctor12_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, 1, new GregorianCalendar()); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond and DateTimeKind value
        var ctor13_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant
        var ctor13_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond calendar and DateTimeKind value
        var ctor14_0 = new DateTime(1, 1, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Compliant
        var ctor14_1 = new DateTime(1, 3, 1, 1, 1, 1, 1, 1, new GregorianCalendar(), DateTimeKind.Utc); // Compliant
    }
}
