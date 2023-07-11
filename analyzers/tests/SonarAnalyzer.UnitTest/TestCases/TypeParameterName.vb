Namespace Tests.Diagnostics
    Public Class Foo(Of t, TOther) ' Noncompliant {{Rename 't' to match the regular expression: '^T(([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?)?'.}}
'                       ^

        Public Sub Noncompliant(Of tType, TSomeOtherTypeTT)() ' Noncompliant
'                                  ^^^^^
        End Sub

        Public Sub Compliant(Of TType, TSomeOtherTypeTT)() ' Compliant
        End Sub
    End Class
End Namespace
