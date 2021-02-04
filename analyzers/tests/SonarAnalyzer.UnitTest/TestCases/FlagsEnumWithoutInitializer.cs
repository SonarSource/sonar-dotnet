using System;
namespace Tests.Diagnostics
{
    [System.Flags]
    enum FruitType    // Noncompliant {{Initialize all the members of this 'Flags' enumeration.}}
//       ^^^^^^^^^
    {
        Banana,
        Orange = 5,
        Strawberry
    }
    enum FruitType2    // Compliant
    {
        Banana,
        Orange,
        Strawberry
    }

    [Flags]
    enum FruitType3    // Compliant
    {
        Banana=1,
        Orange =4,
        Strawberry =5
    }

    [System.Flags]
    enum FruitType4
    {
        Banana,
        Orange,
        Strawberry = 5
    }

    [System.Flags]
    enum FruitType5
    {
        Banana,
        Orange,
        Strawberry
    }

    [System.Flags]
    enum FruitType6 // Noncompliant
    {
        None,
        Banana,
        Orange,
        Strawberry
    }

    [System.Flags]
    enum FruitType7 // Noncompliant
    {
        Banana,
        Orange,
        Apple,
        Pear,
        Strawberry = 5
    }

    [System.Flags]
    enum FruitType8 // Noncompliant
    {
        Banana = 1,
        Orange,
        Strawberry
    }
}
