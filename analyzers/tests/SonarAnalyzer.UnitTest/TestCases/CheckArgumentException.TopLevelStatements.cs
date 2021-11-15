using System;
using System.Collections.Generic;

if (args == null)
{
    throw new ArgumentException(); // Noncompliant {{Use a constructor overloads that allows a more meaningful exception message to be provided.}}
    throw new ArgumentNullException("foo"); // Noncompliant {{The parameter name 'foo' is not declared in the argument list.}}
    throw new ArgumentException("args", "message"); // Noncompliant {{ArgumentException constructor arguments have been inverted.}}
    throw new ArgumentException("message", "args");
    throw new ArgumentNullException("args");
}

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        Console.WriteLine(arg);
    }
}

void LocalFunction(string localArg)
{
    throw new ArgumentNullException(nameof(localArg));  // Compliant
    throw new ArgumentNullException(nameof(args));      // Noncompliant
}

static void StaticLocalFunction(string localArg)
{
    throw new ArgumentNullException(nameof(localArg));  // Compliant
    throw new ArgumentNullException(nameof(args));      // Noncompliant, this method even doesn't see args value
}
