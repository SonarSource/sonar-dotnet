' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

Imports TimeZoneConverter

Namespace IntentionalFindings
    Public Class S6575
        Public Sub ConvertTimeZone()
            TZConvert.IanaToWindows("Asia/Tokyo") ' Noncompliant (S6575)
        End Sub
    End Class
End Namespace
