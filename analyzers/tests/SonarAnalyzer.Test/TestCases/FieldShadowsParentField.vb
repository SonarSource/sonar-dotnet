Public Class Fruit

    Protected Ripe As Integer
    Protected First, Second As Integer
    Protected Shared Leafs As Integer
    Protected Shadowed As Integer

End Class

Public Class Raspberry
    Inherits Fruit

    Private Ripe As Boolean  ' Noncompliant {{'Ripe' is the name of a field in 'Fruit'.}}
    '       ^^^^
    Protected FirstIsDifferent, Second As Integer  ' Noncompliant {{'Second' is the name of a field in 'Fruit'.}}

    Protected Shared Leafs As Integer       ' Compliant, shared is ignored
    Protected Shadows Shadowed As Integer   ' Compliant, it's explicitly marked as Shadows

End Class
