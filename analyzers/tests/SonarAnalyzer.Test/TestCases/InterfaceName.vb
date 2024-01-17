Namespace Tests.Diagnostics
    Public Interface MyInterface ' Noncompliant
'                    ^^^^^^^^^^^
    End Interface
    Public Interface IMyInterface ' Compliant
    End Interface

    Public Interface IMMyInterface ' Compliant
    End Interface
    Public Interface IMMMMyInterface ' Noncompliant
    End Interface

End Namespace