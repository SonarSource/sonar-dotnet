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

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports System.Drawing.Drawing2D
Imports System.Threading
Imports EmberAPI

Public Class dlgNMTMovies

#Region "Fields"

    Friend WithEvents bwBuildHTML As New System.ComponentModel.BackgroundWorker

    Private template_Path As String
    Private bCancelled As Boolean = False
    Private bexportFlags As Boolean = False
    Private bexportPosters As Boolean = False
    Private bexportBackDrops As Boolean = False
    Private bFiltered As Boolean = False
    Private DontSaveExtra As Boolean = False
    Private FilterMovies As New List(Of Long)
    Private FilterTVShows As New List(Of Long)
    Private HTMLMovieBody As New StringBuilder
    Private HTMLTVBody As New StringBuilder
    Private isCL As Boolean = False
    Private use_filter As Boolean = False
    Private workerCanceled As Boolean = False
    Private sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
    Private conf As NMTExporterModule.Config
    Private confs As New List(Of NMTExporterModule.Config)
    Private selectedSources As New Hashtable
    Private outputFolder As String
    Private MoviesCountries As New List(Of String)
    Private MoviesGenres As New List(Of String)
    Private TVShowsGenres As New List(Of String)
    Private OutputExist As Boolean = False
    Private outputChanged As Boolean = False
    Private HaveTV As Boolean
    Private HaveMovies As Boolean
    Private bHighPriority As Boolean = False

    Public Loaded As Boolean = False
    Public CanBuild As Boolean = False
    Public Shared dtMovieMedia As DataTable = Nothing
    Public Shared dtEpisodesPaths As DataTable = Nothing
    Public Shared dtEpisodes As DataTable = Nothing
    Public Shared dtSeasons As DataTable = Nothing
    Public Shared dtShows As DataTable = Nothing

#End Region 'Fields


#Region "Methods"
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Try
            Me.SetUp()
            'If dtMovieMedia Is Nothing Then
            dtMovieMedia = New DataTable
            Master.DB.FillDataTable(dtMovieMedia, "SELECT * FROM movies ORDER BY ListTitle COLLATE NOCASE;")
            'End If
            'If dtShows Is Nothing Then
            dtShows = New DataTable
            Master.DB.FillDataTable(dtShows, "SELECT * FROM TVShows ORDER BY Title COLLATE NOCASE;")
            'End If
            'If dtSeasons Is Nothing Then
            dtSeasons = New DataTable
            Master.DB.FillDataTable(dtSeasons, "SELECT * FROM TVSeason ORDER BY Season COLLATE NOCASE;")
            'End If
            'If dtEpisodes Is Nothing Then
            dtEpisodes = New DataTable
            Master.DB.FillDataTable(dtEpisodes, "SELECT * FROM TVEps ORDER BY Episode COLLATE NOCASE;")
            'End If
            dtEpisodesPaths = New DataTable
            Master.DB.FillDataTable(dtEpisodesPaths, "SELECT ID,TVEpPath FROM TVEpPaths;")

            txtOutputFolder.Text = AdvancedSettings.GetSetting("BasePath", "")
            Dim fxml As String
            Dim di As DirectoryInfo = New DirectoryInfo(Path.Combine(sBasePath, "Templates"))
            For Each i As DirectoryInfo In di.GetDirectories
                If Not (i.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then
                    fxml = Path.Combine(sBasePath, String.Concat("Templates", Path.DirectorySeparatorChar, i.Name))
                    conf = NMTExporterModule.Config.Load(Path.Combine(fxml, "config.xml"))
                    If Not String.IsNullOrEmpty(conf.Name) AndAlso Convert.ToSingle(conf.DesignVersion.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)) >= NMTExporterModule.MinDesignVersion Then
                        conf.TemplatePath = fxml
                        confs.Add(conf)
                        cbTemplate.Items.Add(conf.Name)
                    Else
                        conf = Nothing
                    End If
                End If
            Next
            If cbTemplate.Items.Count > 0 Then
                Dim idx As Integer = cbTemplate.FindStringExact(AdvancedSettings.GetSetting("Template", "***"))
                If idx >= 0 Then
                    cbTemplate.SelectedIndex = idx
                Else
                    cbTemplate.SelectedIndex = 0
                End If
            End If
            dgvSources.ShowCellToolTips = True
            For Each s As Structures.MovieSource In Master.MovieSources
                Dim i As Integer = dgvSources.Rows.Add(New Object() {False, s.Name, My.Resources.film, String.Empty, "movie"})
                dgvSources.Rows(i).Cells(1).ToolTipText = s.Path
                'dgvSources.Rows(i).Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.Movie.", conf.Name, ".", s.Name), "")
                'dgvSources.Rows(i).Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.Movie.Status.", conf.Name, ".", s.Name), False)
            Next
            For Each s As Structures.TVSource In Master.TVSources
                Dim i As Integer = dgvSources.Rows.Add(New Object() {False, s.Name, My.Resources.television, String.Empty, "tv"})
                dgvSources.Rows(i).Cells(1).ToolTipText = s.Path
                'dgvSources.Rows(i).Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.TV.", conf.Name, ".", s.Name), "")
                'dgvSources.Rows(i).Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.TV.Status.", conf.Name, ".", s.Name), False)
            Next
            If Not conf Is Nothing Then
                PopulateParams()
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End If
            Loaded = True
            btnSave.Enabled = False
            pbWarning.Image = Nothing 'ilNMT.Images("green")
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub SaveConfig()
        Try
            If IsNothing(conf) Then
                MsgBox(Master.eLang.GetString(42, "Please Choose/Install a Template First"), MsgBoxStyle.OkCancel, "Error")
                Return
            End If
            AdvancedSettings.SetSetting("Template", cbTemplate.Text)
            AdvancedSettings.SetSetting("BasePath", txtOutputFolder.Text.ToString)
            AdvancedSettings.SetBooleanSetting(String.Concat("HighPriority.", conf.Name), chHighPriority.Checked)

            For Each r As NMTExporterModule.Config._Param In conf.Params.Where(Function(y) Not y.access = "internal")
                AdvancedSettings.SetSetting(String.Concat("Param.", conf.Name, ".", r.name), r.value)
            Next
            For Each r As NMTExporterModule.Config._Property In conf.Properties.Where(Function(y) y.type = "list")
                Dim v As String = r.value
                AdvancedSettings.SetSetting(String.Concat("Property.", conf.Name, ".", r.name), r.values.FirstOrDefault(Function(y) y.value = v).label)
            Next
            For Each s As DataGridViewRow In dgvSources.Rows
                If IsNothing(s.Cells(3).Value) Then s.Cells(3).Value = String.Empty
                If IsNothing(s.Cells(1).Value) Then Continue For
                If IsNothing(s.Cells(0).Value) Then s.Cells(0).Value = False

                If s.Cells(4).Value.ToString = "movie" Then
                    AdvancedSettings.SetSetting(String.Concat("Path.Movie.", conf.Name, ".", s.Cells(1).Value.ToString), s.Cells(3).Value.ToString)
                    AdvancedSettings.SetBooleanSetting(String.Concat("Path.Movie.Status.", conf.Name, ".", s.Cells(1).Value.ToString), Convert.ToBoolean(s.Cells(0).Value))
                Else
                    AdvancedSettings.SetSetting(String.Concat("Path.TV.", conf.Name, ".", s.Cells(1).Value.ToString), s.Cells(3).Value.ToString)
                    AdvancedSettings.SetBooleanSetting(String.Concat("Path.TV.Status.", conf.Name, ".", s.Cells(1).Value.ToString), Convert.ToBoolean(s.Cells(0).Value))
                End If
            Next
            'If Not conf Is Nothing Then conf.Save(Path.Combine(conf.TemplatePath, "config.xml"))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub ExportSingle()
        Try
            Dim MySelf As New dlgNMTMovies
            MySelf.isCL = True
            While Not MySelf.Loaded
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
            Application.DoEvents()
            If MySelf.CanBuild Then
                MySelf.DoBuild()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub CopyDirectory(ByVal SourcePath As String, ByVal DestPath As String, Optional ByVal Overwrite As Boolean = False)
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(SourcePath)
        Dim DestDir As DirectoryInfo = New DirectoryInfo(DestPath)
        Dim IsRoot As Boolean = False

        ' the source directory must exist, otherwise throw an exception
        If SourceDir.Exists Then

            'is this a root directory?
            If DestDir.Root.FullName = DestDir.FullName Then
                IsRoot = True
            End If

            ' if destination SubDir's parent SubDir does not exist throw an exception (also check it isn't the root)
            If Not IsRoot AndAlso Not DestDir.Parent.Exists Then
                Throw New DirectoryNotFoundException _
                    ("Destination directory does not exist: " + DestDir.Parent.FullName)
            End If

            If Not DestDir.Exists Then
                DestDir.Create()
            End If

            ' copy all the files of the current directory
            Dim ChildFile As FileInfo
            For Each ChildFile In SourceDir.GetFiles()
                If (ChildFile.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden OrElse Path.GetExtension(ChildFile.FullName) = ".htm" Then Continue For
                If Overwrite Then
                    ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), True)
                Else
                    ' if Overwrite = false, copy the file only if it does not exist
                    ' this is done to avoid an IOException if a file already exists
                    ' this way the other files can be copied anyway...
                    If Not File.Exists(Path.Combine(DestDir.FullName, ChildFile.Name)) Then
                        ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), False)
                    End If
                End If
            Next

            ' copy all the sub-directories by recursively calling this same routine
            Dim SubDir As DirectoryInfo
            For Each SubDir In SourceDir.GetDirectories()
                If (SubDir.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then Continue For
                CopyDirectory(SubDir.FullName, Path.Combine(DestDir.FullName, _
                    SubDir.Name), Overwrite)
            Next
        Else
            Throw New DirectoryNotFoundException("Source directory does not exist: " + SourceDir.FullName)
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        If bwBuildHTML.IsBusy Then
            bwBuildHTML.CancelAsync()
        End If
        btnCancel.Enabled = False
    End Sub
    Private Sub PreProcessProperties(ByRef str As String)
        Dim propreties As List(Of String)
        propreties = GetPropertiesPre(str)
        For Each s As NMTExporterModule.Config._Property In conf.Properties
            If propreties.Contains(String.Concat("{$", s.name, "}")) Then
                str = str.Replace(String.Concat("{$", s.name, "}"), s.value)
            End If
        Next
        For Each s As String In propreties
            str = str.Replace(s, String.Empty)
        Next
        str = str.Replace("<$EVERSION>", My.Application.Info.Version.Revision.ToString)
        str = str.Replace("<$MVERSION>", FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString)
        str = str.Replace("<$SVERSION>", conf.Version)
    End Sub
    Public Function GetPropertiesPre(ByVal s As String) As List(Of String)
        Dim rets As New List(Of String)
        Try
            Dim regStat As MatchCollection = Regex.Matches(s, "\{\$(?<values>.*?)\}", RegexOptions.Multiline)
            If regStat.Count > 0 Then
                For Each status As Match In regStat
                    rets.Add(status.Value)
                Next
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return rets
    End Function

    Private Sub BuildMovieHTML(ByVal template As String, ByVal outputbase As String)
        Try
            bwBuildHTML.ReportProgress(0, Master.eLang.GetString(2, "Compiling Movie List..."))
            Dim destPathShort As String = Path.Combine(outputbase, GetUserParam("MoviesDetailsPath", "html/").Replace("/", Path.DirectorySeparatorChar))
            HTMLMovieBody.Length = 0
            Dim sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
            Dim htmlPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").Name)
            Dim htmlDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movie").Name)
            Dim pattern As String = String.Empty
            Dim patternDetails As String = String.Empty
            Dim movieheader As String = String.Empty
            Dim moviefooter As String = String.Empty
            Dim movierow As String = String.Empty

            pattern = File.ReadAllText(htmlPath)
            PreProcessProperties(pattern)
            patternDetails = File.ReadAllText(htmlDetailsPath)
            PreProcessProperties(patternDetails)
            Dim s = pattern.IndexOf("<$MOVIE>")
            If s >= 0 Then
                Dim e = pattern.IndexOf("<$/MOVIE>")
                If e >= 0 Then
                    movieheader = pattern.Substring(0, s)
                    movierow = pattern.Substring(s + 8, e - s - 8)
                    moviefooter = pattern.Substring(e + 9, pattern.Length - e - 9)
                Else
                    'error
                End If
            Else
                'error
            End If

            HTMLMovieBody.Append(movieheader)
            Dim counter As Integer = 1
            FilterMovies.Clear()
            MoviesGenres.Clear()
            MoviesCountries.Clear()

            For Each _curMovie As DataRow In dtMovieMedia.Rows
                If bwBuildHTML.CancellationPending Then Return
                'now check if we need to include this movie
                If Not selectedSources.Contains(_curMovie.Item("Source").ToString) Then
                    bwBuildHTML.ReportProgress(1)
                    Continue For
                End If

                FilterMovies.Add(Convert.ToInt32(_curMovie.Item("ID")))
                Dim hfile As String = Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").DestPath.Replace("/", Path.DirectorySeparatorChar))
                HTMLMovieBody.Append(ProcessMovieTags(_curMovie, hfile, counter, _curMovie.Item("ID").ToString, movierow))
                Dim detailsoutput As String = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movie").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat(_curMovie.Item("ID").ToString, ".htm"))
                File.WriteAllText(detailsoutput, ProcessMovieTags(_curMovie, String.Concat(Path.GetDirectoryName(detailsoutput), Path.DirectorySeparatorChar), counter, _curMovie.Item("ID").ToString, patternDetails))
                counter += 1
                bwBuildHTML.ReportProgress(1)
            Next
            HTMLMovieBody.Append(moviefooter)
            HTMLMovieBody.Replace("<$GENRES_LIST>", StringUtils.HtmlEncode(Strings.Join(MoviesGenres.ToArray, ",")))
            HTMLMovieBody.Replace("<$COUNTRIES_LIST>", StringUtils.HtmlEncode(Strings.Join(MoviesCountries.ToArray, ",")))
            DontSaveExtra = False
            Me.SaveMovieFiles(Path.GetDirectoryName(htmlPath), outputbase)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub GetHtmlParts(ByVal pattern As String, ByRef header As String, ByRef row As String, ByRef footer As String, ByVal tag As String)
        Dim s = pattern.IndexOf(String.Concat("<$", tag, ">"))
        If s >= 0 Then
            Dim e = pattern.IndexOf(String.Concat("<$/", tag, ">"))
            If e >= 0 Then
                header = pattern.Substring(0, s)
                row = pattern.Substring(s + tag.Length + 3, e - s - tag.Length - 3)
                footer = pattern.Substring(e + tag.Length + 4, pattern.Length - e - tag.Length - 4)
            Else 'error
            End If
        Else 'error
            header = pattern
            row = String.Empty
            footer = String.Empty
        End If
    End Sub

    Private Sub BuildTVHTML(ByVal template As String, ByVal outputbase As String)
        Try
            bwBuildHTML.ReportProgress(0, Master.eLang.GetString(42, "Compiling TV Shows List..."))
            Dim destPathShort As String = Path.Combine(outputbase, GetUserParam("TVDetailsPath", "html/").Replace("/", Path.DirectorySeparatorChar))
            HTMLTVBody.Length = 0
            Dim sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
            Dim htmlPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvindex").Name)
            Dim htmlShowDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvshow").Name)
            Dim htmlSeasonDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvseason").Name)
            Dim htmlEpDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvepisode").Name)
            Dim patternIndexDetails As String = String.Empty
            Dim patternShowDetails As String = String.Empty
            Dim patternSeasonDetails As String = String.Empty
            Dim patternEpDetails As String = String.Empty
            Dim tvheader As String = String.Empty
            Dim tvfooter As String = String.Empty
            Dim tvrow As String = String.Empty

            patternIndexDetails = File.ReadAllText(htmlPath)
            If File.Exists(htmlShowDetailsPath) Then patternShowDetails = File.ReadAllText(htmlShowDetailsPath)
            If File.Exists(htmlSeasonDetailsPath) Then patternSeasonDetails = File.ReadAllText(htmlSeasonDetailsPath)
            If File.Exists(htmlEpDetailsPath) Then patternEpDetails = File.ReadAllText(htmlEpDetailsPath)
            GetHtmlParts(patternIndexDetails, tvheader, tvrow, tvfooter, "TVSHOW")
            HTMLTVBody.Append(tvheader)
            Dim ShowCounter As Integer = 1

            FilterTVShows.Clear()
            TVShowsGenres.Clear()

            For Each _curShow As DataRow In dtShows.Rows
                Dim detailsShowOutput As String = String.Empty
                Dim detailsSeasonOutput As String = String.Empty
                Dim detailsEpsOutput As String = String.Empty
                If bwBuildHTML.CancellationPending Then Return
                If Not selectedSources.Contains(_curShow.Item("Source").ToString) OrElse String.IsNullOrEmpty(_curShow.Item("NfoPath").ToString) Then
                    bwBuildHTML.ReportProgress(1)
                    Continue For
                End If
                FilterTVShows.Add(Convert.ToInt32(_curShow.Item("ID")))
                Dim hfile As String = Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvindex").DestPath.Replace("/", Path.DirectorySeparatorChar))
                HTMLTVBody.Append(ProcessTVShowsTags(_curShow, hfile, ShowCounter, _curShow.Item("ID").ToString, tvrow))



                ShowCounter += 1
                Dim SeasonCounter As Integer = 1
                Dim tvshowheader As String = String.Empty
                Dim tvshowfooter As String = String.Empty
                Dim tvshowrow As String = String.Empty
                Dim HTMLTVShowBody As New StringBuilder
                Dim ShowDetailsrow As String = patternShowDetails
                Dim showrow As String = String.Empty
                If Not String.IsNullOrEmpty(htmlShowDetailsPath) Then
                    detailsShowOutput = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvshow").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat("Show", _curShow.Item("ID").ToString, ".htm"))
                    ShowDetailsrow = ProcessTVShowsTags(_curShow, detailsShowOutput, ShowCounter, _curShow.Item("ID").ToString, ShowDetailsrow, GetUserParam("RelativePathToBase", "../../"))
                End If
                GetHtmlParts(ShowDetailsrow, tvshowheader, tvshowrow, tvshowfooter, "TVSEASON")
                HTMLTVShowBody.Append(tvshowheader)
                For Each _curSeason As DataRow In dtSeasons.Select(String.Format("TVShowID = {0} AND Season <> 999", _curShow.Item("ID").ToString))
                    If dtEpisodes.Select(String.Format("TVShowID = {0} AND Season = {1}", _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString)).Count = 0 Then
                        Continue For
                    End If
                    If Not String.IsNullOrEmpty(htmlShowDetailsPath) Then
                        showrow = ProcessTVSeasonTags(_curShow, _curSeason, detailsShowOutput, SeasonCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, tvshowrow, GetUserParam("RelativePathToBase", "../../"))
                        '**File.WriteAllText(detailsShowOutput, ProcessTVSeasonTags(_curSeason, detailsShowOutput, SeasonCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, patternSeasonDetails, GetUserParam("RelativePathToBase", "../../")))
                    End If
                    Dim SeasonId As String = String.Concat(_curShow.Item("ID").ToString, ".", _curSeason.Item("Season").ToString)
                    'Dim eprow As String = String.Empty
                    If Not String.IsNullOrEmpty(htmlSeasonDetailsPath) Then
                        detailsSeasonOutput = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvseason").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat("Season", SeasonId, ".htm"))
                    End If

                    Dim tvshowSeasonheader As String = String.Empty
                    Dim tvshowSeasonfooter As String = String.Empty
                    Dim tvshowSeasonrow As String = String.Empty
                    GetHtmlParts(showrow, tvshowSeasonheader, tvshowSeasonrow, tvshowSeasonfooter, "TVEPISODE")

                    Dim EpisodeCounter As Integer = 1
                    showrow = tvshowSeasonheader
                    For Each _curEp As DataRow In dtEpisodes.Select(String.Format("TVShowID = {0} AND Season = {1}", _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString))
                        If bwBuildHTML.CancellationPending Then Return
                        If Not String.IsNullOrEmpty(htmlShowDetailsPath) Then
                            Dim showepisode As String = ProcessTVEpisodeTags(_curEp, detailsShowOutput, EpisodeCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, _curEp.Item("ID").ToString, tvshowSeasonrow, GetUserParam("RelativePathToBase", "../../"))
                            showrow = String.Concat(showrow, showepisode)
                        End If
                        If Not String.IsNullOrEmpty(htmlSeasonDetailsPath) Then
                            'HTMLTVSeasonBody.Append(ProcessTVEpisodeTags(_curEp, detailsSeasonOutput, EpisodeCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, _curEp.Item("ID").ToString, patternSeasonDetails, GetUserParam("RelativePathToBase", "../../")))
                            '**File.WriteAllText(detailsSeasonOutput, ProcessTVEpisodeTags(_curEp, detailsSeasonOutput, EpisodeCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, _curEp.Item("ID").ToString, patternEpDetails, GetUserParam("RelativePathToBase", "../../")))
                        End If
                        If Not String.IsNullOrEmpty(htmlEpDetailsPath) Then
                            detailsEpsOutput = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvepisode").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat("Eps", _curEp.Item("ID").ToString, ".htm"))
                            'File.WriteAllText(detailsEpsOutput, ProcessTVEpisodeTags(_curEp, detailsEpsOutput, EpisodeCounter, _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString, _curEp.Item("ID").ToString, patternEpDetails, GetUserParam("RelativePathToBase", "../../")))
                        End If
                        EpisodeCounter += 1
                        'bwBuildHTML.ReportProgress(1)
                    Next

                    If Not String.IsNullOrEmpty(htmlShowDetailsPath) Then
                        showrow = String.Concat(showrow, tvshowSeasonfooter)
                        HTMLTVShowBody.Append(showrow)
                    End If

                    SeasonCounter += 1
                Next
                HTMLTVShowBody.Append(tvshowfooter)
                If File.Exists(detailsShowOutput) Then
                    System.IO.File.Delete(detailsShowOutput)
                End If
                Dim writer As New StreamWriter(detailsShowOutput, False, Encoding.Default)
                writer.Write(HTMLTVShowBody.ToString)
                writer.Close()
            Next
            HTMLTVBody.Append(tvfooter)
            'HTMLTVBody.Replace("<$GENRES_LIST>", StringUtils.HtmlEncode(Strings.Join(MoviesGenres.ToArray, ",")))
            DontSaveExtra = False
            Me.SaveTVFiles(Path.GetDirectoryName(htmlPath), outputbase)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Function GetMovieActorForID(ByVal id As String) As String
        Dim dtActorMedia = New DataTable
        Dim actors As String = String.Empty
        Master.DB.FillDataTable(dtActorMedia, String.Format("SELECT ActorName FROM MoviesActors WHERE MovieID = {0}", id))
        For Each s As DataRow In dtActorMedia.Rows
            actors = String.Concat(actors, If(Not String.IsNullOrEmpty(actors), " / ", String.Empty), s.Item("ActorNAme").ToString)
        Next
        Return actors
    End Function
    Private Function GetRelativePath(ByVal mediaPath As String, ByVal sourcePath As String, ByVal mapPath As String, ByVal outputPath As String) As String
        Dim ret As String = String.Empty
        If Not String.IsNullOrEmpty(mapPath) Then
            ret = mediaPath.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/")
        Else
            Dim pr As String ' = Path.GetPathRoot(outputPath)
            outputPath = outputPath.Substring(outputFolder.Length) 'outputPath.Substring(pr.Length)
            Dim count As Integer
            For Each ch As Char In outputPath
                If ch = Convert.ToChar(Path.DirectorySeparatorChar) Then
                    count += 1
                End If
            Next ch
            For c As Integer = 1 To count
                ret = String.Concat(ret, "../")
            Next
            pr = String.Empty
            If Not String.IsNullOrEmpty(mediaPath) Then pr = Path.GetPathRoot(mediaPath)
            mediaPath = mediaPath.Substring(pr.Length).Replace(Path.DirectorySeparatorChar, "/")
            ret = String.Concat(ret, mediaPath)
        End If
        'String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curMovie.Item("Source").ToString))
        Return ret
    End Function

    Private Function GetSufix(ByVal _type As String) As String
        Dim p As NMTExporterModule.Config._ImageProcessing = conf.ImageProcessing.FirstOrDefault(Function(y) y._type = _type)
        If Not IsNothing(p) Then
            Dim c As NMTExporterModule.Config._ImageProcessingCommand = p.Commands.FirstOrDefault(Function(x) x.prefix = String.Empty)
            If Not IsNothing(c) Then Return c.sufix
        End If
        Return ".jpg"
    End Function

    Function ProcessMovieTags(ByVal _curMovie As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String) As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("ThumbsPath", "Thumbs/")
            Dim BackdropPath As String = GetUserParam("BackdropPath", "Thumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curMovie.Item("Source").ToString).ToString), String.Empty, selectedSources(_curMovie.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = _curMovie.Item("Source").ToString).Path
            row = row.Replace("<$ID>", id.ToString)
            row = row.Replace("<$COUNTER>", counter.ToString)
            ' NMT don't like StringUtils.HtmlEncode, Seem like it use local Ansi (Enconding.Default)
            row = row.Replace("<$THUMB_PATH>", GetRelativePath(ThumbsPath, String.Empty, String.Empty, outputbase))
            row = row.Replace("<$MOVIE_PATH>", GetRelativePath(_curMovie.Item("MoviePath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$POSTER_THUMB>", GetRelativePath(String.Concat(ThumbsPath, id.ToString, GetSufix("Thumb")), String.Empty, String.Empty, outputbase))
            row = row.Replace("<$BACKDROP_THUMB>", GetRelativePath(String.Concat(BackdropPath, id.ToString, "-backdrop.jpg"), String.Empty, String.Empty, outputbase))
            row = row.Replace("<$POSTER_FILE>", GetRelativePath(_curMovie.Item("PosterPath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$FANART_FILE>", GetRelativePath(_curMovie.Item("FanartPath").ToString, sourcePath, mapPath, outputbase))
            If Not String.IsNullOrEmpty(_curMovie.Item("Title").ToString) Then
                row = row.Replace("<$MOVIENAME>", (_curMovie.Item("Title").ToString))
            Else
                row = row.Replace("<$MOVIENAME>", (_curMovie.Item("ListTitle").ToString))
            End If
            row = row.Replace("<$ORIGINALTITLE>", (_curMovie.Item("OriginalTitle").ToString))
            row = row.Replace("<$ACTORS>", (GetMovieActorForID(_curMovie.Item("ID").ToString)))
            row = row.Replace("<$DIRECTOR>", (_curMovie.Item("Director").ToString))
            row = row.Replace("<$CERTIFICATION>", (_curMovie.Item("Certification").ToString))
            row = row.Replace("<$IMDBID>", (_curMovie.Item("IMDB").ToString))
            row = row.Replace("<$MPAA>", (_curMovie.Item("MPAA").ToString))
            row = row.Replace("<$RELEASEDATE>", (_curMovie.Item("ReleaseDate").ToString))
            row = row.Replace("<$RUNTIME>", (_curMovie.Item("Runtime").ToString))
            row = row.Replace("<$TAGLINE>", (_curMovie.Item("Tagline").ToString))
            row = row.Replace("<$RATING>", (_curMovie.Item("Rating").ToString))
            row = row.Replace("<$VOTES>", (_curMovie.Item("Votes").ToString))
            row = row.Replace("<$LISTTITLE>", (_curMovie.Item("ListTitle").ToString))
            row = row.Replace("<$YEAR>", _curMovie.Item("Year").ToString)
            row = row.Replace("<$FILENAME>", (Path.GetFileName(_curMovie.Item("MoviePath").ToString)))
            row = row.Replace("<$DIRNAME>", (Path.GetDirectoryName(_curMovie.Item("MoviePath").ToString)))
            row = row.Replace("<$OUTLINE>", ToStringNMT(_curMovie.Item("Outline").ToString))
            row = row.Replace("<$PLOT>", ToStringNMT(_curMovie.Item("Plot").ToString))
            row = row.Replace("<$FILESOURCE>", If(Not String.IsNullOrEmpty(_curMovie.Item("FileSource").ToString), _curMovie.Item("FileSource").ToString, "default"))
            row = row.Replace("<$COUNTRY>", _curMovie.Item("Country").ToString)
            For Each s As String In _curMovie.Item("Country").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesCountries.Contains(s.Trim) Then MoviesCountries.Add(s.Trim)
            Next
            row = row.Replace("<$GENRES>", (_curMovie.Item("Genre").ToString))
            For Each s As String In _curMovie.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesGenres.Contains(s.Trim) Then MoviesGenres.Add(s.Trim)
            Next
            row = row.Replace("<$SIZE>", (MovieSize(_curMovie.Item("MoviePath").ToString).ToString))
            If row.Contains("<$EXTRATHUMB>") Then
                If DirectCast(_curMovie.Item("HasExtra"), Boolean) Then
                    Dim di As DirectoryInfo = New DirectoryInfo(Path.GetDirectoryName(_curMovie.Item("ExtraPath").ToString))
                    Dim c As Integer = di.GetFiles("thumb*.jpg").Count
                    row = row.Replace("<$EXTRATHUMB>", c.ToString)
                Else
                    row = row.Replace("<$EXTRATHUMB>", "0")
                End If
            End If
            row = row.Replace("<$DATEADD>", (Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curMovie.Item("DateAdd").ToString)).ToShortDateString))
            Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curMovie.Item("ID").ToString)
            If row.Contains("<$VIDEO>") OrElse row.Contains("<$VIDEO_DIMENSIONS>") OrElse row.Contains("<$AUDIO>") Then

                Dim _vidDetails As String = String.Empty
                If row.Contains("<$VIDEO>") OrElse row.Contains("<$VIDEO_DIMENSIONS>") Then
                    Dim _vidDimensions As String = String.Empty
                    If Not IsNothing(fiAV) Then
                        If fiAV.StreamDetails.Video.Count > 0 Then
                            tVid = NFO.GetBestVideo(fiAV)
                            tRes = NFO.GetResFromDimensions(tVid)
                            _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                            _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                        End If
                    End If
                    row = row.Replace("<$VIDEO>", _vidDetails)
                    row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
                End If
                If row.Contains("<$AUDIO>") Then
                    Dim _audDetails As String = String.Empty
                    If fiAV.StreamDetails.Audio.Count > 0 Then
                        tAud = NFO.GetBestAudio(fiAV, False)
                        _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
                    End If
                    row = row.Replace("<$AUDIO>", _audDetails)
                End If
            End If
            row = GetAVImages(fiAV, row, _curMovie.Item("MoviePath").ToString, _curMovie.Item("FileSource").ToString, GetRelativePath(String.Empty, String.Empty, String.Empty, outputbase))
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVShowsTags(ByVal _curShow As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curShow.Item("Source").ToString).ToString), String.Empty, selectedSources(_curShow.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.TVSources.FirstOrDefault(Function(y) y.Name = _curShow.Item("Source").ToString).Path
            row = row.Replace("<$SHOWID>", id.ToString)
            row = row.Replace("<$SHOWCOUNTER>", counter.ToString)
            row = row.Replace("<$SHOWNAME>", (_curShow.Item("Title").ToString))
            row = row.Replace("<$TVPOSTER_THUMB>", GetRelativePath(String.Concat(ThumbsPath, id.ToString, ".jpg"), String.Empty, String.Empty, outputbase))
            row = row.Replace("<$TVBACKDROP_THUMB>", GetRelativePath(String.Concat(BackdropPath, id.ToString, "-backdrop.jpg"), String.Empty, String.Empty, outputbase))
            row = row.Replace("<$TVSHOW_PATH>", GetRelativePath(_curShow.Item("TVShowPath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$SHOWPLOT>", ToStringNMT(_curShow.Item("Plot").ToString))
            row = row.Replace("<$SHOWGENRES>", (_curShow.Item("Genre").ToString))
            row = row.Replace("<$TVPOSTER_FILE>", GetRelativePath(_curShow.Item("PosterPath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$TVFANART_FILE>", GetRelativePath(_curShow.Item("FanartPath").ToString, sourcePath, mapPath, outputbase))
            If row.Contains("<$TVAllSEASONS_POSTER>") Then
                Dim _allSeasons As DataRow = dtSeasons.Select(String.Format("TVShowID = {0} AND Season = 999", _curShow.Item("ID").ToString))(0)
                row = row.Replace("<$TVAllSEASONS_POSTER>", GetRelativePath(_allSeasons.Item("PosterPath").ToString, sourcePath, mapPath, outputbase))
            End If

            For Each s As String In _curShow.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not TVShowsGenres.Contains(s.Trim) Then TVShowsGenres.Add(s.Trim)
            Next
            'row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curShow.Item("ID").ToString)))
            'row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curShow.Item("Director").ToString))
            'row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curShow.Item("Certification").ToString))
            'row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curShow.Item("IMDB").ToString))
            'row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curShow.Item("MPAA").ToString))
            'row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curShow.Item("ReleaseDate").ToString))
            'row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curShow.Item("Runtime").ToString))
            'row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curShow.Item("Tagline").ToString))
            'row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curShow.Item("Rating").ToString))
            'row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curShow.Item("Votes").ToString))
            'row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curShow.Item("ListTitle").ToString))
            'row = row.Replace("<$YEAR>", _curShow.Item("Year").ToString)
            'row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curShow.Item("MoviePath").ToString)))
            'row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curShow.Item("MoviePath").ToString)))
            'row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curShow.Item("Outline").ToString))
            'row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curShow.Item("MoviePath").ToString).ToString))
            'row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curShow.Item("DateAdd").ToString)).ToShortDateString))
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVSeasonTags(ByVal _curShow As DataRow, ByVal _curSeason As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal showid As String, ByVal seasonid As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String

        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")

            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curShow.Item("Source").ToString).ToString), String.Empty, selectedSources(_curShow.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.TVSources.FirstOrDefault(Function(y) y.Name = _curShow.Item("Source").ToString).Path
            'row = row.Replace("<$SHOWID>", showid)
            row = row.Replace("<$SEASONID>", seasonid)
            row = row.Replace("<$SEASONCOUNTER>", counter.ToString)
            row = row.Replace("<$SEASON_POSTER_FILE>", GetRelativePath(_curSeason.Item("PosterPath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$SEASONNAME>", (_curSeason.Item("SeasonText").ToString))
            'row = row.Replace("<$SEASON_POSTER>", String.Concat(relpath, ThumbsPath, seasonid.ToString, ".jpg"))
            'row = row.Replace("<$SEASON_BACKDROP>", String.Concat(relpath, BackdropPath, seasonid.ToString, "-backdrop.jpg"))            '_curSeason.Item("PosterPath").ToString.Replace(sourcePath, mappath).Replace(Path.DirectorySeparatorChar, "/"))
            'row = row.Replace("<$SEASON_FANART_FILE>", _curSeason.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            'row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curSeason.Item("Director").ToString))
            'row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curSeason.Item("Certification").ToString))
            'row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curSeason.Item("Tagline").ToString))
            'row = row.Replace("<$YEAR>", _curSeason.Item("Year").ToString)
            'row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curSeason.Item("Outline").ToString))
            'row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curSeason.Item("Plot").ToString))
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVEpisodeTags(ByVal _curEpisode As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal showid As String, ByVal seasonid As String, ByVal epid As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")
            Dim uni As New UnicodeEncoding()
            'Bellow need to be fixed
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curEpisode.Item("Source").ToString).ToString), String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curEpisode.Item("Source").ToString)), selectedSources(_curEpisode.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.TVSources.FirstOrDefault(Function(y) y.Name = _curEpisode.Item("Source").ToString).Path
            row = row.Replace("<$EPISODEID>", epid.ToString)
            row = row.Replace("<$EPISODECOUNTER>", counter.ToString)
            row = row.Replace("<$EPISODE_POSTER_FILE>", GetRelativePath(_curEpisode.Item("PosterPath").ToString, sourcePath, mapPath, outputbase))
            row = row.Replace("<$EPISODENAME>", (_curEpisode.Item("Title").ToString))
            row = row.Replace("<$EPISODEPLOT>", ToStringNMT(_curEpisode.Item("Plot").ToString))
            For Each _epPath As DataRow In dtEpisodesPaths.Select(String.Format("ID = {0}", _curEpisode.Item("TVEpPathid").ToString))
                row = row.Replace("<$EPISODE_PATH>", _epPath.Item("TVEpPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            Next

            'row = row.Replace("<$POSTER_THUMB>", String.Concat(relpath, ThumbsPath, epid.ToString, ".jpg"))
            'row = row.Replace("<$BACKDROP_THUMB>", String.Concat(relpath, BackdropPath, epid.ToString, "-backdrop.jpg"))
            'row = row.Replace("<$FANART_FILE>", _curEpisode.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            'row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curEpisode.Item("ID").ToString)))
            'row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curEpisode.Item("Director").ToString))
            'row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curEpisode.Item("Certification").ToString))
            'row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curEpisode.Item("IMDB").ToString))
            'row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curEpisode.Item("MPAA").ToString))
            'row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curEpisode.Item("ReleaseDate").ToString))
            'row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curEpisode.Item("Runtime").ToString))
            'row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curEpisode.Item("Tagline").ToString))
            'row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curEpisode.Item("Rating").ToString))
            'row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curEpisode.Item("Votes").ToString))
            'row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curEpisode.Item("ListTitle").ToString))
            'row = row.Replace("<$YEAR>", _curEpisode.Item("Year").ToString)
            'row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curEpisode.Item("MoviePath").ToString)))
            'row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curEpisode.Item("MoviePath").ToString)))
            'row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curEpisode.Item("Outline").ToString))
            'row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curEpisode.Item("Genre").ToString))
            'For Each s As String In _curEpisode.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
            'If Not TVShowsGenres.Contains(s.Trim) Then TVShowsGenres.Add(s.Trim)
            'Next
            'row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curEpisode.Item("MoviePath").ToString).ToString))
            'row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curEpisode.Item("DateAdd").ToString)).ToShortDateString))
            'If row.Contains("<$VIDEO>") OrElse row.Contains("<$VIDEO_DIMENSIONS>") OrElse row.Contains("<$AUDIO>") Then
            'Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curEpisode.Item("ID").ToString)
            'If row.Contains("<$VIDEO>") OrElse row.Contains("<$VIDEO_DIMENSIONS>") Then
            'Dim _vidDetails As String = String.Empty
            'Dim _vidDimensions As String = String.Empty
            'If Not IsNothing(fiAV) Then
            'If fiAV.StreamDetails.Video.Count > 0 Then
            'tVid = NFO.GetBestVideo(fiAV)
            'tRes = NFO.GetResFromDimensions(tVid)
            '_vidDimensions = NFO.GetDimensionsFromVideo(tVid)
            '_vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
            'End If
            'End If
            'row = row.Replace("<$VIDEO>", _vidDetails)
            'row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
            'End If
            'If row.Contains("<$AUDIO>") Then
            ' Dim _audDetails As String = String.Empty
            ' If fiAV.StreamDetails.Audio.Count > 0 Then
            ' tAud = NFO.GetBestAudio(fiAV, False)
            ' _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
            ' End If
            ' row = row.Replace("<$AUDIO>", _audDetails)
            ' End If
            ' row = GetAVImages(fiAV, row, _curEpisode.Item("MoviePath").ToString, "", relpath)
            ' End If


        Catch ex As Exception
        End Try

        Return row
    End Function


    Public Shared Function ToUTF8(ByVal unicodeString As String) As String
        Dim utf8 As Encoding = Encoding.UTF8
        Dim [unicode] As Encoding = Encoding.Unicode
        Dim unicodeBytes As Byte() = [unicode].GetBytes(unicodeString)
        Dim utf8Bytes As Byte() = Encoding.Convert([unicode], utf8, unicodeBytes)
        Dim utf8Chars(utf8.GetCharCount(utf8Bytes, 0, utf8Bytes.Length) - 1) As Char
        utf8.GetChars(utf8Bytes, 0, utf8Bytes.Length, utf8Chars, 0)
        Dim utf8String As New String(utf8Chars)

        Return utf8String
    End Function




    Private Function GetMovieFileInfo(ByVal MovieID As String) As MediaInfo.Fileinfo
        Dim fi As New MediaInfo.Fileinfo
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesVStreams WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim video As MediaInfo.Video
                While SQLreader.Read
                    video = New MediaInfo.Video
                    If Not DBNull.Value.Equals(SQLreader("Video_Width")) Then video.Width = SQLreader("Video_Width").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Height")) Then video.Height = SQLreader("Video_Height").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Codec")) Then video.Codec = SQLreader("Video_Codec").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Duration")) Then video.Duration = SQLreader("Video_Duration").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_ScanType")) Then video.Scantype = SQLreader("Video_ScanType").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_AspectDisplayRatio")) Then video.Aspect = SQLreader("Video_AspectDisplayRatio").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Language")) Then video.Language = SQLreader("Video_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_LongLanguage")) Then video.LongLanguage = SQLreader("Video_LongLanguage").ToString
                    fi.StreamDetails.Video.Add(video)
                End While
            End Using
        End Using

        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesAStreams WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim audio As MediaInfo.Audio
                While SQLreader.Read
                    audio = New MediaInfo.Audio
                    If Not DBNull.Value.Equals(SQLreader("Audio_Language")) Then audio.Language = SQLreader("Audio_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_LongLanguage")) Then audio.LongLanguage = SQLreader("Audio_LongLanguage").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_Codec")) Then audio.Codec = SQLreader("Audio_Codec").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_Channel")) Then audio.Channels = SQLreader("Audio_Channel").ToString
                    fi.StreamDetails.Audio.Add(audio)
                End While
            End Using
        End Using
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesSubs WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim subtitle As MediaInfo.Subtitle
                While SQLreader.Read
                    subtitle = New MediaInfo.Subtitle
                    If Not DBNull.Value.Equals(SQLreader("Subs_Language")) Then subtitle.Language = SQLreader("Subs_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_LongLanguage")) Then subtitle.LongLanguage = SQLreader("Subs_LongLanguage").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_Type")) Then subtitle.SubsType = SQLreader("Subs_Type").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_Path")) Then subtitle.SubsPath = SQLreader("Subs_Path").ToString
                    fi.StreamDetails.Subtitle.Add(subtitle)
                End While
            End Using
        End Using
        Return fi
    End Function


    Private Sub cbTemplate_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbTemplate.MouseHover
        lblHelpa.Text = Master.eLang.GetString(5, "Choose a template")
    End Sub

    Private Sub cbTemplate_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbTemplate.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub cbTemplate_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTemplate.SelectedIndexChanged
        If cbTemplate.SelectedIndex >= 0 Then
            conf = confs(cbTemplate.SelectedIndex)
            template_Path = conf.TemplatePath
            PopulateParams()
            lblTemplateInfo.Text = conf.Description
            If File.Exists(Path.Combine(conf.TemplatePath, "logo.jpg")) Then
                pbTemplateLogo.Load(Path.Combine(conf.TemplatePath, "logo.jpg"))
            Else
                pbTemplateLogo.Image = Nothing
            End If
            pbHelp.Visible = File.Exists(Path.Combine(conf.TemplatePath, "help.txt"))
            chHighPriority.Checked = AdvancedSettings.GetBooleanSetting(String.Concat("HighPriority.", conf.Name), False)
            DontSaveExtra = False
            'btnSave.Enabled = False
        End If

    End Sub


    Private Sub dlgExportMovies_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        'FileUtils.Delete.DeleteDirectory(Me.TempPath)
    End Sub

    Private Sub dlgExportMovies_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    End Sub

    Private Sub PopulateParams()
        For Each s As DataGridViewRow In dgvSources.Rows
            ' Dim i As Integer = dgvSources.Rows.Add(New Object() {False, s.Name, My.Resources.film, String.Empty, "movie"})
            If s.Cells(4).Value.ToString = "movie" Then
                s.Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.Movie.", conf.Name, ".", s.Cells(1).Value.ToString), "")
                s.Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.Movie.Status.", conf.Name, ".", s.Cells(1).Value.ToString), False)
            Else
                s.Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.TV.", conf.Name, ".", s.Cells(1).Value.ToString), "")
                s.Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.TV.Status.", conf.Name, ".", s.Cells(1).Value.ToString), False)
            End If
        Next

        dgvSettings.Rows.Clear()
        For Each c As NMTExporterModule.Config._Param In conf.Params.OrderByDescending(Function(y) y.access)
            Dim i As Integer
            i = dgvSettings.Rows.Add(New Object() {c.name})
            If c.access = "user" AndAlso c.type = "bool" Then
                Dim cCell As New DataGridViewComboBoxCell()
                dgvSettings.Rows(i).Cells(1) = cCell
                Dim dcb As DataGridViewComboBoxCell = DirectCast(dgvSettings.Rows(i).Cells(1), DataGridViewComboBoxCell)
                dcb.DataSource = New String() {"true", "false"}
                dcb.Value = AdvancedSettings.GetSetting(String.Concat("Param.", conf.Name, ".", c.name), c.value)
            Else
                Dim cCell As New DataGridViewTextBoxCell()
                dgvSettings.Rows(i).Cells(1) = cCell
                Dim dcb As DataGridViewTextBoxCell = DirectCast(dgvSettings.Rows(i).Cells(1), DataGridViewTextBoxCell)
                If c.access = "internal" Then
                    dcb.Value = c.value
                Else
                    dcb.Value = AdvancedSettings.GetSetting(String.Concat("Param.", conf.Name, ".", c.name), c.value)
                End If

                If c.access = "hidden" Then
                    dgvSettings.Rows(i).Visible = False
                End If
            End If
            If Not c.access = "user" Then
                dgvSettings.Rows(i).ReadOnly = True
                dgvSettings.Rows(i).DefaultCellStyle.ForeColor = Color.Gray
                dgvSettings.Rows(i).DefaultCellStyle.SelectionBackColor = Color.White
                dgvSettings.Rows(i).DefaultCellStyle.SelectionForeColor = Color.Gray
            End If
        Next
        dgvProperties.Rows.Clear()
        For Each c As NMTExporterModule.Config._Property In conf.Properties.OrderByDescending(Function(y) y.group)
            Dim i As Integer
            Dim lst As New List(Of String)
            i = dgvProperties.Rows.Add(New Object() {c.label})
            dgvProperties.Rows(i).Tag = c.name
            If c.type = "list" Then
                For Each s As NMTExporterModule.Config._value In c.values
                    lst.Add(s.label)
                Next
                Dim cCell As New DataGridViewComboBoxCell()
                dgvProperties.Rows(i).Cells(1) = cCell
                Dim dcb As DataGridViewComboBoxCell = DirectCast(dgvProperties.Rows(i).Cells(1), DataGridViewComboBoxCell)
                dcb.DataSource = lst.ToArray
                'If lst.Count > 0 Then dcb.Value = lst(0)
                Dim saved As String = AdvancedSettings.GetSetting(String.Concat("Property.", conf.Name, ".", c.name), lst(0))
                Dim defvalue As String = c.values.FirstOrDefault(Function(y) y.label = saved).value
                defvalue = If(IsNothing(defvalue), String.Empty, defvalue)
                c.value = defvalue
                If lst.Count > 0 Then dcb.Value = saved
            End If
        Next
    End Sub

    Private Sub SetUserParam(ByVal param As String, ByVal value As String)
        Dim c As NMTExporterModule.Config._Param = conf.Params.FirstOrDefault(Function(y) y.name = param)
        If Not c Is Nothing Then
            c.value = value
        End If
    End Sub

    Private Function GetUserParam(ByVal param As String, ByVal defvalue As String) As String
        Dim c As NMTExporterModule.Config._Param = conf.Params.FirstOrDefault(Function(y) y.name = param)
        If Not c Is Nothing Then
            Return c.value
        Else
            Return defvalue
        End If
    End Function

    Private Sub SetAllUserParam()
        For Each r As DataGridViewRow In dgvSettings.Rows
            Dim r0 As DataGridViewRow = r
            Dim c As NMTExporterModule.Config._Param = conf.Params.FirstOrDefault(Function(y) y.name = r0.Cells(0).Value.ToString)
            If Not c Is Nothing Then c.value = r.Cells(1).Value.ToString
        Next
    End Sub

    Private Sub dgvProperties_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvProperties.CurrentCellDirtyStateChanged
        If dgvProperties.IsCurrentCellDirty AndAlso dgvProperties.CurrentCell.ColumnIndex = 1 Then
            dgvProperties.CommitEdit(DataGridViewDataErrorContexts.Commit)
            btnSave.Enabled = True
            Dim s As String = dgvProperties.CurrentCell.Value.ToString
            Dim p As NMTExporterModule.Config._Property = conf.Properties.FirstOrDefault(Function(y) y.name = dgvProperties.Rows(dgvProperties.CurrentCell.RowIndex).Tag.ToString)
            p.value = p.values.FirstOrDefault(Function(y) y.label = s).value
            p.value = If(IsNothing(p.value), String.Empty, p.value)
        End If
    End Sub

    Private Sub dlgMoviesReport_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub DoCancel()
        btnCancel.Visible = False
        lblCompiling.Visible = False
        pbCompile.Style = ProgressBarStyle.Marquee
        pbCompile.MarqueeAnimationSpeed = 25
        lblCanceling.Visible = True
    End Sub

    Private Function ImageNeedProcess(ByVal _type As String) As Boolean
        For Each s As NMTExporterModule.Config._ImageProcessing In conf.ImageProcessing
            If s._type = _type Then Return True
        Next
        Return False
    End Function

    Private Sub ExportTVShowPosterThumb(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim finalpath As String = Path.Combine(fpath, GetUserParam("TVThumbsPath", "Thumbs/").Replace("/", Path.DirectorySeparatorChar))
            Dim counter As Integer = 1
            Directory.CreateDirectory(finalpath)
            For Each _curShow As DataRow In dtShows.Rows
                If Not FilterTVShows.Contains(Convert.ToInt32(_curShow.Item("ID"))) Then Continue For

                Try
                    Dim posterfile As String = Path.Combine(finalpath, String.Concat(_curShow.Item("ID").ToString, ".jpg"))
                    If File.Exists(_curShow.Item("PosterPath").ToString) AndAlso Not File.Exists(posterfile) Then
                        If ImageNeedProcess("TVThumb") Then
                            For Each s As NMTExporterModule.Config._ImageProcessing In conf.ImageProcessing.Where(Function(y) y._type = "Thumb")
                                For Each c As NMTExporterModule.Config._ImageProcessingCommand In s.Commands
                                    posterfile = Path.Combine(finalpath, String.Concat(c.prefix, _curShow.Item("ID").ToString, c.sufix))
                                    Dim exe As String = Path.Combine(Path.Combine(sBasePath, "bin"), c.execute)
                                    Dim params As String = c.params.Replace("$1", String.Concat("""", _curShow.Item("PosterPath").ToString, """")).Replace("$2", String.Concat("""", posterfile, """"))
                                    Using execute As New Process
                                        execute.StartInfo.FileName = exe
                                        execute.StartInfo.Arguments = params
                                        execute.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                                        execute.Start()
                                        If Not bHighPriority Then
                                            While Not execute.HasExited
                                                Application.DoEvents()
                                                execute.Refresh()
                                                Threading.Thread.Sleep(50)
                                            End While
                                        End If
                                    End Using
                                Next
                            Next
                        Else
                            Dim im As New Images
                            If new_width > 0 Then

                                im.FromFile(_curShow.Item("PosterPath").ToString)
                                ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                                im.Save(posterfile)
                            Else
                                File.Copy(_curShow.Item("PosterPath").ToString, posterfile, True)
                            End If
                        End If
                        If ImageNeedProcess("TVPoster") Then
                            For Each s As NMTExporterModule.Config._ImageProcessing In conf.ImageProcessing.Where(Function(y) y._type = "Poster")
                                For Each c As NMTExporterModule.Config._ImageProcessingCommand In s.Commands
                                    posterfile = Path.Combine(finalpath, String.Concat(c.prefix, _curShow.Item("ID").ToString, c.sufix))
                                    Dim exe As String = Path.Combine(Path.Combine(sBasePath, "bin"), c.execute)
                                    Dim params As String = c.params.Replace("$1", String.Concat("""", _curShow.Item("PosterPath").ToString, """")).Replace("$2", String.Concat("""", posterfile, """"))
                                    Using execute As New Process
                                        execute.StartInfo.FileName = exe
                                        execute.StartInfo.Arguments = params
                                        execute.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                                        execute.Start()
                                        If Not bHighPriority Then
                                            While Not execute.HasExited
                                                Application.DoEvents()
                                                execute.Refresh()
                                                Threading.Thread.Sleep(50)
                                            End While
                                        End If
                                    End Using
                                Next
                            Next
                        End If
                    End If
                Catch ex As Exception
                End Try
                counter += 1
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                bwBuildHTML.ReportProgress(1)
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ExportMoviePosterThumb(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim finalpath As String = Path.Combine(fpath, GetUserParam("ThumbsPath", "Thumbs/").Replace("/", Path.DirectorySeparatorChar))
            Dim counter As Integer = 1
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As DataRow In dtMovieMedia.Rows
                If Not FilterMovies.Contains(Convert.ToInt32(_curMovie.Item("ID"))) Then Continue For

                Try
                    Dim posterfile As String = Path.Combine(finalpath, String.Concat(_curMovie.Item("ID").ToString, ".jpg"))
                    If File.Exists(_curMovie.Item("PosterPath").ToString) AndAlso Not File.Exists(posterfile) Then
                        If ImageNeedProcess("Thumb") Then
                            For Each s As NMTExporterModule.Config._ImageProcessing In conf.ImageProcessing.Where(Function(y) y._type = "Thumb")
                                For Each c As NMTExporterModule.Config._ImageProcessingCommand In s.Commands
                                    posterfile = Path.Combine(finalpath, String.Concat(c.prefix, _curMovie.Item("ID").ToString, c.sufix))
                                    Dim exe As String = Path.Combine(Path.Combine(sBasePath, "bin"), c.execute)
                                    Dim params As String = c.params.Replace("$1", String.Concat("""", _curMovie.Item("PosterPath").ToString, """")).Replace("$2", String.Concat("""", posterfile, """"))
                                    Using execute As New Process
                                        execute.StartInfo.FileName = exe
                                        execute.StartInfo.Arguments = params
                                        execute.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                                        execute.Start()
                                        If Not bHighPriority Then
                                            While Not execute.HasExited
                                                Application.DoEvents()
                                                execute.Refresh()
                                                Threading.Thread.Sleep(50)
                                            End While
                                        End If
                                    End Using
                                Next
                            Next
                        Else
                            Dim im As New Images
                            If new_width > 0 Then

                                im.FromFile(_curMovie.Item("PosterPath").ToString)
                                ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                                im.Save(posterfile)
                            Else
                                File.Copy(_curMovie.Item("PosterPath").ToString, posterfile, True)
                            End If
                        End If
                        If ImageNeedProcess("Poster") Then
                            For Each s As NMTExporterModule.Config._ImageProcessing In conf.ImageProcessing.Where(Function(y) y._type = "Poster")
                                For Each c As NMTExporterModule.Config._ImageProcessingCommand In s.Commands
                                    posterfile = Path.Combine(finalpath, String.Concat(c.prefix, _curMovie.Item("ID").ToString, c.sufix))
                                    Dim exe As String = Path.Combine(Path.Combine(sBasePath, "bin"), c.execute)
                                    Dim params As String = c.params.Replace("$1", String.Concat("""", _curMovie.Item("PosterPath").ToString, """")).Replace("$2", String.Concat("""", posterfile, """"))
                                    Using execute As New Process
                                        execute.StartInfo.FileName = exe
                                        execute.StartInfo.Arguments = params
                                        execute.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                                        execute.Start()
                                        If Not bHighPriority Then
                                            While Not execute.HasExited
                                                Application.DoEvents()
                                                execute.Refresh()
                                                Threading.Thread.Sleep(50)
                                            End While
                                        End If
                                    End Using
                                Next
                            Next
                        End If
                    End If
                Catch ex As Exception
                End Try
                counter += 1
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                bwBuildHTML.ReportProgress(1)
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ExportMovieBackDrops(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim counter As Integer = 1
            Dim finalpath As String = Path.Combine(fpath, GetUserParam("BackdropPath", "Thumbs/").Replace("/", Path.DirectorySeparatorChar))
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As DataRow In dtMovieMedia.Rows
                If Not FilterMovies.Contains(Convert.ToInt32(_curMovie.Item("ID"))) Then Continue For
                Try
                    Dim Fanartfile As String = Path.Combine(finalpath, String.Concat(_curMovie.Item("ID").ToString, "-backdrop.jpg"))
                    If File.Exists(_curMovie.Item("FanartPath").ToString) AndAlso Not File.Exists(Fanartfile) Then
                        If new_width > 0 Then
                            Dim im As New Images
                            im.FromFile(_curMovie.Item("FanartPath").ToString)
                            ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                            im.Save(Fanartfile, 65)
                        Else
                            File.Copy(_curMovie.Item("FanartPath").ToString, Fanartfile, True)
                        End If
                    End If

                Catch
                End Try
                counter += 1
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                bwBuildHTML.ReportProgress(1)
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function GetAVImages(ByVal fiAV As MediaInfo.Fileinfo, ByVal line As String, ByVal filename As String, ByVal FileSource As String, Optional ByVal relpath As String = "") As String
        If APIXML.lFlags.Count > 0 Then
            Try
                Dim flagspath As String = GetUserParam("FlagsPath", "Flags/")
                Dim tVideo As MediaInfo.Video = NFO.GetBestVideo(fiAV)
                Dim tAudio As MediaInfo.Audio = NFO.GetBestAudio(fiAV, False)

                Dim vresFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = NFO.GetResFromDimensions(tVideo).ToLower AndAlso f.Type = APIXML.FlagType.VideoResolution)
                If Not IsNothing(vresFlag) Then
                    line = line.Replace("<$FLAG_VRES>", String.Concat(relpath, flagspath, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                Else
                    vresFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoResolution)
                    If Not IsNothing(vresFlag) Then
                        line = line.Replace("<$FLAG_VRES>", String.Concat(relpath, flagspath, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                    End If
                End If

                'Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = APIXML.GetFileSource(filename) AndAlso f.Type = APIXML.FlagType.VideoSource)
                Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name.ToLower = FileSource.ToLower AndAlso f.Type = APIXML.FlagType.VideoSource)
                If Not IsNothing(vsourceFlag) Then
                    line = line.Replace("<$FLAG_VSOURCE>", String.Concat(relpath, flagspath, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                Else
                    vsourceFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoSource)
                    If Not IsNothing(vsourceFlag) Then
                        line = line.Replace("<$FLAG_VSOURCE>", String.Concat(relpath, flagspath, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim vcodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tVideo.Codec.ToLower AndAlso f.Type = APIXML.FlagType.VideoCodec)
                If Not IsNothing(vcodecFlag) Then
                    line = line.Replace("<$FLAG_VTYPE>", String.Concat(relpath, flagspath, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                Else
                    vcodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoCodec)
                    If Not IsNothing(vcodecFlag) Then
                        line = line.Replace("<$FLAG_VTYPE>", String.Concat(relpath, flagspath, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim acodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Codec.ToLower AndAlso f.Type = APIXML.FlagType.AudioCodec)
                If Not IsNothing(acodecFlag) Then
                    line = line.Replace("<$FLAG_ATYPE>", String.Concat(relpath, flagspath, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                Else
                    acodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioCodec)
                    If Not IsNothing(acodecFlag) Then
                        line = line.Replace("<$FLAG_ATYPE>", String.Concat(relpath, flagspath, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim achanFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Channels AndAlso f.Type = APIXML.FlagType.AudioChan)
                If Not IsNothing(achanFlag) Then
                    line = line.Replace("<$FLAG_ACHAN>", String.Concat(relpath, flagspath, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                Else
                    achanFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioChan)
                    If Not IsNothing(achanFlag) Then
                        line = line.Replace("<$FLAG_ACHAN>", String.Concat(relpath, flagspath, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                    End If
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If
        Return line
    End Function

    Function MovieSize(ByVal spath As String) As Long
        Dim MovieFilesSize As Long = 0
        If StringUtils.IsStacked(Path.GetFileNameWithoutExtension(spath), True) OrElse FileUtils.Common.isVideoTS(spath) OrElse FileUtils.Common.isBDRip(spath) Then
            Try
                Dim sExt As String = Path.GetExtension(spath).ToLower
                Dim oFile As String = StringUtils.CleanStackingMarkers(spath, False)
                Dim sFile As New List(Of String)
                Dim bIsVTS As Boolean = False

                If sExt = ".ifo" OrElse sExt = ".bup" OrElse sExt = ".vob" Then
                    bIsVTS = True
                End If

                If bIsVTS Then
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, "VTS*.VOB"))
                    Catch
                    End Try
                ElseIf sExt = ".m2ts" Then
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, "*.m2ts"))
                    Catch
                    End Try
                Else
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, StringUtils.CleanStackingMarkers(Path.GetFileName(spath), True)))
                    Catch
                    End Try
                End If

                For Each File As String In sFile
                    MovieFilesSize += FileLen(File)
                Next
            Catch ex As Exception
            End Try
        End If
        Return MovieFilesSize
    End Function

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(1, "Jukebox Builder")
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.lblCompiling.Text = Master.eLang.GetString(2, "Compiling Movie List...")
        Me.lblCanceling.Text = Master.eLang.GetString(3, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Label2.Text = Master.eLang.GetString(4, "Template")
        Me.Label1.Text = Master.eLang.GetString(14, "Output Folder")
        Me.btnSave.Text = Master.eLang.GetString(15, "Save Settings")
        Me.btnBuild.Text = Master.eLang.GetString(16, "Build")
        Me.gbHelp.Text = String.Concat("     ", Master.eLang.GetString(17, "Help"))
        Me.dgvProperties.Columns(0).HeaderText = Master.eLang.GetString(29, "Property")
        Me.dgvProperties.Columns(1).HeaderText = Master.eLang.GetString(30, "Value")
        Me.dgvSources.Columns(1).HeaderText = Master.eLang.GetString(31, "Ember Source")
        Me.dgvSources.Columns(3).HeaderText = Master.eLang.GetString(32, "NMT Path")
        Me.dgvSettings.Columns(0).HeaderText = Master.eLang.GetString(33, "Setting")
        Me.dgvSettings.Columns(1).HeaderText = Master.eLang.GetString(30, "Value")
        Me.TabPage1.Text = Master.eLang.GetString(34, "Template Settings")
        Me.TabPage2.Text = Master.eLang.GetString(35, "Skin Properties")
        Me.chHighPriority.Text = Master.eLang.GetString(39, "High Priority")

    End Sub

    Private Structure Arguments
        Dim destPath As String
        Dim resizePoster As Integer
        Dim srcPath As String
    End Structure

    Private Sub btnBuild_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuild.Click
        DoBuild()
    End Sub
    Public Sub DoBuild()
        btnSave.Tag = btnSave.Enabled
        Me.bexportPosters = If(GetUserParam("ExportPosters", "true").ToLower = "true", True, False)
        Me.bexportBackDrops = If(GetUserParam("ExportBackdrops", "true").ToLower = "true", True, False)
        Me.bexportFlags = If(GetUserParam("ExportFlags", "true").ToLower = "true", True, False)
        cbTemplate.Enabled = False
        txtOutputFolder.Enabled = False
        btnBuild.Enabled = False
        CanBuild = False
        dgvProperties.Enabled = False
        dgvSettings.Enabled = False
        dgvSources.Enabled = False
        chHighPriority.Enabled = False
        btnSave.Enabled = False
        TabControl1.Enabled = False
        If HaveMovies Then pbCompile.Maximum = dtMovieMedia.Rows.Count + If(bexportPosters, dtMovieMedia.Rows.Count, 0) + If(bexportBackDrops, dtMovieMedia.Rows.Count, 0)
        If HaveTV Then pbCompile.Maximum = pbCompile.Maximum + dtShows.Rows.Count
        pbCompile.Value = pbCompile.Minimum
        btnCancel.Visible = True
        btnCancel.Enabled = True
        lblCompiling.Visible = True
        pbCompile.Visible = True
        lblCompiling.Text = Master.eLang.GetString(20, "Starting...")
        pbCompile.Style = ProgressBarStyle.Continuous
        pnlCancel.Visible = True
        pnlCancel.BringToFront()
        outputFolder = txtOutputFolder.Text
        bHighPriority = chHighPriority.Checked
        outputFolder = If(outputFolder.EndsWith(Path.DirectorySeparatorChar), outputFolder, String.Concat(outputFolder, Path.DirectorySeparatorChar))
        Try
            While True
                If GetUserParam("CleanBasePath", "true").ToLower = "true" Then
                    lblCompiling.Text = Master.eLang.GetString(21, "Cleaning output folder...")
                    Dim mythread As New Thread(AddressOf DoDelete)
                    Dim bpath As String = Path.Combine(outputFolder, GetUserParam("BasePath", ".Ember/").Replace("/", Path.DirectorySeparatorChar))
                    If bpath = outputFolder Then
                        MessageBox.Show(Master.eLang.GetString(6, "BasePath can not be the same as Output Folder"), Master.eLang.GetString(7, "Warning"), MessageBoxButtons.OK)
                        Return
                    End If
                    mythread.Start(bpath)
                    While mythread.IsAlive
                        Application.DoEvents()
                        Threading.Thread.Sleep(50)
                    End While
                End If
                lblCompiling.Text = Master.eLang.GetString(23, "Preparing folders")
                For Each s As NMTExporterModule.Config._Param In conf.Params.Where(Function(y) y.type = "path")
                    If Not Directory.Exists(Path.Combine(outputFolder, s.value.Replace("/", Path.DirectorySeparatorChar))) Then
                        Try
                            Directory.CreateDirectory(Path.Combine(outputFolder, s.value.Replace("/", Path.DirectorySeparatorChar)))
                        Catch ex As Exception
                            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                            Exit While
                        End Try
                    End If
                Next
                Me.bwBuildHTML.WorkerReportsProgress = True
                Me.bwBuildHTML.WorkerSupportsCancellation = True
                Me.bwBuildHTML.RunWorkerAsync()
                While bwBuildHTML.IsBusy
                    Application.DoEvents()
                    Threading.Thread.Sleep(50)
                End While

                Exit While
            End While
        Catch ex As Exception
        End Try
        pnlCancel.Visible = False
        cbTemplate.Enabled = True
        txtOutputFolder.Enabled = True
        btnBuild.Enabled = True
        CanBuild = True
        TabControl1.Enabled = True
        dgvSettings.Enabled = True
        dgvSources.Enabled = True
        dgvProperties.Enabled = True
        chHighPriority.Enabled = True
        btnSave.Enabled = DirectCast(btnSave.Tag, Boolean)
    End Sub
    Private Shared Sub DoDelete(ByVal state As Object)
        If DirectCast(state, String).Contains("..") Then Return
        Try
            Directory.Delete(DirectCast(state, String), True)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub bwBuildHTML_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwBuildHTML.DoWork
        Try
            If HaveMovies Then BuildMovieHTML(template_Path, outputFolder)
            If HaveTV Then BuildTVHTML(template_Path, outputFolder)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub bwBuildHTML_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwBuildHTML.ProgressChanged
        If Not e.UserState Is Nothing AndAlso Not String.IsNullOrEmpty(e.UserState.ToString) Then
            'pbCompile.Style = ProgressBarStyle.Marquee
            'pbCompile.MarqueeAnimationSpeed = 25
            lblCompiling.Text = e.UserState.ToString '"Exporting Data..."
        Else
            pbCompile.Value += e.ProgressPercentage
        End If

    End Sub
    Private Function ToStringNMT(ByVal instr As String) As String
        Return instr.Replace("""", "\""").Replace(vbCr, String.Empty).Replace(vbLf, String.Empty)
    End Function
    Private Sub SaveMovieFiles(ByVal srcPath As String, ByVal destPath As String)
        Try
            bwBuildHTML.ReportProgress(0, Master.eLang.GetString(8, "Movies - Exporting Data..."))
            If Not DontSaveExtra Then
                For Each f As NMTExporterModule.Config._File In conf.Files.Where(Function(y) y.Type = "other")
                    Dim dstPath As String = Path.Combine(Path.Combine(outputFolder, f.DestPath.Replace("/", Path.DirectorySeparatorChar)), f.Name)
                    File.Copy(Path.Combine(srcPath, f.Name), dstPath, True)
                    If f.Process Then
                        Dim fileContent As String = File.ReadAllText(dstPath)
                        PreProcessProperties(fileContent)
                        File.WriteAllText(dstPath, fileContent)
                    End If
                    'CopyDirectory(srcPath, Path.GetDirectoryName(destPath), True)
                    If bwBuildHTML.CancellationPending Then Return
                Next
                For Each f As NMTExporterModule.Config._File In conf.Files.Where(Function(y) y.Type = "folder")
                    Dim srcf As String = Path.Combine(srcPath, f.Name)
                    Dim destf As String = Path.Combine(outputFolder, Path.Combine(f.DestPath.Replace("/", Path.DirectorySeparatorChar), f.Name))
                    If Not srcf.EndsWith(Path.DirectorySeparatorChar) Then srcf = String.Concat(srcf, Path.DirectorySeparatorChar)
                    If Not destf.EndsWith(Path.DirectorySeparatorChar) Then destf = String.Concat(destf, Path.DirectorySeparatorChar)
                    CopyDirectory(srcf, destf, True)
                    If bwBuildHTML.CancellationPending Then Return
                Next
                If Me.bexportFlags Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(9, "Movies - Exporting Flags..."))
                    srcPath = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags", Path.DirectorySeparatorChar)
                    Dim flagspath As String = Path.Combine(destPath, GetUserParam("FlagsPath", "Flags/").Replace("/", Path.DirectorySeparatorChar))
                    CopyDirectory(srcPath, flagspath, True)
                End If
                If bwBuildHTML.CancellationPending Then Return
                If Me.bexportPosters Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(10, "Movies - Exporting Posters..."))
                    Me.ExportMoviePosterThumb(destPath, Convert.ToInt32(GetUserParam("PostersThumbWidth", "160")))
                End If
                If bwBuildHTML.CancellationPending Then Return
                If Me.bexportBackDrops Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(11, "Movies - Exporting Backdrops..."))
                    Me.ExportMovieBackDrops(destPath, Convert.ToInt32(GetUserParam("BackdropWidth", "1280")))
                End If
                If bwBuildHTML.CancellationPending Then Return
                DontSaveExtra = True
            End If
            Dim hfile As String = Path.Combine(Path.Combine(destPath, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").DestPath), conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").Name)
            If File.Exists(hfile) Then
                System.IO.File.Delete(hfile)
            End If
            ' ANSI based on Regional Setting , seems to be only thing NMT likes
            Dim writer As New StreamWriter(hfile, False, Encoding.Default)
            writer.Write(HTMLMovieBody.ToString)
            writer.Close()
            'Dim myStream As Stream = File.OpenWrite(hfile)
            'If Not IsNothing(myStream) Then
            'myStream.Write(System.Text.Encoding.UTF8.GetBytes(HTMLMovieBody.ToString), 0, HTMLMovieBody.ToString.Length)
            'myStream.Close()
            'End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SaveTVFiles(ByVal srcPath As String, ByVal destPath As String)
        Try
            bwBuildHTML.ReportProgress(0, Master.eLang.GetString(48, "TV - Exporting Data..."))
            If Not DontSaveExtra Then
                If Not HaveMovies Then ' If have Movies already done!
                    For Each f As NMTExporterModule.Config._File In conf.Files.Where(Function(y) y.Type = "other")
                        Dim dstPath As String = Path.Combine(Path.Combine(outputFolder, f.DestPath.Replace("/", Path.DirectorySeparatorChar)), f.Name)
                        File.Copy(Path.Combine(srcPath, f.Name), dstPath, True)
                        If f.Process Then
                            Dim fileContent As String = File.ReadAllText(dstPath)
                            PreProcessProperties(fileContent)
                            File.WriteAllText(dstPath, fileContent)
                        End If
                        'CopyDirectory(srcPath, Path.GetDirectoryName(destPath), True)
                        If bwBuildHTML.CancellationPending Then Return
                    Next
                    For Each f As NMTExporterModule.Config._File In conf.Files.Where(Function(y) y.Type = "folder")
                        Dim srcf As String = Path.Combine(srcPath, f.Name)
                        Dim destf As String = Path.Combine(outputFolder, Path.Combine(f.DestPath.Replace("/", Path.DirectorySeparatorChar), f.Name))
                        If Not srcf.EndsWith(Path.DirectorySeparatorChar) Then srcf = String.Concat(srcf, Path.DirectorySeparatorChar)
                        If Not destf.EndsWith(Path.DirectorySeparatorChar) Then destf = String.Concat(destf, Path.DirectorySeparatorChar)
                        CopyDirectory(srcf, destf, True)
                        If bwBuildHTML.CancellationPending Then Return
                    Next
                    If Me.bexportFlags Then
                        bwBuildHTML.ReportProgress(0, Master.eLang.GetString(49, "TV - Exporting Flags..."))
                        srcPath = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags", Path.DirectorySeparatorChar)
                        Dim flagspath As String = Path.Combine(destPath, GetUserParam("FlagsPath", "Flags/").Replace("/", Path.DirectorySeparatorChar))
                        CopyDirectory(srcPath, flagspath, True)
                    End If
                End If

                If bwBuildHTML.CancellationPending Then Return
                If Me.bexportPosters Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(50, "TV - Exporting Posters..."))
                    Me.ExportTVShowPosterThumb(destPath, Convert.ToInt32(GetUserParam("TVPostersThumbWidth", "160")))
                End If
                If bwBuildHTML.CancellationPending Then Return
                If Me.bexportBackDrops Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(51, "TV - Exporting Backdrops..."))
                    'Me.ExportBackDrops(destPath, Convert.ToInt32(GetUserParam("BackdropWidth", "1280")))
                End If
                If bwBuildHTML.CancellationPending Then Return
                DontSaveExtra = True
            End If
            Dim hfile As String = Path.Combine(Path.Combine(destPath, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvindex").DestPath), conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvindex").Name)
            If File.Exists(hfile) Then
                System.IO.File.Delete(hfile)
            End If
            ' ANSI based on Regional Setting , seems to be only thing NMT likes
            Dim writer As New StreamWriter(hfile, False, Encoding.Default)
            writer.Write(HTMLTVBody.ToString)
            writer.Close()

            'Dim myStream As Stream = File.OpenWrite(hfile)
            'If Not IsNothing(myStream) Then
            'myStream.Write(System.Text.Encoding.ASCII.GetBytes((HTMLTVBody.ToString)), 0, (HTMLTVBody.ToString).Length)
            'myStream.Close()
            'End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvSources_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSources.CellValueChanged
        ValidatedToBuild.Start()
        'btnSave.Enabled = False
    End Sub

    Private Sub dgvSources_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.CurrentCellDirtyStateChanged
        If dgvSources.IsCurrentCellDirty Then
            dgvSources.CommitEdit(DataGridViewDataErrorContexts.Commit)
            btnSave.Enabled = True
        End If
    End Sub


    Private Sub dgvSources_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.MouseHover
        lblHelpa.Text = String.Format(Master.eLang.GetString(12, "Use the NMT Path when the source is not on the same Drive/Share as the Output folder.{0}Some common paths are:{0}file:///opt/sybhttpd/localhost.drives/NETWORK_SHARE/[remote_filesystem_name]/[Path_to_Source]{0}file:///opt/sybhttpd/localhost.drives/HARD_DISK/USB_DRIVE_A-1/[Path_to_Source]"), vbCrLf)
    End Sub

    Private Sub dgvSources_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub ValidatedToBuild_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ValidatedToBuild.Tick
        ValidatedToBuild.Stop()
        ValidatedToBuild.Interval = 300
        selectedSources.Clear()
        HaveMovies = False
        HaveTV = False
        Cursor = Cursors.WaitCursor
        pbWarning.Image = ilNMT.Images("red")
        lblWarning.Text = Master.eLang.GetString(24, "Validating Info...")
        Application.DoEvents()
        Dim warn As String = String.Empty
        If outputChanged Then
            outputChanged = False
            OutputExist = Directory.Exists(txtOutputFolder.Text)
        End If
        If Not OutputExist Then
            warn = Master.eLang.GetString(25, "Invalid Output Folder")
        End If
        If String.IsNullOrEmpty(warn) Then
            For Each row As DataGridViewRow In dgvSources.Rows
                Dim dcb As DataGridViewCheckBoxCell = DirectCast(row.Cells(0), DataGridViewCheckBoxCell)
                If DirectCast(dcb.Value, Boolean) = True Then
                    Try
                        row.Cells(3).Value = If(IsNothing(row.Cells(3).Value), String.Empty, row.Cells(3).Value)
                        If String.IsNullOrEmpty(row.Cells(3).Value.ToString) AndAlso Not Path.GetPathRoot(row.Cells(1).ToolTipText).ToLower = Path.GetPathRoot(txtOutputFolder.Text).ToLower Then
                            warn = Master.eLang.GetString(26, "Output Folder don't match Selected Sources and no NMT Path defined")
                            Exit For
                        Else
                            selectedSources.Add(row.Cells(1).Value.ToString, row.Cells(3).Value.ToString)
                            If row.Cells(4).Value.ToString = "tv" Then HaveTV = True
                            If row.Cells(4).Value.ToString = "movie" Then HaveMovies = True
                        End If
                    Catch ex As Exception
                        ' TODO Strings
                        warn = Master.eLang.GetString(25, "Invalid Output Folder")
                        Exit For
                    End Try
                End If
            Next
        End If
        If String.IsNullOrEmpty(warn) AndAlso Not conf Is Nothing AndAlso selectedSources.Count > 0 AndAlso OutputExist Then
            btnBuild.Enabled = True
            CanBuild = True
        Else
            btnBuild.Enabled = False
            CanBuild = False
        End If
        lblWarning.Text = warn
        If String.IsNullOrEmpty(warn) Then
            pbWarning.Image = Nothing 'ilNMT.Images("green")
        Else
            pbWarning.Image = ilNMT.Images("block")
        End If
        Cursor = Cursors.Default
    End Sub

    Private Sub txtOutputFolder_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtOutputFolder.LostFocus
        ValidatedToBuild.Start()
        btnSave.Enabled = True
    End Sub

    Private Sub txtOutputFolder_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtOutputFolder.MouseHover
        lblHelpa.Text = Master.eLang.GetString(13, "Select Root Folder where Jukebox files will be exported")
    End Sub

    Private Sub txtOutputFolder_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtOutputFolder.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub txtOutputFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtOutputFolder.TextChanged
        btnBuild.Enabled = False
        outputChanged = True
        ValidatedToBuild.Interval = 2000
        ValidatedToBuild.Start()
        btnSave.Enabled = True
    End Sub
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SaveConfig()
        btnSave.Enabled = False
    End Sub

    Private Sub dgvSettings_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellValueChanged
        btnSave.Enabled = True
        SetAllUserParam()
    End Sub

    Private Sub dgvSettings_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSettings.CurrentCellDirtyStateChanged
        If dgvSettings.IsCurrentCellDirty Then
            dgvSettings.CommitEdit(DataGridViewDataErrorContexts.Commit)
            btnSave.Enabled = True
        End If
    End Sub
    Private Sub dgvSettings_CellMouseEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellMouseEnter
        If e.RowIndex >= 0 Then
            lblHelpa.Text = conf.Params.FirstOrDefault(Function(y) y.name = dgvSettings.Rows(e.RowIndex).Cells(0).Value.ToString).description
        End If

    End Sub

    Private Sub dgvSettings_CellMouseLeave(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellMouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub dgvProperties_CellMouseEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvProperties.CellMouseEnter
        If e.RowIndex >= 0 AndAlso Not IsNothing(dgvProperties.Rows(e.RowIndex).Tag) Then
            lblHelpa.Text = conf.Properties.FirstOrDefault(Function(y) y.name = dgvProperties.Rows(e.RowIndex).Tag.ToString).description
        End If
    End Sub

    Private Sub dgvProperties_CellMouseLeave(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvProperties.CellMouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub lblTemplateInfo_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblTemplateInfo.MouseHover
        If Not IsNothing(conf) Then
            lblHelpa.Text = If(Not String.IsNullOrEmpty(conf.Author), String.Concat("Author ", conf.Author, vbCrLf), String.Empty)
            If conf.ReadMe AndAlso File.Exists(Path.Combine(conf.TemplatePath, "readme.txt")) Then
                Dim readme As String = File.ReadAllText(Path.Combine(conf.TemplatePath, "readme.txt"))
                lblHelpa.Text = String.Concat(lblHelpa.Text, readme)
            End If
        End If
    End Sub

    Private Sub lblTemplateInfo_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblTemplateInfo.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Try
            Using fbdBrowse As New System.Windows.Forms.FolderBrowserDialog
                If fbdBrowse.ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not String.IsNullOrEmpty(fbdBrowse.SelectedPath) Then
                        Me.txtOutputFolder.Text = fbdBrowse.SelectedPath
                    End If
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub
    Private oldWarning As String = String.Empty
    Private Sub pbWarning_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbWarning.Click
        If Not IsNothing(pbWarning.Image) Then
            ValidatedToBuild.Start()
        End If
    End Sub
    Private Sub pbWarning_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbWarning.MouseHover
        If Not IsNothing(pbWarning.Image) Then
            Cursor = Cursors.Hand
            oldWarning = lblWarning.Text
        End If
    End Sub
    Private Sub pbWarning_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbWarning.MouseLeave
        If Not IsNothing(pbWarning.Image) Then
            Cursor = Cursors.Default
            lblWarning.Text = oldWarning
        End If
    End Sub

    Private Sub chHighPriority_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles chHighPriority.MouseHover
        lblHelpa.Text = Master.eLang.GetString(40, "When using external tools, run several instances for a faster build")
    End Sub

    Private Sub chHighPriority_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles chHighPriority.MouseLeave
        lblHelpa.Text = ""
    End Sub
    Private Sub pbHelp_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbHelp.MouseHover
        lblHelpa.Text = Master.eLang.GetString(41, "Click for more details")
        Cursor = Cursors.Help
    End Sub


    Private Sub pbHelp_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbHelp.MouseLeave
        lblHelpa.Text = ""
        Cursor = Cursors.Default
    End Sub
    Private Sub chHighPriority_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chHighPriority.CheckedChanged
        btnSave.Enabled = True
    End Sub
    Private Sub pbHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbHelp.Click
        Using dlg As New dlgHelp
            dlg.ShowDialog(conf.TemplatePath)
        End Using
    End Sub
#End Region
End Class