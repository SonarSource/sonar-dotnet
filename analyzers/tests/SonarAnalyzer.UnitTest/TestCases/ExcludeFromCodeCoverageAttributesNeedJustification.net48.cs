using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
class Ignores
{
    [ExcludeFromCodeCoverage(Justification = "not existing")] // Error[CS0246]
    void JustifcationDoesNotExist() { }

    [ExcludeFromCodeCoverage()] // Compliant: "Justification" property was added in .Net 5
    void WithBrackets() { }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    void FullyDeclaredNamespace() { }

    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    void GloballyDeclaredNamespace() { }

    [ExcludeFromCodeCoverage]
    [CLSCompliant(false)]
    uint Multiple() { return 0; }

    [ExcludeFromCodeCoverage, CLSCompliant(false)]
    uint Combined() { return 0; }

    [ExcludeFromCodeCoverage]
    Ignores() { }

    [ExcludeFromCodeCoverage]
    void Method() { }

    [ExcludeFromCodeCoverage]
    event EventHandler Event;
}

interface IInterface
{
    [ExcludeFromCodeCoverage]
    void Method();
}

[ExcludeFromCodeCoverage]
struct ProgramStruct
{
    [ExcludeFromCodeCoverage]
    void Method() { }
}
