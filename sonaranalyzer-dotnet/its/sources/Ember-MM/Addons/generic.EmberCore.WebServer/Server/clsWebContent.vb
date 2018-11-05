Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports EmberAPI

Public Class ProccessPages
    Private Shared dtMovieMedia As DataTable = Nothing
    Public Shared ReloadDatabase As Boolean = False
    Sub New()
        Try
            If dtMovieMedia Is Nothing Then
                dtMovieMedia = New DataTable
                Master.DB.FillDataTable(dtMovieMedia, "SELECT * FROM movies ORDER BY ListTitle COLLATE NOCASE;")
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Function MovieBuildHTML(ByVal htmlPath As String, ByVal qscoll As NameValueCollection) As String
        If ReloadDatabase Then
            ReloadDatabase = False
            dtMovieMedia = New DataTable
            Master.DB.FillDataTable(dtMovieMedia, "SELECT * FROM movies ORDER BY ListTitle COLLATE NOCASE;")
        End If
        Dim HTMLBody As New StringBuilder
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim MovieTag As String = String.Empty
            Dim movieQuery As String = String.Empty
            Dim pattern As String
            Dim movieheader As String = String.Empty
            Dim moviefooter As String = String.Empty
            Dim movierow As String = String.Empty
            Dim numRows As Integer = 0
            Dim abscounter As Integer = 0
            HTMLBody.Length = 0

            If Not File.Exists(htmlPath) Then
                Return String.Empty
            End If
            pattern = File.ReadAllText(htmlPath)

            If Not qscoll Is Nothing AndAlso pattern.Contains("[$GET(") Then
                Dim i As Integer = pattern.IndexOf("[$GET(")
                While i >= 0
                    Dim f As Integer = pattern.IndexOf(")]", i)
                    If f >= 0 Then
                        Dim getString As String = pattern.Substring(i, f - i + 2)
                        pattern = pattern.Replace(getString, qscoll.Item(GetValueGET(getString)))
                    End If
                    i = pattern.IndexOf("[$GET(")
                End While
            End If

            Dim s As Integer = pattern.IndexOf("<$MOVIE>")
            If s >= 0 Then
                Dim e As Integer = pattern.IndexOf("<$/MOVIE>")
                If e >= 0 Then
                    movieheader = pattern.Substring(0, s)
                    movierow = pattern.Substring(s + 8, e - s - 8)
                    moviefooter = pattern.Substring(e + 9, pattern.Length - e - 9)
                Else
                    'error
                End If
            Else
                Return pattern
            End If
            s = movierow.IndexOf("<$MQUERYID")
            If s >= 0 Then
                Dim e As Integer = movierow.Substring(s).IndexOf(">")
                If e >= 0 Then
                    Dim getString As String = movierow.Substring(s, e - s + 2)
                    movieQuery = GetValueQueryID(getString)
                    movierow = movierow.Replace(getString, String.Empty)
                End If
            End If
            s = movierow.IndexOf("<$MQUERYORDERID")
            If s >= 0 Then
                Dim e As Integer = movierow.Substring(s).IndexOf(">")
                If e >= 0 Then
                    Dim getString As String = movierow.Substring(s, e - s + 2)
                    'movieQuery = _movies(Convert.ToInt32(GetValueQueryORDERID(getString))).ID.ToString
                    If Convert.ToInt32(GetValueQueryORDERID(getString)) <= dtMovieMedia.Rows.Count Then
                        movieQuery = dtMovieMedia.Rows(Convert.ToInt32(GetValueQueryORDERID(getString)) - 1).Item("ID").ToString
                        abscounter = Convert.ToInt32(GetValueQueryORDERID(getString))
                    End If
                    movierow = movierow.Replace(getString, String.Empty)
                End If
            End If
            s = movierow.IndexOf("<$MNUMROWS")
            If s >= 0 Then
                Dim e As Integer = movierow.Substring(s).IndexOf(">")
                If e >= 0 Then
                    Dim getString As String = movierow.Substring(s, e - s + 2)
                    numRows = Convert.ToInt32(GetValueQueryROWS(getString))
                    movierow = movierow.Replace(getString, String.Empty)
                End If
            End If

            HTMLBody.Append(movieheader)
            Dim counter As Integer = 1

            For Each _curMovie As DataRow In dtMovieMedia.Select(If(String.IsNullOrEmpty(movieQuery), "", String.Concat("ID =", movieQuery)))
                'If Not String.IsNullOrEmpty(movieQuery) AndAlso Not _curMovie.Item("ID").ToString = movieQuery Then Continue For
                If numRows > 0 AndAlso counter > numRows Then Exit For
                If counter = 1 Then
                    If pattern.Contains("<$MCOUNT>") Then
                        Dim count As Integer = dtMovieMedia.Select(If(String.IsNullOrEmpty(movieQuery), "", String.Concat("ID =", movieQuery))).Count
                        movieheader = movieheader.Replace("<$MCOUNT>", count.ToString)
                        movierow = movierow.Replace("<$MCOUNT>", count.ToString)
                        moviefooter = moviefooter.Replace("<$MCOUNT>", count.ToString)
                    End If
                    If pattern.Contains("<$MFIRSTID>") Then
                        movieheader = movieheader.Replace("<$MFIRSTID>", _curMovie.Item("ID").ToString)
                        movierow = movierow.Replace("<$MFIRSTID>", _curMovie.Item("ID").ToString)
                        moviefooter = moviefooter.Replace("<$MFIRSTID>", _curMovie.Item("ID").ToString)
                    End If
                End If
                'now check if we need to include this movie
                Dim uni As New UnicodeEncoding()
                Dim row As String = movierow

                Dim regStat As MatchCollection = Regex.Matches(movierow, "\<\$(?<status>.*?)\>", RegexOptions.Multiline)
                If regStat.Count > 0 Then
                    Dim mSourceName As String = _curMovie.Item("Source").ToString
                    For Each status As Match In regStat
                        Select Case status.Value
                            Case "<$MID>"
                                row = row.Replace("<$MID>", _curMovie.Item("ID").ToString)
                            Case "<$MORDERID>"
                                row = row.Replace("<$MORDERID>", If(abscounter > 0, abscounter.ToString, counter.ToString))
                            Case "<$MOVIE_FILE>"
                                Dim mSourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).Path
                                Dim mSourceID As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).id
                                row = row.Replace("<$MOVIE_FILE>", String.Concat("<$MSOURCE>", "/", mSourceID, Web.HttpUtility.UrlEncode(_curMovie.Item("Filename").ToString.Substring(mSourcePath.Length).Replace(Path.DirectorySeparatorChar, "/"))))
                            Case "<$MPOSTER_FILE>"
                                Dim mSourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).Path
                                Dim mSourceID As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).id
                                If Not String.IsNullOrEmpty(_curMovie.Item("PosterPath").ToString) Then row = row.Replace("<$MPOSTER_FILE>", String.Concat("<$MSOURCE>", "/", mSourceID, Web.HttpUtility.UrlEncode(_curMovie.Item("PosterPath").ToString.Substring(mSourcePath.Length).Replace(Path.DirectorySeparatorChar, "/"))))
                            Case "<$MFANART_FILE>"
                                Dim mSourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).Path
                                Dim mSourceID As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = mSourceName).id
                                If Not String.IsNullOrEmpty(_curMovie.Item("FanartPath").ToString) Then row = row.Replace("<$MFANART_FILE>", String.Concat("<$MSOURCE>", "/", mSourceID, Web.HttpUtility.UrlEncode(_curMovie.Item("FanartPath").ToString.Substring(mSourcePath.Length).Replace(Path.DirectorySeparatorChar, "/"))))
                            Case "<$MOVIENAME>"
                                If Not String.IsNullOrEmpty(_curMovie.Item("Title").ToString) Then
                                    row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.Item("Title").ToString))
                                Else
                                    row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.Item("ListTitle").ToString))
                                End If
                            Case "<$MORIGINALTITLE>"
                                row = row.Replace("<$MORIGINALTITLE>", StringUtils.HtmlEncode(_curMovie.Item("OriginalTitle").ToString))
                            Case "<$MACTORS>"
                                row = row.Replace("<$MACTORS>", StringUtils.HtmlEncode(GetActorForID(_curMovie.Item("ID").ToString)))
                            Case "<$MDIRECTOR>"
                                row = row.Replace("<$MDIRECTOR>", StringUtils.HtmlEncode(_curMovie.Item("Director").ToString))
                            Case "<$MCERTIFICATION>"
                                row = row.Replace("<$MCERTIFICATION>", StringUtils.HtmlEncode(_curMovie.Item("Certification").ToString))
                            Case "<$MIMDBID>"
                                row = row.Replace("<$MIMDBID>", StringUtils.HtmlEncode(_curMovie.Item("IMDB").ToString))
                            Case "<$MMPAA>"
                                row = row.Replace("<$MMPAA>", StringUtils.HtmlEncode(_curMovie.Item("MPAA").ToString))
                            Case "<$MRELEASEDATE>"
                                row = row.Replace("<$MRELEASEDATE>", StringUtils.HtmlEncode(_curMovie.Item("ReleaseDate").ToString))
                            Case "<$MRUNTIME>"
                                row = row.Replace("<$MRUNTIME>", StringUtils.HtmlEncode(_curMovie.Item("Runtime").ToString))
                            Case "<$MTAGLINE>"
                                row = row.Replace("<$MTAGLINE>", StringUtils.HtmlEncode(_curMovie.Item("Tagline").ToString))
                            Case "<$MRATING>"
                                row = row.Replace("<$MRATING>", StringUtils.HtmlEncode(_curMovie.Item("Rating").ToString))
                            Case "<$MVOTES>"
                                row = row.Replace("<$MVOTES>", StringUtils.HtmlEncode(_curMovie.Item("Votes").ToString))
                            Case "<$MLISTTITLE>"
                                row = row.Replace("<$MLISTTITLE>", StringUtils.HtmlEncode(_curMovie.Item("ListTitle").ToString))
                            Case "<$MYEAR>"
                                row = row.Replace("<$MYEAR>", _curMovie.Item("Year").ToString)
                            Case "<$MCOUNTRY>"
                                row = row.Replace("<$MCOUNTRY>", StringUtils.HtmlEncode(_curMovie.Item("Country").ToString))
                            Case "<$MROWID>"
                                row = row.Replace("<$MROWID>", counter.ToString)
                            Case "<$MFILENAME>"
                                row = row.Replace("<$MFILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curMovie.Item("MoviePath").ToString)))
                            Case "<$MDIRNAME>"
                                row = row.Replace("<$MDIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curMovie.Item("MoviePath").ToString)))
                            Case "<$MOUTLINE>"
                                row = row.Replace("<$MOUTLINE>", StringUtils.HtmlEncode(_curMovie.Item("Outline").ToString))
                            Case "<$MPLOT>"
                                row = row.Replace("<$MPLOT>", StringUtils.HtmlEncode(_curMovie.Item("Plot").ToString))
                            Case "<$MGENRES>"
                                row = row.Replace("<$MGENRES>", StringUtils.HtmlEncode(_curMovie.Item("Genre").ToString))
                            Case "<$MSIZE>"
                                row = row.Replace("<$MSIZE>", StringUtils.HtmlEncode(MovieSize(_curMovie.Item("Filename").ToString).ToString))
                            Case "<$MDATEADD>"
                                row = row.Replace("<$MDATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curMovie.Item("DateAdd").ToString)).ToShortDateString))
                            Case "<$MVIDEO>", "<$MVIDEO_DIMENSIONS>", "<$MAUDIO>", "<$MFLAG_ACHAN>", "<$MFLAG_ATYPE>", "<$MFLAG_VTYPE>", "<$MFLAG_VSOURCE>", "<$MFLAG_VRES>"
                                Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curMovie.Item("ID").ToString)
                                Select Case status.Value
                                    Case "<$MVIDEO>", "<$MVIDEO_DIMENSIONS>"
                                        Dim _vidDetails As String = String.Empty
                                        Dim _vidDimensions As String = String.Empty
                                        If Not IsNothing(fiAV) Then
                                            If fiAV.StreamDetails.Video.Count > 0 Then
                                                tVid = NFO.GetBestVideo(fiAV)
                                                tRes = NFO.GetResFromDimensions(tVid)
                                                _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                                                _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                                            End If
                                        End If
                                        row = row.Replace("<$MVIDEO>", _vidDetails)
                                        row = row.Replace("<$MVIDEO_DIMENSIONS>", _vidDimensions)
                                    Case "<$MAUDIO>"
                                        Dim _audDetails As String = String.Empty
                                        If fiAV.StreamDetails.Audio.Count > 0 Then
                                            tAud = NFO.GetBestAudio(fiAV, False)
                                            _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
                                        End If
                                        row = row.Replace("<$MAUDIO>", _audDetails)
                                    Case "<$MFLAG_ACHAN>", "<$MFLAG_ATYPE>", "<$MFLAG_VTYPE>", "<$MFLAG_VSOURCE>", "<$MFLAG_VRES>"
                                        row = GetAVImages(fiAV, row, _curMovie.Item("MoviePath").ToString)
                                End Select

                        End Select

                    Next
                End If
                'row = row.Replace("[$GET(", "[$_GET(")
                HTMLBody.Append(row)
                counter += 1
            Next
            HTMLBody.Append(moviefooter)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return HTMLBody.ToString
    End Function
    Public Function GetSourceRealPath(ByVal s As String) As String
        Try
            Dim regStat As Match = Regex.Match(s, "\<\$MSOURCE\>\/(?<status>.*?)\/", RegexOptions.IgnoreCase)
            If regStat.Success Then
                Dim tStatus As String = regStat.Groups("status").Value
                If Not String.IsNullOrEmpty(tStatus) Then
                    Return String.Concat(Master.MovieSources.FirstOrDefault(Function(y) y.id = tStatus).Path, Path.DirectorySeparatorChar, s.Substring(s.IndexOf("<$MSOURCE>") + regStat.Value.Length))
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function
    Public Function GetActorForID(ByVal id As String) As String
        Dim dtActorMedia = New DataTable
        Dim actors As String = String.Empty
        Master.DB.FillDataTable(dtActorMedia, String.Format("SELECT ActorName FROM MoviesActors WHERE MovieID = {0}", id))
        For Each s As DataRow In dtActorMedia.Rows
            actors = String.Concat(actors, If(Not String.IsNullOrEmpty(actors), " / ", String.Empty), s.Item("ActorNAme").ToString)
        Next
        Return actors
    End Function
    Public Function GetValueGET(ByVal s As String) As String
        Try
            Dim regStat As Match = Regex.Match(s, "\[\$GET\((?<status>.*?)\)\]", RegexOptions.IgnoreCase)
            If regStat.Success Then
                Dim tStatus As String = regStat.Groups("status").Value
                If Not String.IsNullOrEmpty(tStatus) Then
                    Return tStatus
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function
    Public Function GetValueQueryID(ByVal s As String) As String
        Try
            Dim regStat As Match = Regex.Match(s, "\<\$MQUERYID\=\""(?<status>.*?)\""\>", RegexOptions.IgnoreCase)
            If regStat.Success Then
                Dim tStatus As String = regStat.Groups("status").Value
                If Not String.IsNullOrEmpty(tStatus) Then
                    Return tStatus
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function
    Public Function GetValueQueryORDERID(ByVal s As String) As String
        Try
            Dim regStat As Match = Regex.Match(s, "\<\$MQUERYORDERID\=\""(?<status>.*?)\""\>", RegexOptions.IgnoreCase)
            If regStat.Success Then
                Dim tStatus As String = regStat.Groups("status").Value
                If Not String.IsNullOrEmpty(tStatus) Then
                    Return tStatus
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function
    Public Function GetValueQueryROWS(ByVal s As String) As String
        Try
            Dim regStat As Match = Regex.Match(s, "\<\$MNUMROWS\=\""(?<status>.*?)\""\>", RegexOptions.IgnoreCase)
            If regStat.Success Then
                Dim tStatus As String = regStat.Groups("status").Value
                If Not String.IsNullOrEmpty(tStatus) Then
                    Return tStatus
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return String.Empty
    End Function
    Private Function GetAVImages(ByVal fiAV As MediaInfo.Fileinfo, ByVal line As String, ByVal filename As String) As String

        Try
            'Dim fiAV As MediaInfo.Fileinfo = AVMovie.Movie.FileInfo
            Dim tVideo As MediaInfo.Video = NFO.GetBestVideo(fiAV)
            Dim tAudio As MediaInfo.Audio = NFO.GetBestAudio(fiAV, False)
            Dim vresFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = NFO.GetResFromDimensions(tVideo).ToLower AndAlso f.Type = APIXML.FlagType.VideoResolution)
            If Not IsNothing(vresFlag) Then
                line = line.Replace("<$MFLAG_VRES>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vresFlag.Path).Replace("\", "/")))
            Else
                vresFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoResolution)
                If Not IsNothing(vresFlag) Then
                    line = line.Replace("<$MFLAG_VRES>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vresFlag.Path).Replace("\", "/")))
                Else
                    line = line.Replace("<$MFLAG_VRES>", String.Empty)
                End If
            End If

            Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = APIXML.GetFileSource(filename) AndAlso f.Type = APIXML.FlagType.VideoSource)
            If Not IsNothing(vsourceFlag) Then
                line = line.Replace("<$MFLAG_VSOURCE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vsourceFlag.Path).Replace("\", "/")))
            Else
                vsourceFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoSource)
                If Not IsNothing(vsourceFlag) Then
                    line = line.Replace("<$MFLAG_VSOURCE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vsourceFlag.Path).Replace("\", "/")))
                Else
                    line = line.Replace("<$MFLAG_VSOURCE>", String.Empty)
                End If
            End If

            Dim vcodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tVideo.Codec.ToLower AndAlso f.Type = APIXML.FlagType.VideoCodec)
            If Not IsNothing(vcodecFlag) Then
                line = line.Replace("<$MFLAG_VTYPE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vcodecFlag.Path).Replace("\", "/")))
            Else
                vcodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoCodec)
                If Not IsNothing(vcodecFlag) Then
                    line = line.Replace("<$MFLAG_VTYPE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(vcodecFlag.Path).Replace("\", "/")))
                Else
                    line = line.Replace("<$MFLAG_VTYPE>", String.Empty)
                End If
            End If

            Dim acodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Codec.ToLower AndAlso f.Type = APIXML.FlagType.AudioCodec)
            If Not IsNothing(acodecFlag) Then
                line = line.Replace("<$MFLAG_ATYPE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(acodecFlag.Path).Replace("\", "/")))
            Else
                acodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultsound" AndAlso f.Type = APIXML.FlagType.AudioCodec)
                If Not IsNothing(acodecFlag) Then
                    line = line.Replace("<$MFLAG_ATYPE>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(acodecFlag.Path).Replace("\", "/")))
                Else
                    line = line.Replace("<$MFLAG_ATYPE>", String.Empty)
                End If
            End If

            Dim achanFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Channels AndAlso f.Type = APIXML.FlagType.AudioChan)
            If Not IsNothing(achanFlag) Then
                line = line.Replace("<$MFLAG_ACHAN>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(achanFlag.Path).Replace("\", "/")))
            Else
                achanFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultsound" AndAlso f.Type = APIXML.FlagType.AudioChan)
                If Not IsNothing(achanFlag) Then
                    line = line.Replace("<$MFLAG_ACHAN>", String.Concat("<$ESOURCE>Images/Flags/", Path.GetFileName(achanFlag.Path).Replace("\", "/")))
                Else
                    line = line.Replace("<$MFLAG_ACHAN>", String.Empty)
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

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


End Class
