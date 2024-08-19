Imports EmberAPI

Public Class genericGenresEditor
    Implements Interfaces.EmberExternalModule

#Region "Fields"

	Private fGenres As frmGenresEditor
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
			Return True
		End Get
		Set(ByVal value As Boolean)
		End Set
	End Property

	Public ReadOnly Property ModuleName() As String Implements EmberAPI.Interfaces.EmberExternalModule.ModuleName
		Get
			Return "Genres Editor"
		End Get
	End Property

	Public ReadOnly Property ModuleType() As System.Collections.Generic.List(Of EmberAPI.Enums.ModuleEventType) Implements EmberAPI.Interfaces.EmberExternalModule.ModuleType
		Get
			Return New List(Of Enums.ModuleEventType)(New Enums.ModuleEventType() {Enums.ModuleEventType.Generic})
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
		Me.fGenres = New frmGenresEditor
		SPanel.Name = Master.eLang.GetString(1, "Genres Editor")
		SPanel.Text = Master.eLang.GetString(1, "Genres Editor")
		SPanel.Prefix = "GenresEditor_"
		SPanel.Type = Master.eLang.GetString(822, "Miscellaneous")
		SPanel.ImageIndex = -1
		SPanel.Image = My.Resources.GenresEditor
		SPanel.Order = 100
		SPanel.Panel = Me.fGenres.pnlGenres
		AddHandler Me.fGenres.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
		Return SPanel
	End Function

	Private Sub Handle_ModuleSettingsChanged()
		RaiseEvent ModuleSettingsChanged()
	End Sub

	Public Function RunGeneric(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object), ByRef _refparam As Object) As EmberAPI.Interfaces.ModuleResult Implements EmberAPI.Interfaces.EmberExternalModule.RunGeneric

	End Function

	Public Sub SaveSetup(ByVal DoDispose As Boolean) Implements EmberAPI.Interfaces.EmberExternalModule.SaveSetup
		If Not fGenres Is Nothing Then fGenres.SaveChanges()
	End Sub

#End Region	'Methods

#Region "Nested Types"

#End Region	'Nested Types

End Class
