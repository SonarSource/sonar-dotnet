using System;
using System.Linq;
using static System.DateTime;
using AliasedDateTime = System.DateTime;

class Test
{
    void TestCases()
    {
        var currentTime = DateTime.Now;                                             // Noncompliant {{Use UTC when recording DateTime instants}}
        //                ^^^^^^^^^^^^
        currentTime = System.DateTime.Now;                                          // Noncompliant
        currentTime = Now;                                                          // FN
        currentTime = AliasedDateTime.Now;                                          // Noncompliant

        var today = DateTime.Today;                                                 // Noncompliant - same as DateTime.Now, but the the time is set to 00:00:00

        var currentTimeWithOffset = DateTimeOffset.Now;                             // Compliant - DateTimeOffset stores the time zone
        currentTimeWithOffset = DateTimeOffset.UtcNow;

        currentTime = DateTimeOffset.Now.DateTime;                                  // Noncompliant - same as DateTime.Now
        currentTime = DateTimeOffset.UtcNow.DateTime;                               // Compliant - same as DateTime.UtcNow

        var currentDate = DateTimeOffset.Now.Date;                                  // Noncompliant - same as DateTime.Now.Date
        currentDate = DateTimeOffset.UtcNow.Date;                                   // Compliant - same as DateTime.UtcNow.Date

        currentDate = DateTime.Now.AddDays(1);                                      // Noncompliant
        currentDate = (DateTime.Now).AddDays(1);                                    // Noncompliant
        currentDate = DateTime.Now + TimeSpan.FromDays(1);                          // Noncompliant

        if (DateTime.Now > currentTime)                                             // Noncompliant
        {
        }

        var hours = Enumerable.Range(0, 10).Select(x => DateTime.Now.AddHours(x));  // Noncompliant

        void InnerMethod()
        {
            var now = DateTime.Now;                                                 // Noncompliant
        }

        var propertyName = nameof(DateTime.Now);                                    // Compliant - the value of the property is not used
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
        _ = DateTime.Now;                                                           // Compliant - this is not System.DateTime
    }
}

class FakeNameOf
{
    string nameof(object o) => "";

    void UsesFakeNameOfMethod()
    {
        nameof(DateTime.Now);                                                       // FN
    }
}
