Imports System
Imports System.Text.RegularExpressions

Module DefaultMatchTimeoutInvalid

    ' No process-wide regex timeout is configured here (the AppDomain key is unrelated), so issues are still raised.
    Sub Main()
        AppDomain.CurrentDomain.SetData("SOME_OTHER_SETTING", TimeSpan.FromMilliseconds(100))
    End Sub

    Sub RegexPattern(input As String)
        Dim ctor = New Regex(".+@.+", RegexOptions.None)    ' Noncompliant
        Dim isMatch = Regex.IsMatch(input, "[0-9]+")        ' Noncompliant
    End Sub

End Module
