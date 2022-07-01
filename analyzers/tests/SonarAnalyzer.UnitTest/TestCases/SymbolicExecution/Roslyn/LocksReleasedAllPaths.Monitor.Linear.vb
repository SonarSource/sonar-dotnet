Imports System.Threading
Imports System.Threading.Tasks

Namespace Monitor_Linear

    Class Program

        Public PublicObject As New Object()
        Private Shared ReadOnly StaticObj As New Object()
        Private Obj As New Object()
        Private Other As New Object()

        Public Sub Method1(Arg As String)
            Monitor.Enter(Obj) ' FN, because arg.Length can throw NullReferenceException
            Console.WriteLine(Arg.Length)
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method1_SafeOperation(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Arg = Nothing
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method2(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Arg = Nothing
            Monitor.Exit(Other)
        End Sub

        Public Sub Method3()
            Monitor.Enter(Obj) ' Compliant
            Dim A As Action = Sub() Monitor.Exit(Obj)
        End Sub

        Public Sub Method4(Arg As String)
            Dim LocalObj = Obj
            Monitor.Enter(LocalObj) ' Compliant
            Arg = Nothing
            Monitor.Exit(LocalObj)
        End Sub

        Public Sub Method5(Arg As String)
            Dim LocalObj = Obj
            Monitor.Enter(Obj) ' Compliant
            Console.WriteLine(Arg.Length)
            Monitor.Exit(LocalObj)
        End Sub

        Public Sub Method6(Arg As String, ParamObj As Object)
            ParamObj = Obj
            Monitor.Enter(ParamObj) ' Compliant
            Arg = Nothing
            Monitor.Exit(ParamObj)
        End Sub

        Public Sub Method7(Arg As String)
            Monitor.Enter(Obj) ' Compliant
            Console.WriteLine(Arg.Length)
            Dim localObj = Obj
            Monitor.Exit(localObj)
        End Sub

        Public Sub Method7(Arg As String, ParamObj As Object)
            Monitor.Enter(ParamObj) ' Compliant
            Arg = Nothing
            Monitor.Exit(ParamObj)
        End Sub

        Public Sub Method8(Arg As String, p As Program)
            Monitor.Enter(p.PublicObject) ' Compliant
            Console.WriteLine(Arg.Length)
            Monitor.Exit(p.PublicObject)
        End Sub

        Public Sub Method9()
            Dim A As Action = Sub() Monitor.Enter(Obj) ' Compliant
        End Sub

        Public Sub Method10()
            Dim GetObj As Func(Of Object) = Function() Obj
            Monitor.Enter(GetObj())
            Monitor.Exit(GetObj())
        End Sub

        Public Sub Method11()
            Dim GetObj As Func(Of Object) = Function() Obj
            Monitor.Enter(Obj)
            Monitor.Exit(GetObj())
        End Sub

        Public Sub Method12()
            Monitor.Enter(Obj) ' Compliant
            Dim A As Action = Sub() Monitor.Exit(Obj)
            A()
        End Sub

        Public Sub Method13(Arg As String)
            Monitor.Exit(Obj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub FieldReference_WithThis(Arg As String)
            Monitor.Exit(Me.Obj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(Me.Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub FieldReference_WithThis_Mixed1(Arg As String)
            Monitor.Exit(Obj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(Me.Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub FieldReference_WithThis_Mixed2(Arg As String)
            Monitor.Exit(Me.Obj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub StaticFieldReference(Arg As String)
            Monitor.Exit(StaticObj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(StaticObj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub StaticFieldReference_Class(Arg As String)
            Monitor.Exit(Program.StaticObj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(Program.StaticObj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub Method13_LocalVar(Arg As String)
            Dim l As New Object()
            Monitor.Exit(l)
            Console.WriteLine(Arg.Length)
            Monitor.Enter(l) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
        End Sub

        Public Sub Method13_Parameter(Arg As Object)
            Monitor.Exit(Arg)
            Monitor.Enter(Arg) ' Noncompliant
        End Sub

        Public Sub Method14(Arg As String)
            Monitor.Exit(Obj)
            Arg = Nothing
            Monitor.Enter(Obj) ' Compliant
            Arg = Nothing
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method15(First As Program, Second As Program)
            Monitor.Enter(First.Obj) ' Compliant
            Monitor.Exit(Second.Obj)
        End Sub

        Public Sub WrongCallNoArgs(Arg As String)
            Monitor.Exit(Obj)
            Console.WriteLine(Arg.Length)
            Monitor.Enter() ' Error BC30516	Overload resolution failed because no accessible 'Enter' accepts this number of arguments.
        End Sub

        Public Sub DifferentFields(First As Program, Second As Program)
            Monitor.Exit(Second.Obj)
            Monitor.Enter(First.Obj)
        End Sub

        Public Shared ReadOnly Property Prop As Integer
            Get
                Dim LockObject As New Object()  ' Adds coverage For handling FlowCaptureReference operations.
                SyncLock (LockObject)
                    Return 1
                End SyncLock
            End Get
        End Property

        Public MustInherit Class Base

            Protected MustOverride Function GetScheduledTasks() As List(Of Task)

        End Class

        Public Class Derived
            Inherits Base

            Private ReadOnly _tasks As New List(Of Task)

            Protected Overrides Function GetScheduledTasks() As List(Of Task)   ' Adds coverage For handling Conversion operations
                Dim lockTaken = False
                Try
                    Monitor.TryEnter(_tasks, lockTaken)
                    If lockTaken Then
                        Return _tasks
                    Else
                        Throw New NotSupportedException()
                    End If
                Finally
                    If lockTaken Then Monitor.Exit(_tasks)
                End Try
            End Function
        End Class

        Public Class PropertyReference

            Public Function PreMovieInfoScraperAction(Context As Context) As Context ' Adds coverage For PropertyReference operations
                If String.IsNullOrEmpty(Context.Movie.Year) Then Context.Movie.Year = "2022"
                Return Context
            End Function

            Public Class Context

                Public ReadOnly Property Movie As Movie

            End Class

            Public Class Movie

                Public Property Year As Integer

            End Class

        End Class

    End Class

End Namespace
