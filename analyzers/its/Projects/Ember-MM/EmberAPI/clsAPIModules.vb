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
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Windows.Forms
Imports System.Drawing

Public Class ModulesManager

#Region "Fields"

    Public Shared AssemblyList As New List(Of AssemblyListItem)
    Public Shared VersionList As New List(Of VersionItem)

    Public externalProcessorModules As New List(Of _externalGenericModuleClass)
    Public externalScrapersModules As New List(Of _externalScraperModuleClass)
    Public externalTVScrapersModules As New List(Of _externalTVScraperModuleClass)
    Public RuntimeObjects As New EmberRuntimeObjects

    'Singleton Instace for module manager .. allways use this one
    Private Shared Singleton As ModulesManager = Nothing

    Private moduleLocation As String = Path.Combine(Functions.AppPath, "Modules")

#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Event MovieScraperEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object)

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Event TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)

#End Region 'Events

#Region "Properties"

    Public Shared ReadOnly Property Instance() As ModulesManager
        Get
            If (Singleton Is Nothing) Then
                Singleton = New ModulesManager()
            End If
            Return Singleton
        End Get
    End Property

    Public ReadOnly Property TVIsBusy() As Boolean
        Get
            Dim ret As Boolean = False
            For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
                ret = ret OrElse _externaltvScraperModule.ProcessorModule.IsBusy
            Next
            Return ret
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Public Function GetScraperByIdx(ByVal idx As Integer) As _externalScraperModuleClass
        Return externalScrapersModules(idx)
    End Function

    Public Function GetSingleEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As MediaContainers.EpisodeDetails
        Dim epDetails As New MediaContainers.EpisodeDetails

        If Not String.IsNullOrEmpty(TVDBID) AndAlso Not String.IsNullOrEmpty(Lang) Then
            Dim ret As Interfaces.ModuleResult
            For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
                ret = _externaltvScraperModule.ProcessorModule.GetSingleEpisode(ShowID, TVDBID, Season, Episode, Lang, Ordering, Options, epDetails)
                If ret.breakChain Then Exit For
            Next
        End If
        Return epDetails
    End Function

    Public Sub GetVersions()
        Dim dlgVersions As New dlgVersions
        Dim li As ListViewItem
        For Each v As VersionItem In VersionList
            li = dlgVersions.lstVersions.Items.Add(v.Name)
            li.SubItems.Add(v.Version)
        Next
        dlgVersions.ShowDialog()
    End Sub

    Public Sub Handler_MovieScraperEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object)
        RaiseEvent MovieScraperEvent(eType, Parameter)
    End Sub

    Public Sub Handler_TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)
        RaiseEvent TVScraperEvent(eType, iProgress, Parameter)
    End Sub

    Public Sub LoadAllModules()
        loadModules()
        loadScrapersModules()
        loadTVScrapersModules()
        BuildVersionList()
		Master.eLang.LoadAllLanguage(Master.eSettings.Language)
    End Sub

    ''' <summary>
    ''' Load all Generic Modules and field in externalProcessorModules List
    ''' </summary>
    Public Sub loadModules(Optional ByVal modulefile As String = "*.dll")
        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, modulefile)
                Try
                    'Load the assembly
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type
                    For Each fileType As Type In assembly.GetTypes
                        Try
                            'Activate the located module
                            Dim t As Type = fileType.GetInterface("EmberExternalModule")
                            If Not t Is Nothing Then
                                Dim ProcessorModule As Interfaces.EmberExternalModule 'Object
                                ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.EmberExternalModule)
                                'Add the activated module to the arraylist
                                Dim _externalProcessorModule As New _externalGenericModuleClass
                                Dim filename As String = file
                                If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                    AssemblyList.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                End If
                                _externalProcessorModule.ProcessorModule = ProcessorModule
                                _externalProcessorModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                _externalProcessorModule.AssemblyFileName = Path.GetFileName(file)
                                _externalProcessorModule.Type = ProcessorModule.ModuleType
                                externalProcessorModules.Add(_externalProcessorModule)
								ProcessorModule.Init(_externalProcessorModule.AssemblyName, Path.GetFileNameWithoutExtension(file))
                                Dim found As Boolean = False
                                For Each i In Master.eSettings.EmberModules
                                    If i.AssemblyName = _externalProcessorModule.AssemblyName Then
                                        _externalProcessorModule.ProcessorModule.Enabled = i.Enabled
                                        found = True
                                    End If
                                Next
                                If Not found AndAlso Path.GetFileNameWithoutExtension(file).Contains("generic.EmberCore") Then
                                    _externalProcessorModule.ProcessorModule.Enabled = True
                                    'SetModuleEnable(_externalProcessorModule.AssemblyName, True)
                                End If
                                AddHandler ProcessorModule.GenericEvent, AddressOf GenericRunCallBack
                                'ProcessorModule.Enabled = _externalProcessorModule.ProcessorModule.Enabled
                            End If
                        Catch ex As Exception
                        End Try
                    Next
                Catch ex As Exception
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalGenericModuleClass In externalProcessorModules.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next

        End If
    End Sub

    ''' <summary>
    ''' Load all Scraper Modules and field in externalScrapersModules List
    ''' </summary>
    Public Sub loadScrapersModules(Optional ByVal modulefile As String = "*.dll")
        Dim ScraperAnyEnabled As Boolean = False
        Dim PostScraperAnyEnabled As Boolean = False
        Dim ScraperFound As Boolean = False
        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, modulefile)
                Try
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type
                    For Each fileType As Type In assembly.GetTypes

                        'Activate the located module
                        Dim t As Type = fileType.GetInterface("EmberMovieScraperModule")
                        If Not t Is Nothing Then
                            Dim ProcessorModule As Interfaces.EmberMovieScraperModule
                            ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.EmberMovieScraperModule)
                            'Add the activated module to the arraylist
                            Dim _externalScraperModule As New _externalScraperModuleClass
                            Dim filename As String = file
                            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                AssemblyList.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                            End If
                            _externalScraperModule.ProcessorModule = ProcessorModule
                            _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                            _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                            externalScrapersModules.Add(_externalScraperModule)
                            _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                            For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName)
                                _externalScraperModule.ProcessorModule.ScraperEnabled = i.ScraperEnabled
                                ScraperAnyEnabled = ScraperAnyEnabled OrElse i.ScraperEnabled
                                PostScraperAnyEnabled = PostScraperAnyEnabled OrElse i.PostScraperEnabled
                                _externalScraperModule.ProcessorModule.PostScraperEnabled = i.PostScraperEnabled
                                PostScraperAnyEnabled = PostScraperAnyEnabled OrElse i.PostScraperEnabled
                                _externalScraperModule.ScraperOrder = i.ScraperOrder
                                _externalScraperModule.PostScraperOrder = i.PostScraperOrder
                                ScraperFound = True
                            Next
                            If Not ScraperFound Then
                                _externalScraperModule.ScraperOrder = 999
                                _externalScraperModule.PostScraperOrder = 999
                            End If
                        End If
                    Next
                Catch ex As Exception
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalScraperModuleClass In externalScrapersModules.OrderBy(Function(x) x.ScraperOrder) ' .Where(Function(x) x.ProcessorModule.ScraperEnabled)
                ext.ScraperOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass In externalScrapersModules.OrderBy(Function(x) x.PostScraperOrder) '.Where(Function(x) x.ProcessorModule.PostScraperEnabled)
                ext.PostScraperOrder = c
                c += 1
            Next
            If Not ScraperAnyEnabled AndAlso Not ScraperFound Then
                SetScraperEnable("scraper.EmberCore.EmberScraperModule.EmberNativeScraperModule", True)
                'SetScraperOrder("scraper.EmberCore.EmberScraperModule.EmberNativeScraperModule", 1)
            End If
            If Not PostScraperAnyEnabled AndAlso Not ScraperFound Then
                SetPostScraperEnable("scraper.EmberCore.EmberScraperModule.EmberNativeScraperModule", True)
                'SetPostScraperOrder("scraper.EmberCore.EmberScraperModule.EmberNativeScraperModule", 1)
            End If
        End If
    End Sub

    Public Sub loadTVScrapersModules()
        Dim ScraperAnyEnabled As Boolean = False
        Dim PostScraperAnyEnabled As Boolean = False
        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, "*.dll")
                Try
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type

                    For Each fileType As Type In assembly.GetTypes

                        'Activate the located module
                        Dim t As Type = fileType.GetInterface("EmberTVScraperModule")
                        If Not t Is Nothing Then
                            Dim ProcessorModule As Interfaces.EmberTVScraperModule
                            ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.EmberTVScraperModule)
                            'Add the activated module to the arraylist
                            Dim _externaltvScraperModule As New _externalTVScraperModuleClass
                            Dim filename As String = file
                            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                AssemblyList.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                            End If

                            _externaltvScraperModule.ProcessorModule = ProcessorModule
                            _externaltvScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                            _externaltvScraperModule.AssemblyFileName = Path.GetFileName(file)
                            Dim found As Boolean = False
                            externalTVScrapersModules.Add(_externaltvScraperModule)
                            _externaltvScraperModule.ProcessorModule.Init(_externaltvScraperModule.AssemblyName)
                            For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externaltvScraperModule.AssemblyName)
                                _externaltvScraperModule.ProcessorModule.ScraperEnabled = i.ScraperEnabled
                                ScraperAnyEnabled = ScraperAnyEnabled OrElse i.ScraperEnabled
                                _externaltvScraperModule.ProcessorModule.PostScraperEnabled = i.PostScraperEnabled
                                PostScraperAnyEnabled = PostScraperAnyEnabled OrElse i.PostScraperEnabled
                                _externaltvScraperModule.ScraperOrder = i.ScraperOrder
                                _externaltvScraperModule.PostScraperOrder = i.PostScraperOrder
                                found = True
                            Next
                            If Not found Then
                                _externaltvScraperModule.ScraperOrder = 999
                                _externaltvScraperModule.PostScraperOrder = 999
                            End If
                            AddHandler _externaltvScraperModule.ProcessorModule.TVScraperEvent, AddressOf Handler_TVScraperEvent
                        End If
                    Next
                Catch ex As Exception
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(x) x.ProcessorModule.ScraperEnabled)
                ext.ScraperOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(x) x.ProcessorModule.PostScraperEnabled)
                ext.PostScraperOrder = c
                c += 1
            Next
            If Not ScraperAnyEnabled Then
                SetTVScraperEnable("scraper.EmberCore.EmberScraperModule.EmberNativeTVScraperModule", True)
                'SetTVScraperOrder("scraper.EmberCore.EmberScraperModule.EmberNativeTVScraperModule", 1)

            End If
            If Not PostScraperAnyEnabled Then
                SetTVPostScraperEnable("scraper.EmberCore.EmberScraperModule.EmberNativeTVScraperModule", True)
                'SetTVPostScraperOrder("scraper.EmberCore.EmberScraperModule.EmberNativeTVScraperModule", 1)
            End If
        End If
    End Sub

    Public Function MoviePostScrapeOnly(ByRef DBMovie As Structures.DBMovie, ByVal ScrapeType As Enums.ScrapeType) As Interfaces.ModuleResult
        Dim ret As Interfaces.ModuleResult
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            AddHandler _externalScraperModule.ProcessorModule.MovieScraperEvent, AddressOf Handler_MovieScraperEvent
            Try
                ret = _externalScraperModule.ProcessorModule.PostScraper(DBMovie, ScrapeType)
            Catch ex As Exception
            End Try
            RemoveHandler _externalScraperModule.ProcessorModule.MovieScraperEvent, AddressOf Handler_MovieScraperEvent
            If ret.breakChain Then Exit For
        Next
        Return ret
    End Function

    Public Function MovieScrapeOnly(ByRef DBMovie As Structures.DBMovie, ByVal ScrapeType As Enums.ScrapeType, ByVal Options As Structures.ScrapeOptions) As Boolean
        Dim ret As Interfaces.ModuleResult
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            AddHandler _externalScraperModule.ProcessorModule.MovieScraperEvent, AddressOf Handler_MovieScraperEvent
            Try
                ret = _externalScraperModule.ProcessorModule.Scraper(DBMovie, ScrapeType, Options)
            Catch ex As Exception
            End Try
            RemoveHandler _externalScraperModule.ProcessorModule.MovieScraperEvent, AddressOf Handler_MovieScraperEvent
            If ret.breakChain Then Exit For
        Next
        Return ret.Cancelled
    End Function

    Public Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), Optional ByVal _refparam As Object = Nothing, Optional ByVal RunOnlyOne As Boolean = False) As Boolean
        Dim ret As Interfaces.ModuleResult
        Try
            For Each _externalGenericModule As _externalGenericModuleClass In externalProcessorModules.Where(Function(e) e.ProcessorModule.ModuleType.Contains(mType) AndAlso e.ProcessorModule.Enabled)
                Try
                    ret = _externalGenericModule.ProcessorModule.RunGeneric(mType, _params, _refparam)
                Catch ex As Exception
                End Try
                If ret.breakChain OrElse RunOnlyOne Then Exit For
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return ret.Cancelled
    End Function
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Sub SaveSettings()
        Dim tmpForXML As New List(Of _XMLEmberModuleClass)

        For Each _externalProcessorModule As _externalGenericModuleClass In externalProcessorModules
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalProcessorModule.AssemblyName
            t.AssemblyFileName = _externalProcessorModule.AssemblyFileName
            t.Enabled = _externalProcessorModule.ProcessorModule.Enabled
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.PostScraperEnabled = _externalScraperModule.ProcessorModule.PostScraperEnabled
            t.ScraperEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.PostScraperOrder = _externalScraperModule.PostScraperOrder
            t.ScraperOrder = _externalScraperModule.ScraperOrder
            tmpForXML.Add(t)
        Next
        For Each _externalTVScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalTVScraperModule.AssemblyName
            t.AssemblyFileName = _externalTVScraperModule.AssemblyFileName
            t.PostScraperEnabled = _externalTVScraperModule.ProcessorModule.PostScraperEnabled
            t.ScraperEnabled = _externalTVScraperModule.ProcessorModule.ScraperEnabled
            t.PostScraperOrder = _externalTVScraperModule.PostScraperOrder
            t.ScraperOrder = _externalTVScraperModule.ScraperOrder
            tmpForXML.Add(t)
        Next
        Master.eSettings.EmberModules = tmpForXML
        Master.eSettings.Save()
    End Sub

    Public Function ScrapersCount() As Integer
        Return externalScrapersModules.Count
    End Function

    Public Sub SetModuleEnable(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalProcessorModule As _externalGenericModuleClass In externalProcessorModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalProcessorModule.ProcessorModule.Enabled = value
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Sub SetPostScraperEnable(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalScraperModule.ProcessorModule.PostScraperEnabled = value
            Catch ex As Exception
            End Try
        Next
    End Sub
    Public Sub SetScraperEnable(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalScraperModule.ProcessorModule.ScraperEnabled = value
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Sub SetTVPostScraperEnable(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalScraperModule.ProcessorModule.PostScraperEnabled = value
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Sub SetTVScraperEnable(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalScraperModule.ProcessorModule.ScraperEnabled = value
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Sub TVCancelAsync()
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            Try
                _externaltvScraperModule.ProcessorModule.CancelAsync()
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Function TVGetLangs(ByVal sMirror As String) As List(Of Containers.TVLanguage)
        Dim ret As Interfaces.ModuleResult
        Dim Langs As New List(Of Containers.TVLanguage)
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            Try
                ret = _externaltvScraperModule.ProcessorModule.GetLangs(sMirror, Langs)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return Langs
    End Function

    Public Function TVScrapeEpisode(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iEpisode As Integer, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As Boolean
        Dim ret As Interfaces.ModuleResult
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            Try
                ret = _externaltvScraperModule.ProcessorModule.ScrapeEpisode(ShowID, ShowTitle, TVDBID, iEpisode, iSeason, Lang, Ordering, Options)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return ret.Cancelled
    End Function

    Public Function TVScrapeOnly(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions, ByVal ScrapeType As Enums.ScrapeType, ByVal WithCurrent As Boolean) As Boolean
        Dim ret As Interfaces.ModuleResult
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            Try
                ret = _externaltvScraperModule.ProcessorModule.Scraper(ShowID, ShowTitle, TVDBID, Lang, Ordering, Options, ScrapeType, WithCurrent)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return ret.Cancelled
    End Function

    Public Function TVScrapeSeason(ByVal ShowID As Integer, ByVal ShowTitle As String, ByVal TVDBID As String, ByVal iSeason As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal Options As Structures.TVScrapeOptions) As Boolean
        Dim ret As Interfaces.ModuleResult
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            Try
                ret = _externaltvScraperModule.ProcessorModule.ScrapeSeason(ShowID, ShowTitle, TVDBID, iSeason, Lang, Ordering, Options)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return ret.Cancelled
    End Function

    Public Function TVSingleImageOnly(ByVal Title As String, ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Type As Enums.TVImageType, ByVal Season As Integer, ByVal Episode As Integer, ByVal Lang As String, ByVal Ordering As Enums.Ordering, ByVal CurrentImage As Image) As Image
        Dim Image As Image = Nothing
        Dim ret As Interfaces.ModuleResult
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled)
            Try
                ret = _externaltvScraperModule.ProcessorModule.GetSingleImage(Title, ShowID, TVDBID, Type, Season, Episode, Lang, Ordering, CurrentImage, Image)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return Image
    End Function

    Private Sub BuildVersionList()
        VersionList.Clear()
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPP", .Name = "Ember Application", .Version = My.Application.Info.Version.ToString()})
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPI", .Name = "Ember API", .Version = Functions.EmberAPIVersion()})
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
                    .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
                    .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalModule As _externalGenericModuleClass In externalProcessorModules
            VersionList.Add(New VersionItem With {.Name = _externalModule.ProcessorModule.ModuleName, _
                    .AssemblyFileName = _externalModule.AssemblyFileName, _
                    .Version = _externalModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalTVScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules
            VersionList.Add(New VersionItem With {.Name = _externalTVScraperModule.ProcessorModule.ModuleName, _
                    .AssemblyFileName = _externalTVScraperModule.AssemblyFileName, _
                    .Version = _externalTVScraperModule.ProcessorModule.ModuleVersion})
        Next
    End Sub

    Function ChangeEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Lang As String) As MediaContainers.EpisodeDetails
        Dim ret As Interfaces.ModuleResult
        Dim epDetails As New MediaContainers.EpisodeDetails
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            Try
                ret = _externaltvScraperModule.ProcessorModule.ChangeEpisode(ShowID, TVDBID, Lang, epDetails)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return epDetails
    End Function

    Private Sub GenericRunCallBack(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        RaiseEvent GenericEvent(mType, _params)
    End Sub

    Function QueryPostScraperCapabilities(ByVal cap As Enums.PostScraperCapabilities) As Boolean
        Dim ret As Boolean = False
        Dim sStudio As New List(Of String)
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ScraperOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.QueryPostScraperCapabilities(cap)
            Catch ex As Exception
            End Try
            If ret Then Exit For
        Next
        Return ret
    End Function

    Function GetMovieStudio(ByRef DBMovie As Structures.DBMovie) As List(Of String)
        Dim ret As Interfaces.ModuleResult
        Dim sStudio As New List(Of String)
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsScraper AndAlso e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ScraperOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.GetMovieStudio(DBMovie, sStudio)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return sStudio
    End Function

    Function ScraperDownloadTrailer(ByRef DBMovie As Structures.DBMovie) As String
        Dim ret As Interfaces.ModuleResult
        Dim sURL As String = String.Empty
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.DownloadTrailer(DBMovie, sURL)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return sURL
    End Function

    Function ScraperSelectImageOfType(ByRef DBMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, ByRef pResults As Containers.ImgResult, Optional ByVal _isEdit As Boolean = False, Optional ByVal preload As Boolean = False) As Boolean
        Dim ret As Interfaces.ModuleResult
        For Each _externalScraperModule As _externalScraperModuleClass In externalScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.SelectImageOfType(DBMovie, _DLType, pResults, _isEdit, preload)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        Return ret.Cancelled
    End Function

    Sub TVSaveImages()
        Dim ret As Interfaces.ModuleResult
        For Each _externaltvScraperModule As _externalTVScraperModuleClass In externalTVScrapersModules.Where(Function(e) e.ProcessorModule.IsPostScraper AndAlso e.ProcessorModule.PostScraperEnabled).OrderBy(Function(e) e.PostScraperOrder)
            Try
                ret = _externaltvScraperModule.ProcessorModule.SaveImages()
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
    End Sub
#End Region 'Methods

#Region "Nested Types"

    Structure AssemblyListItem

#Region "Fields"

        Public Assembly As System.Reflection.Assembly
        Public AssemblyName As String

#End Region 'Fields

    End Structure

    Structure VersionItem

#Region "Fields"

        Public AssemblyFileName As String
        Public Name As String
        Public NeedUpdate As Boolean
        Public Version As String

#End Region 'Fields

    End Structure

    Class EmberRuntimeObjects

#Region "Fields"

        Private _LoadMedia As LoadMedia
        Private _MainTool As System.Windows.Forms.ToolStrip
        Private _MediaList As System.Windows.Forms.DataGridView
        Private _MenuMediaList As System.Windows.Forms.ContextMenuStrip
        Private _MenuTVShowList As System.Windows.Forms.ContextMenuStrip
        Private _OpenImageViewer As OpenImageViewer
        Private _TopMenu As System.Windows.Forms.MenuStrip
        Private _TrayMenu As System.Windows.Forms.ContextMenuStrip
        Private _MediaTabSelected As Integer = 0

#End Region 'Fields

#Region "Delegates"

        Delegate Sub LoadMedia(ByVal Scan As Structures.Scans, ByVal SourceName As String)

        'all runtime object including Function (delegate) that need to be exposed to Modules
        Delegate Sub OpenImageViewer(ByVal _Image As Image)

#End Region 'Delegates

#Region "Properties"
        Public Property MediaTabSelected() As Integer
            Get
                Return _MediaTabSelected
            End Get
            Set(ByVal value As Integer)
                _MediaTabSelected = value
            End Set
        End Property
        Public Property MainTool() As System.Windows.Forms.ToolStrip
            Get
                Return _MainTool
            End Get
            Set(ByVal value As System.Windows.Forms.ToolStrip)
                _MainTool = value
            End Set
        End Property

        Public Property MediaList() As System.Windows.Forms.DataGridView
            Get
                Return _MediaList
            End Get
            Set(ByVal value As System.Windows.Forms.DataGridView)
                _MediaList = value
            End Set
        End Property

        Public Property MenuMediaList() As System.Windows.Forms.ContextMenuStrip
            Get
                Return _MenuMediaList
            End Get
            Set(ByVal value As System.Windows.Forms.ContextMenuStrip)
                _MenuMediaList = value
            End Set
        End Property

        Public Property MenuTVShowList() As System.Windows.Forms.ContextMenuStrip
            Get
                Return _MenuTVShowList
            End Get
            Set(ByVal value As System.Windows.Forms.ContextMenuStrip)
                _MenuTVShowList = value
            End Set
        End Property

        Public Property TopMenu() As System.Windows.Forms.MenuStrip
            Get
                Return _TopMenu
            End Get
            Set(ByVal value As System.Windows.Forms.MenuStrip)
                _TopMenu = value
            End Set
        End Property

        Public Property TrayMenu() As System.Windows.Forms.ContextMenuStrip
            Get
                Return _TrayMenu
            End Get
            Set(ByVal value As System.Windows.Forms.ContextMenuStrip)
                _TrayMenu = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub DelegateLoadMedia(ByRef lm As LoadMedia)
            'Setup from EmberAPP
            _LoadMedia = lm
        End Sub

        Public Sub DelegateOpenImageViewer(ByRef IV As OpenImageViewer)
            _OpenImageViewer = IV
        End Sub

        Public Sub InvokeLoadMedia(ByVal Scan As Structures.Scans, ByVal SourceName As String)
            'Invoked from Modules
            _LoadMedia.Invoke(Scan, SourceName)
        End Sub

        Public Sub InvokeOpenImageViewer(ByRef _image As Image)
            _OpenImageViewer.Invoke(_image)
        End Sub

#End Region 'Methods

    End Class

    Class _externalGenericModuleClass

#Region "Fields"

        Public AssemblyFileName As String

        'Public Enabled As Boolean
        Public AssemblyName As String
        Public ModuleOrder As Integer 'TODO: not important at this point.. for 1.5
        Public ProcessorModule As Interfaces.EmberExternalModule 'Object
        Public Type As List(Of Enums.ModuleEventType)

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public PostScraperOrder As Integer
        Public ProcessorModule As Interfaces.EmberMovieScraperModule 'Object
        Public ScraperOrder As Integer

#End Region 'Fields

    End Class

    Class _externalTVScraperModuleClass

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public PostScraperOrder As Integer
        Public ProcessorModule As Interfaces.EmberTVScraperModule 'Object
        Public ScraperOrder As Integer

#End Region 'Fields

    End Class

    <XmlRoot("EmberModule")> _
    Class _XMLEmberModuleClass

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public Enabled As Boolean
        Public PostScraperEnabled As Boolean
        Public PostScraperOrder As Integer
        Public ScraperEnabled As Boolean
        Public ScraperOrder As Integer

#End Region 'Fields

    End Class

#End Region 'Nested Types

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class