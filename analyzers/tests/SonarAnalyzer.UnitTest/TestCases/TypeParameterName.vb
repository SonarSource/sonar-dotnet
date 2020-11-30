Namespace Tests.Diagnostics
    Public Class Foo(Of t, TOther) ' Noncompliant
'                       ^

        Public Sub MyMethod(Of tType, TSomeOtherTypeTT)() ' Noncompliant
'                              ^^^^^
        End Sub
    End Class
End Namespace