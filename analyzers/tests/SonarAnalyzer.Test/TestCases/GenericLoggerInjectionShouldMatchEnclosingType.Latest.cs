using System;
using Microsoft.Extensions.Logging;

public partial class Correct
{
    partial Correct(ILogger<Correct> logger);
    partial Correct(ILogger<Wrong> logger);
}

public partial class Correct
{
    partial Correct(ILogger<Correct> logger) { }    // Compliant
    partial Correct(ILogger<Wrong> logger) { }      // Noncompliant
}

public class Wrong { }

// Repro NET-2999
public class PrimaryConstructorCorrectClass(ILogger<PrimaryConstructorCorrectClass> logger);    // Compliant
public class PrimaryConstructorWrongClass(ILogger<Wrong> logger);                               // Noncompliant

public record PrimaryConstructorCorrectRecord(ILogger<PrimaryConstructorCorrectRecord> logger);  // Compliant
public record PrimaryConstructorWrongRecord(ILogger<Wrong> logger);                              // Noncompliant

public class PrimaryConstructorCorrectGeneric<T>(ILogger<PrimaryConstructorCorrectGeneric<T>> logger);    // Compliant
public class PrimaryConstructorWrongGeneric<T>(ILogger<Wrong> logger);                                   // Noncompliant

public class PrimaryConstructorCorrectCustomLogger(Logger<PrimaryConstructorCorrectCustomLogger> logger);    // Compliant
public class PrimaryConstructorWrongCustomLogger(Logger<Wrong> logger);                                      // Noncompliant

public class Logger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState x) => null;
    public bool IsEnabled(LogLevel x) => false;
    void ILogger.Log<TState>(LogLevel x1, EventId x2, TState x3, Exception x4, Func<TState, Exception, string> x5) { }
}

// Structs and record structs are intentionally not validated: per Microsoft's Framework Design Guidelines,
// structs should be small, immutable, and have a valid default state - none of which holds when a struct
// captures an ILogger<T> reference. Logger injection into value types is itself a design issue.
// See: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
// Related rule: https://musical-adventure-r9qk65j.pages.github.io/rspec/#/rspec/S3898
public struct IgnoreStruct
{
    public IgnoreStruct(ILogger<Wrong> logger) { } // Compliant
}
public record struct IgnoreRecordStruct
{
    public IgnoreRecordStruct(ILogger<Wrong> logger) { } // Compliant
}

public struct IgnoreStructPrimaryConstructor(ILogger<Wrong> logger);                  // Compliant
public record struct IgnoreRecordStructPrimaryConstructor(ILogger<Wrong> logger);     // Compliant
