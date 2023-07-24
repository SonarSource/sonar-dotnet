Imports System
Imports System.Web.UI

Class RestrictDeserializedTypes
    Public Sub DefaultConstructor()
        Dim formatter = New LosFormatter()                                                      ' Noncompliant {{Serialized data signature (MAC) should be verified.}}
    End Sub

    Public Sub LiteralExpression()
        Dim formatter1 = New LosFormatter(False, "")                                            ' Noncompliant
        Dim formatter2 = New LosFormatter(False, New Byte(-1) {})                               ' Noncompliant
        Dim formatter3 = New LosFormatter(True, "")                                             ' Compliant - MAC filtering is enabled
        Dim formatter4 = New LosFormatter(True, New Byte(-1) {})                                ' Compliant
        Dim formatter5 = New LosFormatter(macKeyModifier:=New Byte(-1) {}, enableMac:=False)    ' Noncompliant
        Dim formatter6 = New LosFormatter(macKeyModifier:=New Byte(-1) {}, enableMac:=True)     ' Compliant
    End Sub

    Public Sub FunctionParameter(condition As Boolean)
        Dim formatter1 = New LosFormatter(condition, "")                                        ' Noncompliant

        If condition Then
            Dim formatter2 = New LosFormatter(condition, "")                                    ' Compliant
        Else
            Dim formatter3 = New LosFormatter(condition, "")                                    ' Noncompliant
        End If
    End Sub

    Public Sub LocalVariables()
        Dim trueVar = True
        Dim formatter1 = New LosFormatter(trueVar, "")                                          ' Compliant
        Dim falseVar = False
        Dim formatter2 = New LosFormatter(falseVar, "")                                         ' Noncompliant
    End Sub

    Public Sub TernaryOp(condition As Boolean)
        Dim falseVar = If(condition, False, False)
        Dim formatter1 = New LosFormatter(falseVar, "")                                         ' Noncompliant
        Dim trueVar = If(condition, True, True)
        Dim formatter2 = New LosFormatter(trueVar, "")                                          ' Compliant
        Dim formatter3 = New LosFormatter(If(condition, False, True), "")                       ' Noncompliant
    End Sub

    Public Function UnsafeReturnValue() As LosFormatter
        Return New LosFormatter(False, "")                                                      ' Noncompliant
    End Function

    Public Function SafeReturnValue() As LosFormatter
        Return New LosFormatter(True, "")
    End Function

    Public Sub InLambdaFunction()
        Dim createSafe As Func(Of LosFormatter) = Function() New LosFormatter(True, "")         ' Compliant
        Dim createUnsafe As Func(Of LosFormatter) = Function() New LosFormatter(False, "")      ' Noncompliant
    End Sub

    Public Function Switch(condition As Boolean) As LosFormatter
        Select Case condition
            Case True
                Return New LosFormatter(condition, "")                                          ' Compliant
            Case Else
                Return New LosFormatter(condition, "")                                          ' Noncompliant
        End Select
    End Function

    Public Sub DataFlow()
        Dim condition = False
        condition = True

        If condition Then
            Dim formatter1 = New LosFormatter(condition, "")
        Else
            Dim formatter2 = New LosFormatter(condition, "")                                    ' Unreachable
        End If

        condition = False

        If condition Then
            Dim formatter3 = New LosFormatter(condition, "")                                    ' Unreachable
        Else
            Dim formatter4 = New LosFormatter(condition, "")                                    ' Noncompliant
        End If

        Dim formatter5 = New LosFormatter(New Boolean(), "")                                    ' Noncompliant
        Dim formatter6 = New LosFormatter(Nothing, "")                                          ' Noncompliant
    End Sub

End Class
