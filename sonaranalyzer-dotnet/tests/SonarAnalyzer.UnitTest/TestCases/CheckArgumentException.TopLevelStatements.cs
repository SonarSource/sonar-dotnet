using System;
using System.Collections.Generic;

if (args == null)
{
    throw new ArgumentException(); // Noncompliant {{Use a constructor overloads that allows a more meaningful exception message to be provided.}}
    throw new ArgumentNullException("foo"); // Noncompliant {{The parameter name 'foo' is not declared in the argument list.}}
    throw new ArgumentException("args", "message"); // Noncompliant {{The parameter name 'message' is not declared in the argument list.}}
    throw new ArgumentException("message", "args"); // Noncompliant FP
    throw new ArgumentNullException("args"); // Noncompliant FP
}

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        Console.WriteLine(arg);
    }
}
