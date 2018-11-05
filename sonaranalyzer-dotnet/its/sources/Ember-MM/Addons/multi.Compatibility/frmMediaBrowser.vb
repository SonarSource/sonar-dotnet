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

Public Class frmMediaBrowser

#Region "Events"
    Public Event ModuleEnabledChanged(ByVal State As Boolean, ByVal difforder As Integer)

    Public Event ModuleSettingsChanged()

    Public Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object))
#End Region 'Events

    Private Sub chkEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnabled.CheckedChanged
        RaiseEvent ModuleEnabledChanged(chkEnabled.Checked, 0)
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
    End Sub

    Private Sub SetUp()
        Me.chkEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
        Me.chkVideoTSParent.Text = Master.eLang.GetString(11, "Compatible VIDEO_TS File Placement/Naming")
        Me.chkBackdrop.Text = Master.eLang.GetString(12, "Fanart as backdrop.jpg")
		Me.chkMyMovies.Text = Master.eLang.GetString(13, "Media Browser mymovies.xml")
		Me.Label1.Text = Master.eLang.GetString(17, "Experimental module")
    End Sub

    Private Sub chkVideoTSParent_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkVideoTSParent.CheckedChanged
        RaiseEvent GenericEvent(Enums.ModuleEventType.SyncModuleSettings, Nothing)
        RaiseEvent ModuleSettingsChanged()
    End Sub
End Class