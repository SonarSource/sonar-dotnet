﻿using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage] // Noncompliant ^2#23 {{Add a justification.}}
class Noncompliant
{
    [ExcludeFromCodeCoverage()] // Noncompliant
    void WithBrackets() { }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // Noncompliant
    void FullyDeclaredNamespace() { }

    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // Noncompliant
    void GloballyDeclaredNamespace() { }

    [ExcludeFromCodeCoverage] // Noncompliant
    [CLSCompliant(false)]
    uint Multiple() { return 0; }

    [ExcludeFromCodeCoverage, CLSCompliant(false)]
//   ^^^^^^^^^^^^^^^^^^^^^^^
    uint Combined() { return 0; }

    [ExcludeFromCodeCoverage] // Noncompliant
    Noncompliant() { }

    [ExcludeFromCodeCoverage] // Noncompliant
    void Method() { }

    [ExcludeFromCodeCoverage] // Noncompliant
    event EventHandler Event;
}

interface IInterface
{
    [ExcludeFromCodeCoverage] // Noncompliant
    void Method();
}

[ExcludeFromCodeCoverage] // Noncompliant
struct ProgramStruct
{
    [ExcludeFromCodeCoverage] // Noncompliant
    void Method() { }
}

[ExcludeFromCodeCoverage(Justification = "justification")]
class Compliant
{
    [ExcludeFromCodeCoverage(Justification = "justification")]
    Compliant() { }

    [ExcludeFromCodeCoverage(Justification = "justification")]
    void Method() { }

    [ExcludeFromCodeCoverage(Justification = "justification")]
    string Property { get; set; }

    [ExcludeFromCodeCoverage(Justification = "justification")]
    event EventHandler Event;
}

interface IComplaintInterface
{
    [ExcludeFromCodeCoverage(Justification = "justification")]
    void Method();
}

[ExcludeFromCodeCoverage(Justification = "justification")]
struct ComplaintStruct
{
    [ExcludeFromCodeCoverage(Justification = "justification")]
    void Method() { }
}

class NotApplicable
{
    [CLSCompliant(false)]
    enum Enum { foo, bar }

    NotApplicable() { }

    void Method() { }

    int Property { get; set; }

    int Field;

    event EventHandler Event;

    delegate void Delegate();

    [OtherNamespace.ExcludeFromCodeCoverage]
    void SameName() { }
}

namespace OtherNamespace
{
    public class ExcludeFromCodeCoverageAttribute : Attribute { }
}
