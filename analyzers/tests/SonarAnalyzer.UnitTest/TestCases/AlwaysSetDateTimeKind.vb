Imports System.Globalization
Imports MyAlias = System.DateTime

Public Class Program

    Public Sub Noncompliant()
        Dim dt = New DateTime() ' Noncompliant {{Provide the "DateTimeKind" when creating this object.}}
        '        ^^^^^^^^^^^^^^
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
        dt = New MyAlias() ' FN
        dt = New System.DateTime() ' Noncompliant
    End Sub

    Public Sub Compliant()
        Dim dt = New DateTime(1623, DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, DateTimeKind.Local)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar(), DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, DateTimeKind.Utc)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, New GregorianCalendar(), DateTimeKind.Unspecified)
        dt = New DateTime(1994, 7, 5, 16, 23, 0, 42, DateTimeKind.Unspecified)
    End Sub

End Class

Class FakeDateTime

    Private Class DateTime

    End Class

    Private Sub Compliant()
        Dim dt = New DateTime()
    End Sub

End Class
