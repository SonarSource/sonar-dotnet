Imports System
Imports System.Runtime.Serialization

Namespace Tests.Diagnostics
    <Serializable>
    Public Class Foo
        <OnSerializing>
        Public Sub OnSerializing(ByVal context As StreamingContext) ' Noncompliant {{Make this method 'private'.}}
'                  ^^^^^^^^^^^^^
        End Sub

        <OnSerialized>
        Private Function OnSerialized(ByVal context As StreamingContext) As Integer ' Noncompliant {{Make this method return 'void'.}}
        End Function

        <OnDeserializing>
        Private Sub OnDeserializing() ' Noncompliant  {{Make this method have a single parameter of type 'StreamingContext'.}}
        End Sub

        <OnDeserialized>
        Private Sub OnDeserialized(ByVal context As StreamingContext, ByVal str As String) ' Noncompliant {{Make this method have a single parameter of type 'StreamingContext'.}}
        End Sub

        Private Sub OnDeserialized(ByVal context As StreamingContext, ByVal str As String)
        End Sub

        <OnDeserialized>
        Private Sub OnDeserialized(Of T)(ByVal context As StreamingContext) ' Noncompliant {{Make this method have no type parameters.}}
        End Sub

        <OnDeserializing>
        Public Function OnDeserializing2(ByVal context As StreamingContext) As Integer ' Noncompliant {{Make this method 'private' and return 'void'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Sub OnDeserializing3() ' Noncompliant {{Make this method 'private' and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Sub

        <OnDeserializing>
        Private Function OnDeserializing4() As Integer ' Noncompliant {{Make this method return 'void' and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Function OnDeserializing5(Of T)() As Integer ' Noncompliant {{Make this method 'private', return 'void', have no type parameters and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Function () As String ' Noncompliant
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
