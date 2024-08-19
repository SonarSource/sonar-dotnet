Imports System.Threading

Namespace Monitor_TryEnter

    Class Program

        Private Condition As Boolean
        Private Obj As New Object()

        Public Sub Method1()
            If Monitor.TryEnter(Obj) AndAlso Condition Then ' Noncompliant
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method2()
            If Monitor.TryEnter(Obj) Then
                Monitor.Exit(Obj)
            Else
            End If
        End Sub

        Public Sub Method3()
            If Monitor.TryEnter(Obj, 42) AndAlso Condition Then ' Noncompliant
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method4()
            Dim IsAcquired As Boolean
            Monitor.TryEnter(Obj, 42, IsAcquired) ' Noncompliant
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method5()
            If Monitor.TryEnter(Obj, New TimeSpan(42)) AndAlso Condition Then ' Noncompliant
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method6()
            Dim IsAcquired As Boolean
            Monitor.TryEnter(Obj, New TimeSpan(42), IsAcquired) ' Noncompliant
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method7()
            Dim IsAcquired As Boolean = Monitor.TryEnter(Obj, 42) ' Noncompliant
            If IsAcquired AndAlso Condition Then
                Monitor.Exit(Obj)
            End If
        End Sub

        Public Sub Method8()
            Dim IsAcquired As Boolean = Monitor.TryEnter(Obj, 42)
            If IsAcquired Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method9()
            Monitor.TryEnter(Obj) ' Compliant
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method10()
            Monitor.TryEnter(Obj) ' Noncompliant {{Unlock this lock along all executions paths of this method.}}
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method12()
            Select Case Monitor.TryEnter(Obj) ' Noncompliant
                Case False
                Case Else
                    If Condition Then Monitor.Exit(Obj)
            End Select
        End Sub

        Public Sub Method13()
            Dim IsAcquired As Boolean
            Monitor.TryEnter(Obj, 42, IsAcquired)
            If IsAcquired Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method14(Condition As Boolean)
            Dim IsAcquired As Boolean
            Monitor.TryEnter(Obj, IsAcquired) ' Noncompliant
            If Condition Then Monitor.Exit(Obj)
        End Sub

        Public Sub Method15()
            Dim IsAcquired As Boolean
            Monitor.TryEnter(Obj, IsAcquired)
            If IsAcquired Then Monitor.Exit(Obj)
        End Sub

    End Class

End Namespace
