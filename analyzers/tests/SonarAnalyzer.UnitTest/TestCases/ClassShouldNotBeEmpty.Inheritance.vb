Imports Microsoft.AspNetCore.Mvc.RazorPages

Public Class BaseClass
    Private ReadOnly Property Prop As Integer
        Get
            Return 0
        End Get
    End Property
End Class

Public Class SubClass       ' Noncompliant - not derived from any special base class
    Inherits BaseClass
End Class

Public Class EmptyPageModel ' Compliant - an empty PageModel can be fully functional, the VB code can be in the vbhtml file
    Inherits PageModel
End Class
