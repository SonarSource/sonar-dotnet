using System;
using Microsoft.Extensions.Logging;

public class Program
{
    ILogger chocolate;                      // Compliant
    ILogger running;                        // Noncompliant {{Rename this field 'running' to match the regular expression '^chocolate$'.}}
}
