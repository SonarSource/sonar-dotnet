Imports System.Threading

Namespace SpinLock_Type

    Class Program

        Private Condition As Boolean

        Public Sub Enter_PartialExit()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.Enter(IsAcquired) ' Noncompliant
            If Condition Then sl.Exit()
        End Sub

        Public Sub Enter_ThreadIdTrackingEnabled_PartialExit()
            Dim sl As New SpinLock(True)
            Dim isAcquired As Boolean = False
            sl.Enter(isAcquired) ' Noncompliant
            If Condition Then sl.Exit(True)
        End Sub

        Public Sub TryEnter_ThreadIdTrackingDisabled_PartialExit()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.TryEnter(IsAcquired) ' Noncompliant
            If Condition Then sl.Exit()
        End Sub

        Public Sub TryEnterIntOverload_PartialExit()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.TryEnter(42, IsAcquired) ' Noncompliant
            If Condition Then sl.Exit()
        End Sub

        Public Sub TryEnterTimeSpanOverload_PartialExit()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.TryEnter(New TimeSpan(42), IsAcquired) ' Noncompliant
            If Condition Then sl.Exit()
        End Sub

        Public Sub TryCatchFinally_Compliant(Arg As String)
            Dim IsAcquired As Boolean
            Dim sl As New SpinLock(False)
            sl.Enter(IsAcquired)
            Try
                Console.WriteLine(Arg.Length)
            Finally
                sl.Exit()
            End Try
        End Sub

        Public Sub Enter_UseReturnValueToReleaseOnlyWhenNeeded()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.Enter(IsAcquired)
            If IsAcquired Then sl.Exit()
        End Sub

        Public Sub TryEnter_UseReturnValueToReleaseOnlyWhenNeeded()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.TryEnter(IsAcquired)
            If IsAcquired Then sl.Exit()
        End Sub

        Public Sub Method9()
            Dim sl As New SpinLock(False)
            Dim IsAcquired As Boolean
            sl.Enter(IsAcquired) ' Compliant
            sl.Exit()
        End Sub

    End Class

End Namespace
