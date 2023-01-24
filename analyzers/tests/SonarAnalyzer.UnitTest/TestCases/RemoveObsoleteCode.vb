Imports System

Namespace Tests

    <Obsolete> ' Noncompliant ^6#8 {{Do not forget to remove this deprecated code someday.}}
    Public Class Program

        <Obsolete("Message")>                   ' Noncompliant
        Public Delegate Sub CloseDelegate(ByVal sender As Object, ByVal eventArgs As EventArgs)

        <Obsolete("Message", True)>             ' Noncompliant
        Public Event OnClose As CloseDelegate

        <ObsoleteAttribute()>                            ' Noncompliant
        Public Sub New()
        End Sub
    End Class
End Namespace
