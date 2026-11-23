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

#pragma warning disable XXX // Noncompliant, leading trivia of record
record R(int X);

#pragma warning disable XXX // Noncompliant, end of file is visited by both the global namespace and the generated Main
