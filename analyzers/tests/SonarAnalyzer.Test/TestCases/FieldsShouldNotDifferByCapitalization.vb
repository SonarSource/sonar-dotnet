Public Class Fruit

    Protected flesh As Integer
    Protected fleshShared As Integer
    Private flesh_color As Integer
    Protected shadowed As Integer

End Class

Public Class Raspberry
    Inherits Fruit

    Private fLeSh As Integer                ' Noncompliant {{Rename this field; it may be confused with 'flesh' in 'Fruit'.}}
    Private Shared FLESHSHARED As Integer   ' Noncompliant {{Rename this field; it may be confused with 'fleshShared' in 'Fruit'.}}
    Private Shared FLESH_COLOR As Integer   ' Compliant, base Class field Is Private

    Protected Shadows Shadowed As Integer   'Compliant, the intention is explicit

End Class
