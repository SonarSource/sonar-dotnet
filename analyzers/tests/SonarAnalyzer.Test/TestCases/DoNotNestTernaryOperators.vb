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

            Dim x4 = If(isMale, Function() "Mr.", Function() If(isMarried, "Mrs.", "Miss")) ' Compliant. Ternary expressions in lambdas are not considered nested.
            Dim x5 = If(isMale, Sub() PrintGreeting("Mr."),  Sub() PrintGreeting(If(isMarried, "Mrs.", "Miss")))
            Dim x6 = If(isMale, Function()
                                    Return "Mr."
                                End Function,
                                Function()
                                    Return If(isMarried, "Mrs.", "Miss")
                                End Function)
            Dim x7 = If(isMale, Sub()
                                    PrintGreeting("Mr.")
                                End Sub,
                                Sub()
                                    PrintGreeting(If(isMarried, "Mrs.", "Miss"))
                                End Sub)

        End Function

        Private Sub PrintGreeting(greeting As String)
        End Sub
    End Class
End Namespace
