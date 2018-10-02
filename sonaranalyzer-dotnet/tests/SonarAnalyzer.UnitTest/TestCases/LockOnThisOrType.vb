Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class LockOnThisOrType
        Public Sub MyLockingMethod()
            SyncLock Me ' Noncompliant {{Lock on a new 'object' instead.}}
'                    ^^
            End SyncLock

            SyncLock lockObj
            End SyncLock

            SyncLock GetType(LockOnThisOrType) ' Noncompliant
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^
            End SyncLock

            SyncLock (New LockOnThisOrType()).[GetType]() ' Noncompliant
'                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            End SyncLock
        End Sub

        Dim lockObj As New Object()
    End Class
End Namespace
