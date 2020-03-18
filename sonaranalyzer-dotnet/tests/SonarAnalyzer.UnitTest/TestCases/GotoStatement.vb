Imports System

Namespace Tests.Diagnostics

    Public Class GotoStatement

        Sub GotoStatement(stubid As Boolean)

            If stubid Then
                GoTo Label 'Noncompliant
            Else
                Return
            End If

Label:
            Throw New Exception()

        End Sub

    End Class

End Namespace
