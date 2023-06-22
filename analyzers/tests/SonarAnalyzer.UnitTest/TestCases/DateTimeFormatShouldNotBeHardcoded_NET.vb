Imports System
Imports System.Globalization

Public Class DateTimeFormatShouldNotBeHardcoded
    Public Sub Noncompliant(ByVal dateOnly As DateOnly, ByVal timeOnly As TimeOnly)
        Dim stringRepresentation = dateOnly.ToString("dd/MM/yyyy") ' Noncompliant
        stringRepresentation = timeOnly.ToString("HH:mm:ss") ' Noncompliant
    End Sub

    Public Sub Compliant(ByVal dateOnly As DateOnly, ByVal timeOnly As TimeOnly)
        Dim stringRepresentation = dateOnly.ToString(CultureInfo.GetCultureInfo("es-MX"))
        stringRepresentation = timeOnly.ToString(CultureInfo.GetCultureInfo("es-MX"))
    End Sub
End Class
