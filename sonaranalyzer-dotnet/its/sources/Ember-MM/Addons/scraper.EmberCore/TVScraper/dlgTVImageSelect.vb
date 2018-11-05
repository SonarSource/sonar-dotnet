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

'TODO: 1.5 - TV Show renaming (including "dump folder")
'TODO: 1.5 - Support VIDEO_TS/BDMV folders for TV Shows

Imports System.IO
Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class dlgTVImageSelect

#Region "Fields"

    Friend WithEvents bwDownloadFanart As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadData As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadImages As New System.ComponentModel.BackgroundWorker

    Private DefaultImages As New Scraper.TVImages
    Private FanartList As New List(Of Scraper.TVDBFanart)
    Private GenericPosterList As New List(Of Scraper.TVDBPoster)
    Private iCounter As Integer = 0
    Private iLeft As Integer = 5
    Private iTop As Integer = 5
    Private lblImage() As Label
    Private pbImage() As PictureBox
    Private pnlImage() As Panel
    Private SeasonList As New List(Of Scraper.TVDBSeasonPoster)
    Private SelIsPoster As Boolean = True
    Private SelSeason As Integer = -999
    Private ShowPosterList As New List(Of Scraper.TVDBShowPoster)
    Private _fanartchanged As Boolean = False
    Private _id As Integer = -1
    Private _season As Integer = -999
    Private _type As Enums.TVImageType = Enums.TVImageType.All
    Private _withcurrent As Boolean = True
    Private _ScrapeType As Enums.ScrapeType

#End Region 'Fields

#Region "Methods"

    Public Function SetDefaults() As Boolean
        Dim iSeason As Integer = -1
        Dim iEpisode As Integer = -1
        Dim iProgress As Integer = 3

        Dim tSea As Scraper.TVDBSeasonPoster

        Try
            Me.bwLoadImages.ReportProgress(Scraper.TVDBImages.SeasonImageList.Count + Scraper.tmpTVDBShow.Episodes.Count + 3, "defaults")

            If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowPoster) AndAlso IsNothing(Scraper.TVDBImages.ShowPoster.Image.Image) Then
                If Master.eSettings.IsShowBanner Then
                    Dim tSP As Scraper.TVDBShowPoster = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Type = Master.eSettings.PreferredShowBannerType AndAlso p.Language = Master.eSettings.TVDBLanguage)

                    If Master.eSettings.OnlyGetTVImagesForSelectedLanguage Then
                        If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Language = Master.eSettings.TVDBLanguage)
                    End If

                    If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Type = Master.eSettings.PreferredShowBannerType)

                    'no preferred size, just get any one of them
                    If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                    If Not IsNothing(tSP) Then
                        Scraper.TVDBImages.ShowPoster.Image.Image = tSP.Image.Image
                        Scraper.TVDBImages.ShowPoster.LocalFile = tSP.LocalFile
                        Scraper.TVDBImages.ShowPoster.URL = tSP.URL
                    Else
                        'still nothing? try to get from generic posters
                        Dim tSPg As Scraper.TVDBPoster = GenericPosterList.FirstOrDefault(Function(p) p.Language = Master.eSettings.TVDBLanguage AndAlso Not IsNothing(p.Image.Image))

                        If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                        If Not IsNothing(tSPg) Then
                            Scraper.TVDBImages.ShowPoster.Image.Image = tSPg.Image.Image
                            Scraper.TVDBImages.ShowPoster.LocalFile = tSPg.LocalFile
                            Scraper.TVDBImages.ShowPoster.URL = tSPg.URL
                        End If
                    End If
                Else
                    Dim tSPg As Scraper.TVDBPoster = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso Me.GetPosterDims(p.Size) = Master.eSettings.PreferredShowPosterSize AndAlso p.Language = Master.eSettings.TVDBLanguage)

                    If Master.eSettings.OnlyGetTVImagesForSelectedLanguage Then
                        If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Language = Master.eSettings.TVDBLanguage)
                    End If

                    If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso Me.GetPosterDims(p.Size) = Master.eSettings.PreferredShowPosterSize)

                    'no preferred size, just get any one of them
                    If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                    If Not IsNothing(tSPg) Then
                        Scraper.TVDBImages.ShowPoster.Image.Image = tSPg.Image.Image
                        Scraper.TVDBImages.ShowPoster.LocalFile = tSPg.LocalFile
                        Scraper.TVDBImages.ShowPoster.URL = tSPg.URL
                    Else
                        Dim tSP As Scraper.TVDBShowPoster = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Language = Master.eSettings.TVDBLanguage)

                        If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                        If Not IsNothing(tSP) Then
                            Scraper.TVDBImages.ShowPoster.Image.Image = tSP.Image.Image
                            Scraper.TVDBImages.ShowPoster.LocalFile = tSP.LocalFile
                            Scraper.TVDBImages.ShowPoster.URL = tSP.URL
                        End If
                    End If
                End If
            End If

            If Me.bwLoadImages.CancellationPending Then
                Return True
            End If
            Me.bwLoadImages.ReportProgress(1, "progress")

            If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowFanart OrElse Me._type = Enums.TVImageType.EpisodeFanart) AndAlso IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                Dim tSF As Scraper.TVDBFanart = FanartList.FirstOrDefault(Function(f) Not IsNothing(f.Image.Image) AndAlso Me.GetFanartDims(f.Size) = Master.eSettings.PreferredShowFanartSize AndAlso f.Language = Master.eSettings.TVDBLanguage)

                If IsNothing(tSF) Then tSF = FanartList.FirstOrDefault(Function(f) Not IsNothing(f.Image.Image) AndAlso Me.GetFanartDims(f.Size) = Master.eSettings.PreferredShowFanartSize)

                'no fanart of the preferred size, just get the first available
                If IsNothing(tSF) Then tSF = FanartList.FirstOrDefault(Function(f) Not IsNothing(f.Image.Image))

                If Not IsNothing(tSF) Then
                    If Not String.IsNullOrEmpty(tSF.LocalFile) AndAlso File.Exists(tSF.LocalFile) Then
                        Scraper.TVDBImages.ShowFanart.Image.FromFile(tSF.LocalFile)
                        Scraper.TVDBImages.ShowFanart.LocalFile = tSF.LocalFile
                        Scraper.TVDBImages.ShowFanart.URL = tSF.URL
                    ElseIf Not String.IsNullOrEmpty(tSF.LocalFile) AndAlso Not String.IsNullOrEmpty(tSF.URL) Then
                        Scraper.TVDBImages.ShowFanart.Image.FromWeb(tSF.URL)
                        If Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                            Directory.CreateDirectory(Directory.GetParent(tSF.LocalFile).FullName)
                            Scraper.TVDBImages.ShowFanart.Image.Save(tSF.LocalFile)
                            Scraper.TVDBImages.ShowFanart.LocalFile = tSF.LocalFile
                            Scraper.TVDBImages.ShowFanart.URL = tSF.URL
                        End If
                    End If
                End If
            End If

            If Me.bwLoadImages.CancellationPending Then
                Return True
            End If
            Me.bwLoadImages.ReportProgress(2, "progress")

            If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.AllSeasonPoster) AndAlso Master.eSettings.AllSeasonPosterEnabled AndAlso IsNothing(Scraper.TVDBImages.AllSeasonPoster.Image.Image) Then
                If Master.eSettings.IsAllSBanner Then
                    Dim tSP As Scraper.TVDBShowPoster = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Type = Master.eSettings.PreferredAllSBannerType AndAlso p.Language = Master.eSettings.TVDBLanguage)

                    If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Type = Master.eSettings.PreferredAllSBannerType)

                    'no preferred size, just get any one of them
                    If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                    If Not IsNothing(tSP) Then
                        Scraper.TVDBImages.AllSeasonPoster.Image.Image = tSP.Image.Image
                        Scraper.TVDBImages.AllSeasonPoster.LocalFile = tSP.LocalFile
                        Scraper.TVDBImages.AllSeasonPoster.URL = tSP.URL
                    Else
                        'still nothing? try to get from generic posters
                        Dim tSPg As Scraper.TVDBPoster = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Language = Master.eSettings.TVDBLanguage)

                        If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                        If Not IsNothing(tSPg) Then
                            Scraper.TVDBImages.AllSeasonPoster.Image.Image = tSPg.Image.Image
                            Scraper.TVDBImages.AllSeasonPoster.LocalFile = tSPg.LocalFile
                            Scraper.TVDBImages.AllSeasonPoster.URL = tSPg.URL
                        End If
                    End If
                Else
                    Dim tSPg As Scraper.TVDBPoster = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso Me.GetPosterDims(p.Size) = Master.eSettings.PreferredAllSPosterSize AndAlso p.Language = Master.eSettings.TVDBLanguage)

                    If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso Me.GetPosterDims(p.Size) = Master.eSettings.PreferredAllSPosterSize)

                    'no preferred size, just get any one of them
                    If IsNothing(tSPg) Then tSPg = GenericPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                    If Not IsNothing(tSPg) Then
                        Scraper.TVDBImages.AllSeasonPoster.Image.Image = tSPg.Image.Image
                        Scraper.TVDBImages.AllSeasonPoster.LocalFile = tSPg.LocalFile
                        Scraper.TVDBImages.AllSeasonPoster.URL = tSPg.URL
                    Else
                        Dim tSP As Scraper.TVDBShowPoster = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Language = Master.eSettings.TVDBLanguage)

                        If IsNothing(tSP) Then tSP = ShowPosterList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image))

                        If Not IsNothing(tSP) Then
                            Scraper.TVDBImages.AllSeasonPoster.Image.Image = tSP.Image.Image
                            Scraper.TVDBImages.AllSeasonPoster.LocalFile = tSP.LocalFile
                            Scraper.TVDBImages.AllSeasonPoster.URL = tSP.URL
                        End If
                    End If
                End If
            End If

            If Me.bwLoadImages.CancellationPending Then
                Return True
            End If
            Me.bwLoadImages.ReportProgress(3, "progress")

            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.SeasonPoster OrElse Me._type = Enums.TVImageType.SeasonFanart Then
                For Each cSeason As Scraper.TVDBSeasonImage In Scraper.TVDBImages.SeasonImageList
                    Try
                        iSeason = cSeason.Season
                        If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.SeasonPoster) AndAlso IsNothing(cSeason.Poster.Image) Then
                            tSea = SeasonList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Season = iSeason AndAlso p.Type = Master.eSettings.PreferredSeasonPosterSize AndAlso p.Language = Master.eSettings.TVDBLanguage)
                            If IsNothing(tSea) Then tSea = SeasonList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Season = iSeason AndAlso p.Type = Master.eSettings.PreferredSeasonPosterSize)
                            If IsNothing(tSea) Then tSea = SeasonList.FirstOrDefault(Function(p) Not IsNothing(p.Image.Image) AndAlso p.Season = iSeason)
                            If Not IsNothing(tSea) Then cSeason.Poster.Image = tSea.Image.Image
                        End If
                        If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.SeasonFanart) AndAlso Master.eSettings.SeasonFanartEnabled AndAlso IsNothing(cSeason.Fanart.Image.Image) AndAlso Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then cSeason.Fanart.Image.Image = Scraper.TVDBImages.ShowFanart.Image.Image

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If
                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

            If Me._type = Enums.TVImageType.All Then
                For Each Episode As Structures.DBTV In Scraper.tmpTVDBShow.Episodes
                    Try
                        If Not String.IsNullOrEmpty(Episode.TVEp.LocalFile) Then
                            Episode.TVEp.Poster.FromFile(Episode.TVEp.LocalFile)
                        ElseIf Not String.IsNullOrEmpty(Episode.EpPosterPath) Then
                            Episode.TVEp.Poster.FromFile(Episode.EpPosterPath)
                        End If

                        If Master.eSettings.EpisodeFanartEnabled Then
                            If Not String.IsNullOrEmpty(Episode.EpFanartPath) Then
                                Episode.TVEp.Fanart.FromFile(Episode.EpFanartPath)
                            ElseIf Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                                Episode.TVEp.Fanart.Image = Scraper.TVDBImages.ShowFanart.Image.Image
                            End If
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If
                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        DefaultImages = Scraper.TVDBImages.Clone

        Return False
    End Function

    Public Overloads Function ShowDialog(ByVal ShowID As Integer, ByVal Type As Enums.TVImageType, ByVal ScrapeType As Enums.ScrapeType, ByVal WithCurrent As Boolean) As DialogResult
        Me._id = ShowID
        Me._type = Type
        Me._withcurrent = WithCurrent
        Me._ScrapeType = ScrapeType
        Return MyBase.ShowDialog
    End Function

    Public Overloads Function ShowDialog(ByVal ShowID As Integer, ByVal Type As Enums.TVImageType, ByVal Season As Integer, ByVal CurrentImage As Image) As Image
        Me._id = ShowID
        Me._type = Type
        Me._season = Season
        Me.pbCurrent.Image = CurrentImage

        If MyBase.ShowDialog = DialogResult.OK Then
            Return Me.pbCurrent.Image
        Else
            Return Nothing
        End If
    End Function

    Private Sub AddImage(ByVal iImage As Image, ByVal sDescription As String, ByVal iIndex As Integer, ByVal fTag As ImageTag)
        Try
            ReDim Preserve Me.pnlImage(iIndex)
            ReDim Preserve Me.pbImage(iIndex)
            ReDim Preserve Me.lblImage(iIndex)
            Me.pnlImage(iIndex) = New Panel()
            Me.pbImage(iIndex) = New PictureBox()
            Me.lblImage(iIndex) = New Label()
            Me.pbImage(iIndex).Name = iIndex.ToString
            Me.pnlImage(iIndex).Name = iIndex.ToString
            Me.lblImage(iIndex).Name = iIndex.ToString
            Me.pnlImage(iIndex).Size = New Size(187, 187)
            Me.pbImage(iIndex).Size = New Size(181, 151)
            Me.lblImage(iIndex).Size = New Size(181, 30)
            Me.pnlImage(iIndex).BackColor = Color.White
            Me.pnlImage(iIndex).BorderStyle = BorderStyle.FixedSingle
            Me.pbImage(iIndex).SizeMode = PictureBoxSizeMode.Zoom
            Me.lblImage(iIndex).AutoSize = False
            Me.lblImage(iIndex).BackColor = Color.White
            Me.lblImage(iIndex).TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            Me.lblImage(iIndex).Text = sDescription
            Me.pbImage(iIndex).Image = iImage
            Me.pnlImage(iIndex).Left = iLeft
            Me.pbImage(iIndex).Left = 3
            Me.lblImage(iIndex).Left = 0
            Me.pnlImage(iIndex).Top = iTop
            Me.pbImage(iIndex).Top = 3
            Me.lblImage(iIndex).Top = 151
            Me.pnlImage(iIndex).Tag = fTag
            Me.pbImage(iIndex).Tag = fTag
            Me.lblImage(iIndex).Tag = fTag
            Me.pnlImages.Controls.Add(Me.pnlImage(iIndex))
            Me.pnlImage(iIndex).Controls.Add(Me.pbImage(iIndex))
            Me.pnlImage(iIndex).Controls.Add(Me.lblImage(iIndex))
            Me.pnlImage(iIndex).BringToFront()
            AddHandler pbImage(iIndex).Click, AddressOf pbImage_Click
            AddHandler pbImage(iIndex).DoubleClick, AddressOf pbImage_DoubleClick
            AddHandler pnlImage(iIndex).Click, AddressOf pnlImage_Click
            AddHandler lblImage(iIndex).Click, AddressOf lblImage_Click

            AddHandler pbImage(iIndex).MouseWheel, AddressOf MouseWheelEvent
            AddHandler pnlImage(iIndex).MouseWheel, AddressOf MouseWheelEvent
            AddHandler lblImage(iIndex).MouseWheel, AddressOf MouseWheelEvent

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.iCounter += 1

        If Me.iCounter = 3 Then
            Me.iCounter = 0
            Me.iLeft = 5
            Me.iTop += 192
        Else
            Me.iLeft += 192
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If Me.bwLoadData.IsBusy Then Me.bwLoadData.CancelAsync()
        If Me.bwLoadImages.IsBusy Then Me.bwLoadImages.CancelAsync()

        While Me.bwLoadData.IsBusy OrElse Me.bwLoadImages.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        DoneAndClose()
    End Sub

    Private Sub DoneAndClose()
        If Me._type = Enums.TVImageType.All Then
            Me.lblStatus.Text = Master.eLang.GetString(87, "Downloading Fullsize Fanart Image...")
            Me.pbStatus.Style = ProgressBarStyle.Marquee
            Me.pnlStatus.Visible = True
            Master.currShow.ShowPosterPath = Scraper.TVDBImages.ShowPoster.LocalFile
            If Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.LocalFile) AndAlso File.Exists(Scraper.TVDBImages.ShowFanart.LocalFile) Then
                Scraper.TVDBImages.ShowFanart.Image.FromFile(Scraper.TVDBImages.ShowFanart.LocalFile)
                Master.currShow.ShowFanartPath = Scraper.TVDBImages.ShowFanart.LocalFile
            ElseIf Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.URL) AndAlso Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.LocalFile) Then
                Scraper.TVDBImages.ShowFanart.Image.Clear()
                Scraper.TVDBImages.ShowFanart.Image.FromWeb(Scraper.TVDBImages.ShowFanart.URL)
                If Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                    Directory.CreateDirectory(Directory.GetParent(Scraper.TVDBImages.ShowFanart.LocalFile).FullName)
                    Scraper.TVDBImages.ShowFanart.Image.Save(Scraper.TVDBImages.ShowFanart.LocalFile)
                    Master.currShow.ShowFanartPath = Scraper.TVDBImages.ShowFanart.LocalFile
                End If
            End If
            If Master.eSettings.AllSeasonPosterEnabled AndAlso Not IsNothing(Scraper.TVDBImages.AllSeasonPoster.Image.Image) Then
                Master.currShow.SeasonPosterPath = Scraper.TVDBImages.AllSeasonPoster.LocalFile
            End If
        ElseIf Me._type = Enums.TVImageType.SeasonFanart AndAlso Me._fanartchanged Then
            Me.lblStatus.Text = Master.eLang.GetString(87, "Downloading Fullsize Fanart Image...")
            Me.pbStatus.Style = ProgressBarStyle.Marquee
            Me.pnlStatus.Visible = True
            If Not String.IsNullOrEmpty(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile) AndAlso File.Exists(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile) Then
                Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.FromFile(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile)
                Me.pbCurrent.Image = Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.Image
            ElseIf Not String.IsNullOrEmpty(Scraper.TVDBImages.SeasonImageList(0).Fanart.URL) AndAlso Not String.IsNullOrEmpty(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile) Then
                Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.Clear()
                Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.FromWeb(Scraper.TVDBImages.SeasonImageList(0).Fanart.URL)
                If Not IsNothing(Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.Image) Then
                    Directory.CreateDirectory(Directory.GetParent(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile).FullName)
                    Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.Save(Scraper.TVDBImages.SeasonImageList(0).Fanart.LocalFile)
                    Me.pbCurrent.Image = Scraper.TVDBImages.SeasonImageList(0).Fanart.Image.Image
                End If
            End If
        ElseIf (Me._type = Enums.TVImageType.ShowFanart OrElse Me._type = Enums.TVImageType.EpisodeFanart) AndAlso Me._fanartchanged Then
            Me.lblStatus.Text = Master.eLang.GetString(87, "Downloading Fullsize Fanart Image...")
            Me.pbStatus.Style = ProgressBarStyle.Marquee
            Me.pnlStatus.Visible = True
            If Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.LocalFile) AndAlso File.Exists(Scraper.TVDBImages.ShowFanart.LocalFile) Then
                Scraper.TVDBImages.ShowFanart.Image.FromFile(Scraper.TVDBImages.ShowFanart.LocalFile)
                Me.pbCurrent.Image = Scraper.TVDBImages.ShowFanart.Image.Image
            ElseIf Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.URL) AndAlso Not String.IsNullOrEmpty(Scraper.TVDBImages.ShowFanart.LocalFile) Then
                Scraper.TVDBImages.ShowFanart.Image.Clear()
                Scraper.TVDBImages.ShowFanart.Image.FromWeb(Scraper.TVDBImages.ShowFanart.URL)
                If Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                    Directory.CreateDirectory(Directory.GetParent(Scraper.TVDBImages.ShowFanart.LocalFile).FullName)
                    Scraper.TVDBImages.ShowFanart.Image.Save(Scraper.TVDBImages.ShowFanart.LocalFile)
                    Me.pbCurrent.Image = Scraper.TVDBImages.ShowFanart.Image.Image
                End If
            End If
        End If

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub


    Private Sub bwLoadData_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadData.DoWork
        Dim cSI As Scraper.TVDBSeasonImage
        Dim iProgress As Integer = 1
        Dim iSeason As Integer = -1

        Me.bwLoadData.ReportProgress(Scraper.tmpTVDBShow.Episodes.Count, "current")

        'initialize the struct
        Scraper.TVDBImages.ShowPoster = New Scraper.TVDBShowPoster
        Scraper.TVDBImages.ShowFanart = New Scraper.TVDBFanart
        Scraper.TVDBImages.AllSeasonPoster = New Scraper.TVDBShowPoster
        Scraper.TVDBImages.SeasonImageList = New List(Of Scraper.TVDBSeasonImage)

        If Me.bwLoadData.CancellationPending Then
            e.Cancel = True
            Return
        End If

        Select Case Me._type
            Case Enums.TVImageType.AllSeasonPoster
                Scraper.TVDBImages.AllSeasonPoster.Image.Image = Me.pbCurrent.Image
            Case Enums.TVImageType.SeasonFanart
                cSI = New Scraper.TVDBSeasonImage
                cSI.Season = Me._season
                cSI.Fanart.Image.Image = Me.pbCurrent.Image
                Scraper.TVDBImages.SeasonImageList.Add(cSI)
            Case Enums.TVImageType.SeasonPoster
                cSI = New Scraper.TVDBSeasonImage
                cSI.Season = Me._season
                cSI.Poster.Image = Me.pbCurrent.Image
                Scraper.TVDBImages.SeasonImageList.Add(cSI)
            Case Enums.TVImageType.ShowFanart, Enums.TVImageType.EpisodeFanart
                Scraper.TVDBImages.ShowFanart.Image.Image = Me.pbCurrent.Image
            Case Enums.TVImageType.ShowPoster
                Scraper.TVDBImages.ShowPoster.Image.Image = Me.pbCurrent.Image
            Case Enums.TVImageType.All

                If _withcurrent Then
                    If Not String.IsNullOrEmpty(Scraper.tmpTVDBShow.Show.ShowPosterPath) Then
                        Scraper.TVDBImages.ShowPoster.Image.FromFile(Scraper.tmpTVDBShow.Show.ShowPosterPath)
                        Scraper.TVDBImages.ShowPoster.LocalFile = Scraper.tmpTVDBShow.Show.ShowPosterPath
                    End If

                    If Me.bwLoadData.CancellationPending Then
                        e.Cancel = True
                        Return
                    End If

                    If Not String.IsNullOrEmpty(Scraper.tmpTVDBShow.Show.ShowFanartPath) Then
                        Scraper.TVDBImages.ShowFanart.Image.FromFile(Scraper.tmpTVDBShow.Show.ShowFanartPath)
                        Scraper.TVDBImages.ShowFanart.LocalFile = Scraper.tmpTVDBShow.Show.ShowFanartPath
                    End If

                    If Me.bwLoadData.CancellationPending Then
                        e.Cancel = True
                        Return
                    End If

                    If Master.eSettings.AllSeasonPosterEnabled AndAlso Not String.IsNullOrEmpty(Scraper.tmpTVDBShow.AllSeason.SeasonPosterPath) Then
                        Scraper.TVDBImages.AllSeasonPoster.Image.FromFile(Scraper.tmpTVDBShow.AllSeason.SeasonPosterPath)
                        Scraper.TVDBImages.AllSeasonPoster.LocalFile = Scraper.tmpTVDBShow.AllSeason.SeasonPosterPath
                    End If

                    If Me.bwLoadData.CancellationPending Then
                        e.Cancel = True
                        Return
                    End If

                    For Each sEpisode As Structures.DBTV In Scraper.tmpTVDBShow.Episodes
                        Try
                            iSeason = sEpisode.TVEp.Season
                            If iSeason > -1 Then
                                If IsNothing(Scraper.TVDBImages.ShowPoster.Image) AndAlso Not String.IsNullOrEmpty(sEpisode.ShowPosterPath) Then
                                    Scraper.TVDBImages.ShowPoster.Image.FromFile(sEpisode.ShowPosterPath)
                                End If

                                If Me.bwLoadData.CancellationPending Then
                                    e.Cancel = True
                                    Return
                                End If

                                If Master.eSettings.EpisodeFanartEnabled AndAlso IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) AndAlso Not String.IsNullOrEmpty(sEpisode.ShowFanartPath) Then
                                    Scraper.TVDBImages.ShowFanart.Image.FromFile(sEpisode.ShowFanartPath)
                                    Scraper.TVDBImages.ShowFanart.LocalFile = sEpisode.ShowFanartPath
                                End If

                                If Me.bwLoadData.CancellationPending Then
                                    e.Cancel = True
                                    Return
                                End If

                                If Scraper.TVDBImages.SeasonImageList.Where(Function(s) s.Season = iSeason).Count = 0 Then
                                    cSI = New Scraper.TVDBSeasonImage
                                    cSI.Season = iSeason
                                    If Not String.IsNullOrEmpty(sEpisode.SeasonPosterPath) Then
                                        cSI.Poster.FromFile(sEpisode.SeasonPosterPath)
                                    End If
                                    If Master.eSettings.SeasonFanartEnabled AndAlso Not String.IsNullOrEmpty(sEpisode.SeasonFanartPath) Then
                                        cSI.Fanart.Image.FromFile(sEpisode.SeasonFanartPath)
                                        cSI.Fanart.LocalFile = sEpisode.SeasonFanartPath
                                    End If
                                    Scraper.TVDBImages.SeasonImageList.Add(cSI)
                                End If

                                If Me.bwLoadData.CancellationPending Then
                                    e.Cancel = True
                                    Return
                                End If
                            End If
                            Me.bwLoadData.ReportProgress(iProgress, "progress")
                            iProgress += 1
                        Catch ex As Exception
                            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                        End Try
                    Next
                Else
                    For Each sEpisode As Structures.DBTV In Scraper.tmpTVDBShow.Episodes
                        Try
                            iSeason = sEpisode.TVEp.Season

                            If Scraper.TVDBImages.SeasonImageList.Where(Function(s) s.Season = iSeason).Count = 0 Then
                                cSI = New Scraper.TVDBSeasonImage
                                cSI.Season = iSeason
                                Scraper.TVDBImages.SeasonImageList.Add(cSI)
                            End If

                            If Me.bwLoadData.CancellationPending Then
                                e.Cancel = True
                                Return
                            End If

                            Me.bwLoadData.ReportProgress(iProgress, "progress")
                            iProgress += 1
                        Catch ex As Exception
                            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                        End Try
                    Next
                End If
        End Select
    End Sub

    Private Sub bwLoadData_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadData.ProgressChanged
        Try
            If e.UserState.ToString = "progress" Then
                Me.pbStatus.Value = e.ProgressPercentage
            ElseIf e.UserState.ToString = "current" Then
                Me.lblStatus.Text = Master.eLang.GetString(88, "Loading Current Images...")
                Me.pbStatus.Value = 0
                Me.pbStatus.Maximum = e.ProgressPercentage
            Else
                Me.pbStatus.Value = 0
                Me.pbStatus.Maximum = e.ProgressPercentage
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadData_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadData.RunWorkerCompleted
        If Not e.Cancelled Then
            Me.GenerateList()

            Me.lblStatus.Text = Master.eLang.GetString(89, "(Down)Loading New Images...")
            Me.bwLoadImages.WorkerReportsProgress = True
            Me.bwLoadImages.WorkerSupportsCancellation = True
            Me.bwLoadImages.RunWorkerAsync()
        End If
    End Sub

    Private Sub bwLoadImages_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadImages.DoWork
        e.Cancel = Me.DownloadAllImages()
    End Sub

    Private Sub bwLoadImages_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadImages.ProgressChanged
        Try
            If e.UserState.ToString = "progress" Then
                Me.pbStatus.Value = e.ProgressPercentage
            ElseIf e.UserState.ToString = "defaults" Then
                Me.lblStatus.Text = Master.eLang.GetString(90, "Setting Defaults...")
                Me.pbStatus.Value = 0
                Me.pbStatus.Maximum = e.ProgressPercentage
            Else
                Me.pbStatus.Value = 0
                Me.pbStatus.Maximum = e.ProgressPercentage
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadImages_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadImages.RunWorkerCompleted
        Me.pnlStatus.Visible = False
        If _ScrapeType = Enums.ScrapeType.FullAuto Then
            DoneAndClose()
        Else
            If Not e.Cancelled Then
                Me.tvList.Enabled = True
                Me.tvList.Visible = True
                Me.tvList.SelectedNode = Me.tvList.Nodes(0)
                Me.tvList.Focus()

                Me.btnOK.Enabled = True
            End If

            Me.pbCurrent.Visible = True
            Me.lblCurrentImage.Visible = True
        End If
    End Sub

    Private Sub CheckCurrentImage()
        Me.pbDelete.Visible = Not IsNothing(Me.pbCurrent.Image) AndAlso Me.pbCurrent.Visible
        Me.pbUndo.Visible = Me.pbCurrent.Visible
    End Sub

    Private Sub ClearImages()
        Try
            Me.iCounter = 0
            Me.iLeft = 5
            Me.iTop = 5
            Me.pbCurrent.Image = Nothing

            If Me.pnlImages.Controls.Count > 0 Then
                For i As Integer = UBound(Me.pnlImage) To 0 Step -1
                    If Not IsNothing(Me.pnlImage(i)) Then
                        If Not IsNothing(Me.lblImage(i)) AndAlso Me.pnlImage(i).Contains(Me.lblImage(i)) Then Me.pnlImage(i).Controls.Remove(Me.lblImage(i))
                        If Not IsNothing(Me.pbImage(i)) AndAlso Me.pnlImage(i).Contains(Me.pbImage(i)) Then Me.pnlImage(i).Controls.Remove(Me.pbImage(i))
                        If Me.pnlImages.Contains(Me.pnlImage(i)) Then Me.pnlImages.Controls.Remove(Me.pnlImage(i))
                    End If
                Next
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dlgTVImageSelect_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        AddHandler pnlImages.MouseWheel, AddressOf MouseWheelEvent
        AddHandler MyBase.MouseWheel, AddressOf MouseWheelEvent
        AddHandler tvList.MouseWheel, AddressOf MouseWheelEvent

        Functions.PNLDoubleBuffer(Me.pnlImages)

        Me.SetUp()
    End Sub

    Private Sub dlgTVImageSelect_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.bwLoadData.WorkerReportsProgress = True
        Me.bwLoadData.WorkerSupportsCancellation = True
        Me.bwLoadData.RunWorkerAsync()
    End Sub

    Private Sub DoSelect(ByVal iIndex As Integer, ByVal SelImage As Image, ByVal SelTag As ImageTag)
        Try
            For i As Integer = 0 To UBound(Me.pnlImage)
                Me.pnlImage(i).BackColor = Color.White
                Me.lblImage(i).BackColor = Color.White
                Me.lblImage(i).ForeColor = Color.Black
            Next

            Me.pnlImage(iIndex).BackColor = Color.Blue
            Me.lblImage(iIndex).BackColor = Color.Blue
            Me.lblImage(iIndex).ForeColor = Color.White

            SetImage(SelImage, SelTag)

            Me.CheckCurrentImage()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function DownloadAllImages() As Boolean
        Dim iProgress As Integer = 1

        Try
            Me.bwLoadImages.ReportProgress(Scraper.tmpTVDBShow.Episodes.Count + Scraper.tmpTVDBShow.SeasonPosters.Count + Scraper.tmpTVDBShow.ShowPosters.Count + Scraper.tmpTVDBShow.Fanart.Count + Scraper.tmpTVDBShow.Posters.Count, "max")

            If Me._type = Enums.TVImageType.All Then
                For Each Epi As Structures.DBTV In Scraper.tmpTVDBShow.Episodes
                    Try
                        If Not File.Exists(Epi.TVEp.LocalFile) Then
                            If Not String.IsNullOrEmpty(Epi.TVEp.PosterURL) Then
                                Epi.TVEp.Poster.FromWeb(Epi.TVEp.PosterURL)
                                If Not IsNothing(Epi.TVEp.Poster.Image) Then
                                    Directory.CreateDirectory(Directory.GetParent(Epi.TVEp.LocalFile).FullName)
                                    Epi.TVEp.Poster.Save(Epi.TVEp.LocalFile)
                                End If
                            End If
                        Else
                            Epi.TVEp.Poster.FromFile(Epi.TVEp.LocalFile)
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If

                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.SeasonPoster OrElse Me._type = Enums.TVImageType.AllSeasonPoster Then
                For Each Seas As Scraper.TVDBSeasonPoster In Scraper.tmpTVDBShow.SeasonPosters
                    Try
                        If Not File.Exists(Seas.LocalFile) Then
                            If Not String.IsNullOrEmpty(Seas.URL) Then
                                Seas.Image.FromWeb(Seas.URL)
                                If Not IsNothing(Seas.Image.Image) Then
                                    Directory.CreateDirectory(Directory.GetParent(Seas.LocalFile).FullName)
                                    Seas.Image.Save(Seas.LocalFile)
                                    SeasonList.Add(Seas)
                                End If
                            End If
                        Else
                            Seas.Image.FromFile(Seas.LocalFile)
                            SeasonList.Add(Seas)
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If

                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowPoster OrElse Me._type = Enums.TVImageType.AllSeasonPoster Then
                For Each SPost As Scraper.TVDBShowPoster In Scraper.tmpTVDBShow.ShowPosters
                    Try
                        If Not File.Exists(SPost.LocalFile) Then
                            If Not String.IsNullOrEmpty(SPost.URL) Then
                                SPost.Image.FromWeb(SPost.URL)
                                If Not IsNothing(SPost.Image.Image) Then
                                    Directory.CreateDirectory(Directory.GetParent(SPost.LocalFile).FullName)
                                    SPost.Image.Save(SPost.LocalFile)
                                    ShowPosterList.Add(SPost)
                                End If
                            End If
                        Else
                            SPost.Image.FromFile(SPost.LocalFile)
                            ShowPosterList.Add(SPost)
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If

                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowFanart OrElse Me._type = Enums.TVImageType.SeasonFanart OrElse Me._type = Enums.TVImageType.EpisodeFanart Then
                For Each SFan As Scraper.TVDBFanart In Scraper.tmpTVDBShow.Fanart
                    Try
                        If Not File.Exists(SFan.LocalThumb) Then
                            If Not String.IsNullOrEmpty(SFan.ThumbnailURL) Then
                                SFan.Image.FromWeb(SFan.ThumbnailURL)
                                If Not IsNothing(SFan.Image.Image) Then
                                    Directory.CreateDirectory(Directory.GetParent(SFan.LocalThumb).FullName)
                                    SFan.Image.Image.Save(SFan.LocalThumb)
                                    FanartList.Add(SFan)
                                End If
                            End If
                        Else
                            SFan.Image.FromFile(SFan.LocalThumb)
                            FanartList.Add(SFan)
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If

                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowPoster OrElse _
            Me._type = Enums.TVImageType.SeasonPoster OrElse Me._type = Enums.TVImageType.AllSeasonPoster Then
                For Each Post As Scraper.TVDBPoster In Scraper.tmpTVDBShow.Posters
                    Try
                        If Not File.Exists(Post.LocalFile) Then
                            If Not String.IsNullOrEmpty(Post.URL) Then
                                Post.Image.FromWeb(Post.URL)
                                If Not IsNothing(Post.Image.Image) Then
                                    Directory.CreateDirectory(Directory.GetParent(Post.LocalFile).FullName)
                                    Post.Image.Save(Post.LocalFile)
                                    GenericPosterList.Add(Post)
                                End If
                            End If
                        Else
                            Post.Image.FromFile(Post.LocalFile)
                            GenericPosterList.Add(Post)
                        End If

                        If Me.bwLoadImages.CancellationPending Then
                            Return True
                        End If

                        Me.bwLoadImages.ReportProgress(iProgress, "progress")
                        iProgress += 1
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return Me.SetDefaults()
    End Function

    Private Function DownloadFanart(ByVal iTag As ImageTag) As Image
        Dim sHTTP As New HTTP

        Using tImage As New Images
            If Not String.IsNullOrEmpty(iTag.Path) AndAlso File.Exists(iTag.Path) Then
                tImage.FromFile(iTag.Path)
            ElseIf Not String.IsNullOrEmpty(iTag.Path) AndAlso Not String.IsNullOrEmpty(iTag.URL) Then
                Me.lblStatus.Text = Master.eLang.GetString(87, "Downloading Fullsize Fanart Image...")
                Me.pbStatus.Style = ProgressBarStyle.Marquee
                Me.pnlStatus.Visible = True

                Application.DoEvents()

                tImage.FromWeb(iTag.URL)
                If Not IsNothing(tImage.Image) Then
                    Directory.CreateDirectory(Directory.GetParent(iTag.Path).FullName)
                    tImage.Save(iTag.Path)
                End If

                sHTTP = Nothing

                Me.pnlStatus.Visible = False
            End If

            Return tImage.Image
        End Using
    End Function

    Private Sub GenerateList()
        Try
            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowPoster Then Me.tvList.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(91, "Show Poster"), .Tag = "showp", .ImageIndex = 0, .SelectedImageIndex = 0})
            If Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.ShowFanart OrElse Me._type = Enums.TVImageType.EpisodeFanart Then Me.tvList.Nodes.Add(New TreeNode With {.Text = If(Me._type = Enums.TVImageType.EpisodeFanart, Master.eLang.GetString(92, "Episode Fanart"), Master.eLang.GetString(93, "Show Fanart")), .Tag = "showf", .ImageIndex = 1, .SelectedImageIndex = 1})
            If (Me._type = Enums.TVImageType.All OrElse Me._type = Enums.TVImageType.AllSeasonPoster) AndAlso Master.eSettings.AllSeasonPosterEnabled Then Me.tvList.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(94, "All Seasons Poster"), .Tag = "allp", .ImageIndex = 2, .SelectedImageIndex = 2})

            Dim TnS As TreeNode
            If Me._type = Enums.TVImageType.All Then
                For Each cSeason As Scraper.TVDBSeasonImage In Scraper.TVDBImages.SeasonImageList.OrderBy(Function(s) s.Season)
                    Try
                        TnS = New TreeNode(String.Format(Master.eLang.GetString(726, "Season {0}", True), cSeason.Season), 3, 3)
                        TnS.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(95, "Season Posters"), .Tag = String.Concat("p", cSeason.Season.ToString), .ImageIndex = 0, .SelectedImageIndex = 0})
                        If Master.eSettings.SeasonFanartEnabled Then TnS.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(96, "Season Fanart"), .Tag = String.Concat("f", cSeason.Season.ToString), .ImageIndex = 1, .SelectedImageIndex = 1})
                        Me.tvList.Nodes.Add(TnS)
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                Next
            ElseIf Me._type = Enums.TVImageType.SeasonPoster Then
                Me.tvList.Nodes.Add(New TreeNode With {.Text = String.Format(Master.eLang.GetString(97, "Season {0} Posters"), Me._season), .Tag = String.Concat("p", Me._season)})
            ElseIf Me._type = Enums.TVImageType.SeasonFanart Then
                If Master.eSettings.SeasonFanartEnabled Then Me.tvList.Nodes.Add(New TreeNode With {.Text = String.Format(Master.eLang.GetString(99, "Season {0} Fanart"), Me._season), .Tag = String.Concat("f", Me._season)})
            End If

            Me.tvList.ExpandAll()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function GetFanartDims(ByVal fSize As Size) As Enums.FanartSize
        Try
            If (fSize.Width > 1000 AndAlso fSize.Height > 750) OrElse (fSize.Height > 1000 AndAlso fSize.Width > 750) Then
                Return Enums.FanartSize.Lrg
            ElseIf (fSize.Width > 700 AndAlso fSize.Height > 400) OrElse (fSize.Height > 700 AndAlso fSize.Width > 400) Then
                Return Enums.FanartSize.Mid
            Else
                Return Enums.FanartSize.Small
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Function

    Private Function GetPosterDims(ByVal pSize As Size) As Enums.PosterSize
        Try
            If (pSize.Width > pSize.Height) AndAlso (pSize.Width > (pSize.Height * 2)) AndAlso (pSize.Width > 300) Then
                'at least twice as wide than tall... consider it wide (also make sure it's big enough)
                Return Enums.PosterSize.Wide
            ElseIf (pSize.Height > 1000 AndAlso pSize.Width > 750) OrElse (pSize.Width > 1000 AndAlso pSize.Height > 750) Then
                Return Enums.PosterSize.Xlrg
            ElseIf (pSize.Height > 700 AndAlso pSize.Width > 500) OrElse (pSize.Width > 700 AndAlso pSize.Height > 500) Then
                Return Enums.PosterSize.Lrg
            ElseIf (pSize.Height > 250 AndAlso pSize.Width > 150) OrElse (pSize.Width > 250 AndAlso pSize.Height > 150) Then
                Return Enums.PosterSize.Mid
            Else
                Return Enums.PosterSize.Small
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Function

    Private Sub lblImage_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim iindex As Integer = Convert.ToInt32(DirectCast(sender, Label).Name)
        Me.DoSelect(iindex, Me.pbImage(iindex).Image, DirectCast(DirectCast(sender, Label).Tag, ImageTag))
    End Sub

    Private Sub MouseWheelEvent(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Delta < 0 Then
            If (pnlImages.VerticalScroll.Value + 50) <= pnlImages.VerticalScroll.Maximum Then
                pnlImages.VerticalScroll.Value += 50
            Else
                pnlImages.VerticalScroll.Value = pnlImages.VerticalScroll.Maximum
            End If
        Else
            If (pnlImages.VerticalScroll.Value - 50) >= pnlImages.VerticalScroll.Minimum Then
                pnlImages.VerticalScroll.Value -= 50
            Else
                pnlImages.VerticalScroll.Value = pnlImages.VerticalScroll.Minimum
            End If
        End If
    End Sub

    Private Sub pbDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbDelete.Click
        Me.pbCurrent.Image = Nothing
        Me.SetImage(Nothing, New ImageTag)
    End Sub

    Private Sub pbImage_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.DoSelect(Convert.ToInt32(DirectCast(sender, PictureBox).Name), DirectCast(sender, PictureBox).Image, DirectCast(DirectCast(sender, PictureBox).Tag, ImageTag))
    End Sub

    Private Sub pbImage_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim tImage As Image = Nothing
        Dim iTag As ImageTag = DirectCast(DirectCast(sender, PictureBox).Tag, ImageTag)
        If Not IsNothing(iTag) OrElse Not iTag.isFanart Then
            tImage = DownloadFanart(iTag)
        Else
            tImage = DirectCast(sender, PictureBox).Image
        End If

        ModulesManager.Instance.RuntimeObjects.InvokeOpenImageViewer(tImage)
    End Sub

    Private Sub pbUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbUndo.Click
        If Me.SelSeason = -999 Then
            If Me.SelIsPoster Then
                Scraper.TVDBImages.ShowPoster.Image.Image = DefaultImages.ShowPoster.Image.Image
                Scraper.TVDBImages.ShowPoster.LocalFile = DefaultImages.ShowPoster.LocalFile
                Scraper.TVDBImages.ShowPoster.URL = DefaultImages.ShowPoster.URL
                Me.pbCurrent.Image = Scraper.TVDBImages.ShowPoster.Image.Image
            Else
                Scraper.TVDBImages.ShowFanart.Image.Image = DefaultImages.ShowFanart.Image.Image
                Scraper.TVDBImages.ShowFanart.LocalFile = DefaultImages.ShowFanart.LocalFile
                Scraper.TVDBImages.ShowFanart.URL = DefaultImages.ShowFanart.URL
                Me.pbCurrent.Image = Scraper.TVDBImages.ShowFanart.Image.Image
            End If
        ElseIf Me.SelSeason = 999 Then
            Scraper.TVDBImages.AllSeasonPoster.Image.Image = DefaultImages.AllSeasonPoster.Image.Image
            Scraper.TVDBImages.AllSeasonPoster.LocalFile = DefaultImages.AllSeasonPoster.LocalFile
            Scraper.TVDBImages.AllSeasonPoster.URL = DefaultImages.AllSeasonPoster.URL
            Me.pbCurrent.Image = Scraper.TVDBImages.AllSeasonPoster.Image.Image
        Else
            If Me.SelIsPoster Then
                Dim dSPost As Image = DefaultImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Poster.Image
                Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Poster.Image = dSPost
                Me.pbCurrent.Image = dSPost
            Else
                Dim dSFan As Scraper.TVDBFanart = DefaultImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Fanart
                Dim tSFan As Scraper.TVDBFanart = Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Fanart
                tSFan.Image.Image = dSFan.Image.Image
                tSFan.LocalFile = dSFan.LocalFile
                tSFan.URL = dSFan.URL
                Me.pbCurrent.Image = dSFan.Image.Image
            End If
        End If
    End Sub

    Private Sub pnlImage_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim iIndex As Integer = Convert.ToInt32(DirectCast(sender, Panel).Name)
        Me.DoSelect(iIndex, Me.pbImage(iIndex).Image, DirectCast(DirectCast(sender, Panel).Tag, ImageTag))
    End Sub

    Private Sub SetImage(ByVal SelImage As Image, ByVal SelTag As ImageTag)
        Me.pbCurrent.Image = SelImage

        Me._fanartchanged = True

        If Me.SelSeason = -999 Then
            If Me.SelIsPoster Then
                Scraper.TVDBImages.ShowPoster.Image.Image = SelImage
                Scraper.TVDBImages.ShowPoster.LocalFile = SelTag.Path
                Scraper.TVDBImages.ShowPoster.URL = SelTag.URL
            Else
                Scraper.TVDBImages.ShowFanart.Image.Image = SelImage
                Scraper.TVDBImages.ShowFanart.LocalFile = SelTag.Path
                Scraper.TVDBImages.ShowFanart.URL = SelTag.URL
            End If
        ElseIf Me.SelSeason = 999 Then
            Scraper.TVDBImages.AllSeasonPoster.Image.Image = SelImage
            Scraper.TVDBImages.AllSeasonPoster.LocalFile = SelTag.Path
            Scraper.TVDBImages.AllSeasonPoster.URL = SelTag.URL
        Else
            If Me.SelIsPoster Then
                Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Poster.Image = SelImage
            Else
                Dim tFan As Scraper.TVDBFanart = Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(s) s.Season = Me.SelSeason).Fanart
                If Not IsNothing(tFan) Then
                    tFan.Image.Image = SelImage
                    tFan.LocalFile = SelTag.Path
                    tFan.URL = SelTag.URL
                End If
            End If
        End If
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(99, "TV Image Selection")
        Me.btnOK.Text = Master.eLang.GetString(179, "OK", True)
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.lblCurrentImage.Text = Master.eLang.GetString(100, "Current Image:")
    End Sub

    Private Sub tvList_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvList.AfterSelect
        Dim iCount As Integer = 0

        Try
            ClearImages()
            If Not IsNothing(e.Node.Tag) AndAlso Not String.IsNullOrEmpty(e.Node.Tag.ToString) Then
                Me.pbCurrent.Visible = True
                Me.lblCurrentImage.Visible = True
                If e.Node.Tag.ToString = "showp" Then
                    Me.SelSeason = -999
                    Me.SelIsPoster = True
                    If Not IsNothing(Scraper.TVDBImages.ShowPoster) AndAlso Not IsNothing(Scraper.TVDBImages.ShowPoster.Image) AndAlso Not IsNothing(Scraper.TVDBImages.ShowPoster.Image.Image) Then
                        Me.pbCurrent.Image = Scraper.TVDBImages.ShowPoster.Image.Image
                    Else
                        Me.pbCurrent.Image = Nothing
                    End If

                    iCount = ShowPosterList.Count
                    For i = 0 To iCount - 1
                        If Not IsNothing(ShowPosterList(i)) AndAlso Not IsNothing(ShowPosterList(i).Image) AndAlso Not IsNothing(ShowPosterList(i).Image.Image) Then
                            Me.AddImage(ShowPosterList(i).Image.Image, String.Format("{0}x{1}", ShowPosterList(i).Image.Image.Width, ShowPosterList(i).Image.Image.Height), i, New ImageTag With {.URL = ShowPosterList(i).URL, .Path = ShowPosterList(i).LocalFile, .isFanart = False})
                        End If
                    Next

                    For i = 0 To GenericPosterList.Count - 1
                        If Not IsNothing(GenericPosterList(i)) AndAlso Not IsNothing(GenericPosterList(i).Image) AndAlso Not IsNothing(GenericPosterList(i).Image.Image) Then
                            Me.AddImage(GenericPosterList(i).Image.Image, String.Format("{0}x{1}", GenericPosterList(i).Image.Image.Width, GenericPosterList(i).Image.Image.Height), i + iCount, New ImageTag With {.URL = GenericPosterList(i).URL, .Path = GenericPosterList(i).LocalFile, .isFanart = False})
                        End If
                    Next

                ElseIf e.Node.Tag.ToString = "showf" Then

                    Me.SelSeason = -999
                    Me.SelIsPoster = False
                    If Not IsNothing(Scraper.TVDBImages.ShowFanart) AndAlso Not IsNothing(Scraper.TVDBImages.ShowFanart.Image) AndAlso Not IsNothing(Scraper.TVDBImages.ShowFanart.Image.Image) Then
                        Me.pbCurrent.Image = Scraper.TVDBImages.ShowFanart.Image.Image
                    Else
                        Me.pbCurrent.Image = Nothing
                    End If

                    For i = 0 To FanartList.Count - 1
                        If Not IsNothing(FanartList(i)) AndAlso Not IsNothing(FanartList(i).Image) AndAlso Not IsNothing(FanartList(i).Image.Image) Then
                            Me.AddImage(FanartList(i).Image.Image, String.Format("{0}x{1}", FanartList(i).Size.Width, FanartList(i).Size.Height), i, New ImageTag With {.URL = FanartList(i).URL, .Path = FanartList(i).LocalFile, .isFanart = True})
                        End If
                    Next

                ElseIf e.Node.Tag.ToString = "allp" Then
                    Me.SelSeason = 999
                    Me.SelIsPoster = True
                    If Not IsNothing(Scraper.TVDBImages.AllSeasonPoster) AndAlso Not IsNothing(Scraper.TVDBImages.AllSeasonPoster.Image) AndAlso Not IsNothing(Scraper.TVDBImages.AllSeasonPoster.Image.Image) Then
                        Me.pbCurrent.Image = Scraper.TVDBImages.AllSeasonPoster.Image.Image
                    Else
                        Me.pbCurrent.Image = Nothing
                    End If

                    iCount = GenericPosterList.Count
                    For i = 0 To iCount - 1
                        If Not IsNothing(GenericPosterList(i)) AndAlso Not IsNothing(GenericPosterList(i).Image) AndAlso Not IsNothing(GenericPosterList(i).Image.Image) Then
                            Me.AddImage(GenericPosterList(i).Image.Image, String.Format("{0}x{1}", GenericPosterList(i).Image.Image.Width, GenericPosterList(i).Image.Image.Height), i, New ImageTag With {.URL = GenericPosterList(i).URL, .Path = GenericPosterList(i).LocalFile, .isFanart = False})
                        End If
                    Next

                    For i = 0 To ShowPosterList.Count - 1
                        If Not IsNothing(ShowPosterList(i)) AndAlso Not IsNothing(ShowPosterList(i).Image) AndAlso Not IsNothing(ShowPosterList(i).Image.Image) Then
                            Me.AddImage(ShowPosterList(i).Image.Image, String.Format("{0}x{1}", ShowPosterList(i).Image.Image.Width, ShowPosterList(i).Image.Image.Height), i + iCount, New ImageTag With {.URL = ShowPosterList(i).URL, .Path = ShowPosterList(i).LocalFile, .isFanart = False})
                        End If
                    Next
                Else
                    Dim tMatch As Match = Regex.Match(e.Node.Tag.ToString, "(?<type>f|p)(?<num>[0-9]+)")
                    If tMatch.Success Then
                        If tMatch.Groups("type").Value = "f" Then
                            Me.SelSeason = Convert.ToInt32(tMatch.Groups("num").Value)
                            Me.SelIsPoster = False
                            Dim tFanart As Scraper.TVDBSeasonImage = Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(f) f.Season = Convert.ToInt32(tMatch.Groups("num").Value))
                            If Not IsNothing(tFanart) AndAlso Not IsNothing(tFanart.Fanart) AndAlso Not IsNothing(tFanart.Fanart.Image) AndAlso Not IsNothing(tFanart.Fanart.Image.Image) Then
                                Me.pbCurrent.Image = tFanart.Fanart.Image.Image
                            Else
                                Me.pbCurrent.Image = Nothing
                            End If
                            For i = 0 To FanartList.Count - 1
                                If Not IsNothing(FanartList(i)) AndAlso Not IsNothing(FanartList(i).Image) AndAlso Not IsNothing(FanartList(i).Image.Image) Then
                                    Me.AddImage(FanartList(i).Image.Image, String.Format("{0}x{1}", FanartList(i).Size.Width, FanartList(i).Size.Height), i, New ImageTag With {.URL = FanartList(i).URL, .Path = FanartList(i).LocalFile, .isFanart = True})
                                End If
                            Next
                        ElseIf tMatch.Groups("type").Value = "p" Then
                            Me.SelSeason = Convert.ToInt32(tMatch.Groups("num").Value)
                            Me.SelIsPoster = True
                            Dim tPoster As Scraper.TVDBSeasonImage = Scraper.TVDBImages.SeasonImageList.FirstOrDefault(Function(f) f.Season = Me.SelSeason)
                            If Not IsNothing(tPoster) AndAlso Not IsNothing(tPoster.Poster) AndAlso Not IsNothing(tPoster.Poster.Image) Then
                                Me.pbCurrent.Image = tPoster.Poster.Image
                            Else
                                Me.pbCurrent.Image = Nothing
                            End If
                            iCount = 0
                            For Each SImage As Scraper.TVDBSeasonPoster In SeasonList.Where(Function(s) s.Season = Convert.ToInt32(tMatch.Groups("num").Value))
                                If Not IsNothing(SImage.Image) AndAlso Not IsNothing(SImage.Image.Image) Then
                                    Me.AddImage(SImage.Image.Image, String.Format("{0}x{1}", SImage.Image.Image.Width, SImage.Image.Image.Height), iCount, New ImageTag With {.URL = SImage.URL, .Path = SImage.LocalFile, .isFanart = False})
                                End If
                                iCount += 1
                            Next
                        End If
                    End If
                End If
            Else
                Me.pbCurrent.Image = Nothing
                Me.pbCurrent.Visible = False
                Me.lblCurrentImage.Visible = False
            End If

            Me.CheckCurrentImage()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Private Structure ImageTag

#Region "Fields"

        Dim isFanart As Boolean
        Dim Path As String
        Dim URL As String

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class