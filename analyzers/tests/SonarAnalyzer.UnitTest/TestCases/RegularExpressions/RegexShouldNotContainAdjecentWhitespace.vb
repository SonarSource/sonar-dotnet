Imports System.ComponentModel.DataAnnotations
Imports System.Text.RegularExpressions

Class Compliant
    Private Sub Ctor()
        Dim defaultOrder = New Regex("some pattern", RegexOptions.None)
        Dim namedArgs = New Regex(options:=RegexOptions.None, pattern:="some pattern")
    End Sub

    Private Sub [Static]()
        Dim isMatch = Regex.IsMatch("some input", "some pattern", RegexOptions.None)
    End Sub

    <RegularExpression("[0-9]+")>
    Public Property Attribute As String
End Class

Class Noncompliant
    Private Sub Ctor()
        Dim patternOnly = New Regex("A  B") ' Noncompliant {{The pattern contains adjacent whitespace.}}
        '                           ^^^^^^
    End Sub

    Private Sub [Static]()
        Dim isMatch = Regex.IsMatch("some input", "A  B") ' Noncompliant
        Dim match = Regex.Match("some input", "A  B") ' Noncompliant
        Dim matches = Regex.Matches("some input", "A  B") ' Noncompliant
        Dim replace = Regex.Replace("some input", "A  B", "some replacement") ' Noncompliant
        Dim split = Regex.Split("some input", "A  B") ' Noncompliant
    End Sub

    <RegularExpression("A  B")> ' Noncompliant
    Public Property Attribute As String
End Class
