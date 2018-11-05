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

Public Class frmMediaSettingsHolder

#Region "Events"

    Public Event ModuleSettingsChanged()

    Public Event SetupPostScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)

#End Region 'Events

#Region "Methods"

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).PostScraperOrder
        If order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).Count - 1 Then
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.PostScraperOrder = order + 1).PostScraperOrder = order
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).PostScraperOrder = order + 1
            RaiseEvent SetupPostScraperChanged(cbEnabled.Checked, 1)
            orderChanged()
        End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).PostScraperOrder
        If order > 0 Then
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.PostScraperOrder = order - 1).PostScraperOrder = order
            ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).PostScraperOrder = order - 1
            RaiseEvent SetupPostScraperChanged(cbEnabled.Checked, -1)
            orderChanged()
        End If
    End Sub

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEnabled.CheckedChanged
        RaiseEvent SetupPostScraperChanged(cbEnabled.Checked, 0)
    End Sub

    Sub CheckTrailer()
        Me.txtTimeout.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkTrailerIMDB.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkTrailerTMDB.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkTrailerTMDBXBMC.Enabled = Me.chkDownloadTrailer.Checked
        If Not Me.chkDownloadTrailer.Checked Then
            Me.txtTimeout.Text = "2"
            Me.chkTrailerTMDB.Checked = False
            Me.chkTrailerIMDB.Checked = False
            Me.chkTrailerTMDBXBMC.Checked = False
        End If
    End Sub

    Private Sub chkDownloadTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDownloadTrailer.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
        CheckTrailer()
    End Sub

    Private Sub chkTrailerTMDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailerTMDB.CheckedChanged
        chkTrailerTMDBXBMC.Enabled = chkTrailerTMDB.Checked
        cbTrailerTMDBPref.Enabled = chkTrailerTMDB.Checked
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTrailerIMDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailerIMDB.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkTrailerTMDBXBMC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailerTMDBXBMC.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkScrapeFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapeFanart.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
        grpSaveFanart.Enabled = chkScrapeFanart.Checked
    End Sub

    Private Sub chkScrapePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScrapePoster.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkUseIMPA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseIMPA.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkUseMPDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseMPDB.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkUseTMDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseTMDB.CheckedChanged
        cbManualETSize.Enabled = chkUseTMDB.Checked
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Sub orderChanged()
        Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberNativeScraperModule._AssemblyName).PostScraperOrder
        btnDown.Enabled = (order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).Count - 1)
        btnUp.Enabled = (order > 1)
    End Sub

    Sub SetUp()
        Me.txtTimeout.Text = Master.eSettings.TrailerTimeout.ToString
        Me.Label23.Text = Master.eLang.GetString(7, "Timeout:")
        Me.GroupBox2.Text = Master.eLang.GetString(8, "Supported Sites:")
        Me.GroupBox9.Text = Master.eLang.GetString(9, "Get Images From:")
        Me.grpSaveFanart.Text = Master.eLang.GetString(8001, "Save Fanart In:")
        Me.chkDownloadTrailer.Text = Master.eLang.GetString(529, "Enable Trailer Support", True)
        Me.Label3.Text = Master.eLang.GetString(168, "Scrape Order", True)
        Me.cbEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
        Me.chkScrapePoster.Text = Master.eLang.GetString(101, "Get Posters")
        Me.chkScrapeFanart.Text = Master.eLang.GetString(102, "Get Fanart")
		Me.Label1.Text = String.Format(Master.eLang.GetString(103, "These settings are specific to this module.{0}Please refer to the global settings for more options."), vbCrLf)
		Me.GroupBox3.Text = Master.eLang.GetString(467, "Images", True)
		Me.GroupBox4.Text = Master.eLang.GetString(108, "TMDB Extrathumbs Size:")
		Me.GroupBox1.Text = Master.eLang.GetString(109, "Trailers")
		Me.GroupBox5.Text = Master.eLang.GetString(110, "Youtube/TMDB Trailer:")
		Me.chkTrailerTMDBXBMC.Text = Master.eLang.GetString(111, "XBMC Format")
		Me.Label2.Text = Master.eLang.GetString(112, "Preferred language")
		Me.grpSaveFanart.Text = Master.eLang.GetString(113, "Save Fanart In:")
	End Sub

    Private Sub txtTimeout_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTimeout.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub optFanartFolderExtraFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optFanartFolderExtraFanart.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub optFanartFolderExtraThumbs_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles optFanartFolderExtraThumbs.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub cbManualETSize_SelectedIndexChanged(ByVal sender As System.Object, e As System.EventArgs) Handles cbManualETSize.SelectedIndexChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub cbTrailerTMDBPref_SelectedIndexChanged(ByVal sender As System.Object, e As System.EventArgs) Handles cbTrailerTMDBPref.SelectedIndexChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

#End Region 'Methods

End Class