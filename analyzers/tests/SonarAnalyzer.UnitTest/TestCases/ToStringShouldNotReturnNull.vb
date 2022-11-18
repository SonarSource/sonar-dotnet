Imports System

Class Condition
    Shared Function [When]() As Boolean
        Return True
    End Function
End Class

Namespace Compliant
    Class OtherMethodReturnsNothingString
        Function Returns() As String
            Return Nothing
        End Function
    End Class

    Class ReturnsSomeString
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return "Hello, world!"
            End If

            Return "Hello, world!"
        End Function
    End Class

    Class ReturnsEmptyString
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return ""
            End If

            Return ""
        End Function
    End Class

    Class ReturnsStringEmpty
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return String.Empty
            End If

            Return String.Empty
        End Function
    End Class

    Class LambdaReturnsNothing
        Public Overrides Function ToString() As String
            Dim lambda As Func(Of String) =
                Function() As String
                    Return Nothing ' Compliant
                End Function

            Return String.Empty
        End Function
    End Class

    Structure StructReturnsStringEmpty
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return String.Empty
            End If

            Return String.Empty
        End Function
    End Structure

    Class ToString

        Public Function SomeMethod() As String
            Return Nothing 'Compliant
        End Function

    End Class

    Class BinaryConditionalExpressionNotSupported

        Public Function SomeMethod() As String
            Return If(Nothing, Nothing) ' Not supported, doesn't make sense
        End Function

    End Class

    Class ToStringSharedMethod

        Public Shared Function ToString() As String
            Return Nothing
        End Function

    End Class

End Namespace

Namespace Noncompliant

    Public Class ReturnsNothing
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return Nothing ' Noncompliant
            '   ^^^^^^^^^^^^^^
            End If

            Return Nothing ' Noncompliant {{Return an empty string instead.}}
        End Function
    End Class

    Public Class ReturnsNothingConditionaly

        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return Nothing ' Noncompliant
            End If

            Return "not-null"
        End Function
    End Class

    Public Class ReturnsNothingViaTernary
        Public Overrides Function ToString() As String
            Return If(Condition.[When](), Nothing, "")  ' Noncompliant
        End Function
    End Class

    Public Class ReturnsNullViaNestedTenary

        Public Overrides Function ToString() As String
            Return If(Condition.When(), ' Noncompliant
                If(Condition.When(), Nothing, "something"),
                If(Condition.When(), "something", Nothing))
        End Function

    End Class

    Structure StructReturnsNothing
        Public Overrides Function ToString() As String
            If Condition.[When]() Then
                Return Nothing ' Noncompliant
            End If

            Return Nothing ' Noncompliant
        End Function
    End Structure

End Namespace
