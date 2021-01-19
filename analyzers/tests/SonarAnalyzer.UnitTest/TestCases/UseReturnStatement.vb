Public Class Implicit_return_statements_are_noncompliant
    Public Function Assigned_return_value_only() As Integer
        Assigned_return_value_only = 42 ' Noncompliant {{Use a 'Return' statement; assigning returned values to function names is obsolete.}}
'       ^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Function

    Public Function Explictly_return_default_return_value() As Integer
        Explictly_return_default_return_value = 42  ' Noncompliant
        Return Explictly_return_default_return_value ' Noncompliant
    End Function

    Public Function Case_insensitive() As Integer
        CASE_INSENSITIVE = 42  ' Noncompliant
    End Function

    Public Shared Function Static_function() As Integer
        Static_function = 42  ' Noncompliant
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

    Public Function call_to_other_function(other As OtherType) As Integer
        With other
            Return .call_to_other_function ' Compliant
        End With
    End Function

    Public Function call_to_other_property() As OtherType
        Return New OtherType With
        {
            .call_to_other_property = 69 ' Compliant
        }
    End Function
End Class

Public Class OtherType
    Public Property call_to_other_property As Integer

    Public Function call_to_other_function() As Integer
        Return 42
    End Function
End Class
