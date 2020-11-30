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
Imports System.IO
Imports System.Xml.Serialization

Public Class NMTExporterModule
    Implements Interfaces.EmberExternalModule

    ' This will Control What Templates are valid!
    Public Shared MinDesignVersion As Single = 1.2

#Region "Fields"
	Private _AssemblyName As String = String.Empty
	Private WithEvents MyMenu As New System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MyTrayMenu As New System.Windows.Forms.ToolStripMenuItem
    Private _enabled As Boolean = False
    Private _Name As String = "NMT Jukebox Builder"
    Private _setup As frmSettingsHolder
    Private sBasePath As String
#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object)) Implements Interfaces.EmberExternalModule.GenericEvent

    Public Event ModuleEnabledChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements Interfaces.EmberExternalModule.ModuleSetupChanged

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberExternalModule.ModuleSettingsChanged

#End Region 'Events

#Region "Properties"

    Public ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType) Implements Interfaces.EmberExternalModule.ModuleType
        Get
            Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic, Enums.ModuleEventType.MovieSync})
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
            Dim movie As New Structures.DBMovie
            Select Case mType
                Case Enums.ModuleEventType.MovieSync
                    movie = DirectCast(_refparam, Structures.DBMovie)
                    dlgNMTMovies.dtMovieMedia = Nothing
                    ' TODO
                Case Enums.ModuleEventType.CommandLine
                    dlgNMTMovies.ExportSingle()
            End Select
        Catch ex As Exception
        End Try
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Sub Disable()
        Dim tsi As New ToolStripMenuItem
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Remove(MyMenu)
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("cmnuTrayIconTools"), ToolStripMenuItem)
        If Not tsi Is Nothing Then tsi.DropDownItems.Remove(MyTrayMenu)
    End Sub

    Sub Enable()
        Dim tsi As New ToolStripMenuItem
        MyMenu.Image = New Bitmap(My.Resources.icon)
		MyMenu.Text = Master.eLang.GetString(0, "NMT Jukebox Builder")
        MyMenu.Tag = New Structures.ModulesMenus With {.IfNoMovies = True, .IfNoTVShow = True}
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Add(MyMenu)
        MyTrayMenu.Image = New Bitmap(My.Resources.icon)
		MyTrayMenu.Text = Master.eLang.GetString(0, "NMT Jukebox Builder")
        MyTrayMenu.Tag = New Structures.ModulesMenus With {.IfNoMovies = True, .IfNoTVShow = True}
        tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayIconTools"), ToolStripMenuItem)
        If Not tsi Is Nothing Then tsi.DropDownItems.Add(MyTrayMenu)
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
		sBasePath = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
		If Not Directory.Exists(sBasePath) Then
			Directory.CreateDirectory(sBasePath)
		End If
		If Not Directory.Exists(Path.Combine(sBasePath, "Templates")) Then
			Directory.CreateDirectory(Path.Combine(sBasePath, "Templates"))
		End If
		If Not Directory.Exists(Path.Combine(sBasePath, "Temp")) Then
			Directory.CreateDirectory(Path.Combine(sBasePath, "Temp"))
		End If

	End Sub

    Function InjectSetup() As Containers.SettingsPanel Implements Interfaces.EmberExternalModule.InjectSetup
        Me._setup = New frmSettingsHolder
        Me._setup.cbEnabled.Checked = Me._enabled
        Dim SPanel As New Containers.SettingsPanel
        SPanel.Name = Me._Name
        SPanel.Text = Master.eLang.GetString(0, "NMT Jukebox Builder")
        SPanel.Prefix = "NMT_"
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

        Using dNMTMovies As New dlgNMTMovies
            dNMTMovies.ShowDialog()
        End Using

        RaiseEvent GenericEvent(Enums.ModuleEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", True}))
    End Sub

    Sub SaveSetup(ByVal DoDispose As Boolean) Implements Interfaces.EmberExternalModule.SaveSetup
        Me.Enabled = Me._setup.cbEnabled.Checked
    End Sub

#End Region 'Methods
    Public Class Config
        Public Name As String
        Public Description As String
        Public Author As String
        Public Version As String
        Public DesignVersion As String
        Public WhatsNew As Boolean

        <XmlArrayItem("File")> _
        Public Files As New List(Of _File)
        <XmlArrayItem("Param")> _
        Public Params As New List(Of _Param)
        <XmlArray("Properties")> _
        <XmlArrayItem("Property")> _
        Public Properties As New List(Of _Property)
        <XmlArray("ImageProcessing")> _
        <XmlArrayItem("Image")> _
        Public ImageProcessing As New List(Of _ImageProcessing)

        <XmlIgnore()> _
        Public ReadMe As Boolean
        <XmlIgnore()> _
        Public TemplatePath As String

        Class _File
            Public Name As String
            Public DestPath As String
            Public Process As Boolean
            Public Type As String
        End Class
        Class _Param
            Public name As String
            Public type As String
            Public value As String
            Public access As String
            Public description As String
        End Class
        Class _Property
            Public label As String
            Public name As String
            Public description As String
            Public group As String
            Public type As String
            Public value As String
            <XmlArray("values")> _
            <XmlArrayItem("value")> _
            Public values As List(Of _value)
        End Class
        Class _value
            <XmlText()> _
            Public value As String
            <XmlAttribute("label")> _
            Public label As String
        End Class
        Class _ImageProcessing
            <XmlElement("Type")> _
            Public _type As String
            <XmlArray("Commands")> _
            <XmlArrayItem("Command")> _
            Public Commands As New List(Of _ImageProcessingCommand)
        End Class
        Class _ImageProcessingCommand
            Public execute As String
            Public params As String
            Public prefix As String
            Public sufix As String
        End Class
        Public Sub Save(ByVal fpath As String)
            Dim xmlSer As New XmlSerializer(GetType(Config))
            Using xmlSW As New StreamWriter(fpath)
                xmlSer.Serialize(xmlSW, Me)
            End Using
        End Sub
        Public Shared Function Load(ByVal fpath As String) As Config
            Dim conf As Config = Nothing
            Try
                If Not File.Exists(fpath) Then Return New Config
                Dim xmlSer As XmlSerializer
                xmlSer = New XmlSerializer(GetType(Config))
                Using xmlSW As New StreamReader(Path.Combine(Functions.AppPath, fpath))
                    conf = DirectCast(xmlSer.Deserialize(xmlSW), Config)
                End Using
                conf.TemplatePath = Path.GetDirectoryName(fpath)
                conf.Version = If(String.IsNullOrEmpty(conf.Version), String.Empty, conf.Version)
                conf.Author = If(String.IsNullOrEmpty(conf.Author), String.Empty, conf.Author)
                conf.ReadMe = File.Exists(Path.Combine(conf.TemplatePath, "readme.txt"))
                For Each p As _Property In conf.Properties
                    p.value = If(String.IsNullOrEmpty(p.value), String.Empty, p.value)
                Next
            Catch ex As Exception
            End Try
            Return conf
        End Function
    End Class
End Class