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

Public Class dlgUpdateMedia

#Region "Fields"

    Private CustomUpdater As New Structures.CustomUpdaterStruct

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog() As Structures.CustomUpdaterStruct
        '//
        ' Overload to pass data
        '\\

        If MyBase.ShowDialog() = DialogResult.OK Then
            Me.CustomUpdater.Canceled = False
        Else
            Me.CustomUpdater.Canceled = True
        End If
        Return Me.CustomUpdater
    End Function

    Private Sub CheckEnable()
        Me.gbOptions.Enabled = chkAllMod.Checked OrElse chkNFOMod.Checked

        If chkAllMod.Checked Then
            chkNFOMod.Checked = chkAllMod.Checked
            chkPosterMod.Checked = chkAllMod.Checked AndAlso ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Poster)
            chkFanartMod.Checked = chkAllMod.Checked AndAlso ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Fanart)
            chkMetaMod.Checked = chkAllMod.Checked AndAlso Not Me.rbUpdateModifier_Missing.Checked AndAlso Master.eSettings.ScanMediaInfo
            chkExtraMod.Checked = chkAllMod.Checked AndAlso (Master.eSettings.AutoThumbs > 0 OrElse Master.eSettings.AutoET)
            chkTrailerMod.Checked = chkAllMod.Checked AndAlso Master.eSettings.DownloadTrailers
        Else
            If chkMetaMod.Checked Then chkMetaMod.Checked = Not Me.rbUpdateModifier_Missing.Checked AndAlso Master.eSettings.ScanMediaInfo AndAlso (Not rbUpdate_Ask.Checked OrElse chkNFOMod.Checked)
        End If

        chkNFOMod.Enabled = Not chkAllMod.Checked
        chkPosterMod.Enabled = Not chkAllMod.Checked AndAlso ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Poster)
        chkFanartMod.Enabled = Not chkAllMod.Checked AndAlso ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Fanart)
        chkMetaMod.Enabled = Not chkAllMod.Checked AndAlso Not Me.rbUpdateModifier_Missing.Checked AndAlso Master.eSettings.ScanMediaInfo AndAlso (Not rbUpdate_Ask.Checked OrElse chkNFOMod.Checked)
        chkExtraMod.Enabled = Not chkAllMod.Checked AndAlso (Master.eSettings.AutoThumbs > 0 OrElse Master.eSettings.AutoET)
        chkTrailerMod.Enabled = Not chkAllMod.Checked AndAlso Master.eSettings.DownloadTrailers

        If chkAllMod.Checked OrElse chkNFOMod.Checked Then
            If chkCast.Checked OrElse chkCrew.Checked OrElse chkDirector.Checked OrElse chkGenre.Checked OrElse _
            chkMPAA.Checked OrElse chkCert.Checked OrElse chkMusicBy.Checked OrElse chkOutline.Checked OrElse chkPlot.Checked OrElse _
            chkProducers.Checked OrElse chkRating.Checked OrElse chkRelease.Checked OrElse chkRuntime.Checked OrElse _
            chkStudio.Checked OrElse chkTagline.Checked OrElse chkTitle.Checked OrElse chkTrailer.Checked OrElse _
            chkVotes.Checked OrElse chkVotes.Checked OrElse chkWriters.Checked OrElse chkYear.Checked OrElse chkTop250.Checked OrElse chkCountry.Checked Then
                Update_Button.Enabled = True
            Else
                Update_Button.Enabled = False
            End If
        ElseIf chkPosterMod.Checked OrElse chkFanartMod.Checked OrElse chkMetaMod.Checked OrElse chkExtraMod.Checked OrElse chkTrailerMod.Checked Then
            Update_Button.Enabled = True
        Else
            Update_Button.Enabled = False
        End If

        If Me.chkAllMod.Checked Then
            Functions.SetScraperMod(Enums.ModType.All, True)
        Else
            Functions.SetScraperMod(Enums.ModType.Extra, chkExtraMod.Checked, False)
            Functions.SetScraperMod(Enums.ModType.Fanart, chkFanartMod.Checked, False)
            Functions.SetScraperMod(Enums.ModType.Meta, chkMetaMod.Checked, False)
            Functions.SetScraperMod(Enums.ModType.NFO, chkNFOMod.Checked, False)
            Functions.SetScraperMod(Enums.ModType.Poster, chkPosterMod.Checked, False)
            Functions.SetScraperMod(Enums.ModType.Trailer, chkTrailerMod.Checked, False)
        End If
    End Sub

    Private Sub CheckNewAndMark()
        Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS ncount FROM movies WHERE new = 1;")
            Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                rbUpdateModifier_New.Enabled = Convert.ToInt32(SQLcount("ncount")) > 0
            End Using

            SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies WHERE mark = 1;")
            Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                rbUpdateModifier_Marked.Enabled = Convert.ToInt32(SQLcount("mcount")) > 0
            End Using
        End Using
    End Sub

    Private Sub chkAllMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkAllMod.Click
        CheckEnable()
    End Sub

    Private Sub chkCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCast.CheckedChanged
        CustomUpdater.Options.bCast = chkCast.Checked
        CheckEnable()
    End Sub

    Private Sub chkCert_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCert.CheckedChanged
        CustomUpdater.Options.bCert = chkCert.Checked
        CheckEnable()
    End Sub

    Private Sub chkCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCrew.CheckedChanged
        CustomUpdater.Options.bOtherCrew = chkCrew.Checked
        CheckEnable()
    End Sub

    Private Sub chkDirector_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDirector.CheckedChanged
        CustomUpdater.Options.bDirector = chkDirector.Checked
        CheckEnable()
    End Sub

    Private Sub chkExtraMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkExtraMod.Click
        CheckEnable()
    End Sub

    Private Sub chkFanartMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkFanartMod.Click
        CheckEnable()
    End Sub

    Private Sub chkGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGenre.CheckedChanged
        CustomUpdater.Options.bGenre = chkGenre.Checked
        CheckEnable()
    End Sub

    Private Sub chkMetaMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkMetaMod.Click
        CheckEnable()
    End Sub

    Private Sub chkMPAA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMPAA.CheckedChanged
        CustomUpdater.Options.bMPAA = chkMPAA.Checked
        CheckEnable()
    End Sub

    Private Sub chkMusicBy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMusicBy.CheckedChanged
        CustomUpdater.Options.bMusicBy = chkMusicBy.Checked
        CheckEnable()
    End Sub

    Private Sub chkNFOMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkNFOMod.Click
        CheckEnable()
    End Sub

    Private Sub chkOutline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOutline.CheckedChanged
        CustomUpdater.Options.bOutline = chkOutline.Checked
        CheckEnable()
    End Sub

    Private Sub chkPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPlot.CheckedChanged
        CustomUpdater.Options.bPlot = chkPlot.Checked
        CheckEnable()
    End Sub

    Private Sub chkPosterMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPosterMod.Click
        CheckEnable()
    End Sub

    Private Sub chkProducers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProducers.CheckedChanged
        CustomUpdater.Options.bProducers = chkProducers.Checked
        CheckEnable()
    End Sub

    Private Sub chkRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRating.CheckedChanged
        CustomUpdater.Options.bRating = chkRating.Checked
        CheckEnable()
    End Sub

    Private Sub chkRelease_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRelease.CheckedChanged
        CustomUpdater.Options.bRelease = chkRelease.Checked
        CheckEnable()
    End Sub

    Private Sub chkRuntime_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRuntime.CheckedChanged
        CustomUpdater.Options.bRuntime = chkRuntime.Checked
        CheckEnable()
    End Sub

    Private Sub chkStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStudio.CheckedChanged
        CustomUpdater.Options.bStudio = chkStudio.Checked
        CheckEnable()
    End Sub

    Private Sub chkTagline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTagline.CheckedChanged
        CustomUpdater.Options.bTagline = chkTagline.Checked
        CheckEnable()
    End Sub

    Private Sub chkTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTitle.CheckedChanged
        CustomUpdater.Options.bTitle = chkTitle.Checked
        CheckEnable()
    End Sub

    Private Sub chkTop250_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTop250.CheckedChanged
        CustomUpdater.Options.bTop250 = chkTop250.Checked
        CheckEnable()
    End Sub

    Private Sub chkCountry_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCountry.CheckedChanged
        CustomUpdater.Options.bCountry = chkCountry.Checked
        CheckEnable()
    End Sub

    Private Sub chkTrailerMod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkTrailerMod.Click
        CheckEnable()
    End Sub

    Private Sub chkTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailer.CheckedChanged
        CustomUpdater.Options.bTrailer = chkTrailer.Checked
        CheckEnable()
    End Sub

    Private Sub chkVotes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkVotes.CheckedChanged
        CustomUpdater.Options.bVotes = chkVotes.Checked
        CheckEnable()
    End Sub

    Private Sub chkWriters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWriters.CheckedChanged
        CustomUpdater.Options.bWriters = chkWriters.Checked
        CheckEnable()
    End Sub

    Private Sub chkYear_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkYear.CheckedChanged
        CustomUpdater.Options.bYear = chkYear.Checked
        CheckEnable()
    End Sub

    Private Sub dlgUpdateMedia_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.SetUp()

            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Using g As Graphics = Graphics.FromImage(iBackground)
                g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
                Me.pnlTop.BackgroundImage = iBackground
            End Using

            'disable options that are locked
            Me.chkPlot.Enabled = Not Master.eSettings.LockPlot
            Me.chkPlot.Checked = Not Master.eSettings.LockPlot
            Me.chkOutline.Enabled = Not Master.eSettings.LockOutline
            Me.chkOutline.Checked = Not Master.eSettings.LockOutline
            Me.chkTitle.Enabled = Not Master.eSettings.LockTitle
            Me.chkTitle.Checked = Not Master.eSettings.LockTitle
            Me.chkTagline.Enabled = Not Master.eSettings.LockTagline
            Me.chkTagline.Checked = Not Master.eSettings.LockTagline
            Me.chkRating.Enabled = Not Master.eSettings.LockRating
            Me.chkRating.Checked = Not Master.eSettings.LockRating
            Me.chkStudio.Enabled = Not Master.eSettings.LockStudio
            Me.chkStudio.Checked = Not Master.eSettings.LockStudio
            Me.chkGenre.Enabled = Not Master.eSettings.LockGenre
            Me.chkGenre.Checked = Not Master.eSettings.LockGenre
            Me.chkTrailer.Enabled = Not Master.eSettings.LockTrailer
            Me.chkTrailer.Checked = Not Master.eSettings.LockTrailer

            'set defaults
            CustomUpdater.ScrapeType = Enums.ScrapeType.FullAuto
            Functions.SetScraperMod(Enums.ModType.All, True)

            Me.CheckEnable()

            'check if there are new or marked movies
            Me.CheckNewAndMark()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dlgUpdateMedia_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub rbUpdateModifier_All_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdateModifier_All.CheckedChanged
        If Me.rbUpdate_Auto.Checked Then
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.FullAuto
        Else
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.FullAsk
        End If
    End Sub

    Private Sub rbUpdateModifier_Marked_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdateModifier_Marked.CheckedChanged
        If Me.rbUpdate_Auto.Checked Then
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.MarkAuto
        Else
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.MarkAsk
        End If
    End Sub

    Private Sub rbUpdateModifier_Missing_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdateModifier_Missing.CheckedChanged
        If Me.rbUpdateModifier_Missing.Checked Then
            Me.chkMetaMod.Checked = False
            Me.chkMetaMod.Enabled = False
        End If

        If Me.rbUpdate_Auto.Checked Then
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.UpdateAuto
        Else
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.UpdateAsk
        End If

        Me.CheckEnable()
    End Sub

    Private Sub rbUpdateModifier_New_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdateModifier_New.CheckedChanged
        If Me.rbUpdate_Auto.Checked Then
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.NewAuto
        Else
            Me.CustomUpdater.ScrapeType = Enums.ScrapeType.NewAsk
        End If
    End Sub

    Private Sub rbUpdate_Ask_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdate_Ask.CheckedChanged
        Select Case True
            Case Me.rbUpdateModifier_All.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.FullAsk
            Case Me.rbUpdateModifier_Missing.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.UpdateAsk
            Case Me.rbUpdateModifier_New.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.NewAsk
            Case rbUpdateModifier_Marked.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.MarkAsk
        End Select
    End Sub

    Private Sub rbUpdate_Auto_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbUpdate_Auto.CheckedChanged
        Select Case True
            Case Me.rbUpdateModifier_All.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.FullAuto
            Case Me.rbUpdateModifier_Missing.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.UpdateAuto
            Case Me.rbUpdateModifier_New.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.NewAuto
            Case Me.rbUpdateModifier_Marked.Checked
                Me.CustomUpdater.ScrapeType = Enums.ScrapeType.MarkAuto
        End Select
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(384, "Custom Scraper")
        Me.OK_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label2.Text = Master.eLang.GetString(385, "Create a custom scraper")
        Me.Label4.Text = Me.Text
        Me.rbUpdateModifier_All.Text = Master.eLang.GetString(68, "All Movies")
        Me.gbUpdateModifier.Text = Master.eLang.GetString(386, "Selection Filter")
        Me.rbUpdateModifier_Marked.Text = Master.eLang.GetString(80, "Marked Movies")
        Me.rbUpdateModifier_New.Text = Master.eLang.GetString(79, "New Movies")
        Me.rbUpdateModifier_Missing.Text = Master.eLang.GetString(78, "Movies Missing Items")
        Me.gbUpdateType.Text = Master.eLang.GetString(387, "Update Mode")
        Me.rbUpdate_Ask.Text = Master.eLang.GetString(77, "Ask (Require Input If No Exact Match)")
        Me.rbUpdate_Auto.Text = Master.eLang.GetString(69, "Automatic (Force Best Match)")
        Me.gbUpdateItems.Text = Master.eLang.GetString(388, "Modifiers")
        Me.chkMetaMod.Text = Master.eLang.GetString(59, "Meta Data")
        Me.chkTrailerMod.Text = Master.eLang.GetString(151, "Trailer")
        Me.chkExtraMod.Text = Master.eLang.GetString(153, "Extrathumbs")
        Me.chkFanartMod.Text = Master.eLang.GetString(149, "Fanart")
        Me.chkPosterMod.Text = Master.eLang.GetString(148, "Poster")
        Me.chkNFOMod.Text = Master.eLang.GetString(150, "NFO")
        Me.chkAllMod.Text = Master.eLang.GetString(70, "All Items")
        Me.Update_Button.Text = Master.eLang.GetString(389, "Begin")
        Me.gbOptions.Text = Master.eLang.GetString(390, "Options")
        Me.chkCrew.Text = Master.eLang.GetString(391, "Other Crew")
        Me.chkMusicBy.Text = Master.eLang.GetString(392, "Music By")
        Me.chkProducers.Text = Master.eLang.GetString(393, "Producers")
        Me.chkWriters.Text = Master.eLang.GetString(394, "Writers")
        Me.chkStudio.Text = Master.eLang.GetString(395, "Studio")
        Me.chkRuntime.Text = Master.eLang.GetString(396, "Runtime")
        Me.chkPlot.Text = Master.eLang.GetString(65, "Plot")
        Me.chkOutline.Text = Master.eLang.GetString(64, "Plot Outline")
        Me.chkGenre.Text = Master.eLang.GetString(20, "Genres")
        Me.chkDirector.Text = Master.eLang.GetString(62, "Director")
        Me.chkTagline.Text = Master.eLang.GetString(397, "Tagline")
        Me.chkCast.Text = Master.eLang.GetString(398, "Cast")
        Me.chkVotes.Text = Master.eLang.GetString(399, "Votes")
        Me.chkTrailer.Text = Master.eLang.GetString(151, "Trailer")
        Me.chkRating.Text = Master.eLang.GetString(400, "Rating")
        Me.chkRelease.Text = Master.eLang.GetString(57, "Release Date")
        Me.chkMPAA.Text = Master.eLang.GetString(401, "MPAA")
        Me.chkCert.Text = Master.eLang.GetString(722, "Certification")
        Me.chkYear.Text = Master.eLang.GetString(278, "Year")
        Me.chkTitle.Text = Master.eLang.GetString(21, "Title")
        Me.chkTop250.Text = Master.eLang.GetString(591, "Top 250")
        Me.chkCountry.Text = Master.eLang.GetString(301, "Country")
    End Sub

    Private Sub Update_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Update_Button.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

#End Region 'Methods

End Class