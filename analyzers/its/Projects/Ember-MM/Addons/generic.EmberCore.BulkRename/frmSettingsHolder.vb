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

Public Class frmSettingsHolder

#Region "Events"

    Public Event ModuleEnabledChanged(ByVal State As Boolean, ByVal difforder As Integer)

    Public Event ModuleSettingsChanged()

#End Region 'Events

#Region "Methods"

    Private Sub chkBulRenamer_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBulkRenamer.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleEnabledChanged(chkEnabled.Checked, 0)
    End Sub

    Private Sub chkGenericModule_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGenericModule.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkOnError_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkRenameMulti_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRenameMulti.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub chkRenameSingle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRenameSingle.CheckedChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Sub SetUp()
        Me.chkRenameMulti.Text = Master.eLang.GetString(21, "Automatically Rename Files During Multi-Scraper")
        Me.chkRenameSingle.Text = Master.eLang.GetString(22, "Automatically Rename Files During Single-Scraper")
        Me.gbRenamerPatterns.Text = Master.eLang.GetString(23, "Default Renaming Patterns")
        Me.lblFilePattern.Text = Master.eLang.GetString(24, "Files Pattern")
        Me.lblFolderPattern.Text = Master.eLang.GetString(25, "Folders Pattern")
        Me.chkEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
        Me.chkGenericModule.Text = Master.eLang.GetString(32, "Enable Generic Rename Module")
        Me.chkBulkRenamer.Text = Master.eLang.GetString(33, "Enable Bulk Renamer Tool")
        Me.Label1.Text = String.Format(Master.eLang.GetString(11, "$1 = First Letter of the Title{0}$A = Audio{0}$B = Base Path{0}$C = Director{0}$D = Directory{0}$E = Sort Title{0}$F = File Name{0}$G = Genre (Follow with a space, dot or hyphen to change separator){0}$I = IMDB ID{0}$L = List Title{0}$M = MPAA{0}$O = OriginalTitle{0}$R = Resolution{0}$S = Source{0}$T = Title{0}$U = Country{0}$Y = Year{0}$X. (Replace Space with .){0}{{}} = Optional{0}$?aaa?bbb? = Replace aaa with bbb{0}$- = Remove previous char if next pattern does not have a value{0}$+ = Remove next char if previous pattern does not have a value{0}$^ = Remove previous and next char if next pattern does not have a value"), vbNewLine)
    End Sub

    Private Sub txtFilePattern_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFilePattern.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub txtFolderPattern_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFolderPattern.TextChanged
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

#End Region 'Methods

    Private Sub frmSettingsHolder_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub pnlSettings_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlSettings.Paint

    End Sub
End Class