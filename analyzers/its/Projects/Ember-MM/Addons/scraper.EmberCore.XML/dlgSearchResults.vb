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

Imports System.Text.RegularExpressions
Imports EmberScraperModule.XMLScraper.ScraperXML
Imports System.IO
Imports System.Text
Imports EmberAPI

Public Class dlgSearchResults

#Region "Fields"
    Friend WithEvents bwDownloadInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwSearchInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents tmrLoad As New System.Windows.Forms.Timer
    Friend WithEvents tmrWait As New System.Windows.Forms.Timer

    Private XMLManager As ScraperManager = Nothing
    Private lMediaTag As XMLScraper.MediaTags.MovieTag ' XMLScraper.MediaTags.MediaTag
    Private sHTTP As New HTTP
    Private _currnode As Integer = -1
    Private _prevnode As Integer = -2
    Private mList As List(Of XMLScraper.ScraperLib.ScrapeResultsEntity)
    Private _scrapername As String
#End Region 'Fields

#Region "Methods"


    Public Overloads Function ShowDialog(ByVal Res As List(Of XMLScraper.ScraperLib.ScrapeResultsEntity), ByVal sMovieTitle As String, ByVal scrapername As String, ByVal ScraperThumb As String, ByVal xmlmanager As ScraperManager) As XMLScraper.MediaTags.MovieTag
        '//
        ' Overload to pass data
        '\\

        Me.Text = String.Concat(Master.eLang.GetString(7, "Search Results - "), sMovieTitle)
        If Not String.IsNullOrEmpty(ScraperThumb) Then Me.pbScraperLogo.Load(ScraperThumb)

        Me.XMLManager = xmlmanager

        Me.mList = Res
        Me._scrapername = scrapername
        SearchResultsDownloaded()

        If MyBase.ShowDialog = Windows.Forms.DialogResult.OK Then
            Return lMediaTag
        Else
            Return Nothing
        End If
    End Function

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If Not String.IsNullOrEmpty(Me.txtSearch.Text) Then
            Me.OK_Button.Enabled = False
            Me.ClearInfo()
            Me.Label3.Text = Master.eLang.GetString(5, "Searching...")
            Me.pnlLoading.Visible = True
            Me.txtSearch.Enabled = False
            Me.btnSearch.Enabled = False
            Me.tvResults.Enabled = False
            bwSearchInfo.RunWorkerAsync(txtSearch.Text)
        End If
    End Sub
    Private Sub bwSearchInfo_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwSearchInfo.DoWork
        mList = XMLManager.GetResults(_scrapername, e.Argument.ToString, String.Empty, XMLScraper.ScraperLib.MediaType.movie)
    End Sub
    Private Sub bwSearchInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwSearchInfo.RunWorkerCompleted
        SearchResultsDownloaded()
    End Sub
    Private Sub bwDownloadInfo_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadInfo.DoWork
        Try
            lMediaTag = Nothing
            Dim idx As Integer = Convert.ToInt32(e.Argument) 'Me.tvResults.SelectedNode.Tag)
            lMediaTag = DirectCast(XMLManager.GetDetails(mList(idx)), XMLScraper.MediaTags.MovieTag)
            If Not lMediaTag.Thumbs Is Nothing AndAlso lMediaTag.Thumbs.Count > 0 Then
                sHTTP.StartDownloadImage(lMediaTag.Thumbs(0).Thumb)
                While sHTTP.IsDownloading
                    Application.DoEvents()
                    Threading.Thread.Sleep(50)
                End While
                e.Result = New Results With {.Result = sHTTP.Image}
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub
    Private Sub bwDownloadInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadInfo.RunWorkerCompleted
        Me.OK_Button.Enabled = True
        Me.txtSearch.Enabled = True
        Me.btnSearch.Enabled = True
        Me.tvResults.Enabled = True
        Me.pnlLoading.Visible = False
        Try
            If Not lMediaTag Is Nothing Then
                'Dim idx As Integer = Convert.ToInt32(Me.tvResults.SelectedNode.Tag)
                lblTitle.Text = Web.HttpUtility.HtmlDecode(lMediaTag.Title)
                lblYear.Text = lMediaTag.Year.ToString
                If Not lMediaTag.Directors Is Nothing Then
                    lblDirector.Text = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Directors.ToArray(), " / "))
                Else
                    lblDirector.Text = String.Empty
                End If
                txtOutline.Text = Web.HttpUtility.HtmlDecode(lMediaTag.Outline)
                If Not lMediaTag.Genres Is Nothing Then
                    lblGenre.Text = Web.HttpUtility.HtmlDecode(Strings.Join(lMediaTag.Genres.ToArray(), " / "))
                Else
                    lblGenre.Text = String.Empty
                End If
                Me.pbPoster.Image = Nothing
                If Not e.Result Is Nothing Then
                    Try
                        Dim iRes As Results = DirectCast(e.Result, Results)
                        Me.pbPoster.Image = iRes.Result
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    End Try
                End If
                lblTagline.Text = Web.HttpUtility.HtmlDecode(lMediaTag.Tagline)
                ControlsVisible(True)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Master.tmpMovie.Clear()

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ClearInfo()
        Me.ControlsVisible(False)
        Me.lblTitle.Text = String.Empty
        Me.lblYear.Text = String.Empty

        Master.tmpMovie.Clear()

    End Sub

    Private Sub ControlsVisible(ByVal areVisible As Boolean)
        Me.lblYearHeader.Visible = areVisible
        Me.lblDirectorHeader.Visible = areVisible
        Me.lblGenreHeader.Visible = areVisible
        Me.lblPlotHeader.Visible = areVisible
        'Me.lblIMDBHeader.Visible = areVisible
        Me.txtOutline.Visible = areVisible
        Me.lblYear.Visible = areVisible
        Me.lblTagline.Visible = areVisible
        Me.lblTitle.Visible = areVisible
        Me.lblDirector.Visible = areVisible
        Me.lblGenre.Visible = areVisible
        'Me.lblIMDB.Visible = areVisible
        Me.pbPoster.Visible = areVisible
    End Sub

    Private Sub dlgIMDBSearchResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub dlgSearchResults_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.tmrWait.Enabled = False
        Me.tmrWait.Interval = 250
        Me.tmrLoad.Enabled = False
        Me.tmrLoad.Interval = 250

        Try
            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Using g As Graphics = Graphics.FromImage(iBackground)
                g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
                Me.pnlTop.BackgroundImage = iBackground
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dlgIMDBSearchResults_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
        Me.tvResults.Focus()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Try

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Close()
    End Sub

    Private Sub SearchResultsDownloaded()
        '//
        ' Process the results that IMDB gave us
        '\\

        Try
            Me.tvResults.Nodes.Clear()
            Me.ClearInfo()
            'Dim TnP As New TreeNode(String.Format(Master.eLang.GetString(297, "Matches ({0})"), mList.Count))
            Dim selNode As New TreeNode

            For c = 0 To mList.Count - 1
                Dim title As String = Web.HttpUtility.HtmlDecode(mList(c).Title)
                Me.tvResults.Nodes.Add(New TreeNode() With {.Tag = c, .Text = String.Concat(title, If(Not String.IsNullOrEmpty(mList(c).Year.ToString), String.Format(" ({0})", mList(c).Year), String.Empty))})
            Next

            'Me.tvResults.Nodes.Add(TnP)
            'selNode = Me.tvResults.Nodes.FirstNode
            Me.pnlLoading.Visible = False
            Me.txtSearch.Enabled = True
            Me.btnSearch.Enabled = True
            Me.tvResults.Enabled = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetUp()
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Label2.Text = Master.eLang.GetString(3, "View details of each result to find the proper movie.")
        Me.Label1.Text = Master.eLang.GetString(4, "Movie Search Results")
        Me.lblYearHeader.Text = Master.eLang.GetString(49, "Year:", True)
        Me.lblDirectorHeader.Text = Master.eLang.GetString(239, "Director:", True)
        Me.lblGenreHeader.Text = Master.eLang.GetString(2, "Genre(s):")
        'Me.lblIMDBHeader.Text = Master.eLang.GetString(289, "IMDB ID:", True)
        Me.lblPlotHeader.Text = Master.eLang.GetString(242, "Plot Outline:", True)
        Me.Label3.Text = Master.eLang.GetString(5, "Searching ...")
    End Sub

    Private Sub tmrLoad_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrWait.Tick
        Me.tmrWait.Enabled = False
        Me.tmrLoad.Enabled = False
        bwDownloadInfo.WorkerReportsProgress = True
        bwDownloadInfo.RunWorkerAsync(Me.tvResults.SelectedNode.Tag)
        Me.Label3.Text = Master.eLang.GetString(6, "Downloading details...")
    End Sub

    Private Sub tmrWait_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrWait.Tick
        If Not Me._prevnode = Me._currnode Then
            Me._prevnode = Me._currnode
            Me.tmrLoad.Enabled = True
        Else
            Me.tmrLoad.Enabled = False
        End If
    End Sub

    Private Sub tvResults_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvResults.AfterSelect
        Try
            Me.tmrWait.Enabled = False
            Me.tmrLoad.Enabled = False
            Me.ClearInfo()
            Me.OK_Button.Enabled = False
            If Not IsNothing(Me.tvResults.SelectedNode.Tag) AndAlso Not String.IsNullOrEmpty(Me.tvResults.SelectedNode.Tag.ToString) Then
                Me.txtSearch.Enabled = False
                Me.btnSearch.Enabled = False
                Me.tvResults.Enabled = False
                Me.pnlLoading.Visible = True
                Me.tmrWait.Enabled = True
            Else
                Me.pnlLoading.Visible = False
                Me.txtSearch.Enabled = True
                Me.btnSearch.Enabled = True
                Me.tvResults.Enabled = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub tvResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles tvResults.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub
    Private Sub txtSearch_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSearch.GotFocus
        Me.AcceptButton = Me.btnSearch
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Private Structure Arguments

#Region "Fields"

        Dim pURL As String

#End Region 'Fields

    End Structure

    Private Structure Results

#Region "Fields"

        Dim Result As Image

#End Region 'Fields

    End Structure

#End Region 'Nested Types
End Class