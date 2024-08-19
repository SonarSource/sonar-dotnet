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

Imports EmberAPI

Public Class MovieExporterModule
    Implements Interfaces.EmberExternalModule

#Region "Fields"

    Private WithEvents MyMenu As New System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MyTrayMenu As New System.Windows.Forms.ToolStripMenuItem
	Private _AssemblyName As String = String.Empty
	Private _enabled As Boolean = False
    Private _Name As String = "Movie List Exporter"
    Private _setup As frmSettingsHolder

#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object)) Implements Interfaces.EmberExternalModule.GenericEvent

    Public Event ModuleEnabledChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements Interfaces.EmberExternalModule.ModuleSetupChanged

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberExternalModule.ModuleSettingsChanged

#End Region 'Events

#Region "Properties"

    Public ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType) Implements Interfaces.EmberExternalModule.ModuleType
        Get
            Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic, Enums.ModuleEventType.CommandLine})
        End Get
    End Property

    Property Enabled() As Boolean Implements Interfaces.EmberExternalModule.Enabled
        Get
            Return _enabled
        End Get
        Set(ByVal value As Boolean)
            If _enabled = value Then Return
            _enabled = value
            If _enabled Then
                Enable()
            Else
                Disable()
            End If
        End Set
    End Property

    ReadOnly Property ModuleName() As String Implements Interfaces.EmberExternalModule.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.EmberExternalModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Public Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), ByRef _refparam As Object) As Interfaces.ModuleResult Implements Interfaces.EmberExternalModule.RunGeneric
        Try
            dlgExportMovies.CLExport(DirectCast(_params(0), String), DirectCast(_params(1), String), DirectCast(_params(2), Int32))

        Catch ex As Exception
        End Try

        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Sub Disable()
        Dim tsi As New ToolStripMenuItem
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Remove(MyMenu)
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("cmnuTrayIconTools"), ToolStripMenuItem)
        tsi.DropDownItems.Remove(MyTrayMenu)
    End Sub

    Sub Enable()
        Dim tsi As New ToolStripMenuItem
        MyMenu.Image = New Bitmap(My.Resources.icon)
        MyMenu.Text = Master.eLang.GetString(15, "Export Movie List")
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Add(MyMenu)
        MyTrayMenu.Image = New Bitmap(My.Resources.icon)
        MyTrayMenu.Text = Master.eLang.GetString(15, "Export Movie List")
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayIconTools"), ToolStripMenuItem)
        tsi.DropDownItems.Add(MyTrayMenu)
    End Sub

    Private Sub Handle_ModuleEnabledChanged(ByVal State As Boolean)
        RaiseEvent ModuleEnabledChanged(Me._Name, State, 0)
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

	Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements Interfaces.EmberExternalModule.Init
		_AssemblyName = sAssemblyName
		Master.eLang.LoadLanguage(Master.eSettings.Language, sExecutable)
	End Sub

    Function InjectSetup() As Containers.SettingsPanel Implements Interfaces.EmberExternalModule.InjectSetup
        Me._setup = New frmSettingsHolder
        Me._setup.cbEnabled.Checked = Me._enabled
        Dim SPanel As New Containers.SettingsPanel
        SPanel.Name = Me._Name
        SPanel.Text = Master.eLang.GetString(0, "Movie List Exporter")
        SPanel.Prefix = "Exporter_"
        SPanel.Type = Master.eLang.GetString(802, "Modules", True)
        SPanel.ImageIndex = If(Me._enabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = Me._setup.pnlSettings
        AddHandler Me._setup.ModuleEnabledChanged, AddressOf Handle_ModuleEnabledChanged
        AddHandler Me._setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Return SPanel
    End Function

    Private Sub MyMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyMenu.Click, MyTrayMenu.Click
        RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", False}))

        Using dExportMovies As New dlgExportMovies
            dExportMovies.ShowDialog()
        End Using

        RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", True}))
    End Sub

    Sub SaveSetup(ByVal DoDispose As Boolean) Implements Interfaces.EmberExternalModule.SaveSetup
        Me.Enabled = Me._setup.cbEnabled.Checked
    End Sub

#End Region 'Methods

End Class