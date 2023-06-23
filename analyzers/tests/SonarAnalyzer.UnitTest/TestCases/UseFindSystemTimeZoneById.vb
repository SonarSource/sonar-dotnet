Imports TimeZoneConverter

Public Class Program
    Public Sub Compliant()
        TZConvert.IanaToWindows("Asia/Tokyo")
        TZConvert.WindowsToIana("Asia/Tokyo")
        Dim resolvedTimeZone As String
        TZConvert.TryIanaToWindows("Asia/Tokyo", resolvedTimeZone)
        TZConvert.TryWindowsToIana("Asia/Tokyo", resolvedTimeZone)

        TZConvert.IanaToRails("Asia/Tokyo")
        TZConvert.TryRailsToIana("Asia/Tokyo", resolvedTimeZone)
    End Sub
End Class
