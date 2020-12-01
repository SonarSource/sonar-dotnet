Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class BaseClass
        Public Overridable Sub MyMethod(ByVal j As Integer, ByVal Optional i As Integer = 1)
        End Sub

        Public Overridable Sub MyMethod2(ByVal Optional i As Integer = 1)
        End Sub
    End Class

    Public Class DerivedClass
        Inherits BaseClass

        Public Overrides Sub MyMethod(ByVal j As Integer, ByVal Optional i As Integer = 1)
            MyBase.MyMethod(1) ' Noncompliant {{Pass the missing user-supplied parameter value to this 'base' call.}}
'           ^^^^^^^^^^^^^^^^^^
            MyBase.MyMethod(j) ' Noncompliant {{Pass the missing user-supplied parameter value to this 'base' call.}}
            MyBase.MyMethod2()
            Me.MyMethod(1)
        End Sub

        Public Overrides Sub MyMethod2(Optional i As Integer = 1)
        ' Special for VB.NET: Call without parentheses must be handled
            MyBase.MyMethod2 ' Noncompliant {{Pass the missing user-supplied parameter value to this 'base' call.}}
'           ^^^^^^^^^^^^^^^^
        End Sub

    End Class
End Namespace
