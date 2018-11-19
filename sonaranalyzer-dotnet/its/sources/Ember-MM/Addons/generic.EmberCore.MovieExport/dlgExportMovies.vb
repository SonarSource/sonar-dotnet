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
Imports EmberAPI

Public Class dlgExportMovies

#Region "Fields"

    Friend WithEvents bwLoadInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwSaveAll As New System.ComponentModel.BackgroundWorker

    Private base_template As String
    Private bCancelled As Boolean = False
    Private bexportFanart As Boolean = False
    Private bexportFlags As Boolean = False
    Private bexportPosters As Boolean = False
    Private bFiltered As Boolean = False
    Private DontSaveExtra As Boolean = False
    Dim FilterMovies As New List(Of Long)
    Private HTMLBody As New StringBuilder
    Private isCL As Boolean = False
    Private TempPath As String = Path.Combine(Master.TempPath, "Export")
    Private use_filter As Boolean = False
    Private workerCanceled As Boolean = False
    Private _movies As New List(Of Structures.DBMovie)

#End Region 'Fields

#Region "Methods"

    Public Shared Sub CLExport(ByVal filename As String, Optional ByVal template As String = "template", Optional ByVal resizePoster As Integer = 0)
        Try
            Dim MySelf As New dlgExportMovies
            If Not Directory.Exists(Path.GetDirectoryName(filename)) Then
                Return
            End If
            MySelf.isCL = True
            MySelf.bwLoadInfo = New System.ComponentModel.BackgroundWorker
            MySelf.bwLoadInfo.WorkerSupportsCancellation = True
            MySelf.bwLoadInfo.WorkerReportsProgress = True
            MySelf.bwLoadInfo.RunWorkerAsync()
            While MySelf.bwLoadInfo.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
            MySelf.BuildHTML(False, String.Empty, String.Empty, template, False)
            Dim srcPath As String = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html", Path.DirectorySeparatorChar, template, Path.DirectorySeparatorChar)
            MySelf.SaveAll(String.Empty, srcPath, filename, resizePoster)
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
                If (ChildFile.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden OrElse Path.GetExtension(ChildFile.FullName) = ".html" Then Continue For
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
        'Me.Close()
        If bwSaveAll.IsBusy Then
            bwSaveAll.CancelAsync()
        End If
        btnCancel.Enabled = False
    End Sub

    Private Sub BuildHTML(ByVal bSearch As Boolean, ByVal strFilter As String, ByVal strIn As String, ByVal template As String, ByVal doNavigate As Boolean)
        Try
            ' Build HTML Documment in Code ... ugly but will work until new option

            HTMLBody.Length = 0

            Me.bexportPosters = False
            Me.bexportFanart = False
            Me.bexportFlags = False

            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim htmlPath As String = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html", Path.DirectorySeparatorChar, template, Path.DirectorySeparatorChar, Master.eSettings.Language, ".html")
            Dim pattern As String
            Dim movieheader As String = String.Empty
            Dim moviefooter As String = String.Empty
            Dim movierow As String = String.Empty
            If Not File.Exists(htmlPath) Then
                htmlPath = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html", Path.DirectorySeparatorChar, template, Path.DirectorySeparatorChar, "English_(en_US).html")
            End If
            If Not File.Exists(htmlPath) Then
                Return
            End If
            pattern = File.ReadAllText(htmlPath)
            If pattern.Contains("<$NEED_POSTERS>") Then
                Me.bexportPosters = True
                pattern = pattern.Replace("<$NEED_POSTERS>", String.Empty)
            End If
            If pattern.Contains("<$NEED_FANART>") Then
                Me.bexportFanart = True
                pattern = pattern.Replace("<$NEED_FANART>", String.Empty)
            End If
            If pattern.Contains("<$NEED_FLAGS>") Then
                Me.bexportFlags = True
                pattern = pattern.Replace("<$NEED_FLAGS>", String.Empty)
            End If
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

            If bSearch Then
                bFiltered = True
            Else
                bFiltered = False
            End If

            HTMLBody.Append(movieheader)
            Dim counter As Integer = 1
            FilterMovies.Clear()
            For Each _curMovie As Structures.DBMovie In _movies
                Dim _vidDetails As String = String.Empty
                Dim _vidDimensions As String = String.Empty
                Dim _audDetails As String = String.Empty
                If Not IsNothing(_curMovie.Movie.FileInfo) Then
                    If _curMovie.Movie.FileInfo.StreamDetails.Video.Count > 0 Then
                        tVid = NFO.GetBestVideo(_curMovie.Movie.FileInfo)
                        tRes = NFO.GetResFromDimensions(tVid)
                        _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                        _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                    End If

                    If _curMovie.Movie.FileInfo.StreamDetails.Audio.Count > 0 Then
                        tAud = NFO.GetBestAudio(_curMovie.Movie.FileInfo, False)
                        _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
                    End If
                End If

                'now check if we need to include this movie
                If bSearch Then
                    If strIn = Master.eLang.GetString(1, "Source Folder") Then
                        Dim found As Boolean = False
                        For Each u As String In strFilter.Split(Convert.ToChar(";"))
                            If _curMovie.Source = u Then
                                found = True
                                Exit For
                            End If
                        Next
                        '_curMovie.IsMark = False
                        If Not found Then Continue For
                    Else
                        If (strIn = Master.eLang.GetString(12, "Video Flag") AndAlso StringUtils.Wildcard.IsMatch(_vidDetails, strFilter)) OrElse _
                           (strIn = Master.eLang.GetString(13, "Audio Flag") AndAlso StringUtils.Wildcard.IsMatch(_audDetails, strFilter)) OrElse _
                           (strIn = Master.eLang.GetString(21, "Title", True) AndAlso StringUtils.Wildcard.IsMatch(_curMovie.Movie.Title, strFilter)) OrElse _
                           (strIn = Master.eLang.GetString(278, "Year", True) AndAlso StringUtils.Wildcard.IsMatch(_curMovie.Movie.Year, strFilter)) Then
                            'included - build the output
                        Else
                            'filtered out - exclude this one
                            '_curMovie.IsMark = False
                            Continue For
                        End If
                    End If
                End If
                FilterMovies.Add(_curMovie.ID)
                Dim uni As New UnicodeEncoding()

                Dim row As String = movierow
                row = row.Replace("<$MOVIE_PATH>", String.Empty)
                row = row.Replace("<$POSTER_FILE>", String.Concat("export/", counter.ToString, ".jpg"))
                row = row.Replace("<$FANART_FILE>", String.Concat("export/", counter.ToString, "-fanart.jpg"))
                If Not String.IsNullOrEmpty(_curMovie.Movie.Title) Then
                    row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.Movie.Title))
                Else
                    row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.ListTitle))
                End If
                row = row.Replace("<$ORIGINALTITLE>", StringUtils.HtmlEncode(_curMovie.Movie.OriginalTitle))
                row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(Functions.ListToStringWithSeparator(_curMovie.Movie.Actors, ",")))
                row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curMovie.Movie.Director))
                row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curMovie.Movie.Certification))
                row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curMovie.Movie.IMDBID))
                row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curMovie.Movie.MPAA))
                row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curMovie.Movie.ReleaseDate))
                row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curMovie.Movie.Runtime))
                row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curMovie.Movie.Tagline))
                row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curMovie.Movie.Rating))
                row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curMovie.Movie.Votes))
                row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curMovie.ListTitle))
                row = row.Replace("<$YEAR>", _curMovie.Movie.Year)
                row = row.Replace("<$COUNTRY>", StringUtils.HtmlEncode(_curMovie.Movie.Country))
                row = row.Replace("<$COUNT>", counter.ToString)
                row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curMovie.Filename)))
                row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curMovie.Filename)))
                row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curMovie.Movie.Outline))
                row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curMovie.Movie.Plot))
                row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curMovie.Movie.Genre))
                row = row.Replace("<$VIDEO>", _vidDetails)
                row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
                row = row.Replace("<$AUDIO>", _audDetails)
                row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curMovie.Filename).ToString))
                row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(_curMovie.DateAdd).ToShortDateString))
                row = GetAVImages(_curMovie, row)
                HTMLBody.Append(row)
                counter += 1
            Next
            HTMLBody.Append(moviefooter)

            If Not Me.isCL Then
                Dim outFile As String = Path.Combine(Me.TempPath, String.Concat(Master.eSettings.Language, ".html"))
                DontSaveExtra = False
                Me.SaveAll(If(doNavigate, Master.eLang.GetString(4, "Preparing preview. Please wait..."), String.Empty), Path.GetDirectoryName(htmlPath), outFile)
                If doNavigate Then LoadHTML()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSource.Click
        If btnSource.ImageIndex = 0 Then
            lstSources.Visible = True
            btnSource.ImageIndex = 1
        Else
            lstSources.Visible = False
            btnSource.ImageIndex = 0
            Dim sFilter As String = String.Empty
            If cbSearch.Text = Master.eLang.GetString(5, "Source Folder") Then
                For Each s In lstSources.CheckedItems
                    sFilter = String.Concat(sFilter, If(sFilter = String.Empty, String.Empty, ";"), s.ToString)
                Next
                txtSearch.Text = sFilter
            End If
        End If
    End Sub

    Private Sub bwLoadInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadInfo.DoWork
        '//
        ' Thread to load movieinformation (from nfo)
        '\\
        Try
            ' Clean up Movies List if any
            _movies.Clear()
            ' Load nfo movies using path from DB
            Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                Dim _tmpMovie As New Structures.DBMovie
                Dim _ID As Integer
                Dim iProg As Integer = 0
                SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies;")
                Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    Me.bwLoadInfo.ReportProgress(-1, SQLcount("mcount")) ' set maximum
                End Using
                SQLNewcommand.CommandText = String.Concat("SELECT ID FROM movies ORDER BY SortTitle COLLATE NOCASE;")
                Using SQLreader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    If SQLreader.HasRows Then
                        While SQLreader.Read()
                            _ID = Convert.ToInt32(SQLreader("ID"))
                            _tmpMovie = Master.DB.LoadMovieFromDB(_ID)
                            _movies.Add(_tmpMovie)
                            Me.bwLoadInfo.ReportProgress(iProg, _tmpMovie.ListTitle) '  show File
                            iProg += 1
                            If bwLoadInfo.CancellationPending Then
                                e.Cancel = True
                                Return
                            End If
                        End While
                        e.Result = True
                    Else
                        e.Cancel = True
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadInfo_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadInfo.ProgressChanged
        If Not Me.isCL Then
            If e.ProgressPercentage >= 0 Then
                Me.pbCompile.Value = e.ProgressPercentage
                Me.lblFile.Text = e.UserState.ToString
            Else
                Me.pbCompile.Maximum = Convert.ToInt32(e.UserState)
            End If
        End If
    End Sub

    Private Sub bwLoadInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadInfo.RunWorkerCompleted
        '//
        ' Thread finished: display it if not cancelled
        '\\
        If Not Me.isCL Then
            bCancelled = e.Cancelled
            If Not e.Cancelled Then
                If Not Me.isCL Then
                    BuildHTML(False, String.Empty, String.Empty, base_template, False)
                End If
                LoadHTML()
            Else
                wbMovieList.DocumentText = String.Concat("<center><h1 style=""color:Red;"">", Master.eLang.GetString(284, "Canceled", True), "</h1></center>")
            End If
            Me.pnlCancel.Visible = False
        End If
    End Sub

    Private Sub bwSaveAll_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwSaveAll.DoWork
        Try

            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Dim destPathShort As String = Path.GetDirectoryName(Args.destPath)
            'Only create extra files once for each template... dont do it when applyng filters
            If Not DontSaveExtra Then
                FileUtils.Delete.DeleteDirectory(Me.TempPath)
                CopyDirectory(Args.srcPath, destPathShort, True)
                If Me.bexportFlags Then
                    Args.srcPath = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags", Path.DirectorySeparatorChar)
                    Directory.CreateDirectory(Path.Combine(destPathShort, "Flags"))
                    CopyDirectory(Args.srcPath, Path.Combine(destPathShort, "Flags"), True)
                End If
                If bwSaveAll.CancellationPending Then
                    e.Cancel = True
                    Return
                End If
                If Me.bexportPosters Then
                    Me.ExportPoster(destPathShort, Args.resizePoster)
                End If
                If bwSaveAll.CancellationPending Then
                    e.Cancel = True
                    Return
                End If
                If Me.bexportFanart Then
                    Me.ExportFanart(destPathShort)
                End If
                If bwSaveAll.CancellationPending Then
                    e.Cancel = True
                    Return
                End If
                DontSaveExtra = True
            Else
                If File.Exists(Args.destPath) Then
                    System.IO.File.Delete(Args.destPath)
                End If
            End If
            Dim myStream As Stream = File.OpenWrite(Args.destPath)
            If Not IsNothing(myStream) Then
                myStream.Write(System.Text.Encoding.ASCII.GetBytes(HTMLBody.ToString), 0, HTMLBody.ToString.Length)
                myStream.Close()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwSaveAll_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwSaveAll.RunWorkerCompleted
        workerCanceled = e.Cancelled
    End Sub

    Private Sub cbFilterSource_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If ((cbSearch.Text = Master.eLang.GetString(5, "Source Folder") AndAlso lstSources.CheckedItems.Count > 0) OrElse txtSearch.Text <> "") AndAlso cbSearch.Text <> "" Then
            Search_Button.Enabled = True
        Else
            Search_Button.Enabled = False
        End If
    End Sub

    Private Sub cbSearch_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbSearch.SelectedIndexChanged
        If ((cbSearch.Text = Master.eLang.GetString(5, "Source Folder") AndAlso lstSources.CheckedItems.Count > 0) OrElse txtSearch.Text <> "") AndAlso cbSearch.Text <> "" Then
            Search_Button.Enabled = True
        Else
            Search_Button.Enabled = False
        End If
        If cbSearch.Text = Master.eLang.GetString(5, "Source Folder") Then
            'cbFilterSource.Visible = True
            btnSource.Visible = True
            txtSearch.ReadOnly = True
        Else
            'cbFilterSource.Visible = False
            btnSource.Visible = False
            txtSearch.ReadOnly = False
        End If
    End Sub

    Private Sub cbTemplate_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTemplate.SelectedIndexChanged
        base_template = cbTemplate.Text
        DontSaveExtra = False
        Dim sFilter As String = String.Empty
        If cbSearch.Text = Master.eLang.GetString(5, "Source Folder") Then
            For Each s As String In lstSources.CheckedItems
                sFilter = String.Concat(sFilter, If(sFilter = String.Empty, String.Empty, ";"), s.ToString)
            Next
        Else
            sFilter = txtSearch.Text
        End If

        BuildHTML(use_filter, sFilter, cbSearch.Text, base_template, True)


    End Sub

    Private Sub Close_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Close_Button.Click
        If bwSaveAll.IsBusy Then
            bwSaveAll.CancelAsync()
        End If
        While bwSaveAll.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While
    End Sub

    Private Sub dlgExportMovies_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.bwLoadInfo.IsBusy Then
            Me.DoCancel()
            While Me.bwLoadInfo.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End If
        FileUtils.Delete.DeleteDirectory(Me.TempPath)
    End Sub

    Private Sub dlgExportMovies_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
        Dim di As DirectoryInfo = New DirectoryInfo(String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html"))
        If di.Exists Then
            For Each i As DirectoryInfo In di.GetDirectories
                If Not (i.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then
                    cbTemplate.Items.Add(i.Name)
                End If
            Next
            If cbTemplate.Items.Count > 0 Then
                RemoveHandler cbTemplate.SelectedIndexChanged, AddressOf cbTemplate_SelectedIndexChanged
                cbTemplate.SelectedIndex = 0
                base_template = cbTemplate.Text
                AddHandler cbTemplate.SelectedIndexChanged, AddressOf cbTemplate_SelectedIndexChanged
            End If
        End If
    End Sub

    Private Sub dlgMoviesReport_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        ' Show Cancel Panel
        btnCancel.Visible = True
        lblCompiling.Visible = True
        pbCompile.Visible = True
        pbCompile.Style = ProgressBarStyle.Continuous
        lblCanceling.Visible = False
        pnlCancel.Visible = True
        Application.DoEvents()

        Me.Activate()

        'Start worker
        Me.bwLoadInfo = New System.ComponentModel.BackgroundWorker
        Me.bwLoadInfo.WorkerSupportsCancellation = True
        Me.bwLoadInfo.WorkerReportsProgress = True
        Me.bwLoadInfo.RunWorkerAsync()
    End Sub

    Private Sub DoCancel()
        Me.bwLoadInfo.CancelAsync()
        btnCancel.Visible = False
        lblCompiling.Visible = False
        pbCompile.Style = ProgressBarStyle.Marquee
        pbCompile.MarqueeAnimationSpeed = 25
        lblCanceling.Visible = True
        lblFile.Visible = False
    End Sub

    Private Sub ExportFanart(ByVal fpath As String)
        Try
            Dim counter As Integer = 1
            Dim finalpath As String = Path.Combine(fpath, "export")
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As Structures.DBMovie In _movies.Where(Function(y) FilterMovies.Contains(y.ID))
                Try
                    Dim fanartfile As String = Path.Combine(finalpath, String.Concat(counter.ToString, "-fanart.jpg"))
                    If File.Exists(_curMovie.FanartPath) Then

                        File.Copy(_curMovie.FanartPath, fanartfile, True)
                    End If
                    counter += 1
                Catch
                End Try

                If bwSaveAll.CancellationPending Then
                    Return
                End If
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ExportPoster(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim counter As Integer = 1
            Dim finalpath As String = Path.Combine(fpath, "export")
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As Structures.DBMovie In _movies.Where(Function(y) FilterMovies.Contains(y.ID))
                Try
                    Dim posterfile As String = Path.Combine(finalpath, String.Concat(counter.ToString, ".jpg"))
                    If File.Exists(_curMovie.PosterPath) Then
                        If new_width > 0 Then
                            Dim im As New Images
                            im.FromFile(_curMovie.PosterPath)
                            ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                            im.Save(posterfile)
                        Else
                            File.Copy(_curMovie.PosterPath, posterfile, True)
                        End If
                    End If

                Catch
                End Try
                counter += 1
                If bwSaveAll.CancellationPending Then
                    Return
                End If

            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function GetAVImages(ByVal AVMovie As Structures.DBMovie, ByVal line As String) As String
        If APIXML.lFlags.Count > 0 Then
            Try
                Dim fiAV As MediaInfo.Fileinfo = AVMovie.Movie.FileInfo
                Dim tVideo As MediaInfo.Video = NFO.GetBestVideo(fiAV)
                Dim tAudio As MediaInfo.Audio = NFO.GetBestAudio(fiAV, False)

                Dim vresFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = NFO.GetResFromDimensions(tVideo).ToLower AndAlso f.Type = APIXML.FlagType.VideoResolution)
                If Not IsNothing(vresFlag) Then
                    line = line.Replace("<$FLAG_VRES>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                Else
                    vresFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoResolution)
                    If Not IsNothing(vresFlag) Then
                        line = line.Replace("<$FLAG_VRES>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                    End If
                End If

                'Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = APIXML.GetFileSource(AVMovie.Filename) AndAlso f.Type = APIXML.FlagType.VideoSource)
                Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name.ToLower = AVMovie.FileSource AndAlso f.Type = APIXML.FlagType.VideoSource)
                If Not IsNothing(vsourceFlag) Then
                    line = line.Replace("<$FLAG_VSOURCE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                Else
                    vsourceFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoSource)
                    If Not IsNothing(vsourceFlag) Then
                        line = line.Replace("<$FLAG_VSOURCE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim vcodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tVideo.Codec.ToLower AndAlso f.Type = APIXML.FlagType.VideoCodec)
                If Not IsNothing(vcodecFlag) Then
                    line = line.Replace("<$FLAG_VTYPE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                Else
                    vcodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoCodec)
                    If Not IsNothing(vcodecFlag) Then
                        line = line.Replace("<$FLAG_VTYPE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim acodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Codec.ToLower AndAlso f.Type = APIXML.FlagType.AudioCodec)
                If Not IsNothing(acodecFlag) Then
                    line = line.Replace("<$FLAG_ATYPE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                Else
                    acodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioCodec)
                    If Not IsNothing(acodecFlag) Then
                        line = line.Replace("<$FLAG_ATYPE>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim achanFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Channels AndAlso f.Type = APIXML.FlagType.AudioChan)
                If Not IsNothing(achanFlag) Then
                    line = line.Replace("<$FLAG_ACHAN>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                Else
                    achanFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioChan)
                    If Not IsNothing(achanFlag) Then
                        line = line.Replace("<$FLAG_ACHAN>", String.Concat("Flags", Path.DirectorySeparatorChar, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                    End If
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If
        Return line
    End Function

    Private Sub LoadHTML()
        Warning(True, Master.eLang.GetString(6, "Loading. Please wait..."))
        Dim tmphtml As String = Path.Combine(Me.TempPath, String.Concat(Master.eSettings.Language, ".html"))
        wbMovieList.Navigate(tmphtml)
    End Sub

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

    Private Sub pnlSearch_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlSearch.Paint
    End Sub

    Private Sub Reset_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Reset_Button.Click
        pnlSearch.Enabled = False
        use_filter = False
        BuildHTML(use_filter, String.Empty, String.Empty, base_template, True)
    End Sub

    Private Sub SaveAll(ByVal sWarning As String, ByVal srcPath As String, ByVal destPath As String, Optional ByVal resizePoster As Integer = 200)
        wbMovieList.Visible = False
        If Not String.IsNullOrEmpty(sWarning) Then Warning(True, sWarning)
        cbSearch.Enabled = False
        cbTemplate.Enabled = False
        Search_Button.Enabled = False
        Reset_Button.Enabled = False
        Save_Button.Enabled = False
        Me.bwSaveAll = New System.ComponentModel.BackgroundWorker
        Me.bwSaveAll.WorkerReportsProgress = True
        Me.bwSaveAll.WorkerSupportsCancellation = True
        Me.bwSaveAll.RunWorkerAsync(New Arguments With {.srcPath = srcPath, .destPath = destPath, .resizePoster = resizePoster})
        While bwSaveAll.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        cbSearch.Enabled = True
        cbTemplate.Enabled = True
        'Search_Button.Enabled = True
        Reset_Button.Enabled = True
        Save_Button.Enabled = True
        If pnlCancel.Visible Then Warning(False)
        If Not workerCanceled Then
            wbMovieList.Visible = True
        End If
    End Sub

    Private Sub Save_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Save_Button.Click
        Dim saveHTML As New SaveFileDialog()
        Dim myStream As Stream
        saveHTML.Filter = "HTML files (*.htm)|*.htm"
        saveHTML.FilterIndex = 2
        saveHTML.RestoreDirectory = True

        If saveHTML.ShowDialog() = DialogResult.OK Then
            myStream = saveHTML.OpenFile()
            myStream.Close()
            If Not IsNothing(myStream) Then
                DontSaveExtra = False 'Force Full Save
                Dim srcPath As String = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html", Path.DirectorySeparatorChar, base_template, Path.DirectorySeparatorChar)
                Me.SaveAll(Master.eLang.GetString(7, "Saving all files. Please wait..."), srcPath, saveHTML.FileName)
            End If
        End If
    End Sub

    Private Sub Search_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Search_Button.Click
        pnlSearch.Enabled = False
        use_filter = True
        Dim sFilter As String = String.Empty
        If cbSearch.Text = Master.eLang.GetString(5, "Source Folder") Then
            For Each s As String In lstSources.CheckedItems
                sFilter = String.Concat(sFilter, If(sFilter = String.Empty, String.Empty, ";"), s.ToString)
            Next
        Else
            sFilter = txtSearch.Text
        End If
        BuildHTML(use_filter, sFilter, cbSearch.Text, base_template, True)
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(8, "Export Movies")
        Me.Save_Button.Text = Master.eLang.GetString(273, "Save", True)
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.Reset_Button.Text = Master.eLang.GetString(9, "Reset")
        Me.Label1.Text = Master.eLang.GetString(10, "Filter")
        Me.Search_Button.Text = Master.eLang.GetString(176, "Apply", True)
        Me.lblIn.Text = Master.eLang.GetString(11, "in")
        Me.lblCompiling.Text = Master.eLang.GetString(12, "Compiling Movie List...")
        Me.lblCanceling.Text = Master.eLang.GetString(13, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Label2.Text = Master.eLang.GetString(14, "Template")

        Me.cbSearch.Items.AddRange(New Object() {Master.eLang.GetString(21, "Title", True), Master.eLang.GetString(278, "Year", True), Master.eLang.GetString(2, "Video Flag"), Master.eLang.GetString(3, "Audio Flag"), Master.eLang.GetString(1, "Source Folder")})
        lstSources.Items.Clear()
        For Each s As Structures.MovieSource In Master.MovieSources
            lstSources.Items.Add(s.Name)
        Next

    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        If txtSearch.Text <> "" AndAlso cbSearch.Text <> "" Then
            Search_Button.Enabled = True
        Else
            Search_Button.Enabled = False
        End If
    End Sub

    Private Sub Warning(ByVal show As Boolean, Optional ByVal txt As String = "")
        Try
            btnCancel.Visible = True
            btnCancel.Enabled = True
            lblCompiling.Visible = True
            pbCompile.Visible = True
            pbCompile.Style = ProgressBarStyle.Marquee
            pbCompile.MarqueeAnimationSpeed = 25
            lblCanceling.Visible = False
            pnlCancel.Visible = show
            lblFile.Visible = False
            lblCompiling.Text = txt
            Application.DoEvents()
            pnlCancel.BringToFront()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles wbMovieList.DocumentCompleted
        If Not bCancelled Then
            'wbMovieList.Visible = True
            If Not cbTemplate.Text = String.Empty Then Me.Save_Button.Enabled = True
            pnlSearch.Enabled = True
            Reset_Button.Enabled = bFiltered
        End If
        Warning(False)
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Private Structure Arguments

#Region "Fields"

        Dim destPath As String
        Dim resizePoster As Integer
        Dim srcPath As String

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class