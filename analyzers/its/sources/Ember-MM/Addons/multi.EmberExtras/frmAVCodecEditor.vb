Imports System.Windows.Forms
Imports EmberAPI

Public Class frmAVCodecEditor
    Public Event ModuleSettingsChanged()

    Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("AudioFormatConvert:"))
            Dim i As Integer = dgvAudio.Rows.Add(New Object() {sett.Name.Substring(19), sett.Value})
            If Not sett.DefaultValue = String.Empty Then
                dgvAudio.Rows(i).Tag = True
                dgvAudio.Rows(i).Cells(0).ReadOnly = True
                'dgvAudio.Rows(i).Cells(0).Style.SelectionBackColor = Drawing.Color.White
                dgvAudio.Rows(i).Cells(0).Style.SelectionForeColor = Drawing.Color.Red
            Else
                dgvAudio.Rows(i).Tag = False
            End If

        Next
        For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("VideoFormatConvert:"))
            Dim i As Integer = dgvVideo.Rows.Add(New Object() {sett.Name.Substring(19), sett.Value})
            If Not sett.DefaultValue = String.Empty Then
                dgvVideo.Rows(i).Tag = True
                dgvVideo.Rows(i).Cells(0).ReadOnly = True
                'dgvVideo.Rows(i).Cells(0).Style.SelectionBackColor = Drawing.Color.White
                dgvVideo.Rows(i).Cells(0).Style.SelectionForeColor = Drawing.Color.Red
            Else
                dgvVideo.Rows(i).Tag = False
            End If

        Next
        dgvAudio.ClearSelection()
        dgvVideo.ClearSelection()
        SetUp()
    End Sub
    Private Sub btnAddAudio_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddAudio.Click
        Dim i As Integer = dgvAudio.Rows.Add(New Object() {String.Empty, String.Empty})
        dgvAudio.Rows(i).Tag = False
        dgvAudio.CurrentCell = dgvAudio.Rows(i).Cells(0)
        dgvAudio.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub
    Private Sub btnAddVideo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddVideo.Click
        Dim i As Integer = dgvVideo.Rows.Add(New Object() {String.Empty, String.Empty})
        dgvVideo.Rows(i).Tag = False
        dgvVideo.CurrentCell = dgvVideo.Rows(i).Cells(0)
        dgvVideo.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub btnRemoveAudio_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAudio.Click
        If dgvAudio.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvAudio.Rows(dgvAudio.SelectedCells(0).RowIndex).Tag) Then
            dgvAudio.Rows.RemoveAt(dgvAudio.SelectedCells(0).RowIndex)
            RaiseEvent ModuleSettingsChanged()
        End If
    End Sub
    Private Sub btnRemoveVideo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveVideo.Click
        If dgvVideo.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvVideo.Rows(dgvVideo.SelectedCells(0).RowIndex).Tag) Then
            dgvVideo.Rows.RemoveAt(dgvVideo.SelectedCells(0).RowIndex)
            RaiseEvent ModuleSettingsChanged()
        End If
    End Sub
    Private Sub dgvAudio_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvAudio.CurrentCellDirtyStateChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub
    Private Sub dgvVideo_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvVideo.CurrentCellDirtyStateChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub dgvAudio_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvAudio.SelectionChanged
        If dgvAudio.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvAudio.Rows(dgvAudio.SelectedCells(0).RowIndex).Tag) Then
            btnRemoveAudio.Enabled = True
        Else
            btnRemoveAudio.Enabled = False
        End If
    End Sub

    Private Sub dgvVideo_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvVideo.SelectionChanged
        If dgvVideo.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvVideo.Rows(dgvVideo.SelectedCells(0).RowIndex).Tag) Then
            btnRemoveVideo.Enabled = True
        Else
            btnRemoveVideo.Enabled = False
        End If
    End Sub


    Private Sub dgvAudio_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvAudio.KeyDown
        e.Handled = (e.KeyCode = Keys.Enter)
    End Sub

    Private Sub dgvVideo_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvVideo.KeyDown
        e.Handled = (e.KeyCode = Keys.Enter)
    End Sub

    Sub SetUp()
        btnAddAudio.Text = Master.eLang.GetString(28, "Add", True)
        btnAddVideo.Text = Master.eLang.GetString(28, "Add", True)
        btnRemoveAudio.Text = Master.eLang.GetString(30, "Remove", True)
        btnRemoveVideo.Text = Master.eLang.GetString(30, "Remove", True)
        Label1.Text = Master.eLang.GetString(30, "Audio")
        Label2.Text = Master.eLang.GetString(31, "Video")
        Me.dgvAudio.Columns(0).HeaderText = Master.eLang.GetString(32, "Mediainfo Codec")
        Me.dgvAudio.Columns(1).HeaderText = Master.eLang.GetString(33, "Mapped Codec")
        Me.dgvVideo.Columns(0).HeaderText = Master.eLang.GetString(32, "Mediainfo Codec")
        Me.dgvVideo.Columns(1).HeaderText = Master.eLang.GetString(33, "Mapped Codec")
    End Sub

    Public Sub SaveChanges()
        Dim deleteitem As New List(Of String)
        For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("AudioFormatConvert:"))
            deleteitem.Add(sett.Name)
        Next
        For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("VideoFormatConvert:"))
            deleteitem.Add(sett.Name)
        Next
        For Each s As String In deleteitem
            AdvancedSettings.CleanSetting(s, "*EmberAPP")
        Next
        For Each r As DataGridViewRow In dgvAudio.Rows
            AdvancedSettings.SetSetting(String.Concat("AudioFormatConvert:", r.Cells(0).Value.ToString), r.Cells(1).Value.ToString, "*EmberAPP")
        Next
        For Each r As DataGridViewRow In dgvVideo.Rows
            AdvancedSettings.SetSetting(String.Concat("VideoFormatConvert:", r.Cells(0).Value.ToString), r.Cells(1).Value.ToString, "*EmberAPP")
        Next
    End Sub
End Class
