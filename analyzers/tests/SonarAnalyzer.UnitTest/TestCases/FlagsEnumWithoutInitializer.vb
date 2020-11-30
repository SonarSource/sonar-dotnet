Namespace Tests.Diagnostics

    <System.Flags()>
    Enum FruitType    ' Noncompliant {{Initialize all the members of this 'Flags' enumeration.}}
'        ^^^^^^^^^
        Banana
        Orange = 5
        Strawberry
    End Enum

    Enum FruitType2    ' Compliant
        Banana
        Orange
        Strawberry
    End Enum

    <System.Flags()>
    Enum FruitType3
        Banana = 1
        Orange = 3
        Strawberry = 4
    End Enum

    <System.Flags()>
    Enum FruitType4
        Banana
        Orange
        Strawberry = 4
    End Enum

    <System.Flags()>
    Enum FruitType5
        Banana
        Orange
        Strawberry
    End Enum

    <System.Flags()>
    Enum FruitType6 ' Noncompliant
        None
        Banana
        Orange
        Strawberry
    End Enum
End Namespace