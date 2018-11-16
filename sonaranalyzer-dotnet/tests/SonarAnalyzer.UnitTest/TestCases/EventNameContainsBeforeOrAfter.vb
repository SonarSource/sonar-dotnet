Imports System
Class Foo
    Event BeforeClose() ' Noncompliant {{Rename this event to remove the 'Before' prefix.}}
'         ^^^^^^^^^^^
    Event afterClose()  ' Noncompliant
    Event Closing()
    Public Custom Event BeforeClose2 As EventHandler ' Noncompliant
        AddHandler(ByVal value As EventHandler)
            Me.Events.AddHandler(Me.EventServerChange, value) ' Error [BC30456,BC30456]
        End AddHandler


        RemoveHandler(ByVal value As EventHandler)
            Me.Events.RemoveHandler(Me.EventServerChange, value) ' Error [BC30456,BC30456]
        End RemoveHandler


        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
            CType(Me.Events(Me.EventServerChange), EventHandler).Invoke(sender, e) ' Error [BC30456,BC30456]
        End RaiseEvent
    End Event
End Class
