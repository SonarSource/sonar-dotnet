Imports EmberAPI

' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Public Class frmSettingsHolder

#Region "Fields"

    Dim isSelected As Boolean = False

#End Region 'Fields

#Region "Events"

    Public Event ModuleEnabledChanged(ByVal State As Boolean)

    Public Event ModuleSettingsChanged()

#End Region 'Events

#Region "Methods"

    Private Sub btnEditSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditSet.Click
        ListView1.SelectedItems(0).SubItems(0).Text = TextBox1.Text
        ListView1.SelectedItems(0).SubItems(1).Text = TextBox2.Text
        TextBox1.Text = ""
        TextBox2.Text = ""
        isSelected = False
        CheckButtons()
    End Sub

    Private Sub btnNewSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewSet.Click
        Dim li As New ListViewItem
        li.Text = TextBox1.Text
        li.SubItems.Add(TextBox2.Text)
        ListView1.Items.Add(li)
        TextBox1.Text = ""
        TextBox2.Text = ""
        CheckButtons()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub btnRemoveSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveSet.Click
        ListView1.Items.RemoveAt(ListView1.SelectedItems(0).Index)
        TextBox1.Text = ""
        TextBox2.Text = ""
        isSelected = False
        CheckButtons()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim fl As New FolderBrowserDialog
        fl.ShowDialog()
        TextBox2.Text = fl.SelectedPath
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEnabled.CheckedChanged
        RaiseEvent ModuleEnabledChanged(cbEnabled.Checked)
    End Sub

    Sub CheckButtons()
        If Not String.IsNullOrEmpty(TextBox1.Text) AndAlso Not String.IsNullOrEmpty(TextBox2.Text) AndAlso Not isSelected Then
            btnNewSet.Enabled = True
        Else
            btnNewSet.Enabled = False
        End If
    End Sub

    Private Sub ListView1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListView1.SelectedIndexChanged
        If ListView1.SelectedItems.Count > 0 Then
            isSelected = True
            btnRemoveSet.Enabled = True
            btnEditSet.Enabled = True
            TextBox1.Text = ListView1.SelectedItems(0).SubItems(0).Text
            TextBox2.Text = ListView1.SelectedItems(0).SubItems(1).Text
        Else
            isSelected = False
            btnRemoveSet.Enabled = False
            btnEditSet.Enabled = False
        End If
        CheckButtons()
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        CheckButtons()
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged
        CheckButtons()
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Private Sub SetUp()
        Me.cbEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
        Me.Label3.Text = Master.eLang.GetString(232, "Name", True)
        Me.Label4.Text = Master.eLang.GetString(410, "Path", True)
        Me.ColumnHeader1.Text = Master.eLang.GetString(232, "Name", True)
        Me.ColumnHeader2.Text = Master.eLang.GetString(410, "Path", True)
    End Sub

#End Region 'Methods

    Private Sub pnlSettings_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlSettings.Paint

    End Sub
End Class