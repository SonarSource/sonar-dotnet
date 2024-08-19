Imports System.IO
Imports EmberAPI

Public Class genericYAMJ
    Implements Interfaces.EmberExternalModule

#Region "Fields"
	Private fYAMJ As frmYAMJ
	Private _enabled As Boolean = False
	Private _name As String = "YAMJ compatibility"
	Private _AssemblyName As String = String.Empty
#End Region	'Fields

#Region "Events"

	Public Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object)) Implements EmberAPI.Interfaces.EmberExternalModule.GenericEvent

	Public Event ModuleSettingsChanged() Implements EmberAPI.Interfaces.EmberExternalModule.ModuleSettingsChanged

	Public Event ModuleSetupChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleSetupChanged

#End Region	'Events

#Region "Properties"
	Public Property Enabled() As Boolean Implements EmberAPI.Interfaces.EmberExternalModule.Enabled
		Get
			Return _enabled
		End Get
		Set(ByVal value As Boolean)
			If _enabled = value Then Return
			_enabled = value
			If _enabled Then
				'Enable()
			Else
				'Disable()
			End If
		End Set
	End Property

	Public ReadOnly Property ModuleName() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleName
		Get
			Return "YAMJ Compatibility"
		End Get
	End Property

	Public ReadOnly Property ModuleType() As System.Collections.Generic.List(Of EmberAPI.Enums.ModuleEventType) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleType
		Get
			Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic, Enums.ModuleEventType.TVImageNaming, Enums.ModuleEventType.OnMovieNFOSave})
		End Get
	End Property

	Public ReadOnly Property ModuleVersion() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleVersion
		Get
			Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
		End Get
	End Property

#End Region	'Properties

#Region "Methods"

	Public Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements EmberAPI.Interfaces.EmberExternalModule.Init
		_AssemblyName = sAssemblyName
		Master.eLang.LoadLanguage(Master.eSettings.Language, sExecutable)
	End Sub

	Public Function InjectSetup() As EmberAPI.Containers.SettingsPanel Implements EmberAPI.Interfaces.EmberExternalModule.InjectSetup
		Dim SPanel As New Containers.SettingsPanel
		Me.fYAMJ = New frmYAMJ
		Me.fYAMJ.chkEnabled.Checked = Me._enabled
		Me.fYAMJ.chkVideoTSParent.Checked = Master.eSettings.VideoTSParent
		Me.fYAMJ.chkYAMJCompatibleSets.Checked = Master.eSettings.YAMJSetsCompatible
		Me.fYAMJ.chkYAMJCompatibleTVImages.Checked = AdvancedSettings.GetBooleanSetting("YAMJTVImageNaming", False)
		Me.fYAMJ.chkYAMJnfoFields.Checked = AdvancedSettings.GetBooleanSetting("YAMJnfoFields", False)
		Me.fYAMJ.chkShowPoster.Checked = AdvancedSettings.GetBooleanSetting("YAMJShowPoster", False)
		Me.fYAMJ.chkShowFanart.Checked = AdvancedSettings.GetBooleanSetting("YAMJShowFanart", False)
		Me.fYAMJ.chkSeasonPoster.Checked = AdvancedSettings.GetBooleanSetting("YAMJSeasonPoster", False)
		Me.fYAMJ.chkSeasonFanart.Checked = AdvancedSettings.GetBooleanSetting("YAMJSeasonFanart", False)
		Me.fYAMJ.chkEpisodePoster.Checked = AdvancedSettings.GetBooleanSetting("YAMJEpisodePoster", False)
		Me.fYAMJ.chkAllSeasonPoster.Checked = AdvancedSettings.GetBooleanSetting("YAMJAllSeasonPoster", False)
		'chkYAMJnfoFields
		SPanel.Name = _name
		SPanel.Text = Master.eLang.GetString(1, "YAMJ Compatibility")
		SPanel.Prefix = "YAMJ_"
		SPanel.Type = Master.eLang.GetString(802, "Modules", True)
		SPanel.ImageIndex = If(Me._enabled, 9, 10)
		SPanel.Order = 100
		SPanel.Panel = Me.fYAMJ.pnlSettings
		AddHandler Me.fYAMJ.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
		AddHandler fYAMJ.ModuleEnabledChanged, AddressOf Handle_SetupChanged
		AddHandler fYAMJ.GenericEvent, AddressOf DeploySyncSettings
		Return SPanel
		'Return Nothing
	End Function

	Private Sub Handle_ModuleSettingsChanged()
		RaiseEvent ModuleSettingsChanged()
	End Sub

	Private Sub Handle_SetupChanged(ByVal state As Boolean, ByVal difforder As Integer)
		RaiseEvent ModuleSetupChanged(Me._name, state, difforder)
	End Sub

	Public Sub SaveSetup(ByVal DoDispose As Boolean) Implements EmberAPI.Interfaces.EmberExternalModule.SaveSetup
		Me.Enabled = Me.fYAMJ.chkEnabled.Checked
		'Master.eSettings.VideoTSParent = Me.fYAMJ.chkVideoTSParent.Checked
		Master.eSettings.YAMJSetsCompatible = Me.fYAMJ.chkYAMJCompatibleSets.Checked
		AdvancedSettings.SetBooleanSetting("YAMJTVImageNaming", Me.fYAMJ.chkYAMJCompatibleTVImages.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJnfoFields", Me.fYAMJ.chkYAMJnfoFields.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJShowPoster", Me.fYAMJ.chkShowPoster.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJShowFanart", Me.fYAMJ.chkShowFanart.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJSeasonPoster", Me.fYAMJ.chkSeasonPoster.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJSeasonFanart", Me.fYAMJ.chkSeasonFanart.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJEpisodePoster", Me.fYAMJ.chkEpisodePoster.Checked)
		AdvancedSettings.SetBooleanSetting("YAMJAllSeasonPoster", Me.fYAMJ.chkAllSeasonPoster.Checked)
	End Sub

	Public Function RunGeneric(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object), ByRef _refparam As Object) As EmberAPI.Interfaces.ModuleResult Implements EmberAPI.Interfaces.EmberExternalModule.RunGeneric
		Dim iType As Enums.TVImageType
		Dim mShow As Structures.DBTV
		Dim imageList As List(Of String)
		Dim doContinue As Boolean
		Dim mMovie As Structures.DBMovie
		If Enabled Then
			Try
				Select Case mType
					Case Enums.ModuleEventType.TVImageNaming
						If AdvancedSettings.GetBooleanSetting("YAMJTVImageNaming", False) Then
							iType = DirectCast(_params(0), Enums.TVImageType)
							mShow = DirectCast(_params(1), Structures.DBTV)
							imageList = DirectCast(_params(2), List(Of String))
							doContinue = DirectCast(_refparam, Boolean)
							Dim tPath As String = String.Empty
							Select Case iType
								Case Enums.TVImageType.AllSeasonPoster
									If AdvancedSettings.GetBooleanSetting("YAMJAllSeasonPoster", False) Then
										Dim seasonPath As String = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, 0)
										If String.IsNullOrEmpty(seasonPath) Then
											Dim dtSeasons As New DataTable
											Master.DB.FillDataTable(dtSeasons, String.Concat("SELECT * FROM TVSeason WHERE TVShowID = ", mShow.ShowID, " AND Season <> 999 ORDER BY Season;"))
											If dtSeasons.Rows.Count > 0 Then
												seasonPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, Convert.ToInt32(dtSeasons.Rows(0).Item("Season").ToString))
											End If
										End If
										tPath = Path.Combine(mShow.ShowPath, seasonPath)
										'tPath = Path.Combine(tPath, String.Concat("Set_", FileUtils.Common.GetDirectory(mShow.ShowPath), "_1.banner.jpg"))
										tPath = Path.Combine(tPath, "Season 1.banner.jpg")
										imageList.Add(tPath)
										'doContinue = False
										'SET_<show>_1.banner.jpg
									End If
								Case Enums.TVImageType.EpisodePoster
									If AdvancedSettings.GetBooleanSetting("YAMJEpisodePoster", False) Then
										tPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), ".videoimage.jpg")
										imageList.Add(tPath)
										doContinue = False
									End If
								Case Enums.TVImageType.EpisodeFanart
									doContinue = False
								Case Enums.TVImageType.SeasonPoster
									If AdvancedSettings.GetBooleanSetting("YAMJSeasonPoster", False) Then
										Dim epPath As String = String.Empty
										Dim dtEpisodes As New DataTable
										Master.DB.FillDataTable(dtEpisodes, String.Concat("SELECT * FROM TVEps INNER JOIN TVEpPaths ON (TVEpPaths.ID = TVEpPathid) WHERE TVShowID = ", mShow.ShowID, " AND Season = ", mShow.TVEp.Season, " ORDER BY Episode;"))
										If dtEpisodes.Rows.Count > 0 Then
											epPath = dtEpisodes.Rows(0).Item("TVEpPath").ToString
											imageList.Add(Path.Combine(Path.GetDirectoryName(epPath), String.Concat(Path.GetFileNameWithoutExtension(epPath), ".jpg")))
											'doContinue = False
										End If
									End If
								Case Enums.TVImageType.SeasonFanart
									If AdvancedSettings.GetBooleanSetting("YAMJSeasonFanart", False) Then
										Dim epPath As String = String.Empty
										Dim dtEpisodes As New DataTable
										Master.DB.FillDataTable(dtEpisodes, String.Concat("SELECT * FROM TVEps INNER JOIN TVEpPaths ON (TVEpPaths.ID = TVEpPathid) WHERE TVShowID = ", mShow.ShowID, " AND Season = ", mShow.TVEp.Season, " ORDER BY Episode;"))
										If dtEpisodes.Rows.Count > 0 Then
											epPath = dtEpisodes.Rows(0).Item("TVEpPath").ToString
											imageList.Add(Path.Combine(Path.GetDirectoryName(epPath), String.Concat(Path.GetFileNameWithoutExtension(epPath), ".fanart.jpg")))
											'doContinue = False
										End If
									End If
								Case Enums.TVImageType.ShowPoster
									If AdvancedSettings.GetBooleanSetting("YAMJShowPoster", False) Then
										Dim seasonPath As String = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, 0)
										If String.IsNullOrEmpty(seasonPath) Then
											Dim dtSeasons As New DataTable
											Master.DB.FillDataTable(dtSeasons, String.Concat("SELECT * FROM TVSeason WHERE TVShowID = ", mShow.ShowID, " AND Season <> 999 ORDER BY Season;"))
											If dtSeasons.Rows.Count > 0 Then
												seasonPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, Convert.ToInt32(dtSeasons.Rows(0).Item("Season").ToString))
											End If
										End If
										tPath = Path.Combine(mShow.ShowPath, seasonPath)
										tPath = Path.Combine(tPath, String.Concat("Set_", FileUtils.Common.GetDirectory(mShow.ShowPath), "_1.jpg"))
										imageList.Add(tPath)
										'doContinue = False
										'SET_<show>_1.jpg
									End If
								Case Enums.TVImageType.ShowFanart
									If AdvancedSettings.GetBooleanSetting("YAMJShowFanart", False) Then
										Dim seasonPath As String = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, 0)
										If String.IsNullOrEmpty(seasonPath) Then
											Dim dtSeasons As New DataTable
											Master.DB.FillDataTable(dtSeasons, String.Concat("SELECT * FROM TVSeason WHERE TVShowID = ", mShow.ShowID, " AND Season <> 999 ORDER BY Season;"))
											If dtSeasons.Rows.Count > 0 Then
												seasonPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, Convert.ToInt32(dtSeasons.Rows(0).Item("Season").ToString))
											End If
										End If
										tPath = Path.Combine(mShow.ShowPath, seasonPath)
										tPath = Path.Combine(tPath, String.Concat("Set_", FileUtils.Common.GetDirectory(mShow.ShowPath), "_1.fanart.jpg"))
										imageList.Add(tPath)
										'doContinue = False
										'SET_<show>_1.fanart.jpg
									End If
							End Select
						End If
					Case Enums.ModuleEventType.OnMovieNFOSave
						mMovie = DirectCast(_params(0), Structures.DBMovie)
						doContinue = DirectCast(_refparam, Boolean)
						If AdvancedSettings.GetBooleanSetting("YAMJnfoFields", False) Then
							If Not String.IsNullOrEmpty(mMovie.FileSource) Then
								mMovie.Movie.VideoSource = mMovie.FileSource
							End If
							mMovie.Movie.IDMovieDB = "imdb"
						Else
							mMovie.Movie.IDMovieDB = String.Empty
							mMovie.Movie.VideoSource = String.Empty
						End If
				End Select
				_refparam = doContinue
			Catch ex As Exception
				Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
			End Try
		End If
	End Function

	Protected Overrides Sub Finalize()
		RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf SyncSettings
		MyBase.Finalize()
	End Sub

	Public Sub New()
		AddHandler ModulesManager.Instance.GenericEvent, AddressOf SyncSettings
	End Sub

	Sub SyncSettings(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
		If mType = Enums.ModuleEventType.SyncModuleSettings AndAlso Not IsNothing(Me.fYAMJ) Then
			RemoveHandler fYAMJ.GenericEvent, AddressOf DeploySyncSettings
			Me.fYAMJ.chkVideoTSParent.Checked = Master.eSettings.VideoTSParent
			AddHandler fYAMJ.GenericEvent, AddressOf DeploySyncSettings
		End If
	End Sub

	Sub DeploySyncSettings(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
		If Not IsNothing(Me.fYAMJ) Then
			Master.eSettings.VideoTSParent = Me.fYAMJ.chkVideoTSParent.Checked
			RaiseEvent GenericEvent(mType, _params)
		End If

	End Sub
#End Region	'Methods

End Class
