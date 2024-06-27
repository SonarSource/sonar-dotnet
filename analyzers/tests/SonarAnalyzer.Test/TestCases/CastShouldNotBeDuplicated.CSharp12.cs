using System;
using System.Collections.Generic;
using Person = (string name, string surname);

class WithAliasAnyType
{
    void ValidCases(Person person)
    {
        _ = (Person)person;             // Compliant: not a duplicated cast
    }

    void InvalidCases(object obj)
    {
        if (obj is Person)              // Noncompliant
        {
            _ = (Person)obj;            // Secondary
        }

        if (obj is (string, string))    // FN: (string, string) and Person are equivalent
        {
            _ = (Person)obj;
        }

        if (obj is Person)              // FN: Person and (string, string) are equivalent
        {
            _ = ((string, string))obj;
        }

        if (obj is Person)              // FN: Person and (string ..., string) are equivalent
        {
            _ = ((string differentName1, string))obj;
        }

        if (obj is Person)              // FN: Person and (string, string ...) are equivalent
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
        if (obj is int)             // Noncompliant
        {
            _ = (int)obj;           // Secondary
        }

        if (obj is double)          // Noncompliant
        {
            _ = (double)obj;        // Secondary
        }

        if (obj is ushort)          // Noncompliant
        {
            _ = (ushort)obj;        // Secondary
        }
    }

    void NullableValueTypes(object obj)
    {
        if (obj is int?)            // Noncompliant
        {
            _ = (int?)obj;          // Secondary
        }

        if (obj is byte?)           // Noncompliant
        {
            _ = (byte?)obj;         // Secondary
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
        if (obj is AnEnum)      // Noncompliant
        {
            _ = (AnEnum)obj;    // Secondary
        }

        if (obj is AnEnum?)     // Noncompliant
        {
            _ = (AnEnum?)obj;   // Secondary
        }
    }

    void UserDefinedStructs(object obj)
    {
        if (obj is AStruct)             // Noncompliant
        {
            _ = (AStruct)obj;           // Secondary
        }

        if (obj is ARecordStruct)       // Noncompliant
        {
            _ = (ARecordStruct)obj;     // Secondary
        }

        if (obj is AReadonlyRefStruct)      // Noncompliant, but irrelevant, because ref structs cannot be casted
        {                                   // Error@+1 [CS0030] Cannot convert type 'object' to 'Repro_223.AReadonlyRefStruct'
            _ = (AReadonlyRefStruct)obj;    // Secondary
        }
    }

    enum AnEnum { Value1, Value2 }
    struct AStruct { }
    record struct ARecordStruct { }
    readonly ref struct AReadonlyRefStruct { }
}


