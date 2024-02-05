Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.RazorPages
Imports System

Namespace Compliant

    Class DefaultImplementation         ' Compliant - the class will use the default implementation of DefaultMethod
        Inherits AbstractBaseWithoutAbstractMethods
    End Class

    Public Class CustomException        ' Compliant - empty exception classes are allowed, the name of the class already provides information
        Inherits Exception
    End Class

    Public Class CustomAttribute        ' Compliant - empty attribute classes are allowed, the name of the class already provides information
        Inherits Exception
    End Class

    Public Class EmptyPageModel         ' Compliant - an empty PageModel can be fully functional, the VB code can be in the vbhtml file
        Inherits PageModel
    End Class

    Public Class CustomActionResult     ' Compliant - an empty action result can still provide information by its name
        Inherits ActionResult
    End Class

End Namespace

Namespace NonComplaint

    Public Class SubClass               ' Noncompliant - not derived from any special base class
        Inherits BaseClass
    End Class

End Namespace

Namespace Ignore

    Class NoImplementation              ' Error [BC30610]- abstract methods should be implemented
        Inherits AbstractBaseWithAbstractMethods
    End Class

End Namespace

Public Class BaseClass
    Private ReadOnly Property Prop As Integer
        Get
            Return 42
        End Get
    End Property
End Class

MustInherit Class AbstractBaseWithAbstractMethods
    Public MustOverride Sub AbstractMethod()
End Class

MustInherit Class AbstractBaseWithoutAbstractMethods
    Public Overridable Sub DefaultMethod()
    End Sub
End Class
