Imports System.Runtime.CompilerServices

Namespace S4225.ExtensionMethodShouldNotExtendObject

    Module Compliant
        <Extension>
        Sub Extends(i As Integer)
        End Sub
        
        <Extension>
        Function Extends(i As Integer, n As Integer) as Integer
            Return i + n
        End Function

        Sub NotAnExtension(obj As Object)
        End Sub
        
        Function NotAnExtension(obj As Object, other as Object) as Object
            return other
        End Function
    End Module

    Module Noncompliant
        <Extension>
        Sub ExtendsObject(obj As Object) ' Noncompliant {{Refactor this extension to extend a more concrete type.}}
'           ^^^^^^^^^^^^^
        End Sub
        
        <Extension>
        Function ExtendsObject(obj As Object, other as Object) as Object ' Noncompliant
            Return other
        End Function
    End Module

End Namespace
