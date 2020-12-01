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
Imports EmberAPI

Public Class dlgIMDBSearchResults

    #Region "Fields"

    Public IMDBURL As String

    Friend  WithEvents bwDownloadPic As New System.ComponentModel.BackgroundWorker
    Friend  WithEvents tmrLoad As New System.Windows.Forms.Timer
    Friend  WithEvents tmrWait As New System.Windows.Forms.Timer

    Private IMDB As New IMDB.Scraper
    Private sHTTP As New HTTP
    Dim UseOFDBGenre As Boolean
    Dim UseOFDBOutline As Boolean
    Dim UseOFDBPlot As Boolean
    Dim UseOFDBTitle As Boolean
    Private _currnode As Integer = -1
    Private _prevnode As Integer = -2

    Private _InfoCache As New Dictionary(Of String, MediaContainers.Movie)
    Private _PosterCache As New Dictionary(Of String, System.Drawing.Image)
    Private _filterOptions As Structures.ScrapeOptions

#End Region 'Fields

    #Region "Methods"

    Public Overloads Function ShowDialog(ByVal sMovieTitle As String, ByVal filterOptions As Structures.ScrapeOptions) As DialogResult
        Me.tmrWait.Enabled = False
        Me.tmrWait.Interval = 250
        Me.tmrLoad.Enabled = False
        Me.tmrLoad.Interval = 100

		_filterOptions = filterOptions        

		Me.Text = String.Concat(Master.eLang.GetString(10, "Search Results - "), sMovieTitle)
		Me.txtSearch.Text = sMovieTitle
        chkManual.Enabled = False
        IMDB.IMDBURL = IMDBURL
        IMDB.SearchMovieAsync(sMovieTitle, _filterOptions)

        Return MyBase.ShowDialog()
    End Function

    Public Overloads Function ShowDialog(ByVal Res As IMDB.MovieSearchResults, ByVal sMovieTitle As String) As DialogResult
        Me.tmrWait.Enabled = False
        Me.tmrWait.Interval = 250
        Me.tmrLoad.Enabled = False
        Me.tmrLoad.Interval = 100

        Me.Text = String.Concat(Master.eLang.GetString(10, "Search Results - "), sMovieTitle)
		Me.txtSearch.Text = sMovieTitle
        SearchResultsDownloaded(Res)

        Return MyBase.ShowDialog()
    End Function

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If Not String.IsNullOrEmpty(Me.txtSearch.Text) Then
            Me.OK_Button.Enabled = False
            pnlPicStatus.Visible = False
            _InfoCache.Clear()
            _PosterCache.Clear()
            Me.ClearInfo()
            Me.Label3.Text = Master.eLang.GetString(11, "Searching IMDB...")
            Me.pnlLoading.Visible = True
            chkManual.Enabled = False
            IMDB.CancelAsync()
            IMDB.IMDBURL = IMDBURL
            IMDB.SearchMovieAsync(Me.txtSearch.Text, _filterOptions)
        End If
    End Sub

    Private Sub btnVerify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVerify.Click
        If Regex.IsMatch(Me.txtIMDBID.Text.Replace("tt", String.Empty), "\d\d\d\d\d\d\d") Then
            IMDB.IMDBURL = IMDBURL
            IMDB.GetSearchMovieInfoAsync(Me.txtIMDBID.Text.Replace("tt", String.Empty), Master.tmpMovie, Master.DefaultOptions)
        Else
            MsgBox(Master.eLang.GetString(12, "The ID you entered is not a valid IMDB ID."), MsgBoxStyle.Exclamation, Master.eLang.GetString(292, "Invalid Entry", True))
        End If
    End Sub

    Private Sub bwDownloadPic_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadPic.DoWork
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)

        sHTTP.StartDownloadImage(Args.pURL)

        While sHTTP.IsDownloading
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        e.Result = New Results With {.Result = sHTTP.Image, .IMDBId = Args.IMDBId}
    End Sub

    Private Sub bwDownloadPic_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadPic.RunWorkerCompleted
        '//
        ' Thread finished: display pic if it was able to get one
        '\\

        Dim Res As Results = DirectCast(e.Result, Results)

        Try
            Me.pbPoster.Image = Res.Result
            If Not _PosterCache.ContainsKey(Res.IMDBId) Then
                _PosterCache.Add(Res.IMDBId, Res.Result)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        Finally
            pnlPicStatus.Visible = False
        End Try
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Master.tmpMovie.Clear()

        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub chkManual_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkManual.CheckedChanged
        Me.ClearInfo()
        Me.OK_Button.Enabled = False
        Me.txtIMDBID.Enabled = Me.chkManual.Checked
        Me.btnVerify.Enabled = Me.chkManual.Checked
        Me.tvResults.Enabled = Not Me.chkManual.Checked

        If Not Me.chkManual.Checked Then
            txtIMDBID.Text = String.Empty
        End If
    End Sub

    Private Sub ClearInfo()
        Me.ControlsVisible(False)
        Me.lblTitle.Text = String.Empty
        Me.lblTagline.Text = String.Empty
        Me.lblYear.Text = String.Empty
        Me.lblDirector.Text = String.Empty
        Me.lblGenre.Text = String.Empty
        Me.txtOutline.Text = String.Empty
        Me.lblIMDB.Text = String.Empty
        Me.pbPoster.Image = Nothing

        Master.tmpMovie.Clear()

        IMDB.CancelAsync()
    End Sub

    Private Sub ControlsVisible(ByVal areVisible As Boolean)
        Me.lblYearHeader.Visible = areVisible
        Me.lblDirectorHeader.Visible = areVisible
        Me.lblGenreHeader.Visible = areVisible
        Me.lblPlotHeader.Visible = areVisible
        Me.lblIMDBHeader.Visible = areVisible
        Me.txtOutline.Visible = areVisible
        Me.lblYear.Visible = areVisible
        Me.lblTagline.Visible = areVisible
        Me.lblTitle.Visible = areVisible
        Me.lblDirector.Visible = areVisible
        Me.lblGenre.Visible = areVisible
        Me.lblIMDB.Visible = areVisible
        Me.pbPoster.Visible = areVisible
    End Sub

    Private Sub dlgIMDBSearchResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub dlgIMDBSearchResults_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
        pnlPicStatus.Visible = False
        IMDB.IMDBURL = IMDBURL
        IMDB.UseOFDBTitle = UseOFDBTitle
        IMDB.UseOFDBOutline = UseOFDBOutline
        IMDB.UseOFDBPlot = UseOFDBPlot
        IMDB.UseOFDBGenre = UseOFDBGenre
        AddHandler IMDB.SearchMovieInfoDownloaded, AddressOf SearchMovieInfoDownloaded
        AddHandler IMDB.SearchResultsDownloaded, AddressOf SearchResultsDownloaded

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
            If Me.chkManual.Checked AndAlso Me.btnVerify.Enabled Then
                If Not Regex.IsMatch(Me.txtIMDBID.Text.Replace("tt", String.Empty), "\d\d\d\d\d\d\d") Then
                    MsgBox(Master.eLang.GetString(12, "The ID you entered is not a valid IMDB ID."), MsgBoxStyle.Exclamation, Master.eLang.GetString(292, "Invalid Entry", True))
                    Exit Sub
                Else
                    If MsgBox(String.Concat(Master.eLang.GetString(13, "You have manually entered an IMDB ID but have not verified it is correct."), vbNewLine, vbNewLine, Master.eLang.GetString(101, "Are you sure you want to continue?", True)), MsgBoxStyle.YesNo, Master.eLang.GetString(14, "Continue without verification?")) = MsgBoxResult.No Then
                        Exit Sub
                    Else
                        Master.tmpMovie.IMDBID = Me.txtIMDBID.Text.Replace("tt", String.Empty)
                    End If
                End If
            End If
            Me.DialogResult = DialogResult.OK
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Close()
    End Sub

    Private Sub SearchMovieInfoDownloaded(ByVal sPoster As String, ByVal bSuccess As Boolean)
        '//
        ' Info downloaded... fill form with data
        '\\

        Me.pnlLoading.Visible = False
        Me.OK_Button.Enabled = True

        Try
            If bSuccess Then
                Me.ControlsVisible(True)
                Me.lblTitle.Text = Master.tmpMovie.Title
                Me.lblTagline.Text = Master.tmpMovie.Tagline
                Me.lblYear.Text = Master.tmpMovie.Year
                Me.lblDirector.Text = Master.tmpMovie.Director
                Me.lblGenre.Text = Master.tmpMovie.Genre
                Me.txtOutline.Text = Master.tmpMovie.Outline
                Me.lblIMDB.Text = Master.tmpMovie.IMDBID

                If _PosterCache.ContainsKey(Master.tmpMovie.IMDBID) Then
                    'just set it
                    Me.pbPoster.Image = _PosterCache(Master.tmpMovie.IMDBID)
                Else
                    'go download it, if available
                If Not String.IsNullOrEmpty(sPoster) Then
                    If Me.bwDownloadPic.IsBusy Then
                        Me.bwDownloadPic.CancelAsync()
                    End If
                        pnlPicStatus.Visible = True
                    Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                    Me.bwDownloadPic.WorkerSupportsCancellation = True
                        Me.bwDownloadPic.RunWorkerAsync(New Arguments With {.pURL = sPoster, .IMDBId = Master.tmpMovie.IMDBID})
                End If

                End If

                'store clone of tmpmovie
                If Not _InfoCache.ContainsKey(Master.tmpMovie.IMDBID) Then
                    _InfoCache.Add(Master.tmpMovie.IMDBID, GetMovieClone(Master.tmpMovie))
                End If


                Me.btnVerify.Enabled = False
            Else
                If Me.chkManual.Checked Then
                    MsgBox(Master.eLang.GetString(15, "Unable to retrieve movie details for the entered IMDB ID. Please check your entry and try again."), MsgBoxStyle.Exclamation, Master.eLang.GetString(16, "Verification Failed"))
                    Me.btnVerify.Enabled = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SearchResultsDownloaded(ByVal M As IMDB.MovieSearchResults)
        '//
        ' Process the results that IMDB gave us
        '\\

        Try
            Me.tvResults.Nodes.Clear()
            Me.ClearInfo()
            If Not IsNothing(M) Then
                If M.PartialMatches.Count > 0 OrElse M.PopularTitles.Count > 0 OrElse M.ExactMatches.Count > 0 Then
                    Dim TnP As New TreeNode(String.Format(Master.eLang.GetString(17, "Partial Matches ({0})"), M.PartialMatches.Count))
                    Dim selNode As New TreeNode

                    If M.PartialMatches.Count > 0 Then
                        M.PartialMatches.Sort()
                        For Each Movie As MediaContainers.Movie In M.PartialMatches
                            TnP.Nodes.Add(New TreeNode() With {.Text = String.Concat(Movie.Title, If(Not String.IsNullOrEmpty(Movie.Year), String.Format(" ({0})", Movie.Year), String.Empty)), .Tag = Movie.IMDBID})
                        Next
                        TnP.Expand()
                        Me.tvResults.Nodes.Add(TnP)
                        selNode = TnP.FirstNode
                    End If

                    If M.PopularTitles.Count > 0 Then
                        M.PopularTitles.Sort()
                        If M.PartialMatches.Count > 0 OrElse M.ExactMatches.Count > 0 Then
                            Me.tvResults.Nodes(TnP.Index).Collapse()
                        End If

                        TnP = New TreeNode(String.Format(Master.eLang.GetString(19, "Popular Titles ({0})"), M.PopularTitles.Count))

                        For Each Movie As MediaContainers.Movie In M.PopularTitles
                            TnP.Nodes.Add(New TreeNode() With {.Text = String.Concat(Movie.Title, If(Not String.IsNullOrEmpty(Movie.Year), String.Format(" ({0})", Movie.Year), String.Empty)), .Tag = Movie.IMDBID})

                        Next
                        TnP.Expand()
                        Me.tvResults.Nodes.Add(TnP)

                        selNode = TnP.FirstNode
                    End If

                    If M.ExactMatches.Count > 0 Then
                        M.ExactMatches.Sort()
                        If M.PartialMatches.Count > 0 Then
                            Me.tvResults.Nodes(TnP.Index).Collapse()
                        End If
                        TnP = New TreeNode(String.Format(Master.eLang.GetString(18, "Exact Matches ({0})"), M.ExactMatches.Count))
                        For Each Movie As MediaContainers.Movie In M.ExactMatches
                            TnP.Nodes.Add(New TreeNode() With {.Text = String.Concat(Movie.Title, If(Not String.IsNullOrEmpty(Movie.Year), String.Format(" ({0})", Movie.Year), String.Empty)), .Tag = Movie.IMDBID})
                        Next
                        TnP.Expand()
                        Me.tvResults.Nodes.Add(TnP)

                        selNode = TnP.FirstNode

                    End If
                    Me._prevnode = -2

                    'determine if we automatically start downloading info for selected node
                    If M.ExactMatches.Count > 0 Then
                        'If M.ExactMatches.Count = 1 Then
                        Me.tvResults.SelectedNode = selNode
                        'Else
                        'Me.tvResults.SelectedNode = Nothing
                        'End If
                    Else
                        Me.tvResults.SelectedNode = Nothing
                    End If
                    Me.tvResults.Focus()
                Else
                    Me.tvResults.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(20, "No Matches Found")})
                End If
            End If
            Me.pnlLoading.Visible = False
            chkManual.Enabled = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetUp()
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Label2.Text = Master.eLang.GetString(21, "View details of each result to find the proper movie.")
        Me.Label1.Text = Master.eLang.GetString(22, "Movie Search Results")
        Me.chkManual.Text = Master.eLang.GetString(23, "Manual IMDB Entry:")
        Me.btnVerify.Text = Master.eLang.GetString(24, "Verify")
        Me.lblYearHeader.Text = Master.eLang.GetString(49, "Year:", True)
        Me.lblDirectorHeader.Text = Master.eLang.GetString(239, "Director:", True)
        Me.lblGenreHeader.Text = Master.eLang.GetString(51, "Genre(s):", True)
        Me.lblIMDBHeader.Text = Master.eLang.GetString(289, "IMDB ID:", True)
        Me.lblPlotHeader.Text = Master.eLang.GetString(242, "Plot Outline:", True)
        Me.Label3.Text = Master.eLang.GetString(25, "Searching IMDB...")
    End Sub

    Private Sub tmrLoad_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrLoad.Tick
        Me.tmrWait.Stop()
        Me.tmrLoad.Stop()
        Me.pnlLoading.Visible = True
        Me.Label3.Text = Master.eLang.GetString(26, "Downloading details...")

        IMDB.IMDBURL = IMDBURL
        IMDB.GetSearchMovieInfoAsync(Me.tvResults.SelectedNode.Tag.ToString, Master.tmpMovie, Master.DefaultOptions)
    End Sub

    Private Sub tmrWait_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrWait.Tick
        If Not Me._prevnode = Me._currnode Then
            Me._prevnode = Me._currnode
            Me.tmrWait.Stop()
            Me.tmrLoad.Start()
        Else
            Me.tmrLoad.Stop()
            Me.tmrWait.Stop()
        End If
    End Sub
    Private Sub tvResults_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvResults.AfterSelect
        Try
            Me.tmrWait.Stop()
            Me.tmrLoad.Stop()

            Me.ClearInfo()
            Me.OK_Button.Enabled = False

            If Not IsNothing(Me.tvResults.SelectedNode.Tag) AndAlso Not String.IsNullOrEmpty(Me.tvResults.SelectedNode.Tag.ToString) Then
                Me._currnode = Me.tvResults.SelectedNode.Index

                'check if this movie is in the cache already
                If _InfoCache.ContainsKey(Me.tvResults.SelectedNode.Tag.ToString) Then
                    Master.tmpMovie = GetMovieClone(_InfoCache(Me.tvResults.SelectedNode.Tag.ToString))
                    SearchMovieInfoDownloaded(String.Empty, True)
                    Return
                End If

                Me.pnlLoading.Visible = True
                Me.tmrWait.Start()
            Else
                Me.pnlLoading.Visible = False
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub tvResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles tvResults.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub txtIMDBID_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtIMDBID.GotFocus
        Me.AcceptButton = Me.btnVerify
    End Sub

    Private Sub txtIMDBID_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtIMDBID.TextChanged
        If Me.chkManual.Checked Then
            Me.btnVerify.Enabled = True
            Me.OK_Button.Enabled = False
        End If
    End Sub

    Private Sub txtSearch_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSearch.GotFocus
        Me.AcceptButton = Me.btnSearch
    End Sub

    Private Function GetMovieClone(ByVal original As MediaContainers.Movie) As MediaContainers.Movie
        'have to do this the old-fashioned way because it is not serializable
        Dim result As New MediaContainers.Movie
        With result
            .IMDBID = original.IMDBID
            .Genre = original.Genre
            .Title = original.Title
            .Tagline = original.Tagline
            .Year = original.Year
            .Director = original.Director
            .Genre = original.Genre
            .Outline = original.Outline
        End With
        Return result
        'Using mem As New IO.MemoryStream()
        '    Dim bin As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext(Runtime.Serialization.StreamingContextStates.Clone))
        '    bin.Serialize(mem, original)
        '    mem.Seek(0, IO.SeekOrigin.Begin)
        '    Return DirectCast(bin.Deserialize(mem), MediaContainers.Movie)
        'End Using
    End Function


    #End Region 'Methods

    #Region "Nested Types"

    Private Structure Arguments

        #Region "Fields"

        Dim pURL As String
        Dim IMDBId As String

        #End Region 'Fields

    End Structure

    Private Structure Results

        #Region "Fields"

        Dim Result As Image
        Dim IMDBId As String

        #End Region 'Fields

    End Structure

    #End Region 'Nested Types
End Class