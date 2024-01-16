Namespace Tests.Diagnostics
    Public Enum MyEnum ' Noncompliant {{Rename this enumeration to remove the 'Enum' suffix.}}
'               ^^^^^^
        Value
    End Enum
    Public Enum MyFlags ' Noncompliant
        Value
    End Enum
    Public Enum MyEnum2
        Value
    End Enum
    Public Class ClassEnum
    End Class
End Namespace