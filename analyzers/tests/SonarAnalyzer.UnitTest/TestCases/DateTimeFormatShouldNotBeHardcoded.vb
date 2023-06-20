Imports System
Imports System.Globalization

Public Class DateTimeFormatShouldNotBeHardcoded
    Public Sub DateTimeCases()
        Dim StringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant {{Do not hardcode the format specifier.}}
        '                                          ^^^^^^^^
        StringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")) ' Noncompliant

        StringRepresentation = DateTime.UtcNow.ToString(CultureInfo.GetCultureInfo("es-MX"))
        StringRepresentation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
        StringRepresentation = DateTime.UtcNow.ToString("d")
        StringRepresentation = DateTime.UtcNow.ToString("d", CultureInfo.GetCultureInfo("es-MX"))
    End Sub

    Public Sub DateTimeOffsetCases(ByVal DateTimeOffset As DateTimeOffset)
        Dim StringRepresentation = DateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant
        StringRepresentation = DateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")) ' Noncompliant

        StringRepresentation = DateTimeOffset.ToString(CultureInfo.GetCultureInfo("es-MX"))
        StringRepresentation = DateTimeOffset.ToString(CultureInfo.InvariantCulture)
        StringRepresentation = DateTimeOffset.ToString("d")
        StringRepresentation = DateTimeOffset.ToString("d", CultureInfo.GetCultureInfo("es-MX"))
    End Sub

    Public Sub DateOnlyCases(ByVal DateOnly As DateOnly)
        Dim StringRepresentation = DateOnly.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant
        StringRepresentation = DateOnly.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")) ' Noncompliant

        StringRepresentation = DateOnly.ToString(CultureInfo.GetCultureInfo("es-MX"))
        StringRepresentation = DateOnly.ToString(CultureInfo.InvariantCulture)
        StringRepresentation = DateOnly.ToString("d")
        StringRepresentation = DateOnly.ToString("f", CultureInfo.GetCultureInfo("es-MX"))
    End Sub

    Public Sub TimeOnlyCases(ByVal TimeOnly As TimeOnly)
        Dim StringRepresentation = TimeOnly.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant
        StringRepresentation = TimeOnly.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")) ' Noncompliant

        StringRepresentation = TimeOnly.ToString(CultureInfo.GetCultureInfo("es-MX"))
        StringRepresentation = TimeOnly.ToString(CultureInfo.InvariantCulture)
        StringRepresentation = TimeOnly.ToString("d")
        StringRepresentation = TimeOnly.ToString("d", CultureInfo.GetCultureInfo("es-MX"))
    End Sub
End Class
