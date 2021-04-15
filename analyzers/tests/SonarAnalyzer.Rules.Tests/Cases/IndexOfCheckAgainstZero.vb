Imports System
Imports System.Linq
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
            Dim Chars = New Char() { "i"c, "l"c }

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

            If Strings.IndexOf(Color) > 0 Then ' Noncompliant {{0 is a valid index, but this check ignores it.}}
            '    ...
            End If

            If 0 < Name.IndexOf("ish") Then ' Noncompliant
            '    ...
            End If

            If -1 < Name.IndexOf("ish") Then
            '    ...
            End If

            If 2 < Name.IndexOf("ish") Then
            '    ...
            End If

            If Name.IndexOf("ae") > 0 Then ' Noncompliant
            '    ...
            End If

            If Array.IndexOf(StringArray, Color) > 0 Then ' Noncompliant
            '    ...
            End If

            If Array.IndexOf(StringArray, Color) >= 0 Then
            '    ...
            End If

            If 0  > 0 Then
            '   ...
            End If

            If Strings.Count() > 0 Then
            '   ...
            End If

            If Name.IndexOfAny(Chars) > 0 Then ' Noncompliant
            '   ...
            End If

            If 0 < Name.IndexOfAny(Chars) Then ' Noncompliant
            '   ...
            End If

            If Name.LastIndexOf("a"c) > 0 Then ' Noncompliant
            '   ...
            End If

            If 0 < Name.LastIndexOf("a"c) Then ' Noncompliant
            '   ...
            End If

            If Name.LastIndexOfAny(Chars) > 0 Then ' Noncompliant
            '   ...
            End If

            If 0 < Name.LastIndexOfAny(Chars) Then ' Noncompliant
            '   ...
            End If
        End Sub

    End Class

End Namespace
