using System;
using System.Collections.Generic;

struct MyStruct // Noncompliant {{Implement 'IEquatable<T>' in value type 'MyStruct'.}}
//     ^^^^^^^^
{
}

struct MyCompliantStruct : IEquatable<MyCompliantStruct> // Compliant
{
    public bool Equals(MyCompliantStruct other)
    {
        throw new NotImplementedException();
    }
}

struct ComparableStruct : IComparable<ComparableStruct> // Noncompliant
{
    public int CompareTo(ComparableStruct other) { return 0; }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3157
ref struct Repro_3157 // Compliant, ref structs can not implement interfaces
{
}
