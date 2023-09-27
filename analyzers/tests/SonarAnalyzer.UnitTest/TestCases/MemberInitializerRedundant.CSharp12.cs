using System;
using System.Runtime.CompilerServices;

/// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7624
class SampleClass(object options)
{
    private readonly object _options = options; // Noncompliant FP
}
