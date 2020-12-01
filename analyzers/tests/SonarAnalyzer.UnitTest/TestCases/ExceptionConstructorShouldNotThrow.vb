Imports System

Namespace Tests.Diagnostics
    Class MyException
        Inherits Exception

        Public Sub New()
            Throw New Exception() ' Noncompliant {{Avoid throwing exceptions in this constructor.}}
'           ^^^^^^^^^^^^^^^^^^^^^
        End Sub
    End Class

    Class MyException2
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal i As Integer)
            If i = 42 Then
                Throw New Exception() ' Noncompliant
            End If
        End Sub
    End Class

    Class MyException3
        Inherits Exception

        Public Sub New(ByVal i As Integer)
            If i = 42 Then
                Throw New Exception() ' Noncompliant
            Else
                Throw New ArgumentException() ' Secondary
            End If
        End Sub
    End Class

    Class SubException
        Inherits MyException

        Public Sub New()
            Throw New FieldAccessException() ' Noncompliant
        End Sub
    End Class

    Class MyException4
        Inherits Exception

        Public Sub New()
            Try

            Catch ex As Exception
                Throw ' Noncompliant
            End Try
        End Sub
    End Class

    Class MyException5
        Inherits Exception

        Public Sub New()
            Dim ex = New Exception()
            Throw ex ' Noncompliant
        End Sub
    End Class

    Class Something
        Public Sub New()
            Throw New Exception() ' Compliant
        End Sub
    End Class
End Namespace
