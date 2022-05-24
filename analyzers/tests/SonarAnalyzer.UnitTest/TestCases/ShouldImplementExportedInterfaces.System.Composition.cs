using System.Collections.Generic;
using System.Composition;

[Export(typeof(List<>))] // Compliant, FN - Foo<T> should derive from List<T>
class Foo<T>
{
}
