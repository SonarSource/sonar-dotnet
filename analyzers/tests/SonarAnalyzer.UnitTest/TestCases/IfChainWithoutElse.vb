Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
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
            ElseIf a Then ' Noncompliant {{Add the missing 'Else' clause with either the appropriate action or a suitable comment as to why no action is taken.}}
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
            ElseIf a Then ' Noncompliant
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
            ElseIf a Then ' Noncompliant
            Else
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            Else
                Console.WriteLine()
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            Else
                ' Single line comment
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then
            Else ' Single line comment
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then ' Noncompliant
            Else
            End If ' Single line comment

            If a Then
            ElseIf a Then
            ElseIf a Then
            Else
#If DEBUG
                Trace.WriteLine("Something to log only in debug", a.ToString())
#End If
            End If

            If a Then
            ElseIf a Then
            ElseIf a Then ' Noncompliant
            Else
#If DEBUG
#End If
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
