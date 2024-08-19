Imports System.Threading

Namespace Mutex_Type

    '    https://docs.microsoft.com/en-us/dotnet/standard/threading/mutexes
    Class Foo

        Private Cond As Boolean
        Public InstanceMutex As Mutex
        Public Shared StaticMutex As Mutex

        Public Sub Noncompliant(Foo As Foo)
            Dim m0WasCreated, m2WasCreated As Boolean
            Dim m0 = New Mutex(True, "bar", m0WasCreated) ' Noncompliant

            Dim m1 = New Mutex(False)
            m1.WaitOne() ' Noncompliant

            Dim m2 = New Mutex(False, "qix", m2WasCreated)
            m2.WaitOne() ' Noncompliant

            Dim m3 = Mutex.OpenExisting("x")
            m3.WaitOne() ' Noncompliant

            Foo.InstanceMutex.WaitOne() ' FN

            Foo.StaticMutex.WaitOne() ' Noncompliant

            If Cond Then
                m0.ReleaseMutex()
                m1.ReleaseMutex()
                m2.ReleaseMutex()
                m3.ReleaseMutex()
                Foo.InstanceMutex.ReleaseMutex()
                Foo.StaticMutex.ReleaseMutex()
            End If

            ' Note that Dispose() closes the underlying WaitHandle, but does Not release the mutex
            m0.Dispose()
            m1.Dispose()
            m2.Dispose()
            m3.Dispose()
        End Sub

        Public Sub Noncompliant2(ParamMutex As Mutex, ParamMutex2 As Mutex)

            ' 'true' means it owns the mutex if no exception gets thrown
            Using mutexInUsing As New Mutex(True, "foo") ' Noncompliant
                If Cond Then mutexInUsing.ReleaseMutex()
            End Using

            Dim mutexInOutVar As Mutex
            If Mutex.TryOpenExisting("y", mutexInOutVar) Then
                mutexInOutVar.WaitOne() ' Noncompliant
                If Cond Then mutexInOutVar.ReleaseMutex()
            End If

            Dim m = New Mutex(False)
            Dim mIsAcquired = m.WaitOne(200, True)
            If mIsAcquired Then
                ' here it should be released
            Else
                m.ReleaseMutex() ' This is a programming mistake, not detected by this rule
            End If

            Dim paramMutexIsAcquired = ParamMutex.WaitOne(400, False) ' Noncompliant
            If paramMutexIsAcquired Then
                If Cond Then
                Else
                    ParamMutex.ReleaseMutex()
                End If
            End If
            While ParamMutex2.WaitOne(400, False) ' Noncompliant
                If Cond Then ParamMutex2.ReleaseMutex()
            End While
        End Sub

        Public Sub NoncompliantReleasedThenAcquiredAndReleased(ParamMutex As Mutex)
            ParamMutex.ReleaseMutex()
            ParamMutex.WaitOne() ' Noncompliant
            If Cond Then ParamMutex.ReleaseMutex()
        End Sub

        Public Sub DifferentInstancesOnThis(Foo As Foo)
            Foo.InstanceMutex.WaitOne() ' Compliant
            InstanceMutex.WaitOne() ' Noncompliant
            If Cond Then InstanceMutex.ReleaseMutex()
        End Sub

        Public Sub DifferentInstancesOnParameter(Foo As Foo)
            Foo.InstanceMutex.WaitOne() ' FN
            InstanceMutex.WaitOne() ' Compliant
            If Cond Then Foo.InstanceMutex.ReleaseMutex()
        End Sub

        Public Sub UnsupportedWaitAny(m1 As Mutex, m2 As Mutex, m3 As Mutex)
            ' it Is too complex to support this scenario
            Dim WHandles() As WaitHandle = {m1, m2, m3}
            Dim Index = WaitHandle.WaitAny(WHandles) ' FN
            ' the mutex at the given index should be released
            Dim acquiredMutex = DirectCast(WHandles(Index), Mutex)
            If Cond Then acquiredMutex.ReleaseMutex()
        End Sub

        Public Sub UnsupportedWaitAll(m1 As Mutex, m2 As Mutex, m3 As Mutex)
            ' it Is too complex to support this scenario
            Dim WHandles() As WaitHandle = {m1, m2, m3}
            Dim allHaveBeenAcquired = WaitHandle.WaitAll(WHandles) ' FN
            If allHaveBeenAcquired Then
                ' all indexes should be released
                If Cond Then DirectCast(WHandles(0), Mutex).ReleaseMutex()
            End If
        End Sub

        Public Sub CompliantAcquiredNotReleased(ParamMutex As Mutex, Foo As Foo)
            Dim m0WasCreated, m2WasCreated As Boolean, m4 As Mutex
            Using m As New Mutex(True, "foo")
                ' do stuff
            End Using

            Dim m0 = New Mutex(True, "bar", m0WasCreated)
            m0.Dispose()

            Dim m1 = New Mutex(False)
            m1.WaitOne()
            m1.Dispose()

            Dim m2 = New Mutex(False, "qix", m2WasCreated)
            m2.WaitOne()
            m2.Dispose()

            Dim m3 = Mutex.OpenExisting("x")
            m3.WaitOne()

            If Mutex.TryOpenExisting("y", m4) Then m4.WaitOne()

            Dim isAcquired = ParamMutex.WaitOne(400, False)
            If isAcquired Then
                ' Not released
            End If

            Foo.InstanceMutex.WaitOne()
            Foo.StaticMutex.WaitOne()
        End Sub

        Public Sub CompliantNotAcquired(ParamMutex As Mutex)
            Dim m1 As New Mutex(False)
            Dim m2 As Mutex = Mutex.OpenExisting("foo")
            Dim m3 As Mutex
            If Mutex.TryOpenExisting("foo", m3) Then
                ' do stuff but don't acquire
            End If
            Dim MutexWasCreated As Boolean
            Dim m4 As New Mutex(False, "foo", MutexWasCreated)
            If ParamMutex IsNot Nothing Then
                ' do stuff but don't acquire
            End If
        End Sub

        Public Sub CompliantAcquiredAndReleased(ParamMutex As Mutex, Foo As Foo)
            Dim m1 As New Mutex(False)
            m1.WaitOne()
            m1.ReleaseMutex()

            Dim m2 = Mutex.OpenExisting("foo")
            If m2.WaitOne(500) Then
                m2.ReleaseMutex()
            End If

            Dim isAcquired = ParamMutex.WaitOne(400, False)
            If isAcquired Then ParamMutex.ReleaseMutex()

            If ParamMutex.WaitOne(400, False) Then
                ParamMutex.ReleaseMutex()
            End If

            Foo.InstanceMutex.WaitOne()
            If Cond Then
                Foo.InstanceMutex.ReleaseMutex()
            Else
                Foo.InstanceMutex.ReleaseMutex()
            End If

            While Cond
                Foo.StaticMutex.WaitOne()
                Foo.StaticMutex.ReleaseMutex()
            End While
        End Sub

        Public Sub ReleasedThenAcquired(ParamMutex As Mutex)
            ParamMutex.WaitOne() ' Noncompliant
            If Cond Then ParamMutex.ReleaseMutex()
        End Sub

        Public Sub CompliantComplex(MutexName As String, ShouldAcquire As Boolean)
            Dim M As Mutex, Acquired As Boolean = False
            Try
                M = Mutex.OpenExisting(MutexName)
                If ShouldAcquire Then
                    M.WaitOne()     ' Compliant, depends on tracking Null constraint for 'm'
                    Acquired = True
                End If
            Catch ex As UnauthorizedAccessException
                Return
            Finally
                If M Is Nothing Then
                    ' can enter also if an exception was thrown when Waiting
                    If Acquired Then M.ReleaseMutex()
                    M.Dispose()
                End If
            End Try
        End Sub

        Public Sub MutexAquireByConstructor_SimpleAssignment_LiteralArgument()
            Dim MutexCreated As Boolean
            Dim m = New Mutex(True, "bar", MutexCreated) ' Noncompliant
            If Cond Then m.ReleaseMutex()
        End Sub

        Public Sub MutexAquireByConstructor_SimpleAssignment_FieldArgument()
            Dim MutexCreated As Boolean
            Dim m = New Mutex(Cond, "bar", MutexCreated)
            If Cond Then m.ReleaseMutex()
        End Sub

        Public Sub MutexAquireByConstructor_ReAssignment()
            Dim mc1, mc2 As Boolean
            Dim m As New Mutex(False, "bar", mc1)
            m = New Mutex(True, "bar", mc2) ' Noncompliant
            If Cond Then m.ReleaseMutex()
        End Sub

        Public Sub MutexAquireByConstructor_MultipleVariableDeclaration()
            Dim MutexCreated As Boolean
            Dim m0, m1 As New Mutex(True, "bar", MutexCreated) ' Noncompliant
            If Cond Then m0.ReleaseMutex()
        End Sub

    End Class

End Namespace
