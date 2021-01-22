Imports System
Imports System.Collections.Generic

Namespace Tests.TestCases

    Class IndexOfCheckAgainstZero

        Public Sub IndexOfCheckAgainstZero()
            Dim color As String = "blue"
            Dim name As String = "ishmael"

            Dim strings As New List(Of String)
            Dim stringIList As System.Collections.IList = New List(Of String) 
            strings.Add(color)
            strings.Add(name)
            Dim stringArray As String() = strings.ToArray()

            If stringIList.IndexOf(color) > 0 Then ' Noncompliant
            '                             ^^^
            '   ...
            End If

            If stringIList.IndexOf(color) > -0 Then ' Noncompliant
            '                             ^^^^
            '   ...
            End If

            If stringIList.iNdExOf(color) > 0 Then ' Noncompliant
            '                             ^^^
            '   ...
            End If

            if strings.IndexOf(color) > 0 Then ' Noncompliant {{0 is a valid index, but this check ignores it.}}
            '    ...
            End If

            if 0 < name.IndexOf("ish") Then ' Noncompliant
            '    ...
            End If

            if -1 < name.IndexOf("ish") Then
            '    ...
            End If

            if 2 < name.IndexOf("ish") Then
            '    ...
            End If

            if name.IndexOf("ae") > 0 Then ' Noncompliant
            '    ...
            End If

            if Array.IndexOf(stringArray, color) > 0 Then ' Noncompliant
            '    ...
            End If

            if Array.IndexOf(stringArray, color) >= 0 Then
            '    ...
            End If

        End Sub

    End Class

End Namespace
