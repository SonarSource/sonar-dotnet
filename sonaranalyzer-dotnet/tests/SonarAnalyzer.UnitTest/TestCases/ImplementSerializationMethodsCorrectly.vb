Imports System
Imports System.Runtime.Serialization

Namespace Tests.Diagnostics
    <Serializable>
    Public Class Foo
        <OnSerializing>
        Public Sub OnSerializing(ByVal context As StreamingContext) ' Noncompliant {{Make this method 'Private'.}}
'                  ^^^^^^^^^^^^^
        End Sub

        <OnSerialized>
        Private Function OnSerialized(ByVal context As StreamingContext) As Integer ' Noncompliant {{Make this method a 'Sub' not a 'Function'.}}
        End Function

        <OnDeserializing>
        Private Sub OnDeserializing() ' Noncompliant  {{Make this method have a single parameter of type 'StreamingContext'.}}
        End Sub

        <OnDeserialized>
        Private Sub OnDeserialized(ByVal context As StreamingContext, ByVal str As String) ' Noncompliant {{Make this method have a single parameter of type 'StreamingContext'.}}
        End Sub

        Private Sub OnDeserialized2(ByVal context As StreamingContext, ByVal str As String)
        End Sub

        <OnDeserialized>
        Private Sub OnDeserialized(Of T)(ByVal context As StreamingContext) ' Noncompliant {{Make this method have no type parameters.}}
        End Sub

        <OnDeserializing>
        Public Function OnDeserializing2(ByVal context As StreamingContext) As Integer ' Noncompliant {{Make this method 'Private' and a 'Sub' not a 'Function'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Sub OnDeserializing3() ' Noncompliant {{Make this method 'Private' and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Sub

        <OnDeserializing>
        Private Function OnDeserializing4() As Integer ' Noncompliant {{Make this method a 'Sub' not a 'Function' and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Function OnDeserializing5(Of T)() As Integer ' Noncompliant {{Make this method 'Private', a 'Sub' not a 'Function', have no type parameters and have a single parameter of type 'StreamingContext'.}}
            Throw New NotImplementedException()
        End Function

        <OnDeserializing>
        Public Function () As String ' Noncompliant ' Error [BC30203]
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
