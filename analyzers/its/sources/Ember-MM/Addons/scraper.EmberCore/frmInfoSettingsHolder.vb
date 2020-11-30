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

Public Class frmInfoSettingsHolder

#Region "Events"

    Public Event ModuleSettingsChanged()

    Public Event SetupScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Methods"

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).ScraperOrder
        If order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsScraper).Count - 1 Then
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.ScraperOrder = order + 1).ScraperOrder = order
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).ScraperOrder = order + 1
            RaiseEvent SetupScraperChanged(cbEnabled.Checked, 1)
            orderChanged()
        End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).ScraperOrder
        If order > 0 Then
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.ScraperOrder = order - 1).ScraperOrder = order
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).ScraperOrder = order - 1
            RaiseEvent SetupScraperChanged(cbEnabled.Checked, -1)
            orderChanged()
        End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEnabled.CheckedChanged
        RaiseEvent SetupScraperChanged(cbEnabled.Checked, 0)
    End Sub

    Private Sub chkCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCast.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkCertification_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCertification.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCrew.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkDirector_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDirector.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkFullCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCast.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkFullCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCrew.CheckedChanged
        RaiseEvent ModuleSettingsChanged()

        Me.chkProducers.Enabled = Me.chkFullCrew.Checked
        Me.chkMusicBy.Enabled = Me.chkFullCrew.Checked
        Me.chkCrew.Enabled = Me.chkFullCrew.Checked

        If Not Me.chkFullCrew.Checked Then
            Me.chkProducers.Checked = False
            Me.chkMusicBy.Checked = False
            Me.chkCrew.Checked = False
        End If
    End Sub

    Private Sub chkGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGenre.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkMPAA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMPAA.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkMusicBy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMusicBy.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOFDBGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOFDBGenre.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOFDBOutline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOFDBOutline.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOFDBPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOFDBPlot.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOFDBTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOFDBTitle.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOutline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOutline.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPlot.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkProducers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProducers.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRating.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkRelease_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRelease.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkRuntime_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRuntime.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStudio.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTagline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTagline.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTitle.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTop250_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTop250.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkCountry_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCountry.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailer.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkVotes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkVotes.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkWriters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWriters.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkYear_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkYear.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Sub orderChanged()
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).ScraperOrder
        btnDown.Enabled = (order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsScraper).Count - 1)
        btnUp.Enabled = (order > 1)
    End Sub

    Private Sub SetUp()
        Me.chkOFDBGenre.Text = Master.eLang.GetString(1, "Use OFDB Genre")
        Me.chkOFDBPlot.Text = Master.eLang.GetString(2, "Use OFDB Plot")
        Me.chkOFDBOutline.Text = Master.eLang.GetString(3, "Use OFDB Outline")
        Me.chkOFDBTitle.Text = Master.eLang.GetString(4, "Use OFDB Title")
        Me.Label18.Text = Master.eLang.GetString(5, "IMDB Mirror:")
        Me.gbOptions.Text = Master.eLang.GetString(6, "Scraper Fields - Scraper specific")
        Me.chkCrew.Text = Master.eLang.GetString(391, "Other Crew", True)
        Me.chkMusicBy.Text = Master.eLang.GetString(392, "Music By", True)
        Me.chkProducers.Text = Master.eLang.GetString(393, "Producers", True)
        Me.chkWriters.Text = Master.eLang.GetString(394, "Writers", True)
        Me.chkStudio.Text = Master.eLang.GetString(395, "Studio", True)
        Me.chkRuntime.Text = Master.eLang.GetString(396, "Runtime", True)
        Me.chkPlot.Text = Master.eLang.GetString(65, "Plot", True)
        Me.chkOutline.Text = Master.eLang.GetString(64, "Plot Outline", True)
        Me.chkGenre.Text = Master.eLang.GetString(20, "Genres", True)
        Me.chkDirector.Text = Master.eLang.GetString(62, "Director", True)
        Me.chkTagline.Text = Master.eLang.GetString(397, "Tagline", True)
        Me.chkCast.Text = Master.eLang.GetString(398, "Cast", True)
        Me.chkVotes.Text = Master.eLang.GetString(399, "Votes", True)
        Me.chkTrailer.Text = Master.eLang.GetString(151, "Trailer", True)
        Me.chkRating.Text = Master.eLang.GetString(400, "Rating", True)
        Me.chkRelease.Text = Master.eLang.GetString(57, "Release Date", True)
        Me.chkMPAA.Text = Master.eLang.GetString(401, "MPAA", True)
        Me.chkYear.Text = Master.eLang.GetString(278, "Year", True)
        Me.chkTitle.Text = Master.eLang.GetString(21, "Title", True)
        Me.chkCertification.Text = Master.eLang.GetString(722, "Certification", True)
        Me.Label2.Text = Master.eLang.GetString(168, "Scrape Order", True)
        Me.cbEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
		Me.Label1.Text = String.Format(Master.eLang.GetString(103, "These settings are specific to this module.{0}Please refer to the global settings for more options."), vbCrLf)
		Me.chkCountry.Text = Master.eLang.GetString(301, "Country", True)
		Me.chkFullCast.Text = Master.eLang.GetString(512, "Scrape Full Cast", True)
		Me.chkFullCrew.Text = Master.eLang.GetString(513, "Scrape Full Crew", True)
		Me.chkTop250.Text = Master.eLang.GetString(868, "Top250", True)
		Me.GroupBox30.Text = Master.eLang.GetString(106, "IMDB")
		Me.GroupBox15.Text = Master.eLang.GetString(107, "OFDB (German)")
    End Sub

    Private Sub txtIMDBURL_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtIMDBURL.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

#End Region 'Methods

End Class