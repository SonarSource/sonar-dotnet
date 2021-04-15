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
                Case 0, 1
                Case Else
            End Select

            Select Case en
                Case MyEnum.A
                Case MyEnum.B
                Case MyEnum.C
                Case MyEnum.D
                Case Else
            End Select

            Select Case n ' Noncompliant {{Consider reworking this 'Select Case' to reduce the number of 'Case's from 3 to at most 2.}}
'           ^^^^^^
                Case 0, 1
                Case 2
                Case Else
            End Select
        End Sub
    End Class
End Namespace
