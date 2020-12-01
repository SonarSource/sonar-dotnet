Namespace Tests.Diagnostics
    <System.Flags>
    Enum X
        Zero = 0 'Noncompliant {{Rename 'Zero' to 'None'.}}
'       ^^^^^^^^
        One = 1
    End Enum
    <System.Flags>
    Enum Y
        None = 0
    End Enum
End Namespace