Public Class Implicit_return_statements_are_noncompliant
    Public Function Assigned_return_value_only() As Integer
        Assigned_return_value_only = 42 ' Noncompliant ^9#26
    End Function

    Public Function explictly_return_default_return_value() As Integer
        explictly_return_default_return_value = 42  ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
        Return explictly_return_default_return_value ' Noncompliant {{Do not make use of the implicit return value.}}
    End Function

    Public Function case_insensitive() As Integer
        CASE_INSENSITIVE = 42  ' Noncompliant
    End Function

    Public Shared Function Static_function() As Integer
        Static_function = 42  ' Noncompliant
    End Function
	
	Public Function read_return_value_only() As Integer
		dim value = read_return_value_only ' Noncompliant {{Do not make use of the implicit return value.}}
		return value
	End Function

End Class

Public Class Does_not_apply_on
    Public Function function_with_explict_return_only()
        Return 42 'Compliant
    End Function

    Public Sub sub_methods(number As Integer)
        Dim Function_name = number ' Compliant
    End Sub

    Public Function recursive_function_calls(number As Integer)
        If number = 42 Then
            Return 42
        Else
            Return recursive_function_calls(42) ' Compliant, method call
        End If
    End Function
	
	Public Function call_to_other_property(other As OtherType) As Integer
        With other
            Return .call_to_other_property ' Compliant
        End With
    End Function

    Public Function call_to_other_function(other As OtherType) As Integer
        With other
            Return .call_to_other_function() ' Compliant
        End With
    End Function

    Public Function write_to_other_property() As OtherType
        Return New OtherType With
        {
            .write_to_other_property = 69 ' Compliant
        }
    End Function
	
	Public Function write_to_other_function(other As OtherType) As Integer
        With other
            Return .write_to_other_function(42)
        End With
    End Function
	
End Class

Public Class OtherType
    Public Property call_to_other_property As Integer
	
	Public Property write_to_other_property As Integer

    Public Function call_to_other_function() As Integer
        Return 42
    End Function
	
	Public Function write_to_other_function(number as Integer) As Integer
        Return number
    End Function
End Class
