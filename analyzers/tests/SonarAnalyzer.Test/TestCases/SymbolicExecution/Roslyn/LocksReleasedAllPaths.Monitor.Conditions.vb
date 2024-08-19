Imports System
Imports System.Threading

Namespace Monitor_Conditions

    Class Program

        Public PublicObject As New Object()
        Private Obj As New Object()
        Private Other As New Object()
        Private Shared ReadOnly StaticObj As New Object()

        Private Condition As Boolean

        Public Sub Method1()
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method2()
            Monitor.Enter(Obj) ' Noncompliant
            Select Case Condition
                Case True : Monitor.Exit(Obj)
                Case Else
            End Select
        End Sub

        Public Sub Method3()
            Dim IsAcquired As Boolean
            Monitor.Enter(Obj, IsAcquired) ' Noncompliant
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method4()
            Monitor.Enter(Obj)     ' Noncompliant
            Monitor.Enter(Other)   ' Noncompliant
            If Condition Then
                Monitor.Exit(Obj)
            Else
                Monitor.Exit(Other)
            End If
        End Sub

        Public Sub Method5()
            Monitor.Enter(Obj) ' Compliant
            If Condition Then Monitor.Exit(Other)
        End Sub

        Public Sub Method6(Arg As String)
            Dim localObj = Obj
            Monitor.Enter(localObj) ' Noncompliant
            Console.WriteLine(Arg.Length)
            If Condition Then Monitor.Exit(localObj)
        End Sub

        Public Sub Method7(Arg As String)
            Dim localObj = Obj
            Monitor.Enter(Obj) ' FN
            Console.WriteLine(Arg.Length)
            If Condition Then Monitor.Exit(localObj)
        End Sub

        Public Sub Method8(Arg As String, paramObj As Object)
            paramObj = Obj
            Monitor.Enter(Obj) ' FN
            Console.WriteLine(Arg.Length)
            If Condition Then Monitor.Exit(paramObj)
        End Sub

        Public Sub Method9(Arg As String, paramObj As Object)
            Monitor.Enter(Obj)
            Console.WriteLine(Arg.Length)
            If Condition Then Monitor.Exit(paramObj)
        End Sub

        Public Sub Method10(Arg As String, p1 As Program)
            Monitor.Enter(p1.PublicObject) ' fn
            console.writeline(Arg.Length)
            If Condition Then Monitor.Exit(p1.PublicObject)
        End Sub

        Public Sub Method11(Arg As String, p1 As Program, p2 As Program)
            Monitor.Enter(p1.PublicObject)
            Console.WriteLine(Arg.Length)
            If Condition Then Monitor.Exit(p2.PublicObject)
        End Sub

        Public Sub Method12()
            Dim GetObj As func(Of Object) = Function() Obj
            Monitor.Enter(GetObj()) ' FN
            If Condition Then Monitor.Exit(GetObj())
        End Sub

        Public Sub Method13()
            Monitor.Enter(Obj) ' FN
            Dim A As Action = Sub() Monitor.Exit(Obj)
            If Condition Then A()
        End Sub

        Public Sub Method14()
            Monitor.Enter(Obj) ' Compliant
            If Condition Then
                Monitor.Exit(Obj)
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method15(Arg As String)
            Monitor.Enter(Obj) ' FN, because arg.Length can throw NullReferenceException
            If Arg.Length = 16 Then
                Monitor.Exit(Obj)
            ElseIf Arg.Length = 23 Then
                Monitor.Exit(Obj)
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method15_SafeCondition(A As Boolean, B As Boolean)
            Monitor.Enter(Obj) ' Compliant
            If A Then
                Monitor.Exit(Obj)
            ElseIf B Then
                Monitor.Exit(Obj)
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method16(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            If Arg.Length = 16 Then
                Monitor.Exit(Obj)
            ElseIf Arg.Length = 23 Then
                Monitor.Exit(Obj)
            Else
            End If
        End Sub

        Public Sub Method17(AnotherCondition As Boolean)
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then
                If Not AnotherCondition Then Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method18(Condition1 As Boolean, Condition2 As Boolean)
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then
                Select Case Condition1
                    Case True
                        If Not Condition2 Then
                            Monitor.Exit(Obj)
                        Else
                            Monitor.Exit(Obj)
                        End If
                End Select
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method19()
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then
                Monitor.Exit(Obj)
            Else
                Monitor.Exit(Other)
            End If
        End Sub

        Public Sub Method20(First As Program, Second As Program)
            Monitor.Enter(First.Obj) ' FN
            Monitor.Exit(Second.Obj)
            If Condition Then Monitor.Exit(First.Obj)
        End Sub

        Public Sub Method21(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant
            If Arg.Length = 16 Then
                Monitor.Exit(Obj)
            ElseIf Arg.Length = 23 Then
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method22()
            Dim IsAcquired As Boolean
            Monitor.Enter(Obj, IsAcquired)
            If IsAcquired Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method23(AnotherCondition As Boolean)
            Monitor.Enter(Obj) ' Noncompliant
            If Condition Then
                If Not AnotherCondition Then Monitor.Exit(Obj)
            Else
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method24(Condition As Boolean)
            Dim IsAcquired As Boolean
            Dim somethingElse As New Object()
            Monitor.Enter(Obj, isAcquired)

            If Not isAcquired Then
                Monitor.Enter(somethingElse) ' Noncompliant
                If Condition Then Monitor.Exit(somethingElse)
            End If
        End Sub

        Public Sub SameObject_SameField(Arg As Program)
            Monitor.Enter(Arg.Obj) ' FN, because we are Not field sensitive
            If Condition Then Monitor.Exit(Arg.Obj)
        End Sub

        Public WriteOnly Property MyProp As Integer
            Set(value As Integer)
                Monitor.Enter(Obj) ' Noncompliant
                If value = 42 Then Monitor.Exit(Obj)
            End Set
        End Property

        Public Sub Lambda()
            Dim l = Function(value)
                        Monitor.Enter(Obj) ' Noncompliant
                        If value = 42 Then Monitor.Exit(Obj)
                        Return value
                    End Function
        End Sub

        Public Sub Method13(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub FieldReference_WithThis(Arg As String)
            Monitor.Enter(Me.Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Me.Obj)
        End Sub

        Public Sub FieldReference_WithThis_Mixed1(Arg As String)
            Monitor.Enter(Me.Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub FieldReference_WithThis_Mixed2(Arg As String)
            Monitor.Enter(Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Me.Obj)
        End Sub

        Public Sub StaticFieldReference(Arg As String)
            Monitor.Enter(StaticObj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(StaticObj)
        End Sub

        Public Sub StaticFieldReference_Class(Arg As String)
            Monitor.Enter(Program.StaticObj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Program.StaticObj)
        End Sub

        Public Sub Method13_LocalVar(Arg As String)
            Dim l As New Object()
            Monitor.Enter(l) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(l)
        End Sub

        Public Sub Method13_Parameter(Arg As Object)
            Monitor.Enter(Arg) ' Noncompliant
            If Condition Then Monitor.Exit(Arg)
        End Sub

        Public Sub AutoInitializedInt()
            Dim i As Integer
            Monitor.Enter(Obj) ' Compliant, because exit is never reached
            If i <> 0 AndAlso Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub AutoInitializedPointeger()
            Dim i As IntPtr
            Monitor.Enter(Obj) ' Compliant, because exit is never reached
            If i <> 0 AndAlso Condition Then Monitor.Exit(Obj)
        End Sub

    End Class

End Namespace
