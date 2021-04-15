Imports System

Namespace Tests.Diagnostics

    <System.Flags>
    Enum F1
        Zero = 0 'Noncompliant ^9#8 {{Rename 'Zero' to 'None'.}}
        One = 1
    End Enum

    <Flags>
    Enum F2
        None = 0
    End Enum

    <Flags>
    Enum F3
        None = 0
        Four = 4
    End Enum

    <Flags>
    Enum F5 As ULong
        First = ULong.MaxValue
        Last = 0
    End Enum

    Enum NotFlags
        Zero = 0
        One = 1
    End Enum

    Enum NoZeroMember
        First = 1
        Second  'This has value 2
    End Enum

End Namespace
