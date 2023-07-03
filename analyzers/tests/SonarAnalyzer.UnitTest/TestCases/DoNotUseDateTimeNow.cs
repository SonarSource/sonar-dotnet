using System;
using static System.DateTime;
using AliasedDateTime = System.DateTime;

class Test
{
    void TestCases()
    {
        var currentTime = DateTime.Now; // Noncompliant {{Do not use 'DateTime.Now' for recording DateTime instants}}
        //                ^^^^^^^^^^^^
        currentTime = System.DateTime.Now; // Noncompliant
        currentTime = Now; // Noncompliant
        currentTime = AliasedDateTime.Now; // Noncompliant
    }
}

class CustomTypeCalledDateTime
{
    public struct DateTime
    {
        public static DateTime Now => new DateTime();
    }

    CustomTypeCalledDateTime()
    {
        _ = DateTime.Now;                               // Compliant - this is not System.DateTime
    }
}
