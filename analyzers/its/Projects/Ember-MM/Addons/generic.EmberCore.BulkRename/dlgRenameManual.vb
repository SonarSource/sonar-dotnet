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

Imports System.IO
Imports EmberAPI

Public Class dlgRenameManual

#Region "Fields"

    Friend WithEvents bwRename As New System.ComponentModel.BackgroundWorker

#End Region 'Fields

#Region "Methods"

    Private Sub bwRename_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwRename.DoWork
        FileFolderRenamer.RenameSingle(Master.currMovie, txtFolder.Text, txtFile.Text, True, True, True)
    End Sub

    Private Sub bwRename_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwRename.RunWorkerCompleted
        Cursor.Current = Cursors.Default
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgRenameManual_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()
        Dim FileName = Path.GetFileNameWithoutExtension(StringUtils.CleanStackingMarkers(Master.currMovie.Filename))
        Dim stackMark As String = Path.GetFileNameWithoutExtension(Master.currMovie.Filename).Replace(FileName, String.Empty).ToLower
        If Not stackMark = String.Empty AndAlso Master.currMovie.Movie.Title.ToLower.EndsWith(stackMark) Then
            FileName = Path.GetFileNameWithoutExtension(Master.currMovie.Filename)
        End If
        If Master.currMovie.isSingle Then
            txtFolder.Text = Directory.GetParent(Master.currMovie.Filename).Name
        Else
            txtFolder.Text = "$D"
            txtFolder.Visible = False
        End If
        txtFile.Text = FileName
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Cursor.Current = Cursors.WaitCursor
        OK_Button.Enabled = False
        Cancel_Button.Enabled = False
        txtFolder.Enabled = False
        txtFile.Enabled = False
        pnlStatus.Visible = True
        Application.DoEvents()
        Me.bwRename = New System.ComponentModel.BackgroundWorker
        Me.bwRename.RunWorkerAsync()
    End Sub

    Sub SetUp()
        Me.Text = String.Concat(Master.eLang.GetString(16, "Manual Rename"), " | ", Master.currMovie.Movie.Title)
        Me.Label1.Text = Master.eLang.GetString(17, "Folder Name")
        Me.Label2.Text = Master.eLang.GetString(18, "File Name")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.lblTitle.Text = Master.eLang.GetString(19, "Title:")
        Me.Label3.Text = Master.eLang.GetString(20, "Renaming Directory/Files...")
        Me.txtTitle.Text = Master.currMovie.Movie.Title
    End Sub

    Private Sub txtFile_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFile.TextChanged
        If Not String.IsNullOrEmpty(txtFolder.Text) AndAlso Not String.IsNullOrEmpty(txtFile.Text) Then
            OK_Button.Enabled = True
        Else
            OK_Button.Enabled = False
        End If
    End Sub

    Private Sub txtFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFolder.TextChanged
        If Not String.IsNullOrEmpty(txtFolder.Text) AndAlso Not String.IsNullOrEmpty(txtFile.Text) Then
            OK_Button.Enabled = True
        Else
            OK_Button.Enabled = False
        End If
    End Sub

#End Region 'Methods

End Class