Imports System
Imports System.Threading.Tasks

Namespace Tests.Diagnostics
    Public Class CompliantUseCases

        Public Async Function GetTaskAsync2() As Task(Of Object)
            Return Nothing
        End Function

        Public Function GetTask1() As Task
            Return Task.FromResult(True)
        End Function

        Public Function GetTask2() As Task
            Dim x = New Task(Of Object)(Function() Nothing)
            Return Task.Delay(0)
        End Function

        Public Property Foo As Task

        Public Function GetFooAsync() As Task(Of Object)
            Return Task.Run(Function()
                                If False Then
                                    Return New Object()
                                Else
                                    Return Nothing
                                End If
                            End Function)
        End Function

        Public Function GetBar() As Task
            Dim func As Func(Of Task) = Function() Nothing
            Return func()
        End Function
    End Class

    Public Class NonCompliantUseCases
        Public Function GetTask1() As Task
            Return Nothing ' Noncompliant {{Do not return null from this method, instead return 'Task.FromResult(Of T)(Nothing)', 'Task.CompletedTask' or 'Task.Delay(0)'.}}
'                  ^^^^^^^
        End Function

        Public Function GetTask2() As Task(Of Object)
            Return Nothing ' Noncompliant
        End Function

        Public Function GetTaskAsync3(ByVal a As Integer) As Task
            If a > 42 Then
                Return Nothing ' Noncompliant
            End If

            Return Task.Delay(0)
        End Function

        Public Function GetTask5(ByVal a As Integer) As Task(Of String)
            If a > 0 Then
                Return Nothing ' Noncompliant
            Else
                Return Nothing ' Noncompliant
            End If
        End Function

        Public Function GetTask6() As Task
            Return (Nothing) ' Noncompliant
        End Function

        Public Function GetTask7(ByVal condition As Boolean) As Task
            Return If(condition, Task.FromResult(5), Nothing) ' Noncompliant
        End Function

        Public Property Foo As Task
            Get
                Return Nothing ' Noncompliant
            End Get
            Set(ByVal value As Task)
                foo = value
            End Set
        End Property
    End Class
End Namespace
