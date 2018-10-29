using System;
using System.ComponentModel.Composition;

namespace Tests.Diagnostics
{
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

    [PartCreationPolicy(CreationPolicy.Any)] // Noncompliant {{Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this class definition.}}
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

    [PartCreationPolicy(CreationPolicy.Any)] // Error [CS0116] - Compliant, illegal use, don't raise
}
