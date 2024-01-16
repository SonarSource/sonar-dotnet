using System;
using System.Collections.Generic;

// Reproducer for #6491 https://github.com/SonarSource/sonar-dotnet/issues/6491
namespace Issue_6491
{
    class SwitchExpressionClass
    {
        List<string> SwitchExpression(string str)
        {
            return str switch
            {
                "First" => null, // FN
                "Second" => new List<string> { },
                _ => null // FN
            };
        }

        List<string> SwitchExpression2(string str) =>
            str switch
            {
                "First" => null, // FN
                "Second" => new List<string> { },
                _ => null // FN
            };
    }
}

namespace NullableAnotated
{
    class MyClass
    {
        public byte[]? GetNullableArrayOfNonNullableValueType(string itemId) => null;
        public byte?[]? GetNullableArrayOfNullableValueType(string itemId) => null;
        public byte?[] GetNonNullableArrayOfNullableValueType(string itemId) => null; // Noncompliant
   
        public object[]? GetNullableArrayOfNonNullableReferenceType(string itemId) => null;
        public object?[]? GetNullableArrayOfNullableReferenceType(string itemId) => null;
        public object?[] GetNonNullableArrayOfNullableReferenceType(string itemId) => null; // Noncompliant
        public byte[] GetArray(string itemId) => null; // Noncompliant
    }

    class MyGenericClass<T> where T : struct
    {
        public List<T>? GetNullableListOfNonNullableValueGenericType(string itemId) => null;
        public List<T?>? GetNullableListOfNullableValueGenericType(string itemId) => null;
        public List<T?> GetNonNullableListOfNullableValueGenericType(string itemId) => null; // Noncompliant
        public List<T> GetList(string itemId) => null; // Noncompliant
    }
}
