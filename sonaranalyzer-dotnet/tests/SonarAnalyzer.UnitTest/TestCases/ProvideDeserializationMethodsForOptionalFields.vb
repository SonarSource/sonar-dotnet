Imports System
Imports System.Runtime.Serialization

Namespace Tests.Diagnostics
    Public Class NoEventHandlerMethods ' Noncompliant {{Add deserialization event handlers.}}
'                ^^^^^^^^^^^^^^^^^^^^^
        <OptionalField>
        Private optionalField As Integer = 5
    End Class

    Public Class OnlyOnDeserializingEventHandlerMethod '  Noncompliant {{Add the missing 'OnDeserializedAttribute' event handler.}}
        <OptionalFieldAttribute>
        Private optionalField As Integer = 5

        <OnDeserializing>
        Private Sub OnDeserializing(ByVal context As StreamingContext)
            optionalField = 5
        End Sub
    End Class

    Public Class OnlyOnDeserializedEventHandlerMethod ' Noncompliant {{Add the missing 'OnDeserializingAttribute' event handler.}}
        <OptionalField>
        Private optionalField As Integer = 5

        <OnDeserialized>
        Private Sub OnDeserialized(ByVal context As StreamingContext)
        End Sub
    End Class

    <Serializable>
    Public Class Compliant
        <OptionalField>
        Private optionalField As Integer = 5

        <OnDeserializing>
        Private Sub OnDeserializing(ByVal context As StreamingContext)
            optionalField = 5
        End Sub

        <OnDeserialized>
        Private Sub OnDeserialized(ByVal context As StreamingContext)
        End Sub
    End Class
End Namespace
