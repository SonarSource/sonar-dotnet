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

Public Class frmXMLMediaSettingsHolder
    Public Event SetupScraperChanged(ByVal state As Boolean, ByVal difforder As Integer)
    Public Event ModuleSettingsChanged()
    Public Event PopulateScrapers()
    Public parentRunning As Boolean = False

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEnabled.CheckedChanged
        RaiseEvent SetupScraperChanged(cbEnabled.Checked, 0)
    End Sub

    Private Sub cbScraper_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RaiseEvent ModuleSettingsChanged()
    End Sub
    Private Sub frmXMLSettingsHolder_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    End Sub

    Sub SetUp()
        Me.cbEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
		Me.Label1.Text = String.Concat(Master.eLang.GetString(8, "Use the Settings in"), " ", Master.eLang.GetString(556, "Scrapers - Data", True))
		Me.Label2.Text = Master.eLang.GetString(168, "Scrape Order", True)
    End Sub
    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Try
            Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberXMLScraperModule._AssemblyName).PostScraperOrder
            If order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).Count - 1 Then
                ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.PostScraperOrder = order + 1).PostScraperOrder = order
                ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberXMLScraperModule._AssemblyName).PostScraperOrder = order + 1
                RaiseEvent SetupScraperChanged(cbEnabled.Checked, 1)
                orderChanged()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub orderChanged()
        Try
            Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberXMLScraperModule._AssemblyName).PostScraperOrder
            btnDown.Enabled = (order < ModulesManager.Instance.externalScrapersModules.Where(Function(y) y.ProcessorModule.IsPostScraper).Count - 1)
            btnUp.Enabled = (order > 1)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Try
            Dim order As Integer = ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberXMLScraperModule._AssemblyName).PostScraperOrder
            If order > 0 Then
                ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.PostScraperOrder = order - 1).PostScraperOrder = order
                ModulesManager.Instance.externalScrapersModules.FirstOrDefault(Function(p) p.AssemblyName = EmberXMLScraperModule._AssemblyName).PostScraperOrder = order - 1
                RaiseEvent SetupScraperChanged(cbEnabled.Checked, -1)
                orderChanged()
            End If
        Catch ex As Exception
        End Try
    End Sub
End Class