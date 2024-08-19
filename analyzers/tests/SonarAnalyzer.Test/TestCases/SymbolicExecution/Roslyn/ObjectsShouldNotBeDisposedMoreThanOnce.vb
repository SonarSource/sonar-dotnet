Imports System.IO
Imports System.Threading.Tasks

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

    Public Sub NotReallyDisposable()
        Dim d As New DoesNotImplementDisposable()
        d.Dispose()
        d.Dispose() ' Compliant - this is not a call to System.IDisposable.Dispose()
    End Sub

    Public Sub DisposedTwice_Conditional(d As IDisposable)
        If d IsNot Nothing Then d.Dispose()
        d.Dispose() ' Noncompliant {{Resource 'd' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
    End Sub

    Public Sub DisposedTwice_MemberAccess(d As IDisposable)
        d.Dispose
        d.Dispose ' Noncompliant
    End Sub

    Public Sub DisposedTwice_Alias()
        Dim d As New DisposableAlias()
        d.CleanUp()
        d.CleanUp() ' Noncompliant
    End Sub

    Public Sub DisposedTwice_PrivateAlias()
        Dim d As New DisposablePrivateAlias()
        DirectCast(d, IDisposable).Dispose()
        DirectCast(d, IDisposable).Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedTwice_PrivateAlias_Using()
        Using d As New DisposablePrivateAlias()  ' Noncompliant
            DirectCast(d, IDisposable).Dispose()
        End Using
    End Sub

    Private disposable As IDisposable

    Public Sub DisposeField()
        disposable.Dispose()
        disposable.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedParameters(d As IDisposable)
        d.Dispose()
        d.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedTwice_Relations()
        Dim d As IDisposable = New Disposable()
        Dim x As IDisposable = d
        x.Dispose()
        d.Dispose() ' FN, requires relation support
    End Sub

    Public Sub DisposedTwice_Try()
        Dim d As IDisposable = New Disposable()
        Try
            d.Dispose()
        Finally
            d.Dispose() ' Noncompliant
        End Try
    End Sub

    Public Sub DisposedTwice_DifferentCase(d As Disposable)
        d.Dispose()
        d.Dispose() ' Noncompliant
    End Sub

    Public Sub DisposedTwice_Array()
        Dim Arr As IDisposable() = {New Disposable()}
        Arr(0).Dispose()
        Arr(0).Dispose() ' FN
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

    Public Sub Close_ParametersOfDifferentTypes(withDispose As IWithDispose, disposable As IDisposable)
        withDispose.Dispose()
        disposable.Dispose()
    End Sub

    Public Sub Close_ParametersOfSameType(withDispose1 As IWithDispose, withDispose2 As IWithDispose)
        withDispose1.Dispose()
        withDispose2.Dispose()
    End Sub

    Public Sub Close_OneParameterDisposedThrice(withDispose1 As IWithDispose, withDispose2 As IWithDispose)
        withDispose1.Dispose()
        withDispose1.Dispose()  ' Noncompliant
        withDispose1.Dispose()  ' Noncompliant
        withDispose2.Dispose()
    End Sub

End Class

Public Class Class1
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

    Public Shared Sub LoopWithBreak(list As String(), condition As Boolean, withDispose As IWithDispose)
        For Each x As String In list
            Try
                If condition Then withDispose.Dispose() ' FN
                Exit For
            Catch __unusedException1__ As Exception
                Continue For
            End Try
        Next
    End Sub

    Public Shared Sub LoopMethod(list As String(), condition As Boolean, withDispose As IWithDispose)
        For Each x As String In list
            If condition Then withDispose.Dispose() ' Noncompliant
        Next
    End Sub
End Class

Class UsingDeclaration

    Public Sub Disposed_UsingStatement()
        Using d As New Disposable()  ' Noncompliant^15#1
            d.Dispose()
        End Using
    End Sub

    Public Sub Disposed_Using_InitializeBeforeUsingStatement()
        Dim d As New Disposable()
        Using d  ' FN
            d.Dispose()
        End Using
    End Sub

    Public Sub Disposed_UsingStatement_MultipleVariables()
        Using a As New Disposable, b As New Disposable, c As New Disposable ' Noncompliant {{Resource 'a' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
            ' Noncompliant@-1 {{Resource 'b' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
            a.Dispose()
            b.Dispose()
        End Using
    End Sub

    Public Sub Disposed_UsingStatement_MultipleVariableInitializer()
        Using a, b, c As New Disposable ' Noncompliant {{Resource 'a' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
            ' Noncompliant@-1 {{Resource 'b' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
            a.Dispose()
            b.Dispose()
        End Using
    End Sub

End Class


' Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8642
Public Class Repro_8642
    Sub Method(instance As InstanceDisposable)
        StaticDisposable.Dispose()
        StaticDisposable.Dispose()             ' Compliant - none of these methods are coming from the System.IDisposable interface

        instance.Dispose()
        instance.Dispose()                     ' Compliant
    End Sub

    Public NotInheritable Class StaticDisposable
        Public Shared Sub Dispose()
        End Sub
    End Class

    Public Class InstanceDisposable
        Public Sub Dispose()
        End Sub
    End Class
End Class
