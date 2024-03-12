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
Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class dlgSortFiles

#Region "Fields"

    Private fSorter As New FileUtils.FileSorter
    Private _hitgo As Boolean = False

#End Region 'Fields

#Region "Methods"

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        With Me.fbdBrowse
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                If Not String.IsNullOrEmpty(.SelectedPath) Then
                    Me.txtPath.Text = .SelectedPath
                End If
            End If
        End With
    End Sub

    Private Sub btnGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGo.Click
        '//
        ' Convert a file source into a folder source by separating everything into separate folders
        '\\

        If Not String.IsNullOrEmpty(Me.txtPath.Text) AndAlso Directory.Exists(Me.txtPath.Text) Then
            If MsgBox(String.Concat(Master.eLang.GetString(220, "WARNING: If you continue, all files will be sorted into separate folders."), vbNewLine, vbNewLine, Master.eLang.GetString(101, "Are you sure you want to continue?")), MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are you sure?")) = MsgBoxResult.Yes Then
                Me._hitgo = True
                fSorter.SortFiles(Me.txtPath.Text)
                lblStatus.Text = "Done!"
                pbStatus.Value = 0

                Master.eSettings.SortPath = Me.txtPath.Text
            End If
        Else
            MsgBox(Master.eLang.GetString(221, "The folder you entered does not exist. Please enter a valid path."), MsgBoxStyle.Exclamation, Master.eLang.GetString(222, "Directory Not Found"))
            Me.txtPath.Focus()
        End If
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = If(Me._hitgo, System.Windows.Forms.DialogResult.OK, System.Windows.Forms.DialogResult.Cancel)
        Me.Close()
    End Sub

    Private Sub dlgSortFiles_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        AddHandler fSorter.ProgressUpdated, AddressOf UpdateProgress
        Me.SetUp()
        Me.txtPath.Text = Master.eSettings.SortPath
    End Sub

    Private Sub dlgSortFiles_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(213, "Sort Files Into Folders")
        Me.Cancel_Button.Text = Master.eLang.GetString(19, "Close")
        Me.btnGo.Text = Master.eLang.GetString(214, "Go")
        Me.GroupBox1.Text = Master.eLang.GetString(215, "Status")
        Me.lblStatus.Text = Master.eLang.GetString(216, "Enter Path and Press ""Go"" to Begin.")
        Me.Label1.Text = Master.eLang.GetString(217, "Path to Sort:")
        Me.fbdBrowse.Description = Master.eLang.GetString(218, "Select the folder which contains the files you wish to sort.")
    End Sub

    Private Sub UpdateProgress(ByVal iPercent As Integer, ByVal sStatus As String)
        If String.IsNullOrEmpty(sStatus) Then
            pbStatus.Maximum = iPercent
        Else
            lblStatus.Text = sStatus
            pbStatus.Value = iPercent
        End If
    End Sub

#End Region 'Methods

End Class