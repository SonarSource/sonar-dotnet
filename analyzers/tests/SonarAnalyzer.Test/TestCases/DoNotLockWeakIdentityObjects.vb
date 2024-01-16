Imports System
Imports System.Reflection
Imports System.Threading

Public Class DoNotLockWeakIdentityObjects

    Private synchronized As New Object()
    Private marshalByRefObject As MarshalByRefObject = New Timer(Nothing)
    Private marshalByRefObjectDerivate As New Timer(Nothing)
    Private executionEngineException As New ExecutionEngineException()
    Private outOfMemoryException As New OutOfMemoryException()
    Private stackOverflowException As New StackOverflowException()
    Private aString As String = "some value"
    Private memberInfo As MemberInfo = GetType(String).GetProperty("Length")
    Private parameterInfo As ParameterInfo = GetType(String).GetMethod("Equals").ReturnParameter
    Private thread As New Thread(DirectCast(Nothing, ThreadStart))

    Public Sub Test()
        SyncLock synchronized ' Compliant
        End SyncLock

        SyncLock marshalByRefObject ' Noncompliant {{Replace this lock on 'MarshalByRefObject' with a lock against an object that cannot be accessed across application domain boundaries.}}
            '    ^^^^^^^^^^^^^^^^^^
        End SyncLock
        SyncLock marshalByRefObjectDerivate ' Noncompliant {{Replace this lock on 'Timer' with a lock against an object that cannot be accessed across application domain boundaries.}}
        End SyncLock
        SyncLock executionEngineException   ' Noncompliant
        End SyncLock
        SyncLock outOfMemoryException       ' Noncompliant
        End SyncLock
        SyncLock stackOverflowException     ' Noncompliant
        End SyncLock
        SyncLock aString        ' Noncompliant
        End SyncLock
        SyncLock memberInfo     ' Noncompliant
        End SyncLock
        SyncLock parameterInfo  ' Noncompliant
        End SyncLock
        SyncLock thread         ' Noncompliant
        End SyncLock
    End Sub

End Class
