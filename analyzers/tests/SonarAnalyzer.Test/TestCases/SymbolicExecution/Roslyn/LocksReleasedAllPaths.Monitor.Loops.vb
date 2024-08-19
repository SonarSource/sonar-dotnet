Imports System.Threading

Namespace Monitor_Loops

    Class Program

        Private Condition As Boolean
        Private Obj As New Object()
        Private Other As New Object()

        Public Sub Method1()
            Monitor.Enter(Obj)      ' Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
            For i As Integer = 0 To 9
                If i = 9 Then Monitor.Exit(Obj)
            Next
            Monitor.Enter(Other)    ' Noncompliant
            If Condition Then Monitor.Exit(Other)
        End Sub

        Public Sub Method2()
            Monitor.Enter(Obj) ' FN
            For i As Integer = 0 To 9
                Exit For
                If i = 9 Then Monitor.Exit(Obj)
            Next
        End Sub

        Public Sub Method3()
            Monitor.Enter(Obj)      ' Noncompliant
            For i As Integer = 0 To 9
                If i = 5 Then Exit For
                If i = 9 Then Monitor.Exit(Obj)
            Next
            Monitor.Enter(Other)    ' Noncompliant
            If Condition Then Monitor.Exit(Other)
        End Sub

        Public Sub Method4()
            Monitor.Enter(Obj)      ' Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
            For i As Integer = 0 To 9
                If i = 10 Then Exit For
                If i = 9 Then Monitor.Exit(Obj)
            Next
            Monitor.Enter(Other)    ' Noncompliant
            If Condition Then Monitor.Exit(Other)
        End Sub

        Public Sub Method5()
            Monitor.Enter(Obj)      ' Compliant, Exit is not reached on any path. Should be covered by S2583.
            For i As Integer = 0 To 9
                If i = 9 Then Continue For
                If i = 9 Then Monitor.Exit(Obj)
            Next
            Monitor.Enter(Other)    ' Noncompliant
            If Condition Then Monitor.Exit(Other)
        End Sub

        Public Sub Method6(Array() As Byte)
            Monitor.Enter(Obj)      ' Noncompliant
            For Each Item As Byte In Array
                If Condition Then Monitor.Exit(Obj)
            Next
        End Sub

        Public Sub Method7(Array() As Byte)
            Monitor.Enter(Obj)      ' Noncompliant, array can be empty
            For Each Item In Array
                Monitor.Exit(Obj)
            Next
        End Sub

        Public Sub Method8(Array As List(Of Byte))
            Monitor.Enter(Obj)      ' Noncompliant
            While Array.Count < 42
                If Condition Then Monitor.Exit(Obj)
                Array.RemoveAt(0)
            End While
        End Sub

        Public Sub Method9(Array As List(Of Byte))
            Monitor.Enter(Obj)      ' Noncompliant, count can be bigger than 42
            While Array.Count < 42
                Monitor.Exit(Obj)
                Array.RemoveAt(0)
            End While
        End Sub

        Public Sub Method10(Array As List(Of Byte))
            Monitor.Enter(Obj)      ' Noncompliant
            Do
                If Condition Then Monitor.Exit(Obj)
                Array.RemoveAt(0)
            Loop While Array.count < 42
        End Sub

        Public Sub Method11(Array As list(Of Byte))
            Monitor.Enter(Obj)      ' Compliant
            Do
                Monitor.Exit(Obj)
                Array.RemoveAt(0)
            Loop While Array.Count < 42
        End Sub

    End Class

End Namespace
