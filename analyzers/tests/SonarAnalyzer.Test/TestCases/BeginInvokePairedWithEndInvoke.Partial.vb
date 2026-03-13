Imports System

Partial Class CrossTreeCallbackFieldVB
    Private CallbackFieldNoncompliant As New AsyncCallback(AddressOf HandlerWithoutEndInvoke)

    Private Sub HandlerWithoutEndInvoke(Result As IAsyncResult)
    End Sub
End Class
