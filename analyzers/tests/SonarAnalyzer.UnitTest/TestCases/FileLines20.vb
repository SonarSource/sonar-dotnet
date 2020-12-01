' Noncompliant {{This file has 20 lines, which is greater than 10 authorized. Split it into smaller files.}}
Namespace Tests.Diagnostics
    Class FooBar
        Sub New()
            Dim x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)
            x = New System.Collections.Generic.List(Of Integer)().Equals(Nothing)

            Dim s As String = "a
                b
                c"
        End Sub
    End Class

End Namespace


' Some comment here
