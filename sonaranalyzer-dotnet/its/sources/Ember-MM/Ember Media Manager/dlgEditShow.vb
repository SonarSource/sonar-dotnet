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

Public Class dlgEditShow

#Region "Fields"

    Private ASPoster As New Images With {.IsEdit = True}
    Private Fanart As New Images With {.IsEdit = True}
    Private lvwActorSorter As ListViewColumnSorter
    Private Poster As New Images With {.IsEdit = True}
    Private tmpRating As String

#End Region 'Fields

#Region "Methods"

    Private Sub btnActorDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnActorDown.Click
        If Me.lvActors.SelectedItems.Count > 0 AndAlso Not IsNothing(Me.lvActors.SelectedItems(0)) AndAlso Me.lvActors.SelectedIndices(0) < (Me.lvActors.Items.Count - 1) Then
            Dim iIndex As Integer = Me.lvActors.SelectedIndices(0)
            Me.lvActors.Items.Insert(iIndex + 2, DirectCast(Me.lvActors.SelectedItems(0).Clone, ListViewItem))
            Me.lvActors.Items.RemoveAt(iIndex)
            Me.lvActors.Items(iIndex + 1).Selected = True
            Me.lvActors.Select()
        End If
    End Sub

    Private Sub btnActorUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnActorUp.Click
        Try
            If Me.lvActors.SelectedItems.Count > 0 AndAlso Not IsNothing(Me.lvActors.SelectedItems(0)) AndAlso Me.lvActors.SelectedIndices(0) > 0 Then
                Dim iIndex As Integer = Me.lvActors.SelectedIndices(0)
                Me.lvActors.Items.Insert(iIndex - 1, DirectCast(Me.lvActors.SelectedItems(0).Clone, ListViewItem))
                Me.lvActors.Items.RemoveAt(iIndex + 1)
                Me.lvActors.Items(iIndex - 1).Selected = True
                Me.lvActors.Select()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnAddActor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddActor.Click
        Try
            Dim eActor As New MediaContainers.Person
            Using dAddEditActor As New dlgAddEditActor
                eActor = dAddEditActor.ShowDialog(True)
            End Using
            If Not IsNothing(eActor) Then
                Dim lvItem As ListViewItem = Me.lvActors.Items.Add(eActor.Name)
                lvItem.SubItems.Add(eActor.Role)
                lvItem.SubItems.Add(eActor.Thumb)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnASChangePosterScrape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASChangePosterScrape.Click
        Dim tImage As Image = ModulesManager.Instance.TVSingleImageOnly(Master.currShow.TVShow.Title, Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVShow.ID, Enums.TVImageType.AllSeasonPoster, 0, 0, Master.currShow.ShowLanguage, Master.currShow.Ordering, Me.pbASPoster.Image)

        If Not IsNothing(tImage) Then
            Me.ASPoster.Image = New Bitmap(tImage)
            Me.pbASPoster.Image = tImage
        End If
    End Sub

    Private Sub btnASChangePoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASChangePoster.Click
        Try
            With ofdImage
                .InitialDirectory = Master.currShow.ShowPath
                .Filter = "Supported Images(*.jpg, *.jpeg, *.tbn)|*.jpg;*.jpeg;*.tbn|jpeg (*.jpg, *.jpeg)|*.jpg;*.jpeg|tbn (*.tbn)|*.tbn"
                .FilterIndex = 0
            End With

            If ofdImage.ShowDialog() = DialogResult.OK Then
                ASPoster.FromFile(ofdImage.FileName)
                pbASPoster.Image = ASPoster.Image

                Me.lblASSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbASPoster.Image.Width, Me.pbASPoster.Image.Height)
                Me.lblASSize.Visible = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnASPosterChangeDL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASPosterChangeDL.Click
        Try
            Using dImgManual As New dlgImgManual
                If dImgManual.ShowDialog(Enums.ImageType.ASPoster) = DialogResult.OK Then
                    ASPoster.FromFile(Path.Combine(Master.TempPath, "asposter.jpg"))
                    pbASPoster.Image = ASPoster.Image

                    Me.lblASSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbASPoster.Image.Width, Me.pbASPoster.Image.Height)
                    Me.lblASSize.Visible = True
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnASPosterRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASPosterRemove.Click
        Me.pbASPoster.Image = Nothing
        Me.ASPoster.Image = Nothing
    End Sub

    Private Sub btnEditActor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditActor.Click
        Me.EditActor()
    End Sub

    Private Sub btnManual_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnManual.Click
        Try
            If dlgManualEdit.ShowDialog(Master.currShow.ShowNfoPath) = DialogResult.OK Then
                Master.currShow.TVShow = NFO.LoadTVShowFromNFO(Master.currShow.ShowNfoPath)
                Me.FillInfo()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnRemoveFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveFanart.Click
        Me.pbFanart.Image = Nothing
        Me.Fanart.Image = Nothing
    End Sub

    Private Sub btnRemovePoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemovePoster.Click
        Me.pbPoster.Image = Nothing
        Me.Poster.Image = Nothing
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.DeleteActors()
    End Sub

    Private Sub btnSetFanartDL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetFanartDL.Click
        Try
            Using dImgManual As New dlgImgManual
                If dImgManual.ShowDialog(Enums.ImageType.Fanart) = DialogResult.OK Then
                    Fanart.FromFile(Path.Combine(Master.TempPath, "fanart.jpg"))
                    pbFanart.Image = Fanart.Image

                    Me.lblFanartSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbFanart.Image.Width, Me.pbFanart.Image.Height)
                    Me.lblFanartSize.Visible = True
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnSetFanartScrape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetFanartScrape.Click
        Dim tImage As Image = ModulesManager.Instance.TVSingleImageOnly(Master.currShow.TVShow.Title, Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVShow.ID, Enums.TVImageType.ShowFanart, 0, 0, Master.currShow.ShowLanguage, Master.currShow.Ordering, Me.pbFanart.Image)

        If Not IsNothing(tImage) Then
            Me.Fanart.Image = New Bitmap(tImage)
            Me.pbFanart.Image = tImage

            Me.lblFanartSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbFanart.Image.Width, Me.pbFanart.Image.Height)
            Me.lblFanartSize.Visible = True
        End If
    End Sub

    Private Sub btnSetFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetFanart.Click
        Try
            With ofdImage
                .InitialDirectory = Master.currShow.ShowPath
                .Filter = "JPEGs|*.jpg"
                .FilterIndex = 4
            End With

            If ofdImage.ShowDialog() = DialogResult.OK Then
                Fanart.FromFile(ofdImage.FileName)
                pbFanart.Image = Fanart.Image

                Me.lblFanartSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbFanart.Image.Width, Me.pbFanart.Image.Height)
                Me.lblFanartSize.Visible = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnSetPosterDL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPosterDL.Click
        Try
            Using dImgManual As New dlgImgManual
                If dImgManual.ShowDialog(Enums.ImageType.Posters) = DialogResult.OK Then
                    Poster.FromFile(Path.Combine(Master.TempPath, "poster.jpg"))
                    pbPoster.Image = Poster.Image

                    Me.lblPosterSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbPoster.Image.Width, Me.pbPoster.Image.Height)
                    Me.lblPosterSize.Visible = True
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnSetPosterScrape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPosterScrape.Click
        Dim tImage As Image = ModulesManager.Instance.TVSingleImageOnly(Master.currShow.TVShow.Title, Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVShow.ID, Enums.TVImageType.ShowPoster, 0, 0, Master.currShow.ShowLanguage, Master.currShow.Ordering, Me.pbPoster.Image)

        If Not IsNothing(tImage) Then
            Me.Poster.Image = New Bitmap(tImage)
            Me.pbPoster.Image = tImage

            Me.lblPosterSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbPoster.Image.Width, Me.pbPoster.Image.Height)
            Me.lblPosterSize.Visible = True
        End If
    End Sub

    Private Sub btnSetPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPoster.Click
        Try
            With ofdImage
                .InitialDirectory = Master.currShow.ShowPath
                .Filter = "Supported Images(*.jpg, *.jpeg, *.tbn)|*.jpg;*.jpeg;*.tbn|jpeg (*.jpg, *.jpeg)|*.jpg;*.jpeg|tbn (*.tbn)|*.tbn"
                .FilterIndex = 0
            End With

            If ofdImage.ShowDialog() = DialogResult.OK Then
                Poster.FromFile(ofdImage.FileName)
                pbPoster.Image = Poster.Image

                Me.lblPosterSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), Me.pbPoster.Image.Width, Me.pbPoster.Image.Height)
                Me.lblPosterSize.Visible = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub BuildStars(ByVal sinRating As Single)
        '//
        ' Convert # rating to star images
        '\\

        Try
            'f'in MS and them leaving control arrays out of VB.NET
            With Me
                .pbStar1.Image = Nothing
                .pbStar2.Image = Nothing
                .pbStar3.Image = Nothing
                .pbStar4.Image = Nothing
                .pbStar5.Image = Nothing

                If sinRating >= 0.5 Then ' if rating is less than .5 out of ten, consider it a 0
                    Select Case (sinRating / 2)
                        Case Is <= 0.5
                            .pbStar1.Image = My.Resources.starhalf
                        Case Is <= 1
                            .pbStar1.Image = My.Resources.star
                        Case Is <= 1.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.starhalf
                        Case Is <= 2
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                        Case Is <= 2.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.starhalf
                        Case Is <= 3
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                        Case Is <= 3.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.starhalf
                        Case Is <= 4
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                        Case Is <= 4.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                            .pbStar5.Image = My.Resources.starhalf
                        Case Else
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                            .pbStar5.Image = My.Resources.star
                    End Select
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub CleanUp()
        Try
            If File.Exists(Path.Combine(Master.TempPath, "poster.jpg")) Then
                File.Delete(Path.Combine(Master.TempPath, "poster.jpg"))
            End If

            If File.Exists(Path.Combine(Master.TempPath, "fanart.jpg")) Then
                File.Delete(Path.Combine(Master.TempPath, "fanart.jpg"))
            End If

            If File.Exists(Path.Combine(Master.TempPath, "asposter.jpg")) Then
                File.Delete(Path.Combine(Master.TempPath, "asposter.jpg"))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub DeleteActors()
        Try
            If Me.lvActors.Items.Count > 0 Then
                While Me.lvActors.SelectedItems.Count > 0
                    Me.lvActors.Items.Remove(Me.lvActors.SelectedItems(0))
                End While
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dlgEditShow_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Master.eSettings.AllSeasonPosterEnabled Then Me.TabControl1.TabPages.Remove(TabPage4)

        Me.SetUp()

        Me.lvwActorSorter = New ListViewColumnSorter()
        Me.lvActors.ListViewItemSorter = Me.lvwActorSorter

        Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
        Using g As Graphics = Graphics.FromImage(iBackground)
            g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
            Me.pnlTop.BackgroundImage = iBackground
        End Using

        Me.LoadGenres()
        Me.LoadRatings()

        Me.FillInfo()
    End Sub

    Private Sub EditActor()
        Try
            If Me.lvActors.SelectedItems.Count > 0 Then
                Dim lvwItem As ListViewItem = Me.lvActors.SelectedItems(0)
                Dim eActor As New MediaContainers.Person With {.Name = lvwItem.Text, .Role = lvwItem.SubItems(1).Text, .Thumb = lvwItem.SubItems(2).Text}
                Using dAddEditActor As New dlgAddEditActor
                    eActor = dAddEditActor.ShowDialog(False, eActor)
                End Using
                If Not IsNothing(eActor) Then
                    lvwItem.Text = eActor.Name
                    lvwItem.SubItems(1).Text = eActor.Role
                    lvwItem.SubItems(2).Text = eActor.Thumb
                    lvwItem.Selected = True
                    lvwItem.EnsureVisible()
                End If
                eActor = Nothing
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub FillInfo()
        With Me
            .cbOrdering.SelectedIndex = Master.currShow.Ordering

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Title) Then .txtTitle.Text = Master.currShow.TVShow.Title
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Plot) Then .txtPlot.Text = Master.currShow.TVShow.Plot
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Premiered) Then .txtPremiered.Text = Master.currShow.TVShow.Premiered
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Studio) Then .txtStudio.Text = Master.currShow.TVShow.Studio

            For i As Integer = 0 To .lbGenre.Items.Count - 1
                .lbGenre.SetItemChecked(i, False)
            Next
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Genre) Then
                Dim genreArray() As String
                genreArray = Strings.Split(Master.currShow.TVShow.Genre, " / ")
                For g As Integer = 0 To UBound(genreArray)
                    If .lbGenre.FindString(genreArray(g).Trim) > 0 Then
                        .lbGenre.SetItemChecked(.lbGenre.FindString(genreArray(g).Trim), True)
                    End If
                Next

                If .lbGenre.CheckedItems.Count = 0 Then
                    .lbGenre.SetItemChecked(0, True)
                End If
            Else
                .lbGenre.SetItemChecked(0, True)
            End If

            Dim lvItem As ListViewItem
            .lvActors.Items.Clear()
            For Each imdbAct As MediaContainers.Person In Master.currShow.TVShow.Actors
                lvItem = .lvActors.Items.Add(imdbAct.Name)
                lvItem.SubItems.Add(imdbAct.Role)
                lvItem.SubItems.Add(imdbAct.Thumb)
            Next

            Dim tRating As Single = NumUtils.ConvertToSingle(Master.currShow.TVShow.Rating)
            .tmpRating = tRating.ToString
            .pbStar1.Tag = tRating
            .pbStar2.Tag = tRating
            .pbStar3.Tag = tRating
            .pbStar4.Tag = tRating
            .pbStar5.Tag = tRating
            If tRating > 0 Then .BuildStars(tRating)

            Me.SelectMPAA()

            Fanart.FromFile(Master.currShow.ShowFanartPath)
            If Not IsNothing(Fanart.Image) Then
                .pbFanart.Image = Fanart.Image

                .lblFanartSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), .pbFanart.Image.Width, .pbFanart.Image.Height)
                .lblFanartSize.Visible = True
            End If

            Poster.FromFile(Master.currShow.ShowPosterPath)
            If Not IsNothing(Poster.Image) Then
                .pbPoster.Image = Poster.Image

                .lblPosterSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), .pbPoster.Image.Width, .pbPoster.Image.Height)
                .lblPosterSize.Visible = True
            End If

            If Master.eSettings.AllSeasonPosterEnabled Then
                .ASPoster.FromFile(Master.currShow.SeasonPosterPath)
                If Not IsNothing(.ASPoster.Image) Then
                    .pbASPoster.Image = .ASPoster.Image

                    .lblASSize.Text = String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), .pbASPoster.Image.Width, .pbASPoster.Image.Height)
                    .lblASSize.Visible = True
                End If
            End If
        End With
    End Sub

    Private Sub LoadGenres()
        Me.lbGenre.Items.Add(Master.eLang.None)

        Me.lbGenre.Items.AddRange(APIXML.GetGenreList)
    End Sub

    Private Sub LoadRatings()
        Me.lbMPAA.Items.Add(Master.eLang.None)

        Me.lbMPAA.Items.AddRange(APIXML.GetTVRatingList)
    End Sub

    Private Sub lvActors_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lvActors.ColumnClick
        ' Determine if the clicked column is already the column that is
        ' being sorted.
        Try
            If (e.Column = Me.lvwActorSorter.SortColumn) Then
                ' Reverse the current sort direction for this column.
                If (Me.lvwActorSorter.Order = SortOrder.Ascending) Then
                    Me.lvwActorSorter.Order = SortOrder.Descending
                Else
                    Me.lvwActorSorter.Order = SortOrder.Ascending
                End If
            Else
                ' Set the column number that is to be sorted; default to ascending.
                Me.lvwActorSorter.SortColumn = e.Column
                Me.lvwActorSorter.Order = SortOrder.Ascending
            End If

            ' Perform the sort with these new sort options.
            Me.lvActors.Sort()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub lvActors_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvActors.DoubleClick
        EditActor()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Try
            Me.SetInfo()

            Master.DB.SaveTVShowToDB(Master.currShow, False, False, True)

            If Master.eSettings.AllSeasonPosterEnabled Then Master.DB.SaveTVSeasonToDB(Master.currShow, False)

            Me.CleanUp()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub pbStar1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbStar1.Click
        Me.tmpRating = Me.pbStar1.Tag.ToString
    End Sub

    Private Sub pbStar1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStar1.MouseLeave
        Try
            Dim tmpDBL As Single = 0
            Single.TryParse(Me.tmpRating, tmpDBL)
            Me.BuildStars(tmpDBL)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbStar1.MouseMove
        Try
            If e.X < 12 Then
                Me.pbStar1.Tag = 1
                Me.BuildStars(1)
            Else
                Me.pbStar1.Tag = 2
                Me.BuildStars(2)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbStar2.Click
        Me.tmpRating = Me.pbStar2.Tag.ToString
    End Sub

    Private Sub pbStar2_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStar2.MouseLeave
        Try
            Dim tmpDBL As Single = 0
            Single.TryParse(Me.tmpRating, tmpDBL)
            Me.BuildStars(tmpDBL)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar2_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbStar2.MouseMove
        Try
            If e.X < 12 Then
                Me.pbStar2.Tag = 3
                Me.BuildStars(3)
            Else
                Me.pbStar2.Tag = 4
                Me.BuildStars(4)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbStar3.Click
        Me.tmpRating = Me.pbStar3.Tag.ToString
    End Sub

    Private Sub pbStar3_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStar3.MouseLeave
        Try
            Dim tmpDBL As Single = 0
            Single.TryParse(Me.tmpRating, tmpDBL)
            Me.BuildStars(tmpDBL)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar3_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbStar3.MouseMove
        Try
            If e.X < 12 Then
                Me.pbStar3.Tag = 5
                Me.BuildStars(5)
            Else
                Me.pbStar3.Tag = 6
                Me.BuildStars(6)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbStar4.Click
        Me.tmpRating = Me.pbStar4.Tag.ToString
    End Sub

    Private Sub pbStar4_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStar4.MouseLeave
        Try
            Dim tmpDBL As Single = 0
            Single.TryParse(Me.tmpRating, tmpDBL)
            Me.BuildStars(tmpDBL)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar4_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbStar4.MouseMove
        Try
            If e.X < 12 Then
                Me.pbStar4.Tag = 7
                Me.BuildStars(7)
            Else
                Me.pbStar4.Tag = 8
                Me.BuildStars(8)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbStar5.Click
        Me.tmpRating = Me.pbStar5.Tag.ToString
    End Sub

    Private Sub pbStar5_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStar5.MouseLeave
        Try
            Dim tmpDBL As Single = 0
            Single.TryParse(Me.tmpRating, tmpDBL)
            Me.BuildStars(tmpDBL)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbStar5_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbStar5.MouseMove
        Try
            If e.X < 12 Then
                Me.pbStar5.Tag = 9
                Me.BuildStars(9)
            Else
                Me.pbStar5.Tag = 10
                Me.BuildStars(10)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectMPAA()
        If Not String.IsNullOrEmpty(Master.currShow.TVShow.MPAA) Then
            Try
                If Not IsNothing(APIXML.RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower)) AndAlso APIXML.RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower).Descendants("tv").Count > 0 Then
                    Dim l As Integer = Me.lbMPAA.FindString(Strings.Trim(Master.currShow.TVShow.MPAA))
                    Me.lbMPAA.SelectedIndex = l
                    If Me.lbMPAA.SelectedItems.Count = 0 Then
                        Me.lbMPAA.SelectedIndex = 0
                    End If

                    Me.lbMPAA.TopIndex = 0

                Else

                    Me.lbMPAA.SelectedIndex = 0
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            Me.lbMPAA.SelectedIndex = 0
        End If
    End Sub

    Private Sub SetInfo()
        Try
            With Me
                Master.currShow.Ordering = DirectCast(.cbOrdering.SelectedIndex, Enums.Ordering)

                Master.currShow.TVShow.Title = .txtTitle.Text.Trim
                Master.currShow.TVShow.Plot = .txtPlot.Text.Trim
                Master.currShow.TVShow.Premiered = .txtPremiered.Text.Trim
                Master.currShow.TVShow.Studio = .txtStudio.Text.Trim

                If .lbMPAA.SelectedIndices.Count > 0 AndAlso Not .lbMPAA.SelectedIndex <= 0 Then
                    Master.currShow.TVShow.MPAA = .lbMPAA.SelectedItem.ToString
                End If

                Master.currShow.TVShow.Rating = .tmpRating

                If .lbGenre.CheckedItems.Count > 0 Then

                    If .lbGenre.CheckedIndices.Contains(0) Then
                        Master.currShow.TVShow.Genre = String.Empty
                    Else
                        Dim strGenre As String = String.Empty
                        Dim isFirst As Boolean = True
                        Dim iChecked = From iCheck In .lbGenre.CheckedItems
                        strGenre = Strings.Join(iChecked.ToArray, " / ")
                        Master.currShow.TVShow.Genre = strGenre.Trim
                    End If
                End If

                Master.currShow.TVShow.Actors.Clear()

                If .lvActors.Items.Count > 0 Then
                    For Each lviActor As ListViewItem In .lvActors.Items
                        Dim addActor As New MediaContainers.Person
                        addActor.Name = lviActor.Text.Trim
                        addActor.Role = lviActor.SubItems(1).Text.Trim
                        addActor.Thumb = lviActor.SubItems(2).Text.Trim

                        Master.currShow.TVShow.Actors.Add(addActor)
                    Next
                End If

                If Not IsNothing(.Fanart.Image) Then
                    Master.currShow.ShowFanartPath = .Fanart.SaveAsShowFanart(Master.currShow)
                Else
                    .Fanart.DeleteShowFanart(Master.currShow)
                    Master.currShow.ShowFanartPath = String.Empty
                End If

                If Not IsNothing(.Poster.Image) Then
                    Master.currShow.ShowPosterPath = .Poster.SaveAsShowPoster(Master.currShow)
                Else
                    .Poster.DeleteShowPosters(Master.currShow)
                    Master.currShow.ShowPosterPath = String.Empty
                End If

                If Master.eSettings.AllSeasonPosterEnabled Then
                    If Not IsNothing(.ASPoster.Image) Then
                        Master.currShow.SeasonPosterPath = .ASPoster.SaveAsAllSeasonPoster(Master.currShow)
                    Else
                        .ASPoster.DeleteAllSeasonPosters(Master.currShow)
                        Master.currShow.SeasonPosterPath = String.Empty
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetUp()
        Dim mTitle As String = Master.currShow.TVShow.Title
        Dim sTitle As String = String.Concat(Master.eLang.GetString(663, "Edit Show"), If(String.IsNullOrEmpty(mTitle), String.Empty, String.Concat(" - ", mTitle)))
        Me.Text = sTitle
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label2.Text = Master.eLang.GetString(664, "Edit the details for the selected show.")
        Me.Label1.Text = Master.eLang.GetString(663, "Edit Show")
        Me.TabPage1.Text = Master.eLang.GetString(26, "Details")
        Me.lblStudio.Text = Master.eLang.GetString(226, "Studio:")
        Me.btnManual.Text = Master.eLang.GetString(230, "Manual Edit")
        Me.lblActors.Text = Master.eLang.GetString(231, "Actors:")
        Me.colName.Text = Master.eLang.GetString(232, "Name")
        Me.colRole.Text = Master.eLang.GetString(233, "Role")
        Me.colThumb.Text = Master.eLang.GetString(234, "Thumb")
        Me.lblGenre.Text = Master.eLang.GetString(51, "Genre(s):")
        Me.lblMPAA.Text = Master.eLang.GetString(235, "MPAA Rating:")
        Me.lblPlot.Text = Master.eLang.GetString(241, "Plot:")
        Me.lblRating.Text = Master.eLang.GetString(245, "Rating:")
        Me.lblPremiered.Text = Master.eLang.GetString(665, "Premiered:")
        Me.lblTitle.Text = Master.eLang.GetString(246, "Title:")
        Me.TabPage2.Text = Master.eLang.GetString(148, "Poster")
        Me.btnRemovePoster.Text = Master.eLang.GetString(247, "Remove Poster")
        Me.btnSetPosterScrape.Text = Master.eLang.GetString(248, "Change Poster (Scrape)")
        Me.btnSetPoster.Text = Master.eLang.GetString(249, "Change Poster (Local)")
        Me.TabPage3.Text = Master.eLang.GetString(149, "Fanart")
        Me.btnRemoveFanart.Text = Master.eLang.GetString(250, "Remove Fanart")
        Me.btnSetFanartScrape.Text = Master.eLang.GetString(251, "Change Fanart (Scrape)")
        Me.btnSetFanart.Text = Master.eLang.GetString(252, "Change Fanart (Local)")
        Me.btnSetPosterDL.Text = Master.eLang.GetString(265, "Change Poster (Download)")
        Me.btnSetFanartDL.Text = Master.eLang.GetString(266, "Change Fanart (Download)")
        Me.TabPage4.Text = Master.eLang.GetString(786, "All Seasons Poster")
        Me.btnASPosterRemove.Text = Master.eLang.GetString(247, "Remove Poster")
        Me.btnASChangePosterScrape.Text = Master.eLang.GetString(248, "Change Poster (Scrape)")
        Me.btnASChangePoster.Text = Master.eLang.GetString(249, "Change Poster (Local)")
        Me.btnASPosterChangeDL.Text = Master.eLang.GetString(265, "Change Poster (Download)")
        Me.lblOrdering.Text = Master.eLang.GetString(739, "Episode Ordering:")

        Me.cbOrdering.Items.AddRange(New String() {Master.eLang.GetString(438, "Standard"), Master.eLang.GetString(350, "DVD"), Master.eLang.GetString(839, "Absolute")})

    End Sub

#End Region 'Methods

End Class