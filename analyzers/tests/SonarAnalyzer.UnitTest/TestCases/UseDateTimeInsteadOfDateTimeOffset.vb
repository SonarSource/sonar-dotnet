Imports System
Imports System.Globalization
Imports MyAlias = System.DateTime

Public Class Program
    Private Sub Constructors()
        Dim a = New DateTime(1)                                                             ' Noncompliant {{Prefer using "DateTimeOffset" instead of "DateTime"}}
'               ^^^^^^^^^^^^^^^
        a = New DateTime(1, 1, 1)                                                           ' Noncompliant
        a = New DateTime(1, 1, 1, New GregorianCalendar())                                  ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1)                                                  ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, New GregorianCalendar())                         ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                                ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1)                                               ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1, New GregorianCalendar())                      ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc)                             ' Noncompliant
        a = New DateTime(1, 1, 1, 1, 1, 1, 1, New GregorianCalendar(), DateTimeKind.Utc)    ' Noncompliant
        a = New Date(1, 1, 1, 1, 1, 1, 1, New GregorianCalendar(), DateTimeKind.Utc)        ' Noncompliant
        a = New DateTime(1) ' Noncompliant
        a = New MyAlias(1) ' FN
        a = New System.DateTime(1) ' Noncompliant
    End Sub

    Private Sub Fields()
        Dim a = Date.MaxValue  ' Noncompliant
'               ^^^^
        a = Date.MinValue  ' Noncompliant
        a = DateTime.MinValue ' Noncompliant
    End Sub

    Private Sub Properties([date] As Date)
        Dim a = [date].Date
        Dim b = [date].Day
        Dim c = [date].DayOfWeek
        Dim d = [date].DayOfYear
        Dim e = [date].Hour
        Dim f = [date].Kind
        Dim g = [date].Millisecond
        Dim h = [date].Minute
        Dim i = [date].Month
        Dim l = [date].Second
        Dim m = [date].Ticks
        Dim n = [date].TimeOfDay
        Dim o = [date].Year
    End Sub

    Private Sub StaticProperties()
        Dim a = Date.Now ' Noncompliant
'               ^^^^
        a = Date.Today ' Noncompliant
        a = Date.UtcNow ' Noncompliant
    End Sub

    Private Sub Methods([date] As Date)
        [date].Add(TimeSpan.Zero)
        [date].AddDays(0)
        [date].AddHours(0)
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
        Date.Parse("06/01/1993")
        Date.ParseExact("06/01/1993", "dd/MM/yyyy", Nothing)
        Date.SpecifyKind([date], DateTimeKind.Local)
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
        Date.TryParse("06/01/1993", [date])
        Date.TryParseExact("06/01/1993", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, [date])
    End Sub
End Class
