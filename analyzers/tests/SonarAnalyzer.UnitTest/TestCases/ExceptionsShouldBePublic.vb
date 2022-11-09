Imports System.Collections.Generic

Public Class PublicException ' Compliant
    Inherits Exception
End Class

Friend Class InternalException
    '        ^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'Public'.}}
    Inherits Exception
End Class

Class ImplicitAccessibilityException
    ' ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'Public'.}}
    Inherits Exception

End Class

Public Class PublicContainer
    Public Class PublicException ' Compliant
        Inherits Exception
    End Class

    Private Class PrivateClass ' Compliant, no exception
    End Class

    Friend Class InternalException ' Noncompliant
        Inherits Exception
    End Class

    Class ImplicitAccessibilityException ' Compliant
        Inherits Exception
    End Class

    Protected Class InheritsFromOtherException ' Compliant
        Inherits DivideByZeroException
    End Class

    Private Class PrivateException ' Noncompliant
        Inherits Exception
    End Class
End Class

Friend Class InternalContainer
    Public Class PublicException
        '        ^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'Public'.}}
        Inherits Exception
    End Class

    Private Class PrivateClassn ' Compliant
    End Class

    Protected Class ProtectedClass ' Compliant
    End Class

    Friend Class InternalException
        Inherits DivideByZeroException ' Compliant
    End Class

    Protected Class ProtectedException ' Compliant
        Inherits DivideByZeroException
    End Class

    Class ImplicitAccessibilityException ' Noncompliant
        Inherits ApplicationException
    End Class

    Private Class PrivateException ' Noncompliant
        Inherits Exception
    End Class
End Class

