using System;
using System.ComponentModel.Composition;

[Export(typeof(object))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, Export is present
class Program1
{
}

[InheritedExport(typeof(object))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, InheritedExport is present
class Program2_Base
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Compliant, InheritedExport is present in base
class Program2 : Program2_Base
{
}

    [PartCreationPolicy(CreationPolicy.Any)] // Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this type definition.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
class Program3
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Noncompliant, Export is not inherited
class Program4 : Program1
{
}

class Program5
{
    [PartCreationPolicy(CreationPolicy.Any)] // Error [CS0592] - Compliant, attribute cannot be used on methods, don't raise
    public void Method() { }

    [PartCreationPolicy(CreationPolicy.Any)] // Error [CS0592] - Compliant, attribute cannot be used on fields, don't raise
    public int Field;

    [PartCreationPolicy(CreationPolicy.Any)] // Error [CS0592] - Compliant, attribute cannot be used on properties, don't raise
    public int Property { get; set; }
}

[MyExport]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, custom Export is present
class Program6 { }

[MyInheritedExport(typeof(object))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, MyInheritedExport is present
class Program7_Base { }

[PartCreationPolicy(CreationPolicy.Any)] // Compliant, MyInheritedExport is present in base
class Program7 : Program2_Base { }

[Foo]
[PartCreationPolicy(CreationPolicy.Any)] // Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this type definition.}}
class Program8 { }

[PartCreationPolicy(CreationPolicy.NonShared)] // Compliant, InheritedExport is present on interface
class Program9 : IMyInheritedExportInterface { }

[PartCreationPolicy(CreationPolicy.Shared)] // Compliant, MyInheritedExport is present on interface MyInheritedExportInterface
class Program11 : IFoo, IBar, IMyInheritedExportInterface, IQix { }

[PartCreationPolicy(CreationPolicy.NonShared)] // Noncompliant
class Program12 : IFoo, IBar, IQix { }

class MyExportAttribute : ExportAttribute { }

class MyInheritedExportAttribute : InheritedExportAttribute
{
    public MyInheritedExportAttribute(System.Type type) { }
}

class FooAttribute : Attribute { }

[InheritedExport(typeof(object))]
interface IMyInheritedExportInterface { }

interface IFoo { }
interface IBar { }
interface IQix { }

[PartCreationPolicy(CreationPolicy.Any)] // Error [CS1671] - Compliant, illegal use, don't raise

namespace Aliases
{
    using AliasForPartCreationPolicy = System.ComponentModel.Composition.PartCreationPolicyAttribute;
    using AliasForExport = System.ComponentModel.Composition.ExportAttribute;
    using AliasForInheritedExport = System.ComponentModel.Composition.InheritedExportAttribute;

    [AliasForExport(typeof(object))]
    [PartCreationPolicy(CreationPolicy.Any)]         // Compliant, Export is present
    class AClass1 { }

    [AliasForInheritedExport(typeof(object))]
    [PartCreationPolicy(CreationPolicy.Any)]         // Compliant, InheritanceExport is present
    class AClass2 { }

    [AliasForExport(typeof(object))]
    [AliasForPartCreationPolicy(CreationPolicy.Any)] // Compliant, Export is present
    class AClass3 { }

    [AliasForPartCreationPolicy(CreationPolicy.Any)] // Noncompliant
    class AClass4 { }
}
