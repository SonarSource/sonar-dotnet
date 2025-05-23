﻿Imports System

Namespace Tests.Diagnostics
    Class Program

        Sub SubFirstNoParentheses
            Dim s As String = "test"
            Dim k As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Sub Foo1()
'           ^^^^ Secondary
'           ^^^^ Secondary@-1
'           ^^^^ Secondary@-2

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
'                   ^^^^ Secondary
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub

        Private Sub Foo2() ' Noncompliant
            Dim s As String = "test"
            Console.WriteLine("Result: {0}", s)
        End Sub
    End Structure

End Namespace
