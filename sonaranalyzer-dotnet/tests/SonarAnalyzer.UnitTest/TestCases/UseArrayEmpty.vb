Option Strict Off

Namespace Tests.Diagnostics

    Public Sub New()
        Dim array As Integer() = New Integer(0) ' Noncompliant
        Dim dbl As Integer() = New Integer(00) ' Noncompliant
        Dim other As Integer() = {} ' // Noncompliant
        Dim dynamic = {} ' Noncompliant

        Dim array_1 As Integer = New Integer(1) ' Compliant
        Dim other_1 As Integer = {17} ' Compliant
    End Sub

End Namespace


