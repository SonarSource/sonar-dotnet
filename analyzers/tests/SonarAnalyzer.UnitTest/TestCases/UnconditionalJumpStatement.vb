Imports System
Imports System.IO
Imports System.Threading.Tasks

Namespace Tests.Diagnostics
    Class Program
        Public Sub Test1(strings As String())
            Dim length As Integer = Nothing
            For i As Integer = 0 To length - 1
                Exit For
'               ^^^^^^^^ Noncompliant {{Refactor the containing loop to do more than one iteration.}}
            Next

            For i As Integer = 0 To length - 1
                Continue For ' Noncompliant
            Next

            For i As Integer = 0 To length - 1
                Return ' Noncompliant
            Next

            For i As Integer = 0 To length - 1
                Throw New Exception() ' Noncompliant
            Next

            For Each s As Char In strings
                Exit For ' Noncompliant
            Next

            For Each s As Char In strings
                Continue For ' Noncompliant
            Next

            For Each s As Char In strings
                Return ' Noncompliant
            Next

            For Each s As Char In strings
                Throw New Exception() ' Noncompliant
            Next

            While True
                Exit While ' Noncompliant
            End While

            While True
                Continue While ' Noncompliant
            End While

            While True
                Return ' Noncompliant
            End While

            While True
                Throw New Exception() ' Noncompliant
            End While

            Do
                Exit Do ' Noncompliant
            Loop While True

            Do
                Continue Do ' Noncompliant
            Loop While True

            Do
                Return ' Noncompliant
            Loop While True

            Do
                Throw New Exception() ' Noncompliant
            Loop While True
        End Sub

        Public Sub Test2(strings As String(), [stop] As Boolean)
            While True
                If [stop] Then
                    Exit While ' Compliant
                End If
            End While

            While True
                If [stop] Then
                    Continue While ' Compliant
                End If
            End While

            While True
                If [stop] Then
                    Return ' Compliant
                End If
            End While

            While True
                If [stop] Then
                    Throw New Exception() ' Compliant
                End If
            End While

            While True
                If [stop] Then Throw New Exception() ' Compliant
            End While

            Dim s As String
            For Each cs As Char In s
                If Not False Then Return ' Compliant
            Next

        End Sub

        Public Sub Test3(strings As String(), [stop] As Boolean)
            If [stop] Then
                While True
                    Exit While ' Noncompliant
                End While

                While True
                    Continue While ' Noncompliant
                End While

                While True
                    Return
'                   ^^^^^^ Noncompliant {{Refactor the containing loop to do more than one iteration.}}
                End While

                While True
                    Throw New Exception()
'                   ^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Refactor the containing loop to do more than one iteration.}}
                End While
            End If
        End Sub

        Public Sub Test4(strings As String(), padding As Integer)
            While True
                Select Case padding
                    Case 1
                        Exit Select ' Compliant
                    Case 2
                        Throw New Exception() ' Compliant
                    Case Else
                        Exit Select ' Compliant
                End Select
            End While

            While True
                Select Case padding
                    Case 1
                        Return ' Compliant
                    Case Else
                        Return ' Compliant
                End Select
            End While
        End Sub

        Public Sub Test5(doSomething As Action, logError As Action(Of Exception))
            While True
                Try
                    doSomething()
                Catch e As Exception
                    logError(e)
                    Throw ' Compliant
                End Try
            End While
        End Sub

        Public Test6 As Func(Of Integer) = Function()
                                               If True Then
                                                   Return 5
                                               Else
                                                   Return 10
                                               End If
                                           End Function

        Public Sub Test7()
            While True
                If True Then
                    Continue While
                End If

                Exit While
                GetHashCode()
            End While
        End Sub

        Public Sub Test8()
            While True
                If True Then
                    Exit While
                End If

                Continue While ' Noncompliant
            End While
        End Sub

        Public Sub Test9()
            While True
                If True Then
                    Continue While
                End If


                Return ' Compliant
            End While
        End Sub

        Public Sub Test10()
            While True
                If True Then
                    Throw New Exception()
                End If

                Continue While ' Noncompliant
            End While
        End Sub

        Public Sub Test11()
            While True
                If True Then
                    Return
                End If


                Exit While ' Noncompliant
            End While
        End Sub
        Public Sub Test12()
            While True
                bar()
                baz()
                Continue While ' Noncompliant
            End While
        End Sub

        Private Sub baz()
            Throw New NotImplementedException()
        End Sub

        Private Sub bar()
            Throw New NotImplementedException()
        End Sub

        Public Sub Test13()
            While True
                bar()
                Continue While ' Noncompliant
                bar()
            End While
        End Sub

        Public Sub Test14()
            While True
                bar()
                Continue While ' Noncompliant
                bar()
            End While
        End Sub

        Public Sub Test15()
            While True
                UtilFunc(Function(lambda)
                             Return True ' Compliant
                         End Function)

                Continue While ' Noncompliant
            End While
        End Sub

        Public Sub Test16()
            While True
                If True Then

                Else
                    Continue While ' Compliant
                End If

                Exit While
            End While
        End Sub

        Public Sub Test17()
            While True
                While True
                    If True Then

                    Else
                        Exit While
                    End If
                End While

                Exit While ' Noncompliant
            End While
        End Sub

        Private Sub UtilFunc(a As Func(Of Boolean, Boolean))
        End Sub

        Public Function TestWithRetry(doSomething As Action) As Boolean
            For i As Integer = 0 To 2
                Try
                    doSomething()
                    Return True ' Compliant
                Catch e As Exception
                    If i = 2 Then
                        Return False ' Compliant
                    End If
                End Try
            Next
        End Function

        Public Function TestWithRetry2() As Boolean
            For i As Integer = 0 To 2
                Try
                    Return True ' Noncompliant
                Catch e As Exception
                    If i = 2 Then
                        Return False ' Compliant
                    End If
                End Try
            Next
        End Function

        Public Function TestWithRetry3(doSomething As Func(Of Boolean)) As Boolean
            For i As Integer = 0 To 2
                Try
                    Return (((doSomething()))) ' Compliant
                Catch e As Exception
                    If i = 2 Then
                        Return False ' Compliant
                    End If
                End Try
            Next
        End Function

        Public Async Function TestWithRetry5(doSomething As Func(Of Task(Of Boolean))) As Task(Of Boolean)
            For i As Integer = 0 To 2
                Try
                    Return (((Await doSomething()))) ' Compliant
                Catch e As Exception
                    If i = 2 Then
                        Return False ' Compliant
                    End If
                End Try
            Next
        End Function

        Protected Overridable Function CreateLock(lockFileName As String, retries As Boolean) As FileStream
            If retries Then
                ResetRetryTimeout()
            End If
            Dim filelock As FileStream = Nothing
            While True
                Try
                    filelock = New FileStream(lockFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.DeleteOnClose)
                    Return filelock
                Catch e As Exception
                    Dim code As Integer = System.Runtime.InteropServices.Marshal.GetHRForException(e)
                    If code = CInt(&H80070020) OrElse code = CInt(&H80070021) Then
                        ' Sharing violation
                        If Not retries Then
                            Return Nothing
                        End If
                        If Not WaitRetryTimeout() Then
                            Throw
                        End If
                    Else
                        ' All others are considered an error and we don't retry
                        Throw
                    End If
                End Try
            End While


        End Function

        Private Function WaitRetryTimeout() As Boolean
            Throw New NotImplementedException()
        End Function

        Private Sub ResetRetryTimeout()
            Throw New NotImplementedException()
        End Sub
    End Class

    Public Class ReproducerClass

        ReadOnly Property FirstProperty As ReproducerClass
            Get
                Return Nothing
            End Get
        End Property

        ReadOnly Property SecondProperty As Int32
            Get
                Throw New Exception()
            End Get
        End Property

        Public Function MethodWithPropertyAccess() As Int32
              While True
                Try
                    Return SecondProperty ' Compliant
                Catch E As Exception

                End Try
            End While
        End Function

        Public Function MethodWithMemberAccess() As Int32
              While True
                Try
                    Return FirstProperty.SecondProperty ' Compliant
                Catch E As Exception

                End Try
            End While
        End Function

        Public Function MethodWithElementAccess(Array As Int32()) As Int32
              While True
                Try
                    Return (((Array(0)))) ' Compliant
                Catch E As Exception

                End Try
            End While
        End Function

        Public Function MethodWithAssignmentInsideReturnStatement() As Int32
            Dim X As Int32
              While True
                Try
                    return X = SecondProperty ' Compliant
                Catch E As Exception

                End Try
            End While
        End Function

    End Class

End Namespace
