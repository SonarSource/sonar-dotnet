Imports System.Threading

Namespace ReaderWriterLock_Type

    Class Program

        Private Condition As Boolean
        Private rwLock As New ReaderWriterLock()

        Public Sub Method1()
            rwLock.AcquireReaderLock(42) ' Noncompliant
            If Condition Then rwLock.ReleaseReaderLock()
        End Sub

        Public Sub Method2()
            rwLock.AcquireReaderLock(New TimeSpan(42)) ' Noncompliant
            If Condition Then rwLock.ReleaseReaderLock()
        End Sub

        Public Sub Method3()
            rwLock.AcquireWriterLock(42) ' Noncompliant
            If Condition Then rwLock.ReleaseWriterLock()
        End Sub

        Public Sub Method4()
            rwLock.AcquireWriterLock(New TimeSpan(42)) ' Noncompliant
            If Condition Then rwLock.ReleaseWriterLock()
        End Sub

        Public Sub Method5()
            rwLock.AcquireReaderLock(42)
            Try
                Dim Cookie = rwLock.UpgradeToWriterLock(42)
                If Condition Then rwLock.DowngradeFromWriterLock(Cookie)
            Catch ex As Exception
                Throw
            Finally
                rwLock.ReleaseReaderLock()
            End Try
        End Sub

        Public Sub Method6()
            Try
                rwLock.AcquireReaderLock(New TimeSpan(42))
                Dim Cookie = rwLock.UpgradeToWriterLock(New TimeSpan(42))
                If Condition Then rwLock.DowngradeFromWriterLock(Cookie)
            Catch ex As Exception
                Throw
            Finally
                rwLock.ReleaseReaderLock()
            End Try
        End Sub

        Public Sub Method7(Arg As String)
            Try
                rwLock.AcquireReaderLock(42)
                Dim Cookie = rwLock.UpgradeToWriterLock(42)
                Try
                    Console.WriteLine(Arg.Length)
                Catch ex As Exception
                    Throw
                Finally
                    rwLock.DowngradeFromWriterLock(Cookie)
                End Try
            Catch ex As Exception
                Throw
            Finally
                rwLock.ReleaseReaderLock()
            End Try
        End Sub

        Public Sub Method8(Arg As String)
            Try
                rwLock.AcquireReaderLock(New TimeSpan(42))
                Dim Cookie = rwLock.UpgradeToWriterLock(New TimeSpan(42))
                Try
                    Console.WriteLine(Arg.Length)
                Catch ex As Exception
                    Throw
                Finally
                    rwLock.DowngradeFromWriterLock(Cookie)
                End Try
            Catch ex As Exception
                Throw
            Finally
                rwLock.ReleaseReaderLock()
            End Try
        End Sub

        Public Sub Method9()
            rwLock.AcquireReaderLock(New TimeSpan(42)) ' Noncompliant
            Dim Cookie As New LockCookie()
            If Condition Then Cookie = rwLock.ReleaseLock()
            rwLock.RestoreLock(Cookie)
        End Sub

        Public Sub Method10()
            rwLock.AcquireReaderLock(42) ' Compliant
            rwLock.ReleaseReaderLock()
        End Sub

        Public Sub WrongOrder()
            rwLock.ReleaseReaderLock()
            rwLock.AcquireReaderLock(1) ' Compliant, source Of FPs On Peach

            Dim a As New ReaderWriterLock()
            a.ReleaseLock()
            a.AcquireWriterLock(1)

            Dim b As New ReaderWriterLock()
            b.ReleaseWriterLock()
            b.AcquireWriterLock(1)
        End Sub

        Public Sub IsReaderLockHeld()
            rwLock.AcquireReaderLock(42)       ' Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            If rwLock.IsReaderLockHeld Then rwLock.ReleaseReaderLock()
        End Sub

        Public Sub IsReaderLockHeld_NoLocking()
            If rwLock.IsReaderLockHeld Then     'Noncompliant
                If Condition Then rwLock.ReleaseReaderLock()
            End If
        End Sub

        Public Sub IsReaderLockHeld_NoLocking_Compliant()
            If rwLock.IsReaderLockHeld Then rwLock.ReleaseReaderLock()
        End Sub

        Public Sub IsReaderLockHeld_Noncompliant()
            rwLock.AcquireReaderLock(42)
            If rwLock.IsReaderLockHeld Then ' Noncompliant
                If Condition Then rwLock.ReleaseReaderLock()
            End If
        End Sub

        Public Sub IsReaderLockHeld_Noncompliant(Arg As Boolean)
            If Arg Then rwLock.AcquireReaderLock(42)
            If rwLock.IsReaderLockHeld Then             ' Noncompliant
                If Condition Then rwLock.ReleaseReaderLock()
            End If
        End Sub

        Public Sub IsReaderLockHeld_Unreachable()
            rwLock.AcquireReaderLock(42)
            If rwLock.IsReaderLockHeld Then     ' Noncompliant, ends up unreleased on If path, and released on Else path
                ' Nothing
            Else
                rwLock.ReleaseReaderLock()
            End If
        End Sub

        Public Sub IsWriterLockHeld()
            rwLock.AcquireWriterLock(42)        ' Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            If rwLock.IsWriterLockHeld Then rwLock.ReleaseWriterLock()
        End Sub

        Public Sub IsWriterLockHeld_Noncompliant()
            If rwLock.IsWriterLockHeld Then     ' Noncompliant
                If Condition Then rwLock.ReleaseWriterLock()
            End If
        End Sub

    End Class

End Namespace
