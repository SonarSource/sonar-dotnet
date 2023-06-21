Imports System

Public Class Program
    Private Sub Benchmark()
        Dim start = Date.Now
        MethodToBeBenchmarked()
        ' something
        Console.WriteLine($"{(Date.Now - start).TotalMilliseconds} ms") ' Noncompliant {{Avoid using "DateTime.Now" for benchmarking or timing operations}}
        '                     ^^^^^^^^^^^^^^^^
    End Sub

    Private Const MinRefreshInterval As Integer = 100

    Private Sub Timing()
        Dim lastRefresh = Date.Now
        If (Date.Now - lastRefresh).TotalMilliseconds > MinRefreshInterval Then ' Noncompliant
        '   ^^^^^^^^^^^^^^^^^^^^^^
            lastRefresh = Date.Now
            ' Refresh
        End If
    End Sub

    Private timeField As TimeSpan

    Public Property Time As TimeSpan
        Get
            Return timeField
        End Get
        Set(ByVal value As TimeSpan)
            Dim start = Date.Now
            MethodToBeBenchmarked()
            timeField = Date.Now - start ' Noncompliant
        End Set
    End Property

    Private Sub SwitchExpression()
        Dim a = 1
        Select Case a
            Case 1
                Dim start = Date.Now
                MethodToBeBenchmarked()
                timeField = Date.Now - start - New TimeSpan(1) ' Noncompliant
            Case 2
        End Select
    End Sub

    Private Function MethodToBeBenchmarked() As Boolean
        Return True
    End Function
End Class
