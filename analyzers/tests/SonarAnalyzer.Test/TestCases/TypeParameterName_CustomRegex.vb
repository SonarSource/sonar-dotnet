Namespace Tests.Diagnostics
    Public Class Foo(Of AB12, CD34) ' Compliant
        Public Sub Noncompliant(Of A12B)() ' Noncompliant {{Rename 'A12B' to match the regular expression: '^[A-Z]{2}\d{2}$'.}}
'                                  ^^^^
        End Sub
    End Class
End Namespace
