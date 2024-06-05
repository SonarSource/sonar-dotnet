Module Module1

    Sub GetSomething(ByVal ID As Integer) ' Noncompliant
        '                  ^^
    End Sub

    Sub GetSomething2(ByVal PrefixSomething As Integer, Name As String) ' Noncompliant
        '                                               ^^^^
    End Sub

    ' Wrong casing
    Public Sub A(PrefixSomething As String) : End Sub   ' Compliant
    Public Sub B(PrefixAnything As String) : End Sub    ' Compliant
    Public Sub C(prefixSomething As String) : End Sub   ' Noncompliant
    Public Sub D(PREFIXSomething As String) : End Sub   ' Noncompliant
    Public Sub E(Prefixsomething As String) : End Sub   ' Noncompliant

End Module

