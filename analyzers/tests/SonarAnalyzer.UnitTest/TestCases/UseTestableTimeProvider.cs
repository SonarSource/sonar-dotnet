using System;

public class DateTimeAsProvider
{
    public void Noncompliant()
    {
        var now = DateTime.Now; // Noncompliant {{Use a testable (date) time provider instead.}}
        //        ^^^^^^^^^^^^
        var utc = DateTime.UtcNow; // Noncompliant
        var today = DateTime.Today; // Noncompliant
    }

    public void CompliantAre()
    {
        var other = DateTime.DaysInMonth(2000, 2); // Compliant
    }
}
