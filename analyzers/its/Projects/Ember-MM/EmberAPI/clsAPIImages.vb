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

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

<Serializable()> _
Public Class Images
    Implements IDisposable

#Region "Fields"

    Private ms As MemoryStream = New MemoryStream()
    Private Ret As Byte()
    <NonSerialized()> _
    Private sHTTP As New HTTP
    Private _image As Image
    Private _isedit As Boolean

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Clear()
    End Sub

#End Region 'Constructors

#Region "Properties"

    Public Property IsEdit() As Boolean
        Get
            Return _isedit
        End Get
        Set(ByVal value As Boolean)
            _isedit = value
        End Set
    End Property

    Public Property [Image]() As Image
        Get
            Return _image
        End Get
        Set(ByVal value As Image)
            _image = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Shared Function GetFanartDims(ByVal imgImage As Image) As Enums.FanartSize
        '//
        ' Check the size of the image and return a generic name for the size
        '\\

        Dim x As Integer = imgImage.Width
        Dim y As Integer = imgImage.Height

        Try
            If (y > 1000 AndAlso x > 750) OrElse (x > 1000 AndAlso y > 750) Then
                Return Enums.FanartSize.Lrg
            ElseIf (y > 700 AndAlso x > 400) OrElse (x > 700 AndAlso y > 400) Then
                Return Enums.FanartSize.Mid
            Else
                Return Enums.FanartSize.Small
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return Enums.FanartSize.Small
        End Try
    End Function

    Public Shared Function GetPosterDims(ByVal imgImage As Image) As Enums.PosterSize
        '//
        ' Check the size of the image and return a generic name for the size
        '\\

        Dim x As Integer = imgImage.Width
        Dim y As Integer = imgImage.Height

        Try
            If (x > y) AndAlso (x > (y * 2)) AndAlso (x > 300) Then
                'at least twice as wide than tall... consider it wide (also make sure it's big enough)
                Return Enums.PosterSize.Wide
            ElseIf (y > 1000 AndAlso x > 750) OrElse (x > 1000 AndAlso y > 750) Then
                Return Enums.PosterSize.Xlrg
            ElseIf (y > 700 AndAlso x > 500) OrElse (x > 700 AndAlso y > 500) Then
                Return Enums.PosterSize.Lrg
            ElseIf (y > 250 AndAlso x > 150) OrElse (x > 250 AndAlso y > 150) Then
                Return Enums.PosterSize.Mid
            Else
                Return Enums.PosterSize.Small
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return Enums.PosterSize.Small
        End Try        
    End Function

    Public Sub Clear()
        _isedit = False
        _image = Nothing
    End Sub

    Public Sub Delete(ByVal sPath As String)
        If Not String.IsNullOrEmpty(sPath) Then
            File.Delete(sPath)
        End If
    End Sub

    Public Sub DeleteAllSeasonPosters(ByVal mShow As Structures.DBTV)
        Try
            Dim tPath As String = mShow.ShowPath

            Delete(Path.Combine(tPath, "season-all.tbn"))
            Delete(Path.Combine(tPath, "season-all.jpg"))
            Delete(Path.Combine(tPath, "season-all-poster.jpg"))

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteEpFanart(ByVal mShow As Structures.DBTV)
        Try
            Delete(String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.ShowPath), "-fanart.jpg"))
            Delete(String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.ShowPath), ".fanart.jpg"))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteEpPosters(ByVal mShow As Structures.DBTV)
        Try
            Dim tPath As String = FileUtils.Common.RemoveExtFromPath(mShow.Filename)

            Delete(String.Concat(tPath, ".tbn"))
            Delete(String.Concat(tPath, ".jpg"))

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteFanart(ByVal mMovie As Structures.DBMovie)
        Try
            Dim tPath As String = Directory.GetParent(mMovie.Filename).FullName
            Dim params As New List(Of Object)(New Object() {mMovie})
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMovieFanartDelete, params, Nothing, False)

            If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(mMovie.Filename) Then
                If Master.eSettings.FanartJPG Then
                    Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "fanart.jpg"))
                ElseIf Master.eSettings.MovieNameFanartJPG Then
                    Delete(String.Concat(Path.Combine(Directory.GetParent(tPath).FullName, Directory.GetParent(tPath).Name), "-fanart.jpg"))
                ElseIf Master.eSettings.MovieNameDotFanartJPG Then
                    Delete(String.Concat(Path.Combine(Directory.GetParent(tPath).FullName, Directory.GetParent(tPath).Name), ".fanart.jpg"))
                Else
                    Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "fanart.jpg"))
                End If
            ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(mMovie.Filename) Then
                If Master.eSettings.FanartJPG Then
                    Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "fanart.jpg"))
                ElseIf Master.eSettings.MovieNameFanartJPG Then
                    Delete(String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, Directory.GetParent(Directory.GetParent(tPath).FullName).Name), "-fanart.jpg"))
                ElseIf Master.eSettings.MovieNameDotFanartJPG Then
                    Delete(String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, Directory.GetParent(Directory.GetParent(tPath).FullName).Name), ".fanart.jpg"))
                Else
                    Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "fanart.jpg"))
                End If
            Else
                If mMovie.isSingle Then
                    Delete(Path.Combine(tPath, "fanart.jpg"))
                End If

                If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                    Delete(Path.Combine(tPath, "video_ts-fanart.jpg"))
                    Delete(Path.Combine(tPath, "video_ts.fanart.jpg"))
                ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                    Delete(Path.Combine(tPath, "index-fanart.jpg"))
                    Delete(Path.Combine(tPath, "index.fanart.jpg"))
                Else
                    Dim fPath As String = Path.Combine(tPath, Path.GetFileNameWithoutExtension(mMovie.Filename))
                    Delete(String.Concat(fPath, "-fanart.jpg"))
                    Delete(String.Concat(fPath, ".fanart.jpg"))
                End If

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeletePosters(ByVal mMovie As Structures.DBMovie)
        Try
            Dim tPath As String = Directory.GetParent(mMovie.Filename).FullName
            Dim params As New List(Of Object)(New Object() {mMovie})
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMoviePosterDelete, params, Nothing, False)
            If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(mMovie.Filename) Then
                With Master.eSettings
                    If .MovieNameJPG Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(tPath).FullName, Directory.GetParent(tPath).Name), ".jpg"))
                    ElseIf .MovieJPG Then
                        Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "movie.jpg"))
                    ElseIf .FolderJPG Then
                        Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "folder.jpg"))
                    ElseIf .PosterJPG Then
                        Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "poster.jpg"))
                    ElseIf .MovieNameTBN Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(tPath).FullName, Directory.GetParent(tPath).Name), ".tbn"))
                    ElseIf .MovieNameDashPosterJPG Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(tPath).FullName, Directory.GetParent(tPath).Name), "-poster.jpg"))
                    ElseIf .MovieTBN Then
                        Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "movie.tbn"))
                    ElseIf .PosterTBN Then
                        Delete(String.Concat(Directory.GetParent(tPath).FullName, "\", "poster.tbn"))
                    End If
                End With
            ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(mMovie.Filename) Then
                With Master.eSettings
                    If .MovieNameJPG Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, Directory.GetParent(Directory.GetParent(tPath).FullName).Name), ".jpg"))
                    ElseIf .MovieJPG Then
                        Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "movie.jpg"))
                    ElseIf .FolderJPG Then
                        Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "folder.jpg"))
                    ElseIf .PosterJPG Then
                        Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "poster.jpg"))
                    ElseIf .MovieNameTBN Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, Directory.GetParent(Directory.GetParent(tPath).FullName).Name), ".tbn"))
                    ElseIf .MovieNameDashPosterJPG Then
                        Delete(String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, Directory.GetParent(Directory.GetParent(tPath).FullName).Name), "-poster.jpg"))
                    ElseIf .MovieTBN Then
                        Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "movie.tbn"))
                    ElseIf .PosterTBN Then
                        Delete(String.Concat(Directory.GetParent(Directory.GetParent(tPath).FullName).FullName, "\", "poster.tbn"))
                    End If
                End With
            Else

                If mMovie.isSingle Then
                    Delete(Path.Combine(tPath, "movie.tbn"))
                    Delete(Path.Combine(tPath, "movie.jpg"))
                    Delete(Path.Combine(tPath, "poster.tbn"))
                    Delete(Path.Combine(tPath, "poster.jpg"))
                    Delete(Path.Combine(tPath, "folder.jpg"))
                End If

                If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                    Delete(Path.Combine(tPath, "video_ts.tbn"))
                    Delete(Path.Combine(tPath, "video_ts.jpg"))
                ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                    Delete(Path.Combine(tPath, "index.tbn"))
                    Delete(Path.Combine(tPath, "index.jpg"))
                Else
                    Dim pPath As String = Path.Combine(tPath, Path.GetFileNameWithoutExtension(mMovie.Filename))
                    Delete(String.Concat(pPath, ".tbn"))
                    Delete(String.Concat(pPath, ".jpg"))
                End If

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteSeasonFanart(ByVal mShow As Structures.DBTV)
        Try
            Dim tPath As String = String.Empty

            Try
                tPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, mShow.TVEp.Season)
            Catch ex As Exception
            End Try

            If Not String.IsNullOrEmpty(tPath) Then
                Delete(Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".fanart.jpg")))
                Delete(Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), "-fanart.jpg")))
                Delete(Path.Combine(tPath, "Fanart.jpg"))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteSeasonPosters(ByVal mShow As Structures.DBTV)
        Try
            Dim tPath As String = String.Empty

            Try
                tPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, mShow.TVEp.Season)
            Catch
            End Try

            If Not String.IsNullOrEmpty(tPath) Then
                Delete(Path.Combine(tPath, "Poster.tbn"))
                Delete(Path.Combine(tPath, "Poster.jpg"))
                Delete(Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".tbn")))
                Delete(Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".jpg")))
                Delete(Path.Combine(tPath, "Folder.jpg"))
            End If

            If mShow.TVEp.Season = 0 Then
                Delete(Path.Combine(mShow.ShowPath, "season-specials.tbn"))
                Delete(Path.Combine(mShow.ShowPath, "season-specials.jpg"))
                Delete(Path.Combine(mShow.ShowPath, "season-specials-poster.jpg"))
            Else
                Delete(Path.Combine(mShow.ShowPath, String.Format("season{0}.tbn", mShow.TVEp.Season)))
                Delete(Path.Combine(mShow.ShowPath, String.Format("season{0}.tbn", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0")))))
                Delete(Path.Combine(mShow.ShowPath, String.Format("season{0}-poster.jpg", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0")))))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteShowFanart(ByVal mShow As Structures.DBTV)
        Try
            Delete(Path.Combine(mShow.ShowPath, "fanart.jpg"))
            Delete(Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), "-fanart.jpg")))
            Delete(Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), ".fanart.jpg")))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub DeleteShowPosters(ByVal mShow As Structures.DBTV)
        Try
            Dim tPath As String = mShow.ShowPath

            Delete(Path.Combine(tPath, "folder.jpg"))
            Delete(Path.Combine(tPath, "poster.tbn"))
            Delete(Path.Combine(tPath, "poster.jpg"))

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ms.Flush()
        ms.Close()
        ms.Dispose()
        ms = Nothing
        Clear()
    End Sub

    Public Sub FromFile(ByVal sPath As String)
        If Not String.IsNullOrEmpty(sPath) AndAlso File.Exists(sPath) Then
            Try
                Using fsImage As New FileStream(sPath, FileMode.Open, FileAccess.Read)
                    ms.SetLength(fsImage.Length)
                    fsImage.Read(ms.GetBuffer(), 0, Convert.ToInt32(fsImage.Length))
                    ms.Flush()
                    _image = New Bitmap(ms)
                End Using
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error: " & sPath)
            End Try
        End If
    End Sub

    Public Sub FromWeb(ByVal sURL As String)
        Try
            sHTTP.StartDownloadImage(sURL)

            While sHTTP.IsDownloading
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If Not IsNothing(sHTTP.Image) Then _image = New Bitmap(sHTTP.Image)
        Catch
        End Try
    End Sub

    Public Function IsAllowedToDownload(ByVal mMovie As Structures.DBMovie, ByVal fType As Enums.ImageType, Optional ByVal isChange As Boolean = False) As Boolean
        Try
            Select Case fType
                Case Enums.ImageType.Fanart
                    If (isChange OrElse (String.IsNullOrEmpty(mMovie.FanartPath) OrElse Master.eSettings.OverwriteFanart)) AndAlso _
                    (Master.eSettings.MovieNameDotFanartJPG OrElse Master.eSettings.MovieNameFanartJPG OrElse Master.eSettings.FanartJPG) AndAlso _
                    AdvancedSettings.GetBooleanSetting("UseTMDB", True) Then
                        Return True
                    Else
                        Return False
                    End If
                Case Else
                    If (isChange OrElse (String.IsNullOrEmpty(mMovie.PosterPath) OrElse Master.eSettings.OverwritePoster)) AndAlso _
                    (Master.eSettings.MovieTBN OrElse Master.eSettings.MovieNameTBN OrElse Master.eSettings.MovieJPG OrElse _
                     Master.eSettings.MovieNameJPG OrElse Master.eSettings.MovieNameDashPosterJPG OrElse Master.eSettings.PosterTBN OrElse Master.eSettings.PosterJPG OrElse Master.eSettings.FolderJPG) AndAlso _
                     (AdvancedSettings.GetBooleanSetting("UseIMPA", False) OrElse AdvancedSettings.GetBooleanSetting("UseMPDB", False) OrElse AdvancedSettings.GetBooleanSetting("UseTMDB", True)) Then
                        Return True
                    Else
                        Return False
                    End If
            End Select
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return False
        End Try
    End Function

    Public Sub ResizeExtraThumb(ByVal fromPath As String, ByVal toPath As String)
        Me.FromFile(fromPath)
        If Not Master.eSettings.ETNative Then
            Dim iWidth As Integer = Master.eSettings.ETWidth
            Dim iHeight As Integer = Master.eSettings.ETHeight
            ImageUtils.ResizeImage(_image, iWidth, iHeight, Master.eSettings.ETPadding, Color.Black.ToArgb)
        End If
        Me.Save(toPath)
    End Sub

	Public Sub Save(ByVal sPath As String, Optional ByVal iQuality As Long = 0, Optional ByVal sUrl As String = "")
		Try
			If IsNothing(_image) Then Exit Sub

			Dim doesExist As Boolean = File.Exists(sPath)
			Dim fAtt As New FileAttributes
			Dim fAttWritable As Boolean = True
			If Not String.IsNullOrEmpty(sPath) AndAlso (Not doesExist OrElse (Not CBool(File.GetAttributes(sPath) And FileAttributes.ReadOnly))) Then
				If doesExist Then
					'get the current attributes to set them back after writing
					fAtt = File.GetAttributes(sPath)
					'set attributes to none for writing
					Try
						File.SetAttributes(sPath, FileAttributes.Normal)
					Catch ex As Exception
						fAttWritable = False
					End Try
				End If

				If Not sUrl = "" Then
					'TODO V3 API implementation to get ALL posters! http://docs.themoviedb.apiary.io/#configuration
					'  GetsImagesFromTMDBv3("URL/MOVIEDID")

					Dim stroriginalurl As String = sUrl
					'Image Download from tmdb is special, need original size
					If Not sUrl.Contains("impawards") AndAlso Not sUrl.Contains("movieposterdb") Then
						'Always get original image...
						'links to images (tmdb) have following structure:  'example: http://d3gtl9l2a4fn1j.cloudfront.net/t/p/w92/x65b4vsFKYuA878pLN1mJiAsgIP.jpg

						Dim stringArray() As String = Split(stroriginalurl, "/")
						If stringArray.Length > 4 Then
							' stringArray(5) contains values like "w185","original", "w154"...-->size -> we want original!
							stringArray(5) = "original"
							stroriginalurl = Join(stringArray, "/")
						End If
					End If

					Dim webclient As New Net.WebClient
					'Download image!
					webclient.DownloadFile(stroriginalurl, sPath)

				Else
					Using msSave As New MemoryStream
						Dim retSave() As Byte
						Dim ICI As ImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg)
						Dim EncPars As EncoderParameters = New EncoderParameters(If(iQuality > 0, 2, 1))

						EncPars.Param(0) = New EncoderParameter(Encoder.RenderMethod, EncoderValue.RenderNonProgressive)

						If iQuality > 0 Then
							EncPars.Param(1) = New EncoderParameter(Encoder.Quality, iQuality)
						End If

						_image.Save(msSave, ICI, EncPars)

						retSave = msSave.ToArray

						'make sure directory exists
						Directory.CreateDirectory(Directory.GetParent(sPath).FullName)
						If sPath.Length <= 260 Then
							Using fs As New FileStream(sPath, FileMode.Create, FileAccess.Write)
								fs.Write(retSave, 0, retSave.Length)
								fs.Flush()
							End Using
						End If
						msSave.Flush()
					End Using
				End If

				If doesExist And fAttWritable Then File.SetAttributes(sPath, fAtt)
			End If
		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
		End Try
	End Sub

    Public Function SaveAsAllSeasonPoster(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty

            If Master.eSettings.ResizeAllSPoster AndAlso (_image.Width > Master.eSettings.AllSPosterWidth OrElse _image.Height > Master.eSettings.AllSPosterHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.AllSPosterWidth, Master.eSettings.AllSPosterHeight)
            End If
            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.AllSeasonPoster, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteAllSPoster) Then
                        Save(s, Master.eSettings.AllSPosterQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.SeasonAllJPG Then
                pPath = Path.Combine(mShow.ShowPath, "season-all.jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteAllSPoster) Then
                    Save(pPath, Master.eSettings.AllSPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.SeasonAllTBN Then
                pPath = Path.Combine(mShow.ShowPath, "season-all.tbn")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteAllSPoster) Then
                    Save(pPath, Master.eSettings.AllSPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.SeasonAllPosterJPG Then
                pPath = Path.Combine(mShow.ShowPath, "season-all-poster.jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteAllSPoster) Then
                    Save(pPath, Master.eSettings.AllSPosterQuality)
                    strReturn = pPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsEpFanart(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim tPath As String = String.Empty

            If Master.eSettings.ResizeEpFanart AndAlso (_image.Width > Master.eSettings.EpFanartWidth OrElse _image.Height > Master.eSettings.EpFanartHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.EpFanartWidth, Master.eSettings.EpFanartHeight)
            End If
            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.EpisodeFanart, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpFanart) Then
                        Save(s, Master.eSettings.EpFanartQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.EpisodeDotFanart Then
                tPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), ".fanart.jpg")
                If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpFanart) Then
                    Save(tPath, Master.eSettings.EpFanartQuality)
                    strReturn = tPath
                End If
            End If

            If Master.eSettings.EpisodeDashFanart Then
                tPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), "-fanart.jpg")
                If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpFanart) Then
                    Save(tPath, Master.eSettings.EpFanartQuality)
                    strReturn = tPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsEpPoster(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty

            If Master.eSettings.ResizeEpPoster AndAlso (_image.Width > Master.eSettings.EpPosterWidth OrElse _image.Height > Master.eSettings.EpPosterHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.EpPosterWidth, Master.eSettings.EpPosterHeight)
            End If
            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.EpisodePoster, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpPoster) Then
                        Save(s, Master.eSettings.EpPosterQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.EpisodeJPG Then
                pPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), ".jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpPoster) Then
                    Save(pPath, Master.eSettings.EpPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.EpisodeTBN Then
                pPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), ".tbn")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpPoster) Then
                    Save(pPath, Master.eSettings.EpPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.EpisodeDashThumbJPG Then
                pPath = String.Concat(FileUtils.Common.RemoveExtFromPath(mShow.Filename), "-thumb.jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteEpPoster) Then
                    Save(pPath, Master.eSettings.EpPosterQuality)
                    strReturn = pPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsFanart(ByVal mMovie As Structures.DBMovie) As String
        Dim strReturn As String = String.Empty

        Try
            Dim fPath As String = String.Empty
            Dim tPath As String = String.Empty

            Try
                Dim params As New List(Of Object)(New Object() {mMovie})
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMovieFanartSave, params, _image, False)
            Catch ex As Exception
            End Try

            If Master.eSettings.ResizeFanart AndAlso (_image.Width > Master.eSettings.FanartWidth OrElse _image.Height > Master.eSettings.FanartHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.FanartWidth, Master.eSettings.FanartHeight)
            End If

            If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(mMovie.Filename) Then
                If Master.eSettings.FanartJPG Then
                    fPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "fanart.jpg")
                ElseIf Master.eSettings.MovieNameFanartJPG Then
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name), "-fanart.jpg")
                ElseIf Master.eSettings.MovieNameDotFanartJPG Then
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name), ".fanart.jpg")
                Else
                    fPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "fanart.jpg")
                End If

                If Not File.Exists(fPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                    Save(fPath, Master.eSettings.FanartQuality)
                    strReturn = fPath
                    If Master.eSettings.AutoBD AndAlso Directory.Exists(Master.eSettings.BDPath) Then
                        Save(Path.Combine(Master.eSettings.BDPath, Path.GetFileName(fPath)), Master.eSettings.FanartQuality)
                    End If
                End If
            ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(mMovie.Filename) Then
                If Master.eSettings.FanartJPG Then
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "fanart.jpg"))
                ElseIf Master.eSettings.MovieNameFanartJPG Then
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name), "-fanart.jpg")
                ElseIf Master.eSettings.MovieNameDotFanartJPG Then
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name), ".fanart.jpg")
                Else
                    fPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "fanart.jpg"))
                End If

                If Not File.Exists(fPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                    Save(fPath, Master.eSettings.FanartQuality)
                    strReturn = fPath
                    If Master.eSettings.AutoBD AndAlso Directory.Exists(Master.eSettings.BDPath) Then
                        Save(Path.Combine(Master.eSettings.BDPath, Path.GetFileName(fPath)), Master.eSettings.FanartQuality)
                    End If
                End If
            Else
                Dim tmpName As String = Path.GetFileNameWithoutExtension(mMovie.Filename)
                fPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, tmpName)

                If Master.eSettings.MovieNameDotFanartJPG AndAlso (Not mMovie.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                    If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "video_ts.fanart.jpg")
                    ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "index.fanart.jpg")
                    Else
                        tPath = String.Concat(fPath, ".fanart.jpg")
                    End If
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteFanart) Then
                        Save(tPath, Master.eSettings.FanartQuality)
                        strReturn = tPath
                        If Master.eSettings.AutoBD AndAlso Directory.Exists(Master.eSettings.BDPath) Then
                            If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            Else
                                Save(Path.Combine(Master.eSettings.BDPath, Path.GetFileName(tPath)), Master.eSettings.FanartQuality)
                            End If
                        End If
                    End If
                End If

                If Master.eSettings.MovieNameFanartJPG AndAlso (Not mMovie.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                    If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "video_ts-fanart.jpg")
                    ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "index-fanart.jpg")
                    Else
                        tPath = String.Concat(fPath, "-fanart.jpg")
                    End If

                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteFanart) Then
                        Save(tPath, Master.eSettings.FanartQuality)
                        strReturn = tPath
                        If Master.eSettings.AutoBD AndAlso Directory.Exists(Master.eSettings.BDPath) Then
                            If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            Else
                                Save(Path.Combine(Master.eSettings.BDPath, Path.GetFileName(tPath)), Master.eSettings.FanartQuality)
                            End If
                        End If
                    End If
                End If

                If Master.eSettings.FanartJPG AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "fanart.jpg")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteFanart) Then
                        Save(tPath, Master.eSettings.FanartQuality)
                        strReturn = tPath
                        If Master.eSettings.AutoBD AndAlso Directory.Exists(Master.eSettings.BDPath) Then
                            If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            Else
                                Save(Path.Combine(Master.eSettings.BDPath, String.Concat(tmpName, "-fanart.jpg")), Master.eSettings.FanartQuality)
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsPoster(ByVal mMovie As Structures.DBMovie) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty
            Try
                Dim params As New List(Of Object)(New Object() {mMovie})
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMoviePosterSave, params, _image, False)
            Catch ex As Exception
            End Try
            If Master.eSettings.ResizePoster AndAlso (_image.Width > Master.eSettings.PosterWidth OrElse _image.Height > Master.eSettings.PosterHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.PosterWidth, Master.eSettings.PosterHeight)
            End If

            If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(mMovie.Filename) Then
                With Master.eSettings
                    If .MovieNameJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name), ".jpg")
                    ElseIf .MovieJPG Then
                        pPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "movie.jpg")
                    ElseIf .FolderJPG Then
                        pPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "folder.jpg")
                    ElseIf .PosterJPG Then
                        pPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "poster.jpg")
                    ElseIf .MovieNameTBN Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name), ".tbn")
                    ElseIf .MovieNameDashPosterJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).Name), "-poster.jpg")
                    ElseIf .MovieTBN Then
                        pPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "movie.tbn")
                    ElseIf .PosterTBN Then
                        pPath = String.Concat(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName, "\", "poster.tbn")
                    Else
                        pPath = String.Empty
                    End If
                End With

                If Not pPath = String.Empty And (Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster)) Then
                    Save(pPath, Master.eSettings.PosterQuality)
                    strReturn = pPath
                End If
            ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(mMovie.Filename) Then
                With Master.eSettings
                    If .MovieNameJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name), ".jpg")
                    ElseIf .MovieJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "movie.jpg"))
                    ElseIf .FolderJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "folder.jpg"))
                    ElseIf .PosterJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "poster.jpg"))
                    ElseIf .MovieNameTBN Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name), ".tbn")
                    ElseIf .MovieNameDashPosterJPG Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).Name), "-poster.jpg")
                    ElseIf .MovieTBN Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "movie.tbn"))
                    ElseIf .PosterTBN Then
                        pPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(mMovie.Filename).FullName).FullName).FullName, "poster.tbn"))
                    Else
                        pPath = String.Empty
                    End If
                End With

                If Not pPath = String.Empty And (Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster)) Then
                    Save(pPath, Master.eSettings.PosterQuality)
                    strReturn = pPath
                End If
            Else
                Dim tPath As String = String.Empty
                Dim tmpName As String = Path.GetFileNameWithoutExtension(mMovie.Filename)
                pPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, tmpName)

                If Master.eSettings.FolderJPG AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "folder.jpg")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.PosterJPG AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "poster.jpg")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.PosterTBN AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "poster.tbn")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.MovieNameJPG AndAlso (Not mMovie.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                    If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "video_ts.jpg")
                    ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "index.jpg")
                    Else
                        tPath = String.Concat(pPath, ".jpg")
                    End If
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.MovieNameDashPosterJPG AndAlso (Not mMovie.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                    If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "video_ts-poster.jpg")
                    ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "index-poster.jpg")
                    Else
                        tPath = String.Concat(pPath, "-poster.jpg")
                    End If
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.MovieJPG AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "movie.jpg")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.MovieNameTBN AndAlso (Not mMovie.isSingle OrElse Not Master.eSettings.MovieNameMultiOnly) Then
                    If FileUtils.Common.isVideoTS(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "video_ts.tbn")
                    ElseIf FileUtils.Common.isBDRip(mMovie.Filename) Then
                        tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "index.tbn")
                    Else
                        tPath = String.Concat(pPath, ".tbn")
                    End If
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If

                If Master.eSettings.MovieTBN AndAlso mMovie.isSingle Then
                    tPath = Path.Combine(Directory.GetParent(mMovie.Filename).FullName, "movie.tbn")
                    If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
                        Save(tPath, Master.eSettings.PosterQuality)
                        strReturn = tPath
                    End If
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function
    Public Function SaveAsActorThumb(ByVal actor As MediaContainers.Person, ByVal fpath As String) As String
        Dim tPath As String = String.Empty
        tPath = Path.Combine(Path.Combine(fpath, ".actors"), String.Concat(actor.Name.Replace(" ", "_"), ".jpg"))
        If Not Directory.Exists(Path.Combine(fpath, ".actors")) Then Directory.CreateDirectory(Path.Combine(fpath, ".actors"))
        If Not File.Exists(tPath) Then ' OrElse (IsEdit OrElse Master.eSettings.OverwritePoster) Then
            Save(tPath, Master.eSettings.PosterQuality)
        End If
        Return tPath
    End Function
    Public Function SaveAsSeasonFanart(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty

            If Master.eSettings.SeasonFanartJPG OrElse Master.eSettings.SeasonDashFanart OrElse Master.eSettings.SeasonXXDashFanartJPG OrElse Master.eSettings.SeasonDotFanart Then
                If Master.eSettings.ResizeSeasonFanart AndAlso (_image.Width > Master.eSettings.SeasonFanartWidth OrElse _image.Height > Master.eSettings.SeasonFanartHeight) Then
                    ImageUtils.ResizeImage(_image, Master.eSettings.SeasonFanartWidth, Master.eSettings.SeasonFanartHeight)
                End If
                Try
                    Dim params As New List(Of Object)(New Object() {Enums.TVImageType.SeasonFanart, mShow, New List(Of String)})
                    Dim doContinue As Boolean = True
                    ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                    For Each s As String In DirectCast(params(2), List(Of String))
                        If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(s, Master.eSettings.SeasonFanartQuality)
                            If String.IsNullOrEmpty(strReturn) Then strReturn = s
                        End If
                    Next
                    If Not doContinue Then Return strReturn
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

                Dim tPath As String = String.Empty

                Try
                    tPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, mShow.TVEp.Season)
                Catch ex As Exception
                End Try

                If Not String.IsNullOrEmpty(tPath) Then

                    If Master.eSettings.SeasonDotFanart Then
                        pPath = Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".fanart.jpg"))
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(pPath, Master.eSettings.SeasonFanartQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonDashFanart Then
                        pPath = Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), "-fanart.jpg"))
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(pPath, Master.eSettings.SeasonFanartQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonFanartJPG Then
                        pPath = Path.Combine(tPath, "Fanart.jpg")
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(pPath, Master.eSettings.SeasonFanartQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonXXDashFanartJPG Then
                        If mShow.TVEp.Season = 0 Then
                            pPath = Path.Combine(mShow.ShowPath, "season-specials-fanart.jpg")
                        Else
                            pPath = Path.Combine(mShow.ShowPath, String.Format("season{0}-fanart.jpg", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0"))))
                        End If
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(pPath, Master.eSettings.SeasonFanartQuality)
                            strReturn = pPath
                        End If
                    End If

                Else
                    If Master.eSettings.SeasonXXDashFanartJPG Then
                        If mShow.TVEp.Season = 0 Then
                            pPath = Path.Combine(mShow.ShowPath, "season-specials-fanart.jpg")
                        Else
                            pPath = Path.Combine(mShow.ShowPath, String.Format("season{0}-fanart.jpg", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0"))))
                        End If
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonFanart) Then
                            Save(pPath, Master.eSettings.SeasonFanartQuality)
                            strReturn = pPath
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsSeasonPoster(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty

            If Master.eSettings.ResizeSeasonPoster AndAlso (_image.Width > Master.eSettings.SeasonPosterWidth OrElse _image.Height > Master.eSettings.SeasonPosterHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.SeasonPosterWidth, Master.eSettings.SeasonPosterHeight)
            End If

            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.SeasonPoster, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                        Save(s, Master.eSettings.SeasonPosterQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.SeasonPosterTBN OrElse Master.eSettings.SeasonPosterJPG OrElse Master.eSettings.SeasonNameTBN OrElse _
            Master.eSettings.SeasonNameJPG OrElse Master.eSettings.FolderJPG Then
                Dim tPath As String = String.Empty


                Try
                    tPath = Functions.GetSeasonDirectoryFromShowPath(mShow.ShowPath, mShow.TVEp.Season)
                Catch
                End Try

                If Not String.IsNullOrEmpty(tPath) Then
                    If Master.eSettings.SeasonPosterTBN Then
                        pPath = Path.Combine(tPath, "Poster.tbn")
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                            Save(pPath, Master.eSettings.SeasonPosterQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonPosterJPG Then
                        pPath = Path.Combine(tPath, "Poster.jpg")
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                            Save(pPath, Master.eSettings.SeasonPosterQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonNameTBN Then
                        pPath = Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".tbn"))
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                            Save(pPath, Master.eSettings.SeasonPosterQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonNameJPG Then
                        pPath = Path.Combine(tPath, String.Concat(FileUtils.Common.GetDirectory(tPath), ".jpg"))
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                            Save(pPath, Master.eSettings.SeasonPosterQuality)
                            strReturn = pPath
                        End If
                    End If

                    If Master.eSettings.SeasonFolderJPG Then
                        pPath = Path.Combine(tPath, "Folder.jpg")
                        If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                            Save(pPath, Master.eSettings.SeasonPosterQuality)
                            strReturn = pPath
                        End If
                    End If
                End If
            End If

            If Master.eSettings.SeasonX Then
                If mShow.TVEp.Season = 0 Then
                    pPath = Path.Combine(mShow.ShowPath, "season-specials.tbn")
                Else
                    pPath = Path.Combine(mShow.ShowPath, String.Format("season{0}.tbn", mShow.TVEp.Season))
                End If
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                    Save(pPath, Master.eSettings.SeasonPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.SeasonXX Then
                If mShow.TVEp.Season = 0 Then
                    pPath = Path.Combine(mShow.ShowPath, "season-specials.tbn")
                Else
                    pPath = Path.Combine(mShow.ShowPath, String.Format("season{0}.tbn", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0"))))
                End If
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                    Save(pPath, Master.eSettings.SeasonPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.SeasonXXDashPosterJPG Then
                If mShow.TVEp.Season = 0 Then
                    pPath = Path.Combine(mShow.ShowPath, "season-specials-poster.jpg")
                Else
                    pPath = Path.Combine(mShow.ShowPath, String.Format("season{0}-poster.jpg", mShow.TVEp.Season.ToString.PadLeft(2, Convert.ToChar("0"))))
                End If
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteSeasonPoster) Then
                    Save(pPath, Master.eSettings.SeasonPosterQuality)
                    strReturn = pPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsShowFanart(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim tPath As String = String.Empty

            If Master.eSettings.ResizeShowFanart AndAlso (_image.Width > Master.eSettings.ShowFanartWidth OrElse _image.Height > Master.eSettings.ShowFanartHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.ShowFanartWidth, Master.eSettings.ShowFanartHeight)
            End If

            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.ShowFanart, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowFanart) Then
                        Save(s, Master.eSettings.ShowFanartQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.ShowDotFanart Then
                tPath = Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), ".fanart.jpg"))
                If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowFanart) Then
                    Save(tPath, Master.eSettings.ShowFanartQuality)
                    strReturn = tPath
                End If
            End If

            If Master.eSettings.ShowDashFanart Then
                tPath = Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), "-fanart.jpg"))
                If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowFanart) Then
                    Save(tPath, Master.eSettings.ShowFanartQuality)
                    strReturn = tPath
                End If
            End If

            If Master.eSettings.ShowFanartJPG Then
                tPath = Path.Combine(mShow.ShowPath, "fanart.jpg")
                If Not File.Exists(tPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowFanart) Then
                    Save(tPath, Master.eSettings.ShowFanartQuality)
                    strReturn = tPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Function SaveAsShowPoster(ByVal mShow As Structures.DBTV) As String
        Dim strReturn As String = String.Empty

        Try
            Dim pPath As String = String.Empty

            If Master.eSettings.ResizeShowPoster AndAlso (_image.Width > Master.eSettings.ShowPosterWidth OrElse _image.Height > Master.eSettings.ShowPosterHeight) Then
                ImageUtils.ResizeImage(_image, Master.eSettings.ShowPosterWidth, Master.eSettings.ShowPosterHeight)
            End If

            Try
                Dim params As New List(Of Object)(New Object() {Enums.TVImageType.ShowPoster, mShow, New List(Of String)})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.TVImageNaming, params, doContinue)
                For Each s As String In DirectCast(params(2), List(Of String))
                    If Not File.Exists(s) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                        Save(s, Master.eSettings.ShowPosterQuality)
                        If String.IsNullOrEmpty(strReturn) Then strReturn = s
                    End If
                Next
                If Not doContinue Then Return strReturn
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Master.eSettings.ShowPosterJPG Then
                pPath = Path.Combine(mShow.ShowPath, "poster.jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                    Save(pPath, Master.eSettings.ShowPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.ShowPosterTBN Then
                pPath = Path.Combine(mShow.ShowPath, "poster.tbn")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                    Save(pPath, Master.eSettings.ShowPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.ShowFolderJPG Then
                pPath = Path.Combine(mShow.ShowPath, "folder.jpg")
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                    Save(pPath, Master.eSettings.ShowPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.ShowJPG Then
                pPath = Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), ".jpg"))
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                    Save(pPath, Master.eSettings.ShowPosterQuality)
                    strReturn = pPath
                End If
            End If

            If Master.eSettings.ShowTBN Then
                pPath = Path.Combine(mShow.ShowPath, String.Concat(FileUtils.Common.GetDirectory(mShow.ShowPath), ".tbn"))
                If Not File.Exists(pPath) OrElse (IsEdit OrElse Master.eSettings.OverwriteShowPoster) Then
                    Save(pPath, Master.eSettings.ShowPosterQuality)
                    strReturn = pPath
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return strReturn
    End Function

    Public Sub SaveFAasET(ByVal faPath As String, ByVal inPath As String)
        Dim iMod As Integer = 0
        Dim iVal As Integer = 1
        Dim extraPath As String = String.Empty

        If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(inPath) Then
            extraPath = Path.Combine(Directory.GetParent(Directory.GetParent(inPath).FullName).FullName, "extrathumbs")
        ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(inPath) Then
            extraPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(inPath).FullName).FullName).FullName, "extrathumbs")
        Else
            extraPath = Path.Combine(Directory.GetParent(inPath).FullName, "extrathumbs")
        End If

        iMod = Functions.GetExtraModifier(extraPath)
        iVal = iMod + 1

        If Not Directory.Exists(extraPath) Then
            Directory.CreateDirectory(extraPath)
        End If

        FileUtils.Common.MoveFileWithStream(faPath, Path.Combine(extraPath, String.Concat("thumb", iVal, ".jpg")))
    End Sub

    Private Shared Function GetEncoderInfo(ByVal Format As ImageFormat) As ImageCodecInfo
        Dim Encoders() As ImageCodecInfo = ImageCodecInfo.GetImageEncoders()

        For i As Integer = 0 To UBound(Encoders)
            If Encoders(i).FormatID = Format.Guid Then
                Return Encoders(i)
            End If
        Next

        Return Nothing
    End Function

#End Region 'Methods

End Class