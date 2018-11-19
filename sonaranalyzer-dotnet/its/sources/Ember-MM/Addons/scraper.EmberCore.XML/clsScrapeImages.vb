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
Imports EmberAPI

Public Class ScrapeImages

#Region "Methods"

    Public Shared Sub GetPreferredFAasET(ByVal thumbs As List(Of XMLScraper.MediaTags.Thumbnail), ByVal IMDBID As String, ByVal sPath As String)
        Dim _Image As Image

        Dim ETHashes As New List(Of String)

        Dim CachePath As String = String.Concat(Master.TempPath, Path.DirectorySeparatorChar, IMDBID, Path.DirectorySeparatorChar, "fanart")

        If Master.eSettings.AutoET Then
            ETHashes = HashFile.CurrentETHashes(sPath)
        End If

        If Master.eSettings.UseImgCacheUpdaters Then
            Dim lFi As New List(Of FileInfo)

            If Not Directory.Exists(CachePath) Then
                Directory.CreateDirectory(CachePath)
            Else
                Dim di As New DirectoryInfo(CachePath)

                Try
                    lFi.AddRange(di.GetFiles("*.jpg"))
                Catch
                End Try
            End If

            If lFi.Count > 0 Then
                For Each sFile As FileInfo In lFi
                    Select Case True
                        Case sFile.Name.Contains("(original)")
                            If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Lrg Then
                                If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                    SaveFAasET(sFile.FullName, sPath)
                                End If
                            End If
                        Case sFile.Name.Contains("(mid)")
                            If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Mid Then
                                If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                    SaveFAasET(sFile.FullName, sPath)
                                End If
                            End If
                        Case sFile.Name.Contains("(thumb)")
                            If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Small Then
                                If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                    SaveFAasET(sFile.FullName, sPath)
                                End If
                            End If
                    End Select
                Next
            Else
                If thumbs.Count > 0 Then

                    'setup fanart for nfo
                    For Each miFanart As XMLScraper.MediaTags.Thumbnail In thumbs
                        Dim Fanart As New Images
                        Fanart.FromWeb(miFanart.Url)
                        If Not IsNothing(Fanart.Image) Then
                            _Image = Fanart.Image
                            Dim savePath As String = Path.Combine(CachePath, String.Concat("fanart_(url=", StringUtils.CleanURL(miFanart.Url), ").jpg"))
                            Save(_Image, savePath)
                            If Master.eSettings.AutoET Then
                                If Not ETHashes.Contains(HashFile.HashCalcFile(savePath)) Then
                                    SaveFAasET(savePath, sPath)
                                End If
                            End If
                        End If
                    Next
                End If
            End If
        Else
            If thumbs.Count > 0 Then

                If Not Directory.Exists(CachePath) Then
                    Directory.CreateDirectory(CachePath)
                End If

                Dim savePath As String
                For Each miFanart As XMLScraper.MediaTags.Thumbnail In thumbs
                    If Master.eSettings.AutoETSize = Enums.FanartSize.Lrg Then
                        Dim Fanart As New Images
                        Fanart.FromWeb(miFanart.Url)
                        If Not IsNothing(Fanart.Image) Then
                            _Image = Fanart.Image
                            savePath = Path.Combine(CachePath, String.Concat("fanart_(url=", StringUtils.CleanURL(miFanart.Url), ").jpg"))
                            Save(_Image, savePath)
                            If Not ETHashes.Contains(HashFile.HashCalcFile(savePath)) Then
                                SaveFAasET(savePath, sPath)
                            End If
                        End If
                    End If
                    'Me.Clear()
                Next

                FileUtils.Delete.DeleteDirectory(CachePath)
            End If
        End If
    End Sub

    Public Shared Sub SaveFAasET(ByVal faPath As String, ByVal inPath As String)
        Dim iMod, iVal As Integer        
        Dim extraPath As String

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


    Public Shared Sub Save(ByVal _image As Image, ByVal sPath As String, Optional ByVal iQuality As Long = 0)
        Try
            If IsNothing(_image) Then Exit Sub

            Dim doesExist As Boolean = File.Exists(sPath)
            Dim fAtt As New FileAttributes
            If Not String.IsNullOrEmpty(sPath) AndAlso (Not doesExist OrElse (Not CBool(File.GetAttributes(sPath) And FileAttributes.ReadOnly))) Then
                If doesExist Then
                    'get the current attributes to set them back after writing
                    fAtt = File.GetAttributes(sPath)
                    'set attributes to none for writing
                    File.SetAttributes(sPath, FileAttributes.Normal)
                End If

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

                    Using fs As New FileStream(sPath, FileMode.Create, FileAccess.Write)
                        fs.Write(retSave, 0, retSave.Length)
                        fs.Flush()
                    End Using
                    msSave.Flush()
                End Using

                If doesExist Then File.SetAttributes(sPath, fAtt)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
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