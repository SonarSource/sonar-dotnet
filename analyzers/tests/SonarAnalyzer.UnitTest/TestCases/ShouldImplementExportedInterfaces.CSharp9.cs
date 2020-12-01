using System;
using System.ComponentModel.Composition;

interface MyInterface { }

[Export(typeof(MyInterface))] // Compliant - FN
[Export(typeof(Exported))] // Compliant - FN
record NotExported
{
}

[Export(typeof(MyInterface))]
[Export(typeof(Exported))]
record Descendant : Exported
{
}

[Export(typeof(MyInterface))]
[Export(typeof(Descendant))] // Compliant - FN
record Exported : MyInterface
{
}

[Export(contractType: typeof(IComparable), contractName: "asdasd")] // Compliant - FN
[Export(contractType: typeof(MyInterface), contractName: "asdasd")]
record NotExported_NamedArgs_ReverseOrder : MyInterface
{
}

interface ISomething<T> { }

[Export(typeof(ISomething<>))] // Compliant - FN
public record Something<T> : ISomething<T>
{
}
