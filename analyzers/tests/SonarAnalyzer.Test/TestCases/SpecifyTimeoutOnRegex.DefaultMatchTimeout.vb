Imports System
Imports System.Text.RegularExpressions

Module DefaultMatchTimeout

    Sub Main()
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100))
    End Sub

    Sub RegexPattern(input As String)
        Dim ctor = New Regex(".+@.+", RegexOptions.None)    ' Compliant - REGEX_DEFAULT_MATCH_TIMEOUT is set process-wide (NET-1626)
        Dim isMatch = Regex.IsMatch(input, "[0-9]+")        ' Compliant - REGEX_DEFAULT_MATCH_TIMEOUT is set process-wide (NET-1626)
    End Sub

End Module
