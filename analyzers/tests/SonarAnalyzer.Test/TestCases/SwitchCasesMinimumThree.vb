Namespace Tests.Diagnostics
    Public Class SwitchCasesMinimumThree
        Public Sub New(ByVal n As Integer)
            Select Case n ' Noncompliant {{Replace this 'Select' statement with 'If' statements to increase readability.}}
'           ^^^^^^
                Case 0
                Case Else
            End Select

            Select Case n ' Noncompliant {{Replace this 'Select' statement with 'If' statements to increase readability.}}
'           ^^^^^^
            End Select

            Select Case n 'Compliant, 3 cases
                Case 0, 1
                Case Else
                    Dim x = 5
            End Select
        End Sub
    End Class
End Namespace

