Imports System.Windows.Forms
Imports EmberAPI

Public Class dlgRestart

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgRestart_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(298, "Restart Ember Media Manager?")
        Me.lblHeader.Text = Me.Text
        Me.lblBody.Text = Master.eLang.GetString(299, "Recent changes require a restart of Ember Media Manager to complete.\n\nWould you like to restart Ember Media Manager now?").Replace("\n", vbCrLf)

        Me.OK_Button.Text = Master.eLang.GetString(300, "Yes")
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
    End Sub
End Class
