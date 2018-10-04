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

    <PartCreationPolicy(CreationPolicy.Any)> ' Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this class definition.}}
    class Program3
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^@-1
    
    End Class

    <PartCreationPolicy(CreationPolicy.Any)> ' Noncompliant, Export is not inherited
    class Program4
        inherits Program1
    
    End Class

    class Program5
    
        <PartCreationPolicy(CreationPolicy.Any)> ' won't compile
        public Sub Method
        End Sub

        <PartCreationPolicy(CreationPolicy.Any)> ' won't compile
        public Dim Field As Int

        <PartCreationPolicy(CreationPolicy.Any)> ' won't compile
        Public Property Name As String
    End Class

End Namespace
