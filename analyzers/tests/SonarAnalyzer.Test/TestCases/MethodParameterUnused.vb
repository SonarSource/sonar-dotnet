Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases

    Class InvalidCodeClass

        Dim field As String

        Private Sub DoSomething1NoParams()
            Console.WriteLine("foo")
        End Sub

        Private Sub DoSomething1(ByVal a As Integer) ' Noncompliant {{Remove this unused procedure parameter 'a'.}}
            '                    ^^^^^^^^^^^^^^^^^^
            Console.WriteLine("foo")
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer ' Noncompliant
            Return 1
        End Function

        Private Sub DoSomething3(ByRef a As Integer) ' Noncompliant
            Console.WriteLine("foo")
        End Sub

        Private Sub DoSomething4(field As String) ' Noncompliant - the parameter is not used
            Me.field = ""
        End Sub

        Private Sub DoSomething5(intField As Integer) ' Noncompliant - the parameter is not used
            Dim x = GetSomething()
            Console.WriteLine(x?.intField)
        End Sub

        Private Function GetSomething() As InvalidCodeStructure?
            Return Nothing
        End Function

        Private Sub OnlyThrows1(ByVal a As Integer) ' Noncompliant
            Throw New InvalidOperationException()
        End Sub

        Private Function OnlyThrows2(ByVal a As Integer) As Integer ' Noncompliant
            Throw New InvalidOperationException()
        End Function

        Public Sub Something(. As Integer) ' Error [BC30203] for coverage
            Dim X As Integer = 42
        End Sub

    End Class

    Structure InvalidCodeStructure

        Dim intField As Integer

        Private Sub DoSomething1(ByVal a As Integer) ' Noncompliant {{Remove this unused procedure parameter 'a'.}}
            '                    ^^^^^^^^^^^^^^^^^^
            Console.WriteLine("foo")
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer ' Noncompliant
            Return 1
        End Function

    End Structure

    Module InvalidCodeModule

        Private Sub DoSomething1(ByVal a As Integer) ' Noncompliant {{Remove this unused procedure parameter 'a'.}}
            '                    ^^^^^^^^^^^^^^^^^^
            Console.WriteLine("foo")
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer ' Noncompliant
            Return 1
        End Function

    End Module

    Class ValidCodeClass

        Private Sub DoSomething1(ByVal a As Integer)
            Dim x = a
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer
            Return a
        End Function

        Private Function Casing(DifferentCasing As Integer) As Integer
            Return differentCASING
        End Function

        Sub DoSomething3(ByVal a As Integer) ' Default accessibility is public
            Console.WriteLine("foo")
        End Sub

        Function DoSomething4(ByVal a As Integer) As Integer ' Default accessibility is public
            Return 1
        End Function

        Public Sub DoSomething5(ByVal a As Integer)
            Console.WriteLine("foo")
        End Sub

        Public Function DoSomething6(ByVal a As Integer) As Integer
            Return 1
        End Function

        Protected Sub DoSomething7(ByVal a As Integer)
            Console.WriteLine("foo")
        End Sub

        Protected Function DoSomething8(ByVal a As Integer) As Integer
            Return 1
        End Function

        Friend Sub DoSomething9(ByVal a As Integer)
            Console.WriteLine("foo")
        End Sub

        Friend Function DoSomething10(ByVal a As Integer) As Integer
            Return 1
        End Function

        <Obsolete()>
        Private Sub Attribute1(ByVal a As Integer) ' Compliant because of the attribute
            Console.WriteLine("foo")
        End Sub

        <Obsolete()>
        Private Function Attribute2(ByVal a As Integer) As Integer ' Compliant because of the attribute
            Return 1
        End Function

        Private Sub Empty1(ByVal a As Integer) ' Compliant because there is no statements
        End Sub

        Private Sub OnlyThrows1(ByVal a As Integer) ' Compliant because it only throws NotImplementedException
            Throw New NotImplementedException()
        End Sub

        Private Function OnlyThrows2(ByVal a As Integer) As Integer ' Compliant because it only throws NotImplementedException
            Throw New NotImplementedException()
        End Function

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As EventArgs) ' Compliant because it is event handler
            Console.WriteLine()
        End Sub

        Private Class PrivateClass

            Public Overridable Sub Overridable1(ByVal a As Integer) ' Compliant because it is Overridable
                Console.WriteLine("foo")
            End Sub

            Public Overridable Function Overridable2(ByVal a As Integer) As Integer ' Compliant because it is Overridable
                Return 1
            End Function

        End Class

    End Class

    Structure ValidCodeStructure

        Private Sub DoSomething1(ByVal a As Integer)
            Dim x = a
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer
            Return a
        End Function

    End Structure

    Module ValidCodeModule

        Private Sub DoSomething1(ByVal a As Integer)
            Dim x = a
        End Sub

        Private Function DoSomething2(ByVal a As Integer) As Integer
            Return a
        End Function

    End Module

    Public MustInherit Class AbstractValidCode

        MustOverride Sub MyAbstractMethod(ByVal a As Integer)

    End Class

    Public Interface IPerson

        Function GetSomething(ByVal age As Integer) As Integer

    End Interface

    Public Class Person
        Inherits AbstractValidCode
        Implements IPerson

        Public Function GetSomething(age As Integer) As Integer Implements IPerson.GetSomething ' Compliant because it's an interface implementation
            Return 42
        End Function

        Public Overrides Sub MyAbstractMethod(a As Integer) ' Compliant because it overrides
            Console.WriteLine()
        End Sub

        Private Function GetFoo(ByVal s As String) ' Noncompliant
            Return ""
        End Function

    End Class

    Module MainModule

        Function Main(ByVal cmdArgs() As String) As Integer ' Compliant because this is the main method
            Return 1
        End Function

    End Module

    Module OtherMainModule

        Sub Main(ByVal cmdArgs() As String) ' Compliant because this is the main method
            Console.WriteLine()
        End Sub

    End Module

    ' https//github.com/SonarSource/sonar-dotnet/issues/4406
    Public Class Source

        Public Event Dirty(Name As String, Count As Integer)

    End Class

    Public Class Consumer

        Private WithEvents fSource As Source

        Private Sub fSource_Dirty(Name As String, Count As Integer) Handles fSource.Dirty 'Compliant, because it's WithEvents event handler
            Dim S As String = Name
        End Sub

    End Class

End Namespace
