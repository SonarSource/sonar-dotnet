Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Formatters.Soap

Imports System.Web.UI

Imports AliasedBinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter


Friend Class Serializer

    Friend Sub BinaryFormatterDeserialize(ByVal memoryStream As MemoryStream)
        Call New BinaryFormatter().Deserialize(memoryStream)                    ' Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        '    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    End Sub

    Friend Sub NetDataContractSerializerDeserialize()
        Call New NetDataContractSerializer().Deserialize(New MemoryStream())    ' Noncompliant {{Restrict types of objects allowed to be deserialized.}}
    End Sub

    Friend Sub SoapFormatterDeserialize()
        Call New SoapFormatter().Deserialize(New MemoryStream())                ' Noncompliant {{Restrict types of objects allowed to be deserialized.}}
    End Sub

    Friend Sub BinderAsVariable(ByVal stream As Stream, ByVal condition As Boolean)
        Dim safeBinder = New SafeBinderStatementWithReturnNothing()
        Dim unsafeBinder = New UnsafeBinder()
        Dim nothingBinder As SerializationBinder = Nothing

        Dim formatter1 = New BinaryFormatter()
        formatter1.Binder = safeBinder
        formatter1.Deserialize(stream)      ' Compliant: safe binder was used

        Dim formatter2 = New BinaryFormatter()
        formatter2.Binder = unsafeBinder
        formatter2.Deserialize(stream)      ' Noncompliant [unsafeBinder1]: unsafe binder used

        Dim formatter3 = New BinaryFormatter()
        formatter3.Binder = nothingBinder
        formatter3.Deserialize(stream)      ' Noncompliant: the binder is Nothing

        Dim possibleNothingBinder = If(condition, Nothing, New SafeBinderStatementWithReturnNothing())
        Dim formatter4 = New BinaryFormatter()
        formatter4.Binder = possibleNothingBinder
        formatter4.Deserialize(stream)      ' Noncompliant: the binder is Nothing

        Dim formatter5 = New BinaryFormatter()
        If condition Then
            formatter5.Binder = New SafeBinderStatementWithReturnNothing()
        End If
        formatter5.Deserialize(stream)      ' Noncompliant: the binder is Nothing

        Dim formatter6 = New BinaryFormatter()
        formatter6.Binder = New SafeBinderExpressionWithNothing()
        formatter6.Binder = New UnsafeBinder()
        formatter6.Deserialize(stream)      ' Noncompliant [unsafeBinder2]: the last binder set is unsafe

        Dim formatter7 = New BinaryFormatter With {.Binder = New SafeBinderExpressionWithNothing()}
        formatter7.Binder = New UnsafeBinder()
        formatter7.Deserialize(stream)      ' Noncompliant [unsafeBinder3]: the last binder set is unsafe

        Dim formatter8 = New BinaryFormatter()
        formatter8.Binder = New UnsafeBinder()
        formatter8.Binder = New SafeBinderExpressionWithNothing()
        formatter8.Deserialize(stream)      ' Compliant: the last binder set is safe

        Dim formatter9 = New BinaryFormatter With {.Binder = New UnsafeBinder()}
        formatter9.Binder = New SafeBinderExpressionWithNothing()
        formatter9.Deserialize(stream)      ' Compliant: the last binder set is safe

        Dim formatter10 = New BinaryFormatter With {.Binder = New UnsafeBinder()}
        formatter10.Deserialize(stream)     ' Noncompliant [unsafeBinder4]: the safe binder was set after deserialize call
        formatter10.Binder = New SafeBinderExpressionWithNothing()

        Dim formatter15 = New BinaryFormatter()
        Dim formatter16 = New BinaryFormatter()
        Try
            formatter15.Binder = New SafeBinderExpressionWithNothing()
            formatter15.Deserialize(stream)         ' Compliant: safe binder

            formatter16.Binder = New UnsafeBinder()
            formatter16.Deserialize(stream)         ' Noncompliant [unsafeBinder5]: unsafe binder
        Catch
            formatter15.Deserialize(stream)         ' Noncompliant
            formatter16.Deserialize(stream)         ' Noncompliant
        End Try

        While True
            Dim formatter17 = New BinaryFormatter With {.Binder = New SafeBinderExpressionWithNothing()}
            formatter17.Deserialize(stream)

            Dim formatter18 = New BinaryFormatter With {.Binder = New UnsafeBinder()}
            formatter18.Deserialize(stream)         ' Noncompliant [unsafeBinder6]: unsafe binder
        End While

    End Sub

    Private Sub Functions(ByVal stream As Stream)
        Dim binderFactoryUnsafe As Func(Of UnsafeBinder) = Function() New UnsafeBinder()
        Call New BinaryFormatter With {.Binder = binderFactoryUnsafe()}.Deserialize(stream) ' Noncompliant [unsafeBinder7]: unsafe binder used

        Dim binderFactorySafe As Func(Of SafeBinderExpressionWithNothing) = Function() New SafeBinderExpressionWithNothing()
        Call New BinaryFormatter With {.Binder = binderFactorySafe()}.Deserialize(stream)
        Call New BinaryFormatter With {.Binder = binderFactoryUnsafe()}.Deserialize(stream) ' Noncompliant [unsafeBinder8]: unsafe binder used
        Call New BinaryFormatter With {.Binder = binderFactorySafe()}.Deserialize(stream)
    End Sub

    Private Shared Function BinderFactoryUnsafe() As UnsafeBinder
        Return New UnsafeBinder()
    End Function

    Private Shared Function BinderFactorySafe() As SafeBinderExpressionWithNothing
        Return New SafeBinderExpressionWithNothing()
    End Function

    Friend Sub DeserializeOnExpression(ByVal memoryStream As MemoryStream, ByVal condition As Boolean)
        Call New BinaryFormatter().Deserialize(memoryStream)                                                            ' Noncompliant: the binder is Nothing
        Call New BinaryFormatter With {.Binder = Nothing}.Deserialize(memoryStream)                                     ' Noncompliant: the binder is Nothing
        Call (If(condition, New BinaryFormatter(), Nothing)).Deserialize(memoryStream)                                  ' FN: the binder is Nothing
        Dim bin As BinaryFormatter = Nothing
        Call New BinaryFormatter With {.Binder = New SafeBinderStatementWithReturnNothing()}.Deserialize(memoryStream)  ' Compliant: the binder is safe
        Call New BinaryFormatter With {.Binder = New UnsafeBinder()}.Deserialize(memoryStream)                          ' Noncompliant [unsafeBinder9]: the binder is unsafe
        Call (If(condition, New BinaryFormatter With {.Binder = New SafeBinderStatementWithReturnNothing()}, New BinaryFormatter With {.Binder = New SafeBinderWithThrowStatement()})).Deserialize(memoryStream)    ' Compliant: the binder is safe in all cases
    End Sub

    Friend Sub Switch(ByVal stream As Stream, ByVal condition As Boolean, ByVal number As Integer)
        Dim formatter4 = New BinaryFormatter()

        Select Case number
            Case 1
                formatter4.Deserialize(stream)                              ' Noncompliant: the binder is Nothing
            Case 2
                formatter4.Binder = Nothing
                formatter4.Deserialize(stream)                              ' Noncompliant [nothingBinder1]: the binder is Nothing
            Case 3
                formatter4.Binder = New UnsafeBinder()
                formatter4.Deserialize(stream)                              ' Noncompliant [unsafeBinder10]: the binder is unsafe
            Case Else
                formatter4.Binder = New SafeBinderExpressionWithNothing()
                formatter4.Deserialize(stream)                              ' Compliant: the binder is safe
        End Select
    End Sub

    Friend Sub BinderCases(ByVal memoryStream As MemoryStream)
        Dim formatter = New BinaryFormatter()
        formatter.Binder = New SafeBinderStatementWithReturnNothing()
        formatter.Deserialize(memoryStream)                             ' Compliant: the binder is safe
        formatter.Binder = New SafeBinderWithThrowStatement()
        formatter.Deserialize(memoryStream)                             ' Compliant: the binder is safe
        formatter.Binder = New SafeBinderWithThrowExpression()
        formatter.Deserialize(memoryStream)                             ' Compliant: the binder is safe
        formatter.Binder = New UnsafeBinder()
        formatter.Deserialize(memoryStream)                             ' Noncompliant [unsafeBinder11]: the binder is unsafe
        formatter.Binder = New UnsafeBinderExpressionBody()
        formatter.Deserialize(memoryStream)                             ' Noncompliant [unsafeBinder12]: the binder is unsafe
        formatter.Binder = New SafeBinderWithOtherMethods()
        formatter.Deserialize(memoryStream)                             ' Compliant: the binder is safe
        formatter.Binder = New UnsafeBinderWithOtherMethods()
        formatter.Deserialize(memoryStream)                             ' Noncompliant [unsafeBinder13]: the binder is unsafe
    End Sub

    Friend Sub UnknownBindersAreSafe(ByVal binder As SerializationBinder, ByVal condition As Boolean)
        Call New BinaryFormatter With {.Binder = binder}.Deserialize(New MemoryStream())
        Dim formatter = New BinaryFormatter With {.Binder = binder}
        formatter.Deserialize(New MemoryStream())
        Call New BinaryFormatter() With {.Binder = If(condition, CType(New SafeBinderExpressionWithNothing(), SerializationBinder), New UnsafeBinder())}.Deserialize(New MemoryStream())    ' Noncompliant [unsafeBinder14]: the binder is unsafe on at least one path
    End Sub

    Friend Function UnknownBinaryFormattersAreSafeByDefault(ByVal formatter As BinaryFormatter) As BinaryFormatter
        formatter.Deserialize(New MemoryStream())
        formatter.Binder = New UnsafeBinder()
        formatter.Deserialize(New MemoryStream())                       ' Noncompliant [unsafeBinder15]: the binder is unsafe
        formatter = UnknownBinaryFormattersAreSafeByDefault(Nothing)
        formatter.Deserialize(New MemoryStream())
        formatter.Binder = New UnsafeBinder()
        formatter.Deserialize(New MemoryStream())                       ' Noncompliant [unsafeBinder16]: the binder is unsafe
        BinaryFormatterField.Deserialize(New MemoryStream())
        BinaryFormatterField.Binder = New UnsafeBinder()
        BinaryFormatterField.Deserialize(New MemoryStream())            ' Noncompliant [unsafeBinder17]: the binder is unsafe
        Return Nothing
    End Function

    Friend Sub UnknownFormattersAreSafeByDefault(ByVal formatter As IFormatter)
        formatter.Binder = New UnsafeBinder()
        formatter.Deserialize(New MemoryStream())
    End Sub

    Private BinaryFormatterField As BinaryFormatter
End Class



Namespace Aliases
    Class BinaryFormatter
        Private Property Binder As SerializationBinder

        Private Function Deserialize(ByVal serializationStream As Stream) As Object
            Return Nothing
        End Function

        Private Sub Test()
            Call New AliasedBinaryFormatter().Deserialize(New MemoryStream())   ' Noncompliant: the binder is Nothing
            Call New BinaryFormatter().Deserialize(New MemoryStream())
        End Sub
    End Class
End Namespace


Friend NotInheritable Class SafeBinderStatementWithReturnNothing
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As System.String) As Type
        If typeName = "typeT" Then
            Return Assembly.Load(assemblyName).[GetType](typeName)
        End If

        Return Nothing
    End Function

End Class

Friend NotInheritable Class SafeBinderExpressionWithNothing
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        Return If(typeName = "typeT", Assembly.Load(assemblyName).[GetType](typeName), Nothing)
    End Function
End Class

Friend NotInheritable Class SafeBinderStatementWithNothing
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        Return If(typeName = "typeT", Assembly.Load(assemblyName).[GetType](typeName), Nothing)
    End Function

End Class

Friend NotInheritable Class SafeBinderWithThrowStatement
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        If typeName = "typeT" Then
            Return Assembly.Load(assemblyName).[GetType](typeName)
        Else
            Throw New SerializationException("An exception has occurred.")
        End If
    End Function

End Class

Friend NotInheritable Class SafeBinderWithThrowExpression
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        If typeName = "typeT" Then
            Return Assembly.Load(assemblyName).[GetType](typeName)
        Else
            Throw New SerializationException("An exception has occurred.")
        End If
    End Function

End Class

Friend NotInheritable Class UnsafeBinder
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type    ' FP: nothingBinder1
        '                     ^^^^^^^^^^ Secondary [unsafeBinder1, unsafeBinder2, unsafeBinder3, unsafeBinder4, unsafeBinder5, unsafeBinder6, unsafeBinder7, unsafeBinder8, unsafeBinder9, unsafeBinder10, unsafeBinder11, unsafeBinder14, unsafeBinder15, unsafeBinder16, unsafeBinder17, nothingBinder1] {{This method allows all types.}}
        Return Assembly.Load(assemblyName).[GetType](typeName)
    End Function
End Class

Friend NotInheritable Class UnsafeBinderExpressionBody
    Inherits SerializationBinder

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        '                     ^^^^^^^^^^ Secondary [unsafeBinder12] {{This method allows all types.}}
        Return Assembly.Load(assemblyName).[GetType](typeName)
    End Function

End Class

Friend NotInheritable Class UnsafeBinderWithOtherMethods
    Inherits SerializationBinder

    Public Sub Accept()
    End Sub

    Public Function ResolveType(ByVal id As String) As Type
        Throw New SerializationException("Not implemented.")
    End Function

    Public Overloads Function BindToType(ByVal assemblyName As String) As Type
        Throw New SerializationException("Not implemented.")
    End Function

    Public Overloads Function BindToType(ByVal assemblyName As String, ByVal typeName As Integer) As Type
        Throw New SerializationException("Not implemented.")
    End Function

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        '                     ^^^^^^^^^^ Secondary [unsafeBinder13] {{This method allows all types.}}
        Return Assembly.Load(assemblyName).[GetType](typeName)
    End Function
End Class

Friend NotInheritable Class SafeBinderWithOtherMethods
    Inherits SerializationBinder

    Public Sub Accept()
    End Sub

    Public Function ResolveType(ByVal id As String) As Type
        Return Type.[GetType](id)
    End Function

    Public Overrides Function BindToType(ByVal assemblyName As String, ByVal typeName As String) As Type
        Throw New SerializationException("Only typeT is allowed")
    End Function
End Class
