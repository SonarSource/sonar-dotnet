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

Public Class dlgNewVersion

#Region "Fields"

    Private WithEvents bwDownloadSetup As New System.ComponentModel.BackgroundWorker

#End Region 'Fields

#Region "Methods"

    Private Sub btnYes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnYes.Click
        If Master.isWindows Then
            Process.Start(Path.Combine(Functions.AppPath, "EmberSetup.exe"), "-force")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = String.Concat(Path.Combine(Functions.AppPath, "EmberSetup.exe"), " -force")
                Explorer.Start()
            End Using
        End If
        DialogResult = Windows.Forms.DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpgrade.Click
        btnUpgrade.Enabled = False
        pnlUpgrade.Visible = True
        bwDownloadSetup.RunWorkerAsync()
        While bwDownloadSetup.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While
        If File.Exists(Path.Combine(Functions.AppPath, "EmberSetup.exe")) Then
            lblStart.Visible = False
            pbUpgrade.Visible = False
            lblUpgrade.Visible = True
            btnNo.Visible = True
            btnYes.Visible = True
        Else
            lblStart.Visible = False
            pbUpgrade.Visible = False
            Me.lblUpgrade.Text = Master.eLang.GetString(210, "Failed to Load Upgrade Application")
            lblUpgrade.Visible = True
            'Error
        End If
    End Sub

    Private Sub bwbwDownloadSetup_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadSetup.DoWork
        If File.Exists(Path.Combine(Functions.AppPath, "EmberSetup.exe")) Then
            File.Delete(Path.Combine(Functions.AppPath, "EmberSetup.exe"))
        End If
        Dim lhttp As New HTTP
        lhttp.DownloadFile(String.Format("http://pcjco.dommel.be/emm-r/{0}/EmberSetup.exe", If(Functions.IsBetaEnabled(), "updatesbeta", "updates")), Path.Combine(Functions.AppPath, "EmberSetup.exe"), False, "other")
    End Sub

    Private Sub dlgNewVersion_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.txtChangelog.Text = Functions.GetChangelog.Replace("\n", vbNewLine)

        Me.SetUp()
    End Sub

    Private Sub dlgNewVersion_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub llClick_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llClick.LinkClicked
        'If Master.isWindows Then
        'Process.Start("http://www.embermm.com/tab/show/embermm")
        'Else
        'Using Explorer As New Process
        'Explorer.StartInfo.FileName = "xdg-open"
        'Explorer.StartInfo.Arguments = "http://www.embermm.com/tab/show/embermm"
        'Explorer.Start()
        'End Using
        'End If
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(209, "A New Version Is Available")
        Me.lblNew.Text = Me.Text
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.llClick.Text = Master.eLang.GetString(211, "Click Here")
        Me.Label2.Text = Master.eLang.GetString(212, "to visit embermm.com.")
        Me.lblStart.Text = Master.eLang.GetString(717, "Preparing for upgrade ...")
        Me.lblUpgrade.Text = Master.eLang.GetString(718, "We are now ready to upgrade. Ember will now close so the Upgrade can start.\n\nDo you want to continue?").Replace("\n", vbCrLf)
        Me.btnYes.Text = Master.eLang.GetString(719, "YES")
        Me.btnNo.Text = Master.eLang.GetString(720, "NO")
        Me.btnUpgrade.Text = Master.eLang.GetString(721, "Upgrade")
    End Sub

#End Region 'Methods

End Class