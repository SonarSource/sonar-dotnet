Imports System.Globalization

Public Class Program

    Public Sub Noncompliant()
        Dim dt = New DateTime() ' Noncompliant
        dt = New Date() ' Noncompliant
        dt = New dATEtIME() ' Noncompliant
        dt = New DateTime(1623) ' Noncompliant
        dt = New DateTime(1994, 7, 5) ' Noncompliant
        dt = New DateTime(1994, 7, 5, New GregorianCalendar()) ' Noncompliant
        dt = New DateTime(1994, 7, 5, 16, 23, 0) ' Noncompliant
        dt = New DateTime(1994, 7, 5, 16, 23, 0, New GregorianCalendar()) ' Noncompliant
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42) ' Noncompliant
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar()) ' Noncompliant
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar()) ' Noncompliant
    End Sub

    Public Sub Compiant()
        Dim dt = New DateTime(1623, DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, DateTimeKind.Local)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar(), DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, DateTimeKind.Utc)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar(), DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, DateTimeKind.Unspecified)
    End Sub

End Class
