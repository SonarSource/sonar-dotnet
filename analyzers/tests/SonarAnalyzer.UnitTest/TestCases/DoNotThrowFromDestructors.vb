Imports System

Namespace Tests.Diagnostics
    Class Program

        Public Sub Foo()
            Throw New Exception()
        End Sub

        Public Sub FooNoParens
            Throw New Exception()
        End Sub

        Public Sub New()
            Throw New Exception()
        End Sub

        Protected Overrides Overloads Sub fiNAliZE()
            Throw New Exception() ' Noncompliant {{Remove this 'Throw' statement.}}
'           ^^^^^^^^^^^^^^^^^^^^^
            Try
                Throw New Exception() ' Noncompliant
            Catch __unusedException1__ As Exception
            End Try

            Try
                Foo()
            Catch __unusedException1__ As Exception
                Throw ' Noncompliant
            End Try
        End Sub
    end class

    Class NoViolation1
        Protected Sub Finalize(i As Integer)
            Throw New Exception()
        End Sub
    End Class
    Class NoViolation2
        Sub Finalize()
            Throw New Exception()
        End Sub
    End Class
    Class NoViolation3
        Protected Sub Finalize(Of T)()
            Throw New Exception()
        End Sub
    End Class
    Class NoViolation4
        Protected Function Finalize() As Integer
            Throw New Exception()
        End Function
    End Class
End Namespace

