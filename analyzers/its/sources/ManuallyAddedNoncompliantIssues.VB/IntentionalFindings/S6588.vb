Imports System

Friend Class S6588
    Private dateTime As Date = New DateTime(1970, 1, 1) ' Noncompliant (S6588)
    Private dateTimeOffset As DateTimeOffset = New DateTimeOffset(1970, 1, 1, 1, 1, 1, TimeSpan.Zero) ' Noncompliant
End Class
