Option Strict Off

Namespace Tests.Diagnostics

    Public Class UseArrayEmpty

        Public Const MinOne As Integer = -1

        Public Sub Arrays()

            Dim eplicit(-1) As Integer ' Noncompliant {{Declare this empty array using Array.Empty(Of T).}}
            '   ^^^^^^^^^^^^^^^^^^^^^^

            Dim implicit1 As Integer() = New Integer() {} ' Noncompliant
            Dim implicit2 As Integer() = {} ' // Noncompliant
            Dim implicit3() As Integer = {} ' Noncompliant
            Dim cnst As Integer(MinOne) ' Nonecompliant

            Dim dynamic = {} ' Noncompliant

            Dim array_1(0) As Integer ' Compliant
            Dim other_1() As Integer = {1} ' Compliant

            Arguments({42}) ' Compliant
            Arguments({}) ' Noncompliant

        End Sub

        Public Sub Arguments(arguments As Integer())
        End Sub

    End Class

End Namespace
