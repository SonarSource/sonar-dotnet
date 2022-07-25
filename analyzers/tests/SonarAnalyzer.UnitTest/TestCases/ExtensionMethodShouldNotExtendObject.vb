Imports System.Runtime.CompilerServices

Module Compliant
    <Extension>
    Sub ExtendsValueType(i As Integer)
    End Sub

    <Extension>
    Function ExtendsWithArguments(i As Integer, n As Integer) As Integer
        Return i + n
    End Function

    <Extension>
    Sub ExtendsReferenceType(obj As Exception)
    End Sub

    Sub NotAnExtension(obj As Object)
    End Sub

    Function NotAnExtension(obj As Object, other As Object) As Object
        Return other
    End Function
End Module

Module Noncompliant
    <Extension>
    Sub ExtendsObject(obj As Object) ' Noncompliant {{Refactor this extension to extend a more concrete type.}}
'       ^^^^^^^^^^^^^
    End Sub

    <Extension>
    Function ExtendsObject(obj As Object, other As Object) As Object ' Noncompliant
        Return other
    End Function

    <ExtensionAttribute>
    Sub ExtendsWithLongName(obj As Object) ' Noncompliant
    End Sub

    <Extension>
    <DebuggerStepThrough>
    Public Sub MultipleAttributes(obj As Object) ' Noncompliant
    End Sub

    <Extension, DebuggerStepThrough>
    Public Sub MultipleConbinedAttributes(obj As Object) ' Noncompliant
    End Sub

    <System.Runtime.CompilerServices.ExtensionAttribute>
    Public Sub FullName(Arg As Object)          ' Noncompliant
    End Sub

    <Runtime.CompilerServices.ExtensionAttribute>
    Public Sub ImplicitSystem(Arg As Object)    ' Noncompliant
    End Sub

    <System.Runtime.CompilerServices.Extension>
    Public Sub ImplicitAttribute(Arg As Object) ' Noncompliant
    End Sub

    <Runtime.CompilerServices.Extension>
    Public Sub ImplicitSystemAndAttribute(Arg As Object)    ' Noncompliant
    End Sub

End Module
