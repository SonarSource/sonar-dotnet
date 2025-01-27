using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

public class Program
{
    private string _first = "";
    private string _second = "";
    private string First { get; set; } = "";
    private string Second { get; set; } = "";
    private Person _person;
    private Person Person { get; set; }

    public void Basics(ILogger logger, int first, int second, int third)
    {
        logger.LogInformation("No placeholder");                        // Compliant
        logger.LogInformation("Arg: {First}", first);
        logger.LogInformation("Arg: {First}", first);                   // Compliant - S6678 should raise for not using Pascal Case naming, but not this rule
        logger.LogInformation("Arg: {First}", 42);
        logger.LogInformation("Arg: {First}", second);                  // Compliant - maybe anotherArg is passed by mistake, hard to decide without knowing the context
        logger.LogInformation("Arg: {First} {Second}", first, first);
        logger.LogInformation("Arg: {First}");
        logger.LogInformation("Arg: {First}", new object());
        logger.LogInformation("Arg: {First}", first.ToString());
        logger.LogInformation("Arg: {First}", second.ToString());
        logger.LogInformation("Arg: {First}", new[] { first });
        logger.LogInformation("Arg: {0}", first);
        logger.LogInformation("Arg: {0} {1}", 1, 0);
        logger.LogInformation("Arg: {_}", first);
        logger.LogInformation("Arg: {}", first);
        logger.LogInformation("Arg: {First} {Second}", first, second);
        logger.LogInformation("Arg: {{First}} {{Second}}", first, second);
        logger.LogInformation("Arg: {First} {Another_Arg}", first, second);
        logger.LogInformation("Arg: {First} {Another Arg}", first, second);
        logger.LogInformation("Arg: {First} {Another.Arg}", first, second);
        logger.LogInformation("Arg: {First} {Second}", first, second);
        logger.LogInformation("Arg: {First} {Second}", first, 42);
        logger.LogInformation("Arg: {First} {First}", first, first);
        logger.LogInformation("Arg: {First} {Second}", first, first);
        logger.LogInformation("Arg: {First} {Second}", new object(), 42);
        logger.LogInformation("Arg: {First} {Second}", new[] { first, second });

        logger.BeginScope("scope");
        logger.BeginScope("Arg: {First} {Second}", first, second);
        logger.BeginScope("Arg: {First} {Second}", second, first);      // Compliant - not a logger method

        logger.LogInformation("Arg: {First} {Second}", second, first);
        //                           ^^^^^ {{Template placeholders should be in the right order: placeholder 'First' does not match with argument 'second'.}}
        //                                             ^^^^^^ Secondary @-1 {{The argument should be 'first' to match placeholder 'First'.}}

        logger.LogInformation("Arg: {First} {Third} {Second}", first, second, third);
        //                                   ^^^^^ {{Template placeholders should be in the right order: placeholder 'Third' does not match with argument 'second'.}}
        //                                                            ^^^^^^ Secondary @-1 {{The argument should be 'third' to match placeholder 'Third'.}}

        logger.LogInformation("Arg: {First} {First}", first, second);   // Compliant - S6677 should raise for duplicate placeholder names, but not this rule
        logger.LogInformation(@"
             {First}
             {Second}", second, first);
        //    ^^^^^ @-1
        //              ^^^^^^ Secondary @-1

        logger.LogInformation("Arg: {First} {Second}", _first, _second);                        // Compliant
        logger.LogInformation("Arg: {First} {Second}", _second, _first);                        // Noncompliant
                                                                                                // Secondary @-1

        logger.LogInformation("Arg: {First} {Second}", First, Second);                          // Compliant
        logger.LogInformation("Arg: {First} {Second}", Second, First);                          // Noncompliant
                                                                                                // Secondary @-1

        logger.LogInformation("Arg: {First} {Second}", _first, Second);                         // Compliant
        logger.LogInformation("Arg: {First} {Second}", _second, First);                         // Noncompliant
                                                                                                // Secondary @-1

        int arg1 = first;
        int arg2 = second;
        logger.LogInformation("Arg: {Arg1} {Arg2}", arg1, arg2);                                // Compliant
        logger.LogInformation("Arg: {Arg1} {Arg2}", arg2, arg1);                                // FN

        logger.LogInformation("Arg: {First} {Second}", (short)second, (float)first);            // Noncompliant
                                                                                                // Secondary @-1

        LoggerExtensions.LogInformation(logger, "Arg: {First} {Second}", second, first);        // Noncompliant
                                                                                                // Secondary @-1
    }

    public void NamedArguments(ILogger logger, int first, int second)
    {
        logger.LogInformation(args: new[] { first, second }, message: "Arg: {Second} {First}"); // Compliant
        logger.LogInformation(args: new[] { first, second }, message: "Arg: {Second} {First}"); // FN
    }

    public void PropertyAccess(ILogger logger, Person person, List<Person> people)
    {
        logger.LogInformation("Person: {FirstName} {Age}", person.FirstName, person.Age);       // Compliant
        logger.LogInformation("Person: {Age} {FirstName}", person.FirstName, person.Age);       // Noncompliant
                                                                                                // Secondary @-1 {{The argument should be 'person.Age' to match placeholder 'Age'.}}

        logger.LogInformation("Person: {FirstName} {Age}", _person.FirstName, _person.Age);     // Compliant
        logger.LogInformation("Person: {Age} {FirstName}", _person.FirstName, _person.Age);     // Noncompliant
                                                                                                // Secondary @-1  {{The argument should be '_person.Age' to match placeholder 'Age'.}}

        logger.LogInformation("Person: {FirstName} {Age}", Person.FirstName, Person.Age);       // Compliant
        logger.LogInformation("Person: {Age} {FirstName}", Person.FirstName, Person.Age);       // Noncompliant
                                                                                                // Secondary @-1  {{The argument should be 'Person.Age' to match placeholder 'Age'.}}

        logger.LogInformation("Father: {Father}, Mother: {Mother}", person.Father.LastName, person.Mother.LastName);    // Compliant
        logger.LogInformation("Father: {Father}, Mother: {Mother}", person.Mother.LastName, person.Father.LastName);    // FN

        logger.LogInformation("Person: {FirstName} {LastName}", person.FirstName, person.LastName);     // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", person.LastName, person.FirstName);     // FN

        logger.LogInformation("People: {Count} {People}", people.Count, people.Select(x => x.FirstName + " " + x.LastName));

        var someFirstName = person.FirstName;
        logger.LogInformation("{PersonFirstName} {PersonAge}", someFirstName, person.Age);      // Compliant

        var first = person.FirstName;
        var last = person.LastName;
        var name = person.LastName;
        var PFN = person.FirstName;
        var PLN = person.LastName;
        logger.LogInformation("Person: {FirstName} {LastName}", first, last);                   // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", last, first);                   // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", last, name);                    // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", name, name);                    // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", name, name);                    // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", PFN, PLN);                      // Compliant
        logger.LogInformation("Person: {FirstName} {LastName}", PLN, PFN);                      // Compliant
        logger.LogInformation("Person: {PLN} {PFN}", PLN, PFN);                                 // Compliant
        logger.LogInformation("Person: {PLN} {PFN}", PFN, PLN);                                 // Compliant

        var allPeople = people;
        logger.LogInformation("People: {Count} {People}", people.Count, allPeople);
    }

    public void Overloads(ILogger logger, int first, int second)
    {
        logger.LogInformation(new EventId(42), "Arg: {First} {Second}", first, second);
        logger.LogInformation(new EventId(42), "Arg: {Second} {First}", first, second);         // Noncompliant
                                                                                                // Secondary @-1
        logger.LogInformation(new Exception(), "Arg: {First} {Second}", first, second);
        logger.LogInformation(new Exception(), "Arg: {Second} {First}", first, second);         // Noncompliant
                                                                                                // Secondary @-1
    }

    public void Casing(ILogger logger, int first, int second)
    {
        logger.LogInformation("Arg: {Second} {First}", first, second);      // Noncompliant
                                                                            // Secondary @-1
        logger.LogInformation("Arg: {_second_} {_first_}", first, second);  // Noncompliant
                                                                            // Secondary @-1
        logger.LogInformation("Arg: {SECOND} {FIRST}", first, second);      // Noncompliant
                                                                            // Secondary @-1
    }

    public void IncorrectPlaceholderFormat(ILogger logger, int first, int second)
    {
        logger.LogInformation("Arg: {@First} {@Second}", second, first);                    // Noncompliant
                                                                                            // Secondary @-1
        logger.LogInformation("Arg: {&First} {&Second}", second, first);
        logger.LogInformation("Arg: {First,42} {Second,42}", second, first);                // Noncompliant
                                                                                            // Secondary @-1
        logger.LogInformation("Arg: {First,First} {Second,Second}", second, first);
    }

    public void ClassImplementsILogger(CustomLogger logger, int first, int second)
    {
        logger.LogInformation("Arg: {First} {Second}", first, second);  // Compliant
        logger.LogInformation("Arg: {First} {Second}", second, first);  // Noncompliant
                                                                        // Secondary @-1
    }

    public void ClassDoesNotImplementILogger(NotILogger notILogger, int first, int second)
    {
        notILogger.LogInformation("Arg: {First} {Second}", first, second);
        notILogger.LogCritical("Arg: {First} {Second}", second, first);
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public Person Mother { get; set; }
    public Person Father { get; set; }
}

public class CustomLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
    }
}

public class NotILogger
{
    public void LogInformation(string message, params object[] args) { }
}

public static class NotILoggerExtensions
{
    public static void LogCritical(this NotILogger logger, string message, params object[] args) { }
}
