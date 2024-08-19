Imports System.Threading

Namespace Monitor_Goto

    Class Program

        Private Obj As New Object

        Public Sub Method1()
            Monitor.Enter(Obj) ' Compliant
            GoTo Release
Release:
            Monitor.Exit(Obj)
        End Sub

        Public Sub Method2()
            Monitor.Enter(Obj) ' FN
            GoTo DoNotRelease
Release:
            Monitor.Exit(Obj)
DoNotRelease:
        End Sub

    End Class

End Namespace
