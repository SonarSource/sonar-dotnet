[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "")] // Noncompliant

#pragma warning disable XXX // Noncompliant

var topLevel = true;

#pragma warning restore XXX // Compliant

void Method()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("", "")] // Noncompliant
    void LocalFunction()
    {

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("", "")] // Noncompliant
    static void StaticLocalFunction()
    {

    }
}

