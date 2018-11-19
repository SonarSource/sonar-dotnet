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
' #
' # Dialog size: 898, 656
' # Enlarge it to see all the panels.

Imports System
Imports System.IO
Imports EmberAPI
Imports System.Net

Public Class dlgSettings

#Region "Fields"

    Private currPanel As New Panel
    Private currText As String = String.Empty
    Private dHelp As New Dictionary(Of String, String)
    Private didApply As Boolean = False
    Private Meta As New List(Of Settings.MetadataPerType)
    Private NoUpdate As Boolean = True
    Private SettingsPanels As New List(Of Containers.SettingsPanel)
    Private ShowRegex As New List(Of Settings.TVShowRegEx)
    Private sResult As New Structures.SettingsResult
    Private tLangList As New List(Of Containers.TVLanguage)
    Private TVMeta As New List(Of Settings.MetadataPerType)
    Public Event LoadEnd()

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog() As Structures.SettingsResult
        MyBase.ShowDialog()
        Return Me.sResult
    End Function

    Private Sub AddButtons()
        Dim TSBs As New List(Of ToolStripButton)
        Dim TSB As ToolStripButton
        Dim ButtonsWidth As Integer = 0

        Me.ToolStrip1.Items.Clear()

        'first create all the buttons so we can get their size to calculate the spacer
        TSB = New ToolStripButton With { _
                                .Text = Master.eLang.GetString(390, "Options"), _
                                .Image = My.Resources.General, _
                                .TextImageRelation = TextImageRelation.ImageAboveText, _
                                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, _
                                .Tag = 100}
        AddHandler TSB.Click, AddressOf ToolStripButton_Click
        TSBs.Add(TSB)
        TSB = New ToolStripButton With { _
                                .Text = Master.eLang.GetString(36, "Movies"), _
                                .Image = My.Resources.Movie, _
                                .TextImageRelation = TextImageRelation.ImageAboveText, _
                                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, _
                                .Tag = 200}
        AddHandler TSB.Click, AddressOf ToolStripButton_Click
        TSBs.Add(TSB)
        TSB = New ToolStripButton With { _
                                .Text = Master.eLang.GetString(698, "TV Shows"), _
                                .Image = My.Resources.TVShows, _
                                .TextImageRelation = TextImageRelation.ImageAboveText, _
                                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, _
                                .Tag = 300}
        AddHandler TSB.Click, AddressOf ToolStripButton_Click
        TSBs.Add(TSB)
        TSB = New ToolStripButton With { _
                                .Text = Master.eLang.GetString(802, "Modules"), _
                                .Image = My.Resources.modules, _
                                .TextImageRelation = TextImageRelation.ImageAboveText, _
                                .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, _
                                .Tag = 400}
        AddHandler TSB.Click, AddressOf ToolStripButton_Click
        TSBs.Add(TSB)

        TSB = New ToolStripButton With { _
                        .Text = Master.eLang.GetString(822, "Miscellaneous"), _
                        .Image = My.Resources.Miscellaneous, _
                        .TextImageRelation = TextImageRelation.ImageAboveText, _
                        .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, _
                        .Tag = 400}
        AddHandler TSB.Click, AddressOf ToolStripButton_Click
        TSBs.Add(TSB)

        If TSBs.Count > 0 Then
            Dim spacerMod As Integer = 4

            'calculate the spacer width
            For Each tsbWidth As ToolStripButton In TSBs
                ButtonsWidth += tsbWidth.Width
            Next

            Using g As Graphics = Me.CreateGraphics
                spacerMod = Convert.ToInt32(4 * (g.DpiX / 100))
            End Using

            Dim sSpacer As String = New String(Convert.ToChar(" "), Convert.ToInt32(((Me.ToolStrip1.Width - ButtonsWidth) / (TSBs.Count + 1)) / spacerMod))

            'add it all
            For Each tButton As ToolStripButton In TSBs.OrderBy(Function(b) Convert.ToInt32(b.Tag))
                If sSpacer.Length > 0 Then Me.ToolStrip1.Items.Add(New ToolStripLabel With {.Text = sSpacer})
                Me.ToolStrip1.Items.Add(tButton)
            Next

            'set default page
            Me.currText = TSBs.Item(0).Text
            Me.FillList(currText)
        End If
    End Sub

    Private Sub AddHelpHandlers(ByVal Parent As Control, ByVal Prefix As String)
        Dim pfName As String = String.Empty

        For Each ctrl As Control In Parent.Controls
            If Not TypeOf ctrl Is GroupBox AndAlso Not TypeOf ctrl Is Panel AndAlso Not TypeOf ctrl Is Label AndAlso _
            Not TypeOf ctrl Is TreeView AndAlso Not TypeOf ctrl Is ToolStrip AndAlso Not TypeOf ctrl Is PictureBox AndAlso _
            Not TypeOf ctrl Is TabControl Then
                pfName = String.Concat(Prefix, ctrl.Name)
                ctrl.AccessibleDescription = pfName
                If dHelp.ContainsKey(pfName) Then
                    dHelp.Item(pfName) = Master.eLang.GetHelpString(pfName)
                Else
                    AddHandler ctrl.MouseEnter, AddressOf HelpMouseEnter
                    AddHandler ctrl.MouseLeave, AddressOf HelpMouseLeave
                    dHelp.Add(pfName, Master.eLang.GetHelpString(pfName))
                End If
            End If
            If ctrl.HasChildren Then
                AddHelpHandlers(ctrl, Prefix)
            End If
        Next
    End Sub

    Private Sub AddPanels()
        Me.SettingsPanels.Clear()

        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlMovies", _
                      .Text = Master.eLang.GetString(38, "General"), _
                      .ImageIndex = 2, _
                      .Type = Master.eLang.GetString(36, "Movies"), _
                      .Panel = Me.pnlMovies, _
                      .Order = 100})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlSources", _
                      .Text = Master.eLang.GetString(555, "Files and Sources"), _
                      .ImageIndex = 5, _
                      .Type = Master.eLang.GetString(36, "Movies"), _
                      .Panel = Me.pnlSources, _
                      .Order = 200})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlMovieData", _
                      .Text = Master.eLang.GetString(556, "Scrapers - Data"), _
                      .ImageIndex = 3, _
                      .Type = Master.eLang.GetString(36, "Movies"), _
                      .Panel = Me.pnlScraper, _
                      .Order = 300})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlMovieMedia", _
                      .Text = Master.eLang.GetString(557, "Scrapers - Images & Trailers"), _
                      .ImageIndex = 6, _
                      .Type = Master.eLang.GetString(36, "Movies"), _
                      .Panel = Me.pnlImages, _
                      .Order = 400})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlShows", _
                      .Text = Master.eLang.GetString(38, "General"), _
                      .ImageIndex = 7, _
                      .Type = Master.eLang.GetString(698, "TV Shows"), _
                      .Panel = Me.pnlShows, _
                      .Order = 100})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlTVSources", _
                      .Text = Master.eLang.GetString(555, "Files and Sources"), _
                      .ImageIndex = 5, _
                      .Type = Master.eLang.GetString(698, "TV Shows"), _
                      .Panel = Me.pnlTVSources, _
                      .Order = 200})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlTVData", _
                      .Text = Master.eLang.GetString(556, "Scrapers - Data"), _
                      .ImageIndex = 3, _
                      .Type = Master.eLang.GetString(698, "TV Shows"), _
                      .Panel = Me.pnlTVScraper, _
                      .Order = 300})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlTVMedia", _
                      .Text = Master.eLang.GetString(837, "Scrapers - Images"), _
                      .ImageIndex = 6, _
                      .Type = Master.eLang.GetString(698, "TV Shows"), _
                      .Panel = Me.pnlTVImages, _
                      .Order = 400})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlGeneral", _
                      .Text = Master.eLang.GetString(38, "General"), _
                      .ImageIndex = 0, _
                      .Type = Master.eLang.GetString(390, "Options"), _
                      .Panel = Me.pnlGeneral, _
                      .Order = 100})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlExtensions", _
                      .Text = Master.eLang.GetString(553, "File System"), _
                      .ImageIndex = 4, _
                      .Type = Master.eLang.GetString(390, "Options"), _
                      .Panel = Me.pnlExtensions, _
                      .Order = 200})
        Me.SettingsPanels.Add(New Containers.SettingsPanel With { _
                      .Name = "pnlXBMCCom", _
                      .Text = Master.eLang.GetString(421, "Connection"), _
                      .ImageIndex = 1, _
                      .Type = Master.eLang.GetString(390, "Options"), _
                      .Panel = Me.pnlXBMCCom, _
                      .Order = 300})
        AddScraperPanels()
    End Sub

    Sub AddScraperPanels()
        Dim ModuleCounter As Integer = 1
        Dim tPanel As New Containers.SettingsPanel
        For Each s As ModulesManager._externalScraperModuleClass In ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsScraper).OrderBy(Function(x) x.ScraperOrder)
            tPanel = s.ProcessorModule.InjectSetupScraper
            tPanel.Order += ModuleCounter
            Me.SettingsPanels.Add(tPanel)
            ModuleCounter += 1
            AddHandler s.ProcessorModule.ScraperSetupChanged, AddressOf Handle_ModuleSetupChanged
            AddHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            Me.AddHelpHandlers(tPanel.Panel, tPanel.Prefix)
        Next
        ModuleCounter = 1
        For Each s As ModulesManager._externalScraperModuleClass In ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).OrderBy(Function(x) x.PostScraperOrder)
            tPanel = s.ProcessorModule.InjectSetupPostScraper
            tPanel.Order += ModuleCounter
            Me.SettingsPanels.Add(tPanel)
            ModuleCounter += 1
            AddHandler s.ProcessorModule.PostScraperSetupChanged, AddressOf Handle_ModuleSetupChanged
            AddHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            Me.AddHelpHandlers(tPanel.Panel, tPanel.Prefix)
        Next
        ModuleCounter = 1
        For Each s As ModulesManager._externalTVScraperModuleClass In ModulesManager.Instance.externalTVScrapersModules.Where(Function(y) y.ProcessorModule.IsScraper).OrderBy(Function(x) x.ScraperOrder)
            tPanel = s.ProcessorModule.InjectSetupScraper
            tPanel.Order += ModuleCounter
            Me.SettingsPanels.Add(tPanel)
            ModuleCounter += 1
            AddHandler s.ProcessorModule.SetupScraperChanged, AddressOf Handle_ModuleSetupChanged
            AddHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            Me.AddHelpHandlers(tPanel.Panel, tPanel.Prefix)
        Next
        ModuleCounter = 1
        For Each s As ModulesManager._externalTVScraperModuleClass In ModulesManager.Instance.externalTVScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).OrderBy(Function(x) x.PostScraperOrder)
            tPanel = s.ProcessorModule.InjectSetupPostScraper
            tPanel.Order += ModuleCounter
            Me.SettingsPanels.Add(tPanel)
            ModuleCounter += 1
            AddHandler s.ProcessorModule.SetupPostScraperChanged, AddressOf Handle_ModuleSetupChanged
            AddHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            Me.AddHelpHandlers(tPanel.Panel, tPanel.Prefix)
        Next
        ModuleCounter = 1
        For Each s As ModulesManager._externalGenericModuleClass In ModulesManager.Instance.externalProcessorModules
            tPanel = s.ProcessorModule.InjectSetup
            If Not tPanel Is Nothing Then
                tPanel.Order += ModuleCounter
                If tPanel.ImageIndex = -1 AndAlso Not tPanel.Image Is Nothing Then
                    ilSettings.Images.Add(String.Concat(s.AssemblyName, tPanel.Name), tPanel.Image)
                    tPanel.ImageIndex = ilSettings.Images.IndexOfKey(String.Concat(s.AssemblyName, tPanel.Name))
                End If
                Me.SettingsPanels.Add(tPanel)
                ModuleCounter += 1
                AddHandler s.ProcessorModule.ModuleSetupChanged, AddressOf Handle_ModuleSetupChanged
                AddHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
                Me.AddHelpHandlers(tPanel.Panel, tPanel.Prefix)
            End If
        Next
    End Sub

    Sub RemoveScraperPanels()
        For Each s As ModulesManager._externalScraperModuleClass In ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsScraper).OrderBy(Function(x) x.ScraperOrder)
            RemoveHandler s.ProcessorModule.ScraperSetupChanged, AddressOf Handle_ModuleSetupChanged
            RemoveHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Next
        For Each s As ModulesManager._externalScraperModuleClass In ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).OrderBy(Function(x) x.PostScraperOrder)
            RemoveHandler s.ProcessorModule.PostScraperSetupChanged, AddressOf Handle_ModuleSetupChanged
            RemoveHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Next
        For Each s As ModulesManager._externalTVScraperModuleClass In ModulesManager.Instance.externalTVScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).OrderBy(Function(x) x.ScraperOrder)
            RemoveHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Next
        For Each s As ModulesManager._externalTVScraperModuleClass In ModulesManager.Instance.externalTVScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).OrderBy(Function(x) x.PostScraperOrder)
            RemoveHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Next
        For Each s As ModulesManager._externalGenericModuleClass In ModulesManager.Instance.externalProcessorModules
            RemoveHandler s.ProcessorModule.ModuleSetupChanged, AddressOf Handle_ModuleSetupChanged
            RemoveHandler s.ProcessorModule.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Next
    End Sub

    Private Sub btnAddEpFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddEpFilter.Click
        If Not String.IsNullOrEmpty(Me.txtEpFilter.Text) Then
            Me.lstEpFilters.Items.Add(Me.txtEpFilter.Text)
            Me.txtEpFilter.Text = String.Empty
            Me.SetApplyButton(True)
            Me.sResult.NeedsUpdate = True
        End If

        Me.txtEpFilter.Focus()
    End Sub

    Private Sub btnAddFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddFilter.Click
        If Not String.IsNullOrEmpty(Me.txtFilter.Text) Then
            Me.lstFilters.Items.Add(Me.txtFilter.Text)
            Me.txtFilter.Text = String.Empty
            Me.SetApplyButton(True)
            Me.sResult.NeedsUpdate = True
        End If

        Me.txtFilter.Focus()
    End Sub

    Private Sub btnAddMovieExt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddMovieExt.Click
        If Not String.IsNullOrEmpty(txtMovieExt.Text) Then
            If Not Strings.Left(txtMovieExt.Text, 1) = "." Then txtMovieExt.Text = String.Concat(".", txtMovieExt.Text)
            If Not lstMovieExts.Items.Contains(txtMovieExt.Text.ToLower) Then
                lstMovieExts.Items.Add(txtMovieExt.Text.ToLower)
                Me.SetApplyButton(True)
                Me.sResult.NeedsUpdate = True
                txtMovieExt.Text = String.Empty
                txtMovieExt.Focus()
            End If
        End If
    End Sub

    Private Sub btnAddNoStack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddNoStack.Click
        If Not String.IsNullOrEmpty(txtNoStack.Text) Then
            If Not Strings.Left(txtNoStack.Text, 1) = "." Then txtNoStack.Text = String.Concat(".", txtNoStack.Text)
            If Not lstNoStack.Items.Contains(txtNoStack.Text) Then
                lstNoStack.Items.Add(txtNoStack.Text)
                Me.SetApplyButton(True)
                Me.sResult.NeedsUpdate = True
                txtNoStack.Text = String.Empty
                txtNoStack.Focus()
            End If
        End If
    End Sub

    Private Sub btnAddShowFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddShowFilter.Click
        If Not String.IsNullOrEmpty(Me.txtShowFilter.Text) Then
            Me.lstShowFilters.Items.Add(Me.txtShowFilter.Text)
            Me.txtShowFilter.Text = String.Empty
            Me.SetApplyButton(True)
            Me.sResult.NeedsUpdate = True
        End If

        Me.txtShowFilter.Focus()
    End Sub

    Private Sub btnAddShowRegex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddShowRegex.Click
        If String.IsNullOrEmpty(Me.btnAddShowRegex.Tag.ToString) Then
            Dim lID = (From lRegex As Settings.TVShowRegEx In Me.ShowRegex Select lRegex.ID).Max
            Me.ShowRegex.Add(New Settings.TVShowRegEx With {.ID = Convert.ToInt32(lID) + 1, .SeasonRegex = Me.txtSeasonRegex.Text, .SeasonFromDirectory = Not Convert.ToBoolean(Me.cboSeasonRetrieve.SelectedIndex), .EpisodeRegex = Me.txtEpRegex.Text, .EpisodeRetrieve = DirectCast(Me.cboEpRetrieve.SelectedIndex, Settings.EpRetrieve)})
        Else
            Dim selRex = From lRegex As Settings.TVShowRegEx In Me.ShowRegex Where lRegex.ID = Convert.ToInt32(Me.btnAddShowRegex.Tag)
            If selRex.Count > 0 Then
                selRex(0).SeasonRegex = Me.txtSeasonRegex.Text
                selRex(0).SeasonFromDirectory = Not Convert.ToBoolean(Me.cboSeasonRetrieve.SelectedIndex)
                selRex(0).EpisodeRegex = Me.txtEpRegex.Text
                selRex(0).EpisodeRetrieve = DirectCast(Me.cboEpRetrieve.SelectedIndex, Settings.EpRetrieve)
            End If
        End If

        Me.ClearRegex()
        Me.SetApplyButton(True)
        Me.LoadShowRegex()
    End Sub

    Private Sub btnAddToken_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddToken.Click
        If Not String.IsNullOrEmpty(txtSortToken.Text) Then
            If Not lstSortTokens.Items.Contains(txtSortToken.Text) Then
                lstSortTokens.Items.Add(txtSortToken.Text)
                Me.sResult.NeedsRefresh = True
                Me.SetApplyButton(True)
                txtSortToken.Text = String.Empty
                txtSortToken.Focus()
            End If
        End If
    End Sub

    Private Sub btnAddTVSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddTVSource.Click
        Using dSource As New dlgTVSource
            If dSource.ShowDialog = DialogResult.OK Then
                RefreshTVSources()
                Me.SetApplyButton(True)
                Me.sResult.NeedsUpdate = True
            End If
        End Using
    End Sub

    Private Sub btnAddWhitelist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddWhitelist.Click
        If Not String.IsNullOrEmpty(Me.txtWhitelist.Text) Then
            If Not Strings.Left(txtWhitelist.Text, 1) = "." Then txtWhitelist.Text = String.Concat(".", txtWhitelist.Text)
            If Not lstWhitelist.Items.Contains(txtWhitelist.Text.ToLower) Then
                lstWhitelist.Items.Add(txtWhitelist.Text.ToLower)
                Me.SetApplyButton(True)
                txtWhitelist.Text = String.Empty
                txtWhitelist.Focus()
            End If
        End If
    End Sub

    Private Sub btnApply_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnApply.Click
        Try
            Me.SaveSettings(True)
            Me.SetApplyButton(False)
            If Me.sResult.NeedsUpdate OrElse Me.sResult.NeedsRefresh Then Me.didApply = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        With Me.fbdBrowse
            If .ShowDialog = DialogResult.OK Then
                If Not String.IsNullOrEmpty(.SelectedPath.ToString) AndAlso Directory.Exists(.SelectedPath) Then
                    Me.txtBDPath.Text = .SelectedPath.ToString
                End If
            End If
        End With
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If Not didApply Then sResult.DidCancel = True
        RemoveScraperPanels()
        Me.Close()
    End Sub

    Private Sub btnClearRegex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearRegex.Click
        Me.ClearRegex()
    End Sub

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Try
            If Me.lstFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstFilters.SelectedItem) AndAlso Me.lstFilters.SelectedIndex < (Me.lstFilters.Items.Count - 1) Then
                Dim iIndex As Integer = Me.lstFilters.SelectedIndices(0)
                Me.lstFilters.Items.Insert(iIndex + 2, Me.lstFilters.SelectedItems(0))
                Me.lstFilters.Items.RemoveAt(iIndex)
                Me.lstFilters.SelectedIndex = iIndex + 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnEditMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditMetaDataFT.Click
        Using dEditMeta As New dlgFileInfo
            Dim fi As New MediaInfo.Fileinfo
            For Each x As Settings.MetadataPerType In Meta
                If x.FileType = lstMetaData.SelectedItems(0).ToString Then
                    fi = dEditMeta.ShowDialog(x.MetaData, False)
                    If Not fi Is Nothing Then
                        Meta.Remove(x)
                        Dim m As New Settings.MetadataPerType
                        m.FileType = x.FileType
                        m.MetaData = New MediaInfo.Fileinfo
                        m.MetaData = fi
                        Meta.Add(m)
                        LoadMetadata()
                        Me.SetApplyButton(True)
                    End If
                    Exit For
                End If
            Next
        End Using
    End Sub

    Private Sub btnEditShowRegex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditShowRegex.Click
        If Me.lvShowRegex.SelectedItems.Count > 0 Then Me.EditShowRegex(lvShowRegex.SelectedItems(0))
    End Sub

    Private Sub btnEditSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditSource.Click
        If lvMovies.SelectedItems.Count > 0 Then
            Using dMovieSource As New dlgMovieSource
                If dMovieSource.ShowDialog(Convert.ToInt32(lvMovies.SelectedItems(0).Text)) = DialogResult.OK Then
                    Me.RefreshSources()
                    Me.sResult.NeedsUpdate = True
                    Me.SetApplyButton(True)
                End If
            End Using
        End If
    End Sub

    Private Sub btnEditTVMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditTVMetaDataFT.Click
        Using dEditMeta As New dlgFileInfo
            Dim fi As New MediaInfo.Fileinfo
            For Each x As Settings.MetadataPerType In TVMeta
                If x.FileType = lstTVMetaData.SelectedItems(0).ToString Then
                    fi = dEditMeta.ShowDialog(x.MetaData, True)
                    If Not fi Is Nothing Then
                        TVMeta.Remove(x)
                        Dim m As New Settings.MetadataPerType
                        m.FileType = x.FileType
                        m.MetaData = New MediaInfo.Fileinfo
                        m.MetaData = fi
                        TVMeta.Add(m)
                        LoadTVMetadata()
                        Me.SetApplyButton(True)
                    End If
                    Exit For
                End If
            Next
        End Using
    End Sub

    Private Sub btnEditTVSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditTVSource.Click
        If lvTVSources.SelectedItems.Count > 0 Then
            Using dTVSource As New dlgTVSource
                If dTVSource.ShowDialog(Convert.ToInt32(lvTVSources.SelectedItems(0).Text)) = DialogResult.OK Then
                    Me.RefreshTVSources()
                    Me.sResult.NeedsUpdate = True
                    Me.SetApplyButton(True)
                End If
            End Using
        End If
    End Sub

    Private Sub btnEpFilterDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEpFilterDown.Click
        Try
            If Me.lstEpFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstEpFilters.SelectedItem) AndAlso Me.lstEpFilters.SelectedIndex < (Me.lstEpFilters.Items.Count - 1) Then
                Dim iIndex As Integer = Me.lstEpFilters.SelectedIndices(0)
                Me.lstEpFilters.Items.Insert(iIndex + 2, Me.lstEpFilters.SelectedItems(0))
                Me.lstEpFilters.Items.RemoveAt(iIndex)
                Me.lstEpFilters.SelectedIndex = iIndex + 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstEpFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnEpFilterUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEpFilterUp.Click
        Try
            If Me.lstEpFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstEpFilters.SelectedItem) AndAlso Me.lstEpFilters.SelectedIndex > 0 Then
                Dim iIndex As Integer = Me.lstEpFilters.SelectedIndices(0)
                Me.lstEpFilters.Items.Insert(iIndex - 1, Me.lstEpFilters.SelectedItems(0))
                Me.lstEpFilters.Items.RemoveAt(iIndex + 1)
                Me.lstEpFilters.SelectedIndex = iIndex - 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstEpFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnMovieAddFolders_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMovieAddFolder.Click
        Using dSource As New dlgMovieSource
            If dSource.ShowDialog = DialogResult.OK Then
                RefreshSources()
                Me.SetApplyButton(True)
                Me.sResult.NeedsUpdate = True
            End If
        End Using
    End Sub

    Private Sub btnMovieRem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMovieRem.Click
        Me.RemoveMovieSource()
        Master.DB.LoadMovieSourcesFromDB()
    End Sub

    Private Sub btnNewMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewMetaDataFT.Click
        If Not txtDefFIExt.Text.StartsWith(".") Then txtDefFIExt.Text = String.Concat(".", txtDefFIExt.Text)
        Using dEditMeta As New dlgFileInfo
            Dim fi As New MediaInfo.Fileinfo
            fi = dEditMeta.ShowDialog(fi, False)
            If Not fi Is Nothing Then
                Dim m As New Settings.MetadataPerType
                m.FileType = txtDefFIExt.Text
                m.MetaData = New MediaInfo.Fileinfo
                m.MetaData = fi
                Meta.Add(m)
                LoadMetadata()
                Me.SetApplyButton(True)
                Me.txtDefFIExt.Text = String.Empty
                Me.txtDefFIExt.Focus()
            End If
        End Using
    End Sub

    Private Sub btnNewTVMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewTVMetaDataFT.Click
        If Not txtTVDefFIExt.Text.StartsWith(".") Then txtTVDefFIExt.Text = String.Concat(".", txtTVDefFIExt.Text)
        Using dEditMeta As New dlgFileInfo
            Dim fi As New MediaInfo.Fileinfo
            fi = dEditMeta.ShowDialog(fi, True)
            If Not fi Is Nothing Then
                Dim m As New Settings.MetadataPerType
                m.FileType = txtTVDefFIExt.Text
                m.MetaData = New MediaInfo.Fileinfo
                m.MetaData = fi
                TVMeta.Add(m)
                LoadTVMetadata()
                Me.SetApplyButton(True)
                Me.txtTVDefFIExt.Text = String.Empty
                Me.txtTVDefFIExt.Focus()
            End If
        End Using
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        NoUpdate = True
        Me.SaveSettings(False)
        RemoveScraperPanels()
        Me.Close()
    End Sub

    Private Sub btnRegexUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegexUp.Click
        Try
            If Me.lvShowRegex.Items.Count > 0 AndAlso Me.lvShowRegex.SelectedItems.Count > 0 AndAlso Not Me.lvShowRegex.SelectedItems(0).Index = 0 Then
                Dim selItem As Settings.TVShowRegEx = Me.ShowRegex.FirstOrDefault(Function(r) r.ID = Convert.ToInt32(Me.lvShowRegex.SelectedItems(0).Text))

                If Not IsNothing(selItem) Then
                    Me.lvShowRegex.SuspendLayout()
                    Dim iIndex As Integer = Me.ShowRegex.IndexOf(selItem)
                    Dim selIndex As Integer = Me.lvShowRegex.SelectedIndices(0)
                    Me.ShowRegex.Remove(selItem)
                    Me.ShowRegex.Insert(iIndex - 1, selItem)

                    Me.RenumberRegex()
                    Me.LoadShowRegex()

                    Me.lvShowRegex.Items(selIndex - 1).Selected = True
                    Me.lvShowRegex.ResumeLayout()
                End If

                Me.SetApplyButton(True)
                Me.lvShowRegex.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnRegexDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRegexDown.Click
        Try
            If Me.lvShowRegex.Items.Count > 0 AndAlso Me.lvShowRegex.SelectedItems.Count > 0 AndAlso Me.lvShowRegex.SelectedItems(0).Index < (Me.lvShowRegex.Items.Count - 1) Then
                Dim selItem As Settings.TVShowRegEx = Me.ShowRegex.FirstOrDefault(Function(r) r.ID = Convert.ToInt32(Me.lvShowRegex.SelectedItems(0).Text))

                If Not IsNothing(selItem) Then
                    Me.lvShowRegex.SuspendLayout()
                    Dim iIndex As Integer = Me.ShowRegex.IndexOf(selItem)
                    Dim selIndex As Integer = Me.lvShowRegex.SelectedIndices(0)
                    Me.ShowRegex.Remove(selItem)
                    Me.ShowRegex.Insert(iIndex + 1, selItem)

                    Me.RenumberRegex()
                    Me.LoadShowRegex()

                    Me.lvShowRegex.Items(selIndex + 1).Selected = True
                    Me.lvShowRegex.ResumeLayout()
                End If

                Me.SetApplyButton(True)
                Me.lvShowRegex.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnResetShowFilters_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetShowFilters.Click
        If MsgBox(Master.eLang.GetString(840, "Are you sure you want to reset to the default list of show filters?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
            Master.eSettings.SetDefaultsForLists(Enums.DefaultType.ShowFilters, True)
            Me.RefreshShowFilters()
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub btnResetEpFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetEpFilter.Click
        If MsgBox(Master.eLang.GetString(841, "Are you sure you want to reset to the default list of episode filters?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
            Master.eSettings.SetDefaultsForLists(Enums.DefaultType.EpFilters, True)
            Me.RefreshEpFilters()
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub btnResetMovieFilters_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetMovieFilters.Click
        If MsgBox(Master.eLang.GetString(842, "Are you sure you want to reset to the default list of movie filters?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
            Master.eSettings.SetDefaultsForLists(Enums.DefaultType.MovieFilters, True)
            Me.RefreshMovieFilters()
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub btnResetValidExts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetValidExts.Click
        If MsgBox(Master.eLang.GetString(843, "Are you sure you want to reset to the default list of valid video extensions?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
            Master.eSettings.SetDefaultsForLists(Enums.DefaultType.ValidExts, True)
            Me.RefreshValidExts()
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub btnGetTVProfiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetTVProfiles.Click
        Using dd As New dlgTVRegExProfiles
            If dd.ShowDialog() = DialogResult.OK Then
                Me.ShowRegex.Clear()
                Me.ShowRegex.AddRange(dd.ShowRegex)
                Me.LoadShowRegex()
                Me.SetApplyButton(True)
            End If
        End Using
    End Sub

    Private Sub btnResetShowRegex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetShowRegex.Click
        If MsgBox(Master.eLang.GetString(844, "Are you sure you want to reset to the default list of show regex?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
            Master.eSettings.SetDefaultsForLists(Enums.DefaultType.ShowRegex, True)
            Me.ShowRegex.Clear()
            Me.ShowRegex.AddRange(Master.eSettings.TVShowRegexes)
            Me.LoadShowRegex()
            Me.SetApplyButton(True)
        End If

    End Sub

    Private Sub btnRemMovieExt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemMovieExt.Click
        Me.RemoveMovieExt()
    End Sub

    Private Sub btnRemoveEpFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveEpFilter.Click
        Me.RemoveEpFilter()
    End Sub

    Private Sub btnRemoveFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveFilter.Click
        Me.RemoveFilter()
    End Sub

    Private Sub btnRemoveMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveMetaDataFT.Click
        Me.RemoveMetaData()
    End Sub

    Private Sub btnRemoveNoStack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveNoStack.Click
        Me.RemoveNoStack()
    End Sub

    Private Sub btnRemoveShowFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveShowFilter.Click
        Me.RemoveShowFilter()
    End Sub

    Private Sub btnRemoveShowRegex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveShowRegex.Click
        Me.RemoveRegex()
    End Sub

    Private Sub btnRemoveToken_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveToken.Click
        Me.RemoveSortToken()
    End Sub

    Private Sub btnRemoveTVMetaDataFT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveTVMetaDataFT.Click
        Me.RemoveTVMetaData()
    End Sub

    Private Sub btnRemoveWhitelist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveWhitelist.Click
        If Me.lstWhitelist.Items.Count > 0 AndAlso Me.lstWhitelist.SelectedItems.Count > 0 Then
            While Me.lstWhitelist.SelectedItems.Count > 0
                lstWhitelist.Items.Remove(Me.lstWhitelist.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub btnRemTVSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemTVSource.Click
        Me.RemoveTVSource()
        Master.DB.LoadTVSourcesFromDB()
    End Sub

    Private Sub btnShowFilterDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowFilterDown.Click
        Try
            If Me.lstShowFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstShowFilters.SelectedItem) AndAlso Me.lstShowFilters.SelectedIndex < (Me.lstShowFilters.Items.Count - 1) Then
                Dim iIndex As Integer = Me.lstShowFilters.SelectedIndices(0)
                Me.lstShowFilters.Items.Insert(iIndex + 2, Me.lstShowFilters.SelectedItems(0))
                Me.lstShowFilters.Items.RemoveAt(iIndex)
                Me.lstShowFilters.SelectedIndex = iIndex + 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstShowFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnShowFilterUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowFilterUp.Click
        Try
            If Me.lstShowFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstShowFilters.SelectedItem) AndAlso Me.lstShowFilters.SelectedIndex > 0 Then
                Dim iIndex As Integer = Me.lstShowFilters.SelectedIndices(0)
                Me.lstShowFilters.Items.Insert(iIndex - 1, Me.lstShowFilters.SelectedItems(0))
                Me.lstShowFilters.Items.RemoveAt(iIndex + 1)
                Me.lstShowFilters.SelectedIndex = iIndex - 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstShowFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnTVLanguageFetch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTVLanguageFetch.Click
        Me.tLangList.Clear()
        Me.tLangList.AddRange(ModulesManager.Instance.TVGetLangs(Master.eSettings.TVDBMirror))
        Me.cbTVLanguage.Items.AddRange((From lLang In tLangList Select lLang.LongLang).ToArray)

        If Me.cbTVLanguage.Items.Count > 0 Then
            Me.cbTVLanguage.Text = Me.tLangList.FirstOrDefault(Function(l) l.ShortLang = Master.eSettings.TVDBLanguage).LongLang
        End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Try
            If Me.lstFilters.Items.Count > 0 AndAlso Not IsNothing(Me.lstFilters.SelectedItem) AndAlso Me.lstFilters.SelectedIndex > 0 Then
                Dim iIndex As Integer = Me.lstFilters.SelectedIndices(0)
                Me.lstFilters.Items.Insert(iIndex - 1, Me.lstFilters.SelectedItems(0))
                Me.lstFilters.Items.RemoveAt(iIndex + 1)
                Me.lstFilters.SelectedIndex = iIndex - 1
                Me.SetApplyButton(True)
                Me.sResult.NeedsRefresh = True
                Me.lstFilters.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnMovieFrodo_Click(sender As Object, e As EventArgs) Handles btnMovieFrodo.Click
        Me.chkFanartJPG.Checked = False
        Me.chkFolderJPG.Checked = False
        Me.chkMovieJPG.Checked = False
        Me.chkMovieNameDashPosterJPG.Checked = True
        Me.chkMovieNameDotFanartJPG.Checked = False
        Me.chkMovieNameFanartJPG.Checked = True
        Me.chkMovieNameJPG.Checked = False
        Me.chkMovieNameNFO.Checked = True
        Me.chkMovieNameTBN.Checked = False
        Me.chkMovieNFO.Checked = False
        Me.chkMovieTBN.Checked = False
        Me.chkPosterJPG.Checked = False
        Me.chkPosterTBN.Checked = False
        Me.rbDashTrailer.Checked = True
    End Sub

    Private Sub btnTVShowFrodo_Click(sender As Object, e As EventArgs) Handles btnTVShowFrodo.Click
        Me.chkEpisodeDashFanart.Checked = False
        Me.chkEpisodeDashThumbJPG.Checked = True
        Me.chkEpisodeDotFanart.Checked = False
        Me.chkEpisodeJPG.Checked = False
        Me.chkEpisodeTBN.Checked = False
        Me.chkSeasonAllJPG.Checked = False
        Me.chkSeasonAllPosterJPG.Checked = True
        Me.chkSeasonAllTBN.Checked = False
        Me.chkSeasonDashFanart.Checked = False
        Me.chkSeasonDotFanart.Checked = False
        Me.chkSeasonFanartJPG.Checked = False
        Me.chkSeasonFolderJPG.Checked = False
        Me.chkSeasonNameJPG.Checked = False
        Me.chkSeasonNameTBN.Checked = False
        Me.chkSeasonPosterJPG.Checked = False
        Me.chkSeasonPosterTBN.Checked = False
        Me.chkSeasonXTBN.Checked = False
        Me.chkSeasonXXDashFanartJPG.Checked = True
        Me.chkSeasonXXDashPosterJPG.Checked = True
        Me.chkSeasonXXTBN.Checked = False
        'Me.chkShowBannerJPG.Checked = True (banners not implemented at time)
        Me.chkShowDashFanart.Checked = False
        Me.chkShowDotFanart.Checked = False
        Me.chkShowFanartJPG.Checked = True
        Me.chkShowFolderJPG.Checked = False
        Me.chkShowJPG.Checked = False
        Me.chkShowPosterJPG.Checked = True
        Me.chkShowPosterTBN.Checked = False
        Me.chkShowTBN.Checked = False
    End Sub

    Private Sub cbAutoETSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbAutoETSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbCert_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbCert.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbFanartSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFanartSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbForce_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbForce.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbIntLang_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbIntLang.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbLanguages_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbLanguages.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbMovieTheme_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbMovieTheme.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbOrdering_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbOrdering.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cboEpRetrieve_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboEpRetrieve.SelectedIndexChanged
        Me.ValidateRegex()
    End Sub

    Private Sub cboSeasonRetrieve_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboSeasonRetrieve.SelectedIndexChanged
        Me.ValidateRegex()
    End Sub

    Private Sub cboTVMetaDataOverlay_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboTVMetaDataOverlay.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cboTVUpdate_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboTVUpdate.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbPosterSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbPosterSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbRatingRegion_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbRatingRegion.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbSeaFanartSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbSeaFanartSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbSeaPosterSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbSeaPosterSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbShowFanartSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbShowFanartSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbTrailerQuality_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTrailerQuality.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbTVEpTheme_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEpTheme.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbTVLanguage_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTVLanguage.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub cbTVShowTheme_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTVShowTheme.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chAllSPosterSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbAllSPosterSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chEpFanartSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEpFanartSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub
    Private Sub chkClickScrape_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkClickScrape.CheckedChanged
        chkAskCheckboxScrape.Enabled = chkClickScrape.Checked
        Me.SetApplyButton(True)

    End Sub
    Private Sub chkAskCheckboxScrape_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAskCheckboxScrape.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkAutoBD_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoBD.CheckedChanged
        Me.SetApplyButton(True)
        Me.txtBDPath.Enabled = chkAutoBD.Checked
        Me.btnBrowse.Enabled = chkAutoBD.Checked

    End Sub

    Private Sub chkAutoDetectVTS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoDetectVTS.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkAutoETSize_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoETSize.CheckedChanged
        Me.SetApplyButton(True)
        Me.cbAutoETSize.Enabled = Me.chkAutoETSize.Checked
    End Sub

    Private Sub chkAutoThumbs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoThumbs.CheckedChanged
        Me.txtAutoThumbs.Enabled = Me.chkAutoThumbs.Checked
        Me.chkNoSpoilers.Enabled = Me.chkAutoThumbs.Checked
        Me.chkUseETasFA.Enabled = Me.chkAutoThumbs.Checked
        If Not chkAutoThumbs.Checked Then
            Me.txtAutoThumbs.Text = String.Empty
            Me.chkNoSpoilers.Checked = False
            Me.chkUseETasFA.Checked = False
        End If
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCastWithImg_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCastWithImg.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCast.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkFullCast.Enabled = Me.chkCast.Checked
        Me.chkCastWithImg.Enabled = Me.chkCast.Checked
        Me.txtActorLimit.Enabled = Me.chkCast.Checked

        If Not chkCast.Checked Then
            Me.chkFullCast.Checked = False
            Me.chkCastWithImg.Checked = False
            Me.txtActorLimit.Text = "0"
        End If
    End Sub

    Private Sub chkCertification_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCertification.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCert_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCert.CheckedChanged
        Me.cbCert.SelectedIndex = -1
        Me.cbCert.Enabled = Me.chkCert.Checked
        Me.chkUseCertForMPAA.Enabled = Me.chkCert.Checked
        If Not Me.chkCert.Checked Then
            Me.chkUseCertForMPAA.Checked = False
            Me.chkOnlyValueForCert.Checked = False
            Me.chkOnlyValueForCert.Enabled = False
        End If
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCheckTitles_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCheckTitles.CheckedChanged
        Me.SetApplyButton(True)
        Me.txtCheckTitleTol.Enabled = Me.chkCheckTitles.Checked
        If Not Me.chkCheckTitles.Checked Then Me.txtCheckTitleTol.Text = String.Empty
    End Sub

    Private Sub chkCleanDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanDB.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanDotFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanDotFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanExtrathumbs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanExtrathumbs.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanFolderJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanFolderJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieNameJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieNameJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieNFOb_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieNFOb.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieNFO_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieNFO.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieTBNb_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieTBNb.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanMovieTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCleanPosterTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanPosterTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCrew.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDeleteAllTrailers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDeleteAllTrailers.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDirector_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDirector.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDisplayAllSeason_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDisplayAllSeason.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDisplayMissingEpisodes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDisplayMissingEpisodes.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDisplayYear_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDisplayYear.CheckedChanged
        Me.sResult.NeedsRefresh = True
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkDownloadTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDownloadTrailer.CheckedChanged
        Me.SetApplyButton(True)
        Me.chkUpdaterTrailer.Enabled = Me.chkDownloadTrailer.Checked

        Me.chkSingleScrapeTrailer.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkOverwriteTrailer.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkNoDLTrailer.Enabled = Me.chkDownloadTrailer.Checked
        Me.chkDeleteAllTrailers.Enabled = Me.chkDownloadTrailer.Checked

        If Not Me.chkDownloadTrailer.Checked Then
            Me.chkUpdaterTrailer.Checked = False
            Me.chkSingleScrapeTrailer.Checked = False
            Me.chkNoDLTrailer.Checked = False
            Me.chkOverwriteTrailer.Checked = False
            Me.chkDeleteAllTrailers.Checked = False
            Me.cbTrailerQuality.Enabled = False
            Me.cbTrailerQuality.SelectedIndex = -1
        Else
            Me.cbTrailerQuality.Enabled = True
        End If
    End Sub

    Private Sub chkEnableCredentials_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableCredentials.CheckedChanged
        Me.SetApplyButton(True)
        Me.txtProxyUsername.Enabled = Me.chkEnableCredentials.Checked
        Me.txtProxyPassword.Enabled = Me.chkEnableCredentials.Checked
        Me.txtProxyDomain.Enabled = Me.chkEnableCredentials.Checked

        If Not Me.chkEnableCredentials.Checked Then
            Me.txtProxyUsername.Text = String.Empty
            Me.txtProxyPassword.Text = String.Empty
            Me.txtProxyDomain.Text = String.Empty
        End If
    End Sub

    Private Sub chkEnableProxy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableProxy.CheckedChanged
        Me.SetApplyButton(True)
        Me.txtProxyURI.Enabled = Me.chkEnableProxy.Checked
        Me.txtProxyPort.Enabled = Me.chkEnableProxy.Checked
        Me.gbCreds.Enabled = Me.chkEnableProxy.Checked

        If Not Me.chkEnableProxy.Checked Then
            Me.txtProxyURI.Text = String.Empty
            Me.txtProxyPort.Text = String.Empty
            Me.chkEnableCredentials.Checked = False
            Me.txtProxyUsername.Text = String.Empty
            Me.txtProxyPassword.Text = String.Empty
            Me.txtProxyDomain.Text = String.Empty
        End If
    End Sub

    Private Sub chkEpisodeDashFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeDashFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeDotFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeDotFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeFanartCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeFanartCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeDashThumbJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeDashThumbJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeNfoCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeNfoCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodePosterCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodePosterCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpisodeTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpisodeTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpLockPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpLockPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpLockRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpLockRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpLockTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpLockTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkEpProperCase_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEpProperCase.CheckedChanged
        Me.SetApplyButton(True)
        Me.sResult.NeedsRefresh = True
    End Sub

    Private Sub chkETPadding_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkETPadding.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkPosterOnly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPosterOnly.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkFanartOnly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFanartOnly.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkFolderJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFolderJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkForceTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkForceTitle.CheckedChanged
        Me.cbForce.SelectedIndex = -1
        Me.cbForce.Enabled = Me.chkForceTitle.Checked
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkFullCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCast.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkFullCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCrew.CheckedChanged
        Me.SetApplyButton(True)

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
        Me.SetApplyButton(True)

        Me.txtGenreLimit.Enabled = Me.chkGenre.Checked

        If Not Me.chkGenre.Checked Then Me.txtGenreLimit.Text = "0"
    End Sub

    Private Sub chkGetEnglishImages_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGetEnglishImages.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkIFOScan_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkIFOScan.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkIgnoreLastScan_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkIgnoreLastScan.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkInfoPanelAnim_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkInfoPanelAnim.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockGenre.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockOutline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockOutline.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockRealStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockRealStudio.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockTagline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockTagline.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLockTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLockTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkLogErrors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLogErrors.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMarkNewEpisodes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMarkNewEpisodes.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMarkNewShows_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMarkNewShows.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMarkNew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMarkNew.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingExtra_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingExtra.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingNFO_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingNFO.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingPoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingSubs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingSubs.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMissingTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMissingTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieExtraCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieExtraCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieFanartCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieFanartCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieInfoCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieInfoCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameDotFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameDotFanartJPG.CheckedChanged
        btnApply.Enabled = True
    End Sub

    Private Sub chkMovieNameFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameDashPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameDashPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameMultiOnly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameMultiOnly.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameNFO_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameNFO.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNameTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNameTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieNFO_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieNFO.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMoviePosterCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMoviePosterCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieSubCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieSubCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMovieTrailerCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieTrailerCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkMPAA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMPAA.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkCert.Enabled = Me.chkMPAA.Checked

        If Not Me.chkMPAA.Checked Then
            Me.chkCert.Checked = False
            Me.cbCert.Enabled = False
            Me.cbCert.SelectedIndex = -1
            Me.chkUseCertForMPAA.Enabled = False
            Me.chkUseCertForMPAA.Checked = False
        End If
    End Sub

    Private Sub chkMusicBy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMusicBy.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkNoDisplayFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoDisplayFanart.CheckedChanged
        Me.SetApplyButton(True)
        If Me.chkNoDisplayFanart.Checked AndAlso Me.chkNoDisplayPoster.Checked Then
            Me.chkShowDims.Enabled = False
            Me.chkShowDims.Checked = False
        Else
            Me.chkShowDims.Enabled = True
        End If
    End Sub

    Private Sub chkNoDisplayPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoDisplayPoster.CheckedChanged
        Me.SetApplyButton(True)
        If Me.chkNoDisplayFanart.Checked AndAlso Me.chkNoDisplayPoster.Checked Then
            Me.chkShowDims.Enabled = False
            Me.chkShowDims.Checked = False
        Else
            Me.chkShowDims.Enabled = True
        End If
    End Sub

    Private Sub chkNoDLTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoDLTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkNoFilterEpisode_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoFilterEpisode.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkEpProperCase.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.lstEpFilters.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.txtEpFilter.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.btnAddEpFilter.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.btnEpFilterUp.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.btnEpFilterDown.Enabled = Not Me.chkNoFilterEpisode.Checked
        Me.btnRemoveEpFilter.Enabled = Not Me.chkNoFilterEpisode.Checked
    End Sub

    Private Sub chkNoSaveImagesToNfo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoSaveImagesToNfo.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkNoSpoilers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNoSpoilers.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOnlyTVImagesLanguage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOnlyTVImagesLanguage.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkGetEnglishImages.Enabled = Me.chkOnlyTVImagesLanguage.Checked

        If Not Me.chkOnlyTVImagesLanguage.Checked Then Me.chkGetEnglishImages.Checked = False
    End Sub

    Private Sub chkOnlyValueForCert_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOnlyValueForCert.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOutlineForPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOutlineForPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOutline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOutline.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkOutlineForPlot.Enabled = Me.chkOutline.Checked
        If Not Me.chkOutline.Checked Then Me.chkOutlineForPlot.Checked = False
    End Sub

    Private Sub chkOverwriteAllSPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteAllSPoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteEpFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteEpFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteEpPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteEpPoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteNfo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteNfo.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwritePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwritePoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteShowFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteShowFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteShowPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteShowPoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkOverwriteTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkPersistImgCache_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPersistImgCache.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkPosterTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPosterTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkProducers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProducers.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkProperCase_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProperCase.CheckedChanged
        Me.SetApplyButton(True)
        Me.sResult.NeedsRefresh = True
    End Sub

    Private Sub chkRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkRelease_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRelease.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkResizeAllSPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeAllSPoster.CheckedChanged
        Me.SetApplyButton(True)

        txtAllSPosterWidth.Enabled = chkResizeAllSPoster.Checked
        txtAllSPosterHeight.Enabled = chkResizeAllSPoster.Checked

        If Not chkResizeAllSPoster.Checked Then
            txtAllSPosterWidth.Text = String.Empty
            txtAllSPosterHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizeEpFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeEpFanart.CheckedChanged
        Me.SetApplyButton(True)

        txtEpFanartWidth.Enabled = chkResizeEpFanart.Checked
        txtEpFanartHeight.Enabled = chkResizeEpFanart.Checked

        If Not chkResizeEpFanart.Checked Then
            txtEpFanartWidth.Text = String.Empty
            txtEpFanartHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizeEpPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeEpPoster.CheckedChanged
        Me.SetApplyButton(True)

        txtEpPosterWidth.Enabled = chkResizeEpPoster.Checked
        txtEpPosterHeight.Enabled = chkResizeEpPoster.Checked

        If Not chkResizeEpFanart.Checked Then
            txtEpPosterWidth.Text = String.Empty
            txtEpPosterHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizeFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeFanart.CheckedChanged
        Me.SetApplyButton(True)

        txtFanartWidth.Enabled = chkResizeFanart.Checked
        txtFanartHeight.Enabled = chkResizeFanart.Checked

        If Not chkResizeFanart.Checked Then
            txtFanartWidth.Text = String.Empty
            txtFanartHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizePoster.CheckedChanged
        Me.SetApplyButton(True)

        txtPosterWidth.Enabled = chkResizePoster.Checked
        txtPosterHeight.Enabled = chkResizePoster.Checked

        If Not chkResizePoster.Checked Then
            txtPosterWidth.Text = String.Empty
            txtPosterHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizeShowFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeShowFanart.CheckedChanged
        Me.SetApplyButton(True)

        txtShowFanartWidth.Enabled = chkResizeShowFanart.Checked
        txtShowFanartHeight.Enabled = chkResizeShowFanart.Checked

        If Not chkResizeShowFanart.Checked Then
            txtShowFanartWidth.Text = String.Empty
            txtShowFanartHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkResizeShowPoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkResizeShowPoster.CheckedChanged
        Me.SetApplyButton(True)

        txtShowPosterWidth.Enabled = chkResizeShowPoster.Checked
        txtShowPosterHeight.Enabled = chkResizeShowPoster.Checked

        If Not chkResizeShowPoster.Checked Then
            txtShowPosterWidth.Text = String.Empty
            txtShowPosterHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkRuntime_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRuntime.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScanMediaInfo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScanMediaInfo.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScanOrderModify_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScanOrderModify.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpActors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpActors.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpAired_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpAired.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpCredits_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpCredits.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpDirector_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpDirector.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpEpisode_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpEpisode.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpSeason_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpSeason.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperEpTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperEpTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowActors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowActors.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowEGU_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowEGU.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowGenre.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowMPAA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowMPAA.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowPremiered_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowPremiered.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowStudio.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkScraperShowTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkScraperShowTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeaOverwriteFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeaOverwriteFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeaOverwritePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeaOverwritePoster.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeaResizeFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeaResizeFanart.CheckedChanged
        Me.SetApplyButton(True)

        txtSeaFanartWidth.Enabled = chkSeaResizeFanart.Checked
        txtSeaFanartHeight.Enabled = chkSeaResizeFanart.Checked

        If Not chkSeaResizeFanart.Checked Then
            txtSeaFanartWidth.Text = String.Empty
            txtSeaFanartHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkSeaResizePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeaResizePoster.CheckedChanged
        Me.SetApplyButton(True)

        txtSeaPosterWidth.Enabled = chkSeaResizePoster.Checked
        txtSeaPosterHeight.Enabled = chkSeaResizePoster.Checked

        If Not chkSeaResizePoster.Checked Then
            txtSeaPosterWidth.Text = String.Empty
            txtSeaPosterHeight.Text = String.Empty
        End If
    End Sub

    Private Sub chkSeasonAllJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonAllJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonAllTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonAllTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonAllPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonAllPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonDashFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonDashFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonXXDashFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonXXDashFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonDotFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonDotFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonFanartCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonFanartCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonFolderJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonFolderJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonNameJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonNameJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonNameTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonNameTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonPosterCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonPosterCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonPosterTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonPosterTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonXTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonXTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonXXTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonXXTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSeasonXXDashPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSeasonXXDashPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowDashFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowDashFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowDims_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowDims.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowDotFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowDotFanart.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowFanartCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowFanartCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowFanartJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowFolderJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowFolderJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowGenresText_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowGenresText.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowLockGenre_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLockGenre.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowLockPlot_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLockPlot.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowLockRating_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLockRating.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowLockStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLockStudio.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowLockTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLockTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowNfoCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowNfoCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowPosterCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowPosterCol.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowPosterJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowPosterJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowBannerJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowBannerJPG.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowPosterTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowPosterTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkShowProperCase_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowProperCase.CheckedChanged
        Me.SetApplyButton(True)
        Me.sResult.NeedsRefresh = True
    End Sub

    Private Sub chkShowTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowTBN.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSingleScrapeImages_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSingleScrapeImages.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSingleScrapeTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSingleScrapeTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSortBeforeScan_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSortBeforeScan.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkSourceFromFolder_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSourceFromFolder.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkStudio_CheckedChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStudio.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTagline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTagline.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTitle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTitle.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTop250_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTop250.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkCountry_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCountry.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTVCleanDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTVCleanDB.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTVIgnoreLastScan_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTVIgnoreLastScan.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkTVScanMetaData_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTVScanMetaData.CheckedChanged
        Me.SetApplyButton(True)

        Me.cboTVMetaDataOverlay.Enabled = Me.chkTVScanMetaData.Checked

        If Not Me.chkTVScanMetaData.Checked Then
            Me.cboTVMetaDataOverlay.SelectedIndex = 0
        End If
    End Sub

    Private Sub chkTVScanOrderModify_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTVScanOrderModify.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUpdaterTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUpdaterTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUpdates_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUpdates.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUseCertForMPAA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseCertForMPAA.CheckedChanged
        Me.SetApplyButton(True)

        Me.chkOnlyValueForCert.Enabled = Me.chkUseCertForMPAA.Checked

        If Not Me.chkUseCertForMPAA.Checked Then Me.chkOnlyValueForCert.Checked = False
    End Sub

    Private Sub chkUseETasFA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseETasFA.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUseImgCacheUpdaters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseImgCacheUpdaters.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUseImgCache_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseImgCache.CheckedChanged
        Me.chkPersistImgCache.Enabled = Me.chkUseImgCache.Checked
        Me.chkUseImgCacheUpdaters.Enabled = Me.chkUseImgCache.Checked
        If Not Me.chkUseImgCache.Checked Then
            Me.chkPersistImgCache.Checked = False
            Me.chkUseImgCacheUpdaters.Checked = False
        End If
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkUseMIDuration_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseMIDuration.CheckedChanged
        Me.txtRuntimeFormat.Enabled = Me.chkUseMIDuration.Checked
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkVideoTSParent_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetApplyButton(True)
        Me.sResult.NeedsUpdate = True
    End Sub

    Private Sub chkVotes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkVotes.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkWhitelistVideo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWhitelistVideo.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkWriters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWriters.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkYAMJCompatibleSets_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetApplyButton(True)
    End Sub

    Private Sub chkYear_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkYear.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub chShowPosterSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbShowPosterSize.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub ClearRegex()
        Me.btnAddShowRegex.Text = Master.eLang.GetString(115, "Add Regex")
        Me.btnAddShowRegex.Tag = String.Empty
        Me.btnAddShowRegex.Enabled = False
        Me.txtSeasonRegex.Text = String.Empty
        Me.cboSeasonRetrieve.SelectedIndex = -1
        Me.txtEpRegex.Text = String.Empty
        Me.cboEpRetrieve.SelectedIndex = -1
    End Sub

    Private Sub dlgSettings_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub EditShowRegex(ByVal lItem As ListViewItem)
        Me.btnAddShowRegex.Text = Master.eLang.GetString(124, "Update Regex")
        Me.btnAddShowRegex.Tag = lItem.Text

        Me.txtSeasonRegex.Text = lItem.SubItems(1).Text.ToString

        Select Case lItem.SubItems(2).Text
            Case "Folder"
                Me.cboSeasonRetrieve.SelectedIndex = 0
            Case "File"
                Me.cboSeasonRetrieve.SelectedIndex = 1
        End Select

        Me.txtEpRegex.Text = lItem.SubItems(3).Text

        Select Case lItem.SubItems(4).Text
            Case "Folder"
                Me.cboEpRetrieve.SelectedIndex = 0
            Case "File"
                Me.cboEpRetrieve.SelectedIndex = 1
            Case "Result"
                Me.cboEpRetrieve.SelectedIndex = 2
        End Select
    End Sub

    Private Sub FillGenres()
        If Not String.IsNullOrEmpty(Master.eSettings.GenreFilter) Then
            Dim genreArray() As String
            genreArray = Strings.Split(Master.eSettings.GenreFilter, ",")
            For g As Integer = 0 To UBound(genreArray)
                If Me.lbGenre.FindString(Strings.Trim(genreArray(g))) > 0 Then Me.lbGenre.SetItemChecked(Me.lbGenre.FindString(Strings.Trim(genreArray(g))), True)
            Next

            If Me.lbGenre.CheckedItems.Count = 0 Then
                Me.lbGenre.SetItemChecked(0, True)
            End If
        Else
            Me.lbGenre.SetItemChecked(0, True)
        End If
    End Sub

    Private Sub FillList(ByVal sType As String)
        Dim pNode As New TreeNode
        Dim cNode As New TreeNode

        Me.tvSettings.Nodes.Clear()
        Me.RemoveCurrPanel()

        For Each pPanel As Containers.SettingsPanel In SettingsPanels.Where(Function(s) s.Type = sType AndAlso String.IsNullOrEmpty(s.Parent)).OrderBy(Function(s) s.Order)
            pNode = New TreeNode(pPanel.Text, pPanel.ImageIndex, pPanel.ImageIndex)
            pNode.Name = pPanel.Name
            For Each cPanel As Containers.SettingsPanel In SettingsPanels.Where(Function(p) p.Type = sType AndAlso p.Parent = pNode.Name).OrderBy(Function(s) s.Order)
                cNode = New TreeNode(cPanel.Text, cPanel.ImageIndex, cPanel.ImageIndex)
                cNode.Name = cPanel.Name
                pNode.Nodes.Add(cNode)
            Next
            Me.tvSettings.Nodes.Add(pNode)
        Next

        If Me.tvSettings.Nodes.Count > 0 Then
            Me.tvSettings.ExpandAll()
            Me.tvSettings.SelectedNode = Me.tvSettings.Nodes(0)
        Else
            Me.pbCurrent.Image = Nothing
            Me.lblCurrent.Text = String.Empty
        End If
    End Sub

    Private Sub FillSettings()
        Try
            Me.chkProperCase.Checked = Master.eSettings.ProperCase
            Me.chkShowProperCase.Checked = Master.eSettings.ShowProperCase
            Me.chkEpProperCase.Checked = Master.eSettings.EpProperCase
            Me.chkCleanFolderJPG.Checked = Master.eSettings.CleanFolderJPG
            Me.chkCleanMovieTBN.Checked = Master.eSettings.CleanMovieTBN
            Me.chkCleanMovieTBNb.Checked = Master.eSettings.CleanMovieTBNB
            Me.chkCleanFanartJPG.Checked = Master.eSettings.CleanFanartJPG
            Me.chkCleanMovieFanartJPG.Checked = Master.eSettings.CleanMovieFanartJPG
            Me.chkCleanMovieNFO.Checked = Master.eSettings.CleanMovieNFO
            Me.chkCleanMovieNFOb.Checked = Master.eSettings.CleanMovieNFOB
            Me.chkCleanPosterTBN.Checked = Master.eSettings.CleanPosterTBN
            Me.chkCleanPosterJPG.Checked = Master.eSettings.CleanPosterJPG
            Me.chkCleanMovieJPG.Checked = Master.eSettings.CleanMovieJPG
            Me.chkCleanMovieNameJPG.Checked = Master.eSettings.CleanMovieNameJPG
            Me.chkCleanDotFanartJPG.Checked = Master.eSettings.CleanDotFanartJPG
            Me.chkCleanExtrathumbs.Checked = Master.eSettings.CleanExtraThumbs
            Me.tcCleaner.SelectedTab = If(Master.eSettings.ExpertCleaner, Me.tpExpert, Me.tpStandard)
            Me.chkWhitelistVideo.Checked = Master.eSettings.CleanWhitelistVideo
            Me.lstWhitelist.Items.AddRange(Master.eSettings.CleanWhitelistExts.ToArray)
            Me.chkOverwriteNfo.Checked = Master.eSettings.OverwriteNfo
            'Me.chkYAMJCompatibleSets.Checked = Master.eSettings.YAMJSetsCompatible
            Me.chkLogErrors.Checked = Master.eSettings.LogErrors
            Me.lstNoStack.Items.AddRange(Master.eSettings.NoStackExts.ToArray)
            Me.chkUpdates.Checked = Master.eSettings.CheckUpdates
            Me.chkInfoPanelAnim.Checked = Master.eSettings.InfoPanelAnim

            If Not String.IsNullOrEmpty(Master.eSettings.CertificationLang) Then
                Me.chkCert.Checked = True
                Me.cbCert.Enabled = True
                Me.cbCert.Text = Master.eSettings.CertificationLang
                Me.chkUseCertForMPAA.Enabled = True
                Me.chkUseCertForMPAA.Checked = Master.eSettings.UseCertForMPAA
            End If
            If Not String.IsNullOrEmpty(Master.eSettings.ForceTitle) Then
                Me.chkForceTitle.Checked = True
                Me.cbForce.Enabled = True
                Me.cbForce.Text = Master.eSettings.ForceTitle
            End If
            Me.chkScanMediaInfo.Checked = Master.eSettings.ScanMediaInfo
            Me.chkTVScanMetaData.Checked = Master.eSettings.ScanTVMediaInfo
            Me.chkFullCast.Checked = Master.eSettings.FullCast
            Me.chkFullCrew.Checked = Master.eSettings.FullCrew
            Me.chkCastWithImg.Checked = Master.eSettings.CastImagesOnly
            Me.chkMoviePosterCol.Checked = Master.eSettings.MoviePosterCol
            Me.chkMovieFanartCol.Checked = Master.eSettings.MovieFanartCol
            Me.chkMovieInfoCol.Checked = Master.eSettings.MovieInfoCol
            Me.chkMovieTrailerCol.Checked = Master.eSettings.MovieTrailerCol
            Me.chkMovieSubCol.Checked = Master.eSettings.MovieSubCol
            Me.chkMovieExtraCol.Checked = Master.eSettings.MovieExtraCol

            Me.cbPosterSize.SelectedIndex = Master.eSettings.PreferredPosterSize
            Me.cbFanartSize.SelectedIndex = Master.eSettings.PreferredFanartSize
            If Master.eSettings.IsShowBanner Then
                Me.rbBanner.Checked = True
                Me.cbShowPosterSize.SelectedIndex = Master.eSettings.PreferredShowBannerType
            Else
                Me.rbPoster.Checked = True
                Me.cbShowPosterSize.SelectedIndex = Master.eSettings.PreferredShowPosterSize
            End If
            If Master.eSettings.IsAllSBanner Then
                Me.rbAllSBanner.Checked = True
                Me.cbAllSPosterSize.SelectedIndex = Master.eSettings.PreferredAllSBannerType
            Else
                Me.rbAllSPoster.Checked = True
                Me.cbAllSPosterSize.SelectedIndex = Master.eSettings.PreferredAllSPosterSize
            End If
            Me.cbShowFanartSize.SelectedIndex = Master.eSettings.PreferredShowFanartSize
            Me.cbEpFanartSize.SelectedIndex = Master.eSettings.PreferredEpFanartSize
            Me.cbSeaPosterSize.SelectedIndex = Master.eSettings.PreferredSeasonPosterSize
            Me.cbSeaFanartSize.SelectedIndex = Master.eSettings.PreferredSeasonFanartSize
            Me.chkAutoETSize.Checked = Master.eSettings.AutoET
            Me.cbAutoETSize.SelectedIndex = Master.eSettings.AutoETSize
            Me.chkFanartOnly.Checked = Master.eSettings.FanartPrefSizeOnly
            Me.chkPosterOnly.Checked = Master.eSettings.PosterPrefSizeOnly
            Me.tbPosterQual.Value = Master.eSettings.PosterQuality
            Me.tbFanartQual.Value = Master.eSettings.FanartQuality
            Me.tbShowPosterQual.Value = Master.eSettings.ShowPosterQuality
            Me.tbShowFanartQual.Value = Master.eSettings.ShowFanartQuality
            Me.tbAllSPosterQual.Value = Master.eSettings.AllSPosterQuality
            Me.tbEpPosterQual.Value = Master.eSettings.EpPosterQuality
            Me.tbEpFanartQual.Value = Master.eSettings.EpFanartQuality
            Me.tbSeaPosterQual.Value = Master.eSettings.SeasonPosterQuality
            Me.tbSeaFanartQual.Value = Master.eSettings.SeasonFanartQuality
            Me.chkOverwritePoster.Checked = Master.eSettings.OverwritePoster
            Me.chkOverwriteFanart.Checked = Master.eSettings.OverwriteFanart
            Me.chkOverwriteShowPoster.Checked = Master.eSettings.OverwriteShowPoster
            Me.chkOverwriteAllSPoster.Checked = Master.eSettings.OverwriteAllSPoster
            Me.chkOverwriteShowFanart.Checked = Master.eSettings.OverwriteShowFanart
            Me.chkOverwriteEpPoster.Checked = Master.eSettings.OverwriteEpPoster
            Me.chkOverwriteEpFanart.Checked = Master.eSettings.OverwriteEpFanart
            Me.chkSeaOverwritePoster.Checked = Master.eSettings.OverwriteSeasonPoster
            Me.chkSeaOverwriteFanart.Checked = Master.eSettings.OverwriteSeasonFanart
            Me.chkMovieTBN.Checked = Master.eSettings.MovieTBN
            Me.chkMovieNameTBN.Checked = Master.eSettings.MovieNameTBN
            Me.chkMovieJPG.Checked = Master.eSettings.MovieJPG
            Me.chkMovieNameJPG.Checked = Master.eSettings.MovieNameJPG
            Me.chkMovieNameDashPosterJPG.Checked = Master.eSettings.MovieNameDashPosterJPG
            Me.chkPosterTBN.Checked = Master.eSettings.PosterTBN
            Me.chkPosterJPG.Checked = Master.eSettings.PosterJPG
            Me.chkFolderJPG.Checked = Master.eSettings.FolderJPG
            Me.chkFanartJPG.Checked = Master.eSettings.FanartJPG
            Me.chkMovieNameFanartJPG.Checked = Master.eSettings.MovieNameFanartJPG
            Me.chkMovieNameDotFanartJPG.Checked = Master.eSettings.MovieNameDotFanartJPG
            Me.chkMovieNFO.Checked = Master.eSettings.MovieNFO
            Me.chkMovieNameNFO.Checked = Master.eSettings.MovieNameNFO
            Me.chkMovieNameMultiOnly.Checked = Master.eSettings.MovieNameMultiOnly
            Me.rbDashTrailer.Checked = Master.eSettings.DashTrailer
            Me.rbBracketTrailer.Checked = Not Master.eSettings.DashTrailer
            'Me.chkVideoTSParent.Checked = Master.eSettings.VideoTSParent
            Me.chkLockPlot.Checked = Master.eSettings.LockPlot
            Me.chkLockOutline.Checked = Master.eSettings.LockOutline
            Me.chkLockTitle.Checked = Master.eSettings.LockTitle
            Me.chkLockTagline.Checked = Master.eSettings.LockTagline
            Me.chkLockRating.Checked = Master.eSettings.LockRating
            Me.chkLockRealStudio.Checked = Master.eSettings.LockStudio
            Me.chkLockGenre.Checked = Master.eSettings.LockGenre
            Me.chkLockTrailer.Checked = Master.eSettings.LockTrailer
            Me.chkSingleScrapeImages.Checked = Master.eSettings.SingleScrapeImages
            Me.chkClickScrape.Checked = Master.eSettings.ClickScrape
            Me.chkAskCheckboxScrape.Enabled = Me.chkClickScrape.Checked
            Me.chkAskCheckboxScrape.Checked = Master.eSettings.AskCheckboxScrape
            Me.chkMarkNew.Checked = Master.eSettings.MarkNew
            Me.chkResizeFanart.Checked = Master.eSettings.ResizeFanart
            If Master.eSettings.ResizeFanart Then
                Me.txtFanartWidth.Text = Master.eSettings.FanartWidth.ToString
                Me.txtFanartHeight.Text = Master.eSettings.FanartHeight.ToString
            End If
            Me.chkResizePoster.Checked = Master.eSettings.ResizePoster
            If Master.eSettings.ResizePoster Then
                Me.txtPosterWidth.Text = Master.eSettings.PosterWidth.ToString
                Me.txtPosterHeight.Text = Master.eSettings.PosterHeight.ToString
            End If
            Me.chkResizeShowFanart.Checked = Master.eSettings.ResizeShowFanart
            If Master.eSettings.ResizeShowFanart Then
                Me.txtShowFanartWidth.Text = Master.eSettings.ShowFanartWidth.ToString
                Me.txtShowFanartHeight.Text = Master.eSettings.ShowFanartHeight.ToString
            End If
            Me.chkResizeShowPoster.Checked = Master.eSettings.ResizeShowPoster
            If Master.eSettings.ResizeShowPoster Then
                Me.txtShowPosterWidth.Text = Master.eSettings.ShowPosterWidth.ToString
                Me.txtShowPosterHeight.Text = Master.eSettings.ShowPosterHeight.ToString
            End If
            Me.chkResizeAllSPoster.Checked = Master.eSettings.ResizeAllSPoster
            If Master.eSettings.ResizeAllSPoster Then
                Me.txtAllSPosterWidth.Text = Master.eSettings.AllSPosterWidth.ToString
                Me.txtAllSPosterHeight.Text = Master.eSettings.AllSPosterHeight.ToString
            End If
            Me.chkResizeEpFanart.Checked = Master.eSettings.ResizeEpFanart
            If Master.eSettings.ResizeEpFanart Then
                Me.txtEpFanartWidth.Text = Master.eSettings.EpFanartWidth.ToString
                Me.txtEpFanartHeight.Text = Master.eSettings.EpFanartHeight.ToString
            End If
            Me.chkResizeEpPoster.Checked = Master.eSettings.ResizeEpPoster
            If Master.eSettings.ResizeEpPoster Then
                Me.txtEpPosterWidth.Text = Master.eSettings.EpPosterWidth.ToString
                Me.txtEpPosterHeight.Text = Master.eSettings.EpPosterHeight.ToString
            End If
            Me.chkSeaResizeFanart.Checked = Master.eSettings.ResizeSeasonFanart
            If Master.eSettings.ResizeSeasonFanart Then
                Me.txtSeaFanartWidth.Text = Master.eSettings.SeasonFanartWidth.ToString
                Me.txtSeaFanartHeight.Text = Master.eSettings.SeasonFanartHeight.ToString
            End If
            Me.chkSeaResizePoster.Checked = Master.eSettings.ResizeSeasonPoster
            If Master.eSettings.ResizeSeasonPoster Then
                Me.txtSeaPosterWidth.Text = Master.eSettings.SeasonPosterWidth.ToString
                Me.txtSeaPosterHeight.Text = Master.eSettings.SeasonPosterHeight.ToString
            End If
            If Master.eSettings.AutoThumbs > 0 Then
                Me.chkAutoThumbs.Checked = True
                Me.txtAutoThumbs.Enabled = True
                Me.txtAutoThumbs.Text = Master.eSettings.AutoThumbs.ToString
                Me.chkNoSpoilers.Enabled = True
                Me.chkNoSpoilers.Checked = Master.eSettings.AutoThumbsNoSpoilers
                Me.chkUseETasFA.Enabled = True
                Me.chkUseETasFA.Checked = Master.eSettings.UseETasFA
            End If

            Me.txtBDPath.Text = Master.eSettings.BDPath
            Me.txtBDPath.Enabled = Master.eSettings.AutoBD
            Me.btnBrowse.Enabled = Master.eSettings.AutoBD
            Me.chkAutoBD.Checked = Master.eSettings.AutoBD
            Me.chkUseMIDuration.Checked = Master.eSettings.UseMIDuration
            Me.txtRuntimeFormat.Enabled = Master.eSettings.UseMIDuration
            Me.txtRuntimeFormat.Text = Master.eSettings.RuntimeMask
            Me.chkUseImgCache.Checked = Master.eSettings.UseImgCache
            Me.chkUseImgCacheUpdaters.Checked = Master.eSettings.UseImgCacheUpdaters
            Me.chkPersistImgCache.Checked = Master.eSettings.PersistImgCache
            Me.txtSkipLessThan.Text = Master.eSettings.SkipLessThan.ToString
            Me.chkSkipStackedSizeCheck.Checked = Master.eSettings.SkipStackSizeCheck
            Me.txtTVSkipLessThan.Text = Master.eSettings.SkipLessThanEp.ToString
            Me.chkNoSaveImagesToNfo.Checked = Master.eSettings.NoSaveImagesToNfo
            Me.chkDownloadTrailer.Checked = Master.eSettings.DownloadTrailers
            Me.chkUpdaterTrailer.Checked = Master.eSettings.UpdaterTrailers
            Me.chkNoDLTrailer.Checked = Master.eSettings.UpdaterTrailersNoDownload
            Me.chkSingleScrapeTrailer.Checked = Master.eSettings.SingleScrapeTrailer

            Me.chkOverwriteTrailer.Checked = Master.eSettings.OverwriteTrailer
            Me.chkDeleteAllTrailers.Checked = Master.eSettings.DeleteAllTrailers

            Me.cbTrailerQuality.SelectedValue = Master.eSettings.PreferredTrailerQuality

            FillGenres()

            Me.chkShowDims.Checked = Master.eSettings.ShowDims
            Me.chkNoDisplayFanart.Checked = Master.eSettings.NoDisplayFanart
            Me.chkNoDisplayPoster.Checked = Master.eSettings.NoDisplayPoster
            Me.chkOutlineForPlot.Checked = Master.eSettings.OutlineForPlot

            Me.chkShowGenresText.Checked = Master.eSettings.AllwaysDisplayGenresText
            Me.chkDisplayYear.Checked = Master.eSettings.DisplayYear

            Me.rbETNative.Checked = Master.eSettings.ETNative
            If Not Master.eSettings.ETNative AndAlso Master.eSettings.ETWidth > 0 AndAlso Master.eSettings.ETHeight > 0 Then
                Me.rbETCustom.Checked = True
                Me.txtETHeight.Text = Master.eSettings.ETHeight.ToString
                Me.txtETWidth.Text = Master.eSettings.ETWidth.ToString
                Me.chkETPadding.Checked = Master.eSettings.ETPadding
            Else
                Me.rbETNative.Checked = True
            End If

            Me.lstSortTokens.Items.AddRange(Master.eSettings.SortTokens.ToArray)

            If Master.eSettings.LevTolerance > 0 Then
                Me.chkCheckTitles.Checked = True
                Me.txtCheckTitleTol.Enabled = True
                Me.txtCheckTitleTol.Text = Master.eSettings.LevTolerance.ToString
            End If
            Me.chkAutoDetectVTS.Checked = Master.eSettings.AutoDetectVTS
            Me.cbLanguages.SelectedItem = If(String.IsNullOrEmpty(Master.eSettings.FlagLang), Master.eLang.Disabled, Master.eSettings.FlagLang)
            Me.cboTVMetaDataOverlay.SelectedItem = If(String.IsNullOrEmpty(Master.eSettings.TVFlagLang), Master.eLang.Disabled, Master.eSettings.TVFlagLang)
            Me.cbIntLang.SelectedItem = Master.eSettings.Language

            Me.chkTitle.Checked = Master.eSettings.FieldTitle
            Me.chkYear.Checked = Master.eSettings.FieldYear
            Me.chkMPAA.Checked = Master.eSettings.FieldMPAA
            Me.chkCertification.Checked = Master.eSettings.FieldCert
            Me.chkRelease.Checked = Master.eSettings.FieldRelease
            Me.chkRuntime.Checked = Master.eSettings.FieldRuntime
            Me.chkRating.Checked = Master.eSettings.FieldRating
            Me.chkVotes.Checked = Master.eSettings.FieldVotes
            Me.chkStudio.Checked = Master.eSettings.FieldStudio
            Me.chkGenre.Checked = Master.eSettings.FieldGenre
            Me.chkTrailer.Checked = Master.eSettings.FieldTrailer
            Me.chkTagline.Checked = Master.eSettings.FieldTagline
            Me.chkOutline.Checked = Master.eSettings.FieldOutline
            Me.chkPlot.Checked = Master.eSettings.FieldPlot
            Me.chkCast.Checked = Master.eSettings.FieldCast
            Me.chkDirector.Checked = Master.eSettings.FieldDirector
            Me.chkWriters.Checked = Master.eSettings.FieldWriters
            If Master.eSettings.FullCrew Then
                Me.chkProducers.Checked = Master.eSettings.FieldProducers
                Me.chkMusicBy.Checked = Master.eSettings.FieldMusic
                Me.chkCrew.Checked = Master.eSettings.FieldCrew
            End If
            Me.chkTop250.Checked = Master.eSettings.Field250
            Me.chkCountry.Checked = Master.eSettings.FieldCountry
            Me.txtActorLimit.Text = Master.eSettings.ActorLimit.ToString
            Me.txtGenreLimit.Text = Master.eSettings.GenreLimit.ToString

            Me.chkMissingPoster.Checked = Master.eSettings.MissingFilterPoster
            Me.chkMissingFanart.Checked = Master.eSettings.MissingFilterFanart
            Me.chkMissingNFO.Checked = Master.eSettings.MissingFilterNFO
            Me.chkMissingTrailer.Checked = Master.eSettings.MissingFilterTrailer
            Me.chkMissingSubs.Checked = Master.eSettings.MissingFilterSubs
            Me.chkMissingExtra.Checked = Master.eSettings.MissingFilterExtras
            Me.cbMovieTheme.SelectedItem = Master.eSettings.MovieTheme
            Me.cbTVShowTheme.SelectedItem = Master.eSettings.TVShowTheme
            Me.cbEpTheme.SelectedItem = Master.eSettings.TVEpTheme
            Me.Meta.AddRange(Master.eSettings.MetadataPerFileType)
            Me.LoadMetadata()
            Me.TVMeta.AddRange(Master.eSettings.TVMetadataperFileType)
            Me.LoadTVMetadata()
            Me.chkIFOScan.Checked = Master.eSettings.EnableIFOScan
            Me.chkCleanDB.Checked = Master.eSettings.CleanDB
            Me.chkIgnoreLastScan.Checked = Master.eSettings.IgnoreLastScan
            Me.chkTVCleanDB.Checked = Master.eSettings.TVCleanDB
            Me.chkTVIgnoreLastScan.Checked = Master.eSettings.TVIgnoreLastScan
            Me.ShowRegex.AddRange(Master.eSettings.TVShowRegexes)
            Me.LoadShowRegex()
            Me.cbRatingRegion.Text = Master.eSettings.ShowRatingRegion
            Me.chkSeasonAllTBN.Checked = Master.eSettings.SeasonAllTBN
            Me.chkSeasonAllJPG.Checked = Master.eSettings.SeasonAllJPG
            Me.chkSeasonAllPosterJPG.Checked = Master.eSettings.SeasonAllPosterJPG
            Me.chkShowTBN.Checked = Master.eSettings.ShowTBN
            Me.chkShowJPG.Checked = Master.eSettings.ShowJPG
            Me.chkShowFolderJPG.Checked = Master.eSettings.ShowFolderJPG
            Me.chkShowPosterTBN.Checked = Master.eSettings.ShowPosterTBN
            Me.chkShowPosterJPG.Checked = Master.eSettings.ShowPosterJPG
            Me.chkShowBannerJPG.Checked = Master.eSettings.ShowBannerJPG
            Me.chkShowFanartJPG.Checked = Master.eSettings.ShowFanartJPG
            Me.chkShowDashFanart.Checked = Master.eSettings.ShowDashFanart
            Me.chkShowDotFanart.Checked = Master.eSettings.ShowDotFanart
            Me.chkSeasonXXTBN.Checked = Master.eSettings.SeasonXX
            Me.chkSeasonXTBN.Checked = Master.eSettings.SeasonX
            Me.chkSeasonXXDashPosterJPG.Checked = Master.eSettings.SeasonXXDashPosterJPG
            Me.chkSeasonPosterTBN.Checked = Master.eSettings.SeasonPosterTBN
            Me.chkSeasonPosterJPG.Checked = Master.eSettings.SeasonPosterJPG
            Me.chkSeasonNameTBN.Checked = Master.eSettings.SeasonNameTBN
            Me.chkSeasonNameJPG.Checked = Master.eSettings.SeasonNameJPG
            Me.chkSeasonFolderJPG.Checked = Master.eSettings.SeasonFolderJPG
            Me.chkSeasonFanartJPG.Checked = Master.eSettings.SeasonFanartJPG
            Me.chkSeasonDashFanart.Checked = Master.eSettings.SeasonDashFanart
            Me.chkSeasonXXDashFanartJPG.Checked = Master.eSettings.SeasonXXDashFanartJPG
            Me.chkSeasonDotFanart.Checked = Master.eSettings.SeasonDotFanart
            Me.chkEpisodeTBN.Checked = Master.eSettings.EpisodeTBN
            Me.chkEpisodeJPG.Checked = Master.eSettings.EpisodeJPG
            Me.chkEpisodeDashThumbJPG.Checked = Master.eSettings.EpisodeDashThumbJPG
            Me.chkEpisodeDashFanart.Checked = Master.eSettings.EpisodeDashFanart
            Me.chkEpisodeDotFanart.Checked = Master.eSettings.EpisodeDotFanart
            Me.chkShowPosterCol.Checked = Master.eSettings.ShowPosterCol
            Me.chkShowFanartCol.Checked = Master.eSettings.ShowFanartCol
            Me.chkShowNfoCol.Checked = Master.eSettings.ShowNfoCol
            Me.chkSeasonPosterCol.Checked = Master.eSettings.SeasonPosterCol
            Me.chkSeasonFanartCol.Checked = Master.eSettings.SeasonFanartCol
            Me.chkEpisodePosterCol.Checked = Master.eSettings.EpisodePosterCol
            Me.chkEpisodeFanartCol.Checked = Master.eSettings.EpisodeFanartCol
            Me.chkEpisodeNfoCol.Checked = Master.eSettings.EpisodeNfoCol
            Me.chkSourceFromFolder.Checked = Master.eSettings.SourceFromFolder
            Me.chkSortBeforeScan.Checked = Master.eSettings.SortBeforeScan
            Me.tLangList.Clear()
            Me.tLangList.AddRange(Master.eSettings.TVDBLanguages)
            Me.cbTVLanguage.Items.AddRange((From lLang In Master.eSettings.TVDBLanguages Select lLang.LongLang).ToArray)
            If Me.cbTVLanguage.Items.Count > 0 Then
                Me.cbTVLanguage.Text = Me.tLangList.FirstOrDefault(Function(l) l.ShortLang = Master.eSettings.TVDBLanguage).LongLang
            End If
            Me.txtTVDBMirror.Text = Master.eSettings.TVDBMirror

            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Me.chkEnableProxy.Checked = True
                Me.txtProxyURI.Text = Master.eSettings.ProxyURI
                Me.txtProxyPort.Text = Master.eSettings.ProxyPort.ToString

                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCreds.UserName) Then
                    Me.chkEnableCredentials.Checked = True
                    Me.txtProxyUsername.Text = Master.eSettings.ProxyCreds.UserName
                    Me.txtProxyPassword.Text = Master.eSettings.ProxyCreds.Password
                    Me.txtProxyDomain.Text = Master.eSettings.ProxyCreds.Domain
                End If
            End If
            Me.txtAPIKey.Text = Master.eSettings.ExternalTVDBAPIKey
            Me.chkScanOrderModify.Checked = Master.eSettings.ScanOrderModify
            Me.chkTVScanOrderModify.Checked = Master.eSettings.TVScanOrderModify
            Me.cboTVUpdate.SelectedIndex = Master.eSettings.TVUpdateTime
            Me.chkNoFilterEpisode.Checked = Master.eSettings.NoFilterEpisode
            Me.chkOnlyTVImagesLanguage.Checked = Master.eSettings.OnlyGetTVImagesForSelectedLanguage
            Me.chkGetEnglishImages.Checked = Master.eSettings.AlwaysGetEnglishTVImages
            Me.chkDisplayMissingEpisodes.Checked = Master.eSettings.DisplayMissingEpisodes
            Me.chkShowLockTitle.Checked = Master.eSettings.ShowLockTitle
            Me.chkShowLockPlot.Checked = Master.eSettings.ShowLockPlot
            Me.chkShowLockRating.Checked = Master.eSettings.ShowLockRating
            Me.chkShowLockGenre.Checked = Master.eSettings.ShowLockGenre
            Me.chkShowLockStudio.Checked = Master.eSettings.ShowLockStudio
            Me.chkEpLockTitle.Checked = Master.eSettings.EpLockTitle
            Me.chkEpLockPlot.Checked = Master.eSettings.EpLockPlot
            Me.chkEpLockRating.Checked = Master.eSettings.EpLockRating
            Me.chkScraperShowTitle.Checked = Master.eSettings.ScraperShowTitle
            Me.chkScraperShowEGU.Checked = Master.eSettings.ScraperShowEGU
            Me.chkScraperShowGenre.Checked = Master.eSettings.ScraperShowGenre
            Me.chkScraperShowMPAA.Checked = Master.eSettings.ScraperShowMPAA
            Me.chkScraperShowPlot.Checked = Master.eSettings.ScraperShowPlot
            Me.chkScraperShowPremiered.Checked = Master.eSettings.ScraperShowPremiered
            Me.chkScraperShowRating.Checked = Master.eSettings.ScraperShowRating
            Me.chkScraperShowStudio.Checked = Master.eSettings.ScraperShowStudio
            Me.chkScraperShowActors.Checked = Master.eSettings.ScraperShowActors
            Me.chkScraperEpTitle.Checked = Master.eSettings.ScraperEpTitle
            Me.chkScraperEpSeason.Checked = Master.eSettings.ScraperEpSeason
            Me.chkScraperEpEpisode.Checked = Master.eSettings.ScraperEpEpisode
            Me.chkScraperEpAired.Checked = Master.eSettings.ScraperEpAired
            Me.chkScraperEpRating.Checked = Master.eSettings.ScraperEpRating
            Me.chkScraperEpPlot.Checked = Master.eSettings.ScraperEpPlot
            Me.chkScraperEpDirector.Checked = Master.eSettings.ScraperEpDirector
            Me.chkScraperEpCredits.Checked = Master.eSettings.ScraperEpCredits
            Me.chkScraperEpActors.Checked = Master.eSettings.ScraperEpActors
            Me.chkDisplayAllSeason.Checked = Master.eSettings.DisplayAllSeason
            Me.chkMarkNewShows.Checked = Master.eSettings.MarkNewShows
            Me.chkMarkNewEpisodes.Checked = Master.eSettings.MarkNewEpisodes
            Me.cbOrdering.SelectedIndex = Master.eSettings.OrderDefault
            Me.chkOnlyValueForCert.Checked = Master.eSettings.OnlyValueForCert
            Me.chkActorCache.Checked = AdvancedSettings.GetBooleanSetting("ScrapeActorsThumbs", False)
            Me.RefreshSources()
            Me.RefreshTVSources()
            Me.RefreshShowFilters()
            Me.RefreshEpFilters()
            Me.RefreshMovieFilters()
            Me.RefreshValidExts()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub frmSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Functions.PNLDoubleBuffer(Me.pnlMain)
            Me.SetUp()
            Me.AddPanels()
            Me.AddButtons()
            Me.AddHelpHandlers(Me, "Core_")

            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Using g As Graphics = Graphics.FromImage(iBackground)
                g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
                Me.pnlTop.BackgroundImage = iBackground
            End Using

            iBackground = New Bitmap(Me.pnlCurrent.Width, Me.pnlCurrent.Height)
            Using b As Graphics = Graphics.FromImage(iBackground)
                b.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlCurrent.ClientRectangle, Color.SteelBlue, Color.FromKnownColor(KnownColor.Control), Drawing2D.LinearGradientMode.Horizontal), pnlCurrent.ClientRectangle)
                Me.pnlCurrent.BackgroundImage = iBackground
            End Using

            Me.LoadGenreLangs()
            Me.LoadIntLangs()
            Me.LoadLangs()
            Me.LoadThemes()
            Me.LoadRatingRegions()
            Me.FillSettings()
            Me.lvMovies.ListViewItemSorter = New ListViewItemComparer(1)
            Me.lvTVSources.ListViewItemSorter = New ListViewItemComparer(1)
            Me.sResult.NeedsUpdate = False
            Me.sResult.NeedsRefresh = False
            Me.sResult.DidCancel = False
            Me.didApply = False
            Me.NoUpdate = False
            RaiseEvent LoadEnd()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        Me.SetApplyButton(True)
    End Sub

    Private Sub Handle_ModuleSetupChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer)
        If Name = "!#RELOAD" Then
            Me.FillSettings()
            Return
        End If
        Dim tSetPan As New Containers.SettingsPanel
        Dim oSetPan As New Containers.SettingsPanel
        Me.SuspendLayout()
        tSetPan = SettingsPanels.FirstOrDefault(Function(s) s.Name = Name)

        If Not IsNothing(tSetPan) Then
            tSetPan.ImageIndex = If(State, 9, 10)

            Try
                'If tvSettings.Nodes.Count > 0 AndAlso tvSettings.Nodes(0).TreeView.IsDisposed Then Return 'Dont know yet why we need this. second call to settings will raise Exception with treview been disposed
                If Not IsNothing(tvSettings.Nodes.Find(Name, True)(0)) Then
                    Dim t As TreeNode = tvSettings.Nodes.Find(Name, True)(0)
                    If Not diffOrder = 0 Then
                        Dim p As TreeNode = t.Parent
                        Dim i As Integer = t.Index
                        If diffOrder < 0 AndAlso Not t.PrevNode Is Nothing Then
                            oSetPan = SettingsPanels.FirstOrDefault(Function(s) s.Name = t.PrevNode.Name)
                            If Not IsNothing(oSetPan) Then oSetPan.Order = i + (diffOrder * -1)
                        End If
                        If diffOrder > 0 AndAlso Not t.NextNode Is Nothing Then
                            oSetPan = SettingsPanels.FirstOrDefault(Function(s) s.Name = t.NextNode.Name)
                            If Not IsNothing(oSetPan) Then oSetPan.Order = i + (diffOrder * -1)
                        End If
                        p.Nodes.Remove(t)
                        p.Nodes.Insert(i + diffOrder, t)
                        t.TreeView.SelectedNode = t
                        tSetPan.Order = i + diffOrder
                    End If
                    t.ImageIndex = If(State, 9, 10)
                    t.SelectedImageIndex = If(State, 9, 10)
                    Me.pbCurrent.Image = Me.ilSettings.Images(If(State, 9, 10))
                End If

                For Each s As ModulesManager._externalScraperModuleClass In (ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.AssemblyName <> Name))
                    s.ProcessorModule.ScraperOrderChanged()
                    s.ProcessorModule.PostScraperOrderChanged()
                Next

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If
        Me.ResumeLayout()
        Me.SetApplyButton(True)
    End Sub

    Private Sub HelpMouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.lblHelp.Text = dHelp.Item(DirectCast(sender, Control).AccessibleDescription)
    End Sub

    Private Sub HelpMouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.lblHelp.Text = String.Empty
    End Sub

    Private Sub lbGenre_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lbGenre.ItemCheck
        If e.Index = 0 Then
            For i As Integer = 1 To lbGenre.Items.Count - 1
                Me.lbGenre.SetItemChecked(i, False)
            Next
        Else
            Me.lbGenre.SetItemChecked(0, False)
        End If
        Me.SetApplyButton(True)
    End Sub

    Private Sub LoadGenreLangs()
        Me.lbGenre.Items.Add(Master.eLang.All)
        Me.lbGenre.Items.AddRange(APIXML.GetGenreList(True))
    End Sub

    Private Sub LoadIntLangs()
        Me.cbIntLang.Items.Clear()
        If Directory.Exists(Path.Combine(Functions.AppPath, "Langs")) Then
            Dim alL As New List(Of String)
            Dim alLangs As New List(Of String)
            Try
                alL.AddRange(Directory.GetFiles(Path.Combine(Functions.AppPath, "Langs"), "*).xml"))
            Catch
            End Try
            alLangs.AddRange(alL.Cast(Of String)().Select(Function(AL) Path.GetFileNameWithoutExtension(AL)).ToList)
            Me.cbIntLang.Items.AddRange(alLangs.ToArray)
        End If
    End Sub

    Private Sub LoadLangs()
        Me.cbLanguages.Items.Add(Master.eLang.Disabled)
        Me.cbLanguages.Items.AddRange(Localization.ISOLangGetLanguagesList.ToArray)
        Me.cboTVMetaDataOverlay.Items.Add(Master.eLang.Disabled)
        Me.cboTVMetaDataOverlay.Items.AddRange(Localization.ISOLangGetLanguagesList.ToArray)
    End Sub

    Private Sub LoadMetadata()
        Me.lstMetaData.Items.Clear()
        For Each x As Settings.MetadataPerType In Meta
            Me.lstMetaData.Items.Add(x.FileType)
        Next
    End Sub

    Private Sub LoadRatingRegions()
        Me.cbRatingRegion.Items.AddRange(APIXML.GetRatingRegions)
    End Sub

    Private Sub LoadShowRegex()
        Dim lvItem As ListViewItem
        lvShowRegex.Items.Clear()
        For Each rShow As Settings.TVShowRegEx In Me.ShowRegex
            lvItem = New ListViewItem(rShow.ID.ToString)
            lvItem.SubItems.Add(rShow.SeasonRegex)
            lvItem.SubItems.Add(If(rShow.SeasonFromDirectory, "Folder", "File"))
            lvItem.SubItems.Add(rShow.EpisodeRegex)
            Select Case rShow.EpisodeRetrieve
                Case Settings.EpRetrieve.FromDirectory
                    lvItem.SubItems.Add("Folder")
                Case Settings.EpRetrieve.FromFilename
                    lvItem.SubItems.Add("File")
                Case Settings.EpRetrieve.FromSeasonResult
                    lvItem.SubItems.Add("Result")
            End Select
            Me.lvShowRegex.Items.Add(lvItem)
        Next
    End Sub

    Private Sub LoadThemes()
        Me.cbMovieTheme.Items.Clear()
        Me.cbTVShowTheme.Items.Clear()
        Me.cbEpTheme.Items.Clear()
        If Directory.Exists(Path.Combine(Functions.AppPath, "Themes")) Then
            Dim mT As New List(Of String)
            Dim sT As New List(Of String)
            Dim eT As New List(Of String)
            Try
                mT.AddRange(Directory.GetFiles(Path.Combine(Functions.AppPath, "Themes"), "movie-*.xml"))
            Catch
            End Try
            Me.cbMovieTheme.Items.AddRange(mT.Cast(Of String)().Select(Function(AL) Path.GetFileNameWithoutExtension(AL).Replace("movie-", String.Empty)).ToArray)
            Try
                sT.AddRange(Directory.GetFiles(Path.Combine(Functions.AppPath, "Themes"), "tvshow-*.xml"))
            Catch
            End Try
            Me.cbTVShowTheme.Items.AddRange(sT.Cast(Of String)().Select(Function(AL) Path.GetFileNameWithoutExtension(AL).Replace("tvshow-", String.Empty)).ToArray)
            Try
                eT.AddRange(Directory.GetFiles(Path.Combine(Functions.AppPath, "Themes"), "tvep-*.xml"))
            Catch
            End Try
            Me.cbEpTheme.Items.AddRange(eT.Cast(Of String)().Select(Function(AL) Path.GetFileNameWithoutExtension(AL).Replace("tvep-", String.Empty)).ToArray)
        End If
    End Sub

    Private Sub LoadTrailerQualities()
        Dim items As New Dictionary(Of String, Enums.TrailerQuality)
        items.Add("1080p", Enums.TrailerQuality.HD1080p)
        items.Add("1080p (VP8)", Enums.TrailerQuality.HD1080pVP8)
        items.Add("720p", Enums.TrailerQuality.HD720p)
        items.Add("720p (VP8)", Enums.TrailerQuality.HD720pVP8)
        items.Add("SQ (MP4)", Enums.TrailerQuality.SQMP4)
        items.Add("HQ (FLV)", Enums.TrailerQuality.HQFLV)
        items.Add("HQ (VP8)", Enums.TrailerQuality.HQVP8)
        items.Add("SQ (FLV)", Enums.TrailerQuality.SQFLV)
        items.Add("SQ (VP8)", Enums.TrailerQuality.SQVP8)
        Me.cbTrailerQuality.DataSource = items.ToList
        Me.cbTrailerQuality.DisplayMember = "Key"
        Me.cbTrailerQuality.ValueMember = "Value"
    End Sub

    Private Sub LoadTVMetadata()
        Me.lstTVMetaData.Items.Clear()
        For Each x As Settings.MetadataPerType In TVMeta
            Me.lstTVMetaData.Items.Add(x.FileType)
        Next
    End Sub

    Private Sub lstEpFilters_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstEpFilters.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveEpFilter()
    End Sub

    Private Sub lstFilters_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstFilters.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveFilter()
    End Sub

    Private Sub lstMetaData_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstMetaData.DoubleClick
        If Me.lstMetaData.SelectedItems.Count > 0 Then
            Using dEditMeta As New dlgFileInfo
                Dim fi As New MediaInfo.Fileinfo
                For Each x As Settings.MetadataPerType In Meta
                    If x.FileType = lstMetaData.SelectedItems(0).ToString Then
                        fi = dEditMeta.ShowDialog(x.MetaData, False)
                        If Not fi Is Nothing Then
                            Meta.Remove(x)
                            Dim m As New Settings.MetadataPerType
                            m.FileType = x.FileType
                            m.MetaData = New MediaInfo.Fileinfo
                            m.MetaData = fi
                            Meta.Add(m)
                            LoadMetadata()
                            Me.SetApplyButton(True)
                        End If
                        Exit For
                    End If
                Next
            End Using
        End If
    End Sub

    Private Sub lstMetaData_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstMetaData.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveMetaData()
    End Sub

    Private Sub lstMetadata_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstMetaData.SelectedIndexChanged
        If lstMetaData.SelectedItems.Count > 0 Then
            btnEditMetaDataFT.Enabled = True
            btnRemoveMetaDataFT.Enabled = True
            txtDefFIExt.Text = String.Empty
        Else
            btnEditMetaDataFT.Enabled = False
            btnRemoveMetaDataFT.Enabled = False
        End If
    End Sub

    Private Sub lstMovieExts_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstMovieExts.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveMovieExt()
    End Sub

    Private Sub lstNoStack_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstNoStack.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveNoStack()
    End Sub

    Private Sub lstShowFilters_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstShowFilters.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveShowFilter()
    End Sub

    Private Sub lstSortTokens_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstSortTokens.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveSortToken()
    End Sub

    Private Sub lstTVMetaData_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstTVMetaData.DoubleClick
        If Me.lstTVMetaData.SelectedItems.Count > 0 Then
            Using dEditMeta As New dlgFileInfo
                Dim fi As New MediaInfo.Fileinfo
                For Each x As Settings.MetadataPerType In TVMeta
                    If x.FileType = lstTVMetaData.SelectedItems(0).ToString Then
                        fi = dEditMeta.ShowDialog(x.MetaData, True)
                        If Not fi Is Nothing Then
                            TVMeta.Remove(x)
                            Dim m As New Settings.MetadataPerType
                            m.FileType = x.FileType
                            m.MetaData = New MediaInfo.Fileinfo
                            m.MetaData = fi
                            TVMeta.Add(m)
                            LoadTVMetadata()
                            Me.SetApplyButton(True)
                        End If
                        Exit For
                    End If
                Next
            End Using
        End If
    End Sub

    Private Sub lstTVMetaData_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstTVMetaData.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveTVMetaData()
    End Sub

    Private Sub lstTVMetadata_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstTVMetaData.SelectedIndexChanged
        If lstTVMetaData.SelectedItems.Count > 0 Then
            btnEditTVMetaDataFT.Enabled = True
            btnRemoveTVMetaDataFT.Enabled = True
            txtTVDefFIExt.Text = String.Empty
        Else
            btnEditTVMetaDataFT.Enabled = False
            btnRemoveTVMetaDataFT.Enabled = False
        End If
    End Sub

    Private Sub lvMovies_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lvMovies.ColumnClick
        Me.lvMovies.ListViewItemSorter = New ListViewItemComparer(e.Column)
    End Sub

    Private Sub lvMovies_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvMovies.DoubleClick
        If lvMovies.SelectedItems.Count > 0 Then
            Using dMovieSource As New dlgMovieSource
                If dMovieSource.ShowDialog(Convert.ToInt32(lvMovies.SelectedItems(0).Text)) = DialogResult.OK Then
                    Me.RefreshSources()
                    Me.sResult.NeedsUpdate = True
                    Me.SetApplyButton(True)
                End If
            End Using
        End If
    End Sub

    Private Sub lvMovies_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lvMovies.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveMovieSource()
    End Sub

    Private Sub lvShowRegex_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvShowRegex.DoubleClick
        If Me.lvShowRegex.SelectedItems.Count > 0 Then Me.EditShowRegex(lvShowRegex.SelectedItems(0))
    End Sub

    Private Sub lvShowRegex_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lvShowRegex.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveRegex()
    End Sub

    Private Sub lvShowRegex_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvShowRegex.SelectedIndexChanged
        If Not String.IsNullOrEmpty(Me.btnAddShowRegex.Tag.ToString) Then Me.ClearRegex()
    End Sub

    Private Sub lvTVSources_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lvTVSources.ColumnClick
        Me.lvTVSources.ListViewItemSorter = New ListViewItemComparer(e.Column)
    End Sub

    Private Sub lvTVSources_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvTVSources.DoubleClick
        If lvTVSources.SelectedItems.Count > 0 Then
            Using dTVSource As New dlgTVSource
                If dTVSource.ShowDialog(Convert.ToInt32(lvTVSources.SelectedItems(0).Text)) = DialogResult.OK Then
                    Me.RefreshTVSources()
                    Me.sResult.NeedsUpdate = True
                    Me.SetApplyButton(True)
                End If
            End Using
        End If
    End Sub

    Private Sub lvTVSources_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lvTVSources.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveTVSource()
    End Sub

    Private Sub rbAllSBanner_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbAllSBanner.CheckedChanged, rbAllSPoster.CheckedChanged
        Me.SetApplyButton(True)

        Me.cbAllSPosterSize.Items.Clear()

        If Me.rbAllSBanner.Checked Then
            Me.cbAllSPosterSize.Items.AddRange(New String() {Master.eLang.GetString(745, "None"), Master.eLang.GetString(746, "Blank"), Master.eLang.GetString(747, "Graphical"), Master.eLang.GetString(748, "Text")})
        Else
            Me.cbAllSPosterSize.Items.AddRange(New String() {Master.eLang.GetString(322, "X-Large"), Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small"), Master.eLang.GetString(558, "Wide")})
        End If
        Me.cbAllSPosterSize.SelectedIndex = 0
    End Sub

    Private Sub rbBanner_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbBanner.CheckedChanged, rbPoster.CheckedChanged
        Me.SetApplyButton(True)

        Me.cbShowPosterSize.Items.Clear()

        If Me.rbBanner.Checked Then
            Me.cbShowPosterSize.Items.AddRange(New String() {Master.eLang.GetString(745, "None"), Master.eLang.GetString(746, "Blank"), Master.eLang.GetString(747, "Graphical"), Master.eLang.GetString(748, "Text")})
        Else
            Me.cbShowPosterSize.Items.AddRange(New String() {Master.eLang.GetString(322, "X-Large"), Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small"), Master.eLang.GetString(558, "Wide")})
        End If
        Me.cbShowPosterSize.SelectedIndex = 0
    End Sub

    Private Sub rbBracketTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbBracketTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub rbDashTrailer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbDashTrailer.CheckedChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub rbETCustom_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbETCustom.CheckedChanged
        Me.SetApplyButton(False)
        Me.txtETHeight.Enabled = Me.rbETCustom.Checked
        Me.txtETWidth.Enabled = Me.rbETCustom.Checked
        Me.chkETPadding.Enabled = Me.rbETCustom.Checked
    End Sub

    Private Sub rbETNative_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbETNative.CheckedChanged
        Me.SetApplyButton(False)
        If rbETNative.Checked Then
            Me.txtETHeight.Text = String.Empty
            Me.txtETWidth.Text = String.Empty
            Me.chkETPadding.Checked = False
        End If
    End Sub

    Private Sub RefreshEpFilters()
        Me.lstEpFilters.Items.Clear()
        Me.lstEpFilters.Items.AddRange(Master.eSettings.EpFilterCustom.ToArray)
    End Sub

    Private Sub RefreshHelpStrings(ByVal Language As String)
        For Each sKey As String In dHelp.Keys
            dHelp.Item(sKey) = Master.eLang.GetHelpString(sKey)
        Next
    End Sub

    Private Sub RefreshMovieFilters()
        Me.lstFilters.Items.Clear()
        Me.lstFilters.Items.AddRange(Master.eSettings.FilterCustom.ToArray)
    End Sub

    Private Sub RefreshShowFilters()
        Me.lstShowFilters.Items.Clear()
        Me.lstShowFilters.Items.AddRange(Master.eSettings.ShowFilterCustom.ToArray)
    End Sub

    Private Sub RefreshSources()
        Dim lvItem As ListViewItem

        lvMovies.Items.Clear()
        Master.DB.LoadMovieSourcesFromDB()
        For Each s As Structures.MovieSource In Master.MovieSources
            lvItem = New ListViewItem(s.id)
            lvItem.SubItems.Add(s.Name)
            lvItem.SubItems.Add(s.Path)
            lvItem.SubItems.Add(If(s.Recursive, "Yes", "No"))
            lvItem.SubItems.Add(If(s.UseFolderName, "Yes", "No"))
            lvItem.SubItems.Add(If(s.IsSingle, "Yes", "No"))
            lvMovies.Items.Add(lvItem)
        Next
    End Sub

    Private Sub RefreshTVSources()
        Dim lvItem As ListViewItem
        Master.DB.LoadTVSourcesFromDB()
        lvTVSources.Items.Clear()
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLcommand.CommandText = "SELECT * FROM TVSources;"
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                While SQLreader.Read
                    lvItem = New ListViewItem(SQLreader("ID").ToString)
                    lvItem.SubItems.Add(SQLreader("Name").ToString)
                    lvItem.SubItems.Add(SQLreader("Path").ToString)
                    lvTVSources.Items.Add(lvItem)
                End While
            End Using
        End Using
    End Sub

    Private Sub RefreshValidExts()
        Me.lstMovieExts.Items.Clear()
        Me.lstMovieExts.Items.AddRange(Master.eSettings.ValidExts.ToArray)
    End Sub

    Private Sub RemoveCurrPanel()
        If Me.pnlMain.Controls.Count > 0 Then
            Me.pnlMain.Controls.Remove(Me.currPanel)
        End If
    End Sub

    Private Sub RemoveEpFilter()
        If Me.lstEpFilters.Items.Count > 0 AndAlso Me.lstEpFilters.SelectedItems.Count > 0 Then
            While Me.lstEpFilters.SelectedItems.Count > 0
                Me.lstEpFilters.Items.Remove(Me.lstEpFilters.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
            Me.sResult.NeedsRefresh = True
        End If
    End Sub

    Private Sub RemoveFilter()
        If Me.lstFilters.Items.Count > 0 AndAlso Me.lstFilters.SelectedItems.Count > 0 Then
            While Me.lstFilters.SelectedItems.Count > 0
                Me.lstFilters.Items.Remove(Me.lstFilters.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
            Me.sResult.NeedsRefresh = True
        End If
    End Sub

    Private Sub RemoveMetaData()
        If Me.lstMetaData.SelectedItems.Count > 0 Then
            For Each x As Settings.MetadataPerType In Meta
                If x.FileType = lstMetaData.SelectedItems(0).ToString Then
                    Meta.Remove(x)
                    LoadMetadata()
                    Me.SetApplyButton(True)
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub RemoveMovieExt()
        If lstMovieExts.Items.Count > 0 AndAlso lstMovieExts.SelectedItems.Count > 0 Then
            While Me.lstMovieExts.SelectedItems.Count > 0
                Me.lstMovieExts.Items.Remove(Me.lstMovieExts.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
            Me.sResult.NeedsUpdate = True
        End If
    End Sub

    Private Sub RemoveMovieSource()
        Try
            If Me.lvMovies.SelectedItems.Count > 0 Then
                If MsgBox(Master.eLang.GetString(418, "Are you sure you want to remove the selected sources? This will remove the movies from these sources from the Ember database."), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
                    Me.lvMovies.BeginUpdate()

                    Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parSource As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSource", DbType.String, 0, "source")
                            While Me.lvMovies.SelectedItems.Count > 0
                                parSource.Value = lvMovies.SelectedItems(0).SubItems(1).Text
                                SQLcommand.CommandText = "SELECT Id FROM movies WHERE source = (?);"
                                Using SQLReader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                                    While SQLReader.Read
                                        Master.DB.DeleteFromDB(Convert.ToInt64(SQLReader("ID")), True)
                                    End While
                                End Using
                                SQLcommand.CommandText = String.Concat("DELETE FROM sources WHERE name = (?);")
                                SQLcommand.ExecuteNonQuery()
                                lvMovies.Items.Remove(lvMovies.SelectedItems(0))
                            End While
                        End Using
                        SQLtransaction.Commit()
                    End Using

                    Me.lvMovies.Sort()
                    Me.lvMovies.EndUpdate()
                    Me.lvMovies.Refresh()

                    Functions.GetListOfSources()

                    Me.SetApplyButton(True)
                    Me.sResult.NeedsUpdate = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub RemoveNoStack()
        If lstNoStack.Items.Count > 0 AndAlso lstNoStack.SelectedItems.Count > 0 Then
            While Me.lstNoStack.SelectedItems.Count > 0
                Me.lstNoStack.Items.Remove(Me.lstNoStack.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
            Me.sResult.NeedsUpdate = True
        End If
    End Sub

    Private Sub RemoveRegex()
        Dim ID As Integer
        For Each lItem As ListViewItem In lvShowRegex.SelectedItems
            ID = Convert.ToInt32(lItem.Text)
            Dim selRex = From lRegex As Settings.TVShowRegEx In Me.ShowRegex Where lRegex.ID = ID
            If selRex.Count > 0 Then
                Me.ShowRegex.Remove(selRex(0))
                Me.SetApplyButton(True)
            End If
        Next
        Me.LoadShowRegex()
    End Sub

    Private Sub RemoveShowFilter()
        If Me.lstShowFilters.Items.Count > 0 AndAlso Me.lstShowFilters.SelectedItems.Count > 0 Then
            While Me.lstShowFilters.SelectedItems.Count > 0
                Me.lstShowFilters.Items.Remove(Me.lstShowFilters.SelectedItems(0))
            End While
            Me.SetApplyButton(True)
            Me.sResult.NeedsRefresh = True
        End If
    End Sub

    Private Sub RemoveSortToken()
        If Me.lstSortTokens.Items.Count > 0 AndAlso Me.lstSortTokens.SelectedItems.Count > 0 Then
            While Me.lstSortTokens.SelectedItems.Count > 0
                Me.lstSortTokens.Items.Remove(Me.lstSortTokens.SelectedItems(0))
            End While
            Me.sResult.NeedsRefresh = True
            Me.SetApplyButton(True)
        End If
    End Sub

    Private Sub RemoveTVMetaData()
        If Me.lstTVMetaData.SelectedItems.Count > 0 Then
            For Each x As Settings.MetadataPerType In TVMeta
                If x.FileType = lstTVMetaData.SelectedItems(0).ToString Then
                    TVMeta.Remove(x)
                    LoadTVMetadata()
                    Me.SetApplyButton(True)
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub RemoveTVSource()
        Try
            If Me.lvTVSources.SelectedItems.Count > 0 Then
                If MsgBox(Master.eLang.GetString(418, "Are you sure you want to remove the selected sources? This will remove the TV Shows from these sources from the Ember database."), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then
                    Me.lvTVSources.BeginUpdate()

                    Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parSource As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSource", DbType.String, 0, "source")
                            While Me.lvTVSources.SelectedItems.Count > 0
                                parSource.Value = lvTVSources.SelectedItems(0).SubItems(1).Text
                                SQLcommand.CommandText = "SELECT Id FROM TVShows WHERE Source = (?);"
                                Using SQLReader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                                    While SQLReader.Read
                                        Master.DB.DeleteTVShowFromDB(Convert.ToInt64(SQLReader("ID")), True)
                                    End While
                                End Using
                                SQLcommand.CommandText = String.Concat("DELETE FROM TVSources WHERE name = (?);")
                                SQLcommand.ExecuteNonQuery()
                                lvTVSources.Items.Remove(lvTVSources.SelectedItems(0))
                            End While
                        End Using
                        SQLtransaction.Commit()
                    End Using

                    Me.lvTVSources.Sort()
                    Me.lvTVSources.EndUpdate()
                    Me.lvTVSources.Refresh()
                    Me.SetApplyButton(True)
                    Me.sResult.NeedsUpdate = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub RenumberRegex()
        For i As Integer = 0 To Me.ShowRegex.Count - 1
            Me.ShowRegex(i).ID = i
        Next
    End Sub

    Private Sub SaveSettings(ByVal isApply As Boolean)
        Try
            Master.eSettings.FilterCustom.Clear()
            Master.eSettings.FilterCustom.AddRange(Me.lstFilters.Items.OfType(Of String).ToList)
            If Master.eSettings.FilterCustom.Count <= 0 Then Master.eSettings.NoFilters = True
            Master.eSettings.ProperCase = Me.chkProperCase.Checked

            Master.eSettings.ShowFilterCustom.Clear()
            Master.eSettings.ShowFilterCustom.AddRange(Me.lstShowFilters.Items.OfType(Of String).ToList)
            If Master.eSettings.ShowFilterCustom.Count <= 0 Then Master.eSettings.NoShowFilters = True
            Master.eSettings.ShowProperCase = Me.chkShowProperCase.Checked

            Master.eSettings.EpFilterCustom.Clear()
            Master.eSettings.EpFilterCustom.AddRange(Me.lstEpFilters.Items.OfType(Of String).ToList)
            If Master.eSettings.EpFilterCustom.Count <= 0 Then Master.eSettings.NoEpFilters = True
            Master.eSettings.EpProperCase = Me.chkEpProperCase.Checked

            If Me.tcCleaner.SelectedTab.Name = "tpExpert" Then
                Master.eSettings.ExpertCleaner = True
                Master.eSettings.CleanFolderJPG = False
                Master.eSettings.CleanMovieTBN = False
                Master.eSettings.CleanMovieTBNB = False
                Master.eSettings.CleanFanartJPG = False
                Master.eSettings.CleanMovieFanartJPG = False
                Master.eSettings.CleanMovieNFO = False
                Master.eSettings.CleanMovieNFOB = False
                Master.eSettings.CleanPosterTBN = False
                Master.eSettings.CleanPosterJPG = False
                Master.eSettings.CleanMovieJPG = False
                Master.eSettings.CleanMovieNameJPG = False
                Master.eSettings.CleanDotFanartJPG = False
                Master.eSettings.CleanExtraThumbs = False
                Master.eSettings.CleanWhitelistVideo = Me.chkWhitelistVideo.Checked
                Master.eSettings.CleanWhitelistExts.Clear()
                Master.eSettings.CleanWhitelistExts.AddRange(Me.lstWhitelist.Items.OfType(Of String).ToList)
            Else
                Master.eSettings.ExpertCleaner = False
                Master.eSettings.CleanFolderJPG = Me.chkCleanFolderJPG.Checked
                Master.eSettings.CleanMovieTBN = Me.chkCleanMovieTBN.Checked
                Master.eSettings.CleanMovieTBNB = Me.chkCleanMovieTBNb.Checked
                Master.eSettings.CleanFanartJPG = Me.chkCleanFanartJPG.Checked
                Master.eSettings.CleanMovieFanartJPG = Me.chkCleanMovieFanartJPG.Checked
                Master.eSettings.CleanMovieNFO = Me.chkCleanMovieNFO.Checked
                Master.eSettings.CleanMovieNFOB = Me.chkCleanMovieNFOb.Checked
                Master.eSettings.CleanPosterTBN = Me.chkCleanPosterTBN.Checked
                Master.eSettings.CleanPosterJPG = Me.chkCleanPosterJPG.Checked
                Master.eSettings.CleanMovieJPG = Me.chkCleanMovieJPG.Checked
                Master.eSettings.CleanMovieNameJPG = Me.chkCleanMovieNameJPG.Checked
                Master.eSettings.CleanDotFanartJPG = Me.chkCleanDotFanartJPG.Checked
                Master.eSettings.CleanExtraThumbs = Me.chkCleanExtrathumbs.Checked
                Master.eSettings.CleanWhitelistVideo = False
                Master.eSettings.CleanWhitelistExts.Clear()
            End If
            Master.eSettings.LogErrors = Me.chkLogErrors.Checked
            Master.eSettings.OverwriteNfo = Me.chkOverwriteNfo.Checked
            Master.eSettings.ValidExts.Clear()
            Master.eSettings.ValidExts.AddRange(lstMovieExts.Items.OfType(Of String).ToList)
            Master.eSettings.NoStackExts.Clear()
            Master.eSettings.NoStackExts.AddRange(lstNoStack.Items.OfType(Of String).ToList)
            Master.eSettings.CheckUpdates = chkUpdates.Checked
            Master.eSettings.InfoPanelAnim = chkInfoPanelAnim.Checked
            'Master.eSettings.YAMJSetsCompatible = chkYAMJCompatibleSets.Checked
            Master.eSettings.CertificationLang = Me.cbCert.Text
            If Not String.IsNullOrEmpty(Me.cbCert.Text) Then
                Master.eSettings.UseCertForMPAA = Me.chkUseCertForMPAA.Checked
            Else
                Master.eSettings.UseCertForMPAA = False
            End If
            Master.eSettings.ForceTitle = Me.cbForce.Text
            Master.eSettings.ScanMediaInfo = Me.chkScanMediaInfo.Checked
            Master.eSettings.ScanTVMediaInfo = Me.chkTVScanMetaData.Checked
            Master.eSettings.FullCast = Me.chkFullCast.Checked
            Master.eSettings.FullCrew = Me.chkFullCrew.Checked
            Master.eSettings.CastImagesOnly = Me.chkCastWithImg.Checked
            Master.eSettings.MoviePosterCol = Me.chkMoviePosterCol.Checked
            Master.eSettings.MovieFanartCol = Me.chkMovieFanartCol.Checked
            Master.eSettings.MovieInfoCol = Me.chkMovieInfoCol.Checked
            Master.eSettings.MovieTrailerCol = Me.chkMovieTrailerCol.Checked
            Master.eSettings.MovieSubCol = Me.chkMovieSubCol.Checked
            Master.eSettings.MovieExtraCol = Me.chkMovieExtraCol.Checked

            Master.eSettings.PreferredPosterSize = DirectCast(Me.cbPosterSize.SelectedIndex, Enums.PosterSize)
            Master.eSettings.PreferredFanartSize = DirectCast(Me.cbFanartSize.SelectedIndex, Enums.FanartSize)
            If Me.rbBanner.Checked Then
                Master.eSettings.IsShowBanner = True
                Master.eSettings.PreferredShowBannerType = DirectCast(Me.cbShowPosterSize.SelectedIndex, Enums.ShowBannerType)
            Else
                Master.eSettings.IsShowBanner = False
                Master.eSettings.PreferredShowPosterSize = DirectCast(Me.cbShowPosterSize.SelectedIndex, Enums.PosterSize)
            End If
            If Me.rbAllSBanner.Checked Then
                Master.eSettings.IsAllSBanner = True
                Master.eSettings.PreferredAllSBannerType = DirectCast(Me.cbAllSPosterSize.SelectedIndex, Enums.ShowBannerType)
            Else
                Master.eSettings.IsAllSBanner = False
                Master.eSettings.PreferredAllSPosterSize = DirectCast(Me.cbAllSPosterSize.SelectedIndex, Enums.PosterSize)
            End If
            Master.eSettings.PreferredShowFanartSize = DirectCast(Me.cbShowFanartSize.SelectedIndex, Enums.FanartSize)
            Master.eSettings.PreferredEpFanartSize = DirectCast(Me.cbEpFanartSize.SelectedIndex, Enums.FanartSize)
            Master.eSettings.PreferredSeasonPosterSize = DirectCast(Me.cbSeaPosterSize.SelectedIndex, Enums.SeasonPosterType)
            Master.eSettings.PreferredEpFanartSize = DirectCast(Me.cbSeaFanartSize.SelectedIndex, Enums.FanartSize)
            Master.eSettings.AutoET = Me.chkAutoETSize.Checked
            Master.eSettings.AutoETSize = DirectCast(Me.cbAutoETSize.SelectedIndex, Enums.FanartSize)
            Master.eSettings.FanartPrefSizeOnly = Me.chkFanartOnly.Checked
            Master.eSettings.PosterPrefSizeOnly = Me.chkPosterOnly.Checked
            Master.eSettings.PosterQuality = Me.tbPosterQual.Value
            Master.eSettings.FanartQuality = Me.tbFanartQual.Value
            Master.eSettings.OverwritePoster = Me.chkOverwritePoster.Checked
            Master.eSettings.OverwriteFanart = Me.chkOverwriteFanart.Checked
            Master.eSettings.ShowPosterQuality = Me.tbShowPosterQual.Value
            Master.eSettings.ShowFanartQuality = Me.tbShowFanartQual.Value
            Master.eSettings.OverwriteShowPoster = Me.chkOverwriteShowPoster.Checked
            Master.eSettings.OverwriteShowFanart = Me.chkOverwriteShowFanart.Checked
            Master.eSettings.AllSPosterQuality = Me.tbAllSPosterQual.Value
            Master.eSettings.OverwriteAllSPoster = Me.chkOverwriteAllSPoster.Checked
            Master.eSettings.EpPosterQuality = Me.tbEpPosterQual.Value
            Master.eSettings.EpFanartQuality = Me.tbEpFanartQual.Value
            Master.eSettings.OverwriteEpPoster = Me.chkOverwriteEpPoster.Checked
            Master.eSettings.OverwriteEpFanart = Me.chkOverwriteEpFanart.Checked
            Master.eSettings.SeasonPosterQuality = Me.tbSeaPosterQual.Value
            Master.eSettings.SeasonFanartQuality = Me.tbSeaFanartQual.Value
            Master.eSettings.OverwriteSeasonPoster = Me.chkSeaOverwritePoster.Checked
            Master.eSettings.OverwriteSeasonFanart = Me.chkSeaOverwriteFanart.Checked
            Master.eSettings.MovieTBN = Me.chkMovieTBN.Checked
            Master.eSettings.MovieNameTBN = Me.chkMovieNameTBN.Checked
            Master.eSettings.MovieJPG = Me.chkMovieJPG.Checked
            Master.eSettings.MovieNameJPG = Me.chkMovieNameJPG.Checked
            Master.eSettings.MovieNameDashPosterJPG = Me.chkMovieNameDashPosterJPG.Checked
            Master.eSettings.PosterTBN = Me.chkPosterTBN.Checked
            Master.eSettings.PosterJPG = Me.chkPosterJPG.Checked
            Master.eSettings.FolderJPG = Me.chkFolderJPG.Checked
            Master.eSettings.FanartJPG = Me.chkFanartJPG.Checked
            Master.eSettings.MovieNameFanartJPG = Me.chkMovieNameFanartJPG.Checked
            Master.eSettings.MovieNameDotFanartJPG = Me.chkMovieNameDotFanartJPG.Checked
            Master.eSettings.MovieNFO = Me.chkMovieNFO.Checked
            Master.eSettings.MovieNameNFO = Me.chkMovieNameNFO.Checked
            Master.eSettings.MovieNameMultiOnly = Me.chkMovieNameMultiOnly.Checked
            Master.eSettings.DashTrailer = Me.rbDashTrailer.Checked
            'Master.eSettings.VideoTSParent = Me.chkVideoTSParent.Checked
            Master.eSettings.LockPlot = Me.chkLockPlot.Checked
            Master.eSettings.LockOutline = Me.chkLockOutline.Checked
            Master.eSettings.LockTitle = Me.chkLockTitle.Checked
            Master.eSettings.LockTagline = Me.chkLockTagline.Checked
            Master.eSettings.LockRating = Me.chkLockRating.Checked
            Master.eSettings.LockStudio = Me.chkLockRealStudio.Checked
            Master.eSettings.LockGenre = Me.chkLockGenre.Checked
            Master.eSettings.LockTrailer = Me.chkLockTrailer.Checked
            Master.eSettings.SingleScrapeImages = Me.chkSingleScrapeImages.Checked
            Master.eSettings.ClickScrape = Me.chkClickScrape.Checked
            Master.eSettings.AskCheckboxScrape = Me.chkAskCheckboxScrape.Checked
            Master.eSettings.MarkNew = Me.chkMarkNew.Checked
            Master.eSettings.ResizeFanart = Me.chkResizeFanart.Checked
            Master.eSettings.FanartHeight = If(Not String.IsNullOrEmpty(Me.txtFanartHeight.Text), Convert.ToInt32(Me.txtFanartHeight.Text), 0)
            Master.eSettings.FanartWidth = If(Not String.IsNullOrEmpty(Me.txtFanartWidth.Text), Convert.ToInt32(Me.txtFanartWidth.Text), 0)
            Master.eSettings.ResizePoster = Me.chkResizePoster.Checked
            Master.eSettings.PosterHeight = If(Not String.IsNullOrEmpty(Me.txtPosterHeight.Text), Convert.ToInt32(Me.txtPosterHeight.Text), 0)
            Master.eSettings.PosterWidth = If(Not String.IsNullOrEmpty(Me.txtPosterWidth.Text), Convert.ToInt32(Me.txtPosterWidth.Text), 0)
            Master.eSettings.ResizeShowFanart = Me.chkResizeShowFanart.Checked
            Master.eSettings.ShowFanartHeight = If(Not String.IsNullOrEmpty(Me.txtShowFanartHeight.Text), Convert.ToInt32(Me.txtShowFanartHeight.Text), 0)
            Master.eSettings.ShowFanartWidth = If(Not String.IsNullOrEmpty(Me.txtShowFanartWidth.Text), Convert.ToInt32(Me.txtShowFanartWidth.Text), 0)
            Master.eSettings.ResizeShowPoster = Me.chkResizeShowPoster.Checked
            Master.eSettings.ShowPosterHeight = If(Not String.IsNullOrEmpty(Me.txtShowPosterHeight.Text), Convert.ToInt32(Me.txtShowPosterHeight.Text), 0)
            Master.eSettings.ShowPosterWidth = If(Not String.IsNullOrEmpty(Me.txtShowPosterWidth.Text), Convert.ToInt32(Me.txtShowPosterWidth.Text), 0)
            Master.eSettings.ResizeAllSPoster = Me.chkResizeAllSPoster.Checked
            Master.eSettings.AllSPosterHeight = If(Not String.IsNullOrEmpty(Me.txtAllSPosterHeight.Text), Convert.ToInt32(Me.txtAllSPosterHeight.Text), 0)
            Master.eSettings.AllSPosterWidth = If(Not String.IsNullOrEmpty(Me.txtAllSPosterWidth.Text), Convert.ToInt32(Me.txtAllSPosterWidth.Text), 0)
            Master.eSettings.ResizeEpFanart = Me.chkResizeEpFanart.Checked
            Master.eSettings.EpFanartHeight = If(Not String.IsNullOrEmpty(Me.txtEpFanartHeight.Text), Convert.ToInt32(Me.txtEpFanartHeight.Text), 0)
            Master.eSettings.EpFanartWidth = If(Not String.IsNullOrEmpty(Me.txtEpFanartWidth.Text), Convert.ToInt32(Me.txtEpFanartWidth.Text), 0)
            Master.eSettings.ResizeEpPoster = Me.chkResizeEpPoster.Checked
            Master.eSettings.EpPosterHeight = If(Not String.IsNullOrEmpty(Me.txtEpPosterHeight.Text), Convert.ToInt32(Me.txtEpPosterHeight.Text), 0)
            Master.eSettings.EpPosterWidth = If(Not String.IsNullOrEmpty(Me.txtEpPosterWidth.Text), Convert.ToInt32(Me.txtEpPosterWidth.Text), 0)
            Master.eSettings.ResizeSeasonFanart = Me.chkSeaResizeFanart.Checked
            Master.eSettings.SeasonFanartHeight = If(Not String.IsNullOrEmpty(Me.txtSeaFanartHeight.Text), Convert.ToInt32(Me.txtSeaFanartHeight.Text), 0)
            Master.eSettings.SeasonFanartWidth = If(Not String.IsNullOrEmpty(Me.txtSeaFanartWidth.Text), Convert.ToInt32(Me.txtSeaFanartWidth.Text), 0)
            Master.eSettings.ResizeSeasonPoster = Me.chkSeaResizePoster.Checked
            Master.eSettings.SeasonPosterHeight = If(Not String.IsNullOrEmpty(Me.txtSeaPosterHeight.Text), Convert.ToInt32(Me.txtSeaPosterHeight.Text), 0)
            Master.eSettings.SeasonPosterWidth = If(Not String.IsNullOrEmpty(Me.txtSeaPosterWidth.Text), Convert.ToInt32(Me.txtSeaPosterWidth.Text), 0)

            If Not String.IsNullOrEmpty(txtAutoThumbs.Text) AndAlso Convert.ToInt32(txtAutoThumbs.Text) > 0 Then
                Master.eSettings.AutoThumbs = Convert.ToInt32(txtAutoThumbs.Text)
                Master.eSettings.AutoThumbsNoSpoilers = Me.chkNoSpoilers.Checked
                Master.eSettings.UseETasFA = Me.chkUseETasFA.Checked
            Else
                Master.eSettings.AutoThumbs = 0
                Master.eSettings.AutoThumbsNoSpoilers = False
                Master.eSettings.UseETasFA = False
            End If
            Master.eSettings.BDPath = Me.txtBDPath.Text
            If Not String.IsNullOrEmpty(Me.txtBDPath.Text) Then
                Master.eSettings.AutoBD = Me.chkAutoBD.Checked
            Else
                Master.eSettings.AutoBD = False
            End If
            Master.eSettings.UseMIDuration = Me.chkUseMIDuration.Checked
            Master.eSettings.RuntimeMask = Me.txtRuntimeFormat.Text
            Master.eSettings.UseImgCache = Me.chkUseImgCache.Checked
            Master.eSettings.UseImgCacheUpdaters = Me.chkUseImgCacheUpdaters.Checked
            Master.eSettings.PersistImgCache = Me.chkPersistImgCache.Checked
            Master.eSettings.SkipLessThan = Convert.ToInt32(Me.txtSkipLessThan.Text)
            Master.eSettings.SkipStackSizeCheck = Me.chkSkipStackedSizeCheck.Checked
            Master.eSettings.SkipLessThanEp = Convert.ToInt32(Me.txtTVSkipLessThan.Text)
            Master.eSettings.NoSaveImagesToNfo = Me.chkNoSaveImagesToNfo.Checked

            If Me.cbTrailerQuality.SelectedValue IsNot Nothing Then
                Master.eSettings.PreferredTrailerQuality = DirectCast(Me.cbTrailerQuality.SelectedValue, Enums.TrailerQuality)
            End If

            Master.eSettings.DownloadTrailers = Me.chkDownloadTrailer.Checked
            Master.eSettings.UpdaterTrailers = Me.chkUpdaterTrailer.Checked

            Master.eSettings.SingleScrapeTrailer = Me.chkSingleScrapeTrailer.Checked
            Master.eSettings.UpdaterTrailersNoDownload = Me.chkNoDLTrailer.Checked
            Master.eSettings.OverwriteTrailer = Me.chkOverwriteTrailer.Checked
            Master.eSettings.DeleteAllTrailers = Me.chkDeleteAllTrailers.Checked


            If Me.lbGenre.CheckedItems.Count > 0 Then
                If Me.lbGenre.CheckedItems.Contains(String.Format("{0}", Master.eLang.GetString(569, Master.eLang.All))) Then
                    Master.eSettings.GenreFilter = String.Format("{0}", Master.eLang.GetString(569, Master.eLang.All))
                Else
                    Dim strGenre As String = String.Empty
                    Dim iChecked = From iCheck In Me.lbGenre.CheckedItems
                    strGenre = Strings.Join(iChecked.ToArray, ",")
                    Master.eSettings.GenreFilter = strGenre.Trim
                End If
            End If

            Master.eSettings.ShowDims = Me.chkShowDims.Checked
            Master.eSettings.NoDisplayFanart = Me.chkNoDisplayFanart.Checked
            Master.eSettings.NoDisplayPoster = Me.chkNoDisplayPoster.Checked
            Master.eSettings.OutlineForPlot = Me.chkOutlineForPlot.Checked

            Master.eSettings.AllwaysDisplayGenresText = Me.chkShowGenresText.Checked
            Master.eSettings.DisplayYear = Me.chkDisplayYear.Checked
            Master.eSettings.ETNative = Me.rbETNative.Checked
            Dim iWidth As Integer = If(Me.txtETWidth.Text.Length > 0, Convert.ToInt32(Me.txtETWidth.Text), 0)
            Dim iHeight As Integer = If(Me.txtETHeight.Text.Length > 0, Convert.ToInt32(Me.txtETHeight.Text), 0)
            If Me.rbETCustom.Checked AndAlso iWidth > 0 AndAlso iHeight > 0 Then
                Master.eSettings.ETWidth = iWidth
                Master.eSettings.ETHeight = iHeight
                Master.eSettings.ETPadding = Me.chkETPadding.Checked
            Else
                Master.eSettings.ETWidth = 0
                Master.eSettings.ETHeight = 0
                Master.eSettings.ETPadding = False
            End If

            Master.eSettings.SortTokens.Clear()
            Master.eSettings.SortTokens.AddRange(lstSortTokens.Items.OfType(Of String).ToList)
            If Master.eSettings.SortTokens.Count <= 0 Then Master.eSettings.NoTokens = True

            Master.eSettings.LevTolerance = If(Not String.IsNullOrEmpty(Me.txtCheckTitleTol.Text), Convert.ToInt32(Me.txtCheckTitleTol.Text), 0)
            Master.eSettings.AutoDetectVTS = Me.chkAutoDetectVTS.Checked
            Master.eSettings.FlagLang = If(Me.cbLanguages.Text = Master.eLang.Disabled, String.Empty, Me.cbLanguages.Text)
            Master.eSettings.TVFlagLang = If(Me.cboTVMetaDataOverlay.Text = Master.eLang.Disabled, String.Empty, Me.cboTVMetaDataOverlay.Text)
            Master.eSettings.Language = Me.cbIntLang.Text
            Me.lbGenre.Items.Clear()
            LoadGenreLangs()
            FillGenres()
            Master.eSettings.FieldTitle = Me.chkTitle.Checked
            Master.eSettings.FieldYear = Me.chkYear.Checked
            Master.eSettings.FieldMPAA = Me.chkMPAA.Checked
            Master.eSettings.FieldCert = Me.chkCertification.Checked
            Master.eSettings.FieldRelease = Me.chkRelease.Checked
            Master.eSettings.FieldRuntime = Me.chkRuntime.Checked
            Master.eSettings.FieldRating = Me.chkRating.Checked
            Master.eSettings.FieldVotes = Me.chkVotes.Checked
            Master.eSettings.FieldStudio = Me.chkStudio.Checked
            Master.eSettings.FieldGenre = Me.chkGenre.Checked
            Master.eSettings.FieldTrailer = Me.chkTrailer.Checked
            Master.eSettings.FieldTagline = Me.chkTagline.Checked
            Master.eSettings.FieldOutline = Me.chkOutline.Checked
            Master.eSettings.FieldPlot = Me.chkPlot.Checked
            Master.eSettings.FieldCast = Me.chkCast.Checked
            Master.eSettings.FieldDirector = Me.chkDirector.Checked
            Master.eSettings.FieldWriters = Me.chkWriters.Checked
            Master.eSettings.FieldProducers = Me.chkProducers.Checked
            Master.eSettings.FieldMusic = Me.chkMusicBy.Checked
            Master.eSettings.FieldCrew = Me.chkCrew.Checked
            Master.eSettings.Field250 = Me.chkTop250.Checked
            Master.eSettings.FieldCountry = Me.chkCountry.Checked

            If Not String.IsNullOrEmpty(Me.txtActorLimit.Text) Then
                Master.eSettings.ActorLimit = Convert.ToInt32(Me.txtActorLimit.Text)
            Else
                Master.eSettings.ActorLimit = 0
            End If
            If Not String.IsNullOrEmpty(Me.txtGenreLimit.Text) Then
                Master.eSettings.GenreLimit = Convert.ToInt32(Me.txtGenreLimit.Text)
            Else
                Master.eSettings.GenreLimit = 0
            End If

            Master.eSettings.MissingFilterPoster = Me.chkMissingPoster.Checked
            Master.eSettings.MissingFilterFanart = Me.chkMissingFanart.Checked
            Master.eSettings.MissingFilterNFO = Me.chkMissingNFO.Checked
            Master.eSettings.MissingFilterTrailer = Me.chkMissingTrailer.Checked
            Master.eSettings.MissingFilterSubs = Me.chkMissingSubs.Checked
            Master.eSettings.MissingFilterExtras = Me.chkMissingExtra.Checked

            Master.eSettings.MovieTheme = Me.cbMovieTheme.Text
            Master.eSettings.TVShowTheme = Me.cbTVShowTheme.Text
            Master.eSettings.TVEpTheme = Me.cbEpTheme.Text
            Master.eSettings.MetadataPerFileType.Clear()
            Master.eSettings.MetadataPerFileType.AddRange(Me.Meta)
            Master.eSettings.TVMetadataperFileType.Clear()
            Master.eSettings.TVMetadataperFileType.AddRange(Me.TVMeta)
            Master.eSettings.EnableIFOScan = Me.chkIFOScan.Checked
            Master.eSettings.CleanDB = Me.chkCleanDB.Checked
            Master.eSettings.IgnoreLastScan = Me.chkIgnoreLastScan.Checked
            Master.eSettings.TVCleanDB = Me.chkTVCleanDB.Checked
            Master.eSettings.TVIgnoreLastScan = Me.chkTVIgnoreLastScan.Checked
            Master.eSettings.TVShowRegexes.Clear()
            Master.eSettings.TVShowRegexes.AddRange(Me.ShowRegex)
            If String.IsNullOrEmpty(Me.cbRatingRegion.Text) Then
                Master.eSettings.ShowRatingRegion = "usa"
            Else
                Master.eSettings.ShowRatingRegion = Me.cbRatingRegion.Text
            End If
            Master.eSettings.SeasonAllTBN = Me.chkSeasonAllTBN.Checked
            Master.eSettings.SeasonAllJPG = Me.chkSeasonAllJPG.Checked
            Master.eSettings.SeasonAllPosterJPG = Me.chkSeasonAllPosterJPG.Checked
            Master.eSettings.ShowTBN = Me.chkShowTBN.Checked
            Master.eSettings.ShowJPG = Me.chkShowJPG.Checked
            Master.eSettings.ShowFolderJPG = Me.chkShowFolderJPG.Checked
            Master.eSettings.ShowPosterTBN = Me.chkShowPosterTBN.Checked
            Master.eSettings.ShowPosterJPG = Me.chkShowPosterJPG.Checked
            Master.eSettings.ShowBannerJPG = Me.chkShowBannerJPG.Checked
            Master.eSettings.ShowFanartJPG = Me.chkShowFanartJPG.Checked
            Master.eSettings.ShowDashFanart = Me.chkShowDashFanart.Checked
            Master.eSettings.ShowDotFanart = Me.chkShowDotFanart.Checked
            Master.eSettings.SeasonXX = Me.chkSeasonXXTBN.Checked
            Master.eSettings.SeasonX = Me.chkSeasonXTBN.Checked
            Master.eSettings.SeasonXXDashPosterJPG = Me.chkSeasonXXDashPosterJPG.Checked
            Master.eSettings.SeasonPosterTBN = Me.chkSeasonPosterTBN.Checked
            Master.eSettings.SeasonPosterJPG = Me.chkSeasonPosterJPG.Checked
            Master.eSettings.SeasonNameTBN = Me.chkSeasonNameTBN.Checked
            Master.eSettings.SeasonNameJPG = Me.chkSeasonNameJPG.Checked
            Master.eSettings.SeasonFolderJPG = Me.chkSeasonFolderJPG.Checked
            Master.eSettings.SeasonFanartJPG = Me.chkSeasonFanartJPG.Checked
            Master.eSettings.SeasonDashFanart = Me.chkSeasonDashFanart.Checked
            Master.eSettings.SeasonXXDashFanartJPG = Me.chkSeasonXXDashFanartJPG.Checked
            Master.eSettings.SeasonDotFanart = Me.chkSeasonDotFanart.Checked
            Master.eSettings.EpisodeTBN = Me.chkEpisodeTBN.Checked
            Master.eSettings.EpisodeJPG = Me.chkEpisodeJPG.Checked
            Master.eSettings.EpisodeDashThumbJPG = Me.chkEpisodeDashThumbJPG.Checked
            Master.eSettings.EpisodeDashFanart = Me.chkEpisodeDashFanart.Checked
            Master.eSettings.EpisodeDotFanart = Me.chkEpisodeDotFanart.Checked
            Master.eSettings.ShowPosterCol = Me.chkShowPosterCol.Checked
            Master.eSettings.ShowFanartCol = Me.chkShowFanartCol.Checked
            Master.eSettings.ShowNfoCol = Me.chkShowNfoCol.Checked
            Master.eSettings.SeasonPosterCol = Me.chkSeasonPosterCol.Checked
            Master.eSettings.SeasonFanartCol = Me.chkSeasonFanartCol.Checked
            Master.eSettings.EpisodePosterCol = Me.chkEpisodePosterCol.Checked
            Master.eSettings.EpisodeFanartCol = Me.chkEpisodeFanartCol.Checked
            Master.eSettings.EpisodeNfoCol = Me.chkEpisodeNfoCol.Checked
            Master.eSettings.SourceFromFolder = Me.chkSourceFromFolder.Checked
            Master.eSettings.SortBeforeScan = Me.chkSortBeforeScan.Checked
            If tLangList.Count > 0 Then
                Dim tLang As String = tLangList.FirstOrDefault(Function(l) l.LongLang = Me.cbTVLanguage.Text).ShortLang
                If Not String.IsNullOrEmpty(tLang) Then
                    Master.eSettings.TVDBLanguage = tLang
                Else
                    Master.eSettings.TVDBLanguage = "en"
                End If
            Else
                Master.eSettings.TVDBLanguage = "en"
            End If
            Master.eSettings.TVDBLanguages = Me.tLangList
            If Not String.IsNullOrEmpty(Me.txtTVDBMirror.Text) Then
                Master.eSettings.TVDBMirror = Strings.Replace(Me.txtTVDBMirror.Text, "http://", String.Empty)
            Else
                Master.eSettings.TVDBMirror = "thetvdb.com"
            End If

            If Not String.IsNullOrEmpty(Me.txtProxyURI.Text) AndAlso Not String.IsNullOrEmpty(Me.txtProxyPort.Text) Then
                Master.eSettings.ProxyURI = Me.txtProxyURI.Text
                Master.eSettings.ProxyPort = Convert.ToInt32(Me.txtProxyPort.Text)

                If Not String.IsNullOrEmpty(Me.txtProxyUsername.Text) AndAlso Not String.IsNullOrEmpty(Me.txtProxyPassword.Text) Then
                    Master.eSettings.ProxyCreds.UserName = Me.txtProxyUsername.Text
                    Master.eSettings.ProxyCreds.Password = Me.txtProxyPassword.Text
                    Master.eSettings.ProxyCreds.Domain = Me.txtProxyDomain.Text
                Else
                    Master.eSettings.ProxyCreds = New NetworkCredential
                End If
            Else
                Master.eSettings.ProxyURI = String.Empty
                Master.eSettings.ProxyPort = -1
            End If

            Master.eSettings.ExternalTVDBAPIKey = Me.txtAPIKey.Text
            Master.eSettings.ScanOrderModify = Me.chkScanOrderModify.Checked
            Master.eSettings.TVScanOrderModify = Me.chkTVScanOrderModify.Checked
            Master.eSettings.TVUpdateTime = DirectCast(Me.cboTVUpdate.SelectedIndex, Enums.TVUpdateTime)
            Master.eSettings.NoFilterEpisode = Me.chkNoFilterEpisode.Checked
            Master.eSettings.OnlyGetTVImagesForSelectedLanguage = Me.chkOnlyTVImagesLanguage.Checked
            Master.eSettings.AlwaysGetEnglishTVImages = Me.chkGetEnglishImages.Checked
            Master.eSettings.DisplayMissingEpisodes = Me.chkDisplayMissingEpisodes.Checked
            Master.eSettings.ShowLockTitle = Me.chkShowLockTitle.Checked
            Master.eSettings.ShowLockPlot = Me.chkShowLockPlot.Checked
            Master.eSettings.ShowLockRating = Me.chkShowLockRating.Checked
            Master.eSettings.ShowLockGenre = Me.chkShowLockGenre.Checked
            Master.eSettings.ShowLockStudio = Me.chkShowLockStudio.Checked
            Master.eSettings.EpLockTitle = Me.chkEpLockTitle.Checked
            Master.eSettings.EpLockPlot = Me.chkEpLockPlot.Checked
            Master.eSettings.EpLockRating = Me.chkEpLockRating.Checked
            Master.eSettings.ScraperShowTitle = Me.chkScraperShowTitle.Checked
            Master.eSettings.ScraperShowEGU = Me.chkScraperShowEGU.Checked
            Master.eSettings.ScraperShowGenre = Me.chkScraperShowGenre.Checked
            Master.eSettings.ScraperShowMPAA = Me.chkScraperShowMPAA.Checked
            Master.eSettings.ScraperShowPlot = Me.chkScraperShowPlot.Checked
            Master.eSettings.ScraperShowPremiered = Me.chkScraperShowPremiered.Checked
            Master.eSettings.ScraperShowRating = Me.chkScraperShowRating.Checked
            Master.eSettings.ScraperShowStudio = Me.chkScraperShowStudio.Checked
            Master.eSettings.ScraperShowActors = Me.chkScraperShowActors.Checked
            Master.eSettings.ScraperEpTitle = Me.chkScraperEpTitle.Checked
            Master.eSettings.ScraperEpSeason = Me.chkScraperEpSeason.Checked
            Master.eSettings.ScraperEpEpisode = Me.chkScraperEpEpisode.Checked
            Master.eSettings.ScraperEpAired = Me.chkScraperEpAired.Checked
            Master.eSettings.ScraperEpRating = Me.chkScraperEpRating.Checked
            Master.eSettings.ScraperEpPlot = Me.chkScraperEpPlot.Checked
            Master.eSettings.ScraperEpDirector = Me.chkScraperEpDirector.Checked
            Master.eSettings.ScraperEpCredits = Me.chkScraperEpCredits.Checked
            Master.eSettings.ScraperEpActors = Me.chkScraperEpActors.Checked
            Master.eSettings.DisplayAllSeason = Me.chkDisplayAllSeason.Checked
            Master.eSettings.MarkNewShows = Me.chkMarkNewShows.Checked
            Master.eSettings.MarkNewEpisodes = Me.chkMarkNewEpisodes.Checked
            Master.eSettings.OrderDefault = DirectCast(Me.cbOrdering.SelectedIndex, Enums.Ordering)
            Master.eSettings.OnlyValueForCert = Me.chkOnlyValueForCert.Checked
            AdvancedSettings.SetBooleanSetting("ScrapeActorsThumbs", Me.chkActorCache.Checked)
            For Each s As ModulesManager._externalScraperModuleClass In ModulesManager.Instance.externalScrapersModules
                Try
                    If s.ProcessorModule.IsScraper Then s.ProcessorModule.SaveSetupScraper(Not isApply)
                    If s.ProcessorModule.IsPostScraper Then s.ProcessorModule.SaveSetupPostScraper(Not isApply)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Next
            For Each s As ModulesManager._externalTVScraperModuleClass In ModulesManager.Instance.externalTVScrapersModules
                Try
                    If s.ProcessorModule.IsScraper Then s.ProcessorModule.SaveSetupScraper(Not isApply)
                    If s.ProcessorModule.IsPostScraper Then s.ProcessorModule.SaveSetupPostScraper(Not isApply)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Next
            For Each s As ModulesManager._externalGenericModuleClass In ModulesManager.Instance.externalProcessorModules
                Try
                    s.ProcessorModule.SaveSetup(Not isApply)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Next
            ModulesManager.Instance.SaveSettings()
            Functions.CreateDefaultOptions()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetApplyButton(ByVal v As Boolean)
        If Not NoUpdate Then Me.btnApply.Enabled = v
    End Sub

    Private Sub SetUp()
        Me.cbForce.Items.AddRange(Strings.Split(AdvancedSettings.GetSetting("ForceTitle", ""), "|"))
        Me.btnAddShowRegex.Tag = String.Empty
        Me.Text = Master.eLang.GetString(420, "Settings")
        Me.GroupBox4.Text = Master.eLang.GetString(429, "Miscellaneous")
        Me.Label32.Text = Master.eLang.GetString(430, "Interface Language:")
        Me.chkInfoPanelAnim.Text = Master.eLang.GetString(431, "Enable Panel Animation")
        Me.chkUpdates.Text = Master.eLang.GetString(432, "Check for Updates")
        Me.chkOverwriteNfo.Text = Master.eLang.GetString(433, "Overwrite Non-conforming nfos")
        Me.Label5.Text = Master.eLang.GetString(434, "(If unchecked, non-conforming nfos will be renamed to <filename>.info)")
        Me.chkLogErrors.Text = Master.eLang.GetString(435, "Log Errors to File")
        Me.Label31.Text = Master.eLang.GetString(436, "Display Overlay if Video Contains an Audio Stream With the Following Language:")
        Me.Label50.Text = Me.Label31.Text
        Me.GroupBox3.Text = Master.eLang.GetString(437, "Clean Files")
        Me.tpStandard.Text = Master.eLang.GetString(438, "Standard")
        Me.tpExpert.Text = Master.eLang.GetString(439, "Expert")
        Me.chkWhitelistVideo.Text = Master.eLang.GetString(440, "Whitelist Video Extensions")
        Me.Label27.Text = Master.eLang.GetString(441, "Whitelisted Extensions:")
        Me.Label25.Text = Master.eLang.GetString(442, "WARNING: Using the Expert Mode Cleaner could potentially delete wanted files. Take care when using this tool.")
        Me.gbFilters.Text = Master.eLang.GetString(451, "Folder/File Name Filters")
        Me.chkProperCase.Text = Master.eLang.GetString(452, "Convert Names to Proper Case")
        Me.chkShowProperCase.Text = Me.chkProperCase.Text
        Me.chkEpProperCase.Text = Me.chkProperCase.Text
        Me.GroupBox12.Text = Me.GroupBox4.Text
        Me.chkShowGenresText.Text = Master.eLang.GetString(453, "Always Display Genre Text")
        Me.gbGenreFilter.Text = Master.eLang.GetString(454, "Genre Language Filter")
        Me.chkNoDisplayFanart.Text = Master.eLang.GetString(455, "Do Not Display Fanart")
        Me.chkNoDisplayPoster.Text = Master.eLang.GetString(456, "Do Not Display Poster")
        Me.chkShowDims.Text = Master.eLang.GetString(457, "Display Image Dimensions")
        Me.chkMarkNew.Text = Master.eLang.GetString(459, "Mark New Movies")
        Me.GroupBox2.Text = Master.eLang.GetString(460, "Media List Options")
        Me.Label30.Text = Master.eLang.GetString(461, "Mismatch Tolerance:")
        Me.chkCheckTitles.Text = Master.eLang.GetString(462, "Check Title Match Confidence")
        Me.GroupBox25.Text = Master.eLang.GetString(463, "Sort Tokens to Ignore")
        Me.chkDisplayYear.Text = Master.eLang.GetString(464, "Display Year in List Title")
        Me.chkMovieExtraCol.Text = Master.eLang.GetString(465, "Hide Extrathumb Column")
        Me.chkMovieSubCol.Text = Master.eLang.GetString(466, "Hide Sub Column")
        Me.chkMovieTrailerCol.Text = Master.eLang.GetString(467, "Hide Trailer Column")
        Me.chkMovieInfoCol.Text = Master.eLang.GetString(468, "Hide Info Column")
        Me.chkMovieFanartCol.Text = Master.eLang.GetString(469, "Hide Fanart Column")
        Me.chkMoviePosterCol.Text = Master.eLang.GetString(470, "Hide Poster Column")
        Me.GroupBox8.Text = Master.eLang.GetString(471, "File Naming")
        Me.gbTVNaming.Text = Me.GroupBox8.Text
        Me.chkMovieNameMultiOnly.Text = Master.eLang.GetString(472, "Use <movie> Only for Folders with Multiple Movies")
        Me.GroupBox21.Text = Master.eLang.GetString(151, "Trailer")
        'Me.chkVideoTSParent.Text = Master.eLang.GetString(473, "YAMJ Compatible VIDEO_TS File Placement/Naming")
        Me.GroupBox6.Text = Master.eLang.GetString(149, "Fanart")
        Me.GroupBox5.Text = Master.eLang.GetString(148, "Poster")
        Me.colName.Text = Master.eLang.GetString(232, "Name")
        Me.colPath.Text = Master.eLang.GetString(410, "Path")
        Me.colRecur.Text = Master.eLang.GetString(411, "Recursive")
        Me.colFolder.Text = Master.eLang.GetString(412, "Use Folder Name")
        Me.colSingle.Text = Master.eLang.GetString(413, "Single Video")
        Me.btnMovieRem.Text = Master.eLang.GetString(30, "Remove")
        Me.btnRemTVSource.Text = Master.eLang.GetString(30, "Remove")
        Me.btnMovieAddFolder.Text = Master.eLang.GetString(407, "Add Source")
        Me.btnAddTVSource.Text = Me.btnMovieAddFolder.Text
        Me.GroupBox14.Text = Me.GroupBox5.Text
        Me.Label24.Text = Master.eLang.GetString(478, "Quality:")
        Me.Label11.Text = Master.eLang.GetString(479, "Max Width:")
        Me.Label12.Text = Master.eLang.GetString(480, "Max Height:")
        Me.chkResizePoster.Text = Master.eLang.GetString(481, "Automatically Resize:")
        Me.lblPosterSize.Text = Master.eLang.GetString(482, "Preferred Size:")
        Me.chkOverwritePoster.Text = Master.eLang.GetString(483, "Overwrite Existing")
        Me.GroupBox13.Text = Me.GroupBox6.Text
        Me.chkFanartOnly.Text = Master.eLang.GetString(145, "Only")
        Me.chkPosterOnly.Text = Master.eLang.GetString(145, "Only")
        Me.Label26.Text = Me.Label24.Text
        Me.Label9.Text = Me.Label11.Text
        Me.Label10.Text = Me.Label12.Text
        Me.chkResizeFanart.Text = Me.chkResizePoster.Text
        Me.lblFanartSize.Text = Me.lblPosterSize.Text
        Me.chkOverwriteFanart.Text = Me.chkOverwritePoster.Text
        Me.GroupBox10.Text = Master.eLang.GetString(488, "Global Locks")
        Me.chkLockTrailer.Text = Master.eLang.GetString(489, "Lock Trailer")
        Me.chkLockGenre.Text = Master.eLang.GetString(490, "Lock Genre")
        Me.chkLockRealStudio.Text = Master.eLang.GetString(491, "Lock Studio")
        Me.chkLockRating.Text = Master.eLang.GetString(492, "Lock Rating")
        Me.chkLockTagline.Text = Master.eLang.GetString(493, "Lock Tagline")
        Me.chkLockTitle.Text = Master.eLang.GetString(494, "Lock Title")
        Me.chkLockOutline.Text = Master.eLang.GetString(495, "Lock Outline")
        Me.chkLockPlot.Text = Master.eLang.GetString(496, "Lock Plot")
        Me.GroupBox9.Text = Master.eLang.GetString(497, "Images")
        Me.chkNoSaveImagesToNfo.Text = Master.eLang.GetString(498, "Do Not Save URLs to Nfo")
        Me.chkSingleScrapeImages.Text = Master.eLang.GetString(499, "Get on Single Scrape")

        Me.chkUseETasFA.Text = Master.eLang.GetString(503, "Use if no Fanart Found")
        Me.chkNoSpoilers.Text = Master.eLang.GetString(505, "No Spoilers")
        Me.Label15.Text = Master.eLang.GetString(506, "Number To Create:")
        Me.chkAutoThumbs.Text = Master.eLang.GetString(507, "Extract During Scrapers")
        Me.chkOutlineForPlot.Text = Master.eLang.GetString(508, "Use Outline for Plot if Plot is Empty")

        Me.chkCastWithImg.Text = Master.eLang.GetString(510, "Scrape Only Actors With Images")
        Me.chkUseCertForMPAA.Text = Master.eLang.GetString(511, "Use Certification for MPAA")
        Me.chkFullCast.Text = Master.eLang.GetString(512, "Scrape Full Cast")
        Me.chkFullCrew.Text = Master.eLang.GetString(513, "Scrape Full Crew")
        Me.chkCert.Text = Master.eLang.GetString(514, "Use Certification Language:")
        Me.gbRTFormat.Text = Master.eLang.GetString(515, "Duration Format")
        Me.chkUseMIDuration.Text = Master.eLang.GetString(516, "Use Duration for Runtime")
        Me.chkScanMediaInfo.Text = Master.eLang.GetString(517, "Scan Meta Data")
        Me.chkTVScanMetaData.Text = Me.chkScanMediaInfo.Text
        Me.btnOK.Text = Master.eLang.GetString(179, "OK")
        Me.btnApply.Text = Master.eLang.GetString(276, "Apply")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label2.Text = Master.eLang.GetString(518, "Configure Ember's appearance and operation.")
        Me.Label4.Text = Me.Text
        Me.GroupBox16.Text = Master.eLang.GetString(520, "Backdrops Folder")
        Me.chkAutoBD.Text = Master.eLang.GetString(521, "Automatically Save Fanart To Backdrops Folder")
        Me.GroupBox26.Text = Master.eLang.GetString(59, "Meta Data")
        Me.GroupBox31.Text = Me.GroupBox26.Text

        Me.chkDeleteAllTrailers.Text = Master.eLang.GetString(522, "Delete All Existing")
        Me.chkOverwriteTrailer.Text = Master.eLang.GetString(483, "Overwrite Existing")
        Me.chkNoDLTrailer.Text = Master.eLang.GetString(524, "Only Get URLs When Scraping")
        Me.chkSingleScrapeTrailer.Text = Master.eLang.GetString(525, "Get During Single Scrape")

        Me.chkUpdaterTrailer.Text = Master.eLang.GetString(527, "Get During Automated Scrapers")

        Me.chkDownloadTrailer.Text = Master.eLang.GetString(529, "Enable Trailer Support")
        Me.GroupBox22.Text = Master.eLang.GetString(530, "No Stack Extensions")

        Me.GroupBox18.Text = Master.eLang.GetString(534, "Valid Video Extensions")
        Me.btnEditSource.Text = Master.eLang.GetString(535, "Edit Source")
        Me.btnEditTVSource.Text = Master.eLang.GetString(535, "Edit Source")
        Me.GroupBox19.Text = Master.eLang.GetString(536, "Miscellaneous Options")
        Me.gbMiscTVSourceOpts.Text = Master.eLang.GetString(536, "Miscellaneous Options")
        Me.chkAutoDetectVTS.Text = Master.eLang.GetString(537, "Automatically Detect VIDEO_TS Folders Even if They Are Not Named ""VIDEO_TS""")
        Me.chkSkipStackedSizeCheck.Text = Master.eLang.GetString(538, "Skip Size Check of Stacked Files")
        Me.Label21.Text = Master.eLang.GetString(539, "MB")
        Me.Label20.Text = Master.eLang.GetString(540, "Skip files smaller than:")
        Me.Label6.Text = Me.Label21.Text
        Me.Label7.Text = Me.Label20.Text
        Me.GroupBox23.Text = Master.eLang.GetString(153, "Extrathumbs")
        Me.GroupBox24.Text = Master.eLang.GetString(541, "Sizing (Extracted Frames)")
        Me.chkETPadding.Text = Master.eLang.GetString(542, "Padding")
        Me.Label28.Text = Master.eLang.GetString(543, "Width:")
        Me.Label29.Text = Master.eLang.GetString(544, "Height:")
        Me.rbETCustom.Text = Master.eLang.GetString(545, "Use Custom Size")
        Me.rbETNative.Text = Master.eLang.GetString(546, "Use Native Resolution")
        Me.GroupBox17.Text = Master.eLang.GetString(547, "Caching")
        Me.chkUseImgCacheUpdaters.Text = Master.eLang.GetString(548, "Use During Automated Scrapers")
        Me.chkPersistImgCache.Text = Master.eLang.GetString(550, "Persistent Image Cache")
        Me.chkUseImgCache.Text = Master.eLang.GetString(551, "Use Image Cache")
        Me.fbdBrowse.Description = Master.eLang.GetString(552, "Select the folder where you wish to store your backdrops.")
        Me.gbOptions.Text = Master.eLang.GetString(577, "Scraper Fields")
        Me.GroupBox32.Text = Master.eLang.GetString(577, "Scraper Fields")
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
        Me.chkCertification.Text = Master.eLang.GetString(722, "Certification")
        Me.chkYear.Text = Master.eLang.GetString(278, "Year")
        Me.chkTitle.Text = Master.eLang.GetString(21, "Title")
        Me.chkScraperShowTitle.Text = Master.eLang.GetString(21, "Title")
        Me.chkScraperShowEGU.Text = Master.eLang.GetString(723, "EpisodeGuideURL")
        Me.chkScraperShowGenre.Text = Master.eLang.GetString(20, "Genres")
        Me.chkScraperShowMPAA.Text = Master.eLang.GetString(401, "MPAA")
        Me.chkScraperShowPlot.Text = Master.eLang.GetString(65, "Plot")
        Me.chkScraperShowPremiered.Text = Master.eLang.GetString(724, "Premiered")
        Me.chkScraperShowRating.Text = Master.eLang.GetString(400, "Rating")
        Me.chkScraperShowStudio.Text = Master.eLang.GetString(395, "Studio")
        Me.chkScraperShowActors.Text = Master.eLang.GetString(725, "Actors")
        Me.chkScraperEpTitle.Text = Master.eLang.GetString(21, "Title")
        Me.chkScraperEpSeason.Text = Master.eLang.GetString(650, "Season")
        Me.chkScraperEpEpisode.Text = Master.eLang.GetString(727, "Episode")
        Me.chkScraperEpAired.Text = Master.eLang.GetString(728, "Aired")
        Me.chkScraperEpRating.Text = Master.eLang.GetString(400, "Rating")
        Me.chkScraperEpPlot.Text = Master.eLang.GetString(65, "Plot")
        Me.chkScraperEpDirector.Text = Master.eLang.GetString(62, "Director")
        Me.chkScraperEpCredits.Text = Master.eLang.GetString(729, "Credits")
        Me.chkScraperEpActors.Text = Master.eLang.GetString(725, "Actors")
        Me.GroupBox1.Text = Me.GroupBox4.Text
        Me.lblLimit.Text = Master.eLang.GetString(578, "Limit:")
        Me.lblLimit2.Text = Me.lblLimit.Text
        Me.GroupBox27.Text = Master.eLang.GetString(581, "Missing Items Filter")
        Me.chkMissingPoster.Text = Master.eLang.GetString(582, "Check for Poster")
        Me.chkMissingFanart.Text = Master.eLang.GetString(583, "Check for Fanart")
        Me.chkMissingNFO.Text = Master.eLang.GetString(584, "Check for NFO")
        Me.chkMissingTrailer.Text = Master.eLang.GetString(585, "Check for Trailer")
        Me.chkMissingSubs.Text = Master.eLang.GetString(586, "Check for Subs")
        Me.chkMissingExtra.Text = Master.eLang.GetString(587, "Check for Extrathumbs")
        Me.chkTop250.Text = Master.eLang.GetString(591, "Top 250")
        Me.chkCountry.Text = Master.eLang.GetString(301, "Country")
        Me.chkClickScrape.Text = Master.eLang.GetString(849, "Enable Click Scrape")

        Me.chkAutoETSize.Text = Master.eLang.GetString(599, "Download All Fanart Images of the Following Size as Extrathumbs:")
        Me.Label35.Text = String.Concat(Master.eLang.GetString(620, "Movie Theme"), ":")
        Me.Label1.Text = String.Concat(Master.eLang.GetString(666, "TV Show Theme"), ":")
        Me.Label3.Text = String.Concat(Master.eLang.GetString(667, "Episode Theme"), ":")
        Me.GroupBox28.Text = Master.eLang.GetString(625, "Defaults by File Type")
        Me.gbTVMIDefaults.Text = Me.gbTVMIDefaults.Text
        Me.Label34.Text = Master.eLang.GetString(626, "File Type")
        Me.Label49.Text = Me.Label34.Text
        Me.chkIFOScan.Text = Master.eLang.GetString(628, "Enable IFO Parsing")
        Me.GroupBox29.Text = Master.eLang.GetString(629, "Themes")
        'Me.chkYAMJCompatibleSets.Text = Master.eLang.GetString(643, "YAMJ Compatible Sets")
        Me.chkCleanDB.Text = Master.eLang.GetString(668, "Clean database after updating library")
        Me.chkTVCleanDB.Text = Me.chkCleanDB.Text
        Me.chkIgnoreLastScan.Text = Master.eLang.GetString(669, "Ignore last scan time when updating library")
        Me.chkTVIgnoreLastScan.Text = Me.chkIgnoreLastScan.Text
        Me.gbShowFilter.Text = Master.eLang.GetString(670, "Show Folder/File Name Filters")
        Me.gbEpFilter.Text = Master.eLang.GetString(671, "Episode Folder/File Name Filters")
        Me.gbProxy.Text = Master.eLang.GetString(672, "Proxy")
        Me.chkEnableProxy.Text = Master.eLang.GetString(673, "Enable Proxy")
        Me.lblProxyURI.Text = Master.eLang.GetString(674, "Proxy URL:")
        Me.lblProxyPort.Text = Master.eLang.GetString(675, "Proxy Port:")
        Me.gbCreds.Text = Master.eLang.GetString(676, "Credentials")
        Me.chkEnableCredentials.Text = Master.eLang.GetString(677, "Enable Credentials")
        Me.lblProxyUN.Text = Master.eLang.GetString(425, "Username:")
        Me.lblProxyPW.Text = Master.eLang.GetString(426, "Password:")
        Me.lblProxyDomain.Text = Master.eLang.GetString(678, "Domain:")
        Me.gbTVMisc.Text = Me.GroupBox4.Text
        Me.lblRatingRegion.Text = Master.eLang.GetString(679, "TV Rating Region")
        Me.gbTVListOptions.Text = Master.eLang.GetString(460, "Media List Options")
        Me.gbShowListOptions.Text = Master.eLang.GetString(680, "Shows")
        Me.gbSeasonListOptions.Text = Master.eLang.GetString(681, "Seasons")
        Me.gbEpisodeListOptions.Text = Master.eLang.GetString(682, "Episodes")
        Me.chkShowPosterCol.Text = Me.chkMoviePosterCol.Text
        Me.chkSeasonPosterCol.Text = Me.chkMoviePosterCol.Text
        Me.chkEpisodePosterCol.Text = Me.chkMoviePosterCol.Text
        Me.chkShowFanartCol.Text = Me.chkMovieFanartCol.Text
        Me.chkSeasonFanartCol.Text = Me.chkMovieFanartCol.Text
        Me.chkEpisodeFanartCol.Text = Me.chkMovieFanartCol.Text
        Me.chkShowNfoCol.Text = Me.chkMovieInfoCol.Text
        Me.chkEpisodeNfoCol.Text = Me.chkMovieInfoCol.Text
        Me.gbShowPosters.Text = Master.eLang.GetString(683, "Show Posters")
        Me.gbShowFanart.Text = Master.eLang.GetString(684, "Show Fanart")
        Me.gbSeasonPosters.Text = Master.eLang.GetString(685, "Season Posters")
        Me.gbSeasonFanart.Text = Master.eLang.GetString(686, "Season Fanart")
        Me.gbEpisodePosters.Text = Master.eLang.GetString(687, "Episode Posters")
        Me.gbEpisodeFanart.Text = Master.eLang.GetString(688, "Episode Fanart")
        Me.btnEditShowRegex.Text = Master.eLang.GetString(690, "Edit Regex")
        Me.btnRemoveShowRegex.Text = Master.eLang.GetString(30, "Remove")
        Me.gbShowRegex.Text = Master.eLang.GetString(691, "Show Match Regex")
        Me.lblSeasonMatch.Text = Master.eLang.GetString(692, "Season Match Regex:")
        Me.lblEpisodeMatch.Text = Master.eLang.GetString(693, "Episode Match Regex:")
        Me.lblSeasonRetrieve.Text = String.Concat(Master.eLang.GetString(694, "Apply To"), ":")
        Me.lblEpisodeRetrieve.Text = Me.lblSeasonRetrieve.Text
        Me.btnAddShowRegex.Text = Master.eLang.GetString(695, "Edit Regex")
        Me.gbShowPosterOpts.Text = Me.GroupBox5.Text
        Me.lblShowPosterSize.Text = Master.eLang.GetString(730, "Preferred Type:")
        Me.chkOverwriteShowPoster.Text = Me.chkOverwritePoster.Text
        Me.chkResizeShowPoster.Text = Me.chkResizePoster.Text
        Me.lblShowPosterWidth.Text = Me.Label11.Text
        Me.lblShowPosterHeight.Text = Me.Label12.Text
        Me.lblShowPosterQ.Text = Me.Label24.Text
        Me.gbShowFanartOpts.Text = Me.GroupBox6.Text
        Me.lblShowFanartSize.Text = Me.lblFanartSize.Text
        Me.chkOverwriteShowFanart.Text = Me.chkOverwriteFanart.Text
        Me.chkResizeShowFanart.Text = Me.chkResizeFanart.Text
        Me.lblShowFanartWidth.Text = Me.Label11.Text
        Me.lblShowFanartHeight.Text = Me.Label12.Text
        Me.lblShowFanartQ.Text = Me.Label26.Text
        Me.gbEpPosterOpts.Text = Me.GroupBox5.Text
        Me.chkOverwriteEpPoster.Text = Me.chkOverwritePoster.Text
        Me.chkResizeEpPoster.Text = Me.chkResizePoster.Text
        Me.lblEpPosterWidth.Text = Me.Label11.Text
        Me.lblEpPosterHeight.Text = Me.Label12.Text
        Me.lblEpPosterQ.Text = Me.Label24.Text
        Me.gbEpFanartOpts.Text = Me.GroupBox6.Text
        Me.lblEpFanartSize.Text = Me.lblFanartSize.Text
        Me.chkOverwriteEpFanart.Text = Me.chkOverwriteFanart.Text
        Me.chkResizeEpFanart.Text = Me.chkResizeFanart.Text
        Me.lblEpFanartWidth.Text = Me.Label11.Text
        Me.lblEpFanartHeight.Text = Me.Label12.Text
        Me.lblEpFanartQ.Text = Me.Label26.Text
        Me.gbSeaPosterOpts.Text = Me.GroupBox5.Text
        Me.lblSeaPosterSize.Text = Me.lblShowPosterSize.Text
        Me.chkSeaOverwritePoster.Text = Me.chkOverwritePoster.Text
        Me.chkSeaResizePoster.Text = Me.chkResizePoster.Text
        Me.lblSeaPosterWidth.Text = Me.Label11.Text
        Me.lblSeaPosterHeight.Text = Me.Label12.Text
        Me.lblSeaPosterQ.Text = Me.Label24.Text
        Me.gbSeaFanartOpts.Text = Me.GroupBox6.Text
        Me.lblSeaFanartSize.Text = Me.lblFanartSize.Text
        Me.chkSeaOverwriteFanart.Text = Me.chkOverwriteFanart.Text
        Me.chkSeaResizeFanart.Text = Me.chkResizeFanart.Text
        Me.lblSeaFanartWidth.Text = Me.Label11.Text
        Me.lblSeaFanartHeight.Text = Me.Label12.Text
        Me.lblSeaFanartQ.Text = Me.Label26.Text
        Me.Label51.Text = Master.eLang.GetString(732, "<h>=Hours <m>=Minutes <s>=Seconds")
        Me.chkDisplayMissingEpisodes.Text = Master.eLang.GetString(733, "Display Missing Episodes")
        Me.chkForceTitle.Text = Master.eLang.GetString(710, "Force Title Language:")
        Me.chkSourceFromFolder.Text = Master.eLang.GetString(711, "Include Folder Name in Source Type Check")
        Me.chkSortBeforeScan.Text = Master.eLang.GetString(712, "Sort files into folder before each library update")
        Me.chkNoFilterEpisode.Text = Master.eLang.GetString(734, "Build Episode Title Instead of Filtering")
        Me.gbAllSeasonPoster.Text = Master.eLang.GetString(735, "All Season Posters")
        Me.chkOnlyTVImagesLanguage.Text = Master.eLang.GetString(736, "Only Get Images for the Selected Language")
        Me.chkGetEnglishImages.Text = Master.eLang.GetString(737, "Also Get English Images")
        Me.lblAPIKey.Text = Master.eLang.GetString(738, "API Key:")
        Me.lblTVUpdate.Text = Master.eLang.GetString(740, "Re-download Show Information Every:")
        Me.gbLanguage.Text = Master.eLang.GetString(610, "Language")
        Me.lblTVLanguagePreferred.Text = Master.eLang.GetString(741, "Preferred Language:")
        Me.btnTVLanguageFetch.Text = Master.eLang.GetString(742, "Fetch Available Languages")
        Me.GroupBox33.Text = Master.eLang.GetString(488, "Global Locks")
        Me.gbShowLocks.Text = Master.eLang.GetString(743, "Show")
        Me.chkShowLockTitle.Text = Master.eLang.GetString(494, "Lock Title")
        Me.chkShowLockPlot.Text = Master.eLang.GetString(496, "Lock Plot")
        Me.chkShowLockRating.Text = Master.eLang.GetString(492, "Lock Rating")
        Me.chkShowLockGenre.Text = Master.eLang.GetString(490, "Lock Genre")
        Me.chkShowLockStudio.Text = Master.eLang.GetString(491, "Lock Studio")
        Me.gbEpLocks.Text = Master.eLang.GetString(727, "Episode")
        Me.chkEpLockTitle.Text = Master.eLang.GetString(494, "Lock Title")
        Me.chkEpLockPlot.Text = Master.eLang.GetString(496, "Lock Plot")
        Me.chkEpLockRating.Text = Master.eLang.GetString(492, "Lock Rating")
        Me.GroupBox35.Text = Master.eLang.GetString(743, "Show")
        Me.GroupBox34.Text = Master.eLang.GetString(727, "Episode")
        Me.gbInterface.Text = Master.eLang.GetString(795, "Interface")
        Me.chkScanOrderModify.Text = Master.eLang.GetString(796, "Scan in order of last write time")
        Me.chkTVScanOrderModify.Text = Me.chkScanOrderModify.Text
        Me.lblPreferredQuality.Text = Master.eLang.GetString(800, "Preferred Quality:")
        Me.gbTVScraperOptions.Text = Master.eLang.GetString(390, "Options")
        Me.lblTVDBMirror.Text = Master.eLang.GetString(801, "TVDB Mirror")
        Me.chkDisplayAllSeason.Text = Master.eLang.GetString(832, "Display All Season Poster")
        Me.gbHelp.Text = String.Concat("     ", Master.eLang.GetString(458, "Help"))
        Me.chkMarkNewShows.Text = Master.eLang.GetString(549, "Mark New Shows")
        Me.chkMarkNewEpisodes.Text = Master.eLang.GetString(621, "Mark New Episodes")
        Me.lblOrdering.Text = Master.eLang.GetString(797, "Default Episode Ordering:")
        Me.chkOnlyValueForCert.Text = Master.eLang.GetString(835, "Only Save the Value to NFO")
        Me.chkActorCache.Text = Master.eLang.GetString(828, "Enable Actors Cache")
        Me.rbBanner.Text = Master.eLang.GetString(838, "Banner")
        Me.rbPoster.Text = Me.GroupBox5.Text
        Me.rbAllSBanner.Text = Me.rbBanner.Text
        Me.rbAllSPoster.Text = Me.GroupBox5.Text
        Me.gbAllSPosterOpts.Text = Me.gbAllSeasonPoster.Text
        Me.lblAllSPosterSize.Text = Me.lblShowPosterSize.Text
        Me.chkOverwriteAllSPoster.Text = Me.chkOverwritePoster.Text
        Me.chkResizeAllSPoster.Text = Me.chkResizePoster.Text
        Me.lblAllSPosterWidth.Text = Me.Label11.Text
        Me.lblAllSPosterHeight.Text = Me.Label12.Text
        Me.lblAllSPosterQ.Text = Me.Label24.Text
        Me.btnClearRegex.Text = Master.eLang.GetString(123, "Clear")
        Me.chkAskCheckboxScrape.Text = Master.eLang.GetString(852, "Ask On Click Scrape")
		Me.btnMovieFrodo.Text = Master.eLang.GetString(867, "XBMC Frodo")
		Me.btnTVShowFrodo.Text = Master.eLang.GetString(867, "XBMC Frodo")

		Me.lvTVSources.Columns(1).Text = Master.eLang.GetString(232, "Name")
        Me.lvTVSources.Columns(2).Text = Master.eLang.GetString(410, "Path")

        Me.lvShowRegex.Columns(1).Text = Master.eLang.GetString(696, "Show Regex")
        Me.lvShowRegex.Columns(2).Text = Master.eLang.GetString(694, "Apply To")
        Me.lvShowRegex.Columns(3).Text = Master.eLang.GetString(697, "Episode Regex")
        Me.lvShowRegex.Columns(4).Text = Master.eLang.GetString(694, "Apply To")

        Me.lvMovies.Columns(1).Text = Master.eLang.GetString(232, "Name")
        Me.lvMovies.Columns(2).Text = Master.eLang.GetString(410, "Path")
        Me.lvMovies.Columns(3).Text = Master.eLang.GetString(411, "Recursive")
        Me.lvMovies.Columns(4).Text = Master.eLang.GetString(412, "Use Folder Name")
        Me.lvMovies.Columns(5).Text = Master.eLang.GetString(413, "Single Video")

        Me.TabPage3.Text = Master.eLang.GetString(38, "General")
        Me.TabPage4.Text = Master.eLang.GetString(699, "Regex")
        Me.TabPage5.Text = Master.eLang.GetString(700, "TV Show")
        Me.TabPage6.Text = Master.eLang.GetString(744, "TV Season")
        Me.TabPage7.Text = Master.eLang.GetString(701, "TV Episode")
        Me.TabPage8.Text = Master.eLang.GetString(38, "General")

        Me.cbPosterSize.Items.Clear()
        Me.cbPosterSize.Items.AddRange(New String() {Master.eLang.GetString(322, "X-Large"), Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small"), Master.eLang.GetString(558, "Wide")})
        Me.cbFanartSize.Items.Clear()
        Me.cbFanartSize.Items.AddRange(New String() {Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small")})
        Me.cbAutoETSize.Items.Clear()
        Me.cbAutoETSize.Items.AddRange(New String() {Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small")})
        Me.cbShowFanartSize.Items.Clear()
        Me.cbShowFanartSize.Items.AddRange(New String() {Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small")})
        Me.cbEpFanartSize.Items.Clear()
        Me.cbEpFanartSize.Items.AddRange(New String() {Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small")})
        Me.cbSeaPosterSize.Items.Clear()
        Me.cbSeaPosterSize.Items.AddRange(New String() {Master.eLang.GetString(745, "None"), Me.GroupBox5.Text, Master.eLang.GetString(558, "Wide")})
        Me.cbSeaFanartSize.Items.Clear()
        Me.cbSeaFanartSize.Items.AddRange(New String() {Master.eLang.GetString(323, "Large"), Master.eLang.GetString(324, "Medium"), Master.eLang.GetString(325, "Small")})

        Me.cboTVUpdate.Items.Clear()
        Me.cboTVUpdate.Items.AddRange(New String() {Master.eLang.GetString(749, "Week"), Master.eLang.GetString(750, "Bi-Weekly"), Master.eLang.GetString(751, "Month"), Master.eLang.GetString(752, "Never"), Master.eLang.GetString(753, "Always")})

        Me.cbOrdering.Items.Clear()
        Me.cbOrdering.Items.AddRange(New String() {Master.eLang.GetString(438, "Standard"), Master.eLang.GetString(350, "DVD"), Master.eLang.GetString(839, "Absolute")})

        Me.cboSeasonRetrieve.Items.Clear()
        Me.cboSeasonRetrieve.Items.AddRange(New String() {Master.eLang.GetString(13, "Folder Name"), Master.eLang.GetString(15, "File Name")})

        Me.cboEpRetrieve.Items.Clear()
        Me.cboEpRetrieve.Items.AddRange(New String() {Master.eLang.GetString(13, "Folder Name"), Master.eLang.GetString(15, "File Name"), Master.eLang.GetString(16, "Season Result")})

        Me.LoadTrailerQualities()
    End Sub

    Private Sub tbAllSPosterQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbAllSPosterQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblAllSPosterQual.Text = tbAllSPosterQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblAllSPosterQual
            Select Case True
                Case tbAllSPosterQual.Value = 0
                    .ForeColor = Color.Black
                Case tbAllSPosterQual.Value > 95 OrElse tbAllSPosterQual.Value < 20
                    .ForeColor = Color.Red
                Case tbAllSPosterQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbAllSPosterQual.Value, 300 - tbAllSPosterQual.Value, 0)
                Case tbAllSPosterQual.Value >= 80 AndAlso tbAllSPosterQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbAllSPosterQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbAllSPosterQual.Value - 20)), 0)
                Case tbAllSPosterQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbAllSPosterQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbEpFanartQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbEpFanartQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblEpFanartQual.Text = tbEpFanartQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblEpFanartQual
            Select Case True
                Case tbEpFanartQual.Value = 0
                    .ForeColor = Color.Black
                Case tbEpFanartQual.Value > 95 OrElse tbEpFanartQual.Value < 20
                    .ForeColor = Color.Red
                Case tbEpFanartQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbEpFanartQual.Value, 300 - tbEpFanartQual.Value, 0)
                Case tbEpFanartQual.Value >= 80 AndAlso tbEpFanartQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbEpFanartQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbEpFanartQual.Value - 20)), 0)
                Case tbEpFanartQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbEpFanartQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbEpPosterQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbEpPosterQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblEpPosterQual.Text = tbEpPosterQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblEpPosterQual
            Select Case True
                Case tbEpPosterQual.Value = 0
                    .ForeColor = Color.Black
                Case tbEpPosterQual.Value > 95 OrElse tbEpPosterQual.Value < 20
                    .ForeColor = Color.Red
                Case tbEpPosterQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbEpPosterQual.Value, 300 - tbEpPosterQual.Value, 0)
                Case tbEpPosterQual.Value >= 80 AndAlso tbEpPosterQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbEpPosterQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbEpPosterQual.Value - 20)), 0)
                Case tbEpPosterQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbEpPosterQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbFanartQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbFanartQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblFanartQual.Text = tbFanartQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblFanartQual
            Select Case True
                Case tbFanartQual.Value = 0
                    .ForeColor = Color.Black
                Case tbFanartQual.Value > 95 OrElse tbFanartQual.Value < 20
                    .ForeColor = Color.Red
                Case tbFanartQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbFanartQual.Value, 300 - tbFanartQual.Value, 0)
                Case tbFanartQual.Value >= 80 AndAlso tbFanartQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbFanartQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbFanartQual.Value - 20)), 0)
                Case tbFanartQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbFanartQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbPosterQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbPosterQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblPosterQual.Text = tbPosterQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblPosterQual
            Select Case True
                Case tbPosterQual.Value = 0
                    .ForeColor = Color.Black
                Case tbPosterQual.Value > 95 OrElse tbPosterQual.Value < 20
                    .ForeColor = Color.Red
                Case tbPosterQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbPosterQual.Value, 300 - tbPosterQual.Value, 0)
                Case tbPosterQual.Value >= 80 AndAlso tbPosterQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbPosterQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbPosterQual.Value - 20)), 0)
                Case tbPosterQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbPosterQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbSeaFanartQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbSeaFanartQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblSeaFanartQual.Text = tbSeaFanartQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblSeaFanartQual
            Select Case True
                Case tbSeaFanartQual.Value = 0
                    .ForeColor = Color.Black
                Case tbSeaFanartQual.Value > 95 OrElse tbSeaFanartQual.Value < 20
                    .ForeColor = Color.Red
                Case tbSeaFanartQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbSeaFanartQual.Value, 300 - tbSeaFanartQual.Value, 0)
                Case tbSeaFanartQual.Value >= 80 AndAlso tbSeaFanartQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbSeaFanartQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbSeaFanartQual.Value - 20)), 0)
                Case tbSeaFanartQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbSeaFanartQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbSeaPosterQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbSeaPosterQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblSeaPosterQual.Text = tbSeaPosterQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblSeaPosterQual
            Select Case True
                Case tbSeaPosterQual.Value = 0
                    .ForeColor = Color.Black
                Case tbSeaPosterQual.Value > 95 OrElse tbSeaPosterQual.Value < 20
                    .ForeColor = Color.Red
                Case tbSeaPosterQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbSeaPosterQual.Value, 300 - tbSeaPosterQual.Value, 0)
                Case tbSeaPosterQual.Value >= 80 AndAlso tbSeaPosterQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbSeaPosterQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbSeaPosterQual.Value - 20)), 0)
                Case tbSeaPosterQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbSeaPosterQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbShowFanartQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbShowFanartQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblShowFanartQual.Text = tbShowFanartQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblShowFanartQual
            Select Case True
                Case tbShowFanartQual.Value = 0
                    .ForeColor = Color.Black
                Case tbShowFanartQual.Value > 95 OrElse tbShowFanartQual.Value < 20
                    .ForeColor = Color.Red
                Case tbShowFanartQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbShowFanartQual.Value, 300 - tbShowFanartQual.Value, 0)
                Case tbShowFanartQual.Value >= 80 AndAlso tbShowFanartQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbShowFanartQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbShowFanartQual.Value - 20)), 0)
                Case tbShowFanartQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbShowFanartQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tbShowPosterQual_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbShowPosterQual.ValueChanged
        Me.SetApplyButton(True)
        Me.lblShowPosterQual.Text = tbShowPosterQual.Value.ToString
        'change text color to indicate recommendations
        With Me.lblShowPosterQual
            Select Case True
                Case tbShowPosterQual.Value = 0
                    .ForeColor = Color.Black
                Case tbShowPosterQual.Value > 95 OrElse tbShowPosterQual.Value < 20
                    .ForeColor = Color.Red
                Case tbShowPosterQual.Value > 85
                    .ForeColor = Color.FromArgb(255, 155 + tbShowPosterQual.Value, 300 - tbShowPosterQual.Value, 0)
                Case tbShowPosterQual.Value >= 80 AndAlso tbShowPosterQual.Value <= 85
                    .ForeColor = Color.Blue
                Case tbShowPosterQual.Value <= 50
                    .ForeColor = Color.FromArgb(255, 255, Convert.ToInt32(8.5 * (tbShowPosterQual.Value - 20)), 0)
                Case tbShowPosterQual.Value < 80
                    .ForeColor = Color.FromArgb(255, Convert.ToInt32(255 - (8.5 * (tbShowPosterQual.Value - 50))), 255, 0)
            End Select
        End With
    End Sub

    Private Sub tcCleaner_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tcCleaner.SelectedIndexChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub ToolStripButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        currText = DirectCast(sender, ToolStripButton).Text
        Me.FillList(currText)
    End Sub

    Private Sub tvSettings_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvSettings.AfterSelect
        Me.pbCurrent.Image = Me.ilSettings.Images(Me.tvSettings.SelectedNode.ImageIndex)
        Me.lblCurrent.Text = String.Format("{0} - {1}", Me.currText, Me.tvSettings.SelectedNode.Text)

        Me.RemoveCurrPanel()

        Me.currPanel = Me.SettingsPanels.FirstOrDefault(Function(p) p.Name = tvSettings.SelectedNode.Name).Panel
        Me.currPanel.Location = New Point(0, 0)
        Me.pnlMain.Controls.Add(Me.currPanel)
        Me.currPanel.Visible = True
        Me.pnlMain.Refresh()
    End Sub

    Private Sub txtActorLimit_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtActorLimit.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtActorLimit_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtActorLimit.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtAllSPosterHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtAllSPosterHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtAllSPosterHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAllSPosterHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtAllSPosterWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtAllSPosterWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtAllSPosterWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAllSPosterWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtAPIKey_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAPIKey.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtAutoThumbs_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtAutoThumbs.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtAutoThumbs_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAutoThumbs.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtBDPath_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtBDPath.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtCheckTitleTol_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCheckTitleTol.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtCheckTitleTol_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCheckTitleTol.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtDefFIExt_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDefFIExt.TextChanged
        btnNewMetaDataFT.Enabled = Not String.IsNullOrEmpty(txtDefFIExt.Text) AndAlso Not Me.lstMetaData.Items.Contains(If(txtDefFIExt.Text.StartsWith("."), txtDefFIExt.Text, String.Concat(".", txtDefFIExt.Text)))
        If btnNewMetaDataFT.Enabled Then
            btnEditMetaDataFT.Enabled = False
            btnRemoveMetaDataFT.Enabled = False
        End If
    End Sub

    Private Sub txtEpFanartHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEpFanartHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtEpFanartHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtEpFanartHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtEpFanartWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEpFanartWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtEpFanartWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtEpFanartWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtEpPosterHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEpPosterHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtEpPosterHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtEpPosterHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtEpPosterWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtEpPosterWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtEpPosterWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtEpPosterWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtEpRegex_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtEpRegex.TextChanged
        Me.ValidateRegex()
    End Sub

    Private Sub txtETHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtETHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtETHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtETHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtETWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtETWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtETWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtETWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtFanartHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtFanartHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtFanartHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFanartHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtFanartWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtFanartWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtFanartWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFanartWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtGenreLimit_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtGenreLimit.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtGenreLimit_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtGenreLimit.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtPosterHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPosterHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtPosterHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPosterHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtPosterWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPosterWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtPosterWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPosterWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtProxyDomain_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtProxyDomain.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtProxyPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtProxyPassword.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtProxyPort_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtProxyPort.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtProxyPort_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtProxyPort.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtProxyUsername_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtProxyUsername.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtRuntimeFormat_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtRuntimeFormat.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSeaFanartHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSeaFanartHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSeaFanartWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSeaFanartWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSeaPosterHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSeaPosterHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSeaPosterWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSeaPosterWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSeasonRegex_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSeasonRegex.TextChanged
        Me.ValidateRegex()
    End Sub

    Private Sub txtShowFanartHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtShowFanartHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtShowFanartHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtShowFanartHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtShowFanartWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtShowFanartWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtShowFanartWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtShowFanartWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtShowPosterHeight_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtShowPosterHeight.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtShowPosterHeight_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtShowPosterHeight.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtShowPosterWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtShowPosterWidth.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtShowPosterWidth_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtShowPosterWidth.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtSkipLessThan_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSkipLessThan.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtSkipLessThan_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSkipLessThan.TextChanged
        Me.SetApplyButton(True)
        Me.sResult.NeedsUpdate = True
    End Sub

    Private Sub txtTVSkipLessThan_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtTVSkipLessThan.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar)
    End Sub

    Private Sub txtTVSkipLessThan_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTVSkipLessThan.TextChanged
        Me.SetApplyButton(True)
        Me.sResult.NeedsUpdate = True
    End Sub

    Private Sub txtTVDBMirror_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTVDBMirror.TextChanged
        Me.SetApplyButton(True)
    End Sub

    Private Sub txtTVDefFIExt_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTVDefFIExt.TextChanged
        btnNewTVMetaDataFT.Enabled = Not String.IsNullOrEmpty(txtTVDefFIExt.Text) AndAlso Not Me.lstTVMetaData.Items.Contains(If(txtTVDefFIExt.Text.StartsWith("."), txtTVDefFIExt.Text, String.Concat(".", txtTVDefFIExt.Text)))
        If btnNewTVMetaDataFT.Enabled Then
            btnEditTVMetaDataFT.Enabled = False
            btnRemoveTVMetaDataFT.Enabled = False
        End If
    End Sub

    Private Sub ValidateRegex()
        If Not String.IsNullOrEmpty(Me.txtSeasonRegex.Text) AndAlso Not String.IsNullOrEmpty(Me.txtEpRegex.Text) Then
            If Me.cboSeasonRetrieve.SelectedIndex > -1 AndAlso Me.cboEpRetrieve.SelectedIndex > -1 Then
                Me.btnAddShowRegex.Enabled = True
            Else
                Me.btnAddShowRegex.Enabled = False
            End If
        End If
    End Sub

    Class ListViewItemComparer
        Implements IComparer
        Private col As Integer
        Public Sub New()
            col = 0
        End Sub
        Public Sub New(ByVal column As Integer)
            col = column
        End Sub
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer _
           Implements IComparer.Compare
            Return [String].Compare(CType(x, ListViewItem).SubItems(col).Text, CType(y, ListViewItem).SubItems(col).Text)
        End Function
    End Class

#End Region 'Methods

End Class