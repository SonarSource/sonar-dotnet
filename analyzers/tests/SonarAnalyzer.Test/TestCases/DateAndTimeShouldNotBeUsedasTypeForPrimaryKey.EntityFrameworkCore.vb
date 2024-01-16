Imports Microsoft.EntityFrameworkCore
Imports System

' DateOnly and TimeOnly types are available in .NET 6+
Class TemporalTypes
    Class DateOnlyKey
        Public Property Id As DateOnly            ' Noncompliant
    End Class

    Class TimeOnlyKey
        Public Property Id As TimeOnly            ' Noncompliant
    End Class
End Class

Class ClassWithPrimaryKeyAttribute
    ' The PrimaryKey attribute was introduced in Entity Framework 7.0.
    ' While it's possible to create a key for a single property with this attribute, it's mainly created to create composite keys.
    <PrimaryKey(NameOf(PrimaryKeyWithSingleProperty.KeyProperty))>
    Class PrimaryKeyWithSingleProperty
        Public Property KeyProperty As Date   ' FN - possible, but unlikely scenario
    End Class

    <PrimaryKey(NameOf(PrimaryKeyWithMultipleProperties.DateProperty), NameOf(PrimaryKeyWithMultipleProperties.IntProperty))>
    Class PrimaryKeyWithMultipleProperties
        Public Property DateProperty As Date  ' Compliant - the rule will not raise warnings when a temporal type is part of a composite key
        Public Property IntProperty As Integer
    End Class
End Class
