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
