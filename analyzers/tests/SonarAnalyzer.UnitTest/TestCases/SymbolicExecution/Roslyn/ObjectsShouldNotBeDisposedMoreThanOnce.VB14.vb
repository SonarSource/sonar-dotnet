Class Program
    Public Sub DisposePotentiallyNullField(d As IDisposable)
        d?.Dispose()
        d?.Dispose() ' Noncompliant
    End Sub
End Class
