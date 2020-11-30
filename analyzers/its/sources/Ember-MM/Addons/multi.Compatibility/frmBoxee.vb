Imports EmberAPI

Public Class frmBoxee


#Region "Events"
    Public Event ModuleEnabledChanged(ByVal State As Boolean, ByVal difforder As Integer)
    Public Event ModuleSettingsChanged()
    Public Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object))
#End Region 'Events

    Private Sub chkEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleEnabledChanged(chkEnabled.Checked, 0)
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Private Sub SetUp()
		Me.chkEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
		Me.chkBoxeeId.Text = Master.eLang.GetString(15, "Replace ID field with Boxee Id Field")
		Me.GroupBox1.Text = Master.eLang.GetString(16, "TV Show Options")
    End Sub


    Private Sub chkBoxeeId_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBoxeeId.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub
End Class