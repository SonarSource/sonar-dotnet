Namespace Tests.Diagnostics

    Module Module1
        Sub bad_subroutine()                      ' Noncompliant
        End Sub

        Public Function Bad_Function() As Integer ' Noncompliant
            Return 42
        End Function

        Sub GoodSubroutine()                      ' Compliant
        End Sub

        Public Function GoodFunction() As Integer ' Compliant
            Return 42
        End Function

        Sub subject__SomeEvent() Handles Obj.Ev_Event ' Error [BC30506]
        End Sub

        Sub subject__SomeEvent22(sender As Object, args As System.EventArgs) ' Might be event handler
        End Sub

    End Module

        Public MustInherit Class Base

        Public MustOverride Function rqSomething1() As Object ' Noncompliant
        Public MustOverride Sub rqSomething2() ' Noncompliant

    End Class

    Public Interface ISomething

    Function rqFirst() As Object      ' Noncompliant
    Function rqSecond() As Object     ' Noncompliant
    Function rqThird() As Object      ' Noncompliant
    Function rqFourth() As Object     ' Noncompliant
    Sub rqFifth()                     ' Noncompliant
    Sub rqSixth()                     ' Noncompliant
    Sub rqSeventh()                   ' Noncompliant
    Sub rqEight()                     ' Noncompliant
    Sub ValidName()

    End Interface

    Public Class Sample
        Inherits Base
        Implements ISomething

        Public Overrides Function rqSomething1() As Object ' Compliant as it is an override
            Throw New NotImplementedException()
        End Function

        Public oVerRides Sub rqSomething2() ' Compliant as it is an override
            Throw New NotImplementedException()
        End Sub

        Public Function rqFirst() As Object Implements ISomething.rqFirst   ' Compliant as it comes from the interface
            Throw New NotImplementedException()
        End Function

        Public Function rqRenamed() As Object Implements ISomething.rqSecond   ' Noncompliant, because existing name doesn't match the interface member
            Throw New NotImplementedException()
        End Function

        Public Function rqThird() As Object Implements ISomething.rqThird, ISomething.rqFourth ' Noncompliant, because it implements multiple functions
            Throw New NotImplementedException()
        End Function

        Public Sub rqFifth() Implements ISomething.rqFifth ' Compliant as it comes from the interface
            Throw New NotImplementedException()
        End Sub

        Public Sub GoodName() Implements ISomething.rqSixth ' Compliant as the rename is compliant
            Throw New NotImplementedException()
        End Sub

        Public Sub VeryNiceRenaming() Implements ISomething.rqSeventh, ISomething.rqEight ' Compliant as the rename is compliant
            Throw New NotImplementedException()
        End Sub

        Public Sub ValidName() Implements ISomething.ValidName ' Compliant
            Throw New NotImplementedException()
        End Sub

        <System.Runtime.InteropServices.DllImport("foo.dll")>
        Public Shared Sub rqExtern(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Compliant as it is extern
        End Sub

        ' Error@+1 [BC31529]
        <System.Runtime.InteropServices.DllImport("foo.dll")>
        Public Sub rqNotShared(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Noncompliant
        End Sub

        <Obsolete()>
        Public Shared Sub rqNotDllImportAttribute(ByVal p1 As Integer, ByVal p2 As Integer, ByVal p3 As Integer, ByVal p4 As Integer) ' Noncompliant
        End Sub

    End Class

End Namespace
