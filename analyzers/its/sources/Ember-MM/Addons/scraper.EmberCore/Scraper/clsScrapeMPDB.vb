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
Imports EmberAPI

Namespace MPDB

    Public Class Scraper

        #Region "Fields"

        Public IMDBURL As String

        Friend  WithEvents bwMPDB As New System.ComponentModel.BackgroundWorker

        #End Region 'Fields

        #Region "Events"

        Public Event PostersDownloaded(ByVal Posters As List(Of MediaContainers.Image))

        Public Event ProgressUpdated(ByVal iPercent As Integer)

        #End Region 'Events

        #Region "Methods"

        Public Sub Cancel()
            If Me.bwMPDB.IsBusy Then Me.bwMPDB.CancelAsync()

            While Me.bwMPDB.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End Sub

        Public Sub GetImagesAsync(ByVal imdbID As String)
            Try
                If Not Me.bwMPDB.IsBusy Then
                    Me.bwMPDB.WorkerSupportsCancellation = True
                    Me.bwMPDB.WorkerReportsProgress = True
                    Me.bwMPDB.RunWorkerAsync(New Arguments With {.Parameter = imdbID})
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Sub

        Public Function GetMPDBPosters(ByVal imdbID As String) As List(Of MediaContainers.Image)
            Dim alPosters As New List(Of MediaContainers.Image)

            If Me.bwMPDB.CancellationPending Then Return Nothing

            Try
                Dim sHTTP As New HTTP
                Dim HTML As String = sHTTP.DownloadData(String.Concat("http://www.movieposterdb.com/movie/", imdbID))
                sHTTP = Nothing

                If Me.bwMPDB.CancellationPending Then Return Nothing

                If bwMPDB.WorkerReportsProgress Then
                    bwMPDB.ReportProgress(1)
                End If

                If Regex.IsMatch(HTML, String.Concat("http://www.imdb.com/title/tt", imdbID), RegexOptions.Singleline Or RegexOptions.IgnoreCase Or RegexOptions.Multiline) Then
                    Dim mcPoster As MatchCollection = Regex.Matches(HTML, "http://www.movieposterdb.com/posters/[0-9_](.*?)/[0-9](.*?)/[0-9](.*?)/[a-z0-9_](.*?).jpg")

                    Dim PosterURL As String = String.Empty

                    For Each mPoster As Match In mcPoster
                        If Me.bwMPDB.CancellationPending Then Return Nothing
                        PosterURL = mPoster.Value.Remove(mPoster.Value.LastIndexOf("/") + 1, 1)
                        PosterURL = PosterURL.Insert(mPoster.Value.LastIndexOf("/") + 1, "l")
                        alPosters.Add(New MediaContainers.Image With {.Description = "poster", .URL = PosterURL})
                    Next
                End If
                If bwMPDB.WorkerReportsProgress Then
                    bwMPDB.ReportProgress(3)
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            Return alPosters
        End Function

        Private Sub bwMPDB_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwMPDB.DoWork
            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Try
                e.Result = GetMPDBPosters(Args.Parameter)
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                e.Result = Nothing
            End Try
        End Sub

        Private Sub bwMPDB_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwMPDB.ProgressChanged
            If Not bwMPDB.CancellationPending Then
                RaiseEvent ProgressUpdated(e.ProgressPercentage)
            End If
        End Sub

        Private Sub bwMPDB_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwMPDB.RunWorkerCompleted
            If Not IsNothing(e.Result) Then
                RaiseEvent PostersDownloaded(DirectCast(e.Result, List(Of MediaContainers.Image)))
            End If
        End Sub

        #End Region 'Methods

        #Region "Nested Types"

        Private Structure Arguments

            #Region "Fields"

            Dim Parameter As String

            #End Region 'Fields

        End Structure

        Private Structure Results

            #Region "Fields"

            Dim Result As Object
            Dim ResultList As List(Of MediaContainers.Image)

            #End Region 'Fields

        End Structure

        #End Region 'Nested Types

    End Class

End Namespace

