Namespace Tests.Diagnostics

    Public Class SwitchWithoutDefault

        Public Sub New(n As Integer)

            Select Case n ' Noncompliant {{Add a 'Case Else' clause to this 'Select' statement.}}
'           ^^^^^^
            End Select

            Select Case n ' Noncompliant
                Case 1
                Case 2
            End Select

            Select n ' Noncompliant
                Case Is = 1
                Case Is = 2
            End Select

            Select Case n
                Case Else

            End Select

            Select Case n
                Case 1

                Case Else

            End Select

            Select Case n
                Case 1
                Case 2
                Case 3
                Case Else
            End Select
        End Sub
    End Class
End Namespace