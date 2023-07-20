Imports System
Imports System.Web.UI

Friend Class RestrictDeserializedTypes
    Public Sub DefaultConstructor()
        New LosFormatter()
    End Sub

    Public Sub LiteralExpression()
        New LosFormatter(False, "")
        New LosFormatter(False, New Byte(-1) {})
        New LosFormatter(True, "")
        New LosFormatter(True, New Byte(-1) {})
        New LosFormatter(macKeyModifier:=New Byte(-1) {}, enableMac:=False)
        New LosFormatter(macKeyModifier:=New Byte(-1) {}, enableMac:=True)
    End Sub

    Public Sub FunctionParameter(ByVal condition As Boolean)
        New LosFormatter(condition, "")

        If condition Then
            New LosFormatter(condition, "")
        Else
            New LosFormatter(condition, "")
        End If
    End Sub

    Public Sub LocalVariables()
        Dim trueVar = True
        New LosFormatter(trueVar, "")
        Dim falseVar = False
        New LosFormatter(falseVar, "")
    End Sub

    Public Sub TernaryOp(ByVal condition As Boolean)
        Dim falseVar = If(condition, False, False)
        New LosFormatter(falseVar, "")
        Dim trueVar = If(condition, True, True)
        New LosFormatter(trueVar, "")
        New LosFormatter(If(condition, False, True), "")
    End Sub

    Public Function ExpressionBodyFalse() As LosFormatter
        Return New LosFormatter(False, "")
    End Function

    Public Function ExpressionBodyTrue() As LosFormatter
        Return New LosFormatter(True, "")
    End Function

    Public Sub InLambdaFunction()
        Dim createSafe As Func(Of LosFormatter) = Function() New LosFormatter(True, "")
        Dim createUnsafe As Func(Of LosFormatter) = Function() New LosFormatter(True, "")
    End Sub

    Public Function Switch(ByVal condition As Boolean) As LosFormatter
        Select Case condition
            Case True
                Return New LosFormatter(condition, "")
            Case Else
                Return New LosFormatter(condition, "")
        End Select
    End Function

    Public Sub DataFlow()
        Dim condition = False
        condition = True

        If condition Then
            New LosFormatter(condition, "")
        Else
            New LosFormatter(condition, "")
        End If

        condition = False

        If condition Then
            New LosFormatter(condition, "")
        Else
            New LosFormatter(condition, "")
        End If

        New LosFormatter(New Boolean(), "")
        New LosFormatter(Nothing, "")
    End Sub

End Class
