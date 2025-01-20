Imports System

Namespace Tests.Diagnostics
    Class Program

        Sub SubFirstNoParentheses
            Dim s As String = "test"
            Dim k As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo1()
'           ^^^^ Secondary {{Update this method so that its implementation is not identical to 'Foo2'.}}
'           ^^^^ Secondary@-1 {{Update this method so that its implementation is not identical to 'Foo3'.}}
'           ^^^^ Secondary@-2 {{Update this method so that its implementation is not identical to 'Foo4'.}}

            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo2() ' Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
'           ^^^^
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo3() ' Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo4() ' Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
            Dim s As String = "test" ' Comment are excluded from comparison
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo5()
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
            Console.WriteLine("different")
        End Sub

        Function Func1() As String
'                ^^^^^ Secondary

            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
            Return s
        End Function

        Function Func2() As String ' Noncompliant {{Update this method so that its implementation is not identical to 'Func1'.}}
'                ^^^^^
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
            Return s
        End Function


        Function DiffBySignature1(arg1 As Integer) As Integer
            Console.WriteLine(arg1)
            Return arg1
        End Function

        Function DiffBySignature2(arg1 As String) As String
            Console.WriteLine(arg1)
            Return arg1
        End Function

        Sub Foo1(arg1 As String)
            Dim s As String = arg1
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Bar1()
            Throw New NotImplementedException()
        End Sub

        Sub Bar2()
            Throw New NotImplementedException()
        End Sub

        Sub FooBar1()
            Throw New NotSupportedException()
        End Sub

        Sub FooBar2()
            Throw New NotSupportedException()
        End Sub

        Function Qux1(val As Integer) As String
            Return val.ToString()
        End Function

        Function Qux2(val As Integer) As String
            Return val.ToString() ' Compliant because we ignore one liner
        End Function

        Sub SubSecondNoParentheses
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Function FunctionNoParentheses As String
            Dim s As String = "test"
            Return s
        End Function

    End Class

    Structure SomeStruct
        Private Sub Foo1()
            '       ^^^^ Secondary
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Private Sub Foo2() ' Noncompliant
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub
    End Structure

    ' https://github.com/SonarSource/sonar-dotnet/issues/9654
    Public Class Repro_9654
        Private Shared Function SameBodyDifferentReturnTypeLiteral1() As Integer
            Console.WriteLine("Test")
            Return 42
        End Function

        Private Shared Function SameBodyDifferentReturnTypeLiteral2() As Double     ' Compliant - different return type
            Console.WriteLine("Test")
            Return 42
        End Function

        Private Shared Function SameReturnTypeWithDifferentName1() As String        ' Secondary [SameReturnTypeWithDifferentName]
            Console.WriteLine("Test")
            Return "A"
        End Function

        Private Shared Function SameReturnTypeWithDifferentName2() As String        ' Noncompliant [SameReturnTypeWithDifferentName]
            Console.WriteLine("Test")
            Return "A"
        End Function

        Private Shared Function SameImplementationWithUnknownReturnType1() As UnkownType1  ' Error[BC30002]
            Console.WriteLine("Test")
            Return "A"
        End Function

        Private Shared Function SameImplementationWithUnknownReturnType2() As UnkownType2  ' Error[BC30002]
            Console.WriteLine("Test")
            Return "A"
        End Function

        Private Shared Sub SubProcedure()
            Console.WriteLine("Test1")
            Console.WriteLine("Test2")
        End Sub

        Private Shared Function Func() As String    ' Compliant - Func() and SubProcedure() has different return types
            Console.WriteLine("Test1")
            Console.WriteLine("Test2")
            Return "A"
        End Function
    End Class

End Namespace
