Imports TimeZoneConverter

Public Class Program
    Public Sub Noncompliant()
        TZConvert.IanaToWindows("Asia/Tokyo") ' Noncompliant {{Use "TimeZoneInfo.FindSystemTimeZoneById" instead of "TZConvert.IanaToWindows"}}
        '         ^^^^^^^^^^^^^
        TZConvert.WindowsToIana("Asia/Tokyo") ' Noncompliant
        Dim resolvedTimeZone As String
        TZConvert.TryIanaToWindows("Asia/Tokyo", resolvedTimeZone) ' Noncompliant
        TZConvert.TryWindowsToIana("Asia/Tokyo", resolvedTimeZone) ' Noncompliant
    End Sub

    Public Sub Compliant()
        TZConvert.IanaToRails("Asia/Tokyo")
        Dim resolvedTimeZone As String
        TZConvert.TryRailsToIana("Asia/Tokyo", resolvedTimeZone)
    End Sub
End Class
