Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Public Class IfChainWithoutElse
        Public Sub New(a As Boolean)
            If a Then
            End If

            If a Then
            Else
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then ' Noncompliant {{Add the missing 'Else' clause.}}
'           ^^^^^^
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then ' Noncompliant
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            Else
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            ElseIf a Then
            Else
            End If

            If a Then
            Else
                If a Then
                    If a Then
                    ElseIf a Then ' Noncompliant
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace
