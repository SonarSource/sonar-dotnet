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
Imports System.Text.RegularExpressions

Public Class ThumbGenerator

    #Region "Methods"

    ''' <summary>
    ''' Begin the process to extract extrathumbs
    ''' </summary>
    ''' <param name="mMovie">DBMovie object (for paths)</param>
    ''' <param name="ThumbCount">How many thumbs to extract</param>
    ''' <param name="isEdit"></param>
    ''' <returns>Fanart path if an extrathumb was set as fanart.</returns>
    Public Shared Function CreateRandomThumbs(ByVal mMovie As Structures.DBMovie, ByVal ThumbCount As Integer, ByVal isEdit As Boolean) As String
        Dim tThumb As New GeneratorThread

        tThumb.Movie = mMovie
        tThumb.ThumbCount = ThumbCount
        tThumb.isEdit = isEdit

        tThumb.Start()

        Return tThumb.SetFA
    End Function

    #End Region 'Methods

    #Region "Nested Types"

    Private Class GeneratorThread

        #Region "Fields"

        Public _isedit As Boolean
        Public _movie As Structures.DBMovie
        Public _setfa As String
        Public _thumbcount As Integer

        Private ffmpeg As New Process
        Private isAborting As Boolean = False

        #End Region 'Fields

        #Region "Properties"

        Public WriteOnly Property isEdit() As Boolean
            Set(ByVal value As Boolean)
                _isedit = value
            End Set
        End Property

        Public WriteOnly Property Movie() As Structures.DBMovie
            Set(ByVal value As Structures.DBMovie)
                _movie = value
            End Set
        End Property

        Public ReadOnly Property SetFA() As String
            Get
                Return _setfa
            End Get
        End Property

        Public WriteOnly Property ThumbCount() As Integer
            Set(ByVal value As Integer)
                _thumbcount = value
            End Set
        End Property

        #End Region 'Properties

        #Region "Methods"

        Public Sub InnerThread()
            Dim tThread As Threading.Thread = New Threading.Thread(AddressOf CreateRandom)

            Try
                tThread.Start()

                If Not tThread.Join(Math.Max(120000, 30000 * Master.eSettings.AutoThumbs)) Then 'give it 30 seconds per image with a minimum of two minutes
                    'something went wrong and the thread is hung (movie is corrupt?)... kill it forcibly
                    isAborting = True
                    If Not ffmpeg.HasExited Then
                        ffmpeg.Kill()
                    End If
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Sub

        ''' <summary>
        ''' Start the thread which extracts the thumbs from the movie file.
        ''' </summary>
        Public Sub Start()
            Dim tThread As Threading.Thread = New Threading.Thread(AddressOf InnerThread)

            tThread.Start()

            While tThread.IsAlive
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End Sub

        ''' <summary>
        ''' Extract thumbs from a movie file.
        ''' </summary>
        Private Sub CreateRandom()
            Try
                Dim pExt As String = Path.GetExtension(_movie.Filename).ToLower
                Dim eMovieFile As String = String.Empty
                If Not pExt = ".rar" AndAlso Not pExt = ".iso" AndAlso Not pExt = ".img" AndAlso _
                Not pExt = ".bin" AndAlso Not pExt = ".cue" Then

                    Dim intSeconds As Integer = 0
                    Dim intAdd As Integer = 0
                    Dim tPath As String = String.Empty
                    Dim exImage As New Images

                    If _isedit Then
                        tPath = Path.Combine(Master.TempPath, "extrathumbs")
                        eMovieFile = _movie.Filename
                    Else
                        If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(_movie.Filename) Then
                            tPath = Path.Combine(Directory.GetParent(Directory.GetParent(_movie.Filename).FullName).FullName, "extrathumbs")
                            eMovieFile = FileUtils.Common.GetLongestFromRip(_movie.Filename)
                        ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(_movie.Filename) Then
                            tPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(_movie.Filename).FullName).FullName).FullName, "extrathumbs")
                            eMovieFile = FileUtils.Common.GetLongestFromRip(_movie.Filename)
                        Else
                            tPath = Path.Combine(Directory.GetParent(_movie.Filename).FullName, "extrathumbs")
                            If FileUtils.Common.isVideoTS(_movie.Filename) OrElse FileUtils.Common.isBDRip(_movie.Filename) Then
                                eMovieFile = FileUtils.Common.GetLongestFromRip(_movie.Filename)
                            Else
                                eMovieFile = _movie.Filename
                            End If
                        End If
                    End If

                    If Not Directory.Exists(tPath) Then
                        Directory.CreateDirectory(tPath)
                    End If

                    ffmpeg.StartInfo.FileName = Functions.GetFFMpeg
                    ffmpeg.EnableRaisingEvents = False
                    ffmpeg.StartInfo.UseShellExecute = False
                    ffmpeg.StartInfo.CreateNoWindow = True
                    ffmpeg.StartInfo.RedirectStandardOutput = True
                    ffmpeg.StartInfo.RedirectStandardError = True

                    'first get the duration

                    ffmpeg.StartInfo.Arguments = String.Format("-i ""{0}"" -an", eMovieFile)

                    ffmpeg.Start()
                    Dim d As StreamReader = ffmpeg.StandardError
                    Do
                        Dim s As String = d.ReadLine()
                        If s.Contains("Duration: ") Then
                            Dim sTime As String = Regex.Match(s, "Duration: (?<dur>.*?),").Groups("dur").ToString
                            If Not sTime = "N/A" Then
                                Dim ts As TimeSpan = CDate(CDate(String.Format("{0} {1}", DateTime.Today.ToString("d"), sTime))).Subtract(CDate(DateTime.Today))
                                intSeconds = ((ts.Hours * 60) + ts.Minutes) * 60 + ts.Seconds
                            End If
                        End If
                    Loop While Not d.EndOfStream AndAlso Not isAborting

                    If isAborting Then Exit Sub

                    ffmpeg.WaitForExit()
                    ffmpeg.Close()

                    If isAborting Then Exit Sub

                    If intSeconds > 0 AndAlso ((Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds / 2 > _thumbcount + 300) OrElse (Not Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds > _thumbcount + 2)) Then
                        If Master.eSettings.AutoThumbsNoSpoilers Then
                            intSeconds = Convert.ToInt32(((intSeconds / 2) - 300) / _thumbcount)
                            intAdd = intSeconds
                            intSeconds += intAdd + 300
                        Else
                            intSeconds = Convert.ToInt32(intSeconds / (_thumbcount + 2))
                            intAdd = intSeconds
                            intSeconds += intAdd
                        End If

                        For i = 0 To (_thumbcount - 1)
                            'check to see if file already exists... if so, don't bother running ffmpeg since we're not
                            'overwriting current thumbs anyway
                            If Not File.Exists(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg"))) Then

                                ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -an -f rawvideo -vframes 1 -vcodec mjpeg ""{2}""", intSeconds, eMovieFile, Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))

                                ffmpeg.Start()
                                ffmpeg.WaitForExit()
                                If isAborting Then Exit Sub
                                ffmpeg.Close()
                                exImage = New Images
                                exImage.ResizeExtraThumb(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")), Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                                exImage.Dispose()
                                exImage = Nothing
                            End If
                            intSeconds += intAdd
                        Next
                    End If

                    If isAborting Then Exit Sub

                    Dim fThumbs As New List(Of String)
                    Try
                        fThumbs.AddRange(Directory.GetFiles(tPath, "thumb*.jpg"))
                    Catch
                    End Try

                    If fThumbs.Count <= 0 Then
                        FileUtils.Delete.DeleteDirectory(tPath)
                    Else
                        'always set to something if extrathumbs are created so we know during scrapers
                        _setfa = "TRUE"
                        Using exFanart As New Images
                            If Master.eSettings.UseETasFA AndAlso String.IsNullOrEmpty(_movie.FanartPath) Then
                                exFanart.FromFile(Path.Combine(tPath, "thumb1.jpg"))
                                _setfa = exFanart.SaveAsFanart(_movie)
                            End If
                        End Using
                    End If

                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            End Try
        End Sub

        #End Region 'Methods

    End Class

    #End Region 'Nested Types

End Class