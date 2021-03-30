Imports System

Public Class DateTimeAsProvider

    Public Sub Noncompliant()
        Dim now As Date = DateTime.Now ' Noncompliant {{Use a testable (Date) time provider instead.}}
        '                     ^^^^^^^^^^^^
        Dim utc As Date = DateTime.UtcNow ' Noncompliant
        Dim today As Date = DateTime.Today ' Noncompliant
    End Sub

    Public Sub CompliantAre()
        Dim other As Integer = DateTime.DaysInMonth(2000, 2) ' Compliant
    End Sub
End Class
