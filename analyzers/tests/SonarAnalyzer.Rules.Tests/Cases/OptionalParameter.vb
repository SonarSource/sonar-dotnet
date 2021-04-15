Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class OptionalParameter

        Public Sub New(Optional ByVal i As Integer = 5, 'Noncompliant
                       Optional ByVal j As Integer = 5) 'Noncompliant {{Use the overloading mechanism instead of the optional parameters.}}
'                      ^^^^^^^^
        End Sub
        Public Function F(Optional ByVal i As Integer = 5) 'Noncompliant
        End Function
        Public Sub S(Optional ByVal i As Integer = 5) 'Noncompliant
        End Sub
        Public Sub SubNoParams
        End Sub
        Public Sub New()
        End Sub

        ReadOnly Property num(Optional i As Integer = 10) As String 'Noncompliant
            Get
                Return 42
            End Get
        End Property


        Friend Function Fr(Optional ByVal i As Integer = 5) ' Compliant, friend
        End Function
    End Class

    Public Class CallerMember
        Public Sub Method(<System.Runtime.CompilerServices.CallerLineNumber> Optional line As Integer = 0)

        End Sub
    End Class
End Namespace

