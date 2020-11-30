Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class LockOnThisOrType
        Dim MyStringField As String

        Public Sub MyLockingMethod()
            SyncLock Me ' Noncompliant {{Lock on a dedicated object instance instead.}}
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

            SyncLock ("foo") ' Noncompliant
            End SyncLock

            SyncLock (MyStringField) ' Noncompliant

            End SyncLock

        End Sub

        Dim lockObj As New Object()
    End Class
End Namespace
