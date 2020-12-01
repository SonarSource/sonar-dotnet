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
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports EmberAPI

Public Class Trailers

#Region "Fields"

    Public IMDBURL As String

    Private WebPage As New HTTP
    Private _ImdbID As String = String.Empty
    Private _ImdbTrailerPage As String = String.Empty
    Private _TrailerList As New List(Of String)    

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Me.Clear()
        AddHandler WebPage.ProgressUpdated, AddressOf DownloadProgressUpdated
    End Sub

#End Region 'Constructors

#Region "Events"

    Public Event ProgressUpdated(ByVal iPercent As Integer)

#End Region 'Events

#Region "Methods"

    Public Sub Cancel()
        Me.WebPage.Cancel()
    End Sub

    Public Sub Clear()
        Me._TrailerList.Clear()
        Me._ImdbID = String.Empty
        Me._ImdbTrailerPage = String.Empty
    End Sub

    Public Sub DeleteTrailers(ByVal sPath As String, ByVal NewTrailer As String)
        Dim parPath As String = Directory.GetParent(sPath).FullName
        Dim tmpName As String = Path.Combine(parPath, StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath)))
        Dim tmpNameNoStack As String = Path.Combine(parPath, Path.GetFileNameWithoutExtension(sPath))
        For Each t As String In Master.eSettings.ValidExts
            If File.Exists(String.Concat(tmpName, "-trailer", t)) AndAlso Not String.Concat(tmpName, "-trailer", t).ToLower = NewTrailer.ToLower Then
                File.Delete(String.Concat(tmpName, "-trailer", t))
            ElseIf File.Exists(String.Concat(tmpName, "[trailer]", t)) AndAlso Not String.Concat(tmpName, "[trailer]", t).ToLower = NewTrailer.ToLower Then
                File.Delete(String.Concat(tmpName, "[trailer]", t))
            ElseIf File.Exists(String.Concat(tmpNameNoStack, "-trailer", t)) AndAlso Not String.Concat(tmpNameNoStack, "-trailer", t).ToLower = NewTrailer.ToLower Then
                File.Delete(String.Concat(tmpNameNoStack, "-trailer", t))
            ElseIf File.Exists(String.Concat(tmpNameNoStack, "[trailer]", t)) AndAlso Not String.Concat(tmpNameNoStack, "[trailer]", t).ToLower = NewTrailer.ToLower Then
                File.Delete(String.Concat(tmpNameNoStack, "[trailer]", t))
            End If
        Next
    End Sub

    Public Sub DownloadProgressUpdated(ByVal iPercent As Integer)
        RaiseEvent ProgressUpdated(iPercent)
    End Sub

    Public Function DownloadSingleTrailer(ByVal sPath As String, ByVal ImdbID As String, ByVal isSingle As Boolean, ByVal currNfoTrailer As String) As String
        Dim tURL As String = String.Empty
        Try
            Me._TrailerList.Clear()

            If Not Master.eSettings.UpdaterTrailersNoDownload AndAlso IsAllowedToDownload(sPath, isSingle, currNfoTrailer, True) Then
                Me.GetTrailers(ImdbID, True)

                If Me._TrailerList.Count > 0 Then

                    Dim tLink As String = String.Empty
                    If Regex.IsMatch(Me._TrailerList.Item(0).ToString, "http:\/\/.*youtube.*\/watch\?v=(.{11})&?.*") Then
                        Dim YT As New YouTube.Scraper
                        YT.GetVideoLinks(Me._TrailerList.Item(0).ToString)
                        If YT.VideoLinks.ContainsKey(Master.eSettings.PreferredTrailerQuality) Then
                            tLink = YT.VideoLinks(Master.eSettings.PreferredTrailerQuality).URL
                        Else
                            Select Case Master.eSettings.PreferredTrailerQuality
                                Case Enums.TrailerQuality.HD1080p
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HD720p) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HD720p).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HQFLV).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQMP4) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQMP4).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQFLV).URL
                                    End If
                                Case Enums.TrailerQuality.HD1080pVP8
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HD720pVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HD720pVP8).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HQVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HQVP8).URL                                    
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQVP8).URL
                                    End If
                                Case Enums.TrailerQuality.HD720p
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HQFLV).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQMP4) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQMP4).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQFLV).URL
                                    End If
                                Case Enums.TrailerQuality.HD720pVP8                                
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.HQVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.HQVP8).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQVP8).URL
                                    End If
                                Case Enums.TrailerQuality.HQFLV
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQMP4) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQMP4).URL
                                    ElseIf YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQFLV).URL
                                    End If
                                Case Enums.TrailerQuality.HQVP8
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQVP8) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQVP8).URL
                                    End If
                                Case Enums.TrailerQuality.SQMP4
                                    If YT.VideoLinks.ContainsKey(Enums.TrailerQuality.SQFLV) Then
                                        tLink = YT.VideoLinks(Enums.TrailerQuality.SQFLV).URL
                                    End If
                                Case Enums.TrailerQuality.SQFLV
                                    tLink = String.Empty
                                Case Enums.TrailerQuality.SQVP8
                                    tLink = String.Empty
                            End Select
                        End If
                    Else
                        tLink = Me._TrailerList.Item(0).ToString
                    End If

                    If Not String.IsNullOrEmpty(tLink) Then
                        tURL = WebPage.DownloadFile(tLink, sPath, False, "trailer")
                        If Not String.IsNullOrEmpty(tURL) Then
                            'delete any other trailer if enabled in settings and download successful
                            If Master.eSettings.DeleteAllTrailers Then
                                Me.DeleteTrailers(sPath, tURL)
                            End If
                        End If
                    End If
                End If
            ElseIf Master.eSettings.UpdaterTrailersNoDownload AndAlso IsAllowedToDownload(sPath, isSingle, currNfoTrailer, False) Then
                Me.GetTrailers(ImdbID, True)

                If Me._TrailerList.Count > 0 Then
                    tURL = Me._TrailerList.Item(0).ToString
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return tURL
    End Function

    Public Function DownloadTrailer(ByVal sPath As String, ByVal sURL As String) As String
        Dim tURL As String = String.Empty
        If Not String.IsNullOrEmpty(sURL) Then
            tURL = WebPage.DownloadFile(sURL, sPath, True, "trailer")

            If Not String.IsNullOrEmpty(tURL) Then
                'delete any other trailer if enabled in settings and download successful
                If Master.eSettings.DeleteAllTrailers Then
                    Me.DeleteTrailers(sPath, tURL)
                End If
            End If
        End If
        Return tURL
    End Function

    Public Function GetTrailers(ByVal ImdbID As String, Optional ByVal BreakAfterFound As Boolean = True) As List(Of String)
        Me._ImdbID = ImdbID
        If AdvancedSettings.GetBooleanSetting("UseIMDBTrailer", False) Then
            Me.GetImdbTrailer()
        End If
        If AdvancedSettings.GetBooleanSetting("UseTMDBTrailer", False) Then
            Me.GetTMDBTrailer()
        End If

        Return Me._TrailerList
    End Function

    Public Function IsAllowedToDownload(ByVal sPath As String, ByVal isDL As Boolean, ByVal currNfoTrailer As String, Optional ByVal isSS As Boolean = False) As Boolean
        Dim fScanner As New Scanner

        If isDL Then
            If String.IsNullOrEmpty(fScanner.GetTrailerPath(sPath)) OrElse Master.eSettings.OverwriteTrailer Then
                Return True
            Else
                If isSS AndAlso String.IsNullOrEmpty(fScanner.GetTrailerPath(sPath)) Then
                    If String.IsNullOrEmpty(currNfoTrailer) OrElse Not Master.eSettings.LockTrailer Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
            End If
        Else
            If String.IsNullOrEmpty(currNfoTrailer) OrElse Not Master.eSettings.LockTrailer Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Private Sub GetIMDBTrailer()
        Dim IMDB As New IMDB.Scraper        
        IMDB.IMDBURL = AdvancedSettings.GetSetting("IMDBURL", "akas.imdb.com")
        Dim results As List(Of String) = IMDB.GetTrailers(_ImdbID)

        Me._TrailerList.AddRange(results)

        IMDB = Nothing
    End Sub

    Private Sub GetTMDBTrailer()
        Dim TMDB As New TMDB.Scraper
        Dim YT As String = TMDB.GetTrailers(_ImdbID)

        If Not String.IsNullOrEmpty(YT) Then
            Me._TrailerList.Add(YT)
        End If

        TMDB = Nothing
    End Sub

#End Region 'Methods

End Class