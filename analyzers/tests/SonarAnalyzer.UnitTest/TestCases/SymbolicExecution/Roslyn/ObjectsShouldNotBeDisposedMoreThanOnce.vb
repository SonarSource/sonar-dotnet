Imports System
Imports System.IO

Interface IWithDispose
    Inherits IDisposable
End Interface

Public Class Disposable
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub

End Class

Public Class DisposableAlias
    Implements IDisposable

    Public Sub CleanUp() Implements IDisposable.Dispose
    End Sub

End Class

Public Class DisposablePrivateAlias
    Implements IDisposable

    Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
    End Sub

End Class

Public Class DoesNotImplementDisposable

    Public Sub Dispose()
    End Sub

End Class

Class Program

    Public Sub DisposedTwice_Alias()
        Dim d As New DisposableAlias()
        d.CleanUp()
        d.CleanUp() ' Noncompliant
    End Sub

End Class
