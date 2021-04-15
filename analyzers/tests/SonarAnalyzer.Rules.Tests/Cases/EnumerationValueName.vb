Namespace Tests.Diagnostics
    Public Enum MyEnum
        Value
        v               ' Noncompliant
        ValueVVV = 42   ' Noncompliant
    End Enum
End Namespace