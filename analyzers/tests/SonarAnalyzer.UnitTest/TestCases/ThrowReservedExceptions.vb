Public Class ThrowReservedExceptions
    Public Sub Method1()
        Throw New Exception()                   ' Noncompliant {{'System.Exception' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^
        Throw New ApplicationException()        ' Noncompliant {{'System.ApplicationException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        Throw New SystemException()             ' Noncompliant {{'System.SystemException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^
        Throw New ExecutionEngineException()    ' Noncompliant {{'System.ExecutionEngineException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Throw New IndexOutOfRangeException()    ' Noncompliant {{'System.IndexOutOfRangeException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Throw New NullReferenceException()      ' Noncompliant {{'System.NullReferenceException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Throw New OutOfMemoryException()        ' Noncompliant {{'System.OutOfMemoryException' should not be thrown by user code.}}
        '     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        Dim e = New OutOfMemoryException()  ' Compliant
        Throw New ArgumentNullException()   ' Compliant

        Try
            Dim a = New Integer(-1) {}
            ' Throw exception
            Console.WriteLine(a(1))
        Catch generatedExceptionName As IndexOutOfRangeException ' Compliant
            Throw
        End Try
    End Sub
End Class
