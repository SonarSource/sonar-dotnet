using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

public class Program
{
    public void Basics(ILogger logger, int arg, int anotherArg, int yetAnotherArg)
    {
        logger.LogInformation("No placeholder");                            // Compliant
        logger.LogInformation("Arg: {Arg}", arg);
        logger.LogInformation("Arg: {arg}", arg);                           // Compliant - S6678 should raise for not using Pascal Case naming, but not this rule
        logger.LogInformation("Arg: {Arg}", 42);
        logger.LogInformation("Arg: {Arg}", anotherArg);                    // Compliant - maybe anotherArg is passed by mistake, hard to decide without knowing the context
        logger.LogInformation("Arg: {Arg} {AnotherArg}", arg, arg);
        logger.LogInformation("Arg: {Arg}");
        logger.LogInformation("Arg: {Arg}", new object());
        logger.LogInformation("Arg: {Arg}", arg.ToString());
        logger.LogInformation("Arg: {Arg}", anotherArg.ToString());
        logger.LogInformation("Arg: {Arg}", new[] { arg });
        logger.LogInformation("Arg: {0}", arg);
        logger.LogInformation("Arg: {0} {1}", 1, 0);
        logger.LogInformation("Arg: {_}", arg);
        logger.LogInformation("Arg: {}", arg);
        logger.LogInformation("Arg: {Arg} {AnotherArg}", arg, anotherArg);
        logger.LogInformation("Arg: {{Arg}} {{AnotherArg}}", arg, anotherArg);
        logger.LogInformation("Arg: {Arg} {Another_Arg}", arg, anotherArg);
        logger.LogInformation("Arg: {Arg} {Another Arg}", arg, anotherArg);
        logger.LogInformation("Arg: {Arg} {Another.Arg}", arg, anotherArg);
        logger.LogInformation("Arg: {ARG} {ANOTHERARG}", arg, anotherArg);
        logger.LogInformation("Arg: {Arg} {AnotherArg}", arg, 42);
        logger.LogInformation("Arg: {Arg} {Arg}", arg, arg);
        logger.LogInformation("Arg: {Arg} {AnotherArg}", arg, arg);
        logger.LogInformation("Arg: {Arg} {AnotherArg}", new object(), 42);
        logger.LogInformation("Arg: {Arg} {AnotherArg}", new[] { arg, anotherArg });

        logger.BeginScope("scope");
        logger.BeginScope("Arg: {Arg} {AnotherArg}", arg, anotherArg);
        logger.BeginScope("Arg: {Arg} {AnotherArg}", anotherArg, arg);      // Compliant - not a logger method

        logger.LogInformation("Arg: {Arg} {AnotherArg}", anotherArg, arg);
        //                           ^^^ {{Template placeholders should be in the right order: placeholder 'Arg' does not match with argument 'anotherArg'.}}
        //                                               ^^^^^^^^^^ Secondary @-1

        logger.LogInformation("Arg: {Arg} {YetAnotherArg} {AnotherArg}", arg, anotherArg, yetAnotherArg);
        //                                 ^^^^^^^^^^^^^ {{Template placeholders should be in the right order: placeholder 'YetAnotherArg' does not match with argument 'anotherArg'.}}
        //                                                                    ^^^^^^^^^^ Secondary @-1

        logger.LogInformation("Arg: {Arg} {Arg}", arg, anotherArg);         // Compliant - S6677 should raise for duplicate placeholder names, but not this rule
        logger.LogInformation(@"
             {Arg}
             {AnotherArg}", anotherArg, arg);
        //    ^^^ @-1
        //                  ^^^^^^^^^^ Secondary @-1

        logger.LogInformation("Arg: {Arg} {AnotherArg}", (short)anotherArg, (float)arg);            // Noncompliant
                                                                                                    // Secondary @-1

        LoggerExtensions.LogInformation(logger, "Arg: {Arg} {AnotherArg}", anotherArg, arg);        // Noncompliant
                                                                                                    // Secondary @-1
    }

    public void NamedArguments(ILogger logger, int arg, int anotherArg)
    {
        logger.LogInformation(args: new[] { arg, anotherArg }, message: "Arg: {AnotherArg} {Arg}");     // Compliant
        logger.LogInformation(args: new[] { arg, anotherArg }, message: "Arg: {AnotherArg} {Arg}");     // FN
    }

    public void PropertyAccess(ILogger logger, Person person, List<Person> people)
    {
        logger.LogInformation("Person: {FirstName} {LastName}", person.FirstName, person.LastName);                     // Compliant
        logger.LogInformation("Person: {LastName} {FirstName}", person.FirstName, person.LastName);                     // Noncompliant
                                                                                                                        // Secondary @-1
        logger.LogInformation("Father: {Father}, Mother: {Mother}", person.Father.LastName, person.Mother.LastName);    // Compliant
        logger.LogInformation("Father: {Father}, Mother: {Mother}", person.Mother.LastName, person.Father.LastName);    // FN
        logger.LogInformation("People: {Count} {People}", people.Count, people.Select(x => x.FirstName + " " + x.LastName));

        var allPeople = people;
        logger.LogInformation("People: {Count} {People}", people.Count, allPeople);
    }

    public void Overloads(ILogger logger, int arg, int anotherArg)
    {
        logger.LogInformation(new EventId(42), "Arg: {Arg} {AnotherArg}", arg, anotherArg);
        logger.LogInformation(new EventId(42), "Arg: {AnotherArg} {Arg}", arg, anotherArg);     // Noncompliant
                                                                                                // Secondary @-1
        logger.LogInformation(new Exception(), "Arg: {Arg} {AnotherArg}", arg, anotherArg);
        logger.LogInformation(new Exception(), "Arg: {AnotherArg} {Arg}", arg, anotherArg);     // Noncompliant
                                                                                                // Secondary @-1
    }

    public void Casing(ILogger logger, int arg, int anotherArg)
    {
        logger.LogInformation("Arg: {anotherarg} {ARG}", arg, anotherArg);      // Noncompliant
                                                                                // Secondary @-1
        logger.LogInformation("Arg: {_another_Arg_} {_Arg_}", arg, anotherArg); // Noncompliant
                                                                                // Secondary @-1
        logger.LogInformation("Arg: {ANOTHERARG} {Arg}", arg, anotherArg);      // Noncompliant
                                                                                // Secondary @-1
    }

    public void IncorrectPlaceholderFormat(ILogger logger, int arg, int anotherArg)
    {
        logger.LogInformation("Arg: {@Arg} {@AnotherArg}", anotherArg, arg);                    // Noncompliant
                                                                                                // Secondary @-1
        logger.LogInformation("Arg: {&Arg} {&AnotherArg}", anotherArg, arg);
        logger.LogInformation("Arg: {Arg,42} {AnotherArg,42}", anotherArg, arg);                // Noncompliant
                                                                                                // Secondary @-1
        logger.LogInformation("Arg: {Arg,Arg} {AnotherArg,AnotherArg}", anotherArg, arg);
    }

    public void ClassImplementsILogger(CustomLogger logger, int arg, int anotherArg)
    {
        logger.LogInformation("Arg: {Arg} {AnotherArg}", arg, anotherArg);  // Compliant
        logger.LogInformation("Arg: {Arg} {AnotherArg}", anotherArg, arg);  // Noncompliant
                                                                            // Secondary @-1
    }

    public void ClassDoesNotImplementILogger(NotILogger notILogger, int arg, int anotherArg)
    {
        notILogger.LogInformation("Arg: {Arg} {AnotherArg}", arg, anotherArg);
        notILogger.LogCritical("Arg: {Arg} {AnotherArg}", anotherArg, arg);
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
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
