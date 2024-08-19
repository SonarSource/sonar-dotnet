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
Imports EmberAPI

Public Class Trailers

#Region "Fields"

    Private WebPage As New HTTP

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        AddHandler WebPage.ProgressUpdated, AddressOf DownloadProgressUpdated
    End Sub

#End Region 'Constructors

#Region "Events"

    Public Event ProgressUpdated(ByVal iPercent As Integer)

#End Region 'Events

#Region "Methods"

    Public Sub Cancel()
        WebPage.Cancel()
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

    Public Function DownloadSingleTrailer(ByVal sPath As String, ByVal trailerlist As List(Of String), ByVal isSingle As Boolean, ByVal currNfoTrailer As String) As String
        Dim tURL As String = String.Empty
        Try
            If Not Master.eSettings.UpdaterTrailersNoDownload AndAlso IsAllowedToDownload(sPath, isSingle, currNfoTrailer, True) Then
                If trailerlist.Count > 0 Then
                    Dim tLink As String = trailerlist.Item(0).ToString
                    If Not String.IsNullOrEmpty(tLink) Then
                        tURL = WebPage.DownloadFile(tLink, sPath, False, "trailer")
                        If Not String.IsNullOrEmpty(tURL) Then
                            'delete any other trailer if enabled in settings and download successful
                            If Master.eSettings.DeleteAllTrailers Then
                                DeleteTrailers(sPath, tURL)
                            End If
                        End If
                    End If
                End If
            ElseIf Master.eSettings.UpdaterTrailersNoDownload AndAlso IsAllowedToDownload(sPath, isSingle, currNfoTrailer, False) Then
                If trailerlist.Count > 0 Then
                    tURL = trailerlist.Item(0).ToString
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
                    DeleteTrailers(sPath, tURL)
                End If
            End If
        End If
        Return tURL
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

#End Region 'Methods

End Class