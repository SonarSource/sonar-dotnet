Namespace Tests.Diagnostics
    Public Class TooManyLabelsInSwitch
        Public Enum MyEnum
            A
            B
            C
            D
        End Enum

        Public Sub New(ByVal n As Integer, ByVal en As MyEnum)
            Select Case n
                Case 0
                Case Else
            End Select

            Select Case n
                Case 0, 1, 2, 3
                Case Else
            End Select

            Select Case en
                Case MyEnum.A
                Case MyEnum.B
                Case MyEnum.C
                Case MyEnum.D
                Case Else
            End Select

            Select Case n ' Compliant
                Case 0, 1
                Case 2
                Case Else
            End Select
        End Sub

        Public Function SwitchCase(ch As Char, value As Integer) As Integer
            Select Case ch ' Noncompliant {{Consider reworking this 'Select Case' to reduce the number of 'Case' clause to at most 2 or have only one statement per 'Case'.}}
'           ^^^^^^
                Case "a"c
                    Return 1
                Case "b"c
                    Return 2
                Case "c"c
                    Return 3
                Case "-"c
                    If value > 10 Then
                        Return 42
                    ElseIf value < 5 AndAlso value > 1 Then
                        Return 21
                    End If
                    Return 99
                Case Else
                    Return 1000
            End Select
        End Function

        Public Function SwitchCaseFallThrough(ch As Char, value As Integer) As Integer
            Select Case ch
                Case "a"c, "b"c, "c"c, "-"c
                    If value > 10 Then
                        Return 42
                    ElseIf value < 5 AndAlso value > 1 Then
                        Return 21
                    End If
                    Return 99
                Case Else
                    Return 1000
            End Select
        End Function

    End Class
End Namespace
