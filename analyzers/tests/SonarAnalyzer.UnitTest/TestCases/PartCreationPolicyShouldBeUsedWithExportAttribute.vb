Imports System
Imports System.ComponentModel.Composition

Namespace Tests.TestCases
    <Export(GetType(Object))>
    <PartCreationPolicy(CreationPolicy.Any)> ' Compliant, Export is present
    class Program1

    End Class

    <InheritedExport(GetType(Object))>
    <PartCreationPolicy(CreationPolicy.Any)> ' Compliant, InheritedExport is present
    class Program2_Base

    End Class

    <PartCreationPolicy(CreationPolicy.Any)> ' Compliant, InheritedExport is present in base
    class Program2
        inherits Program2_Base

    End Class

    <PartCreationPolicy(CreationPolicy.Any)> ' Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this type definition.}}
    class Program3
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^@-1

    End Class

    <PartCreationPolicy(CreationPolicy.Any)> ' Noncompliant, Export is not inherited
    class Program4
        inherits Program1

    End Class

    <MyExportAttribute>
    <PartCreationPolicy(CreationPolicy.Any)>
    Class Program6
    End Class

    <MyInheritedExport(GetType(Object))>
    <PartCreationPolicy(CreationPolicy.Any)>
    Class Program7_Base
    End Class

    <PartCreationPolicy(CreationPolicy.Any)>
    Class Program7
        Inherits Program2_Base
    End Class

    <Foo>
    <PartCreationPolicy(CreationPolicy.Any)> ' Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this type definition.}}
    Class Program8
    End Class

    <PartCreationPolicy(CreationPolicy.NonShared)>
    Class Program9
        Implements IMyInheritedExportInterface
    End Class

    <PartCreationPolicy(CreationPolicy.[Shared])>
    Class Program11
        Implements IFoo, IBar, IMyInheritedExportInterface, IQix
    End Class

    <PartCreationPolicy(CreationPolicy.[NonShared])> ' Noncompliant
    Class Program12
        Implements IFoo, IBar, IQix
    End Class

    Class MyExportAttribute
        Inherits ExportAttribute
    End Class

    Class MyInheritedExportAttribute
        Inherits InheritedExportAttribute

        Public Sub New(ByVal type As System.Type)
        End Sub
    End Class

    Class FooAttribute
        Inherits Attribute
    End Class

    <InheritedExport(GetType(Object))>
    Interface IMyInheritedExportInterface
    End Interface

    Interface IFoo
    End Interface

    Interface IBar
    End Interface

    Interface IQix
    End Interface

End Namespace
