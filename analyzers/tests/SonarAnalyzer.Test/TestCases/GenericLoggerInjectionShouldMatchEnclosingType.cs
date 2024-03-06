using System;
using Microsoft.Extensions.Logging;

public class Correct : Base
{
    Correct(ILogger<Correct> logger) : base(logger) { }              // Compliant
    Correct(ILogger<Correct> logger, ILogger<Correct> logger2) { }   // Compliant

    Correct(Wrapper<Correct> logger) { }                             // Compliant
    Correct(Wrapper<Wrong> logger) { }                               // Compliant
    Correct(Wrapper<Correct> logger, Wrapper<Wrong> logger2) { }     // Compliant

    Correct(ILogger<string> logger) { }                              // Noncompliant {{Update this logger to use its enclosing type.}}
    //              ^^^^^^
    Correct(ILogger<Wrong> logger) { }                               // Noncompliant
    //              ^^^^^
    Correct(ILogger<Base> logger) : base(logger) { }                 // Noncompliant, need exact type match
    //              ^^^^
    Correct(ILogger<Wrapper<Correct>> logger) { }                    // Noncompliant
    //              ^^^^^^^^^^^^^^^^
    Correct(ILogger<ILogger<Correct>> logger) { }                    // Noncompliant
    //              ^^^^^^^^^^^^^^^^

    Correct(ILogger<string> logger, ILogger<Wrong> logger2) { }
    //              ^^^^^^ {{Update this logger to use its enclosing type.}}
    //                                      ^^^^^ @-1 {{Update this logger to use its enclosing type.}}

    Correct(ILogger<string> logger, ILogger<Correct> logger2, ILogger<ILogger<Correct>> logger3) { }
    //              ^^^^^^ {{Update this logger to use its enclosing type.}}
    //                                                                ^^^^^^^^^^^^^^^^ @-1 {{Update this logger to use its enclosing type.}}

    Correct(Logger<Correct> logger, Logger<Wrong> logger2) { }
    //                                     ^^^^^ {{Update this logger to use its enclosing type.}}

    Correct(Logger logger) { } // Compliant, Logger is not a generic type
}

public class Base
{
    public Base(ILogger<Base> logger) { } // Compliant
    public Base() { }
}

public class Wrong { }
public class Wrapper<T> { }

public class Logger : Logger<int> { }

public class Logger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState x) => null;
    public bool IsEnabled(LogLevel x) => false;
    void ILogger.Log<TState>(LogLevel x1, EventId x2, TState x3, Exception x4, Func<TState, Exception, string> x5) { }
}

public class DerivedGeneric<T> : Generic<T> { }

public class Generic<T>
{
    Generic(ILogger<Generic<T>> logger) { }         // Compliant

    Generic(ILogger<DerivedGeneric<T>> logger) { }  // Noncompliant, generic types do not match
    Generic(ILogger<Generic<int>> logger) { }       // Noncompliant, generic argument types do not match
    Generic(ILogger<T> logger) { }                  // Noncompliant
    Generic(ILogger<int> logger) { }                // Noncompliant

    public Generic() { }
}
