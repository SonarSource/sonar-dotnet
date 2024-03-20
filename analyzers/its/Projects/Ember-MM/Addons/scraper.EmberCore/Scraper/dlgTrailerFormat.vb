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

Public Class dlgTrailerFormat

#Region "Fields"

    Private WithEvents YouTube As YouTube.Scraper
    Private _selectedformaturl As String
    Private _yturl As String

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal YTURL As String) As String
        Me._yturl = YTURL

        If MyBase.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Return _selectedformaturl
        Else
            Return String.Empty
        End If
    End Function

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgTrailerFormat_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.SetUp()

            lstFormats.DataSource = Nothing

            YouTube = New YouTube.Scraper
            YouTube.GetVideoLinksAsync(Me._yturl)

        Catch ex As Exception
            MsgBox(Master.eLang.GetString(71, "The video format links could not be retrieved."), MsgBoxStyle.Critical, Master.eLang.GetString(72, "Error Retrieving Video Format Links"))
        End Try
    End Sub

    Private Sub lstFormats_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstFormats.SelectedIndexChanged
        Try
            Me._selectedformaturl = DirectCast(lstFormats.SelectedItem, YouTube.VideoLinkItem).URL

            If Me.lstFormats.SelectedItems.Count > 0 Then
                Me.OK_Button.Enabled = True
            Else
                Me.OK_Button.Enabled = False
            End If
        Catch
        End Try
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(73, "Select Format")
        Me.lblStatus.Text = Master.eLang.GetString(74, "Getting available formats...")
        Me.GroupBox1.Text = Master.eLang.GetString(75, "Available Formats")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
    End Sub

    Private Sub YouTube_VideoLinksRetrieved(ByVal bSuccess As Boolean) Handles YouTube.VideoLinksRetrieved
        Try

            If bSuccess Then
                lstFormats.DataSource = YouTube.VideoLinks.Values.ToList
                lstFormats.DisplayMember = "Description"
                lstFormats.ValueMember = "URL"

                If YouTube.VideoLinks.ContainsKey(Master.eSettings.PreferredTrailerQuality) Then
                    Me.lstFormats.SelectedIndex = YouTube.VideoLinks.IndexOfKey(Master.eSettings.PreferredTrailerQuality)
                ElseIf Me.lstFormats.Items.Count = 1 Then
                    Me.lstFormats.SelectedIndex = 0
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        Finally
            Me.pnlStatus.Visible = False
            Me.lstFormats.Enabled = True
        End Try
    End Sub

#End Region 'Methods

End Class