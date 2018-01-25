' Noncompliant {{This file has 20 lines, which is greater than 10 authorized. Split it into smaller files.}}
Namespace Tests.Diagnostics
    Class FooBar
        Sub New()
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)
            System.Collections.Generic.List(Of Integer).Enumerator.Equals(Nothing)

            Dim s As String = "a
                b
                c"
        End Sub
    End Class

End Namespace


' Some comment here
