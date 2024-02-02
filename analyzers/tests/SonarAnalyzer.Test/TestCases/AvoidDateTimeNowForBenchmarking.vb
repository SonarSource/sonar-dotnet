Imports System

Public Class Program
    Private Sub Benchmark()
        Dim start = Date.Now
        ' Some method
        Console.WriteLine($"{(Date.Now - start).TotalMilliseconds} ms") ' Noncompliant {{Avoid using "DateTime.Now" for benchmarking or timespan calculation operations.}}
        '                     ^^^^^^^^^^^^^^^^

        start = Date.Now
        ' Some method
        Console.WriteLine($"{Date.Now.Subtract(start).TotalMilliseconds} ms") ' Noncompliant
        '                    ^^^^^^^^^^^^^^^^^
    End Sub

    Private Const MinRefreshInterval As Integer = 100

    Private Sub Timing()
        Dim lastRefresh = Date.Now
        If (Date.Now - lastRefresh).TotalMilliseconds > MinRefreshInterval Then ' Noncompliant
            lastRefresh = Date.Now
            ' Refresh
        End If
    End Sub

    Private Sub Combinations(ByVal timeSpan As TimeSpan, ByVal dateTime As Date)
        Dim a1 = (Date.Now - dateTime).Milliseconds         ' Noncompliant
        Dim a2 = Date.Now.Subtract(dateTime).Milliseconds   ' Noncompliant

        Dim b1 = (Date.Now - TimeSpan.FromSeconds(1)).Millisecond       ' Compliant
        Dim b2 = Date.Now.Subtract(TimeSpan.FromDays(1)).Millisecond    ' Compliant

        Dim c1 = (Date.Now - timeSpan).Millisecond              ' Compliant
        Dim c2 = Date.Now.Subtract(timeSpan).Millisecond        ' Compliant

        Dim d1 = (Date.UtcNow - dateTime).Milliseconds          ' Compliant
        Dim d2 = Date.UtcNow.Subtract(dateTime).Milliseconds    ' Compliant

        Dim e1 = (New DateTime(1) - dateTime).Milliseconds          ' Compliant
        Dim e2 = New DateTime(1).Subtract(dateTime).Milliseconds    ' Compliant
    End Sub

    Private timeField As TimeSpan

    Public Property Time As TimeSpan
        Get
            Return timeField
        End Get
        Set(ByVal value As TimeSpan)
            Dim start = Date.Now
            timeField = Date.Now - start ' Noncompliant
        End Set
    End Property

    Private Sub SwitchExpression(ByVal start As Date)
        Dim a = 1
        Select Case a
            Case 1
                timeField = Date.Now - start - New TimeSpan(1) ' Noncompliant
        End Select
    End Sub

    Private Sub NonInLineDateTimeNow()
        Dim start = Date.Now
        ' Some method
        Dim [end] = Date.Now
        Dim elapsedTime = [end] - start ' FN
    End Sub

    Private Sub EdgeCases(ByVal dateTime As Date, ByVal timeSpan As TimeSpan)
        Call (If(True, Date.Now, New DateTime(1))).Subtract(dateTime) ' FN
        Call (If(True, Date.Now, New DateTime(1))).Subtract(timeSpan) ' Compliant

        Date.Now.AddDays(1).Subtract(dateTime) ' FN
        Date.Now.Subtract() ' Error [BC30516]
        Date.Now.Subtract ' Error [BC30516]

        Date.Now.Subtract(dateTime) ' Noncompliant
        Dim span = Date.Now - dateTime ' Noncompliant
    End Sub
End Class

Public Class FakeDateTimeSubtract
    Private Sub MyMethod(ByVal dateTime As Date)
        Dim a = FakeDateTimeSubtract.DateTime.Now.Subtract(dateTime).Milliseconds ' Compliant
    End Sub

    Public NotInheritable Class DateTime
        Public Shared ReadOnly Property Now As Date
    End Class
End Class
