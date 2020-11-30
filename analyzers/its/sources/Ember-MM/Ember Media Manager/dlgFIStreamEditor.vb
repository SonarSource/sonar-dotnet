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

Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class dlgFIStreamEditor

#Region "Fields"

    Private stream_a As New MediaInfo.Audio
    Private stream_s As New MediaInfo.Subtitle
    Private stream_v As New MediaInfo.Video

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal stream_type As String, ByVal movie As MediaInfo.Fileinfo, ByVal idx As Integer) As Object
        Try

            GroupBox1.Visible = False
            GroupBox2.Visible = False
            GroupBox2.Visible = False

            If stream_type = Master.eLang.GetString(595, "Video Stream") Then
                GroupBox1.Visible = True
                cbVideoCodec.Items.AddRange((From vCo In APIXML.lFlags Where vCo.Type = APIXML.FlagType.VideoCodec AndAlso Not vCo.Name = "defaultscreen" Select vCo.Name).ToArray)
                Dim xShortLang = Localization.ISOLangGetLanguagesList.ToArray
                cbVideoLanguage.Items.AddRange(xShortLang.ToArray)
                If Not movie Is Nothing Then
                    cbVideoCodec.Text = movie.StreamDetails.Video(idx).Codec
                    txtARatio.Text = movie.StreamDetails.Video(idx).Aspect
                    txtWidth.Text = movie.StreamDetails.Video(idx).Width
                    txtHeight.Text = movie.StreamDetails.Video(idx).Height
                    If movie.StreamDetails.Video(idx).Scantype = Master.eLang.GetString(616, "Progressive") Then
                        rbProgressive.Checked = True
                    Else
                        rbInterlaced.Checked = True
                    End If
                    txtDuration.Text = movie.StreamDetails.Video(idx).Duration
                    cbVideoLanguage.Text = movie.StreamDetails.Video(idx).LongLanguage
                End If
            End If
            If stream_type = Master.eLang.GetString(596, "Audio Stream") Then
                GroupBox2.Visible = True
                cbAudioCodec.Items.AddRange((From aCo In APIXML.lFlags Where aCo.Type = APIXML.FlagType.AudioCodec AndAlso Not aCo.Name = "defaultaudio" Select aCo.Name).ToArray)
                Dim xShortLang = Localization.ISOLangGetLanguagesList.ToArray
                cbAudioLanguage.Items.AddRange(xShortLang.ToArray)
                cbAudioChannels.Items.AddRange(New String() {"8", "7", "6", "2", "1"})
                If Not movie Is Nothing Then
                    cbAudioCodec.Text = movie.StreamDetails.Audio(idx).Codec
                    cbAudioLanguage.Text = movie.StreamDetails.Audio(idx).LongLanguage
                    cbAudioChannels.Text = movie.StreamDetails.Audio(idx).Channels
                End If
            End If
            If stream_type = Master.eLang.GetString(597, "Subtitle Stream") Then
                GroupBox3.Visible = True
                Dim xShortLang = Localization.ISOLangGetLanguagesList.ToArray
                cbSubsLanguage.Items.AddRange(xShortLang.ToArray)
                If Not movie Is Nothing Then
                    cbSubsLanguage.Text = movie.StreamDetails.Subtitle(idx).LongLanguage
                    If movie.StreamDetails.Subtitle(idx).SubsType = "Embedded" Then
                        rbEmbedded.Checked = True
                    Else
                        rbExternal.Checked = True
                    End If
                End If

            End If

            If MyBase.ShowDialog() = DialogResult.OK Then
                If stream_type = Master.eLang.GetString(595, "Video Stream") Then
                    stream_v.Codec = If(cbVideoCodec.SelectedItem Is Nothing, "", cbVideoCodec.SelectedItem.ToString)
                    stream_v.Aspect = txtARatio.Text
                    stream_v.Width = txtWidth.Text
                    stream_v.Height = txtHeight.Text
                    stream_v.Scantype = If(rbProgressive.Checked, Master.eLang.GetString(616, "Progressive"), Master.eLang.GetString(615, "Interlaced"))
                    stream_v.Duration = txtDuration.Text
                    If Not cbVideoLanguage.SelectedItem Is Nothing Then stream_v.LongLanguage = cbVideoLanguage.SelectedItem.ToString
                    If Not cbVideoLanguage.SelectedItem Is Nothing Then stream_v.Language = Localization.ISOLangGetCode3ByLang(cbVideoLanguage.SelectedItem.ToString)
                    Return stream_v
                End If
                If stream_type = Master.eLang.GetString(596, "Audio Stream") Then
                    stream_a.Codec = If(cbAudioCodec.SelectedItem Is Nothing, "", cbAudioCodec.SelectedItem.ToString)
                    If Not cbAudioLanguage.SelectedItem Is Nothing Then stream_a.LongLanguage = cbAudioLanguage.SelectedItem.ToString
                    If Not cbAudioLanguage.SelectedItem Is Nothing Then stream_a.Language = Localization.ISOLangGetCode3ByLang(cbAudioLanguage.SelectedItem.ToString)
                    If Not cbAudioChannels.SelectedItem Is Nothing Then stream_a.Channels = cbAudioChannels.SelectedItem.ToString
                    Return stream_a
                End If
                If stream_type = Master.eLang.GetString(597, "Subtitle Stream") Then
                    If Not cbSubsLanguage.SelectedItem Is Nothing Then stream_s.LongLanguage = If(cbSubsLanguage.SelectedItem Is Nothing, "", cbSubsLanguage.SelectedItem.ToString)
                    If Not cbSubsLanguage.SelectedItem Is Nothing Then stream_s.Language = Localization.ISOLangGetCode3ByLang(cbSubsLanguage.SelectedItem.ToString)
                    If Not cbSubsLanguage.SelectedItem Is Nothing Then
                        stream_s.SubsType = If(rbEmbedded.Checked, "Embedded", "External")
                    End If
                    Return stream_s
                End If
                Return Nothing
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub cbAudioCodec_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbAudioCodec.SelectedIndexChanged
        'If cbAudioCodec.SelectedIndex >= 0 Then
        'Dim xAChanFlag = From xAChan In XML.FlagsXML...<achan>...<name> Where Regex.IsMatch(cbAudioCodec.SelectedItem, Regex.Match(xAChan.@searchstring, "\{atype=([^\}]+)\}").Groups(1).Value.ToString) Select xAChan.@searchstring
        'End If
    End Sub

    Private Sub dlgFIStreamEditor_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(613, "Stream Editor")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.GroupBox1.Text = Master.eLang.GetString(595, "Video Streams")
        Me.Label5.Text = Master.eLang.GetString(614, "Aspect Ratio")
        Me.rbInterlaced.Text = Master.eLang.GetString(615, "Interlaced")
        Me.rbProgressive.Text = Master.eLang.GetString(616, "Progressive")
        Me.Label4.Text = Master.eLang.GetString(604, "Codec")
        Me.Label3.Text = Master.eLang.GetString(609, "Duration")
        Me.Label2.Text = Master.eLang.GetString(607, "Height")
        Me.Label1.Text = Master.eLang.GetString(606, "Width")
        Me.GroupBox2.Text = Master.eLang.GetString(596, "Audio Streams")
        Me.Label9.Text = Master.eLang.GetString(611, "Channels")
        Me.Label7.Text = Me.Label4.Text
        Me.Label6.Text = Master.eLang.GetString(610, "Language")
        Me.GroupBox3.Text = Master.eLang.GetString(597, "Subtitle  Streams")
        Me.Label10.Text = Me.Label6.Text
        Me.Label8.Text = Me.Label6.Text
    End Sub

#End Region 'Methods

End Class