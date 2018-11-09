Imports System
Imports System.Text.RegularExpressions
Imports RE = System.Text.RegularExpressions.Regex

Namespace Tests.Diagnostics
    Class Program
        Private Sub Main()
            Dim r As Regex
            r = New Regex("") ' Noncompliant {{Make sure that using a regular expression is safe here.}}
'               ^^^^^^^^^^^^^
            r = New Regex("", RegexOptions.Compiled) ' Noncompliant
            r = New Regex("", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            Regex.IsMatch("", "") ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^
            Regex.IsMatch("", "", RegexOptions.Compiled) ' Noncompliant
            Regex.IsMatch("", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            Regex.Match("", "") ' Noncompliant
            Regex.Match("", "", RegexOptions.Compiled) ' Noncompliant
            Regex.Match("", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            Regex.Matches("", "") ' Noncompliant
            Regex.Matches("", "", RegexOptions.Compiled) ' Noncompliant
            Regex.Matches("", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            Regex.Replace("", "", Function(match) "") ' Noncompliant
            Regex.Replace("", "", "") ' Noncompliant
            Regex.Replace("", "", Function(match) "", RegexOptions.Compiled) ' Noncompliant
            Regex.Replace("", "", "", RegexOptions.Compiled) ' Noncompliant
            Regex.Replace("", "", Function(match) "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Replace("", "", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            Regex.Split("", "") ' Noncompliant
            Regex.Split("", "", RegexOptions.Compiled) ' Noncompliant
            Regex.Split("", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant

            r = New System.Text.RegularExpressions.Regex("") ' Noncompliant
            r = New RE("") ' Noncompliant
            System.Text.RegularExpressions.Regex.IsMatch("", "") ' Noncompliant
            RE.IsMatch("", "") ' Noncompliant

            ' Non-static methods are compliant
            r.IsMatch("")
            r.IsMatch("", 0)
            r.Match("")
            r.Match("", 0)
            r.Match("", 0, 1)
            r.Matches("")
            r.Matches("", 0)
            r.Replace("", Function(match) "")
            r.Replace("", "")
            r.Replace("", Function(match) "", 0)
            r.Replace("", "", 0)
            r.Replace("", Function(match) "", 0, 0)
            r.Replace("", "", 0, 0)
            r.Split("")
            r.Split("", 0)
            r.Split("", 0, 0)
        End Sub
    End Class
End Namespace
