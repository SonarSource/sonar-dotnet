Public Class ImplicitReturnStatementsAreNoncompliant
    Public Function AssignedReturnValueOnly() As Integer
        AssignedReturnValueOnly = 42 ' Noncompliant ^9#23
    End Function

    Public Function ExplictlyReturnDefaultReturnValue() As Integer
        ExplictlyReturnDefaultReturnValue = 42  ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        Return ExplictlyReturnDefaultReturnValue ' Noncompliant {{Do not make use of the implicit return value.}}
    End Function

    Public Function CaseInsensitive() As Integer
        CASEINSENSITIVE = 42  ' Noncompliant
    End Function

    Public Shared Function SharedFunction() As Integer
        SharedFunction = 42  ' Noncompliant
    End Function
	
	Public Function ReadReturnValueOnly() As Integer
		dim value = ReadReturnValueOnly ' Noncompliant {{Do not make use of the implicit return value.}}
		return value
	End Function

End Class

Public Class DoesNotApplyOn
    Public Function FunctionWithExplictReturnOnly()
        Return 42 'Compliant
    End Function

    Public Sub SubMethods(number As Integer)
        Dim SubMethods = number ' Compliant
    End Sub

    Public Function RecursiveFunctionCalls(number As Integer)
        If number = 42 Then
            Return 42
        Else
            Return RecursiveFunctionCalls(42) ' Compliant, method call
        End If
    End Function
	
	Public Function CallToOtherProperty(other As OtherType) As Integer
        With other
            Return .CallToOtherProperty ' Compliant
        End With
    End Function

    Public Function CallToOtherFunction(other As OtherType) As Integer
        With other
            Return .CallToOtherFunction() ' Compliant
        End With
    End Function

    Public Function WriteToOtherProperty() As OtherType
        Return New OtherType With
        {
            .WriteToOtherProperty = 69 ' Compliant
        }
    End Function
	
	Public Function WriteToOtherFunction(other As OtherType) As Integer
        With other
            Return .WriteToOtherFunction(42)
        End With
    End Function
	
End Class

Public Class OtherType
    Public Property CallToOtherProperty As Integer
	
	Public Property WriteToOtherProperty As Integer

    Public Function CallToOtherFunction() As Integer
        Return 42
    End Function
	
	Public Function WriteToOtherFunction(number as Integer) As Integer
        Return number
    End Function
End Class
