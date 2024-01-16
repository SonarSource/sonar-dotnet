Imports System.Threading

Namespace ReaderWriterLockSlim_Type

    Class Program

        Private Condition As Boolean
        Private rwLockSlim As New ReaderWriterLockSlim()

        Public Sub Method1()
            rwLockSlim.EnterReadLock() ' Noncompliant
            If Condition Then rwLockSlim.ExitReadLock()
        End Sub

        Public Sub Method2()
            rwLockSlim.EnterWriteLock() ' Noncompliant
            If Condition Then rwLockSlim.ExitWriteLock()
        End Sub

        Public Sub Method3()
            rwLockSlim.EnterUpgradeableReadLock() ' Noncompliant
            If Condition Then rwLockSlim.ExitUpgradeableReadLock()
        End Sub

        Public Sub Method4()
            If rwLockSlim.TryEnterReadLock(42) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub Method5()
            If rwLockSlim.TryEnterReadLock(New TimeSpan(42)) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub Method6()
            If rwLockSlim.TryEnterWriteLock(42) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitWriteLock()
            End If
        End Sub

        Public Sub Method7()
            If rwLockSlim.TryEnterWriteLock(New TimeSpan(42)) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitWriteLock()
            End If
        End Sub

        Public Sub Method8()
            If rwLockSlim.TryEnterUpgradeableReadLock(42) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub Method9()
            If rwLockSlim.TryEnterUpgradeableReadLock(New TimeSpan(42)) AndAlso Condition Then ' Noncompliant
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub Method10()
            Try
                rwLockSlim.EnterUpgradeableReadLock()
                rwLockSlim.EnterWriteLock() ' Compliant
                If Condition Then rwLockSlim.ExitWriteLock()
            Catch ex As Exception
                Throw
            Finally
                rwLockSlim.ExitUpgradeableReadLock()
            End Try
        End Sub

        Public Sub Method11(Arg As String)
            Try
                rwLockSlim.EnterUpgradeableReadLock()
                rwLockSlim.EnterWriteLock()
                Try
                    Console.WriteLine(Arg.Length)
                Catch ex As Exception
                    Throw
                Finally
                    rwLockSlim.ExitWriteLock()
                End Try
            Catch ex As Exception
                Throw
            Finally
                rwLockSlim.ExitUpgradeableReadLock()
            End Try
        End Sub

        Public Sub Method12()
            rwLockSlim.EnterReadLock() ' Compliant
            rwLockSlim.ExitReadLock()
        End Sub

        Public Sub Method13()
            rwLockSlim.EnterReadLock() ' Compliant, this rule doesn't care if it was released with correct API
            rwLockSlim.ExitWriteLock()
        End Sub

        Public Sub WrongOrder()
            rwLockSlim.ExitReadLock()
            rwLockSlim.EnterReadLock()  ' Compliant, source of FPs on Peach

            Dim a = New ReaderWriterLockSlim()
            a.ExitWriteLock()
            a.EnterWriteLock()

            Dim b = New ReaderWriterLockSlim()
            b.ExitUpgradeableReadLock()
            b.TryEnterReadLock(1)

            Dim c = New ReaderWriterLockSlim()
            c.ExitReadLock()
            c.TryEnterWriteLock(1)

            Dim d = New ReaderWriterLockSlim()
            d.ExitReadLock()
            d.EnterUpgradeableReadLock()

            Dim e = New ReaderWriterLockSlim()
            e.ExitReadLock()
            e.TryEnterUpgradeableReadLock(1)
        End Sub

        Public Sub Method14()
            rwLockSlim.EnterReadLock() ' Noncompliant, this rule doesn't care if it was released with correct API
            If Condition Then rwLockSlim.ExitWriteLock()
        End Sub

        Public Sub Method15()
            If rwLockSlim.TryEnterReadLock(42) Then
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub IsReadLockHeld()
            rwLockSlim.EnterReadLock()          ' Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            If rwLockSlim.IsReadLockHeld Then rwLockSlim.ExitReadLock()
        End Sub

        Public Sub IsReadLockHeld_NoLocking()
            If rwLockSlim.IsReadLockHeld Then   'Noncompliant
                If Condition Then rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub IsReadLockHeld_NoLocking_Compliant()
            If rwLockSlim.IsReadLockHeld Then rwLockSlim.ExitReadLock()
        End Sub

        Public Sub IsReadLockHeld_Noncompliant()
            rwLockSlim.EnterReadLock()
            If rwLockSlim.IsReadLockHeld Then   ' Noncompliant
                If Condition Then rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub IsReadLockHeld_Noncompliant(Arg As Boolean)
            If Arg Then rwLockSlim.EnterReadLock()
            If rwLockSlim.IsReadLockHeld Then       ' Noncompliant
                If Condition Then rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub IsReadLockHeld_Unreachable()
            rwLockSlim.EnterReadLock()
            If rwLockSlim.IsReadLockHeld Then       ' Noncompliant, ends up unreleased on If path, and released on Else path
                ' Nothing
            Else
                rwLockSlim.ExitReadLock()
            End If
        End Sub

        Public Sub IsWriteLockHeld()
            rwLockSlim.EnterWriteLock()             ' Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            If rwLockSlim.IsWriteLockHeld Then rwLockSlim.ExitWriteLock()
        End Sub

        Public Sub IsWriteLockHeld_Noncompliant()
            If rwLockSlim.IsWriteLockHeld Then      ' Noncompliant
                If Condition Then rwLockSlim.ExitWriteLock()
            End If
        End Sub

        Public Sub IsUpgradeableReadLockHeld_Noncompliant()
            If rwLockSlim.IsUpgradeableReadLockHeld Then    ' Noncompliant
                If Condition Then rwLockSlim.ExitReadLock()
            End If
        End Sub

    End Class

End Namespace
