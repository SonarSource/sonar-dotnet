Imports System
Imports System.Globalization

Public Class Program
    Private Sub Constructors()
        Dim a = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, New GregorianCalendar())                   ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                          ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, New GregorianCalendar(), DateTimeKind.Utc) ' Noncompliant
    End Sub

    Private Sub Fields([date] As Date)
        Dim a = Date.UnixEpoch ' Noncompliant
        Dim b = [date].Microsecond
        Dim c = [date].Nanosecond
    End Sub

    Private Sub Methods([date] As Date)
        [date].AddMicroseconds(0)
    End Sub
End Class
