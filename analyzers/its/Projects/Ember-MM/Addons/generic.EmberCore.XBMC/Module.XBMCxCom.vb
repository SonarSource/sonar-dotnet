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

Imports System
Imports System.IO
Imports System.Xml.Serialization

Imports EmberAPI

Public Class XBMCxCom
    Implements Interfaces.EmberExternalModule

#Region "Fields"

    Private WithEvents MyMenu As New System.Windows.Forms.ToolStripMenuItem
    Private WithEvents MyTrayMenu As New System.Windows.Forms.ToolStripMenuItem
	Private _AssemblyName As String = String.Empty
	Private _enabled As Boolean = False
    Private _MySettings As New MySettings
    Private _name As String = "XBMC Controller"
    Private _setup As frmSettingsHolder
    Friend WithEvents bwRunUpdate As New System.ComponentModel.BackgroundWorker
    Private RunQueue As New Queue(Of Structures.DBMovie)
    Private _httpTimeOut As Integer = 10000
    Private _sendNotification As Boolean = False
#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object)) Implements EmberAPI.Interfaces.EmberExternalModule.GenericEvent

    Public Event ModuleEnabledChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements Interfaces.EmberExternalModule.ModuleSetupChanged

    Public Event ModuleSettingsChanged() Implements Interfaces.EmberExternalModule.ModuleSettingsChanged

#End Region 'Events

#Region "Properties"

    Public Property Enabled() As Boolean Implements EmberAPI.Interfaces.EmberExternalModule.Enabled
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

    Public ReadOnly Property ModuleName() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleName
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property ModuleType() As System.Collections.Generic.List(Of EmberAPI.Enums.ModuleEventType) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleType
        Get
            Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.MovieSync, Enums.ModuleEventType.BeforeEditMovie, Enums.ModuleEventType.ShowMovie, Enums.ModuleEventType.ShowTVShow})
        End Get
    End Property

    Public ReadOnly Property ModuleVersion() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

	Public Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements EmberAPI.Interfaces.EmberExternalModule.Init
		'_MySettings.XComs.AddRange(Master.eSettings.XBMCComs)
		_AssemblyName = sAssemblyName
		Master.eLang.LoadLanguage(Master.eSettings.Language, sExecutable)
		_MySettings = MySettings.Load
	End Sub

    Public Function InjectSetup() As EmberAPI.Containers.SettingsPanel Implements EmberAPI.Interfaces.EmberExternalModule.InjectSetup
        _MySettings = MySettings.Load
        Dim SPanel As New Containers.SettingsPanel
        Me._setup = New frmSettingsHolder
        Me._setup.cbEnabled.Checked = Me._enabled
        _setup.chkPlayCount.Checked = AdvancedSettings.GetBooleanSetting("XBMCSyncPlayCount", False)
        _setup.chkRealTime.Checked = AdvancedSettings.GetBooleanSetting("XBMCSync", False)
        _setup.chkNotification.Checked = AdvancedSettings.GetBooleanSetting("XBMCNotifications", False)
        _setup.XComs = _MySettings.XComs
        _setup.LoadXComs()
        SPanel.Name = Me._name
        SPanel.Text = Master.eLang.GetString(0, "XBMC Controller")
        SPanel.Prefix = "XBMCCom_"
        SPanel.Type = Master.eLang.GetString(802, "Modules", True)
        SPanel.ImageIndex = If(Me._enabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = Me._setup.pnlSettings
        AddHandler _setup.ModuleEnabledChanged, AddressOf Handle_SetupChanged
        AddHandler _setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Return SPanel
    End Function

    Private Function GetPlayCount(ByRef DBMovie As Structures.DBMovie, ByVal s As XBMCxCom.XBMCCom) As Integer
        If s Is Nothing Then Return 0
        Dim files As List(Of String()) = Nothing
        Dim str As String
        Dim eSource As String = String.Empty
        Try
            Dim source As String = DBMovie.Source
            eSource = Master.MovieSources.FirstOrDefault(Function(y) y.Name = source).Path
            'For Each s As XBMCxCom.XBMCCom In _MySettings.XComs.Where(Function(y) y.RealTime)
            Dim remoteSource As String = s.Paths(eSource).ToString
            If Not eSource.EndsWith(Path.DirectorySeparatorChar) Then eSource = String.Concat(eSource, Path.DirectorySeparatorChar)
            Dim remoteFullFilename As String = DBMovie.Filename.Replace(eSource, remoteSource)
            remoteFullFilename = remoteFullFilename.Replace(Path.DirectorySeparatorChar, s.RemotePathSeparator)
            Dim i As Integer = remoteFullFilename.LastIndexOf(s.RemotePathSeparator) + 1
            Dim RemotePath As String = remoteFullFilename.Substring(0, i)
            Dim RemoteFilename As String = remoteFullFilename.Substring(i)
            'Dim ret As String
            'Dim cmd As String
            str = String.Format("command=queryvideodatabase(select idFile,playCount from files inner join path on files.idpath = path.idpath Where path.strpath=""{0}"" and files.strfilename=""{1}"")", RemotePath, RemoteFilename)
            files = XBMCxCom.SplitResponse(XBMCxCom.SendCmd(s, str))
            If files.Count = 1 AndAlso files(0).Count >= 2 Then
                Dim id As String = files(0)(0)
                If IsNumeric(files(0)(1)) Then
                    Return Convert.ToInt32(files(0)(1))
                End If
            End If
            'Next
        Catch ex As Exception
        End Try
        Return 0
    End Function

	Private Sub bwRunUpdate_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwRunUpdate.DoWork
		Dim files As List(Of String()) = Nothing
		Dim str As String
		Dim eSource As String = String.Empty
		Try
			_sendNotification = AdvancedSettings.GetBooleanSetting("XBMCNotifications", False)
			_httpTimeOut = Convert.ToInt32(AdvancedSettings.GetSetting("HTTPTimeOut", "10000"))
			Dim DBMovie As Structures.DBMovie '= DirectCast(e.Argument, Structures.DBMovie)
			While RunQueue.Count > 0
				DBMovie = RunQueue.Dequeue
				eSource = Master.MovieSources.FirstOrDefault(Function(y) y.Name = DBMovie.Source).Path
				For Each s As XBMCxCom.XBMCCom In _MySettings.XComs.Where(Function(y) y.RealTime)
					If s.Paths.Count = 0 Then Continue For
					Dim remoteSource As String = s.Paths(eSource).ToString
					If Not eSource.EndsWith(Path.DirectorySeparatorChar) Then eSource = String.Concat(eSource, Path.DirectorySeparatorChar)
					Dim remoteFullFilename As String = DBMovie.Filename.Replace(eSource, remoteSource)
					remoteFullFilename = remoteFullFilename.Replace(Path.DirectorySeparatorChar, s.RemotePathSeparator)
					Dim i As Integer = remoteFullFilename.LastIndexOf(s.RemotePathSeparator) + 1
					Dim RemotePath As String = remoteFullFilename.Substring(0, i)
					Dim RemoteFilename As String = remoteFullFilename.Substring(i)
					Dim ret As String
					Dim cmd As String

					str = String.Format("command=queryvideodatabase(select movie.idMovie,files.idFile,path.strpath,files.strfilename,path.strcontent,path.strHash from movie inner join files on movie.idfile=files.idfile inner join path on files.idpath = path.idpath Where path.strpath=""{0}"" and files.strfilename=""{1}"")", RemotePath, RemoteFilename)
					files = XBMCxCom.SplitResponse(XBMCxCom.SendCmd(s, str))
					If files.Count = 1 AndAlso files(0).Count >= 6 Then
						If _sendNotification Then
							str = String.Format("command=ExecBuiltIn(Notification(EmberMM - Updating Movie,{0}))", DBMovie.Movie.Title)
							ret = SendCmd(s, str)
						End If
						Dim id As String = files(0)(0)
						Dim idfile As String = files(0)(1)
						If AdvancedSettings.GetBooleanSetting("XBMCSyncPlayCount", False) AndAlso s.Name = AdvancedSettings.GetSetting("XBMCSyncPlayCountHost", "") Then
							cmd = String.Concat("update files set ", _
								String.Format("playCount =""{0}"" ", StringEscape(If(IsNumeric(DBMovie.Movie.PlayCount), DBMovie.Movie.PlayCount, "0"))), _
								String.Format(" Where idFile ={0}", idfile))
							str = String.Format("command=execvideodatabase({0})", Web.HttpUtility.UrlEncode(cmd))
							ret = SendCmd(s, str)
							If Not ret.Contains("Exec Done") Then
								Master.eLog.WriteToErrorLog("Unable to Update XBMC PlayCount", cmd, "Error")
							End If
						End If

						' separated update so uri don't get too long
						cmd = String.Concat("update movie set ", _
							String.Format("c01 =""{0}"" ", StringEscape(DBMovie.Movie.Plot)), _
							String.Format(" Where idMovie ={0}", id))
						str = String.Format("command=execvideodatabase({0})", Web.HttpUtility.UrlEncode(cmd))
						ret = SendCmd(s, str)
						If Not ret.Contains("Exec Done") Then
							Master.eLog.WriteToErrorLog("Unable to Update XBMC Info - Plot", cmd, "Error")
						End If
						cmd = String.Concat("update movie set ", _
						String.Format("c00=""{0}"",", StringEscape(DBMovie.Movie.Title)), _
						String.Format("c02=""{0}"",", StringEscape(DBMovie.Movie.Outline)), _
						String.Format("c03=""{0}"",", StringEscape(DBMovie.Movie.Tagline)), _
						String.Format("c04=""{0}"",", StringEscape(DBMovie.Movie.Votes)), _
						String.Format("c05=""{0}"",", StringEscape(DBMovie.Movie.Rating)), _
						String.Format("c07=""{0}"",", StringEscape(DBMovie.Movie.Year)), _
						String.Format("c09=""{0}"",", StringEscape(DBMovie.Movie.IMDBID)), _
						String.Format("c11=""{0}"",", StringEscape(DBMovie.Movie.Runtime)), _
						String.Format("c12=""{0}"",", StringEscape(DBMovie.Movie.MPAA)), _
						String.Format("c14=""{0}"",", StringEscape(DBMovie.Movie.Genre)), _
						String.Format("c15=""{0}"",", StringEscape(DBMovie.Movie.Director)), _
						String.Format("c16=""{0}"",", StringEscape(DBMovie.Movie.OriginalTitle)), _
						String.Format("c18=""{0}"",", StringEscape(DBMovie.Movie.Studio)), _
						String.Format("c19=""{0}"",", StringEscape(DBMovie.Movie.Trailer)), _
						String.Format("c21=""{0}""", StringEscape(DBMovie.Movie.Country)), _
						String.Format(" Where idMovie ={0}", id.ToString))
						str = String.Format("command=execvideodatabase({0})", Web.HttpUtility.UrlEncode(cmd))
						ret = SendCmd(s, str)
						If Not ret.Contains("Exec Done") Then
							Master.eLog.WriteToErrorLog("Unable to Update XBMC Info", cmd, "Error")
						End If
						Dim imagefile As String
						Dim thumbpath As String
						Dim hash As String = XBMCHash(remoteFullFilename)
						If File.Exists(DBMovie.PosterPath) Then
							imagefile = String.Concat(RemotePath, Path.GetFileName(DBMovie.PosterPath))
							thumbpath = String.Format("special://profile/Thumbnails/Video/{0}/{1}", hash.Substring(0, 1), String.Concat(hash, ".tbn"))
							str = String.Format("command=FileCopy({0};{1})", imagefile, thumbpath)
							ret = SendCmd(s, str)
							If Not ret.Contains("OK") Then
								Master.eLog.WriteToErrorLog("Unable to Update XBMC Poster", str, "Error")
							End If
						End If
						If File.Exists(DBMovie.FanartPath) Then
							imagefile = String.Concat(RemotePath, Path.GetFileName(DBMovie.FanartPath))
							thumbpath = String.Format("special://profile/Thumbnails/Video/Fanart/{0}", String.Concat(hash, ".tbn"))
							str = String.Format("command=FileCopy({0};{1})", imagefile, thumbpath)
							ret = SendCmd(s, str)
							If Not ret.Contains("OK") Then
								Master.eLog.WriteToErrorLog("Unable to Update XBMC Fanart", str, "Error")
							End If
						End If
					End If
				Next
			End While
		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
		End Try
	End Sub

    Public Function RunGeneric(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object), ByRef _refparam As Object) As EmberAPI.Interfaces.ModuleResult Implements EmberAPI.Interfaces.EmberExternalModule.RunGeneric
        Select Case True
            Case mType = Enums.ModuleEventType.MovieSync AndAlso AdvancedSettings.GetBooleanSetting("XBMCSync", False)
                Dim DBMovie As Structures.DBMovie = DirectCast(_refparam, Structures.DBMovie)
                RunQueue.Enqueue(DBMovie)
                If Not bwRunUpdate.IsBusy Then
                    bwRunUpdate.RunWorkerAsync()
                End If
            Case mType = Enums.ModuleEventType.BeforeEditMovie AndAlso AdvancedSettings.GetBooleanSetting("XBMCSyncPlayCount", False)
                Dim DBMovie As Structures.DBMovie = DirectCast(_refparam, Structures.DBMovie)
                Dim c As Integer = GetPlayCount(DBMovie, _MySettings.XComs.FirstOrDefault(Function(y) y.Name = AdvancedSettings.GetSetting("XBMCSyncPlayCountHost", "")))
                If IsNumeric(DBMovie.Movie.PlayCount) Then
                    DBMovie.Movie.PlayCount = Math.Max(Convert.ToInt32(DBMovie.Movie.PlayCount), c).ToString
                Else
                    DBMovie.Movie.PlayCount = c.ToString
                End If
            Case mType = Enums.ModuleEventType.ShowMovie
                Dim DBMovie As Structures.DBMovie = DirectCast(_refparam, Structures.DBMovie)
        End Select

	End Function

    Function StringEscape(ByVal str As String) As String
        Return str.Replace("""", """""").Replace(";", ";;").Replace("&", "&&")
	End Function

    Public Sub SaveSetup(ByVal DoDispose As Boolean) Implements EmberAPI.Interfaces.EmberExternalModule.SaveSetup
        Me.Enabled = _setup.cbEnabled.Checked
        _MySettings.XComs = _setup.XComs
        MySettings.Save(_MySettings)
        If _setup.cbPlayCountHost.SelectedIndex >= 0 Then AdvancedSettings.SetSetting("XBMCSyncPlayCountHost", _setup.cbPlayCountHost.SelectedItem.ToString)
        AdvancedSettings.SetBooleanSetting("XBMCSyncPlayCount", _setup.chkPlayCount.Checked)
        AdvancedSettings.SetBooleanSetting("XBMCSync", _setup.chkRealTime.Checked)
        AdvancedSettings.SetBooleanSetting("XBMCNotifications", _setup.chkNotification.Checked)
        If Me._enabled Then
            Me.Disable()
            Me.Enable()
        End If
    End Sub

    Sub Disable()
        Try
            Dim tsb As New ToolStripSplitButton
            tsb = DirectCast(ModulesManager.Instance.RuntimeObjects.MainTool.Items("tsbMediaCenters"), ToolStripSplitButton)
            tsb.DropDownItems.Remove(MyMenu)
            MyMenu.DropDownItems.Clear()
            tsb.Visible = (tsb.DropDownItems.Count > 0)
            Dim tsi As New ToolStripMenuItem
            tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayIconMediaCenters"), ToolStripMenuItem)
            tsi.DropDownItems.Remove(MyTrayMenu)
            tsi.Visible = (tsi.DropDownItems.Count > 0)
        Catch ex As Exception
        End Try
	End Sub

    Sub Enable()
        Try
            _MySettings = MySettings.Load
            Dim tSettingsHolder As New frmSettingsHolder
            Dim tsb As New ToolStripSplitButton
            MyMenu.Image = New Bitmap(tSettingsHolder.Icon.ToBitmap)
            MyMenu.Text = Master.eLang.GetString(18, "XBMC")
            tsb = DirectCast(ModulesManager.Instance.RuntimeObjects.MainTool.Items("tsbMediaCenters"), ToolStripSplitButton)
            Dim tsi As New ToolStripMenuItem
            MyTrayMenu.Image = New Bitmap(tSettingsHolder.Icon.ToBitmap)
            MyTrayMenu.Text = Master.eLang.GetString(19, "XBMC Controller")
            tsi = DirectCast(ModulesManager.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayIconMediaCenters"), ToolStripMenuItem)
            tSettingsHolder.Dispose()
            MyMenu.DropDownItems.Clear()
            MyTrayMenu.DropDownItems.Clear()
            If _MySettings.XComs.Count > 0 Then
                Dim tMenu As New System.Windows.Forms.ToolStripMenuItem With {.Text = Master.eLang.GetString(649, "Update All", True), .Tag = Nothing}
                AddHandler tMenu.Click, AddressOf xCom_Click
                MyMenu.DropDownItems.Add(tMenu)
                Dim tTrayMenu As New System.Windows.Forms.ToolStripMenuItem With {.Text = Master.eLang.GetString(649, "Update All", True), .Tag = Nothing}
                AddHandler tTrayMenu.Click, AddressOf xCom_Click
                MyTrayMenu.DropDownItems.Add(tTrayMenu)

                For Each xCom As XBMCCom In _MySettings.XComs
                    tMenu = New System.Windows.Forms.ToolStripMenuItem With {.Text = String.Format(Master.eLang.GetString(143, "Update {0} Only", True), xCom.Name), .Tag = xCom, .DropDownDirection = ToolStripDropDownDirection.Left}
                    AddHandler tMenu.Click, AddressOf xCom_Click
                    MyMenu.DropDownItems.Add(tMenu)
                    tTrayMenu = New System.Windows.Forms.ToolStripMenuItem With {.Text = String.Format(Master.eLang.GetString(143, "Update {0} Only", True), xCom.Name), .Tag = xCom, .DropDownDirection = ToolStripDropDownDirection.Left}
                    AddHandler tTrayMenu.Click, AddressOf xCom_Click
                    MyTrayMenu.DropDownItems.Add(tTrayMenu)
                Next
                tsb.DropDownItems.Add(MyMenu)
                tsi.DropDownItems.Add(MyTrayMenu)
            End If
            tsb.Visible = (tsb.DropDownItems.Count > 0)
            tsi.Visible = (tsi.DropDownItems.Count > 0)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub Handle_SetupChanged(ByVal state As Boolean, ByVal difforder As Integer)
        RaiseEvent ModuleEnabledChanged(Me._name, state, difforder)
    End Sub

    Private Sub xCom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim tMenu As New System.Windows.Forms.ToolStripMenuItem
        tMenu = DirectCast(sender, System.Windows.Forms.ToolStripMenuItem)
        If tMenu.Tag Is Nothing Then
            Try
                For Each tCom As XBMCCom In _MySettings.XComs
                    SendCmd(tCom, "command=ExecBuiltIn(XBMC.updatelibrary(video))")
                Next
            Catch
            End Try
        Else
            Dim xCom As XBMCCom = DirectCast(tMenu.Tag, XBMCCom)
            SendCmd(xCom, "command=ExecBuiltIn(XBMC.updatelibrary(video))")

        End If
    End Sub

    Public Shared Function SendCmd(ByVal xCom As XBMCCom, ByVal str As String) As String
        Dim Wr As HttpWebRequest
        Dim Sr As String = String.Empty
        Dim retry As Integer = 3
        Dim needRetry As Boolean = False
        Dim errorMessage As String = String.Empty

        Do
            needRetry = False
            Try
                Wr = DirectCast(HttpWebRequest.Create(String.Format("http://{0}:{1}/xbmcCmds/xbmcHttp?{2}", xCom.IP, xCom.Port, str)), HttpWebRequest)
                Wr.Timeout = 15000
                If Not String.IsNullOrEmpty(xCom.Username) AndAlso Not String.IsNullOrEmpty(xCom.Password) Then
                    Wr.Credentials = New NetworkCredential(xCom.Username, xCom.Password)
                End If
                Using Wres As HttpWebResponse = DirectCast(Wr.GetResponse, HttpWebResponse)
                    Sr = New StreamReader(Wres.GetResponseStream()).ReadToEnd
                End Using
                Wr = Nothing
                Sr = Sr.Replace("<html>", String.Empty).Replace("</html>", String.Empty)
            Catch ex As Exception
                needRetry = True
                retry -= 1
                errorMessage = ex.Message
            End Try
        Loop While needRetry AndAlso retry > 0
        If needRetry AndAlso retry <= 0 Then
            MsgBox(errorMessage, MsgBoxStyle.OkOnly, "XBMC Controller")
        End If
        Return Sr
    End Function

    Public Shared Function SplitResponse(ByVal sr As String) As List(Of String())
        Dim rec As New List(Of String())
        Dim trec() As String = Nothing
        sr = sr.Replace("<record>", "")
        trec = sr.Split(New String() {"</record>"}, StringSplitOptions.RemoveEmptyEntries)
        For Each t As String In trec
            Dim tt As String() = t.Replace("<field>", "").Split(New String() {"</field>"}, StringSplitOptions.None)
            rec.Add(tt)
        Next
        Return rec
    End Function

    Public Function XBMCHash(ByVal input As String) As String
        Dim chars As Char() = input.ToCharArray()
        Dim index As Integer = 0
        While index < chars.Length
            'If Convert.ToSByte(chars(index)) <= 127 Then
            chars(index) = System.[Char].ToLowerInvariant(chars(index))
            'End If
            System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1)
        End While
        input = New String(chars)
        Dim m_crc As UInteger = 4294967295
        Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(input)
        For Each myByte As Byte In bytes
            m_crc = m_crc Xor (System.Convert.ToUInt32(myByte) << 24)
            Dim i As Integer = 0
            While i < 8
                If (System.Convert.ToUInt32(m_crc) And System.Convert.ToUInt32(2147483648)) = (2147483648) Then
                    m_crc = (m_crc << 1) Xor System.Convert.ToUInt32(&H4C11DB7)
                Else
                    m_crc <<= 1
                End If
                System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
            End While
        Next
        Return [String].Format("{0:x8}", m_crc)
    End Function

#End Region 'Methods

#Region "Nested Types"

    Public Class XBMCCom

#Region "Fields"

        Private _xbmcip As String
        Private _xbmcname As String
        Private _xbmcpassword As String
        Private _xbmcport As String
        Private _xbmcusername As String
        Private _paths As Hashtable
        Private _RemotePathSeparator As String
        Private _realtime As Boolean
#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"
        Public Property Paths() As Hashtable
            Get
                Return Me._paths
            End Get
            Set(ByVal value As Hashtable)
                Me._paths = value
            End Set
        End Property
        Public Property RemotePathSeparator() As String
            Get
                Return Me._RemotePathSeparator
            End Get
            Set(ByVal value As String)
                Me._RemotePathSeparator = value
            End Set
        End Property
        Public Property RealTime() As Boolean
            Get
                Return Me._realtime
            End Get
            Set(ByVal value As Boolean)
                Me._realtime = value
            End Set
        End Property

        Public Property IP() As String
            Get
                Return Me._xbmcip
            End Get
            Set(ByVal value As String)
                Me._xbmcip = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return Me._xbmcname
            End Get
            Set(ByVal value As String)
                Me._xbmcname = value
            End Set
        End Property

        Public Property Password() As String
            Get
                If String.IsNullOrEmpty(Me._xbmcpassword) Then
                    Return String.Empty
                Else
                    Return StringUtils.Decode(Me._xbmcpassword)
                End If
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then
                    Me._xbmcpassword = value
                Else
                    Me._xbmcpassword = StringUtils.Encode(value)
                End If
            End Set
        End Property

        Public Property Port() As String
            Get
                Return Me._xbmcport
            End Get
            Set(ByVal value As String)
                Me._xbmcport = value
            End Set
        End Property

        Public Property Username() As String
            Get
                Return Me._xbmcusername
            End Get
            Set(ByVal value As String)
                Me._xbmcusername = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._xbmcname = String.Empty
            Me._xbmcip = String.Empty
            Me._xbmcport = String.Empty
            Me._xbmcusername = String.Empty
            Me._xbmcpassword = String.Empty
            Me._RemotePathSeparator = Path.DirectorySeparatorChar
        End Sub

#End Region 'Methods

    End Class

    Class MySettings

#Region "Fields"

        Public XComs As New List(Of XBMCxCom.XBMCCom)

#End Region 'Fields

#Region "Methods"

        Public Shared Function Load() As MySettings
            Dim tmp As New MySettings
            Try
                Dim Asett As New Hashtable
                Asett = AdvancedSettings.GetComplexSetting("XBMCHosts")
                If Not Asett Is Nothing Then
                    For Each n In Asett.Keys
                        Dim t As New XBMCxCom.XBMCCom
                        Dim Aett As New Hashtable
                        Aett = AdvancedSettings.GetComplexSetting(String.Concat("XBMCHosts.", n.ToString))
                        For Each k In Aett.Keys
                            Select Case k.ToString
                                Case "Name"
                                    t.Name = Aett.Item("Name").ToString
                                Case "IP"
                                    t.IP = Aett.Item("IP").ToString
                                Case "Port"
                                    t.Port = Aett.Item("Port").ToString
                                Case "Username"
                                    t.Username = Aett.Item("Username").ToString
                                Case "Password"
                                    t.Password = Aett.Item("Password").ToString
                                Case "RemotePathSeparator"
                                    t.RemotePathSeparator = Aett.Item("RemotePathSeparator").ToString
                                Case "RealTime"
                                    t.RealTime = Convert.ToBoolean(Aett.Item("RealTime").ToString)
                            End Select
                        Next
                        Dim AsettPath As New Hashtable
                        AsettPath = AdvancedSettings.GetComplexSetting(String.Concat("XBMCHosts.Paths", ".", t.Name))
                        If Not AsettPath Is Nothing Then
                            t.Paths = AsettPath
                        End If
                        tmp.XComs.Add(t)
                    Next

                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                tmp = New MySettings
            End Try
            Return tmp
        End Function

        Public Shared Sub Save(ByVal tmp As MySettings)
            Try

                Dim Asett As New Hashtable
                For Each t As XBMCxCom.XBMCCom In tmp.XComs
                    Dim h As New Hashtable
                    h.Add("Name", t.Name)
                    h.Add("IP", t.IP)
                    h.Add("Port", t.Port)
                    h.Add("Username", t.Username)
                    h.Add("Password", t.Password)
                    h.Add("RemotePathSeparator", t.RemotePathSeparator)
                    h.Add("RealTime", t.RealTime.ToString)
                    Asett.Add(t.Name, t.Name)
                    Dim name As String = t.Name
                    If Not t.Paths Is Nothing AndAlso t.Paths.Count > 0 Then
                        Dim AsettPath As New Hashtable
                        AsettPath = DirectCast(t.Paths.Clone, Hashtable)
                        AdvancedSettings.ClearComplexSetting(String.Concat("XBMCHosts.Paths.", name))
                        AdvancedSettings.SetComplexSetting(String.Concat("XBMCHosts.Paths.", name), AsettPath)
                    End If
                    AdvancedSettings.ClearComplexSetting(String.Concat("XBMCHosts.", name))
                    AdvancedSettings.SetComplexSetting(String.Concat("XBMCHosts.", name), h)
                Next
                AdvancedSettings.ClearComplexSetting("XBMCHosts")
                AdvancedSettings.SetComplexSetting("XBMCHosts", Asett)
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class