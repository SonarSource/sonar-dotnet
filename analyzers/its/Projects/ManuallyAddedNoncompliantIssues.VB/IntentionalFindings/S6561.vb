Imports System

Friend Class S6561
    Public Sub Benchmark()
        Dim start = Date.Now
        MethodToBeBenchmarked()
        Console.WriteLine($"{(Date.Now - start).TotalMilliseconds} ms") ' Noncompliant (S6561)
    End Sub
    Private Sub MethodToBeBenchmarked()
    End Sub
End Class
