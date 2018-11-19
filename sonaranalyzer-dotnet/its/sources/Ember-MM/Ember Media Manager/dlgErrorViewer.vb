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
Imports System.Text
Imports System.Net
Imports EmberAPI

Public Class dlgErrorViewer

#Region "Fields"

    Private sBuilder As StringBuilder

#End Region 'Fields

#Region "Methods"

    Public Overloads Sub Show(ByVal owner As System.Windows.Forms.Form)
        If Me.Visible Then
            Me.BuildErrorLog()
            Me.Activate()
        Else
            MyBase.Owner = owner
            MyBase.Show(owner)
        End If
    End Sub

    Public Sub UpdateLog()
        Me.BuildErrorLog()
    End Sub

    Private Sub btnCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCopy.Click
        Select Case Me.btnCopy.Tag.ToString
            Case "p"
                Dim bReturn As String = String.Empty
                Using wc As New WebClient
                    System.Net.ServicePointManager.Expect100Continue = False
                    Dim nvColl As New Specialized.NameValueCollection
                    nvColl.Add("paste_code", sBuilder.ToString)
                    nvColl.Add("paste_subdomain", "embermm")
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
                    bReturn = System.Text.Encoding.ASCII.GetString(wc.UploadValues("http://pastebin.com/api_public.php", "POST", nvColl))
                    nvColl = Nothing
                End Using

                If Not String.IsNullOrEmpty(bReturn) OrElse Not bReturn.ToLower.Contains("error") Then
                    Me.txtPastebinURL.Text = bReturn
                Else
                    Me.txtPastebinURL.Text = Master.eLang.GetString(807, "An error occurred when attempting to send data to Pastebin.com")
                End If
            Case Else
                Clipboard.SetText(Me.sBuilder.ToString)
        End Select
    End Sub

    Private Sub BuildErrorLog()
        Me.btnCopy.Enabled = False

        Me.sBuilder = New StringBuilder

        Dim sPath As String = String.Concat(Functions.AppPath, Path.DirectorySeparatorChar, "Log", Path.DirectorySeparatorChar, "errlog.txt")

        Me.sBuilder.AppendLine("================= <Assembly Versions> =================")
        Me.sBuilder.AppendLine(String.Empty)
        Me.sBuilder.AppendLine("Platform: x86")
        For Each v As ModulesManager.VersionItem In ModulesManager.VersionList
            Me.sBuilder.AppendLine(String.Format("{0} (Revision: {1})", v.Name, v.Version))
        Next
        Me.sBuilder.AppendLine(String.Empty)
        Me.sBuilder.AppendLine("================= <Assembly Versions> =================")

        Me.sBuilder.AppendLine(String.Empty)
        Me.sBuilder.AppendLine(String.Empty)

        If File.Exists(sPath) Then
            Using fs As FileStream = New FileStream(sPath, FileMode.Open, FileAccess.Read)
                Using sr As New StreamReader(fs)
                    While Not sr.EndOfStream
                        Me.sBuilder.AppendLine(sr.ReadLine)
                    End While
                End Using
            End Using
        End If

        Me.txtError.Text = Me.sBuilder.ToString

        If Me.txtError.Lines.Count > 50 Then
            Me.btnCopy.Text = Master.eLang.GetString(805, "Send to PasteBin.com")
            Me.btnCopy.Tag = "p"
            Me.btnCopy.Visible = True
            Me.txtPastebinURL.Visible = True
            Me.lblPastebinURL.Visible = True
        Else
            Me.btnCopy.Text = Master.eLang.GetString(806, "Copy to Clipboard")
            Me.btnCopy.Tag = "c"
            Me.btnCopy.Visible = True
            Me.txtPastebinURL.Visible = False
            Me.lblPastebinURL.Visible = False
        End If

        Me.btnCopy.Enabled = True
    End Sub

    Private Sub dlgErrorViewer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
        Me.BuildErrorLog()
    End Sub

    Private Sub llblURL_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llblURL.LinkClicked
        If Master.isWindows Then
            Process.Start("https://sourceforge.net/apps/trac/emm-r/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "https://sourceforge.net/apps/trac/emm-r/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.Close()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(808, "Error Log Viewer")
        Me.lblInfo.Text = Master.eLang.GetString(809, "Before submitting bug reports, please verify that the bug has not already been reported. You can view a listing of all known bugs here:")
        Me.llblURL.Text = Master.eLang.GetString(810, "https://sourceforge.net/apps/trac/emm-r/")
        Me.lblPastebinURL.Text = Master.eLang.GetString(811, "PasteBin URL:")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
    End Sub

#End Region 'Methods

End Class