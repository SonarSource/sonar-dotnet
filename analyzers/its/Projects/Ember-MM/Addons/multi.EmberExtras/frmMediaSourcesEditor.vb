Imports System.Windows.Forms
Imports EmberAPI

Public Class frmMediaSources
    Public Event ModuleSettingsChanged()


    Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        Try
            ' Add any initialization after the InitializeComponent() call.
            LoadSources()

            dgvByFile.Rows.Clear()
            For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("MediaSourcesByExtension:"))
                Dim i As Integer = dgvByFile.Rows.Add(New Object() {sett.Name.Substring(24), sett.Value})
            Next
            SetByFileStatus(False)
            chkMapByFile.Checked = AdvancedSettings.GetBooleanSetting("MediaSourcesByExtension", False, "*EmberAPP")
        Catch ex As Exception
        End Try
        SetUp()
    End Sub

    Private Sub LoadSources()
        dgvSources.Rows.Clear()
        Dim sources As Hashtable = AdvancedSettings.GetComplexSetting("MovieSources", "*EmberAPP")
        For Each sett As Object In sources.Keys
            Dim i As Integer = dgvSources.Rows.Add(New Object() {sett.ToString, sources.Item(sett.ToString)})
        Next
        dgvSources.ClearSelection()
    End Sub


    Private Sub btnAddSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddSource.Click
        Dim i As Integer = dgvSources.Rows.Add(New Object() {String.Empty, String.Empty})
        dgvSources.Rows(i).Tag = False
        dgvSources.CurrentCell = dgvSources.Rows(i).Cells(0)
        dgvSources.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub
    Private Sub btnRemoveSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveSource.Click
        If dgvSources.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvSources.Rows(dgvSources.SelectedCells(0).RowIndex).Tag) Then
            dgvSources.Rows.RemoveAt(dgvSources.SelectedCells(0).RowIndex)
            RaiseEvent ModuleSettingsChanged()
        End If
    End Sub
    Private Sub dgvSources_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.CurrentCellDirtyStateChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub


    Private Sub dgvSources_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.SelectionChanged
        If dgvSources.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvSources.Rows(dgvSources.SelectedCells(0).RowIndex).Tag) Then
            btnRemoveSource.Enabled = True
        Else
            btnRemoveSource.Enabled = False
        End If
    End Sub

    Private Sub dgvSources_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvSources.KeyDown
        e.Handled = (e.KeyCode = Keys.Enter)
    End Sub

    Private Sub dgvVideo_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        e.Handled = (e.KeyCode = Keys.Enter)
    End Sub

    Sub SetUp()
        btnAddSource.Text = Master.eLang.GetString(28, "Add", True)
        btnRemoveSource.Text = Master.eLang.GetString(30, "Remove", True)
        btnAddByFile.Text = Master.eLang.GetString(28, "Add", True)
        btnRemoveByFile.Text = Master.eLang.GetString(30, "Remove", True)
        btnSetDefaults.Text = Master.eLang.GetString(67, "Set Defaults")
        Label1.Text = Master.eLang.GetString(62, "Sources")
        Me.dgvSources.Columns(0).HeaderText = Master.eLang.GetString(63, "Search String")
        Me.dgvSources.Columns(1).HeaderText = Master.eLang.GetString(64, "Source Name")
        Me.chkMapByFile.Text = Master.eLang.GetString(65, "Map Media Source by File Extension")
        Me.dgvByFile.Columns(0).HeaderText = Master.eLang.GetString(66, "File Extension")
        Me.dgvByFile.Columns(1).HeaderText = Master.eLang.GetString(64, "Source Name")

    End Sub

    Public Sub SaveChanges()
        Dim deleteitem As New List(Of String)
        For Each sett As AdvancedSettings.SettingItem In AdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("MediaSourcesByExtension:"))
            deleteitem.Add(sett.Name)
        Next
        For Each s As String In deleteitem
            AdvancedSettings.CleanSetting(s, "*EmberAPP")
        Next

        Dim sources As New Hashtable
        For Each r As DataGridViewRow In dgvSources.Rows
            If Not String.IsNullOrEmpty(r.Cells(0).Value.ToString) AndAlso Not sources.ContainsKey(r.Cells(0).Value.ToString) Then
                sources.Add(r.Cells(0).Value.ToString, r.Cells(1).Value.ToString)
            End If
        Next
        AdvancedSettings.SetComplexSetting("MovieSources", sources, "*EmberAPP")
        AdvancedSettings.SetBooleanSetting("MediaSourcesByExtension", chkMapByFile.Checked, "*EmberAPP")
        For Each r As DataGridViewRow In dgvByFile.Rows
            If Not String.IsNullOrEmpty(r.Cells(0).Value.ToString) AndAlso Not sources.ContainsKey(r.Cells(0).Value.ToString) Then
                AdvancedSettings.SetSetting(String.Concat("MediaSourcesByExtension:", r.Cells(0).Value.ToString), r.Cells(1).Value.ToString, "*EmberAPP")
            End If
        Next
    End Sub

    Private Sub btnSetDefaults_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetDefaults.Click
        AdvancedSettings.SetDefaults(True, "MovieSources")
        LoadSources()
    End Sub

    Private Sub chkMapByFile_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMapByFile.CheckedChanged
        SetByFileStatus(chkMapByFile.Checked)
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub SetByFileStatus(ByVal b As Boolean)
        dgvByFile.ClearSelection()
        dgvByFile.Enabled = b
        btnAddByFile.Enabled = b
        btnRemoveByFile.Enabled = b
    End Sub

    Private Sub btnAddByFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddByFile.Click
        Dim i As Integer = dgvByFile.Rows.Add(New Object() {String.Empty, String.Empty})
        dgvByFile.Rows(i).Tag = False
        dgvByFile.CurrentCell = dgvByFile.Rows(i).Cells(0)
        dgvByFile.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub btnremoveByFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveByFile.Click
        If dgvByFile.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvByFile.Rows(dgvByFile.SelectedCells(0).RowIndex).Tag) Then
            dgvByFile.Rows.RemoveAt(dgvByFile.SelectedCells(0).RowIndex)
            RaiseEvent ModuleSettingsChanged()
        End If
    End Sub

    Private Sub dgvByFile_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvByFile.CurrentCellDirtyStateChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub


    Private Sub dgvByFile_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvByFile.SelectionChanged
        If dgvByFile.Enabled AndAlso dgvByFile.SelectedCells.Count > 0 AndAlso Not Convert.ToBoolean(dgvByFile.Rows(dgvByFile.SelectedCells(0).RowIndex).Tag) Then
            btnRemoveByFile.Enabled = True
        Else
            btnRemoveByFile.Enabled = False
        End If
    End Sub
End Class
