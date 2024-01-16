using System;
using System.Collections.Generic;
using Person = (string name, string surname);

class WithAliasAnyType
{
    void ValidCases(Person person)
    {
        _ = (Person)person;           // Compliant: not a duplicated cast
    }

    void InvalidCases(object obj)
    {
        if (obj is Person)            // FN: Person is alias for a struct
        {
            _ = (Person)obj;
        }

        if (obj is (string, string))  // FN: (string, string) and Person are equivalent
        {
            _ = (Person)obj;
        }

        if (obj is Person)            // FN: Person and (string, string) are equivalent
        {
            _ = ((string, string))obj;
        }

        if (obj is Person)            // FN: Person and (string ..., string) are equivalent
        {
            _ = ((string differentName1, string))obj;
        }

        if (obj is Person)            // FN: Person and (string, string ...) are equivalent
        {
            _ = ((string, string differentName2))obj;
        }

        if (obj is (string differentName1, string))  // FN: (string ..., string) and Person are equivalent
        {
            _ = (Person)obj;
        }

        if (obj is (string, string differentName2))  // FN: (string, string ...) and Person are equivalent
        {
            _ = (Person)obj;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/223
class Repro_223
{
    void NumericTypes(object obj)
    {
        if (obj is int)             // FN
        {
            _ = (int)obj;
        }

        if (obj is double)          // FN
        {
            _ = (double)obj;
        }

        if (obj is ushort)          // FN
        {
            _ = (ushort)obj;
        }
    }

    void NullableValueTypes(object obj)
    {
        if (obj is int?)             // FN
        {
            _ = (int?)obj;
        }

        if (obj is byte?)            // FN
        {
            _ = (byte?)obj;
        }
    }

    void UsingLanguageKeywordAndFrameworkName(object obj)
    {
        if (obj is Nullable<int>)    // FN
        {
            _ = (int?)obj;
        }

        if (obj is int?)             // FN
        {
            _ = (Nullable<int>)obj;
        }

        if (obj is IntPtr)           // FN
        {
            _ = (nint)obj;
        }

        if (obj is nint)             // FN
        {
            _ = (IntPtr)obj;
        }

        if (obj is System.UIntPtr)   // FN
        {
            _ = (nuint)obj;
        }
    }

    void Enums(object obj)
    {
        if (obj is AnEnum)    // Noncompliant
        {
            _ = (AnEnum)obj;  // Secondary
        }

        if (obj is AnEnum?)   // FN
        {
            _ = (AnEnum?)obj;
        }
    }

    void UserDefinedStructs(object obj)
    {
        if (obj is AStruct)            // FN
        {
            _ = (AStruct)obj;
        }

        if (obj is ARecordStruct)      // FN
        {
            _ = (ARecordStruct)obj;
        }

        if (obj is AReadonlyRefStruct) // FN
        {
            _ = (AStruct)obj;
        }
    }

    enum AnEnum { Value1, Value2 }
    struct AStruct { }
    record struct ARecordStruct { }
    readonly ref struct AReadonlyRefStruct { }
}


