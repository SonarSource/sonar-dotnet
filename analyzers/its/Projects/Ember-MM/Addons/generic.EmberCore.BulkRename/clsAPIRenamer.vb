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
Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class FileFolderRenamer

#Region "Fields"

    Public MovieFolders As New List(Of String)

    Private _movies As New List(Of FileRename)

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Dim mePath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags")

        _movies.Clear()
        Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLNewcommand.CommandText = String.Concat("SELECT Path FROM Sources;")
            Using SQLReader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                While SQLReader.Read
                    MovieFolders.Add(If(SQLReader("Path").ToString.EndsWith(Path.DirectorySeparatorChar), SQLReader("Path").ToString, String.Concat(SQLReader("Path").ToString, Path.DirectorySeparatorChar)))
                End While
            End Using
        End Using

        'put them in order so when we're checking for basepath the last one used will be the longest one
        'case:
        'Source 1 = C:/Movies/BluRay/FullRips
        'Source 2 = C:/Movies/BluRay
        'stupid to add sources this way, but possible
        MovieFolders.Sort()
    End Sub

#End Region 'Constructors

#Region "Delegates"

    Public Delegate Function ShowProgress(ByVal movie As String, ByVal iProgress As Integer) As Boolean

#End Region 'Delegates

#Region "Methods"

    Public Shared Function HaveBase(ByVal fpath As String) As Boolean
        If fpath.Contains("$B") Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function ProccessPattern(ByVal f As FileRename, ByVal opattern As String) As String
        Try
            Dim pattern As String = opattern
            Dim strSource As String = f.FileSource  ' APIXML.GetFileSource(Path.Combine(f.Path.ToLower, f.FileName.ToLower))

            'pattern = "$T{($S.$S)}"
            Dim joinIndex As Integer
            Dim nextC = pattern.IndexOf("$")
            Dim nextIB = pattern.IndexOf("{")
            Dim nextEB = pattern.IndexOf("}")
            Dim strCond As String
            Dim strBase As String
            Dim strNoFlags As String
            Dim strJoin As String
            While Not nextC = -1
                If nextC > nextIB AndAlso nextC < nextEB AndAlso Not nextC = -1 AndAlso Not nextIB = -1 AndAlso Not nextEB = -1 Then
                    strCond = pattern.Substring(nextIB, nextEB - nextIB + 1)
                    strNoFlags = strCond
                    strBase = strCond
                    strCond = ApplyPattern(strCond, "1", f.SortTitle.Substring(0, 1))
                    strCond = ApplyPattern(strCond, "A", f.Audio)
                    strCond = ApplyPattern(strCond, "B", String.Empty) 'This is not need here, Only to HaveBase
                    strCond = ApplyPattern(strCond, "C", f.Director)
                    strCond = ApplyPattern(strCond, "D", f.Parent) '.Replace("\", String.Empty))
                    strCond = ApplyPattern(strCond, "E", f.SortTitle)
                    strCond = ApplyPattern(strCond, "F", f.FileName.Replace("\", String.Empty))
                    '                                G   Genres
                    strCond = ApplyPattern(strCond, "I", If(Not String.IsNullOrEmpty(f.IMDBID), String.Concat("tt", f.IMDBID), String.Empty))
                    strCond = ApplyPattern(strCond, "L", f.ListTitle)
                    strCond = ApplyPattern(strCond, "M", f.MPAARate)
                    strCond = ApplyPattern(strCond, "O", f.OriginalTitle)
                    strCond = ApplyPattern(strCond, "R", f.Resolution)
                    strCond = ApplyPattern(strCond, "S", strSource)
                    strCond = ApplyPattern(strCond, "T", f.Title)
                    strCond = ApplyPattern(strCond, "Y", f.Year)
                    joinIndex = strCond.IndexOf("$G")
                    If Not joinIndex = -1 Then
                        If strCond.Length > joinIndex + 2 Then
                            strJoin = strCond.Substring(joinIndex + 2, 1)
                            If Not ". -".IndexOf(strJoin) = -1 Then
                                strCond = ApplyPattern(strCond, String.Concat("G", strJoin), f.Genre.Replace(" / ", strJoin))
                            Else
                                strCond = ApplyPattern(strCond, "G", f.Genre.Replace(" / ", " "))
                            End If
                        Else
                            strCond = ApplyPattern(strCond, "G", f.Genre.Replace(" / ", " "))
                        End If
                    End If
                    joinIndex = strCond.IndexOf("$U")
                    If Not joinIndex = -1 Then
                        If strCond.Length > joinIndex + 2 Then
                            strJoin = strCond.Substring(joinIndex + 2, 1)
                            If Not ". -".IndexOf(strJoin) = -1 Then
                                strCond = ApplyPattern(strCond, String.Concat("U", strJoin), f.Country.Replace(" / ", strJoin))
                            Else
                                strCond = ApplyPattern(strCond, "U", f.Country.Replace(" / ", " "))
                            End If
                        Else
                            strCond = ApplyPattern(strCond, "U", f.Country.Replace(" / ", " "))
                        End If
                    End If
                    strNoFlags = Regex.Replace(strNoFlags, "\$((?:[1ABCDEFILMORSTY]|G[. -]|U[. -]?))", String.Empty) '"(?i)\$([DFTYRAS])"  "\$((?i:[DFTYRAS]))"
                    If strCond.Trim = strNoFlags.Trim Then
                        strCond = String.Empty
                    Else
                        strCond = strCond.Substring(1, strCond.Length - 2)
                    End If
                    pattern = pattern.Replace(strBase, strCond)
                    nextC = pattern.IndexOf("$")
                Else
                    nextC = pattern.IndexOf("$", nextC + 1)
                End If
                nextIB = pattern.IndexOf("{")
                nextEB = pattern.IndexOf("}")
            End While
            pattern = ApplyPattern(pattern, "1", f.SortTitle.Substring(0, 1))
            pattern = ApplyPattern(pattern, "A", f.Audio)
            pattern = ApplyPattern(pattern, "B", String.Empty) 'This is not need here, Only to HaveBase
            pattern = ApplyPattern(pattern, "C", f.Director)
            pattern = ApplyPattern(pattern, "D", f.Parent) '.Replace("\", String.Empty))
            pattern = ApplyPattern(pattern, "E", f.SortTitle)
            pattern = ApplyPattern(pattern, "F", f.FileName.Replace("\", String.Empty))
            '                                G   Genres
            pattern = ApplyPattern(pattern, "I", If(Not String.IsNullOrEmpty(f.IMDBID), String.Concat("tt", f.IMDBID), String.Empty))
            pattern = ApplyPattern(pattern, "L", f.ListTitle)
            pattern = ApplyPattern(pattern, "M", f.MPAARate)
            pattern = ApplyPattern(pattern, "O", f.OriginalTitle)
            pattern = ApplyPattern(pattern, "R", f.Resolution)
            pattern = ApplyPattern(pattern, "S", strSource)
            pattern = ApplyPattern(pattern, "T", f.Title)
            pattern = ApplyPattern(pattern, "Y", f.Year)
            nextC = pattern.IndexOf("$G")
            If Not nextC = -1 Then
                If pattern.Length > nextC + 2 Then
                    strCond = pattern.Substring(nextC + 2, 1)
                    If Not ". -".IndexOf(strCond) = -1 Then
                        pattern = ApplyPattern(pattern, String.Concat("G", strCond), f.Genre.Replace(" / ", strCond))
                    Else
                        pattern = ApplyPattern(pattern, "G", f.Genre.Replace(" / ", " "))
                    End If
                Else
                    pattern = ApplyPattern(pattern, "G", f.Genre.Replace(" / ", " "))
                End If
            End If
            nextC = pattern.IndexOf("$U")
            If Not nextC = -1 Then
                If pattern.Length > nextC + 2 Then
                    strCond = pattern.Substring(nextC + 2, 1)
                    If Not ". -".IndexOf(strCond) = -1 Then
                        pattern = ApplyPattern(pattern, String.Concat("U", strCond), f.Country.Replace(" / ", strCond))
                    Else
                        pattern = ApplyPattern(pattern, "U", f.Country.Replace(" / ", " "))
                    End If
                Else
                    pattern = ApplyPattern(pattern, "U", f.Country.Replace(" / ", " "))
                End If
            End If
            nextC = pattern.IndexOf("$X")
            If Not nextC = -1 AndAlso pattern.Length > nextC + 2 Then
                strCond = pattern.Substring(nextC + 2, 1)
                pattern = pattern.Replace(String.Concat("$X", strCond), "")
                pattern = pattern.Replace(" ", strCond)
            End If
            nextC = pattern.IndexOf("$?")
            Dim strmore As String = String.Empty
            While nextC > -1
                'If nextC > -1 Then
                strBase = pattern.Substring(nextC + 2)
                pattern = pattern.Substring(0, nextC)
                If Not strBase = String.Empty Then
                    nextIB = strBase.IndexOf("?")
                    If nextIB > -1 Then
                        nextEB = strBase.Substring(nextIB + 1).IndexOf("?")
                        If nextEB > -1 Then
                            strCond = strBase.Substring(nextIB + 1, nextEB)
                            strmore = strBase.Substring(nextIB + nextEB + 2)
                            strBase = strBase.Substring(0, nextIB)
                            If Not strBase = String.Empty Then pattern = pattern.Replace(strBase, strCond)
                        End If
                    End If
                End If
                'End If
                pattern = String.Concat(pattern, strmore)
                nextC = pattern.IndexOf("$?")
            End While
            pattern = pattern.Replace(":", " -")
            pattern = pattern.Replace("/", String.Empty)
            'pattern = pattern.Replace("\", String.Empty)
            pattern = pattern.Replace("|", String.Empty)
            pattern = pattern.Replace("<", String.Empty)
            pattern = pattern.Replace(">", String.Empty)
            pattern = pattern.Replace("?", String.Empty)
            pattern = pattern.Replace("*", String.Empty)
            pattern = pattern.Replace("  ", " ")

            For Each Invalid As Char In Path.GetInvalidPathChars
                pattern = pattern.Replace(Invalid, String.Empty)
            Next
            Return pattern.Trim
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return String.Empty
        End Try
    End Function

    Public Shared Sub RenameSingle(ByRef _tmpMovie As Structures.DBMovie, ByVal folderPattern As String, ByVal filePattern As String, ByVal BatchMode As Boolean, ByVal toNfo As Boolean, ByVal ShowError As Boolean)
        Dim MovieFile As New FileRename

        If Not IsNothing(_tmpMovie.Movie.FileInfo) Then
            Try
                If _tmpMovie.Movie.FileInfo.StreamDetails.Video.Count > 0 Then
                    Dim tVid As MediaInfo.Video = NFO.GetBestVideo(_tmpMovie.Movie.FileInfo)
                    Dim tRes As String = NFO.GetResFromDimensions(tVid)
                    MovieFile.Resolution = String.Format("{0}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes))
                Else
                    MovieFile.Resolution = String.Empty
                End If

                If _tmpMovie.Movie.FileInfo.StreamDetails.Audio.Count > 0 Then
                    Dim tAud As MediaInfo.Audio = NFO.GetBestAudio(_tmpMovie.Movie.FileInfo, False)
                    MovieFile.Audio = String.Format("{0}-{1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels))
                Else
                    MovieFile.Audio = String.Empty
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error FileInfo")
            End Try
        Else
            MovieFile.Resolution = String.Empty
            MovieFile.Audio = String.Empty
        End If

        MovieFile.Title = _tmpMovie.Movie.Title
        MovieFile.ListTitle = _tmpMovie.ListTitle
        MovieFile.OriginalTitle = _tmpMovie.Movie.OriginalTitle
        MovieFile.Year = _tmpMovie.Movie.Year
        MovieFile.IsSingle = _tmpMovie.isSingle
        MovieFile.SortTitle = _tmpMovie.Movie.SortTitle
        MovieFile.Genre = _tmpMovie.Movie.Genre
        MovieFile.Director = _tmpMovie.Movie.Director
        MovieFile.FileSource = _tmpMovie.FileSource
        MovieFile.Country = _tmpMovie.Movie.Country
        Dim mFolders As New List(Of String)
        Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLNewcommand.CommandText = String.Concat("SELECT Path FROM Sources;")
            Using SQLReader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                While SQLReader.Read
                    mFolders.Add(SQLReader("Path").ToString)
                End While
            End Using
        End Using
        mFolders.Sort()

        Dim tPath As String = String.Empty
        For Each i As String In mFolders
            If _tmpMovie.Filename.StartsWith(i, StringComparison.OrdinalIgnoreCase) Then
                MovieFile.BasePath = i
                If FileUtils.Common.isVideoTS(_tmpMovie.Filename) Then
                    MovieFile.Parent = Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).Name
                    If MovieFile.BasePath = Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName Then
                        MovieFile.OldPath = String.Empty
                        MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                    Else
                        MovieFile.OldPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName).FullName.Replace(i, String.Empty)
                    End If
                    MovieFile.IsVideo_TS = True
                ElseIf FileUtils.Common.isBDRip(_tmpMovie.Filename) Then
                    MovieFile.Parent = Directory.GetParent(Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName).Name
                    If MovieFile.BasePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName).FullName Then
                        MovieFile.OldPath = String.Empty
                        MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                    Else
                        MovieFile.OldPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName).FullName).FullName.Replace(i, String.Empty)
                    End If
                    MovieFile.IsBDMV = True
                Else
                    MovieFile.Parent = Directory.GetParent(_tmpMovie.Filename).Name
                    If MovieFile.BasePath = Directory.GetParent(_tmpMovie.Filename).FullName Then
                        MovieFile.OldPath = String.Empty
                        MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                    Else
                        MovieFile.OldPath = Directory.GetParent(Directory.GetParent(_tmpMovie.Filename).FullName).FullName.Replace(i, String.Empty)
                    End If
                End If
            End If
        Next

        MovieFile.Path = Path.Combine(MovieFile.OldPath, MovieFile.Parent)
        MovieFile.Path = If(MovieFile.Path.StartsWith(Path.DirectorySeparatorChar), MovieFile.Path.Substring(1), MovieFile.Path)

        If Not MovieFile.IsVideo_TS AndAlso Not MovieFile.IsBDMV Then
            MovieFile.FileName = StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(_tmpMovie.Filename))
            Dim stackMark As String = Path.GetFileNameWithoutExtension(_tmpMovie.Filename).Replace(MovieFile.FileName, String.Empty).ToLower
            If Not stackMark = String.Empty AndAlso _tmpMovie.Movie.Title.ToLower.EndsWith(stackMark) Then
                MovieFile.FileName = Path.GetFileNameWithoutExtension(_tmpMovie.Filename)
            End If
            MovieFile.NewFileName = ProccessPattern(MovieFile, filePattern).Trim
        ElseIf MovieFile.IsBDMV Then
            MovieFile.FileName = String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM")
            MovieFile.NewFileName = MovieFile.FileName
        Else
            MovieFile.FileName = "VIDEO_TS"
            MovieFile.NewFileName = MovieFile.FileName
        End If

        If HaveBase(folderPattern) Then
            MovieFile.NewPath = ProccessPattern(MovieFile, If(_tmpMovie.isSingle, folderPattern, "$D")).Trim
        Else
            MovieFile.NewPath = Path.Combine(MovieFile.OldPath, ProccessPattern(MovieFile, If(_tmpMovie.isSingle, folderPattern, "$D")).Trim)
        End If
        MovieFile.NewPath = If(MovieFile.NewPath.StartsWith(Path.DirectorySeparatorChar), MovieFile.NewPath.Substring(1), MovieFile.NewPath)

        MovieFile.FileExist = File.Exists(Path.Combine(MovieFile.BasePath, Path.Combine(MovieFile.NewPath, MovieFile.NewFileName))) AndAlso Not (MovieFile.FileName = MovieFile.NewFileName)
        MovieFile.DirExist = File.Exists(Path.Combine(MovieFile.BasePath, MovieFile.NewPath)) AndAlso Not (MovieFile.Path = MovieFile.NewPath)

        If Not MovieFile.NewPath = MovieFile.Path OrElse Not MovieFile.NewFileName = MovieFile.FileName Then
            DoRenameSingle(MovieFile, _tmpMovie, BatchMode, toNfo, ShowError)
        Else
            Master.DB.SaveMovieToDB(_tmpMovie, False, True, Not String.IsNullOrEmpty(_tmpMovie.Movie.IMDBID))
        End If
    End Sub

    Public Shared Function SelectMPAA(ByVal _movies As MediaContainers.Movie) As String
        If Not String.IsNullOrEmpty(_movies.MPAA) Then
            Try
                Dim strMPAA As String = _movies.MPAA
                If strMPAA.ToLower.StartsWith("rated g") Then
                    Return "0"
                ElseIf strMPAA.ToLower.StartsWith("rated pg-13") Then
                    Return "13"
                ElseIf strMPAA.ToLower.StartsWith("rated pg") Then
                    Return "7"
                ElseIf strMPAA.ToLower.StartsWith("rated r") Then
                    Return "17"
                ElseIf strMPAA.ToLower.StartsWith("rated nc-17") Then
                    Return "17+"
                ElseIf strMPAA.Contains(":") Then 'might be a certification
                    Dim tReturn As String = strMPAA.Split(Convert.ToChar(":")).Last
                    'just in case
                    For Each fnC As Char In Path.GetInvalidFileNameChars
                        tReturn = tReturn.Replace(fnC, String.Empty)
                    Next
                    For Each fC As Char In Path.GetInvalidPathChars
                        tReturn = tReturn.Replace(fC, String.Empty)
                    Next
                    Return tReturn
                Else
                    Return String.Empty
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            Return String.Empty
        End If
        Return String.Empty
    End Function

    Public Sub AddMovie(ByVal _movie As FileRename)
        _movies.Add(_movie)
    End Sub

    Public Sub DoRename(Optional ByVal sfunction As ShowProgress = Nothing)
        Dim DoDB As Boolean
        Dim DoUpdate As Boolean
        Dim _movieDB As Structures.DBMovie = Nothing
        Dim iProg As Integer = 0
        Try
            For Each f As FileFolderRenamer.FileRename In _movies
                If f.IsRenamed Then
                    iProg += 1
                    DoUpdate = False

                    If Not f.IsLocked Then
                        Dim srcDir As String = Path.Combine(f.BasePath, f.Path)
                        Dim destDir As String = Path.Combine(f.BasePath, f.NewPath)

                        If f.IsVideo_TS Then
                            srcDir = Path.Combine(srcDir, "VIDEO_TS")
                            destDir = Path.Combine(destDir, "VIDEO_TS")
                        ElseIf f.IsBDMV Then
                            srcDir = Path.Combine(srcDir, String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM"))
                            destDir = Path.Combine(destDir, String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM"))
                        End If

                        If Not f.ID = -1 Then
                            _movieDB = Master.DB.LoadMovieFromDB(f.ID)
                            DoDB = True
                        Else
                            _movieDB = Nothing
                            DoDB = False
                        End If
                        'Rename Directory
                        If Not srcDir = destDir Then

                            If Not sfunction Is Nothing Then
                                If Not sfunction(f.NewPath, iProg) Then Return
                            End If

                            Try
                                If Not f.IsSingle Then
                                    Directory.CreateDirectory(destDir)
                                Else
                                    If srcDir.ToLower = destDir.ToLower Then
                                        Directory.Move(srcDir, String.Concat(destDir, ".$emm"))
                                        Directory.Move(String.Concat(destDir, ".$emm"), destDir)
                                    Else
                                        If Not Directory.Exists(Directory.GetParent(destDir).FullName) Then Directory.CreateDirectory(Directory.GetParent(destDir).FullName)
                                        Directory.Move(srcDir, destDir)
                                    End If
                                End If
                                DoUpdate = True
                            Catch ex As Exception
                                Master.eLog.WriteToErrorLog(ex.Message, "Dir: " & srcDir & " " & destDir, "Error")
                                'Need to make some type of failure log
                                Continue For
                            End Try

                        End If
                        'Rename Files
                        If Not f.IsVideo_TS AndAlso Not f.IsBDMV Then
                            If (Not f.NewFileName = f.FileName) OrElse (f.Path = String.Empty AndAlso Not f.NewPath = String.Empty) OrElse Not f.IsSingle Then
                                Dim tmpList As New List(Of String)
                                Dim di As DirectoryInfo

                                If f.IsSingle Then
                                    di = New DirectoryInfo(destDir)
                                Else
                                    di = New DirectoryInfo(srcDir)
                                End If

                                Dim lFi As New List(Of FileInfo)
                                If Not sfunction Is Nothing Then
                                    If Not sfunction(f.NewFileName, iProg) Then Return
                                End If
                                Try
                                    lFi.AddRange(di.GetFiles())
                                Catch
                                End Try
                                If lFi.Count > 0 Then
                                    Dim srcFile As String
                                    Dim dstFile As String
                                    For Each lFile As FileInfo In lFi.OrderBy(Function(s) s.Name)
                                        srcFile = lFile.FullName
                                        dstFile = Path.Combine(destDir, lFile.Name.Replace(f.FileName.Trim, f.NewFileName.Trim))

                                        If Not srcFile = dstFile Then
                                            Try

                                                If srcFile.ToLower = dstFile.ToLower Then
                                                    File.Move(srcFile, String.Concat(dstFile, ".$emm$"))
                                                    File.Move(String.Concat(dstFile, ".$emm$"), dstFile)
                                                Else
                                                    If lFile.Name.StartsWith(f.FileName, StringComparison.OrdinalIgnoreCase) Then
                                                        File.Move(srcFile, dstFile)
                                                    End If
                                                End If

                                                DoUpdate = True
                                            Catch ex As Exception
                                                Master.eLog.WriteToErrorLog(ex.Message, "File " & srcFile & " " & dstFile, "Error")
                                                'Need to make some type of failure log
                                            End Try
                                        End If
                                    Next
                                End If
                            End If
                        End If

                        If DoDB AndAlso DoUpdate Then
                            UpdateFaSPaths(_movieDB, destDir, f.FileName, f.NewFileName)
                            Master.DB.SaveMovieToDB(_movieDB, False)
                            If Not f.IsSingle Then
                                Dim fileCount As Integer = 0
                                Dim dirCount As Integer = 0

                                If Directory.Exists(srcDir) Then
                                    Dim di As DirectoryInfo = New DirectoryInfo(srcDir)

                                    Try
                                        fileCount = di.GetFiles().Count
                                    Catch
                                    End Try

                                    Try
                                        dirCount = di.GetDirectories().Count
                                    Catch
                                    End Try

                                    If fileCount = 0 AndAlso dirCount = 0 Then
                                        di.Delete()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Function GetCount() As Integer
        Return _movies.Count
    End Function

    Public Function GetCountLocked() As Integer
        Dim c As Integer = c
        For Each f As FileRename In _movies
            If f.IsLocked Then c += 1
        Next
        Return c
    End Function

    Public Function GetMovies() As DataTable
        Dim dtMovies As New DataTable

        dtMovies.Columns.Add(Master.eLang.GetString(21, "Title", True), GetType(String))
        dtMovies.Columns.Add(Master.eLang.GetString(410, "Path", True), GetType(String))
        dtMovies.Columns.Add(Master.eLang.GetString(15, "File Name", True), GetType(String))
        dtMovies.Columns.Add(Master.eLang.GetString(30, "New Path"), GetType(String))
        dtMovies.Columns.Add(Master.eLang.GetString(31, "New File Name"), GetType(String))
        dtMovies.Columns.Add("IsLocked", GetType(Boolean))
        dtMovies.Columns.Add("DirExist", GetType(Boolean))
        dtMovies.Columns.Add("FileExist", GetType(Boolean))
        dtMovies.Columns.Add("IsSingle", GetType(Boolean))
        dtMovies.Columns.Add("IsRenamed", GetType(Boolean))

        For Each dtRow As FileRename In _movies
            dtMovies.Rows.Add(dtRow.Title, dtRow.Path, dtRow.FileName, dtRow.NewPath, _
                              dtRow.NewFileName, dtRow.IsLocked, dtRow.DirExist, _
                              dtRow.FileExist, dtRow.IsSingle, dtRow.IsRenamed)
        Next

        Return dtMovies
    End Function

    Public Function GetMoviesCount() As Integer
        Dim Renamed = From rList In _movies Where rList.IsRenamed = True
        Return Renamed.Count
    End Function

    Public Sub ProccessFiles(ByVal folderPattern As String, ByVal filePattern As String, Optional ByVal folderPatternIsNotSingle As String = "$D")
        Try
            Dim localFolderPattern As String
            For Each f As FileRename In _movies

                If f.IsSingle Then
                    localFolderPattern = folderPattern
                Else
                    localFolderPattern = folderPatternIsNotSingle
                End If

                f.Path = Path.Combine(f.OldPath, f.Parent)
                f.Path = If(f.Path.StartsWith(Path.DirectorySeparatorChar), f.Path.Substring(1), f.Path)

                If f.IsVideo_TS Then
                    f.NewFileName = "VIDEO_TS"
                ElseIf f.IsBDMV Then
                    f.NewFileName = String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM")
                Else
                    f.NewFileName = ProccessPattern(f, filePattern).Trim
                End If

                If HaveBase(localFolderPattern) Then
                    f.NewPath = ProccessPattern(f, localFolderPattern).Trim
                Else
                    f.NewPath = Path.Combine(f.OldPath, ProccessPattern(f, localFolderPattern).Trim)
                End If
                f.NewPath = If(f.NewPath.StartsWith(Path.DirectorySeparatorChar), f.NewPath.Substring(1), f.NewPath)

                f.FileExist = File.Exists(Path.Combine(f.BasePath, Path.Combine(f.NewPath, f.NewFileName))) AndAlso Not (f.FileName = f.NewFileName)
                f.DirExist = File.Exists(Path.Combine(f.BasePath, f.NewPath)) AndAlso Not (f.Path = f.NewPath)

                f.IsRenamed = Not f.NewPath = f.Path OrElse Not f.NewFileName = f.FileName
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub SetIsLocked(ByVal path As String, ByVal filename As String, ByVal lock As Boolean)
        For Each f As FileRename In _movies
            If (f.Path = path AndAlso f.FileName = filename) OrElse filename = String.Empty Then f.IsLocked = lock
        Next
    End Sub

    Private Shared Function ApplyPattern(ByVal pattern As String, ByVal flag As String, ByVal v As String) As String
        pattern = pattern.Replace(String.Concat("$", flag), v)
        If Not v = String.Empty Then
            pattern = pattern.Replace(String.Concat("$-", flag), v)
            pattern = pattern.Replace(String.Concat("$+", flag), v)
            pattern = pattern.Replace(String.Concat("$^", flag), v)

        Else
            Dim pos = -1
            Dim size = 3
            Dim nextC = pattern.IndexOf(String.Concat("$+", flag))
            If nextC >= 0 Then
                If nextC + 3 < pattern.Length Then size += 1
                pos = nextC
            End If
            Dim prevC = pattern.IndexOf(String.Concat("$-", flag))
            If prevC >= 0 Then
                If prevC + 3 < pattern.Length Then size += 1
                If prevC > 0 Then
                    prevC -= 1
                End If
                pos = prevC
            End If
            Dim bothC = pattern.IndexOf(String.Concat("$^", flag))
            If bothC >= 0 Then
                If bothC + 3 < pattern.Length Then size += 1
                If bothC > 0 Then
                    size += 1
                    bothC -= 1
                End If
                pos = bothC
            End If

            If Not pos = -1 Then pattern = pattern.Remove(pos, size)
        End If
        Return pattern
    End Function

    Private Shared Sub DoRenameSingle(ByVal _frename As FileRename, ByRef _movie As Structures.DBMovie, ByVal BatchMode As Boolean, ByVal toNfo As Boolean, ByVal ShowError As Boolean)
        Try
            If Not _movie.IsLock Then
                Dim srcDir As String = Path.Combine(_frename.BasePath, _frename.Path)
                Dim destDir As String = Path.Combine(_frename.BasePath, _frename.NewPath)

                If _frename.IsVideo_TS Then
                    srcDir = Path.Combine(srcDir, "VIDEO_TS")
                    destDir = Path.Combine(destDir, "VIDEO_TS")

                ElseIf _frename.IsBDMV Then
                    srcDir = Path.Combine(srcDir, String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM"))
                    destDir = Path.Combine(destDir, String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM"))
                End If

                'Rename Directory
                If Not srcDir = destDir Then

                    Try
                        If Not _movie.isSingle Then
                            Directory.CreateDirectory(destDir)
                        Else
                            If srcDir.ToLower = destDir.ToLower Then
                                Directory.Move(srcDir, String.Concat(destDir, ".$emm"))
                                Directory.Move(String.Concat(destDir, ".$emm"), destDir)
                            Else
                                If Not Directory.Exists(Directory.GetParent(destDir).FullName) Then Directory.CreateDirectory(Directory.GetParent(destDir).FullName)
                                Directory.Move(srcDir, destDir)
                            End If
                        End If
                    Catch ex As Exception
                        If ShowError Then
                            MsgBox(String.Format(Master.eLang.GetString(12, "An error occured while attempting to rename the directory:{0}{0}{1}{0}{0}Please ensure that you are not accessing this directory or any of its files from another program (including browsing via Windows Explorer)."), vbNewLine, ex.Message), MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, Master.eLang.GetString(13, "Unable to Rename Directory"))
                        Else
                            Master.eLog.WriteToErrorLog(ex.Message, "Dir: " & srcDir & " " & destDir, "Error")
                        End If
                    End Try

                End If
                'Rename Files
                If Not _frename.IsVideo_TS AndAlso Not _frename.IsBDMV Then
                    If (Not _frename.NewFileName = _frename.FileName) OrElse (_frename.Path = String.Empty AndAlso Not _frename.NewPath = String.Empty) OrElse Not _movie.isSingle Then
                        Dim di As DirectoryInfo

                        If _frename.IsSingle Then
                            di = New DirectoryInfo(destDir)
                        Else
                            di = New DirectoryInfo(srcDir)
                        End If

                        Dim lFi As New List(Of FileInfo)
                        Try
                            lFi.AddRange(di.GetFiles())
                        Catch
                        End Try
                        If lFi.Count > 0 Then
                            Dim srcFile As String
                            Dim dstFile As String
                            For Each lFile As FileInfo In lFi.OrderBy(Function(s) s.Name)
                                srcFile = lFile.FullName
                                dstFile = Path.Combine(destDir, lFile.Name.Replace(_frename.FileName.Trim, _frename.NewFileName.Trim))
                                If Not srcFile = dstFile Then
                                    Try
                                        If srcFile.ToLower = dstFile.ToLower Then
                                            File.Move(srcFile, String.Concat(dstFile, ".$emm$"))
                                            File.Move(String.Concat(dstFile, ".$emm$"), dstFile)
                                        Else
                                            If lFile.Name.StartsWith(_frename.FileName, StringComparison.OrdinalIgnoreCase) Then
                                                File.Move(srcFile, dstFile)
                                            End If
                                        End If

                                    Catch ex As Exception
                                        If ShowError Then
                                            MsgBox(String.Format(Master.eLang.GetString(14, "An error occured while attempting to rename a file:{0}{0}{1}{0}{0}Please ensure that you are not accessing this file from another program."), vbNewLine, ex.Message), MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, Master.eLang.GetString(15, "Unable to Rename File"))
                                        Else
                                            Master.eLog.WriteToErrorLog(ex.Message, "File " & srcFile & " " & dstFile, "Error")
                                        End If
                                    End Try
                                End If
                            Next
                        End If
                    End If
                End If

                UpdateFaSPaths(_movie, destDir, _frename.FileName, _frename.NewFileName)
                Master.DB.SaveMovieToDB(_movie, False, BatchMode, toNfo)
                If Not _frename.IsSingle Then
                    Dim fileCount As Integer = 0
                    Dim dirCount As Integer = 0

                    If Directory.Exists(srcDir) Then
                        Dim di As DirectoryInfo = New DirectoryInfo(srcDir)

                        Try
                            fileCount = di.GetFiles().Count
                        Catch
                        End Try

                        Try
                            dirCount = di.GetDirectories().Count
                        Catch
                        End Try

                        If fileCount = 0 AndAlso dirCount = 0 Then
                            di.Delete()
                        End If
                    End If
                End If

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub UpdateFaSPaths(ByRef _DBM As Structures.DBMovie, ByVal newPath As String, ByVal oldFile As String, ByVal newFile As String)
        If Not String.IsNullOrEmpty(_DBM.FanartPath) Then _DBM.FanartPath = Path.Combine(newPath, Path.GetFileName(_DBM.FanartPath).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.ExtraPath) Then _DBM.ExtraPath = Path.Combine(newPath, Path.GetFileName(_DBM.ExtraPath).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.Filename) Then _DBM.Filename = Path.Combine(newPath, Path.GetFileName(_DBM.Filename).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.NfoPath) Then _DBM.NfoPath = Path.Combine(newPath, Path.GetFileName(_DBM.NfoPath).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.PosterPath) Then _DBM.PosterPath = Path.Combine(newPath, Path.GetFileName(_DBM.PosterPath).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.SubPath) Then _DBM.SubPath = Path.Combine(newPath, Path.GetFileName(_DBM.SubPath).Replace(oldFile, newFile))
        If Not String.IsNullOrEmpty(_DBM.TrailerPath) Then _DBM.TrailerPath = Path.Combine(newPath, Path.GetFileName(_DBM.TrailerPath).Replace(oldFile, newFile))
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Class FileRename

#Region "Fields"

        Private _audio As String
        Private _basePath As String
        Private _dirExist As Boolean
        Private _fileExist As Boolean
        Private _fileName As String
        Private _id As Integer
        Private _isbdmv As Boolean
        Private _islocked As Boolean
        Private _isRenamed As Boolean
        Private _isSingle As Boolean
        Private _isvideo_ts As Boolean
        Private _listtitle As String
        Private _mpaarate As String
        Private _newFileName As String
        Private _newPath As String
        Private _oldpath As String
        Private _originalTitle As String
        Private _parent As String
        Private _path As String
        Private _resolution As String
        Private _title As String
        Private _year As String
        Private _country As String
        Private _sorttitle As String
        Private _imdbid As String
        Private _genre As String
        Private _director As String
        Private _filesource As String

#End Region 'Fields

#Region "Properties"

        Public Property Audio() As String
            Get
                Return Me._audio
            End Get
            Set(ByVal value As String)
                Me._audio = value
            End Set
        End Property

        Public Property BasePath() As String
            Get
                Return Me._basePath
            End Get
            Set(ByVal value As String)
                _basePath = value
            End Set
        End Property

        Public Property DirExist() As Boolean
            Get
                Return Me._dirExist
            End Get
            Set(ByVal value As Boolean)
                Me._dirExist = value
            End Set
        End Property

        Public Property FileExist() As Boolean
            Get
                Return Me._fileExist
            End Get
            Set(ByVal value As Boolean)
                Me._fileExist = value
            End Set
        End Property

        Public Property FileName() As String
            Get
                Return Me._fileName
            End Get
            Set(ByVal value As String)
                Me._fileName = value.Trim
            End Set
        End Property

        Public Property ID() As Integer
            Get
                Return Me._id
            End Get
            Set(ByVal value As Integer)
                Me._id = value
            End Set
        End Property

        Public Property IsBDMV() As Boolean
            Get
                Return Me._isbdmv
            End Get
            Set(ByVal value As Boolean)
                _isbdmv = value
            End Set
        End Property

        Public Property IsLocked() As Boolean
            Get
                Return Me._islocked
            End Get
            Set(ByVal value As Boolean)
                Me._islocked = value
            End Set
        End Property

        Public Property IsRenamed() As Boolean
            Get
                Return Me._isRenamed
            End Get
            Set(ByVal value As Boolean)
                Me._isRenamed = value
            End Set
        End Property

        Public Property IsSingle() As Boolean
            Get
                Return Me._isSingle
            End Get
            Set(ByVal value As Boolean)
                Me._isSingle = value
            End Set
        End Property

        Public Property IsVideo_TS() As Boolean
            Get
                Return Me._isvideo_ts
            End Get
            Set(ByVal value As Boolean)
                _isvideo_ts = value
            End Set
        End Property

        Public Property ListTitle() As String
            Get
                Return Me._listtitle
            End Get
            Set(ByVal value As String)
                Me._listtitle = value.Trim
            End Set
        End Property

        Public Property MPAARate() As String
            Get
                Return Me._mpaarate
            End Get
            Set(ByVal value As String)
                Me._mpaarate = value
            End Set
        End Property

        Public Property NewFileName() As String
            Get
                Return Me._newFileName
            End Get
            Set(ByVal value As String)
                Me._newFileName = value.Trim
            End Set
        End Property

        Public Property NewPath() As String
            Get
                Return Me._newPath
            End Get
            Set(ByVal value As String)
                Me._newPath = value.Trim
            End Set
        End Property

        Public Property OldPath() As String
            Get
                Return Me._oldpath
            End Get
            Set(ByVal value As String)
                Me._oldpath = value.Trim
            End Set
        End Property

        Public Property OriginalTitle() As String
            Get
                Return Me._originalTitle
            End Get
            Set(ByVal value As String)
                Me._originalTitle = value.Trim
            End Set
        End Property

        Public Property Parent() As String
            Get
                Return Me._parent
            End Get
            Set(ByVal value As String)
                Me._parent = value.Trim
            End Set
        End Property

        Public Property Path() As String
            Get
                Return Me._path
            End Get
            Set(ByVal value As String)
                Me._path = value.Trim
            End Set
        End Property

        Public Property Resolution() As String
            Get
                Return Me._resolution
            End Get
            Set(ByVal value As String)
                Me._resolution = value
            End Set
        End Property

        Public Property Country() As String
            Get
                Return Me._country
            End Get
            Set(ByVal value As String)
                Me._country = value.Trim
            End Set
        End Property

        Public Property Title() As String
            Get
                Return Me._title
            End Get
            Set(ByVal value As String)
                Me._title = value.Trim
            End Set
        End Property

        Public Property SortTitle() As String
            Get
                Return Me._sorttitle
            End Get
            Set(ByVal value As String)
                Me._sorttitle = value.Trim
            End Set
        End Property

        Public Property Year() As String
            Get
                Return Me._year
            End Get
            Set(ByVal value As String)
                Me._year = value
            End Set
        End Property

        Public Property IMDBID() As String
            Get
                Return Me._imdbid
            End Get
            Set(ByVal value As String)
                Me._imdbid = value.Trim
            End Set
        End Property

        Public Property Genre() As String
            Get
                Return Me._genre
            End Get
            Set(ByVal value As String)
                Me._genre = value.Trim
            End Set
        End Property

        Public Property Director() As String
            Get
                Return Me._director
            End Get
            Set(ByVal value As String)
                Me._director = value.Trim
            End Set
        End Property

        Public Property FileSource() As String
            Get
                Return Me._filesource
            End Get
            Set(ByVal value As String)
                Me._filesource = value.Trim
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _id = -1
            _title = String.Empty
            _listtitle = String.Empty
            _sorttitle = String.Empty
            _year = String.Empty
            _basePath = String.Empty
            _oldpath = String.Empty
            _path = String.Empty
            _fileName = String.Empty
            _newPath = String.Empty
            _newFileName = String.Empty
            _parent = String.Empty
            _islocked = False
            _dirExist = True
            _fileExist = True
            _isSingle = True
            _isRenamed = False
            _mpaarate = String.Empty
            _resolution = String.Empty
            _audio = String.Empty
            _originalTitle = String.Empty
            _isvideo_ts = False
            _isbdmv = False
            _genre = String.Empty
            _director = String.Empty
            _country = String.Empty
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class