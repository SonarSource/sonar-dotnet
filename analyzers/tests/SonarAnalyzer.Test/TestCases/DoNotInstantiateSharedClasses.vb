Imports System.ComponentModel.Composition

Namespace Tests.Diagnostics
    <PartCreationPolicy(CreationPolicy.Shared)>
    Class SharedClass
    End Class

    <System.ComponentModel.Composition.PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)>
    Class SharedClassFullNamespace
    End Class

    <PartCreationPolicy(CreationPolicy.NonShared)>
    Class NonSharedClass
    End Class

    <PartCreationPolicy(CreationPolicy.Any)>
    Class AnyClass
    End Class

    ' Error@+1 [BC30451] - Foo doesn't exist
    <PartCreationPolicy(Foo)>
    Class InvalidAttrParameter
    End Class

    ' Error@+1 [BC30455] Argument not specified for parameter 'creationPolicy' of ...
    <PartCreationPolicy()>
    Class NoAttrParameter
    End Class

    Class NoAttr
    End Class

    Class Program
        Sub Foo()
            Dim x1 = New SharedClass() ' Noncompliant {{Refactor this code so that it doesn't invoke the constructor of this class.}}
'                    ^^^^^^^^^^^^^^^^^
            Dim x2 = New SharedClassFullNamespace() ' Noncompliant
            Dim x3 = New NonSharedClass()
            Dim x4 = New AnyClass()
            Dim x5 = New InvalidAttrParameter()
            Dim x6 = New NoAttrParameter()
            Dim x7 = New NoAttr()
        End Sub
    End Class
End Namespace
