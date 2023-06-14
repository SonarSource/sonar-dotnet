Imports System
Imports System.IO
Imports System.Data.Common

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

Public Class DoesNotImplementDisposable

    Public Sub Dispose()
    End Sub

End Class

Class Program

    Public Sub NotReallyDisposable()
        Dim d = New DoesNotImplementDisposable()
        d.Dispose()
        d.Dispose() ' Noncompliant - IDisposal interface implementation is not checked
'       ^^^^^^^^^^^
    End Sub

    Public Sub DisposedTwice_Conditional()
        Dim d As IDisposable = Nothing
        d = New Disposable()
        If d IsNot Nothing Then
            d.Dispose()
        End If
        d.Dispose() ' Noncompliant {{Resource 'd' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
 '      ^^^^^^^^^^^       
    End Sub

    Private disposable As IDisposable

    Public Sub DisposeField()
        disposable.Dispose()
        disposable.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedParameters(ByVal d As IDisposable)
        d.Dispose()
        d.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposePotentiallyNullField(ByVal d As IDisposable)
        d?.Dispose()
        d?.Dispose() ' FN
    End Sub

    Public Sub DisposedTwice_Relations()
        Dim d As IDisposable = New Disposable()
        Dim x = d
        x.Dispose()
        d.Dispose() ' FN, requires relation support
    End Sub

    Public Sub DisposedTwice_Try()
        Dim d As IDisposable = Nothing
        Try
            d = New Disposable()
            d.Dispose()
        Finally
            d.Dispose() ' Noncompliant
        End Try
    End Sub

    Public Sub DisposedTwice_DifferentCase(ByVal d As Disposable)
        d.Dispose()
        d.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedTwice_Array()
        Dim a = {New Disposable()}
        a(0).Dispose()
        a(0).Dispose() ' FN
    End Sub

    Public Sub Dispose_Stream_LeaveOpenFalse()
        Using memoryStream As MemoryStream = New MemoryStream() ' Compliant
            Using writer As StreamWriter = New StreamWriter(memoryStream, New System.Text.UTF8Encoding(False), 1024, leaveOpen:=False)
            End Using
        End Using
    End Sub

    Public Sub Dispose_Stream_LeaveOpenTrue()
        Using memoryStream As MemoryStream = New MemoryStream() ' Compliant
            Using writer As StreamWriter = New StreamWriter(memoryStream, New System.Text.UTF8Encoding(False), 1024, leaveOpen:=True)
            End Using
        End Using
    End Sub

    Public Sub Disposed_Using_WithDeclaration()
        Using d = New Disposable()  ' Noncompliant
        '     ^
            d.Dispose()
        End Using
    End Sub

    Public Sub Disposed_Using_WithExpressions()
        Dim d = New Disposable()
        Using d  ' FN
            d.Dispose()
        End Using
    End Sub

    Public Sub Close_ParametersOfDifferentTypes(ByVal withDispose As IWithDispose, ByVal disposable As IDisposable)
        withDispose.Dispose()
        disposable.Dispose()
    End Sub

    Public Sub Close_ParametersOfSameType(ByVal withDispose1 As IWithDispose, ByVal withDispose2 As IWithDispose)
        withDispose1.Dispose()
        withDispose2.Dispose()
    End Sub

    Public Sub Close_OneParameterDisposedThrice(ByVal withDispose1 As IWithDispose, ByVal withDispose2 As IWithDispose)
        withDispose1.Dispose()
        withDispose1.Dispose()  ' Noncompliant
        withDispose1.Dispose()  ' Noncompliant
        withDispose2.Dispose()
    End Sub

End Class

Public Class ClassExample
    Implements IDisposable

    Private Sub Dispose() Implements IDisposable.Dispose
    End Sub

    Public Sub DisposeMultipleTimes()
        Dispose()
        Me.Dispose() ' FN
        Dispose() ' FN
    End Sub

    Public Sub DoSomething()
        Dispose()
    End Sub

End Class

Class TestLoops

    Public Shared Sub LoopWithBreak(ByVal list As String(), ByVal condition As Boolean, ByVal withDispose As IWithDispose)
        For Each x As String In list
            Try
                If condition Then
                    withDispose.Dispose() ' FN
                End If

                Exit For
            Catch __unusedException1__ As Exception
                Continue For
            End Try
        Next
    End Sub

    Public Shared Sub LoopMethod(ByVal list As String(), ByVal condition As Boolean, ByVal withDispose As IWithDispose)
        For Each x As String In list
            If condition Then
                withDispose.Dispose()  ' Noncompliant
            End If
        Next
    End Sub
End Class

Class UsingDeclaration

    Public Sub Disposed_UsingStatement()
        Using d = New Disposable()  ' Noncompliant
            d.Dispose()
        End Using
    End Sub

End Class

Public Class Close

    Public Sub CloseStreamTwice()
        Dim fs = New FileStream("c:\foo.txt", FileMode.Open)
        fs.Close()
        fs.Close() ' FN - Close On streams is disposing resources
    End Sub

    Private Sub CloseTwiceDBConnection(ByVal connection As DbConnection)
        connection.Open()
        connection.Close()
        connection.Open()
        connection.Close() ' Compliant - close() in DB connection does not dispose the connection object.
    End Sub

End Class
