Imports System
Imports System.Text.RegularExpressions
Imports RE = System.Text.RegularExpressions.Regex

Namespace Tests.Diagnostics
    Class Program
        Private Sub Main(ByVal s As String)
            Dim r As Regex
            r = New Regex("")
            r = New Regex("**")
            r = New Regex("+*")
            r = New Regex("abcdefghijklmnopqrst")
            r = New Regex("abcdefghijklmnopqrst+")
            r = New Regex("{abc}+defghijklmnopqrst") ' Noncompliant {{Make sure that using a regular expression is safe here.}}
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            r = New Regex("{abc}+{a}") ' Noncompliant
            r = New Regex("+++") ' Noncompliant
            r = New Regex("\\+\\+\\+") ' Noncompliant FP (escaped special characters)
            r = New Regex("{{{") ' Noncompliant
            r = New Regex("\\{\\{\\{") ' Noncompliant FP (escaped special characters)
            r = New Regex("***") ' Noncompliant
            r = New Regex("\\*\\*\\*") ' Noncompliant FP (escaped special characters)
            r = New Regex("(a+)+s", RegexOptions.Compiled) ' Noncompliant
            r = New Regex("(a+)+s", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            r = New Regex("{ab}*{ab}+{cd}+foo*") ' Noncompliant
            Regex.IsMatch("", "(a+)+s") ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Regex.IsMatch(s, "(a+)+s", RegexOptions.Compiled) ' Noncompliant
            Regex.IsMatch("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Match(s, "{foo}{bar}") ' Noncompliant
            Regex.Match("", "{foo}{bar}", RegexOptions.Compiled) ' Noncompliant
            Regex.Match("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Matches(s, "{foo}{bar}") ' Noncompliant
            Regex.Matches("", "{foo}{bar}", RegexOptions.Compiled) ' Noncompliant
            Regex.Matches("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Replace(s, "ab*cd*", Function(match) "") ' Noncompliant
            Regex.Replace("", "ab*cd*", "") ' Noncompliant
            Regex.Replace("", "ab*cd*", Function(match) "", RegexOptions.Compiled) ' Noncompliant
            Regex.Replace("", "ab*cd*", s, RegexOptions.Compiled) ' Noncompliant
            Regex.Replace("", "ab*cd*", Function(match) "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Replace("", "ab*cd*", "", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Regex.Split("", "a+a+") ' Noncompliant
            Regex.Split("", "a+a+", RegexOptions.Compiled) ' Noncompliant
            Regex.Split("", "a+a+", RegexOptions.Compiled, TimeSpan.Zero) ' Noncompliant
            Dim x1 = New System.Text.RegularExpressions.Regex("a+a+") ' Noncompliant
            Dim x2 = New RE("a+b+") ' Noncompliant
            System.Text.RegularExpressions.Regex.IsMatch("", "{}{}") ' Noncompliant
            RE.IsMatch("", "a**") ' Noncompliant
            RE.IsMatch("", "a\\**") ' Noncompliant FP (escaped special character)

            ' Non-static methods are compliant
            r.IsMatch("a+a+")
            r.IsMatch("{ab}*{ab}+{cd}+foo*", 0)
            r.Match("{ab}*{ab}+{cd}+foo*")
            r.Match("{ab}*{ab}+{cd}+foo*", 0)
            r.Match("{ab}*{ab}+{cd}+foo*", 0, 1)
            r.Matches("{ab}*{ab}+{cd}+foo*")
            r.Matches("{ab}*{ab}+{cd}+foo*", 0)
            r.Replace("{ab}*{ab}+{cd}+foo*", Function(match) "{ab}*{ab}+{cd}+foo*")
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*")
            r.Replace("{ab}*{ab}+{cd}+foo*", Function(match) "{ab}*{ab}+{cd}+foo*", 0)
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*", 0)
            r.Replace("{ab}*{ab}+{cd}+foo*", Function(match) "{ab}*{ab}+{cd}+foo*", 0, 0)
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*", 0, 0)
            r.Split("{ab}*{ab}+{cd}+foo*")
            r.Split("{ab}*{ab}+{cd}+foo*", 0)
            r.Split("{ab}*{ab}+{cd}+foo*", 0, 0)

            ' not hardcoded strings are compliant
            r = New Regex(s)
            r = New Regex(s, RegexOptions.Compiled, TimeSpan.Zero)
            Regex.Replace("{ab}*{ab}+{cd}+foo*", s, "{ab}*{ab}+{cd}+foo*", RegexOptions.Compiled, TimeSpan.Zero)
            Regex.Split("{ab}*{ab}+{cd}+foo*", s, RegexOptions.Compiled, TimeSpan.Zero)
        End Sub
    End Class

End Namespace
