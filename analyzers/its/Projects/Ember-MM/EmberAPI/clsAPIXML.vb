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
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Drawing
Imports System.Xml.Linq

Public Class APIXML

#Region "Fields"

    Public Shared lFlags As New List(Of Flag)
    Public Shared alGenres As New List(Of String)
    Public Shared dStudios As New Dictionary(Of String, String)
    Public Shared GenreXML As New XDocument
    Public Shared RatingXML As New XDocument
    Public Shared SourceList As New List(Of String)(New String() {"bluray", "hddvd", "hdtv", "dvd", "sdtv"})

#End Region 'Fields

#Region "Methods"

    Public Shared Sub CacheXMLs()
        Try
            Dim fPath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags")
            If Directory.Exists(fPath) Then
                Dim cFileName As String = String.Empty
                Dim fType As FlagType = FlagType.Unknown
                Try
                    For Each lFile As String In Directory.GetFiles(fPath, "*.png")
                        cFileName = Path.GetFileNameWithoutExtension(lFile)
                        If cFileName.Contains("_") Then
                            fType = GetFlagTypeFromString(cFileName.Substring(0, cFileName.IndexOf("_")))
                            If Not fType = FlagType.Unknown Then
                                Using fsImage As New FileStream(lFile, FileMode.Open, FileAccess.Read)
                                    lFlags.Add(New Flag With {.Name = cFileName.Remove(0, cFileName.IndexOf("_") + 1), .Image = Image.FromStream(fsImage), .Path = lFile, .Type = fType})
                                End Using
                            End If
                        End If
                    Next
                Catch
                End Try
            End If

            Dim gPath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Genres", Path.DirectorySeparatorChar, "Genres.xml")
            If File.Exists(gPath) Then
                GenreXML = XDocument.Load(gPath)
            End If

            If Directory.Exists(Directory.GetParent(gPath).FullName) Then
                Try
                    alGenres.AddRange(Directory.GetFiles(Directory.GetParent(gPath).FullName, "*.jpg"))
                Catch
                End Try
                alGenres = alGenres.ConvertAll(Function(s) s.ToLower)
            End If

            Dim sPath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Studios", Path.DirectorySeparatorChar, "Studios.xml")

            If Directory.Exists(Directory.GetParent(sPath).FullName) Then
                Try
                    'get all images in the main folder
                    For Each lFile As String In Directory.GetFiles(Directory.GetParent(sPath).FullName, "*.png")
                        dStudios.Add(Path.GetFileNameWithoutExtension(lFile).ToLower, lFile)
                    Next

                    'now get all images in sub folders
                    For Each iDir As String In Directory.GetDirectories(Directory.GetParent(sPath).FullName)
                        For Each lFile As String In Directory.GetFiles(iDir, "*.png")
                            'hard code "\" here, then resplace when retrieving images
                            dStudios.Add(String.Concat(Directory.GetParent(iDir).Name, "\", Path.GetFileNameWithoutExtension(lFile).ToLower), lFile)
                        Next
                    Next
                Catch
                End Try
            End If

            Dim rPath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Ratings", Path.DirectorySeparatorChar, "Ratings.xml")
            If File.Exists(rPath) Then
                RatingXML = XDocument.Load(rPath)
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Function GetAVImages(ByVal fiAV As MediaInfo.Fileinfo, ByVal fName As String, ByVal ForTV As Boolean, ByVal fileSource As String) As Image()
        Dim iReturn(5) As Image
        Dim tVideo As MediaInfo.Video = NFO.GetBestVideo(fiAV)
        Dim tAudio As MediaInfo.Audio = NFO.GetBestAudio(fiAV, ForTV)

        If lFlags.Count > 0 Then
            Try
                Dim vRes As String = NFO.GetResFromDimensions(tVideo).ToLower
                Dim vresFlag As Flag = lFlags.FirstOrDefault(Function(f) f.Name = vRes AndAlso f.Type = FlagType.VideoResolution)
                If Not IsNothing(vresFlag) Then
                    iReturn(0) = vresFlag.Image
                Else
                    vresFlag = lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = FlagType.VideoResolution)
                    If Not IsNothing(vresFlag) Then
                        iReturn(0) = vresFlag.Image
                    Else
                        iReturn(0) = My.Resources.defaultscreen
                    End If
                End If

                Dim vSource As String = fileSource 'GetFileSource(fName)
                Dim vsourceFlag As Flag = lFlags.FirstOrDefault(Function(f) f.Name.ToLower = vSource.ToLower AndAlso f.Type = FlagType.VideoSource)
                If Not IsNothing(vsourceFlag) Then
                    iReturn(1) = vsourceFlag.Image
                Else
                    vsourceFlag = lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = FlagType.VideoSource)
                    If Not IsNothing(vsourceFlag) Then
                        iReturn(1) = vsourceFlag.Image
                    Else
                        iReturn(1) = My.Resources.defaultscreen
                    End If
                End If

                Dim vcodecFlag As Flag = lFlags.FirstOrDefault(Function(f) f.Name = tVideo.Codec.ToLower AndAlso f.Type = FlagType.VideoCodec)
                If Not IsNothing(vcodecFlag) Then
                    iReturn(2) = vcodecFlag.Image
                Else
                    vcodecFlag = lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = FlagType.VideoCodec)
                    If Not IsNothing(vcodecFlag) Then
                        iReturn(2) = vcodecFlag.Image
                    Else
                        iReturn(2) = My.Resources.defaultscreen
                    End If
                End If

                Dim acodecFlag As Flag = lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Codec.ToLower AndAlso f.Type = FlagType.AudioCodec)
                If Not IsNothing(acodecFlag) Then
                    If tAudio.HasPreferred Then
                        iReturn(3) = ImageUtils.SetOverlay(acodecFlag.Image, 64, 44, My.Resources.haslanguage, 4)
                    Else
                        iReturn(3) = acodecFlag.Image
                    End If
                Else
                    acodecFlag = lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = FlagType.AudioCodec)
                    If Not IsNothing(acodecFlag) Then
                        If tAudio.HasPreferred Then
                            iReturn(3) = ImageUtils.SetOverlay(acodecFlag.Image, 64, 44, My.Resources.haslanguage, 4)
                        Else
                            iReturn(3) = acodecFlag.Image
                        End If
                    Else
                        If tAudio.HasPreferred Then
                            iReturn(3) = ImageUtils.SetOverlay(My.Resources.defaultsound, 64, 44, My.Resources.haslanguage, 4)
                        Else
                            iReturn(3) = My.Resources.defaultsound
                        End If
                    End If
                End If

                Dim achanFlag As Flag = lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Channels AndAlso f.Type = FlagType.AudioChan)
                If Not IsNothing(achanFlag) Then
                    iReturn(4) = achanFlag.Image
                Else
                    achanFlag = lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = FlagType.AudioChan)
                    If Not IsNothing(achanFlag) Then
                        iReturn(4) = achanFlag.Image
                    Else
                        iReturn(4) = My.Resources.defaultsound
                    End If
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            iReturn(0) = My.Resources.defaultscreen
            iReturn(1) = My.Resources.defaultscreen
            iReturn(2) = My.Resources.defaultscreen
            If tAudio.HasPreferred Then
                iReturn(3) = ImageUtils.SetOverlay(My.Resources.defaultsound, 64, 44, My.Resources.haslanguage, 4)
            Else
                iReturn(3) = My.Resources.defaultsound
            End If
            iReturn(4) = My.Resources.defaultsound
        End If

        Return iReturn
    End Function

    Public Shared Function GetFileSource(ByVal sPath As String) As String
        Dim sourceCheck As String = String.Empty

        Try
            If FileUtils.Common.isVideoTS(sPath) Then
                Return "dvd"
            ElseIf FileUtils.Common.isBDRip(sPath) Then
                Return "bluray"
            Else
                sourceCheck = If(Master.eSettings.SourceFromFolder, String.Concat(Directory.GetParent(sPath).Name.ToLower, Path.DirectorySeparatorChar, Path.GetFileName(sPath).ToLower), Path.GetFileName(sPath).ToLower)
                Dim mySources As New Hashtable
                mySources = AdvancedSettings.GetComplexSetting("MovieSources")
                If Not mySources Is Nothing Then
                    For Each k In mySources.Keys
                        If Regex.IsMatch(sourceCheck, k.ToString) Then
                            Return mySources.Item(k).ToString
                        End If
                    Next
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return String.Empty
    End Function

    Public Shared Function GetGenreImage(ByVal strGenre As String) As Image
        '//
        ' Set the proper images based on the genre string
        '\\

        Dim imgGenre As Image = Nothing
        Dim imgGenreStr As String = String.Empty

        If GenreXML.Nodes.Count > 0 Then

            Dim mePath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Genres")

            Try

                Dim xDefault = From xDef In GenreXML...<default> Select xDef.<icon>.Value
                If xDefault.Count > 0 Then
                    imgGenreStr = Path.Combine(mePath, xDefault(0).ToString)
                End If

                Dim xGenre = From xGen In GenreXML...<name> Where strGenre.ToLower = xGen.@searchstring.ToLower Select xGen.<icon>.Value
                If xGenre.Count > 0 Then
                    imgGenreStr = Path.Combine(mePath, xGenre(0).ToString)
                End If

                If Not String.IsNullOrEmpty(imgGenreStr) AndAlso alGenres.Contains(imgGenreStr.ToLower) Then
                    Using fsImage As New FileStream(imgGenreStr, FileMode.Open, FileAccess.Read)
                        imgGenre = Image.FromStream(fsImage)
                    End Using
                Else
                    imgGenre = My.Resources.defaultgenre
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            imgGenre = My.Resources.defaultgenre
        End If

        Return imgGenre
    End Function

    Public Shared Function GetGenreList(Optional ByVal LangsOnly As Boolean = False) As Object()
        Dim retGenre As New List(Of String)
        Try
            If LangsOnly Then
                Dim xGenre = From xGen In GenreXML...<supported>.Descendants Select xGen.Value
                If xGenre.Count > 0 Then
                    retGenre.AddRange(xGenre.ToArray)
                End If
            Else
                Dim splitLang() As String
                Dim xGenre = From xGen In GenreXML...<name> Select xGen.@searchstring, xGen.@language
                If xGenre.Count > 0 Then
                    For i As Integer = 0 To xGenre.Count - 1
                        If Not IsNothing(xGenre(i).language) Then
                            splitLang = xGenre(i).language.Split(Convert.ToChar("|"))
                            For Each strGen As String In splitLang
                                If Not retGenre.Contains(xGenre(i).searchstring) AndAlso (Master.eSettings.GenreFilter.Contains(String.Format("{0}", Master.eLang.GetString(569, Master.eLang.All))) OrElse Master.eSettings.GenreFilter.Split(Convert.ToChar(",")).Contains(strGen)) Then
                                    retGenre.Add(xGenre(i).searchstring)
                                End If
                            Next
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return retGenre.ToArray
    End Function

    Public Shared Function GetRatingImage(ByVal strRating As String) As Image
        '//
        ' Parse the floating Rating box
        '\\

        Dim imgRating As Image = Nothing
        Dim imgRatingStr As String = String.Empty

        If RatingXML.Nodes.Count > 0 Then
            Dim mePath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Ratings")

            Try

                If Master.eSettings.UseCertForMPAA AndAlso Not Master.eSettings.CertificationLang = "USA" AndAlso Not IsNothing(RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)) AndAlso RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)...<movie>.Descendants.Count > 0 Then
                    Dim xRating = From xRat In RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)...<movie>...<name> Where strRating.ToLower = xRat.@searchstring.ToLower OrElse strRating.ToLower = xRat.@searchstring.ToLower.Split(Convert.ToChar(":"))(1) Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(xRating.Count - 1).ToString)
                    End If
                Else
                    Dim xRating = From xRat In RatingXML...<usa>...<movie>...<name> Where strRating.ToLower.StartsWith(xRat.@searchstring.ToLower) Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(xRating.Count - 1).ToString)
                    End If
                End If

                If Not String.IsNullOrEmpty(imgRatingStr) AndAlso File.Exists(imgRatingStr) Then
                    Using fsImage As New FileStream(imgRatingStr, FileMode.Open, FileAccess.Read)
                        imgRating = Image.FromStream(fsImage)
                    End Using
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If

        Return imgRating
    End Function

    Public Shared Function GetRatingList() As Object()
        Dim retRatings As New List(Of String)
        Try
            If Master.eSettings.UseCertForMPAA AndAlso Not Master.eSettings.CertificationLang = "USA" AndAlso Not IsNothing(RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)) AndAlso RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower).Descendants("movie").Count > 0 Then
                Dim xRating = From xRat In RatingXML.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)...<movie>...<name> Select xRat.@searchstring
                If xRating.Count > 0 Then
                    retRatings.AddRange(xRating.ToArray)
                End If
            Else
                Dim xRating = From xRat In RatingXML...<usa>...<movie>...<name> Select xRat.@searchstring
                If xRating.Count > 0 Then
                    retRatings.AddRange(xRating.ToArray)
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return retRatings.ToArray
    End Function

    Public Shared Function GetRatingRegions() As Object()
        Dim retRatings As New List(Of String)
        Try
            Dim xRating = From xRat In RatingXML...<ratings>.Elements.Descendants("tv") Select (xRat.Parent.Name.ToString)
            If xRating.Count > 0 Then
                retRatings.AddRange(xRating.ToArray)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return retRatings.ToArray
    End Function

    Public Shared Function GetStudioImage(ByVal strStudio As String) As Image
        Dim imgStudio As Image = Nothing

        If dStudios.ContainsKey(strStudio.ToLower) Then
            Using fsImage As New FileStream(dStudios.Item(strStudio.ToLower).Replace(Convert.ToChar("\"), Path.DirectorySeparatorChar), FileMode.Open, FileAccess.Read)
                imgStudio = Image.FromStream(fsImage)
            End Using
        End If

        If IsNothing(imgStudio) Then imgStudio = My.Resources.defaultscreen

        Return imgStudio
    End Function

    Public Shared Function GetTVRatingImage(ByVal strRating As String) As Image
        Dim imgRating As Image = Nothing
        Dim imgRatingStr As String = String.Empty

        If RatingXML.Nodes.Count > 0 Then
            Dim mePath As String = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Ratings")

            Try

                If Master.eSettings.ShowRatingRegion = "usa" AndAlso RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower)...<tv>.Descendants.Count > 0 Then
                    Dim xRating = From xRat In RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower)...<tv>...<name> Where strRating.ToLower = xRat.@searchstring.ToLower Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(xRating.Count - 1).ToString)
                    End If
                Else
                    Dim xRating = From xRat In RatingXML...<usa>...<tv>...<name> Where strRating.ToLower.StartsWith(xRat.@searchstring.ToLower) Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(xRating.Count - 1).ToString)
                    End If
                End If

                If Not String.IsNullOrEmpty(imgRatingStr) AndAlso File.Exists(imgRatingStr) Then
                    Using fsImage As New FileStream(imgRatingStr, FileMode.Open, FileAccess.Read)
                        imgRating = Image.FromStream(fsImage)
                    End Using
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If

        Return imgRating
    End Function

    Public Shared Function GetTVRatingList() As Object()
        Dim retRatings As New List(Of String)
        Try
            If Not Master.eSettings.ShowRatingRegion = "USA" AndAlso RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower).Descendants("tv").Count > 0 Then
                Dim xRating = From xRat In RatingXML.Element("ratings").Element(Master.eSettings.ShowRatingRegion.ToLower)...<tv>...<name> Select xRat.@searchstring
                If xRating.Count > 0 Then
                    retRatings.AddRange(xRating.ToArray)
                End If
            Else
                Dim xRating = From xRat In RatingXML...<usa>...<tv>...<name> Select xRat.@searchstring
                If xRating.Count > 0 Then
                    retRatings.AddRange(xRating.ToArray)
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return retRatings.ToArray
    End Function

    Public Shared Function XMLToLowerCase(ByVal sXML As String) As String
        Dim sMatches As MatchCollection = Regex.Matches(sXML, "\<(.*?)\>", RegexOptions.IgnoreCase)
        For Each sMatch As Match In sMatches
            sXML = sXML.Replace(sMatch.Groups(1).Value, sMatch.Groups(1).Value.ToLower)
        Next
        Return sXML
    End Function

    Public Shared Function GetFlagTypeFromString(ByVal sType As String) As FlagType
        Select Case sType
            Case "vcodec"
                Return FlagType.VideoCodec
            Case "vres"
                Return FlagType.VideoResolution
            Case "vsource"
                Return FlagType.VideoSource
            Case "acodec"
                Return FlagType.AudioCodec
            Case "achan"
                Return FlagType.AudioChan
            Case Else
                Return FlagType.Unknown
        End Select
    End Function

#End Region 'Methods

    Public Enum FlagType
        VideoCodec = 0
        VideoResolution = 1
        VideoSource = 3
        AudioCodec = 4
        AudioChan = 5
        Unknown = 6
    End Enum

    Public Class Flag
        Private _name As String
        Private _image As Image
        Private _path As String
        Private _type As FlagType

        Public Property Name() As String
            Get
                Return Me._name
            End Get
            Set(ByVal value As String)
                Me._name = value
            End Set
        End Property

        Public Property Image() As Image
            Get
                Return Me._image
            End Get
            Set(ByVal value As Image)
                Me._image = value
            End Set
        End Property

        Public Property Path() As String
            Get
                Return Me._path
            End Get
            Set(ByVal value As String)
                Me._path = value
            End Set
        End Property

        Public Property Type() As FlagType
            Get
                Return Me._type
            End Get
            Set(ByVal value As FlagType)
                Me._type = value
            End Set
        End Property

        Public Sub New()
            Me.Clear()
        End Sub

        Public Sub Clear()
            Me._name = String.Empty
            Me._image = Nothing
            Me._path = String.Empty
            Me._type = FlagType.VideoCodec
        End Sub
    End Class
End Class