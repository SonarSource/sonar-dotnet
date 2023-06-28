Imports System.Globalization

Public Class Program
    Private ReadOnly Epoch As Date = New DateTime(1970, 1, 1) ' Compliant

    Private ReadOnly EpochOff As DateTimeOffset = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero) ' Compliant
End Class
