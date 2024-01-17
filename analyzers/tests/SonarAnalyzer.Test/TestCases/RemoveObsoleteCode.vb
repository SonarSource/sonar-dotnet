Imports System

<Obsolete> ' Noncompliant ^2#8 {{Do not forget to remove this deprecated code someday.}}
Public Class Program

    <Obsolete("Message")>                   ' Noncompliant
    Public Delegate Sub CloseDelegate(sender As Object, eventArgs As EventArgs)

    <Obsolete("Message", True)>             ' Noncompliant
    Public Event OnClose As CloseDelegate

    <ObsoleteAttribute()>                   ' Noncompliant
    Public Sub New()
    End Sub
End Class
