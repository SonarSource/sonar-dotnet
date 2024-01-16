using System;

[Obsolete] // Noncompliant ^2#8 {{Add an explanation.}}
class Noncompliant
{
    [Obsolete()] // Noncompliant
    void WithBrackets() { }

    [System.Obsolete] // Noncompliant
    void FullyDeclaredNamespace() { }

    [global::System.Obsolete] // Noncompliant
    void GloballyDeclaredNamespace() { }

    [Obsolete(null)] // Noncompliant
    void WithNull() { }

    [Obsolete("")] // Noncompliant
    void WithEmptyString() { }

    [Obsolete("  ")] // Noncompliant
    void WithWhiteSpace() { }

    [Obsolete("", true)] // Noncompliant
    void WithTwoArguments() { }

    [Obsolete] // Noncompliant
    [CLSCompliant(false)]
    uint Multiple() { return 0; }

    [Obsolete, CLSCompliant(false)]
//   ^^^^^^^^
    uint Combined() { return 0; }

    [Obsolete] // Noncompliant
    enum Enum { foo, bar }

    [Obsolete] // Noncompliant
    Noncompliant() { }

    [Obsolete] // Noncompliant
    void Method() { }

    [Obsolete] // Noncompliant
    int Property { get; set; }

    [Obsolete] // Noncompliant
    int Field;

    [Obsolete] // Noncompliant
    event EventHandler Event;

    [Obsolete] // Noncompliant
    delegate void Delegate();
}

[Obsolete] // Noncompliant
interface IInterface
{
    [Obsolete] // Noncompliant
    void Method();
}


[Obsolete] // Noncompliant
struct ProgramStruct
{
    [Obsolete] // Noncompliant
    void Method() { }
}

[Obsolete("explanation")]
class Compliant
{
    [Obsolete("explanation")]
    enum Enum { foo, bar }

    [Obsolete("explanation")]
    Compliant() { }

    [Obsolete("explanation")]
    void Method() { }

    [Obsolete("explanation", true)]
    void WithTwoArguments() { }

    [Obsolete("explanation")]
    string Property { get; set; }

    [Obsolete("explanation", true)]
    int Field;

    [Obsolete("explanation", false)]
    event EventHandler Event;

    [Obsolete("explanation")]
    delegate void Delegate();
}

[Obsolete("explanation")]
interface IComplaintInterface
{
    [Obsolete("explanation")]
    void Method();
}

[Obsolete("explanation")]
struct ComplaintStruct
{
    [Obsolete("explanation")]
    void Method() { }
}

class Ignore
{
    // FP the value of error is taken.
    [Obsolete(error: true, message: "explanation")] // Noncompliant
    public void NamedParmetersDifferentOrderFP() { }

    [Obsolete(error: true, message: "")] // Noncompliant for wrong reason 
    public void NamedParmetersDifferentOrder() { }
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

    [NotSystem.Obsolete]
    void SameName() { }
}

namespace NotSystem
{
    public class ObsoleteAttribute : Attribute { }
}
