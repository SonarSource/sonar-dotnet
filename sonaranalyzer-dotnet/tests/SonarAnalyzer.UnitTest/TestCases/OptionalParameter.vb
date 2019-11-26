Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class OptionalParameter

        ReadOnly Property num(Optional i As Integer = 10) As String 'Noncompliant
            Get
                Return 42
            End Get
        End Property

    End Class

    Public Class CallerMember
        Public Sub Method(<System.Runtime.CompilerServices.CallerLineNumber> Optional line As Integer = 0)

        End Sub
    End Class
End Namespace

