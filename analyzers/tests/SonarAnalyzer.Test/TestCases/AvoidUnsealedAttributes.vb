' Test framework can't assert location on 1st line
Public Class PublicAttribute ' Noncompliant {{Seal this attribute or make it abstract.}}
    Inherits Attribute
    '        ^^^^^^^^^^^^^^^ @-1
End Class

Public NotInheritable Class SealedAttribute
    Inherits Attribute ' Compliant
End Class

Public MustInherit Class AbstractAttribute
    Inherits Attribute ' Compliant
End Class

Public Class NotAnAttribute ' Compliant - not an attribute
End Class

Public Class Container

    Protected Class ProtectedAttribute ' Noncompliant
        Inherits Attribute
    End Class

    Private Class PrivateAttribute ' Compliant
        Inherits Attribute

        Public Class SubClassAttribute ' Compliant - effective accessibility is private
            Inherits Attribute
        End Class
    End Class
End Class
