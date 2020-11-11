Option Strict Off

Namespace Tests.Diagnostics

    Public Class UseArrayEmpty

        Public Sub Arrays()

            Dim eplicit(-1) As Integer ' Noncompliant {{Declare this empty array using Array.Empty(Of T).}}
            '   ^^^^^^^^^^^^^^^^^^^^^^

            Dim implicit1 As Integer() = New Integer() {} ' Noncompliant
            Dim implicit2 As Integer() = {} ' // Noncompliant
            Dim implicit3() As Integer = {} ' Noncompliant


            Dim dynamic = {} ' Noncompliant

            Dim array_1(0) As Integer ' Compliant
            Dim other_1() As Integer = {1} ' Compliant

        End Sub

    End Class

End Namespace
