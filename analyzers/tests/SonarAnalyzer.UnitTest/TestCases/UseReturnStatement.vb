﻿Public Class UseReturnStatement

    Public Function FunctionName() As Integer
    FunctionName = 42 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
'       ^^^^^^^^^^^^
    End Function

    Public Function ReturnStatement() As Integer
        Return 42 ' Compliant
    End Function

    Public Function ReturnWithVariable() As Integer
        Dim value As Integer = 42
        Return value  ' Compliant
    End Function

    Public Function ReturnWithFunctionName() As Integer
        ReturnWithFunctionName = 42 ' Noncompliant
        Return ReturnWithFunctionName  ' Noncompliant
    End Function

    Public Function WithRecursion(number As Integer) As Integer
        If number = 42 Then
            Return number
        Else
            Return WithRecursion(42) ' Compliant, method call
        End If
    End Function

End Class
