Imports TimeZoneConverter

Namespace IntentionalFindings
    Public Class S6575
        Public Sub ConvertTimeZone()
            TZConvert.IanaToWindows("Asia/Tokyo") ' Noncompliant (S6575)
        End Sub
    End Class
End Namespace
