Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Public Function Foo() As Boolean
            Dim b = System.Environment.NewLine
            Return If(True, FooImpl(True, False), True)
        End Function

        Public Function FooImpl(ByVal isMale As Boolean, ByVal isMarried As Boolean) As Boolean
            Dim x = If(isMale, "Mr. ", If(isMarried, "Mrs. ", "Miss ")) ' Noncompliant {{Extract this nested If operator into independent If...Then...Else statements.}}
'                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim x2 = If(isMale, "Mr. ", If(isMarried, "Mrs. ", If(True, "Miss ", "what? ")))
'                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
'                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1
            Dim x3 = If(isMale, "Mr. ",
                If(isMarried, "Mrs. ", "Miss "))
'               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        End Function
    End Class
End Namespace
