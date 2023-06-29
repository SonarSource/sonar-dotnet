using System;
using System.Globalization;

public class Program
{
    public void Noncompliant()
    {
        var dt = new DateTime(); // Noncompliant
        dt = new DateTime(1623); // Noncompliant
        dt = new DateTime(1994, 07, 05); // Noncompliant
        dt = new DateTime(1994, 07, 05, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, 42); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar()); // Noncompliant
    }

    public void Compliant()
    {
        var dt = new DateTime(1623, DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, DateTimeKind.Local);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar(), DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, DateTimeKind.Utc);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar(), DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, DateTimeKind.Unspecified);
    }
}
