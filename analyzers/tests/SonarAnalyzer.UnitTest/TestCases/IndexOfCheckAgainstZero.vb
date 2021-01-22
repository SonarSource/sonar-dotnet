Imports System
Imports System.Collections.Generic

Namespace Tests.TestCases

    Class IndexOfCheckAgainstZero

        Public Sub IndexOfCheckAgainstZero()
            Dim Color As String = "blue"
            Dim Name As String = "ishmael"

            Dim Strings As New List(Of String)
            Dim StringIList As System.Collections.IList = New List(Of String) 
            Strings.Add(Color)
            Strings.Add(Name)
            Dim StringArray As String() = Strings.ToArray()

            If StringIList.IndexOf(Color) > 0 Then ' Noncompliant
            '                             ^^^
            '   ...
            End If

            If StringIList.IndexOf(Color) > -0 Then ' Noncompliant
            '                             ^^^^
            '   ...
            End If

            If StringIList.iNdExOf(Color) > 0 Then ' Noncompliant
            '                             ^^^
            '   ...
            End If

            if Strings.IndexOf(Color) > 0 Then ' Noncompliant {{0 is a valid index, but this check ignores it.}}
            '    ...
            End If

            if 0 < Name.IndexOf("ish") Then ' Noncompliant
            '    ...
            End If

            if -1 < Name.IndexOf("ish") Then
            '    ...
            End If

            if 2 < Name.IndexOf("ish") Then
            '    ...
            End If

            if Name.IndexOf("ae") > 0 Then ' Noncompliant
            '    ...
            End If

            if Array.IndexOf(StringArray, Color) > 0 Then ' Noncompliant
            '    ...
            End If

            if Array.IndexOf(StringArray, Color) >= 0 Then
            '    ...
            End If

        End Sub

    End Class

End Namespace
