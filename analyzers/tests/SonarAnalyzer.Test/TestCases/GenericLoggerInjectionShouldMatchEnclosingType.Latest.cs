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
public class PrimaryConstructorCorrect(ILogger<PrimaryConstructorCorrect> logger);  // Compliant
public class PrimaryConstructorWrong(ILogger<Wrong> logger);                        // FN
