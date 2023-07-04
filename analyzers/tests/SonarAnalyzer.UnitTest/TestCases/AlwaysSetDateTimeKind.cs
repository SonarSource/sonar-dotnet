using System;
using System.Globalization;
using MyAlias = System.DateTime;

public class Program
{
    public void Noncompliant()
    {
        var dt = new DateTime(); // Noncompliant {{Provide the "DateTimeKind" when creating this object.}}
        //       ^^^^^^^^^^^^^^
        dt = new DateTime(1623); // Noncompliant
        dt = new DateTime(1994, 07, 05); // Noncompliant
        dt = new DateTime(1994, 07, 05, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar()); // Noncompliant
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar()); // Noncompliant
        dt = new MyAlias(); // FN
        dt = new System.DateTime(); // Noncompliant
    }

    public void Compliant()
    {
        var dt = new DateTime(1623, DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, DateTimeKind.Local);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar(), DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, DateTimeKind.Utc);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, new GregorianCalendar(), DateTimeKind.Unspecified);
        dt = new DateTime(1994, 07, 05, 16, 23, 00, 42, DateTimeKind.Unspecified);
        dt = new System.(1623); // Error [CS1001]
    }
}

public class FakeDateTime
{
    private class DateTime
    {

    }

    private void Compliant()
    {
        var dt = new DateTime();
    }
}
