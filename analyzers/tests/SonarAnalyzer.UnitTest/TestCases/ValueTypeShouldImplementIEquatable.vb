Public Structure MyCompliantStruct ' Compliant
    Implements IEquatable(Of MyCompliantStruct)

    Public Overloads Function Equals(other As MyCompliantStruct) As Boolean Implements IEquatable(Of MyCompliantStruct).Equals
        Return True
    End Function
End Structure

Structure MyStruct ' Noncompliant {{Implement 'IEquatable<T>' in value type 'MyStruct'.}}
    '     ^^^^^^^^
End Structure
