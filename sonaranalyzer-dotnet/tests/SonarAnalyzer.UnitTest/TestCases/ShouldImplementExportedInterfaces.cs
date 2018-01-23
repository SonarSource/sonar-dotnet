using System;
using System.ComponentModel.Composition;

namespace Classes
{
    interface MyInterface { }

    [Export(typeof(MyInterface))] // Noncompliant {{Implement 'MyInterface' on 'NotExported' or remove this Export attribute.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    class NotExported
    {
    }

    [Export(contractType: typeof(IComparable), contractName: "asdasd")] // Noncompliant
    [Export(contractType: typeof(MyInterface), contractName: "asdasd")]
    class NotExported_NamedArgs_ReverseOrder : MyInterface
    {
    }

    [Export("something", typeof(MyInterface))] // Noncompliant
    class NotExported_MultipleArgs
    {
    }

    [Export(typeof(MyInterface)), Export(typeof(IComparable)), Export(typeof(IDisposable))]
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Implement 'MyInterface' on 'NotExported_Multiple' or remove this Export attribute.}}
//                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Implement 'IComparable' on 'NotExported_Multiple' or remove this Export attribute.}}
    class NotExported_Multiple : IDisposable
    {
        public void Dispose() { }
    }

    [ExportAttribute(typeof(MyInterface))] // Noncompliant
    class NotExported_FullAttributeName
    {
    }

    [Export(typeof(MyInterface))]
    class Exported : MyInterface
    {
    }

    [Export]
    [Export("something")] // Exposing ourselves
    [Export(typeof(Exporting_Ourselves))]
    class Exporting_Ourselves
    {
    }

    [Export(typeof(ASDASD))]
    [Export(typeof(MyInterface), typeof(IComparable))]
    [Export(1)]
    class InvalidSyntax // Don't report on invalid syntax to reduce the noise
    {
    }
}
