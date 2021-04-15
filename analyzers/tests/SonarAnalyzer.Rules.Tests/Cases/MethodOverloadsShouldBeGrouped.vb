Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class A
        Public Overloads Sub Test() ' Noncompliant {{All 'Test' method overloads should be adjacent.}}
'                            ^^^^
        End Sub

        Public Overloads Sub Test(ByVal a As Integer)
        End Sub

        Property Size As Integer

        Public Overloads Sub Test(ByVal a As Double) ' Secondary {{Non-adjacent overload}}
'                            ^^^^
        End Sub

        Public Overloads Function Test2() ' Noncompliant {{All 'Test2' method overloads should be adjacent.}}
'                                 ^^^^^
        End Function

        Public Overloads Function Test2(ByVal a As Integer)
        End Function

        Private Class NestedClass
        End Class

        Public Overloads Function Test2(ByVal a As Integer, ByVal b As Integer) ' Secondary {{Non-adjacent overload}}
'                                 ^^^^^
        End Function

        Public Overloads Function Test3() ' Noncompliant {{All 'Test3' method overloads should be adjacent.}}
        End Function

        Public Overloads Function Test3(ByVal a As Integer)
        End Function

        Public Age As Integer

        Public Overloads Function test3(ByVal a As Double) ' Secondary {{Non-adjacent overload}}
        End Function

        Public Overloads Function Test4()
        End Function

        Public Overloads Function Test4(ByVal a As Integer)
        End Function

        Public Overloads Function Test4(ByVal a As Double)
        End Function

        Public Overloads Function TEST3(ByVal a As Double, ByVal b As Integer) ' Secondary {{Non-adjacent overload}}
        End Function
    End Class

    Class B
        Public Sub New() ' Noncompliant {{All 'New' method overloads should be adjacent.}}
        End Sub

        Public Sub New(ByVal x As Integer)
        End Sub

        Protected Overrides Sub Finalize()
        End Sub

        Public Sub New(ByVal x As Double) ' Secondary {{Non-adjacent overload}}
        End Sub
    End Class

    Class C
        Public Overloads Sub Test()
        End Sub

        Public Overloads Sub TEST(ByVal a As Double)
        End Sub

        Public Overloads Sub Test(ByVal a As Integer)
        End Sub

        Public Overloads Function Test2()
        End Function

        Public Overloads Function Test2(ByVal a As Integer)
        End Function
    End Class

    Interface D
        Function TEST() As Integer ' Noncompliant {{All 'TEST' method overloads should be adjacent.}}

        Function test(ByVal a As Double) As Integer

        Event MyEvent(ByVal Success As Boolean)

        Function TEST2() As Integer

        Function tEst2(ByVal a As Integer) As Integer

        Function test2(ByVal a As Double) As Integer

        Function tEst(ByVal a As Integer) As Integer ' Secondary {{Non-adjacent overload}}

        Event MyEvent2(ByVal Success As Boolean)
    End Interface

    Interface E
        Function TEST() As Integer

        Function test(ByVal a As Double) As Integer

        Function tEst(ByVal a As Integer) As Integer

        Event MyEvent2(ByVal Success As Boolean)
    End Interface

    Structure F
        Public myField1 As String

        Public Sub Test() ' Noncompliant {{All 'Test' method overloads should be adjacent.}}
        End Sub

        Public myField2 As String

        Public Sub Test(a As Double) ' Secondary {{Non-adjacent overload}}
        End Sub

        Public myField3 As String

        Public Sub Test(a As Integer) ' Secondary {{Non-adjacent overload}}
        End Sub
    End Structure

    Class K

        Public Sub Lorem()
        End Sub

        Public Sub DoSomething()
        End Sub

        Private Sub DoSomething(i As Integer)   ' Compliant interleaving With different accesibility
        End Sub

        Protected Sub DoSomething(b As Boolean)
        End Sub

        Public Sub DoSomething(s As String)
        End Sub

        Private Sub Lorem(i As Integer)     ' Compliant, different accessibility
        End Sub

    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/2776
    Public Class StaticMethodsTogether

        Public Sub New()
        End Sub

        Public Sub New(i As Integer)
        End Sub

        Public Shared Sub MethodA() ' Compliant - Static methods are grouped together, it's ok
        End Sub

        Public Shared Sub MethodB()
        End Sub

        Shared Sub New()            ' Compliant - static constructor can be grouped with static methods
        End Sub

        Public Sub MethodA(ByVal i As Integer)
        End Sub

        Public Shared Sub MethodC() ' Noncompliant - When there're more shared methods, they still should be together
        End Sub

        Public Shared Sub MethodD()
        End Sub

        Public Shared Function MethodD(i As Integer) As Integer ' Compliant
        End Function

        Public Shared Sub MethodD(b As Boolean)         ' Compliant
        End Sub

        Public Shared Sub MethodC(b As Boolean)         ' Secondary
        End Sub

        Public Shared Sub Separator()
        End Sub

        Public Shared Function MethodC(i As Integer)    ' Secondary
        End Function

    End Class

    Public MustInherit Class MustOverrideMethodsTogether

        Protected MustOverride Sub DoWork(b As Boolean) 'Compliant - MustOverride methods are grouped together, it's OK
        Protected MustOverride Sub DoWork(i As Integer)

        Protected Sub New()
        End Sub

        Protected Sub DoWork() 'Compliant - MustOverride methods are grouped together, it's OK
            DoWork(True)
            DoWork(42)
        End Sub

        Protected MustOverride Sub OnEvent(b As Boolean)    'Noncompliant
        Protected MustOverride Sub OnProgress()
        Protected MustOverride Sub OnEvent(i As Integer)    'Secondary

        Public Sub DoSomething()
        End Sub

        Protected MustOverride Sub DoSomething(b As Boolean)

        Public Sub DoSomething(i As Integer)
        End Sub

    End Class

    Public Class Inheritor
        Inherits MustOverrideMethodsTogether

        Protected Overrides Sub DoWork(b As Boolean)
        End Sub

        Public Overloads Shared Sub DoWork(s As String)           'Compliant interleaving with static
        End Sub

        Protected Overrides Sub DoWork(i As Integer)
        End Sub

        Protected Overrides Sub OnEvent(b As Boolean)   'Noncompliant
        End Sub

        Protected Overrides Sub OnProgress()
        End Sub

        Protected Overrides Sub OnEvent(i As Integer)   'Secondary
        End Sub

        Protected Overrides Sub DoSomething(b As Boolean)
        End Sub

    End Class

    Public Class InterfaceImplementationTogether
        Implements IEqualityComparer(Of Integer), IEqualityComparer(Of String)

        Private Function ComparerEquals(x As Integer, y As Integer) As Boolean Implements IEqualityComparer(Of Integer).Equals
        End Function

        Private Function ComparerGetHashCode(obj As Integer) As Integer Implements IEqualityComparer(Of Integer).GetHashCode
        End Function

        Private Function ComparerEquals(x As String, y As String) As Boolean Implements IEqualityComparer(Of String).Equals
        End Function

        Private Function ComparerGetHashCode(obj As String) As Integer Implements IEqualityComparer(Of String).GetHashCode
        End Function

    End Class

    Public Interface ITest(Of ItemType)

        Sub Aaa(X As ItemType)
        Event Eee(Arg As ItemType)
        Property Ppp() As ItemType
        Sub Zzz(X As ItemType)

    End Interface

    Public Class InterfaceImplementationTogetherWithPropertiesAndEvents
        Implements ITest(Of Integer), ITest(Of String)

        Public Sub Aaa(X As Integer) Implements ITest(Of Integer).Aaa
            Throw New NotImplementedException()
        End Sub

        Public Property Ppp() As Integer Implements ITest(Of Integer).Ppp

        Public Event Eee(Arg As Integer) Implements ITest(Of Integer).Eee

        Public Sub Zzz(X As Integer) Implements ITest(Of Integer).Zzz
        End Sub

        Public Sub Aaa(X As String) Implements ITest(Of String).Aaa
        End Sub

        Private Property ITest_Ppp() As String Implements ITest(Of String).Ppp

        Private Event ITest_Eee(Arg As String) Implements ITest(Of String).Eee

        Public Sub Zzz(X As String) Implements ITest(Of String).Zzz
        End Sub

    End Class

    Public Interface ICancel

        Sub Cancel()
        Sub Cancel(b As Boolean)
        Sub Renew()

    End Interface

    Public Class InterfaceImplementationMethodsTogether
        Implements ICancel

        Public Sub Cancel() Implements ICancel.Cancel   ' Noncompliant, it should be adjecent inside same Interface implementation group
        End Sub

        Public Sub Renew() Implements ICancel.Renew
        End Sub

        Public Sub Cancel(b As Boolean) Implements ICancel.Cancel ' Secondary
        End Sub

    End Class

End Namespace
