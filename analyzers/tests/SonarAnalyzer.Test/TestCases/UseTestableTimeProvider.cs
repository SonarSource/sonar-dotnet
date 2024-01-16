using System;

public class DateTimeAsProvider
{
    public void Noncompliant()
    {
        var now = DateTime.Now; // Noncompliant {{Use a testable (date) time provider instead.}}
        //        ^^^^^^^^^^^^
        var utc = DateTime.UtcNow; // Noncompliant
        var today = DateTime.Today; // Noncompliant

        var offsetNow = DateTimeOffset.Now; // Noncompliant
        var offsetUTC = DateTimeOffset.UtcNow; // Noncompliant
    }

    /// <see cref="DateTimeOffset.UtcNow"/> Compliant
    public void CompliantAre()
    {
        var other = DateTime.DaysInMonth(2000, 2); // Compliant
        var noSystemDateTime = NotSystem.DateTime.Now; // Compliant

        var otherOffset = DateTimeOffset.MaxValue; // Compliant
        var noSystemDateTimeOffset = NotSystem.DateTimeOffset.Now; // Compliant

        var str = nameof(DateTime.Now); // Compliant
    }
}

namespace NotSystem
{
    public class DateTime
    {
        public static DateTime Now => new DateTime();
    }

    public class DateTimeOffset
    {
        public static DateTimeOffset Now => new DateTimeOffset();
    }
}
