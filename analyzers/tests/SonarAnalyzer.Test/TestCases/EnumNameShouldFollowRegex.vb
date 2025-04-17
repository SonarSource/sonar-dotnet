Imports System

Namespace Tests.Diagnostics
    Public Enum MyEnum
        Value
    End Enum
    Public Enum myEnum2 ' Noncompliant {{Rename the enumeration 'myEnum2' to match the regular expression: '^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$'.}}
'               ^^^^^^^
        Value
    End Enum
    Public Enum MyEnumTTTT ' Noncompliant
        Value
    End Enum
    <Flags()>
    Public Enum MyFlagEnums
        Value
    End Enum
    <Flags()>
    Public Enum MyFlagEnum ' Noncompliant {{Rename the enumeration 'MyFlagEnum' to match the regular expression: '^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?s$'.}}
'               ^^^^^^^^^^
        Value
    End Enum
    <Flags()>
    Public Enum myFlagEnums2 ' Noncompliant
        Value
    End Enum
    <Flags()>
    Public Enum MyFlagEnumTTTTs ' Noncompliant
        Value
    End Enum
End Namespace
