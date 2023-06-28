Imports System
Imports System.Globalization

Public Class DateTimeFormatShouldNotBeHardcoded
    Private Format As String = $"dd/MM"

    Public Sub Noncompliant(ByVal dateTimeOffset As DateTimeOffset, ByVal timeSpan As TimeSpan)
        Dim stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant {{Do not hardcode the format specifier.}}
        '                                                   ^^^^^^^^^^^^^^^^^^^^^
        stringRepresentation = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("es-MX")) ' Noncompliant
        stringRepresentation = DateTime.UtcNow.ToString(Format) ' FN
        Dim stringFormat = String.Format("{0:yy/MM/dd}", DateTime.Now) ' FN
        Console.WriteLine("{0:HH:mm}", DateTime.Now) ' FN
        stringRepresentation = dateTimeOffset.ToString("dd/MM/yyyy HH:mm:ss") ' Noncompliant
        stringRepresentation = timeSpan.ToString("dd\.hh\:mm\:ss") ' Noncompliant
    End Sub

    Public Sub Compliant(ByVal dateTimeOffset As DateTimeOffset, ByVal timeSpan As TimeSpan)
        Dim stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.GetCultureInfo("es-MX"))
        stringRepresentation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
        stringRepresentation = DateTime.UtcNow.ToString("d")
        stringRepresentation = DateTime.UtcNow.ToString("d", CultureInfo.GetCultureInfo("es-MX"))
        stringRepresentation = dateTimeOffset.ToString(CultureInfo.GetCultureInfo("es-MX"))
        stringRepresentation = timeSpan.ToString("d")
        stringRepresentation = dATEtIME.Now.tOsTRING
        stringRepresentation = dateTimeOffset.ToString()

        MyDate.ToString("dd/MM/yyy")
    End Sub

    Class MyDate
        Shared Function ToString(ByVal str As String) As String
            Return str
        End Function
    End Class
End Class
