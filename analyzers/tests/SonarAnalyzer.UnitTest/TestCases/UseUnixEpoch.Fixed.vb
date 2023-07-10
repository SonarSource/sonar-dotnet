Imports System.Globalization
Imports MyAlias = System.DateTime

Public Class Program
    Private ReadOnly Epoch As Date = DateTime.UnixEpoch ' Fixed

    Private ReadOnly EpochOff As DateTimeOffset = DateTimeOffset.UnixEpoch ' Fixed

    Private Const EpochTicks As Long = 621355968000000000
    Private Const EpochTicksUnderscores As Long = 621_355_968_000_000_000
    Private Const EpochTicksBinary As Long = &B100010011111011111111111010111110111101101011000000000000000
    Private Const EpochTicksHex As Long = &H89F7FF5F7B58000
    Private Const SomeLongConst As Long = 6213

    Private Sub BasicCases(ByVal dateTime As Date)
        Dim timeSpan = dateTime - DateTime.UnixEpoch ' Fixed

        If dateTime < DateTime.UnixEpoch Then ' Fixed
            Return
        End If

        Dim compliant0 = New DateTime(1971, 1, 1) ' Compliant
        Dim compliant1 = New DateTime(1970, 2, 1) ' Compliant
        Dim compliant2 = New DateTime(1970, 1, 2) ' Compliant
        Dim compliant3 = Date.UnixEpoch ' Compliant
        Dim compliant4 = DateTimeOffset.UnixEpoch ' Compliant

        Dim year = 1970
        Dim dateTime2 = New DateTime(year, 1, 1) ' FN
    End Sub

    Private Sub EdgeCases()
        Dim dateTimeOffset = New DateTimeOffset(DateTime.UnixEpoch, New TimeSpan(0, 0, 0)) ' Fixed
        Dim dateTime = New DateTime(If(True, 1970, 1971), 1, 1) ' FN
        dateTime = DATETIME.UnixEpoch ' Fixed
        Dim dateTime2 As Date = Date.UnixEpoch ' Fixed
        Dim dateTime3 = System.DateTime.UnixEpoch ' Fixed
        Dim dateTime4 = MyAlias.UnixEpoch ' Fixed
    End Sub

    Private Sub DateTimeConstructors(ByVal ticks As Integer, ByVal year As Integer, ByVal month As Integer, ByVal day As Integer, ByVal hour As Integer, ByVal minute As Integer, ByVal second As Integer, ByVal millisecond As Integer, ByVal calendar As Calendar, ByVal kind As DateTimeKind)
        ' default date
        Dim ctor0_0 = New DateTime() ' Compliant

        ' ticks
        Dim ctor1_0 = New DateTime(1970) ' Compliant
        Dim ctor1_1 = New DateTime(ticks) ' Compliant
        Dim ctor1_2 = New DateTime(ticks:=ticks) ' Compliant
        Dim ctor1_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor1_4 = DateTime.UnixEpoch ' Fixed
        Dim ctor1_5 = DateTime.UnixEpoch ' Fixed
        Dim ctor1_6 = DateTime.UnixEpoch ' Fixed
        Dim ctor1_7 = DateTime.UnixEpoch ' Fixed
        Dim ctor1_8 = New DateTime(SomeLongConst) ' Compliant

        ' year, month, and day
        Dim ctor2_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor2_1 = New DateTime(year, month, day) ' Compliant
        Dim ctor2_2 = New DateTime(month:=month, day:=day, year:=year) ' Compliant
        Dim ctor2_3 = DateTime.UnixEpoch ' Fixed

        ' year, month, day, and calendar
        Dim ctor3_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor3_1 = New DateTime(1970, 3, 1, New GregorianCalendar()) ' Compliant
        Dim ctor3_2 = New DateTime(1970, 1, 1, New ChineseLunisolarCalendar()) ' Compliant
        Dim ctor3_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor3_4 = New DateTime(month:=1, day:=1, calendar:=New ChineseLunisolarCalendar(), year:=1970) ' Compliant

        ' year, month, day, hour, minute, and second
        Dim ctor4_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor4_1 = New DateTime(1970, 1, 1, 0, 0, 1) ' Compliant
        Dim ctor4_2 = New DateTime(year:=1970, minute:=minute, month:=1, day:=1, hour:=0, second:=0) ' Compliant
        Dim ctor4_3 = DateTime.UnixEpoch ' Fixed

        ' year, month, day, hour, minute, second, and calendar
        Dim ctor5_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor5_1 = New DateTime(1970, 1, 1, 0, 1, 0, New GregorianCalendar()) ' Compliant
        Dim ctor5_2 = New DateTime(1970, 1, 1, 0, 0, 0, New ChineseLunisolarCalendar()) ' Compliant
        Dim ctor5_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor5_4 = New DateTime(year:=1970, second:=0, minute:=0, day:=1, month:=1, hour:=0, calendar:=calendar) ' Compliant

        ' year, month, day, hour, minute, second, and DateTimeKind value
        Dim ctor6_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor6_1 = New DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc) ' Compliant
        Dim ctor6_2 = New DateTime(1970, 1, 1, hour, 0, 0, DateTimeKind.Utc) ' Compliant
        Dim ctor6_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor6_4 = New DateTime(month:=1, year:=1970, day:=1, hour:=hour, second:=0, minute:=0, kind:=DateTimeKind.Utc) ' Compliant
        Dim ctor6_5 = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) ' Compliant
        Dim ctor6_6 = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local) ' Compliant

        ' year, month, day, hour, minute, second, and millisecond
        Dim ctor7_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor7_1 = New DateTime(1970, 1, 1, 0, 0, 0, 1) ' Compliant
        Dim ctor7_3 = New DateTime(year, month, day, hour, minute, second, millisecond) ' Compliant
        Dim ctor7_4 = New DateTime(year:=1970, minute:=minute, month:=1, day:=1, hour:=0, millisecond:=0, second:=0) ' Compliant
        Dim ctor7_5 = DateTime.UnixEpoch ' Fixed

        ' year, month, day, hour, minute, second, millisecond, and calendar
        Dim ctor8_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor8_1 = New DateTime(1970, 1, 1, 0, 0, 0, 1, New GregorianCalendar()) ' Compliant
        Dim ctor8_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, New ChineseLunisolarCalendar()) ' Compliant
        Dim ctor8_3 = DateTime.UnixEpoch ' Fixed

        ' year, month, day, hour, minute, second, millisecond, and DateTimeKind value
        Dim ctor9_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor9_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) ' Compliant
        Dim ctor9_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) ' Compliant
        Dim ctor9_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor9_4 = New DateTime(year:=1970, minute:=0, month:=1, day:=1, hour:=0, millisecond:=0, second:=0, kind:=kind) ' Compliant

        ' year, month, day, hour, minute, second, millisecond, calendar and DateTimeKind value
        Dim ctor10_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor10_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, New GregorianCalendar(), DateTimeKind.Local) ' Compliant
        Dim ctor10_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, New ChineseLunisolarCalendar(), DateTimeKind.Utc) ' Compliant
        Dim ctor10_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor10_4 = New DateTime(year:=1970, minute:=0, month:=1, day:=1, hour:=0, calendar:=New GregorianCalendar(), millisecond:=0, second:=second, kind:=DateTimeKind.Utc) ' Compliant

        ' year, month, day, hour, minute, second, millisecond and microsecond
        Dim ctor11_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor11_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 1) ' Compliant
        Dim ctor11_2 = DateTime.UnixEpoch ' Fixed
        Dim ctor11_3 = New DateTime(year:=1970, microsecond:=0, minute:=minute, month:=1, hour:=0, day:=1, millisecond:=millisecond, second:=0) ' Compliant

        ' year, month, day, hour, minute, second, millisecond, microsecond and calendar
        Dim ctor12_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor12_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 1, New GregorianCalendar()) ' Compliant
        Dim ctor12_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 0, New ChineseLunisolarCalendar()) ' Compliant
        Dim ctor12_3 = DateTime.UnixEpoch ' Fixed
        Dim ctor12_4 = New DateTime(year:=1970, minute:=minute, month:=1, day:=1, hour:=0, calendar:=New GregorianCalendar(), millisecond:=0, second:=0, microsecond:=0) ' Compliant

        ' year, month, day, hour, minute, second, millisecond, microsecond and DateTimeKind value
        Dim ctor13_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor13_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 1, DateTimeKind.Utc) ' Compliant
        Dim ctor13_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Unspecified) ' Compliant
        Dim ctor13_3 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Local) ' Compliant
        Dim ctor13_4 = DateTime.UnixEpoch ' Fixed
        Dim ctor13_5 = New DateTime(year:=1970, minute:=minute, month:=1, day:=1, hour:=0, kind:=DateTimeKind.Utc, millisecond:=0, second:=0, microsecond:=0) ' Compliant

        ' year, month, day, hour, minute, second, millisecond, microsecond calendar and DateTimeKind value
        Dim ctor14_0 = DateTime.UnixEpoch ' Fixed
        Dim ctor14_1 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 1, New GregorianCalendar(), DateTimeKind.Utc) ' Compliant
        Dim ctor14_2 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 0, New GregorianCalendar(), DateTimeKind.Unspecified) ' Compliant
        Dim ctor14_3 = New DateTime(1970, 1, 1, 0, 0, 0, 0, 0, New ChineseLunisolarCalendar(), DateTimeKind.Utc) ' Compliant
        Dim ctor14_4 = DateTime.UnixEpoch ' Fixed
        Dim ctor14_5 = New DateTime(year:=1970, minute:=minute, month:=1, day:=1, hour:=0, kind:=DateTimeKind.Utc, millisecond:=0, second:=0, microsecond:=0, calendar:=calendar) ' Compliant
    End Sub

    Private Sub DateTimeOffsetConstructors(ByVal timeSpan As TimeSpan, ByVal dateTime As Date, ByVal ticks As Integer, ByVal year As Integer, ByVal month As Integer, ByVal day As Integer, ByVal hour As Integer, ByVal minute As Integer, ByVal second As Integer, ByVal millisecond As Integer, ByVal microsecond As Integer, ByVal calendar As Calendar, ByVal kind As DateTimeKind)
        ' default date
        Dim ctor0_0 = New DateTimeOffset() ' Compliant

        ' datetime
        Dim ctor1_0 = New DateTimeOffset(New DateTime()) ' Compliant
        Dim ctor1_1 = New DateTimeOffset(dateTime) ' Compliant
        Dim ctor1_2 = New DateTimeOffset(DateTime.UnixEpoch) ' Fixed

        ' datetime and timespan
        Dim ctor2_0 = New DateTimeOffset(New DateTime(), TimeSpan.Zero) ' Compliant
        Dim ctor2_1 = New DateTimeOffset(New DateTime(), timeSpan) ' Compliant
        Dim ctor2_2 = New DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero) ' Fixed

        ' year, month, day, hour, minute, second, millisecond, offset and calendar
        Dim ctor3_0 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, calendar, timeSpan) ' Compliant
        Dim ctor3_1 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor3_2 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor3_3 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, New GregorianCalendar(), New TimeSpan(1)) ' Compliant
        Dim ctor3_4 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor3_5 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor3_6 = New DateTimeOffset(hour:=0, month:=1, day:=1, year:=1970, minute:=0, second:=0, millisecond:=0, calendar:=New GregorianCalendar(), offset:=New TimeSpan(2)) ' Compliant
        Dim ctor3_7 = New DateTimeOffset(hour:=0, month:=1, day:=1, year:=1970, minute:=0, second:=0, millisecond:=0, calendar:=calendar, offset:=New TimeSpan(0)) ' Compliant

        ' year, month, day, hour, minute, second, millisecond, microsecond, offset and calendar
        Dim ctor4_0 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, calendar, timeSpan) ' Compliant
        Dim ctor4_1 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor4_2 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor4_3 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, New GregorianCalendar(), New TimeSpan(1)) ' Compliant
        Dim ctor4_4 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 1, New GregorianCalendar(), New TimeSpan(0)) ' Compliant
        Dim ctor4_5 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor4_6 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor4_7 = New DateTimeOffset(hour:=0, month:=1, day:=1, year:=1970, minute:=0, second:=0, microsecond:=0, millisecond:=0, calendar:=New GregorianCalendar(), offset:=New TimeSpan(2)) ' Compliant
        Dim ctor4_8 = New DateTimeOffset(hour:=0, month:=1, day:=1, year:=1970, minute:=0, second:=0, microsecond:=0, millisecond:=0, calendar:=calendar, offset:=New TimeSpan(0)) ' Compliant
        Dim ctor4_9 = New DateTimeOffset(hour:=0, month:=1, day:=1, year:=1970, minute:=0, second:=0, microsecond:=1, millisecond:=0, calendar:=New GregorianCalendar(), offset:=New TimeSpan(0)) ' Compliant

        ' year, month, day, hour, minute, second and offset
        Dim ctor5_0 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor5_1 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, New TimeSpan(1)) ' Compliant
        Dim ctor5_2 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, timeSpan) ' Compliant
        Dim ctor5_3 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, timeSpan) ' Compliant
        Dim ctor5_4 = New DateTimeOffset(1970, 1, 1, 0, 0, 2, New TimeSpan(0)) ' Compliant
        Dim ctor5_5 = DateTimeOffset.UnixEpoch ' Fixed

        ' year, month, day, hour, minute, second, millisecond and offset
        Dim ctor6_0 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor6_1 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor6_2 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, New TimeSpan(2, 14, 18)) ' Compliant
        Dim ctor6_3 = New DateTimeOffset(1970, 1, 1, 0, 1, 0, 0, TimeSpan.Zero) ' Compliant
        Dim ctor6_4 = DateTimeOffset.UnixEpoch ' Fixed

        ' year, month, day, hour, minute, second, millisecond and offset
        Dim ctor7_0 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor7_1 = New DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, New TimeSpan(2, 14, 18)) ' Compliant
        Dim ctor7_2 = DateTimeOffset.UnixEpoch ' Fixed
        Dim ctor7_3 = New DateTimeOffset(1970, 1, 1, 0, 1, 0, 0, 0, TimeSpan.Zero) ' Compliant
        Dim ctor7_4 = DateTimeOffset.UnixEpoch ' Fixed
    End Sub
End Class
