using System;
using System.ComponentModel.Composition;

namespace Classes
{
    static class Constants
    {
        public const string ContractName = "asdasd";
    }

    interface MyInterface { }

    [Export(typeof(MyInterface))] // Noncompliant {{Implement 'MyInterface' on 'NotExported' or remove this export attribute.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [Export(typeof(Exported))] // Noncompliant {{Derive from 'Exported' on 'NotExported' or remove this export attribute.}}
//   ^^^^^^^^^^^^^^^^^^^^^^^^
    class NotExported
    {
    }

    [Export(contractType: typeof(IComparable), contractName: "asdasd")] // Noncompliant
    [Export(contractType: typeof(MyInterface), contractName: "asdasd")]
    class NotExported_NamedArgs_ReverseOrder : MyInterface
    {
    }

    [Export("something", typeof(MyInterface))] // Noncompliant
    [Export(Constants.ContractName, typeof(IDisposable))] // Noncompliant
    class NotExported_MultipleArgs
    {
    }

    [Export(typeof(MyInterface)), Export(typeof(IComparable)), Export(typeof(IDisposable))]
//   ^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Implement 'MyInterface' on 'NotExported_Multiple' or remove this export attribute.}}
//                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Implement 'IComparable' on 'NotExported_Multiple' or remove this export attribute.}}
    class NotExported_Multiple : IDisposable
    {
        public void Dispose() { }
    }

    [ExportAttribute(typeof(MyInterface))] // Noncompliant
    class NotExported_FullAttributeName
    {
    }

    [Export(typeof(MyInterface))]
    [Export(typeof(Descendant))] // Noncompliant
    class Exported : MyInterface
    {
    }

    [Export]
    [Export("something")] // Exposing ourselves
    [Export(typeof(Exporting_Ourselves))]
    class Exporting_Ourselves
    {
    }

    [Export(1)]
    [Export(1, typeof(IComparable))]
    [Export(typeof(ASDASD))]
    [Export(typeof(MyInterface), typeof(IComparable))]
    class InvalidSyntax
    {
    }

    [Import(typeof(MyInterface))]
    [InheritedExport(typeof(MyInterface))] // Noncompliant
    [InheritedExport(typeof(OtherAttributes))]
    class OtherAttributes
    {
    }

    [Export(typeof(MyInterface))]
    [Export(typeof(Exported))]
    class Descendant : Exported
    {
    }

    class ExportingMembers_Are_Ignored
    {
        [Export(typeof(MyInterface))]
        [Export(typeof(Exported))]
        public NotExported MyProperty { get; set; }

        [Export(typeof(MyInterface))]
        [Export(typeof(Exported))]
        public NotExported MyField;

        [Export(typeof(MyInterface))]
        [Export(typeof(Exported))]
        public NotExported MyMethod() { }
    }

    interface ISomething<T> { }
    public class BaseThing { }
    public class BaseThing2 { }

    [Export(typeof(ISomething<BaseThing>))]
    public class BaseSomethingImplementation : ISomething<BaseThing>
    {

    }

    [Export(typeof(ISomething<BaseThing>))] // Noncompliant {{Implement 'ISomething<BaseThing>' on 'Something<BaseThing>' or remove this export attribute.}}
    public class Something<BaseThing>
    {
    }

    [Export(typeof(ISomething<BaseThing>))] // Noncompliant
    public class SomethingImplementation : ISomething<BaseThing2>
    {

    }

    [Export(typeof(ISomething<>))] // Noncompliant
    public class Something<T> : ISomething<T>
    {
    }
}
