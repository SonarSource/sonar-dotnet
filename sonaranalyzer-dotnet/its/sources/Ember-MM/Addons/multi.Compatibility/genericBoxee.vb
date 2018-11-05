Imports System.IO
Imports EmberAPI

Public Class genericBoxee
    Implements Interfaces.EmberExternalModule

#Region "Fields"
	Private fBoxee As frmBoxee
	Private _enabled As Boolean = False
	Private _name As String = "Boxee compatibility"
	Private _AssemblyName As String = String.Empty
#End Region	'Fields

#Region "Events"
	Public Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object)) Implements EmberAPI.Interfaces.EmberExternalModule.GenericEvent

	Public Event ModuleSettingsChanged() Implements EmberAPI.Interfaces.EmberExternalModule.ModuleSettingsChanged

	Public Event ModuleSetupChanged(ByVal Name As String, ByVal State As Boolean, ByVal diffOrder As Integer) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleSetupChanged
#End Region	'Events

#Region "Properties"
	Public ReadOnly Property ModuleName() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleName
		Get
			Return "Boxee Compatibility"
		End Get
	End Property

	Public ReadOnly Property ModuleType() As System.Collections.Generic.List(Of EmberAPI.Enums.ModuleEventType) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleType
		Get
			Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic, Enums.ModuleEventType.OnTVShowNFORead, Enums.ModuleEventType.OnTVShowNFOSave})
		End Get
	End Property

	Public ReadOnly Property ModuleVersion() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleVersion
		Get
			Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
		End Get
	End Property

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
#End Region	'Properties

#Region "Methods"
	Public Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements EmberAPI.Interfaces.EmberExternalModule.Init
		_AssemblyName = sAssemblyName
		Master.eLang.LoadLanguage(Master.eSettings.Language, sExecutable)
	End Sub

	Public Function InjectSetup() As EmberAPI.Containers.SettingsPanel Implements EmberAPI.Interfaces.EmberExternalModule.InjectSetup
		Dim SPanel As New Containers.SettingsPanel
		Me.fBoxee = New frmBoxee
		Me.fBoxee.chkEnabled.Checked = Me._enabled
		Me.fBoxee.chkBoxeeId.Checked = AdvancedSettings.GetBooleanSetting("BoxeeTVShowId", False)
		'chkYAMJnfoFields
		SPanel.Name = _name
		SPanel.Text = Master.eLang.GetString(1, "Boxee Compatibility")
		SPanel.Prefix = "Boxee_"
		SPanel.Type = Master.eLang.GetString(802, "Modules", True)
		SPanel.ImageIndex = If(Me._enabled, 9, 10)
		SPanel.Order = 100
		SPanel.Panel = Me.fBoxee.pnlSettings
		AddHandler Me.fBoxee.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
		AddHandler fBoxee.ModuleEnabledChanged, AddressOf Handle_SetupChanged
		AddHandler fBoxee.GenericEvent, AddressOf DeploySyncSettings
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
		Me.Enabled = Me.fBoxee.chkEnabled.Checked()
		AdvancedSettings.SetBooleanSetting("BoxeeTVShowId", Me.fBoxee.chkBoxeeId.Checked)
	End Sub

	Public Function RunGeneric(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object), ByRef _refparam As Object) As EmberAPI.Interfaces.ModuleResult Implements EmberAPI.Interfaces.EmberExternalModule.RunGeneric
		Dim doContinue As Boolean
		Dim mTvShow As Structures.DBTV
		If Enabled Then
			Try
				Select Case mType
					Case Enums.ModuleEventType.OnTVShowNFOSave
						mTvShow = DirectCast(_params(0), Structures.DBTV)
						doContinue = DirectCast(_refparam, Boolean)
						If AdvancedSettings.GetBooleanSetting("BoxeeTVShowId", False) Then
							Dim mTVDetails As MediaContainers.TVShow
							mTVDetails = mTvShow.TVShow
							If mTVDetails.IDSpecified() Then
								mTVDetails.BoxeeTvDb = mTVDetails.ID
								mTVDetails.BlankId()
							End If
						End If

					Case Enums.ModuleEventType.OnTVShowNFORead
						Dim mTVDetails As New MediaContainers.TVShow
						mTVDetails = DirectCast(_params(0), MediaContainers.TVShow)
						doContinue = DirectCast(_refparam, Boolean)
						If AdvancedSettings.GetBooleanSetting("BoxeeTVShowId", False) Then
							If mTVDetails.BoxeeIDSpecified() Then
								mTVDetails.ID = mTVDetails.BoxeeTvDb
								mTVDetails.BlankBoxeeId()
							End If


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
		If mType = Enums.ModuleEventType.SyncModuleSettings AndAlso Not IsNothing(Me.fBoxee) Then
			RemoveHandler fBoxee.GenericEvent, AddressOf DeploySyncSettings
			Me.fBoxee.chkBoxeeId.Checked = AdvancedSettings.GetBooleanSetting("BoxeeTVShowId", False)
			AddHandler fBoxee.GenericEvent, AddressOf DeploySyncSettings
		End If
	End Sub
	Sub DeploySyncSettings(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
		If Not IsNothing(Me.fBoxee) Then
			AdvancedSettings.SetBooleanSetting("BoxeeTVShowId", Me.fBoxee.chkBoxeeId.Checked)
			RaiseEvent GenericEvent(mType, _params)
		End If

	End Sub
#End Region	'Methods
End Class
