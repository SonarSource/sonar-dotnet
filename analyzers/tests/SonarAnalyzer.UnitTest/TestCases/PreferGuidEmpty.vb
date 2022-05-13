Imports System.Collections.Generic

Class Compliant

    Private NullableField As Guid? ' Compliant
    Private ReadOnly ReadonlyNullableField As Guid? ' Compliant
    Property Prop As Guid ' Compliant

    Sub Empty()
        Dim empty As Guid = Guid.Empty ' Compliant
    End Sub

    Sub NotEmpty()
        Dim rnd As Guid = Guid.NewGuid() ' Compliant
        Dim bytes As Guid = New Guid({0, 3, 4}) ' Compliant
        Dim str As Guid = New Guid("FA97FFE7-532C-4015-8698-49D8CE4126F4") ' Compliant
    End Sub

    Sub NullableDefault()
        Dim nullable As Guid? = Nothing ' Compliant, not equivalent to Guid.Empty.
        Dim guidAsParameter As New NullableGuidClass(Nothing) ' Compliant
        guidAsParameter.Method(Nothing) ' Compliant
    End Sub

    Sub NotInitialized(str As String)
        Dim parsed As Guid ' Compliant
        Guid.TryParse(str, parsed)
    End Sub

    Sub OptionalParameter(Optional guid As Guid = Nothing) ' Compliant, default has to be a run-time constant
    End Sub

    Sub NotGuid()
        Dim int As Integer = Nothing ' Compliant
        Dim dat As Date = Nothing ' Compliant
    End Sub

    Function DoesNotCrashOnDictionaryCreation() As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {{"a", "b"}, {"c", "d"}}
    End Function

End Class

Class NonCompliant

    Private Field As Guid
    Private ReadOnly ReadonlyField As Guid
    Property Prop As Guid

    Sub DefaultCtor()
        Dim ctor As Guid = New Guid() ' Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.}}
        '                  ^^^^^^^^^^
        Dim asignend As New Guid() ' Noncompliant
    End Sub

    Sub DefaultInitialization()
        Dim defaultValue As Guid = Nothing ' Noncompliant
        Dim unasignend As Guid ' FN
        Dim guidAsParameter As New GuidClass(Nothing) ' Noncompliant
        guidAsParameter.Method(Nothing) ' Noncompliant
        Prop = Nothing ' Noncompliant
        Field = Nothing ' Noncompliant
    End Sub

    Sub EmptyString()
        Dim ctor1 As Guid = New Guid("00000000-0000-0000-0000-000000000000") ' Noncompliant
        Dim ctor2 As New Guid("00000000-0000-0000-0000-000000000000") ' Noncompliant
        Dim parse As Guid = Guid.Parse("00000000-0000-0000-0000-000000000000") ' FN
    End Sub

End Class

Structure GuidAssignmentStruct

    Private Shared ReadOnly StaticField As Guid
    Private Field As Guid                   ' Compliant, structs do not allow assigned instance values
    Private ReadOnly ReadOnlyField As Guid  ' Compliant, structs do not allow assigned instance values

End Structure

Class GuidClass

    Public Sub New(param As Guid)
    End Sub

    Public Sub Method(param As Guid)
    End Sub

End Class

Class NullableGuidClass

    Public Sub New(param As Guid?)
    End Sub

    Public Sub Method(param As Guid?)
    End Sub

End Class
