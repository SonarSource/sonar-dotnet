Imports System.ComponentModel.DataAnnotations
Imports System.Text.RegularExpressions

Class Compliant

    Sub Ctor()
        Dim ctor As New Regex("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Compliant
    End Sub

    Sub Instance()
        Dim regex As New Regex("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1))

        Dim isMatch = regex.IsMatch("some input") ' Compliant
        Dim match = regex.Match("some input") ' Compliant
        Dim matches = regex.Matches("some input") ' Compliant
        Dim replace = regex.Replace("some input", "some replacement") ' Complaint
        Dim split = regex.Split("some input") ' Compliant
    End Sub

    Sub NonBacktrackingSpecified()
        Dim newonBacktrackingOnly As New Regex("some pattern", CType(1024, RegexOptions)) ' Compliant
        Dim newonAlsoBacktracking As New Regex("some pattern", CType(1025, RegexOptions)) ' Compliant
        Dim split = Regex.Split("some input", "some pattern", CType(1024, RegexOptions)) ' Compliant
    End Sub

    Sub _Static()
        Dim isMatch = Regex.IsMatch("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Compliant
        Dim match = Regex.Match("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Compliant
        Dim matches = Regex.Matches("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Compliant
        Dim replace = Regex.Replace("some input", "some pattern", "some replacement", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Complaint
        Dim split = Regex.Split("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)) ' Compliant
    End Sub

    <RegularExpression("[0-9]+")> ' Compliant, Default timeout is 2000 ms.
    Public Property AttributeWithoutTimeout As String

    <RegularExpression("[0-9]+", MatchTimeoutInMilliseconds:=200)> ' Compliant
    Public Property AttributeWithTimeout As String

End Class

Class Noncompliant

    Sub Ctor()
        Dim patternOnly As New Regex("some pattern") ' Noncompliant {{Pass a timeout to limit the execution time.}}
        '                  ^^^^^^^^^^^^^^^^^^^^^^^^^
        Dim withOptions As New Regex("some pattern", RegexOptions.None) ' Noncompliant
    End Sub

    Sub _Static(options As RegexOptions)
        Dim isMatch = Regex.IsMatch("some input", "some pattern") ' Noncompliant
        Dim match = Regex.Match("some input", "some pattern") ' Noncompliant
        Dim matches = Regex.Matches("some input", "some pattern", RegexOptions.None) ' Noncompliant
        Dim replace = Regex.Replace("some input", "some pattern", "some replacement", RegexOptions.None) ' Noncompliant
        Dim split = Regex.Split("some input", "some pattern", options) ' Noncompliant
    End Sub
End Class

Class DoesNotCrash
    Private Sub MethodWithoutIdentifier(__arglist)
        MethodWithoutIdentifier("__arglist"(""))
    End Sub
End Class
