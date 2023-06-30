Imports System
Imports System.Globalization

Public Class Program
    Private Sub Constructors()
        __ = New DateTime(1)                                                                 ' Noncompliant
        __ = New DateTime(1, 1, 1)                                                           ' Noncompliant
        __ = New DateTime(1, 1, 1, New GregorianCalendar())                                  ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1)                                                  ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, New GregorianCalendar())                         ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                                ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1)                                               ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, New GregorianCalendar())                      ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                             ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, New GregorianCalendar(), DateTimeKind.Utc)    ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, New GregorianCalendar())                   ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                          ' Noncompliant
        __ = New DateTime(1, 1, 1, 1, 1, 1, 1, 1, New GregorianCalendar(), DateTimeKind.Utc) ' Noncompliant
    End Sub

    Private Sub Fields()
        __ = Date.MaxValue  ' Noncompliant
        __ = Date.MinValue  ' Noncompliant
        __ = Date.UnixEpoch ' Noncompliant
    End Sub

    Private Sub Properties(ByVal [date] As Date)
        __ = [date].Date
        __ = [date].Day
        __ = [date].DayOfWeek
        __ = [date].DayOfYear
        __ = [date].Hour
        __ = [date].Kind
        __ = [date].Microsecond
        __ = [date].Millisecond
        __ = [date].Minute
        __ = [date].Month
        __ = [date].Nanosecond
        __ = [date].Second
        __ = [date].Ticks
        __ = [date].TimeOfDay
        __ = [date].Year
    End Sub

    Private Sub StaticProperties()
        __ = Date.Now ' Noncompliant
        __ = Date.Today ' Noncompliant
        __ = Date.UtcNow ' Noncompliant
    End Sub

    Private Sub Methods(ByVal [date] As Date, ByVal span As Span(Of Char))
        [date].Add(TimeSpan.Zero)
        [date].AddDays(0)
        [date].AddHours(0)
        [date].AddMicroseconds(0)
        [date].AddMilliseconds(0)
        [date].AddMinutes(0)
        [date].AddMonths(0)
        [date].AddSeconds(0)
        [date].AddTicks(0)
        [date].AddYears(0)
        [date].CompareTo([date])
        Date.Compare([date], [date])
        Date.DaysInMonth(1, 1)
        Date.Equals([date], [date])
        [date].Equals([date])
        Date.FromBinary(1)
        Date.FromFileTime(1)
        Date.FromFileTimeUtc(1)
        Date.FromOADate(1)
        [date].GetDateTimeFormats("a"c)
        [date].GetHashCode()
        [date].GetTypeCode()
        [date].IsDaylightSavingTime()
        Date.IsLeapYear(1)
        Date.Parse("06/01/1993")                          ' Noncompliant
        Date.ParseExact("06/01/1993", "dd/MM/yyyy", Nothing) ' Noncompliant
        Date.SpecifyKind([date], DateTimeKind.Local)        ' Noncompliant
        [date].Subtract([date])
        [date].ToBinary()
        [date].ToFileTime()
        [date].ToFileTimeUtc()
        [date].ToLocalTime()
        [date].ToLongDateString()
        [date].ToLongTimeString()
        [date].ToShortDateString()
        [date].ToShortTimeString()
        [date].ToString()
        [date].ToUniversalTime()
        Dim myInt As Integer = Nothing
        [date].TryFormat(span, myInt)
        Date.TryParse("06/01/1993", [date])                                                                                    ' Noncompliant
        Date.TryParseExact("06/01/1993", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, [date]) ' Noncompliant
    End Sub
End Class
