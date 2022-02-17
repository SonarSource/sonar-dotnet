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
            rwLock.AcquireReaderLock(1) ' Noncompliant

            Dim a As New ReaderWriterLock()
            a.ReleaseLock()
            a.AcquireWriterLock(1) ' Noncompliant

            Dim b As New ReaderWriterLock()
            b.ReleaseWriterLock()
            b.AcquireWriterLock(1) ' Noncompliant
        End Sub

    End Class

End Namespace
