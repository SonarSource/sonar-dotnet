Imports System
Imports System.Web.Script.Serialization

Class Serializer
    Public Sub JavaScriptSerializerDefaultConstructorIsSafe(json As String)
        Call New JavaScriptSerializer().Deserialize(Of String)(json)                                        ' Compliant - default constructor is considered safe
    End Sub

    Private Shared Function CtorInitializer() As JavaScriptSerializer
        Return New JavaScriptSerializer(New SimpleTypeResolver()) With {                                    ' Compliant: deserialize method is not called
            .MaxJsonLength = Integer.MaxValue
        }
    End Function

    Public Sub NullResolverIsSafe(json As String)
        Call New JavaScriptSerializer(Nothing).Deserialize(Of String)(json)                                 ' Compliant - a null resolver is considered safe
    End Sub

    Public Sub SimpleTypeResolverIsNotSafe(json As String)
        Call New JavaScriptSerializer(New SimpleTypeResolver()).Deserialize(Of String)(json)                ' Noncompliant {{Restrict types of objects allowed to be deserialized.}}
    End Sub

    Public Sub CustomResolver(json As String)
        Call New JavaScriptSerializer(New UnsafeTypeResolver()).Deserialize(Of String)(json)                ' Noncompliant [unsafeResolver1]: unsafe resolver
        Call New JavaScriptSerializer(New SafeTypeResolver()).Deserialize(Of String)(json)                  ' Compliant: safe resolver
        Call New JavaScriptSerializer(New UnsafeResolverWithOtherMethods()).Deserialize(Of String)(json)    ' Noncompliant [unsafeResolver2]: unsafe resolver
        Call New JavaScriptSerializer(New SafeTypeResolverWithOtherMethods()).Deserialize(Of String)(json)  ' Compliant: safe resolver
    End Sub

    Public Sub UnknownResolverType(json As String, resolver As JavaScriptTypeResolver)
        Call New JavaScriptSerializer(resolver).Deserialize(Of String)(json)                                ' Compliant: the resolver type is known only at runtimme
    End Sub

    Public Sub LocalVariable(json As String)
        Dim unsafeResolver = New UnsafeTypeResolver()
        Dim safeResolver = New SafeTypeResolver()

        Dim serializer1 = New JavaScriptSerializer(unsafeResolver)
        serializer1.Deserialize(Of String)(json)                                                            ' Noncompliant [unsafeResolver3]: unsafe resolver

        Dim serializer2 = New JavaScriptSerializer(safeResolver)
        serializer2.Deserialize(Of String)(json)                                                            ' Compliant: safe resolver
    End Sub

    Public Function LambdaSafe(json As String) As String
        Return New JavaScriptSerializer(New SafeTypeResolver()).Deserialize(Of String)(json)                ' Compliant: safe resolver
    End Function

    Public Function LambdaUnsafe(json As String) As String
        Return New JavaScriptSerializer(New UnsafeTypeResolver()).Deserialize(Of String)(json)              ' Noncompliant [unsafeResolver4] unsafe resolver
    End Function
End Class

Class UnsafeTypeResolver
    Inherits JavaScriptTypeResolver
    Public Overrides Function ResolveType(id As String) As Type
        '                     ^^^^^^^^^^^ Secondary [unsafeResolver1, unsafeResolver3, unsafeResolver4] {{This method allows all types.}}
        Return Type.GetType(id)
    End Function

    Public Overrides Function ResolveTypeId(type As Type) As String
        Throw New NotImplementedException()
    End Function
End Class

Class SafeTypeResolver
    Inherits JavaScriptTypeResolver
    Public Overrides Function ResolveType(id As String) As Type
        Throw New NotImplementedException()
    End Function

    Public Overrides Function ResolveTypeId(type As Type) As String
        Throw New NotImplementedException()
    End Function
End Class

Class SafeTypeResolverWithOtherMethods
    Inherits JavaScriptTypeResolver
    Public Function BindToType(assemblyName As String, typeName As String) As Type
        Return Type.GetType(typeName)
    End Function

    Public Overloads Function ResolveType(id As String, wrongNumberOfParameters As String) As Type
        Return Type.GetType(id)
    End Function

    Public Overrides Function ResolveType(id As String) As Type
        Throw New NotImplementedException()
    End Function

    Public Overrides Function ResolveTypeId(type As Type) As String
        Return String.Empty
    End Function
End Class

Class UnsafeResolverWithOtherMethods
    Inherits JavaScriptTypeResolver
    Public Function BindToType(assemblyName As String, typeName As String) As Type
        Throw New NotImplementedException()
    End Function

    Public Overloads Function ResolveType(id As String, wrongNumberOfParameters As String) As Type
        Throw New NotImplementedException()
    End Function

    Public Overrides Function ResolveType(id As String) As Type
        '                     ^^^^^^^^^^^ Secondary [unsafeResolver2] {{This method allows all types.}}
        Return Type.GetType(id)
    End Function

    Public Overrides Function ResolveTypeId(type As Type) As String
        Throw New NotImplementedException()
    End Function
End Class
