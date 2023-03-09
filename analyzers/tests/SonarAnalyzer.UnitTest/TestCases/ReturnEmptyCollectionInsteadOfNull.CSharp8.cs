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

// Reproducer for #6878 https://github.com/SonarSource/sonar-dotnet/issues/6494
namespace Issue_6878
{
    class MyClass
    {
        public byte[]? GetNullableArrayOfNonNullableValueType(string itemId) => null; // Noncompliant FP, Should not raise because nullable is allowed
        public byte?[] GetNonNullableArrayOfNullableValueType(string itemId) => null; // Noncompliant
        public byte?[]? GetNullableArrayOfNullableValueType(string itemId) => null; // Noncompliant, FP
        
        public object[]? GetNullableArrayOfNonNullableReferenceType(string itemId) => null; // Noncompliant FP
        public object?[] GetNonNullableArrayOfNullableReferenceType(string itemId) => null; // Noncompliant
        public object?[]? GetNullableArrayOfNullableReferenceType(string itemId) => null; // Noncompliant FP
        public byte[] GetArray(string itemId) => null; // Noncompliant
    }
}
