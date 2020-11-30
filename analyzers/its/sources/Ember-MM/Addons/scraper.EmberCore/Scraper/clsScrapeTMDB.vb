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

Namespace TMDB

    Public Class Scraper

#Region "Fields"

        Public IMDBURL As String

        Friend WithEvents bwTMDB As New System.ComponentModel.BackgroundWorker

        Private Const APIKey As String = "b1ecff8c76278262b27de1569f57f6bd"

#End Region 'Fields

#Region "Events"

        Public Event PostersDownloaded(ByVal Posters As List(Of MediaContainers.Image))

        Public Event ProgressUpdated(ByVal iPercent As Integer)

#End Region 'Events

#Region "Methods"

        Public Sub Cancel()
            If bwTMDB.IsBusy Then bwTMDB.CancelAsync()

            While bwTMDB.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End Sub

        Public Sub GetImagesAsync(ByVal imdbID As String, ByVal sType As String)
            Try
                If Not bwTMDB.IsBusy Then
                    bwTMDB.WorkerSupportsCancellation = True
                    bwTMDB.WorkerReportsProgress = True
                    bwTMDB.RunWorkerAsync(New Arguments With {.Parameter = imdbID, .sType = sType})
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Sub

        Public Function GetTMDBImages(ByVal imdbID As String, ByVal sType As String) As List(Of MediaContainers.Image)
            Dim alPosters As New List(Of MediaContainers.Image)
            Dim xmlTMDB As XDocument
            Dim sHTTP As New HTTP

            If bwTMDB.CancellationPending Then Return Nothing
            Try
                Dim ApiXML As String = sHTTP.DownloadData(String.Format("http://api.themoviedb.org/2.1/Movie.getImages/en/xml/{0}/tt{1}", APIKey, imdbID))

                If Not String.IsNullOrEmpty(ApiXML) Then
                    Try
                        xmlTMDB = XDocument.Parse(ApiXML)
                    Catch
                        Return alPosters
                    End Try

                    If bwTMDB.WorkerReportsProgress Then
                        bwTMDB.ReportProgress(1)
                    End If

                    If bwTMDB.CancellationPending Then Return Nothing

                    If Not xmlTMDB...<OpenSearchDescription>...<movies>.Value = "Nothing found." Then
                        If sType = "poster" Then
                            Dim tmdbImages = From iNode In xmlTMDB...<OpenSearchDescription>...<movies>...<movie>...<images>...<poster>.Elements Select iNode
                            If tmdbImages.Count > 0 Then
                                For Each tmdbI As XElement In tmdbImages
                                    Dim parentID As String = tmdbI.Parent.Attribute("id").Value
                                    If bwTMDB.CancellationPending Then Return Nothing
                                    Dim tmpPoster As New MediaContainers.Image With {.URL = tmdbI.@url, .Description = tmdbI.@size, .Width = tmdbI.@width, .Height = tmdbI.@height, .ParentID = parentID}
                                    alPosters.Add(tmpPoster)
                                Next
                            End If
                        ElseIf sType = "backdrop" Then
                            Dim tmdbImages = From iNode In xmlTMDB...<OpenSearchDescription>...<movies>...<movie>...<images>...<backdrop>.Elements Select iNode
                            If tmdbImages.Count > 0 Then
                                For Each tmdbI As XElement In tmdbImages
                                    Dim parentID As String = tmdbI.Parent.Attribute("id").Value
                                    If bwTMDB.CancellationPending Then Return Nothing
                                    Dim tmpPoster As New MediaContainers.Image With {.URL = tmdbI.@url, .Description = tmdbI.@size, .Width = tmdbI.@width, .Height = tmdbI.@height, .ParentID = parentID}
                                    alPosters.Add(tmpPoster)
                                Next
                            End If
                        End If
                    End If
                End If

                If bwTMDB.WorkerReportsProgress Then
                    bwTMDB.ReportProgress(2)
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            sHTTP = Nothing

            Return alPosters
        End Function

        Public Function GetTrailers(ByVal imdbID As String) As String
            Dim xmlTMDB As XDocument
            Dim sHTTP As New HTTP
            Dim tLang As String

            tLang = AdvancedSettings.GetSetting("UseTMDBTrailerPref", "en")

            If bwTMDB.CancellationPending Then Return Nothing
            Try
                Dim ApiXML As String = sHTTP.DownloadData(String.Format("http://api.themoviedb.org/2.1/Movie.imdbLookup/en/xml/{0}/tt{1}", APIKey, imdbID))
                sHTTP = Nothing

                If Not String.IsNullOrEmpty(ApiXML) Then
                    Try
                        xmlTMDB = XDocument.Parse(ApiXML)
                    Catch
                        Return String.Empty
                    End Try

                    If bwTMDB.WorkerReportsProgress Then
                        bwTMDB.ReportProgress(1)
                    End If
                    If bwTMDB.CancellationPending Then Return Nothing

                    Dim tmdbNode = From xNode In xmlTMDB.Elements

                    If tmdbNode.Count > 0 Then
                        If Not tmdbNode(0).Value = "Your query didn't return any results." Then
                            Dim movieID As String = xmlTMDB...<OpenSearchDescription>...<movies>...<movie>...<id>.Value
                            Dim i As Integer


                            For i = 0 To 1 Step 1
                                sHTTP = New HTTP
                                ApiXML = sHTTP.DownloadData(String.Format("http://api.themoviedb.org/2.1/Movie.getInfo/{0}/xml/{1}/{2}", tLang, APIKey, movieID))
                                sHTTP = Nothing

                                If Not String.IsNullOrEmpty(ApiXML) Then

                                    Try
                                        xmlTMDB = XDocument.Parse(ApiXML)
                                    Catch
                                        Return String.Empty
                                    End Try

                                    If bwTMDB.WorkerReportsProgress Then
                                        bwTMDB.ReportProgress(2)
                                    End If

                                    If bwTMDB.CancellationPending Then Return Nothing

                                    Dim Trailers = From tNode In xmlTMDB...<OpenSearchDescription>...<movies>...<movie> Select tNode.<trailer>
                                    If Trailers.Count > 0 AndAlso Not String.IsNullOrEmpty(Trailers(0).Value) Then
                                        If Trailers(0).Value.ToLower.IndexOf("youtube.com") > 0 Then
                                            Return Trailers(0).Value
                                            i += 1
                                        End If
                                    Else
                                        tLang = "en"
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If
                If bwTMDB.WorkerReportsProgress Then
                    bwTMDB.ReportProgress(3)
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            Return String.Empty
        End Function

        Private Sub bwTMDB_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwTMDB.DoWork
            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Try
                e.Result = GetTMDBImages(Args.Parameter, Args.sType)
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                e.Result = Nothing
            End Try
        End Sub

        Private Sub bwTMDB_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwTMDB.ProgressChanged
            If Not bwTMDB.CancellationPending Then
                RaiseEvent ProgressUpdated(e.ProgressPercentage)
            End If
        End Sub

        Private Sub bwTMDB_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwTMDB.RunWorkerCompleted
            If Not IsNothing(e.Result) Then
                RaiseEvent PostersDownloaded(DirectCast(e.Result, List(Of MediaContainers.Image)))
            End If
        End Sub

#End Region 'Methods

#Region "Nested Types"

        Private Structure Arguments

#Region "Fields"

            Dim Parameter As String
            Dim sType As String

#End Region 'Fields

        End Structure

#End Region 'Nested Types

    End Class

End Namespace