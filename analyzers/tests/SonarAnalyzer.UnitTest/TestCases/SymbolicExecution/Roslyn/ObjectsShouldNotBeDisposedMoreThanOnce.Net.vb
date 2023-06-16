Public Class DisposeAsync

    Private Async Function DisposeAsyncTwiceUsingStatement(d As DisposableAsync) As Task
        Using d ' FN
        End Using
        Await d.DisposeAsync()
    End Function

    Private Async Function DisposeAsyncTwice() As Task
        Dim d As New DisposableAsync()
        Await d.DisposeAsync()
        Await d.DisposeAsync() ' Noncompliant
    End Function

    Private Async Function DisposeTwiceMixed() As Task
        Dim d As New DisposableAsync()
        Await d.DisposeAsync()
        d.Dispose()  ' Noncompliant
    End Function

    Private Async Function DisposeTwiceWithAlias() As Task
        Dim d As New DisposableAsyncAlias()
        Await d.CleanAsync()
        Await d.CleanAsync()  ' Noncompliant
    End Function

End Class

Public Class DisposableAsync
    Implements IDisposable, IAsyncDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub

    Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
    End Function

End Class

Public Class DisposableAsyncAlias
    Implements IAsyncDisposable

    Public Function CleanAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
    End Function

End Class
