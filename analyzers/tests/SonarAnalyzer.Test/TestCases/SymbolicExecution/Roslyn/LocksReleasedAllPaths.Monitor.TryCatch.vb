Imports System.Threading

Namespace Monitor_TryCatch

    Class Program

        Private Condition As Boolean
        Private Obj As New Object()

        Public Sub Method1(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)
            Catch ex As Exception
                Monitor.Exit(Obj)
                Throw
            End Try
        End Sub

        Public Sub Method3(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Try
                Console.WriteLine(Arg.Length)
            Catch ex As Exception
                Throw
            Finally
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method4()
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then Throw New Exception()
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method5(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)
            Catch nre As NullReferenceException
                Monitor.Exit(Obj)
                Throw
            End Try
            Monitor.Exit(Obj)
        End Sub

        Public Sub CatchWhen(condition As Boolean)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine()
                Monitor.Exit(Obj)
            Catch When (condition)
                Monitor.Exit(Obj)
                Throw
            End Try
        End Sub

        Public Sub Method6(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)
            Catch nre As NullReferenceException When nre.Message.Contains("Dummy string")
                Monitor.Exit(Obj)
                Throw
            End Try
        End Sub

        Public Sub Method7(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)
            Catch ex As Exception When TypeOf ex Is NullReferenceException
                Monitor.Exit(Obj)
                Throw
            End Try
        End Sub

        Public Sub Method8(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Try
                Console.WriteLine(Arg.Length)
                Monitor.Exit(Obj)
            Catch ex As Exception
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method9(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)   ' Can throw NullReferenceException when Arg is Nothing
                Monitor.Exit(Obj)
            Catch ex As InvalidOperationException
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method10(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Try
                Console.WriteLine(Arg.Length)
                Monitor.Exit(Obj)
            Catch
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method11(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Try
                Console.WriteLine(Arg.Length)
                Monitor.Exit(Obj)
            Catch nre As NullReferenceException
                Monitor.Exit(Obj)
            Catch ex As Exception
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method12(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            Try
                Console.WriteLine(Arg.Length)
                Monitor.Exit(Obj)
            Catch nre As NullReferenceException
            Catch ex As Exception
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method13(Arg As String)
            Monitor.Enter(Obj)
            Try
                Throw New InvalidOperationException()
            Catch ex As InvalidOperationException
                Monitor.Exit(Obj)
            End Try
        End Sub

        Public Sub Method14(Arg As String)
            Monitor.Enter(Obj) ' FN
            Try
                Throw New NotImplementedException()
            Catch ex As InvalidOperationException
                Monitor.Exit(Obj)
            End Try
        End Sub

    End Class

End Namespace
