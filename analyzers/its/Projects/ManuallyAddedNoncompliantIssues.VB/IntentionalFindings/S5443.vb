﻿' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Imports System.IO

Public Class S5443

    Public Sub NonCompliant(PartOfPath As String)
        Dim Tmp As String = Path.GetTempPath() ' Noncompliant (S5443)
        Tmp = Path.GetTempPath() ' Noncompliant
        Tmp = Environment.GetEnvironmentVariable("TMPDIR") ' Noncompliant
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Tmp = Environment.GetEnvironmentVariable("TMP") ' Noncompliant
        Tmp = Environment.GetEnvironmentVariable("TEMP") ' Noncompliant
        Tmp = Environment.ExpandEnvironmentVariables("%TMPDIR%") ' Noncompliant
        Tmp = Environment.ExpandEnvironmentVariables("%TMP%") ' Noncompliant
        Tmp = Environment.ExpandEnvironmentVariables("%TEMP%") ' Noncompliant
        Tmp = "%USERPROFILE%\AppData\Local\Temp\f" ' Noncompliant
        Tmp = "%TEMP%\f" ' Noncompliant
        Tmp = "%TMP%\f" ' Noncompliant
        Tmp = "%TMPDIR%\f" ' Noncompliant
        '     ^^^^^^^^^^^^
        Tmp = $"/tmp/{PartOfPath}" ' Noncompliant
        '     ^^^^^^^^^^^^^^^^^^^^
    End Sub

End Class
