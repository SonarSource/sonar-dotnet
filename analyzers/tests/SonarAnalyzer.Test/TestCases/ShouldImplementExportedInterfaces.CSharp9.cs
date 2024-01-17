using System;
using System.ComponentModel.Composition;

interface MyInterface { }

[Export(typeof(MyInterface))] // Noncompliant
[Export(typeof(Exported))] // Noncompliant
record NotExported
{
}

[Export(typeof(MyInterface))] // Noncompliant
[Export(typeof(Exported))] // Noncompliant
record NotExportedPositional(string Value)
{
}

[Export(typeof(MyInterface))]
[Export(typeof(Exported))]
record Descendant : Exported
{
}

[Export(typeof(MyInterface))]
[Export(typeof(Descendant))] // Noncompliant
record Exported : MyInterface
{
}

[Export(contractType: typeof(IComparable), contractName: "asdasd")] // Noncompliant
[Export(contractType: typeof(MyInterface), contractName: "asdasd")]
record NotExported_NamedArgs_ReverseOrder : MyInterface
{
}

interface ISomething<T> { }

[Export(typeof(ISomething<>))]
public record Something<T> : ISomething<T>
{
}
