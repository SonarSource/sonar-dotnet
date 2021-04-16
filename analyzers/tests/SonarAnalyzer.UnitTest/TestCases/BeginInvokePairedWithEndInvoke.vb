Imports System
Imports System.Collections.Generic
Imports System.Threading

Public Delegate Sub AsyncMethodCaller(Name As String)

Module Common

    Public Sub AsyncMethod(Msg As String)
        Console.WriteLine($"AsyncMethod: {Msg}")
    End Sub

    Public Sub Main()
        BeginInvokeAndEndInvokeOnDelegateWithoutCallback()
    End Sub

    Private Sub BeginInvokeOnDelegateWithoutCallbackSub()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Caller.BeginInvoke("delegate", Nothing, Nothing) ' Noncompliant {{Pair this "BeginInvoke" with an "EndInvoke".}}
        ' https:'docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously
        ' «Important: No matter which technique you use, always call EndInvoke to complete your asynchronous call.»
    End Sub

    Private Function BeginInvokeOnDelegateWithoutCallbackFunction() As String
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Caller.BeginInvoke("delegate", Nothing, Nothing) ' Noncompliant {{Pair this "BeginInvoke" with an "EndInvoke".}}
        ' https:'docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously
        ' «Important: No matter which technique you use, always call EndInvoke to complete your asynchronous call.»
    End Function

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithoutCallback()
        Console.WriteLine("BeginInvokeAndEndInvokeOnDelegateWithoutCallback")
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Result As IAsyncResult = Caller.BeginInvoke("delegate", Nothing, Nothing) ' Compliant
        Caller.EndInvoke(Result)
    End Sub

    Private Sub BeginInvokeOnDelegateWithLambdaCallback1()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Caller.BeginInvoke("delegate", Sub(Result)          ' Noncompliant
                                       End Sub, Nothing)
    End Sub

    Private Sub BeginInvokeOnDelegateWithLambdaCallback2()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As New AsyncCallback(Sub(Result)
                                          End Sub)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Noncompliant
    End Sub

    Private Sub BeginInvokeOnDelegateWithLambdaCallback3()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As AsyncCallback = New AsyncCallback(Sub(Result)
                                                          End Sub)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Noncompliant
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback1()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Caller.BeginInvoke(Name:="delegate", DelegateAsyncState:=Nothing, DelegateCallback:=Sub(Result) Caller.EndInvoke(Result))   ' Compliant
        Caller.BeginInvoke(Name:="delegate", DelegateAsyncState:=Nothing, DelegateCallback:=Sub(Result)                             ' Compliant
                                                                                                Caller.EndInvoke(Result)
                                                                                            End Sub)
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithLambdaCallback2()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As New AsyncCallback(Sub(Result) Caller.EndInvoke(Result))
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Compliant, EndInvoke is called by Callback
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithVariableCallback()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As AsyncCallback = Sub(Result) Caller.EndInvoke(Result)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Compliant
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithStaticCallback1()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As New AsyncCallback(AddressOf SharedCallEndInvoke)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Compliant, EndInvoke is called by SharedCallEndInvoke
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithStaticCallback2()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Caller.BeginInvoke("delegate", New AsyncCallback(AddressOf SharedDoNothing), Nothing) ' Noncompliant
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithStaticCallback3()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As New AsyncCallback(AddressOf SharedDoNothing)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Noncompliant
    End Sub

    Private Sub BeginInvokeOnDelegateWithCallbackAssignment()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Callback As AsyncCallback
        Callback = New AsyncCallback(AddressOf SharedDoNothing)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' false-negative, we only look at the variable initialization and not at all its assignments
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithWrapperCallback1()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Wrapper As New CallerWrapper(Caller)
        Dim Callback As New AsyncCallback(AddressOf Wrapper.CallEndInvoke)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Compliant, EndInvoke is called by wrapper.CallEndInvoke
    End Sub

    Private Sub BeginInvokeAndEndInvokeOnDelegateWithWrapperCallback2()
        Dim Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
        Dim Wrapper As New CallerWrapper(Caller)
        Dim Callback As New AsyncCallback(AddressOf Wrapper.DoNothing)
        Caller.BeginInvoke("delegate", Callback, Nothing) ' Noncompliant
    End Sub

    Private Sub BeginInvokeOnAnyClassButDelegate()
        Dim NotADelegate As New AnyClassWithOptionalEndInvoke()
        Dim Result As IAsyncResult = NotADelegate.BeginInvoke(New AsyncMethodCaller(AddressOf AsyncMethod))
        ' Compliant, NotADelegate class declared below does not required a call to EndInvoke
        ' Same as System.Windows.Forms.Control see
        ' https:'docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.begininvoke?view=netframework-4.6
        ' «You can call EndInvoke to retrieve the return value from the delegate, if necessary, but this is not required.»
    End Sub

    Private Sub SharedCallEndInvoke(Result As IAsyncResult)
        Dim Caller As AsyncMethodCaller = DirectCast(DirectCast(Result, AsyncResult).AsyncDelegate, AsyncMethodCaller)
        Caller.EndInvoke(Result)
    End Sub

    Private Sub SharedDoNothing(Result As IAsyncResult)
    End Sub

    Public Sub NullConditionalIndexing(List As List(Of Integer))
        Dim Value As Integer = List?(0)
    End Sub

End Module

Public Class CallerWrapper

    Private Caller As AsyncMethodCaller

    Public Sub New(Caller As AsyncMethodCaller)
        Me.Caller = Caller
    End Sub

    Public Sub CallEndInvoke(Result As IAsyncResult)
        Caller.EndInvoke(Result)
    End Sub

    Public Sub DoNothing(Result As IAsyncResult)
    End Sub

End Class

Public Class AnyClassWithOptionalEndInvoke

    Public Function BeginInvoke(Method As AsyncMethodCaller) As IAsyncResult
        Return Method.BeginInvoke("NotADelegate", Sub(ar) Method.EndInvoke(ar), Nothing)
    End Function

    ' It's not required to call EndInvoke after BeginInvoke on this class
    Public Function EndInvoke(asyncResult As IAsyncResult) As Object
        Return Nothing
    End Function

End Class

Public Module CommonMod

    Public Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
    Public Result As IAsyncResult = Caller.BeginInvoke("Foo", Nothing, Nothing)     ' Noncompliant
    Public Property AutoProperty As IAsyncResult = Caller.BeginInvoke("Foo", Nothing, Nothing)     ' Noncompliant

    Public Class Fake

        Public Sub Misleading()
            Caller.EndInvoke(Nothing)
        End Sub

    End Class

End Module

Public Class ForCoverage

    Public Shared Caller As New AsyncMethodCaller(AddressOf AsyncMethod)
    Public Result As IAsyncResult = Caller.BeginInvoke("Foo", Nothing, Nothing)     ' Noncompliant
    Public Property AutoProperty As IAsyncResult = Caller.BeginInvoke("Foo", Nothing, Nothing)     ' Noncompliant

    Public Action1 As Action = Sub()
                                   Caller.BeginInvoke("Foo", Nothing, Nothing)      ' Noncompliant
                               End Sub

    Public Action2 As Action = Sub() Caller.BeginInvoke("Foo", Nothing, Nothing)    ' Noncompliant

    Public Action3 As Action(Of String) = Function(Name) Caller.BeginInvoke(Name, Nothing, Nothing) ' Noncompliant

    Public Action4 As Action(Of String) = Function(Name)
                                              Return Caller.BeginInvoke(Name, Nothing, Nothing) ' Noncompliant
                                          End Function

    Public ReadOnly Property Prop As Integer
        Get
            Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
        End Get
    End Property

    Public Property PropFake As Integer
        Get
            Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
        End Get
        Set(value As Integer)
            Caller.EndInvoke(Nothing)
        End Set
    End Property

    Public Custom Event CustomHandler As EventHandler
        AddHandler(value As EventHandler)
            Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
        End AddHandler
        RemoveHandler(value As EventHandler)
            Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
        End RemoveHandler
        RaiseEvent(sender As Object, e As EventArgs)
            Caller.EndInvoke(Nothing)
        End RaiseEvent
    End Event

    Public Shared Widening Operator CType(F As ForCoverage) As Integer
        Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
    End Operator

    Public Shared Operator +(a As ForCoverage, b As ForCoverage) As ForCoverage
        Caller.BeginInvoke("prop", Nothing, Nothing) ' Noncompliant
    End Operator

    Shared Sub New()
        Caller.BeginInvoke("Foo", Nothing, Nothing) ' Noncompliant
    End Sub

    Public Sub New()
        Caller.BeginInvoke("Foo", Nothing, Nothing) ' Noncompliant
    End Sub

    Public Sub Compliant()
        Dim Result As IAsyncResult = Caller.BeginInvoke("method", Nothing, Nothing) ' Compliant
        Caller.EndInvoke(Result)
    End Sub

    Public Sub Bar()
        Caller.BeginInvoke("Foo", Nothing, Nothing) ' Noncompliant
    End Sub

    Protected Overrides Sub Finalize()
        Caller.BeginInvoke("Foo", Nothing, Nothing) ' Noncompliant
    End Sub

    Public Sub BeginInvoke()
    End Sub

    Private Sub EndInvoke()
    End Sub

    Public Sub BeginInvoke(Arg As String)
    End Sub

    Private Sub InvokeFakeMethod()
        BeginInvoke()
        BeginInvoke("Wrong parameter type")
        EndInvoke()
    End Sub

    Private Sub InvokeSomethingElse()
        Dim BeginInvoke As String = "MemberBinding"
        Dim EndInvoke As String = "MemberBinding"
        BeginInvoke.ToString()
        EndInvoke.ToString()
    End Sub

    Private Sub InvokeFromInvocation()
        Caller.BeginInvoke("FromInvocation", CreateCallback(), Nothing) 'Noncompliant
    End Sub

    Private Function CreateCallback() As AsyncCallback
        Return Nothing
    End Function

    Public Structure FooStruct

        Public Field As String

        Public Sub New(Field As String)
            Me.Field = Field
            Caller.BeginInvoke("FooStruct", Nothing, Nothing) ' Noncompliant
        End Sub

    End Structure

End Class

' Dummy implementation of IAsyncResult compatible with both .Net Framework and .Net Core
Friend Class AsyncResult
    Implements IAsyncResult

    Public ReadOnly Property AsyncDelegate As Object

    Public ReadOnly Property IsCompleted As Boolean Implements IAsyncResult.IsCompleted
    Public ReadOnly Property AsyncWaitHandle As WaitHandle Implements IAsyncResult.AsyncWaitHandle
    Public ReadOnly Property AsyncState As Object Implements IAsyncResult.AsyncState
    Public ReadOnly Property CompletedSynchronously As Boolean Implements IAsyncResult.CompletedSynchronously

End Class

Class ReproEndinvokeDelegate

    Public Sub BeginInvokeWithEndinvokeDelegate()
        Dim A As Action = Sub() Return
        A.BeginInvoke(AddressOf A.EndInvoke, Nothing) ' Compliant
     End Sub

End Class
