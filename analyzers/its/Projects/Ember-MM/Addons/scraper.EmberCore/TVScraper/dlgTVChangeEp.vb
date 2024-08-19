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

Public Class dlgTVChangeEp

#Region "Fields"

    Private _episode As MediaContainers.EpisodeDetails = Nothing
    Private _tepisodes As New List(Of MediaContainers.EpisodeDetails)

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal tEpisodes As List(Of MediaContainers.EpisodeDetails)) As MediaContainers.EpisodeDetails
        Me._tepisodes = tEpisodes

        If MyBase.ShowDialog = Windows.Forms.DialogResult.OK Then
            Return _episode
        Else
            Return Nothing
        End If
    End Function

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ClearInfo()
        Me.pbPreview.Image = Nothing
        Me.lblTitle.Text = String.Empty
        Me.txtPlot.Text = String.Empty
    End Sub

    Private Sub dlgTVChangeEp_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()

        Dim lGroup As New ListViewGroup
        Dim lItem As New ListViewItem
        Dim tSeason As Integer = Nothing

        For Each Season As Integer In _tepisodes.GroupBy(Function(s) s.Season).Select(Function(group) group.Key)
            tSeason = Season
            lGroup = New ListViewGroup
            lGroup.Header = String.Format(Master.eLang.GetString(726, "Season {0}", True), tSeason)
            lvEpisodes.Groups.Add(lGroup)
            For Each Episode As MediaContainers.EpisodeDetails In _tepisodes.Where(Function(s) s.Season = tSeason).OrderBy(Function(s) s.Episode)
                lItem = lvEpisodes.Items.Add(Episode.Episode.ToString)
                lItem.Tag = Episode
                lItem.SubItems.Add(Episode.Title)
                lGroup.Items.Add(lItem)
            Next
        Next
    End Sub

    Private Sub lvEpisodes_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvEpisodes.SelectedIndexChanged
        Me.ClearInfo()
        If lvEpisodes.SelectedItems.Count > 0 AndAlso Not IsNothing(lvEpisodes.SelectedItems(0).Tag) Then
            Me._episode = DirectCast(lvEpisodes.SelectedItems(0).Tag, MediaContainers.EpisodeDetails)

            If Not IsNothing(Me._episode.Poster.Image) Then
                Me.pbPreview.Image = Me._episode.Poster.Image
            ElseIf Not String.IsNullOrEmpty(Me._episode.LocalFile) AndAlso File.Exists(Me._episode.LocalFile) Then
                Me._episode.Poster.FromFile(Me._episode.LocalFile)
                If Not IsNothing(Me._episode.Poster.Image) Then
                    Me.pbPreview.Image = Me._episode.Poster.Image
                End If
            ElseIf Not String.IsNullOrEmpty(Me._episode.PosterURL) Then
                Me._episode.Poster.FromWeb(Me._episode.PosterURL)
                If Not IsNothing(Me._episode.Poster.Image) Then
                    Directory.CreateDirectory(Directory.GetParent(Me._episode.LocalFile).FullName)
                    Me._episode.Poster.Save(Me._episode.LocalFile)
                    Me.pbPreview.Image = Me._episode.Poster.Image
                End If
            End If

            Me.lblTitle.Text = Me._episode.Title
            Me.txtPlot.Text = Me._episode.Plot
        End If
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(772, "Change Episode", True)

        Me.lvEpisodes.Columns(0).Text = Master.eLang.GetString(727, "Episode", True)
        Me.lvEpisodes.Columns(1).Text = Master.eLang.GetString(21, "Title", True)

        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
    End Sub

#End Region 'Methods

End Class