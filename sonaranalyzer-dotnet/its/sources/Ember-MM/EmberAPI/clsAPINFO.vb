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
Imports System.Xml
Imports System.Xml.Serialization

Public Class NFO

    #Region "Methods"

    Public Shared Function FIToString(ByVal miFI As MediaInfo.Fileinfo, ByVal isTV As Boolean) As String
        '//
        ' Convert Fileinfo into a string to be displayed in the GUI
        '\\

        Dim strOutput As New StringBuilder
        Dim iVS As Integer = 1
        Dim iAS As Integer = 1
        Dim iSS As Integer = 1

        Try
            If Not IsNothing(miFI) Then

                If Not IsNothing(miFI.StreamDetails) Then
                    If miFI.StreamDetails.Video.Count > 0 Then
                        strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(595, "Video Streams"), miFI.StreamDetails.Video.Count.ToString, vbNewLine)
                    End If

                    If miFI.StreamDetails.Audio.Count > 0 Then
                        strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(596, "Audio Streams"), miFI.StreamDetails.Audio.Count.ToString, vbNewLine)
                    End If

                    If miFI.StreamDetails.Subtitle.Count > 0 Then
                        strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(597, "Subtitle  Streams"), miFI.StreamDetails.Subtitle.Count.ToString, vbNewLine)
                    End If

                    For Each miVideo As MediaInfo.Video In miFI.StreamDetails.Video
                        strOutput.AppendFormat("{0}{1} {2}{0}", vbNewLine, Master.eLang.GetString(617, "Video Stream"), iVS)
                        If Not String.IsNullOrEmpty(miVideo.Width) AndAlso Not String.IsNullOrEmpty(miVideo.Height) Then
                            strOutput.AppendFormat("- {0}{1}", String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), miVideo.Width, miVideo.Height), vbNewLine)
                        End If
                        If Not String.IsNullOrEmpty(miVideo.Aspect) Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(614, "Aspect Ratio"), miVideo.Aspect, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Scantype) Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(605, "Scan Type"), miVideo.Scantype, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Codec) Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(604, "Codec"), miVideo.Codec, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Duration) Then strOutput.AppendFormat("- {0}: {1}", Master.eLang.GetString(609, "Duration"), miVideo.Duration)
                        If Not String.IsNullOrEmpty(miVideo.LongLanguage) Then strOutput.AppendFormat("{0}- {1}: {2}", vbNewLine, Master.eLang.GetString(610, "Language"), miVideo.LongLanguage)
                        iVS += 1
                    Next

                    strOutput.Append(vbNewLine)

                    For Each miAudio As MediaInfo.Audio In miFI.StreamDetails.Audio
                        'audio
                        strOutput.AppendFormat("{0}{1} {2}{0}", vbNewLine, Master.eLang.GetString(618, "Audio Stream"), iAS.ToString)
                        If Not String.IsNullOrEmpty(miAudio.Codec) Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(604, "Codec"), miAudio.Codec, vbNewLine)
                        If Not String.IsNullOrEmpty(miAudio.Channels) Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(611, "Channels"), miAudio.Channels, vbNewLine)
                        If Not String.IsNullOrEmpty(miAudio.LongLanguage) Then strOutput.AppendFormat("- {0}: {1}", Master.eLang.GetString(610, "Language"), miAudio.LongLanguage)
                        iAS += 1
                    Next

                    strOutput.Append(vbNewLine)

                    For Each miSub As MediaInfo.Subtitle In miFI.StreamDetails.Subtitle
                        'subtitles
                        strOutput.AppendFormat("{0}{1} {2}{0}", vbNewLine, Master.eLang.GetString(619, "Subtitle Stream"), iSS.ToString)
                        If Not String.IsNullOrEmpty(miSub.LongLanguage) Then strOutput.AppendFormat("- {0}: {1}", Master.eLang.GetString(610, "Language"), miSub.LongLanguage)
                        iSS += 1
                    Next
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        If strOutput.ToString.Trim.Length > 0 Then
            Return strOutput.ToString
        Else
            If isTV Then
                Return Master.eLang.GetString(504, "Meta Data is not available for this episode. Try rescanning.")
            Else
                Return Master.eLang.GetString(419, "Meta Data is not available for this movie. Try rescanning.")
            End If
        End If
    End Function

    Public Shared Function GetBestAudio(ByVal miFIA As MediaInfo.Fileinfo, ByVal ForTV As Boolean) As MediaInfo.Audio
        '//
        ' Get the highest values from file info
        '\\

        Dim fiaOut As New MediaInfo.Audio
        Try
            Dim sinMostChannels As Single = 0
            Dim sinChans As Single = 0

            fiaOut.Codec = String.Empty
            fiaOut.Channels = String.Empty
            fiaOut.Language = String.Empty

            For Each miAudio As MediaInfo.Audio In miFIA.StreamDetails.Audio
                If Not String.IsNullOrEmpty(miAudio.Channels) Then
                    sinChans = NumUtils.ConvertToSingle(miAudio.Channels)
                    If sinChans > sinMostChannels Then
                        sinMostChannels = sinChans
                        fiaOut.Codec = miAudio.Codec
                        fiaOut.Channels = sinChans.ToString
                        fiaOut.Language = miAudio.Language
                    End If
                End If

                If ForTV Then
                    If Not String.IsNullOrEmpty(Master.eSettings.TVFlagLang) AndAlso miAudio.LongLanguage.ToLower = Master.eSettings.TVFlagLang.ToLower Then fiaOut.HasPreferred = True
                Else
                    If Not String.IsNullOrEmpty(Master.eSettings.FlagLang) AndAlso miAudio.LongLanguage.ToLower = Master.eSettings.FlagLang.ToLower Then fiaOut.HasPreferred = True
                End If
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return fiaOut
    End Function

    Public Shared Function GetBestVideo(ByVal miFIV As MediaInfo.Fileinfo) As MediaInfo.Video
        '//
        ' Get the highest values from file info
        '\\

        Dim fivOut As New MediaInfo.Video
        Try
            Dim iWidest As Integer = 0
            Dim iWidth As Integer = 0

            'set some defaults to make it easy on ourselves
            fivOut.Width = String.Empty
            fivOut.Height = String.Empty
            fivOut.Aspect = String.Empty
            fivOut.Codec = String.Empty
            fivOut.Duration = String.Empty
            fivOut.Scantype = String.Empty
            fivOut.Language = String.Empty

            For Each miVideo As MediaInfo.Video In miFIV.StreamDetails.Video
                If Not String.IsNullOrEmpty(miVideo.Width) Then
                    iWidth = Convert.ToInt32(miVideo.Width)
                    If iWidth > iWidest Then
                        iWidest = iWidth
                        fivOut.Width = miVideo.Width
                        fivOut.Height = miVideo.Height
                        fivOut.Aspect = miVideo.Aspect
                        fivOut.Codec = miVideo.Codec
                        fivOut.Duration = miVideo.Duration
                        fivOut.Scantype = miVideo.Scantype
                        fivOut.Language = miVideo.Language
                    End If
                End If
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return fivOut
    End Function

    Public Shared Function GetDimensionsFromVideo(ByVal fiRes As MediaInfo.Video) As String
        '//
        ' Get the dimension values of the video from the information provided by MediaInfo.dll
        '\\

        Dim result As String = String.Empty
        Try
            If Not String.IsNullOrEmpty(fiRes.Width) AndAlso Not String.IsNullOrEmpty(fiRes.Height) AndAlso Not String.IsNullOrEmpty(fiRes.Aspect) Then
                Dim iWidth As Integer = Convert.ToInt32(fiRes.Width)
                Dim iHeight As Integer = Convert.ToInt32(fiRes.Height)
                Dim sinADR As Single = NumUtils.ConvertToSingle(fiRes.Aspect)

                result = String.Format("{0}x{1} ({2})", iWidth, iHeight, sinADR.ToString("0.00"))
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return result
    End Function

    Public Shared Function GetEpNfoPath(ByVal EpPath As String) As String
        Dim nPath As String = String.Empty

        If File.Exists(String.Concat(FileUtils.Common.RemoveExtFromPath(EpPath), ".nfo")) Then
            nPath = String.Concat(FileUtils.Common.RemoveExtFromPath(EpPath), ".nfo")
        End If

        Return nPath
    End Function

    Public Shared Function GetIMDBFromNonConf(ByVal sPath As String, ByVal isSingle As Boolean) As NonConf
        Dim tNonConf As New NonConf
        Dim dirPath As String = Directory.GetParent(sPath).FullName
        Dim lFiles As New List(Of String)

        If isSingle Then
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, "*.nfo"))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, "*.info"))
            Catch
            End Try
        Else
            Dim fName As String = StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath)).ToLower
            Dim oName As String = Path.GetFileNameWithoutExtension(sPath)
            fName = If(fName.EndsWith("*"), fName, String.Concat(fName, "*"))
            oName = If(oName.EndsWith("*"), oName, String.Concat(oName, "*"))

            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(fName, ".nfo")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(oName, ".nfo")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(fName, ".info")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(oName, ".info")))
            Catch
            End Try
        End If

        For Each sFile As String In lFiles
            Using srInfo As New StreamReader(sFile)
                Dim sInfo As String = srInfo.ReadToEnd
                Dim sIMDBID As String = Regex.Match(sInfo, "tt\d\d\d\d\d\d\d", RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToString

                If Not String.IsNullOrEmpty(sIMDBID) Then
                    tNonConf.IMDBID = sIMDBID
                    'now lets try to see if the rest of the file is a proper nfo
                    If sInfo.ToLower.Contains("</movie>") Then
                        tNonConf.Text = APIXML.XMLToLowerCase(sInfo.Substring(0, sInfo.ToLower.IndexOf("</movie>") + 8))
                    End If
                    Exit For
                Else
                    sIMDBID = Regex.Match(sPath, "tt\d\d\d\d\d\d\d", RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToString
                    If Not String.IsNullOrEmpty(sIMDBID) Then
                        tNonConf.IMDBID = sIMDBID
                    End If
                End If
            End Using
        Next
        Return tNonConf
    End Function

    Public Shared Function GetNfoPath(ByVal sPath As String, ByVal isSingle As Boolean) As String
        '//
        ' Get the proper path to NFO
        '\\

        Dim nPath As String = String.Empty

        If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(sPath) Then
            If Master.eSettings.MovieNameNFO Then
                nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Directory.GetParent(Directory.GetParent(sPath).FullName).Name), ".nfo")
            ElseIf Master.eSettings.MovieNFO Then
                nPath = String.Concat(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, "\", "movie.nfo")
            Else
                nPath = String.Concat(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, "\", "movie.nfo")
            End If

            If File.Exists(nPath) Then
                Return nPath
            Else
                If Not isSingle Then
                    Return String.Empty
                Else
                    'return movie path so we can use it for looking for non-conforming nfos
                    Return sPath
                End If
            End If
        ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(sPath) Then
            If Master.eSettings.MovieNameNFO Then
                nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).Name), ".nfo")
            ElseIf Master.eSettings.MovieNFO Then
                nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).FullName, "movie.nfo"))
            Else
                nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).FullName, "movie.nfo"))
            End If

            If File.Exists(nPath) Then
                Return nPath
            Else
                If Not isSingle Then
                    Return String.Empty
                Else
                    'return movie path so we can use it for looking for non-conforming nfos
                    Return sPath
                End If
            End If
        Else
            Dim tmpName As String = StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath))
            Dim tmpNameNoStack As String = Path.GetFileNameWithoutExtension(sPath)
            nPath = Path.Combine(Directory.GetParent(sPath).FullName, tmpName).ToLower
            Dim nPathWithStack As String = Path.Combine(Directory.GetParent(sPath).FullName, tmpNameNoStack).ToLower

            Dim fList As New List(Of String)
            Try
                fList.AddRange(Directory.GetFiles(Directory.GetParent(sPath).FullName, "*.nfo"))
            Catch
            End Try
            fList = fList.ConvertAll(Function(s) s.ToLower)

            If isSingle AndAlso Master.eSettings.MovieNFO AndAlso fList.Contains(Path.Combine(Directory.GetParent(sPath).FullName.ToLower, "movie.nfo")) Then
                Return Path.Combine(Directory.GetParent(nPath).FullName.ToLower, "movie.nfo")
            ElseIf Master.eSettings.MovieNameNFO AndAlso fList.Contains(String.Concat(nPathWithStack, ".nfo")) Then
                Return String.Concat(nPathWithStack, ".nfo")
            ElseIf Master.eSettings.MovieNameNFO AndAlso fList.Contains(String.Concat(nPath, ".nfo")) Then
                Return String.Concat(nPath, ".nfo")
            Else
                If Not isSingle Then
                    Return String.Empty
                Else
                    'return movie path so we can use it for looking for non-conforming nfos
                    Return sPath
                End If
            End If
        End If
    End Function

    Public Shared Function GetResFromDimensions(ByVal fiRes As MediaInfo.Video) As String
        '//
        ' Get the resolution of the video from the dimensions provided by MediaInfo.dll
        '\\

        Dim resOut As String = String.Empty
        Try
            If Not String.IsNullOrEmpty(fiRes.Width) AndAlso Not String.IsNullOrEmpty(fiRes.Height) AndAlso Not String.IsNullOrEmpty(fiRes.Aspect) Then
                Dim iWidth As Integer = Convert.ToInt32(fiRes.Width)
                Dim iHeight As Integer = Convert.ToInt32(fiRes.Height)
                Dim sinADR As Single = NumUtils.ConvertToSingle(fiRes.Aspect)

                Select Case True
                    Case iWidth < 640
                        resOut = "SD"
                        'exact
                    Case (iWidth = 1920 AndAlso (iHeight = 1080 OrElse iHeight = 800)) OrElse (iWidth = 1440 AndAlso iHeight = 1080) OrElse (iWidth = 1280 AndAlso iHeight = 1080)
                        resOut = "1080"
                    Case (iWidth = 1366 AndAlso iHeight = 768) OrElse (iWidth = 1024 AndAlso iHeight = 768)
                        resOut = "768"
                    Case (iWidth = 960 AndAlso iHeight = 720) OrElse (iWidth = 1280 AndAlso (iHeight = 720 OrElse iHeight = 544))
                        resOut = "720"
                    Case (iWidth = 1024 AndAlso iHeight = 576) OrElse (iWidth = 720 AndAlso iHeight = 576)
                        resOut = "576"
                    Case (iWidth = 720 OrElse iWidth = 960) AndAlso iHeight = 540
                        resOut = "540"
                    Case (iWidth = 852 OrElse iWidth = 720 OrElse iWidth = 704 OrElse iWidth = 640) AndAlso iHeight = 480
                        resOut = "480"
                        'by ADR
                    Case sinADR >= 1.4 AndAlso iWidth = 1920
                        resOut = "1080"
                    Case sinADR >= 1.4 AndAlso iWidth = 1366
                        resOut = "768"
                    Case sinADR >= 1.4 AndAlso iWidth = 1280
                        resOut = "720"
                    Case sinADR >= 1.4 AndAlso iWidth = 1024
                        resOut = "576"
                    Case sinADR >= 1.4 AndAlso iWidth = 960
                        resOut = "540"
                    Case sinADR >= 1.4 AndAlso iWidth = 852
                        resOut = "480"
                        'loose
                    Case iWidth >= 1200 AndAlso iHeight > 768
                        resOut = "1080"
                    Case iWidth >= 1000 AndAlso iHeight > 720
                        resOut = "768"
                    Case iWidth >= 1000 AndAlso iHeight > 500
                        resOut = "720"
                    Case iWidth >= 700 AndAlso iHeight > 540
                        resOut = "576"
                    Case iWidth >= 700 AndAlso iHeight > 480
                        resOut = "540"
                    Case Else
                        resOut = "480"
                End Select
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        If Not String.IsNullOrEmpty(resOut) Then
            If String.IsNullOrEmpty(fiRes.Scantype) Then
                Return String.Concat(resOut)
            Else
                Return String.Concat(resOut, If(fiRes.Scantype.ToLower = "progressive", "p", "i"))
            End If
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function GetShowNfoPath(ByVal ShowPath As String) As String
        Dim nPath As String = String.Empty

        If File.Exists(Path.Combine(ShowPath, "tvshow.nfo")) Then
            nPath = Path.Combine(ShowPath, "tvshow.nfo")
        End If

        Return nPath
    End Function

    Public Shared Function IsConformingEpNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.EpisodeDetails))
        Dim testEp As New MediaContainers.EpisodeDetails

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using xmlSR As StreamReader = New StreamReader(sPath)
                    Dim xmlStr As String = xmlSR.ReadToEnd
                    Dim rMatches As MatchCollection = Regex.Matches(xmlStr, "<episodedetails.*?>.*?</episodedetails>", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.IgnorePatternWhitespace)
                    If rMatches.Count = 1 Then
                        Using xmlRead As StringReader = New StringReader(rMatches(0).Value)
                            testEp = DirectCast(testSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                            testSer = Nothing
                            testEp = Nothing
                            Return True
                        End Using
                    ElseIf rMatches.Count > 1 Then
                        'read them all... if one fails, the entire nfo is non conforming
                        For Each xmlReg As Match In rMatches
                            Using xmlRead As StringReader = New StringReader(xmlReg.Value)
                                testEp = DirectCast(testSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                testEp = Nothing
                            End Using
                        Next
                        testSer = Nothing
                        Return True
                    Else
                        testSer = Nothing
                        If Not IsNothing(testEp) Then
                            testEp = Nothing
                        End If
                        Return False
                    End If
                End Using
            Else
                testSer = Nothing
                testEp = Nothing
                Return False
            End If
        Catch
            If Not IsNothing(testSer) Then
                testSer = Nothing
            End If
            If Not IsNothing(testEp) Then
                testEp = Nothing
            End If
            Return False
        End Try
    End Function

    Public Shared Function IsConformingNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = Nothing

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using testSR As StreamReader = New StreamReader(sPath)
                    testSer = New XmlSerializer(GetType(MediaContainers.Movie))
                    Dim testMovie As MediaContainers.Movie = DirectCast(testSer.Deserialize(testSR), MediaContainers.Movie)
                    testMovie = Nothing
                    testSer = Nothing
                End Using
                Return True
            Else
                Return False
            End If
        Catch
            If Not IsNothing(testSer) Then
                testSer = Nothing
            End If

            Return False
        End Try
    End Function

    Public Shared Function IsConformingShowNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = Nothing

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using testSR As StreamReader = New StreamReader(sPath)
                    testSer = New XmlSerializer(GetType(MediaContainers.TVShow))
                    Dim testShow As MediaContainers.TVShow = DirectCast(testSer.Deserialize(testSR), MediaContainers.TVShow)
                    testShow = Nothing
                    testSer = Nothing
                End Using
                Return True
            Else
                Return False
            End If
        Catch
            If Not IsNothing(testSer) Then
                testSer = Nothing
            End If

            Return False
        End Try
    End Function

    Public Shared Function LoadMovieFromNFO(ByVal sPath As String, ByVal isSingle As Boolean) As MediaContainers.Movie
        '//
        ' Deserialze the NFO to pass all the data to a MediaContainers.Movie
        '\\

        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlMov As New MediaContainers.Movie

        If Not String.IsNullOrEmpty(sPath) Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                        xmlMov = DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.Movie)
                        xmlMov.Genre = Strings.Join(xmlMov.LGenre.ToArray, " / ")
                        xmlMov.Outline = xmlMov.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                        xmlMov.Plot = xmlMov.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                    End Using
                Else
                    If Not String.IsNullOrEmpty(sPath) Then
                        Dim sReturn As New NonConf
                        sReturn = GetIMDBFromNonConf(sPath, isSingle)
                        xmlMov.IMDBID = sReturn.IMDBID
                        Try
                            If Not String.IsNullOrEmpty(sReturn.Text) Then
                                Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                                    xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                                    xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                                    xmlMov.Genre = Strings.Join(xmlMov.LGenre.ToArray, " / ")
                                    xmlMov.Outline = xmlMov.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                                    xmlMov.Plot = xmlMov.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                                    xmlMov.IMDBID = sReturn.IMDBID
                                End Using
                            End If
                        Catch
                        End Try
                    End If
                End If

            Catch
                xmlMov.Clear()
                If Not String.IsNullOrEmpty(sPath) Then

                    'go ahead and rename it now, will still be picked up in getimdbfromnonconf
                    If Not Master.eSettings.OverwriteNfo Then
                        RenameNonConfNfo(sPath, True)
                    End If

                    Dim sReturn As New NonConf
                    sReturn = GetIMDBFromNonConf(sPath, isSingle)
                    xmlMov.IMDBID = sReturn.IMDBID
                    Try
                        If Not String.IsNullOrEmpty(sReturn.Text) Then
                            Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                                xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                                xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                                xmlMov.Genre = Strings.Join(xmlMov.LGenre.ToArray, " / ")
                                xmlMov.Outline = xmlMov.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                                xmlMov.Plot = xmlMov.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                                xmlMov.IMDBID = sReturn.IMDBID
                            End Using
                        End If
                    Catch
                    End Try
                End If
            End Try

            If Not IsNothing(xmlSer) Then
                xmlSer = Nothing
            End If
        End If

        Return xmlMov
    End Function

    Public Shared Function LoadTVEpFromNFO(ByVal sPath As String, ByVal SeasonNumber As Integer, ByVal EpisodeNumber As Integer) As MediaContainers.EpisodeDetails
        Dim xmlSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.EpisodeDetails))
        Dim xmlEp As New MediaContainers.EpisodeDetails

        If Not String.IsNullOrEmpty(sPath) AndAlso SeasonNumber >= -1 Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    'better way to read multi-root xml??
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        Dim xmlStr As String = xmlSR.ReadToEnd
                        Dim rMatches As MatchCollection = Regex.Matches(xmlStr, "<episodedetails.*?>.*?</episodedetails>", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.IgnorePatternWhitespace)
                        If rMatches.Count = 1 Then
                            'only one episodedetail... assume it's the proper one
                            Using xmlRead As StringReader = New StringReader(rMatches(0).Value)
                                xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                xmlSer = Nothing
                                Return xmlEp
                            End Using
                        ElseIf rMatches.Count > 1 Then
                            For Each xmlReg As Match In rMatches
                                Using xmlRead As StringReader = New StringReader(xmlReg.Value)
                                    xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                    If xmlEp.Episode = EpisodeNumber AndAlso xmlEp.Season = SeasonNumber Then
                                        xmlSer = Nothing
                                        Return xmlEp
                                    End If
                                End Using
                            Next
                        End If
                    End Using

                Else
                    'not really anything else to do with non-conforming nfos aside from rename them
                    If Not Master.eSettings.OverwriteNfo Then
                        RenameEpNonConfNfo(sPath, True)
                    End If
                End If

            Catch
                'not really anything else to do with non-conforming nfos aside from rename them
                If Not Master.eSettings.OverwriteNfo Then
                    RenameEpNonConfNfo(sPath, True)
                End If
            End Try
        End If

        Return New MediaContainers.EpisodeDetails
    End Function

    Public Shared Function LoadTVShowFromNFO(ByVal sPath As String) As MediaContainers.TVShow
        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlShow As New MediaContainers.TVShow

        If Not String.IsNullOrEmpty(sPath) Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        xmlSer = New XmlSerializer(GetType(MediaContainers.TVShow))
                        xmlShow = DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.TVShow)
                        xmlShow.Genre = Strings.Join(xmlShow.LGenre.ToArray, " / ")
                    End Using
                Else
                    'not really anything else to do with non-conforming nfos aside from rename them
                    If Not Master.eSettings.OverwriteNfo Then
                        RenameShowNonConfNfo(sPath)
                    End If
                End If

            Catch
                'not really anything else to do with non-conforming nfos aside from rename them
                If Not Master.eSettings.OverwriteNfo Then
                    RenameShowNonConfNfo(sPath)
                End If
            End Try

            Try
                Dim params As New List(Of Object)(New Object() {xmlShow})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnTVShowNFORead, params, doContinue, False)

            Catch ex As Exception
            End Try

            If Not IsNothing(xmlSer) Then
                xmlSer = Nothing
            End If
        End If

        Return xmlShow
    End Function

    Public Shared Sub SaveMovieToNFO(ByRef movieToSave As Structures.DBMovie)
        '//
        ' Serialize MediaContainers.Movie to an NFO
        '\\
        Try
            Try
                Dim params As New List(Of Object)(New Object() {movieToSave})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMovieNFOSave, params, doContinue, False)
                If Not doContinue Then Return
            Catch ex As Exception
            End Try

            If Not String.IsNullOrEmpty(movieToSave.Filename) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.Movie))

                Dim tPath As String = String.Empty
                Dim nPath As String = String.Empty
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True

                If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(movieToSave.Filename) Then
                    If Master.eSettings.MovieNameNFO Then
                        tPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).Name), ".nfo")
                    ElseIf Master.eSettings.MovieNFO Then
                        tPath = String.Concat(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName, "\", "movie.nfo")
                    Else
                        tPath = String.Concat(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName, "\", "movie.nfo")
                    End If

                    If Not Master.eSettings.OverwriteNfo Then
                        RenameNonConfNfo(tPath, False)
                    End If

                    doesExist = File.Exists(tPath)
                    If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                        If doesExist Then
                            fAtt = File.GetAttributes(tPath)
                            Try
                                File.SetAttributes(tPath, FileAttributes.Normal)
                            Catch ex As Exception
                                fAttWritable = False
                            End Try
                        End If

                        Using xmlSW As New StreamWriter(tPath)
                            movieToSave.NfoPath = tPath
                            xmlSer.Serialize(xmlSW, movieToSave.Movie)
                        End Using

                        If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                    End If
                ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(movieToSave.Filename) Then
                    If Master.eSettings.MovieNameNFO Then
                        tPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName).Name), ".nfo")
                    ElseIf Master.eSettings.MovieNFO Then
                        tPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName).FullName, "movie.nfo"))
                    Else
                        tPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(movieToSave.Filename).FullName).FullName).FullName, "movie.nfo"))
                    End If

                    If Not Master.eSettings.OverwriteNfo Then
                        RenameNonConfNfo(tPath, False)
                    End If

                    doesExist = File.Exists(tPath)
                    If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                        If doesExist Then
                            fAtt = File.GetAttributes(tPath)
                            Try
                                File.SetAttributes(tPath, FileAttributes.Normal)
                            Catch ex As Exception
                                fAttWritable = False
                            End Try
                        End If

                        Using xmlSW As New StreamWriter(tPath)
                            movieToSave.NfoPath = tPath
                            xmlSer.Serialize(xmlSW, movieToSave.Movie)
                        End Using

                        If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                    End If
                Else
                    Dim tmpName As String = Path.GetFileNameWithoutExtension(movieToSave.Filename)
                    nPath = Path.Combine(Directory.GetParent(movieToSave.Filename).FullName, tmpName)

                    If Master.eSettings.MovieNameNFO AndAlso (Not movieToSave.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                        If FileUtils.Common.isVideoTS(movieToSave.Filename) Then
                            tPath = Path.Combine(Directory.GetParent(movieToSave.Filename).FullName, "video_ts.nfo")
                        ElseIf FileUtils.Common.isBDRip(movieToSave.Filename) Then
                            tPath = Path.Combine(Directory.GetParent(movieToSave.Filename).FullName, "index.nfo")
                        Else
                            tPath = String.Concat(nPath, ".nfo")
                        End If

                        If Not Master.eSettings.OverwriteNfo Then
                            RenameNonConfNfo(tPath, False)
                        End If

                        doesExist = File.Exists(tPath)
                        If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                            If doesExist Then
                                fAtt = File.GetAttributes(tPath)
                                Try
                                    File.SetAttributes(tPath, FileAttributes.Normal)
                                Catch ex As Exception
                                    fAttWritable = False
                                End Try
                            End If

                            Using xmlSW As New StreamWriter(tPath)
                                movieToSave.NfoPath = tPath
                                xmlSer.Serialize(xmlSW, movieToSave.Movie)
                            End Using

                            If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                        End If
                    End If

                    If movieToSave.isSingle AndAlso Master.eSettings.MovieNFO Then
                        tPath = Path.Combine(Directory.GetParent(nPath).FullName, "movie.nfo")

                        If Not Master.eSettings.OverwriteNfo Then
                            RenameNonConfNfo(tPath, False)
                        End If

                        doesExist = File.Exists(tPath)
                        If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                            If doesExist Then
                                fAtt = File.GetAttributes(tPath)
                                Try
                                    File.SetAttributes(tPath, FileAttributes.Normal)
                                Catch ex As Exception
                                    fAttWritable = False
                                End Try
                            End If

                            Using xmlSW As New StreamWriter(tPath)
                                movieToSave.NfoPath = tPath
                                xmlSer.Serialize(xmlSW, movieToSave.Movie)
                            End Using
                            If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub SaveSingleNFOItem(ByVal sPath As String, ByVal strToWrite As String, ByVal strNode As String)
        '//
        ' Save just one item of an NFO file
        '\\

        Try
            Dim xmlDoc As New XmlDocument()
            'use streamreader to open NFO so we don't get any access violations when trying to save
            Dim xmlSR As New StreamReader(sPath)
            'copy NFO to string
            Dim xmlString As String = xmlSR.ReadToEnd
            'close the streamreader... we're done with it
            xmlSR.Close()
            xmlSR = Nothing

            xmlDoc.LoadXml(xmlString)
            Dim xNode As XmlNode = xmlDoc.SelectSingleNode(strNode)
            xNode.InnerText = strToWrite
            xmlDoc.Save(sPath)

            xmlDoc = Nothing
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public NotInheritable Class Utf8StringWriter
        Inherits StringWriter
        Public Overloads Overrides ReadOnly Property Encoding() As Encoding
            Get
                Return Encoding.UTF8
            End Get
        End Property
    End Class

    Public Shared Sub SaveTVEpToNFO(ByRef tvEpToSave As Structures.DBTV)
        Try

            If Not String.IsNullOrEmpty(tvEpToSave.Filename) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.EpisodeDetails))

                Dim tPath As String = String.Empty
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True
                Dim EpList As New List(Of MediaContainers.EpisodeDetails)
                Dim sBuilder As New StringBuilder

                Dim tmpName As String = Path.GetFileNameWithoutExtension(tvEpToSave.Filename)
                tPath = String.Concat(Path.Combine(Directory.GetParent(tvEpToSave.Filename).FullName, tmpName), ".nfo")

                If Not Master.eSettings.OverwriteNfo Then
                    RenameEpNonConfNfo(tPath, False)
                End If

                doesExist = File.Exists(tPath)
                If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                    If doesExist Then
                        fAtt = File.GetAttributes(tPath)
                        Try
                            File.SetAttributes(tPath, FileAttributes.Normal)
                        Catch ex As Exception
                            fAttWritable = False
                        End Try
                    End If

                    Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        SQLCommand.CommandText = "SELECT ID FROM TVEps WHERE ID <> (?) AND TVEpPathID IN (SELECT ID FROM TVEpPaths WHERE TVEpPath = (?)) ORDER BY Episode"
                        Dim parID As SQLite.SQLiteParameter = SQLCommand.Parameters.Add("parID", DbType.Int64, 0, "ID")
                        Dim parTVEpPath As SQLite.SQLiteParameter = SQLCommand.Parameters.Add("parTVEpPath", DbType.String, 0, "TVEpPath")

                        parID.Value = tvEpToSave.EpID
                        parTVEpPath.Value = tvEpToSave.Filename

                        Using SQLreader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                            While SQLreader.Read
                                EpList.Add(Master.DB.LoadTVEpFromDB(Convert.ToInt64(SQLreader("ID")), False).TVEp)
                            End While
                        End Using

                        EpList.Add(tvEpToSave.TVEp)

                        Dim NS As New XmlSerializerNamespaces
                        NS.Add(String.Empty, String.Empty)

                        For Each tvEp As MediaContainers.EpisodeDetails In EpList.OrderBy(Function(s) s.Season)
                            Using xmlSW As New Utf8StringWriter
                                xmlSer.Serialize(xmlSW, tvEp, NS)
                                If sBuilder.Length > 0 Then
                                    sBuilder.Append(vbNewLine)
                                    xmlSW.GetStringBuilder.Remove(0, xmlSW.GetStringBuilder.ToString.IndexOf(vbNewLine) + 1)
                                End If
                                sBuilder.Append(xmlSW.ToString)
                            End Using
                        Next

                        tvEpToSave.EpNfoPath = tPath

                        If sBuilder.Length > 0 Then
                            Using fSW As New StreamWriter(tPath)
                                fSW.Write(sBuilder.ToString)
                            End Using
                        End If
                    End Using
                    If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub SaveTVShowToNFO(ByRef tvShowToSave As Structures.DBTV)
        '//
        ' Serialize MediaContainers.TVShow to an NFO
        '\\

        Try
            Dim params As New List(Of Object)(New Object() {tvShowToSave})
            Dim doContinue As Boolean = True
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnTVShowNFOSave, params, doContinue, False)
            If Not doContinue Then Return
        Catch ex As Exception
        End Try

        Try
            If Not String.IsNullOrEmpty(tvShowToSave.ShowPath) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.TVShow))

                Dim tPath As String = String.Empty
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True

                tPath = Path.Combine(tvShowToSave.ShowPath, "tvshow.nfo")

                If Not Master.eSettings.OverwriteNfo Then
                    RenameShowNonConfNfo(tPath)
                End If

                doesExist = File.Exists(tPath)
                If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                    If doesExist Then
                        fAtt = File.GetAttributes(tPath)
                        Try
                            File.SetAttributes(tPath, FileAttributes.Normal)
                        Catch ex As Exception
                            fAttWritable = False
                        End Try
                    End If

                    Using xmlSW As New StreamWriter(tPath)
                        tvShowToSave.ShowNfoPath = tPath
                        xmlSer.Serialize(xmlSW, tvShowToSave.TVShow)
                    End Using

                    If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub RenameEpNonConfNfo(ByVal sPath As String, ByVal isChecked As Boolean)
        'test if current nfo is non-conforming... rename per setting

        Try
            If File.Exists(sPath) AndAlso Not IsConformingEpNfo(sPath) Then
                RenameToInfo(sPath)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub RenameNonConfNfo(ByVal sPath As String, ByVal isChecked As Boolean)
        'test if current nfo is non-conforming... rename per setting

        Try
            If isChecked OrElse Not IsConformingNfo(sPath) Then
                If isChecked OrElse File.Exists(sPath) Then
                    RenameToInfo(sPath)
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub RenameShowNonConfNfo(ByVal sPath As String)
        'test if current nfo is non-conforming... rename per setting

        Try
            If File.Exists(sPath) AndAlso Not IsConformingShowNfo(sPath) Then
                RenameToInfo(sPath)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub RenameToInfo(ByVal sPath As String)
        Try
            Dim i As Integer = 1
            Dim strNewName As String = String.Concat(FileUtils.Common.RemoveExtFromPath(sPath), ".info")
            'in case there is already a .info file
            If File.Exists(strNewName) Then
                Do
                    strNewName = String.Format("{0}({1}).info", FileUtils.Common.RemoveExtFromPath(sPath), i)
                    i += 1
                Loop While File.Exists(strNewName)
                strNewName = String.Format("{0}({1}).info", FileUtils.Common.RemoveExtFromPath(sPath), i)
            End If
            My.Computer.FileSystem.RenameFile(sPath, Path.GetFileName(strNewName))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub LoadTVEpDuration(ByVal _TVEpDB As Structures.DBTV)
        'Fix for runtime to display in gui without watching episode first.
        Try
            If Not IsNothing(_TVEpDB.TVEp.FileInfo.StreamDetails) AndAlso _TVEpDB.TVEp.FileInfo.StreamDetails.Video.Count > 0 Then
                Dim cTotal As String = String.Empty
                For Each tVid As MediaInfo.Video In _TVEpDB.TVEp.FileInfo.StreamDetails.Video
                    cTotal = cTotal + tVid.Duration
                Next
                _TVEpDB.TVEp.Runtime = cTotal
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    #End Region 'Methods

    #Region "Nested Types"

    Public Class NonConf

        #Region "Fields"

        Private _imdbid As String
        Private _text As String

        #End Region 'Fields

        #Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

        #End Region 'Constructors

        #Region "Properties"

        Public Property IMDBID() As String
            Get
                Return Me._imdbid
            End Get
            Set(ByVal value As String)
                Me._imdbid = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(ByVal value As String)
                Me._text = value
            End Set
        End Property

        #End Region 'Properties

        #Region "Methods"

        Public Sub Clear()
            Me._imdbid = String.Empty
            Me._text = String.Empty
        End Sub

        #End Region 'Methods

    End Class

    #End Region 'Nested Types

End Class