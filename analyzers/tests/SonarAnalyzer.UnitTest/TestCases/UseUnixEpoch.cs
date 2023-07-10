using System;
using System.Globalization;
using MyAlias = System.DateTime;

public class Program
{
    private readonly DateTime Epoch = new DateTime(1970, 1, 1); // Noncompliant {{Use "DateTime.UnixEpoch" instead of creating DateTime instances that point to the unix epoch time}}
    //                                ^^^^^^^^^^^^^^^^^^^^^^^^

    private readonly DateTimeOffset EpochOff = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero); // Noncompliant {{Use "DateTimeOffset.UnixEpoch" instead of creating DateTimeOffset instances that point to the unix epoch time}}
    //                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    private const long EpochTicks = 621355968000000000;
    private const long EpochTicksUnderscores = 621_355_968_000_000_000;
    private const long EpochTicksBinary = 0b100010011111011111111111010111110111101101011000000000000000;
    private const long EpochTicksHex = 0x89F7FF5F7B58000;
    private const long SomeLongConst = 6213;

    void BasicCases(DateTime dateTime)
    {
        var timeSpan = dateTime - new DateTime(1970, 1, 1); // Noncompliant

        if (dateTime < new DateTime(1970, 1, 1)) // Noncompliant
        {
            return;
        }

        var compliant0 = new DateTime(1971, 1, 1); // Compliant
        var compliant1 = new DateTime(1970, 2, 1); // Compliant
        var compliant2 = new DateTime(1970, 1, 2); // Compliant
        var compliant3 = DateTime.UnixEpoch; // Compliant
        var compliant4 = DateTimeOffset.UnixEpoch; // Compliant

        var year = 1970;
        var dateTime2 = new DateTime(year, 1, 1); // FN
    }

    void EdgeCases()
    {
        var dateTimeOffset = new DateTimeOffset(new DateTime(1970, 1, 1), new TimeSpan(0, 0, 0)); // Noncompliant
        var dateTime = new DateTime(true ? 1970 : 1971, 1, 1); // FN
        dateTime = new DateTime(1970, 01, 01); // Noncompliant
        dateTime = new DateTime(1970, 0x1, 0x1); // Noncompliant
        dateTime = new System.DateTime(1970, 1, 1); // Noncompliant
        dateTime = new MyAlias(1970, 1, 1); // Noncompliant
    }

    void DateTimeConstructors(int ticks, int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
    {
        // default date
        var ctor0_0 = new DateTime(); // Compliant

        // ticks
        var ctor1_0 = new DateTime(1970); // Compliant
        var ctor1_1 = new DateTime(ticks); // Compliant
        var ctor1_2 = new DateTime(ticks: ticks); // Compliant
        var ctor1_3 = new DateTime(621355968000000000); // Noncompliant
        var ctor1_4 = new DateTime(EpochTicks); // Noncompliant: const variables are tracked
        var ctor1_5 = new DateTime(EpochTicksUnderscores); // Noncompliant: const variables are tracked
        var ctor1_6 = new DateTime(EpochTicksBinary); // Noncompliant: const variables are tracked
        var ctor1_7 = new DateTime(EpochTicksHex); // Noncompliant: const variables are tracked
        var ctor1_8 = new DateTime(SomeLongConst); // Compliant

        // year, month, and day
        var ctor2_0 = new DateTime(1970, 1, 1); // Noncompliant
        var ctor2_1 = new DateTime(year, month, day); // Compliant
        var ctor2_2 = new DateTime(month: month, day: day, year: year); // Compliant
        var ctor2_3 = new DateTime(month: 1, day: 1, year: 1970); // Noncompliant

        // year, month, day, and calendar
        var ctor3_0 = new DateTime(1970, 1, 1, new GregorianCalendar()); // Noncompliant
        var ctor3_1 = new DateTime(1970, 3, 1, new GregorianCalendar()); // Compliant
        var ctor3_2 = new DateTime(1970, 1, 1, new ChineseLunisolarCalendar()); // Compliant
        var ctor3_3 = new DateTime(month: 1, day: 1, calendar: new GregorianCalendar(), year: 1970); // Noncompliant
        var ctor3_4 = new DateTime(month: 1, day: 1, calendar: new ChineseLunisolarCalendar(), year: 1970); // Compliant

        // year, month, day, hour, minute, and second
        var ctor4_0 = new DateTime(1970, 1, 1, 0, 0, 0); // Noncompliant
        var ctor4_1 = new DateTime(1970, 1, 1, 0, 0, 1); // Compliant
        var ctor4_2 = new DateTime(year: 1970, minute: minute, month: 1, day: 1, hour: 0, second: 0); // Compliant
        var ctor4_3 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, second: 0); // Noncompliant

        // year, month, day, hour, minute, second, and calendar
        var ctor5_0 = new DateTime(1970, 1, 1, 0, 0, 0, new GregorianCalendar()); // Noncompliant
        var ctor5_1 = new DateTime(1970, 1, 1, 0, 1, 0, new GregorianCalendar()); // Compliant
        var ctor5_2 = new DateTime(1970, 1, 1, 0, 0, 0, new ChineseLunisolarCalendar()); // Compliant
        var ctor5_3 = new DateTime(year: 1970, second: 0, minute: 0, day: 1, month: 1, hour: 0, calendar: new GregorianCalendar()); // Noncompliant
        var ctor5_4 = new DateTime(year: 1970, second: 0, minute: 0, day: 1, month: 1, hour: 0, calendar: calendar); // Compliant

        // year, month, day, hour, minute, second, and DateTimeKind value
        var ctor6_0 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Noncompliant
        var ctor6_1 = new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc); // Compliant
        var ctor6_2 = new DateTime(1970, 1, 1, hour, 0, 0, DateTimeKind.Utc); // Compliant
        var ctor6_3 = new DateTime(month: 1, year: 1970, day: 1, hour: 0, second: 0, minute: 0, kind: DateTimeKind.Utc); // Noncompliant
        var ctor6_4 = new DateTime(month: 1, year: 1970, day: 1, hour: hour, second: 0, minute: 0, kind: DateTimeKind.Utc); // Compliant
        var ctor6_5 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified); // Compliant
        var ctor6_6 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local); // Compliant

        // year, month, day, hour, minute, second, and millisecond
        var ctor7_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0); // Noncompliant
        var ctor7_1 = new DateTime(1970, 1, 1, 0, 0, 0, 1); // Compliant
        var ctor7_3 = new DateTime(year, month, day, hour, minute, second, millisecond); // Compliant
        var ctor7_4 = new DateTime(year: 1970, minute: minute, month: 1, day: 1, hour: 0, millisecond: 0, second: 0); // Compliant
        var ctor7_5 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0); // Noncompliant

        // year, month, day, hour, minute, second, millisecond, and calendar
        var ctor8_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar()); // Noncompliant
        var ctor8_1 = new DateTime(1970, 1, 1, 0, 0, 0, 1, new GregorianCalendar()); // Compliant
        var ctor8_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, new ChineseLunisolarCalendar()); // Compliant
        var ctor8_3 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, calendar: new GregorianCalendar()); // Noncompliant

        // year, month, day, hour, minute, second, millisecond, and DateTimeKind value
        var ctor9_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // Noncompliant
        var ctor9_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified); // Compliant
        var ctor9_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local); // Compliant
        var ctor9_3 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, kind: DateTimeKind.Utc); // Noncompliant
        var ctor9_4 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, kind: kind); // Compliant

        // year, month, day, hour, minute, second, millisecond, calendar and DateTimeKind value
        var ctor10_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), DateTimeKind.Utc); // Noncompliant
        var ctor10_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), DateTimeKind.Local); // Compliant
        var ctor10_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, new ChineseLunisolarCalendar(), DateTimeKind.Utc); // Compliant
        var ctor10_3 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, calendar: new GregorianCalendar(), millisecond: 0, second: 0, kind: DateTimeKind.Utc); // Noncompliant
        var ctor10_4 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, calendar: new GregorianCalendar(), millisecond: 0, second: second, kind: DateTimeKind.Utc); // Compliant

        // year, month, day, hour, minute, second, millisecond and microsecond
        var ctor11_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0); // Noncompliant
        var ctor11_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 1); // Compliant
        var ctor11_2 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, microsecond: 0); // Noncompliant
        var ctor11_3 = new DateTime(year: 1970, microsecond: 0, minute: minute, month: 1, hour: 0, day: 1, millisecond: millisecond, second: 0); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond and calendar
        var ctor12_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar()); // Noncompliant
        var ctor12_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 1, new GregorianCalendar()); // Compliant
        var ctor12_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, new ChineseLunisolarCalendar()); // Compliant
        var ctor12_3 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, calendar: new GregorianCalendar(), millisecond: 0, second: 0, microsecond: 0); // Noncompliant
        var ctor12_4 = new DateTime(year: 1970, minute: minute, month: 1, day: 1, hour: 0, calendar: new GregorianCalendar(), millisecond: 0, second: 0, microsecond: 0); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond and DateTimeKind value
        var ctor13_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Utc); // Noncompliant
        var ctor13_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 1, DateTimeKind.Utc); // Compliant
        var ctor13_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Unspecified); // Compliant
        var ctor13_3 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Local); // Compliant
        var ctor13_4 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, kind: DateTimeKind.Utc, millisecond: 0, second: 0, microsecond: 0); // Noncompliant
        var ctor13_5 = new DateTime(year: 1970, minute: minute, month: 1, day: 1, hour: 0, kind: DateTimeKind.Utc, millisecond: 0, second: 0, microsecond: 0); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond calendar and DateTimeKind value
        var ctor14_0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar(), DateTimeKind.Utc); // Noncompliant
        var ctor14_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 1, new GregorianCalendar(), DateTimeKind.Utc); // Compliant
        var ctor14_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar(), DateTimeKind.Unspecified); // Compliant
        var ctor14_3 = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, new ChineseLunisolarCalendar(), DateTimeKind.Utc); // Compliant
        var ctor14_4 = new DateTime(year: 1970, minute: 0, month: 1, day: 1, hour: 0, kind: DateTimeKind.Utc, millisecond: 0, second: 0, microsecond: 0, calendar: new GregorianCalendar()); // Noncompliant
        var ctor14_5 = new DateTime(year: 1970, minute: minute, month: 1, day: 1, hour: 0, kind: DateTimeKind.Utc, millisecond: 0, second: 0, microsecond: 0, calendar: calendar); // Compliant
    }

    void DateTimeOffsetConstructors(TimeSpan timeSpan, DateTime dateTime, int ticks, int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, Calendar calendar, DateTimeKind kind)
    {
        // default date
        var ctor0_0 = new DateTimeOffset(); // Compliant

        // datetime
        var ctor1_0 = new DateTimeOffset(new DateTime()); // Compliant
        var ctor1_1 = new DateTimeOffset(dateTime); // Compliant
        var ctor1_2 = new DateTimeOffset(new DateTime(1970, 1, 1)); // Noncompliant

        // datetime and timespan
        var ctor2_0 = new DateTimeOffset(new DateTime(), TimeSpan.Zero); // Compliant
        var ctor2_1 = new DateTimeOffset(new DateTime(), timeSpan); // Compliant
        var ctor2_2 = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero); // Noncompliant

        // year, month, day, hour, minute, second, millisecond, offset and calendar
        var ctor3_0 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, calendar, timeSpan); // Compliant
        var ctor3_1 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), TimeSpan.Zero); // Noncompliant
        var ctor3_2 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), new TimeSpan(0)); // Noncompliant
        var ctor3_3 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), new TimeSpan(1)); // Compliant
        var ctor3_4 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: TimeSpan.Zero); // Noncompliant
        var ctor3_5 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: new TimeSpan(0)); // Noncompliant
        var ctor3_6 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: new TimeSpan(2)); // Compliant
        var ctor3_7 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, millisecond: 0, calendar: calendar, offset: new TimeSpan(0)); // Compliant

        // year, month, day, hour, minute, second, millisecond, microsecond, offset and calendar
        var ctor4_0 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, calendar, timeSpan); // Compliant
        var ctor4_1 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar(), TimeSpan.Zero); // Noncompliant
        var ctor4_2 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar(), new TimeSpan(0)); // Noncompliant
        var ctor4_3 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, new GregorianCalendar(), new TimeSpan(1)); // Compliant
        var ctor4_4 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 1, new GregorianCalendar(), new TimeSpan(0)); // Compliant
        var ctor4_5 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, microsecond: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: TimeSpan.Zero); // Noncompliant
        var ctor4_6 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, microsecond: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: new TimeSpan(0)); // Noncompliant
        var ctor4_7 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, microsecond: 0, millisecond: 0, calendar: new GregorianCalendar(), offset: new TimeSpan(2)); // Compliant
        var ctor4_8 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, microsecond: 0, millisecond: 0, calendar: calendar, offset: new TimeSpan(0)); // Compliant
        var ctor4_9 = new DateTimeOffset(hour: 0, month: 1, day: 1, year: 1970, minute: 0, second: 0, microsecond: 1, millisecond: 0, calendar: new GregorianCalendar(), offset: new TimeSpan(0)); // Compliant

        // year, month, day, hour, minute, second and offset
        var ctor5_0 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0)); // Noncompliant
        var ctor5_1 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(1)); // Compliant
        var ctor5_2 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, timeSpan); // Compliant
        var ctor5_3 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, timeSpan); // Compliant
        var ctor5_4 = new DateTimeOffset(1970, 1, 1, 0, 0, 2, new TimeSpan(0)); // Compliant
        var ctor5_5 = new DateTimeOffset(year: 1970, minute: 0, month: 1, day: 1, hour: 0, second: 0, offset: new TimeSpan(0)); // Noncompliant

        // year, month, day, hour, minute, second, millisecond and offset
        var ctor6_0 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new TimeSpan(0)); // Noncompliant
        var ctor6_1 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero); // Noncompliant
        var ctor6_2 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new TimeSpan(2, 14, 18)); // Compliant
        var ctor6_3 = new DateTimeOffset(1970, 1, 1, 0, 1, 0, 0, TimeSpan.Zero); // Compliant
        var ctor6_4 = new DateTimeOffset(year: 1970, minute: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, offset: new TimeSpan(0)); // Noncompliant

        // year, month, day, hour, minute, second, millisecond and offset
        var ctor7_0 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, new TimeSpan(0)); // Noncompliant
        var ctor7_1 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, new TimeSpan(2, 14, 18)); // Compliant
        var ctor7_2 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, TimeSpan.Zero); // Noncompliant
        var ctor7_3 = new DateTimeOffset(1970, 1, 1, 0, 1, 0, 0, 0, TimeSpan.Zero); // Compliant
        var ctor7_4 = new DateTimeOffset(year: 1970, minute: 0, microsecond: 0, month: 1, day: 1, hour: 0, millisecond: 0, second: 0, offset: new TimeSpan(0)); // Noncompliant
    }
}

public class FakeDateTime
{
    void MyMethod()
    {
        _ = new DateTime(1970, 1, 1); // Compliant
        _ = new DateTime("hello"); // Compliant
    }

    public class DateTime
    {
        public DateTime(int year, int month, int day) { }
        public DateTime(string ticks) { }
    }
}
