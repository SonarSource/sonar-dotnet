Imports System

Friend Class AvoidDateTimeNowForBenchmarking
    Public Sub S6561()
        Dim start = Date.Now
        MethodToBeBenchmarked()
        Console.WriteLine($"{(Date.Now - start).TotalMilliseconds} ms")
    End Sub
    Private Sub MethodToBeBenchmarked()
    End Sub
End Class
