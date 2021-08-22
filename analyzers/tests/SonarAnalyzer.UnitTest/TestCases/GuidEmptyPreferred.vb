Imports System

Class GuidAssignment

    Private NullableField As Guid? ' Compliant
    Private ReadOnly ReadonlyNullableField As Guid? ' Compliant
    Property Prop as Guid ' Compliant

    Sub Allowed()
        Dim empty As Guid = Guid.Empty ' Compliant
        Dim rnd As Guid = Guid.NewGuid() ' Compliant
        Dim bytes As Guid = New Guid({0, 3, 4}) ' Compliant
        Dim str As Guid = New Guid("FA97FFE7-532C-4015-8698-49D8CE4126F4") ' Compliant
        Dim nullable As Guid? = Nothing ' Compliant, not equivalent to Guid.Empty.
		Dim instance As New NullableGuidClass(Nothing) ' Compliant
		instance.Method(Nothing) ' Compliant

        Dim parsed As Guid ' Compliant
        Guid.TryParse("FA97FFE7-532C-4015-8698-49D8CE4126F4", parsed)
    End Sub

    Private Field As Guid ' Noncompliant
    Private ReadOnly ReadonlyField As Guid ' Noncompliant

    Sub NotAllowed()
        Dim ctor As Guid = New Guid() ' Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.}}
        '                  ^^^^^^^^^^
        Dim defaultValue As Guid = Nothing ' Noncompliant
        Dim emptyString As Guid = New Guid("00000000-0000-0000-0000-000000000000") ' Noncompliant
        Dim unasignend As Guid ' Noncompliant
		Dim asignend As New Guid() ' Noncompliant
        Prop = Nothing ' Noncompliant
		Dim instance As New GuidClass(Nothing) ' Noncompliant
		instance.Method(Nothing) ' Noncompliant
    End Sub
End Class

Structure GuidAssignmentStruct
    Private Shared ReadOnly StaticField As Guid ' Noncompliant
    Private Field As Guid ' Compliant, structs Do Not allow assigned instance values
    Private ReadOnly ReadOnlyField As Guid ' Compliant, structs Do Not allow assigned instance values
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