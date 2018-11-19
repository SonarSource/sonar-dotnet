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
Imports System.Threading
Imports System.Net.Sockets
Imports System.Net
Imports System.Xml.Serialization

Public Class WebServerModule
    Implements Interfaces.EmberExternalModule

    <XmlRoot("MimeTypes")> _
Class _Mimetypes
        Structure MimeType
            <XmlAttribute("Ext")> _
            Public Ext As String
            <XmlText()> _
            Public MineType As String
        End Structure
        <XmlArray("List")> _
         Public MimeTypes As New List(Of MimeType)
    End Class
    Public Shared Function Load(ByVal fpath As String) As _Mimetypes
        If Not File.Exists(fpath) Then Return New _Mimetypes
        Dim xmlSer As XmlSerializer
        xmlSer = New XmlSerializer(GetType(_Mimetypes))
        Using xmlSW As New StreamReader(Path.Combine(Functions.AppPath, fpath))
            Return DirectCast(xmlSer.Deserialize(xmlSW), _Mimetypes)
        End Using
    End Function

#Region "Fields"
    Private _enabled As Boolean = False
	Private _AssemblyName As String = String.Empty
	Private _Name As String = "Web Server"
    Private _setup As frmSettingsHolder
    Private DoStop As Boolean = False
    Private sBasePath As String
    Private sPhysPath As String
    Private sLogPath As String
    Public Shared MimeTypes As New _Mimetypes
#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object)) Implements Interfaces.EmberExternalModule.GenericEvent

    Public Event ModuleEnabledChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements Interfaces.EmberExternalModule.ModuleSetupChanged

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberExternalModule.ModuleSettingsChanged

#End Region 'Events

#Region "Properties"

    Public ReadOnly Property ModuleType() As List(Of Enums.ModuleEventType) Implements Interfaces.EmberExternalModule.ModuleType
        Get
            Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic})
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
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Sub Disable()
        StopServer()
    End Sub

    Sub Enable()
        StartServer()
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
        SPanel.Text = Master.eLang.GetString(0, "Web Server")
        SPanel.Prefix = "WebServer_"
        SPanel.Type = Master.eLang.GetString(802, "Modules", True)
        SPanel.ImageIndex = If(Me._enabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = Me._setup.pnlSettings
        AddHandler Me._setup.ModuleEnabledChanged, AddressOf Handle_ModuleEnabledChanged
        AddHandler Me._setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Return SPanel
    End Function
    Sub SaveSetup(ByVal DoDispose As Boolean) Implements Interfaces.EmberExternalModule.SaveSetup
        Me.Enabled = Me._setup.cbEnabled.Checked
    End Sub

    Public Sub StartHTTPServer()
        ' Get IP address of the adapter to run the server on
        Dim address As IPAddress = IPAddress.Parse(AdvancedSettings.GetSetting("IP", "127.0.0.1"))
        ' map the end point with IP Address and Port
        Dim _EndPoint As IPEndPoint = New IPEndPoint(address, Int32.Parse(AdvancedSettings.GetSetting("TCP.Port", "80")))

        ' Create a new socket and bind it to the address and port and listen.
        Dim ss As Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Try
            ss.Bind(_EndPoint)
            ss.Listen(20)
        Catch ex As Exception
            DoStop = True
        End Try

        Dim WebRoot As String = sPhysPath
        Dim DefaultPage As String = AdvancedSettings.GetSetting("DefaultPage", "index.htme")


        Do While Not Me.DoStop
            ' Wait for an incoming connections
            Dim sock As Socket = ss.Accept()
            ' Connection accepted
            ' Initialise the Server class
            Dim ServerRun As New clsServer(sock, WebRoot, DefaultPage)
            ' Create a new thread to handle the connection
            Dim t As Thread = New Thread(AddressOf ServerRun.HandleConnection)
            t.IsBackground = True
            t.Priority = ThreadPriority.Normal
            t.Start()
            ' Loop and wait for more connections
        Loop

    End Sub

    Protected Sub StartServer()
        sBasePath = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
        If Not Directory.Exists(sBasePath) Then
            Directory.CreateDirectory(sBasePath)
        End If
        sPhysPath = Path.Combine(sBasePath, "html")
        If Not Directory.Exists(sPhysPath) Then
            Directory.CreateDirectory(sPhysPath)
        End If
        sLogPath = Path.Combine(sBasePath, "log")
        If Not Directory.Exists(sLogPath) Then
            Directory.CreateDirectory(sLogPath)
        End If

        Log("Ember WebServer started", sLogPath)

        MimeTypes = Load(Path.Combine(Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location)), "mimetype.xml"))

        ' Start the webserver on a thread then exit
        Dim t As New Thread(AddressOf StartHTTPServer)

        t.IsBackground = True
        t.Priority = ThreadPriority.Normal
        t.Start()

    End Sub

    Protected Sub StopServer()
        Log("Ember WebServer stopped", sLogPath)
        ' set the dostop variable so the thread exits normally
        Me.DoStop = True
    End Sub
#End Region 'Methods

End Class