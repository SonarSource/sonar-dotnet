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
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class frmMain

#Region "Fields"
    Private fLoading As New frmSplash

    Friend WithEvents bwCleanDB As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwDownloadPic As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadEpInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadSeasonInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadShowInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwMediaInfo As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwMovieScraper As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwNonScrape As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwRefreshMovies As New System.ComponentModel.BackgroundWorker

    Private alActors As New List(Of String)
    Private aniFilterRaise As Boolean = False
    Private aniRaise As Boolean = False
    Private aniShowType As Integer = 0 '0 = down, 1 = mid, 2 = up
    Private aniType As Integer = 0 '0 = down, 1 = mid, 2 = up
    Private bsEpisodes As New BindingSource

    Private bsMedia As New BindingSource
    Private bsSeasons As New BindingSource
    Private bsShows As New BindingSource

    Private dtEpisodes As New DataTable
    Private dtMedia As New DataTable
    Private dtSeasons As New DataTable
    Private dtShows As New DataTable
    Private dScrapeRow As DataRow = Nothing

    Private fScanner As New Scanner
    Private GenreImage As Image
    Private InfoCleared As Boolean = False
    Private isCL As Boolean = False
    Private LoadingDone As Boolean = False
    Private MainAllSeason As New Images
    Private MainFanart As New Images
    Private MainPoster As New Images
    Private pbGenre() As PictureBox = Nothing
    Private pnlGenre() As Panel = Nothing
    Private prevText As String = String.Empty
    Private ReportDownloadPercent As Boolean = False
    Private ScrapeList As New List(Of DataRow)
    Private ScraperDone As Boolean = False
    Private sHTTP As New HTTP
    Private tmpLang As String = String.Empty
    Private tmpTitle As String = String.Empty
    Private tmpTVDB As String = String.Empty
    Private tmpOrdering As Enums.Ordering = Enums.Ordering.Standard

    'Loading Delays
    Private currRow As Integer = -1
    Private currEpRow As Integer = -1
    Private currSeasonRow As Integer = -1
    Private currShowRow As Integer = -1
    Private currList As Integer = 0
    Private currText As String = String.Empty
    Private currThemeType As Theming.ThemeType
    Private prevEpRow As Integer = -1
    Private prevRow As Integer = -1
    Private prevSeasonRow As Integer = -1
    Private prevShowRow As Integer = -1
    Private bDoingSearch As Boolean = False

    'filters
    Private FilterArray As New List(Of String)
    Private filSearch As String = String.Empty
    Private filSource As String = String.Empty
    Private filYear As String = String.Empty
    Private filGenre As String = String.Empty
    Private filMissing As String = String.Empty

    'Theme Information
    Private _postermaxheight As Integer = 160
    Private _postermaxwidth As Integer = 160
    Private tTheme As New Theming
    Private _genrepanelcolor As Color = Color.Gainsboro
    Private _ipmid As Integer = 280
    Private _ipup As Integer = 500
    Private CloseApp As Boolean = False

    Private oldStatus As String = String.Empty

    Private KeyBuffer As String = String.Empty

#End Region 'Fields

#Region "Delegates"

    Delegate Sub DelegateEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object)

    Delegate Sub MydtListUpdate(ByVal drow As DataRow, ByVal i As Integer, ByVal v As Object)

    Delegate Sub MySettingsShow(ByVal dlg As dlgSettings)

#End Region 'Delegates

#Region "Properties"

    Public Property GenrePanelColor() As Color
        Get
            Return _genrepanelcolor
        End Get
        Set(ByVal value As Color)
            _genrepanelcolor = value
        End Set
    End Property

    Public Property IPMid() As Integer
        Get
            Return _ipmid
        End Get
        Set(ByVal value As Integer)
            _ipmid = value
        End Set
    End Property

    Public Property IPUp() As Integer
        Get
            Return _ipup
        End Get
        Set(ByVal value As Integer)
            _ipup = value
        End Set
    End Property

    Public Property PosterMaxHeight() As Integer
        Get
            Return _postermaxheight
        End Get
        Set(ByVal value As Integer)
            _postermaxheight = value
        End Set
    End Property

    Public Property PosterMaxWidth() As Integer
        Get
            Return _postermaxwidth
        End Get
        Set(ByVal value As Integer)
            _postermaxwidth = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Sub InstallNewFiles(ByVal fname As String)
        Dim _cmds As Containers.InstallCommands = Containers.InstallCommands.Load(fname)
        For Each _cmd As Containers.InstallCommand In _cmds.Command
            Try
                Select Case _cmd.CommandType
                    Case "FILE.Move"
                        Dim s() As String = _cmd.CommandExecute.Split("|"c)
                        If s.Count >= 2 Then
                            If File.Exists(s(1)) Then File.Delete(s(1))
                            If Not Directory.Exists(Path.GetDirectoryName(s(1))) Then
                                Directory.CreateDirectory(Path.GetDirectoryName(s(1)))
                            End If
                            File.Move(s(0), s(1))
                        End If
                    Case "FILE.Delete"
                        If File.Exists(_cmd.CommandExecute) Then File.Delete(_cmd.CommandExecute)
                End Select
            Catch ex As Exception
                Dim log As New StreamWriter(Path.Combine(Functions.AppPath, "install.log"), True)
                log.WriteLine(String.Format("--- Error: {0}", ex.Message))
                log.WriteLine(ex.StackTrace)
                log.Close()
            End Try
        Next
    End Sub

    Public Sub ClearInfo(Optional ByVal WithAllSeasons As Boolean = True)

        Try
            With Me
                .InfoCleared = True

                If .bwDownloadPic.IsBusy Then .bwDownloadPic.CancelAsync()
                If .bwLoadInfo.IsBusy Then .bwLoadInfo.CancelAsync()
                If .bwLoadShowInfo.IsBusy Then .bwLoadShowInfo.CancelAsync()
                If .bwLoadSeasonInfo.IsBusy Then .bwLoadSeasonInfo.CancelAsync()
                If .bwLoadEpInfo.IsBusy Then .bwLoadEpInfo.CancelAsync()

                While .bwDownloadPic.IsBusy OrElse .bwLoadInfo.IsBusy OrElse .bwLoadShowInfo.IsBusy OrElse _
                        .bwLoadSeasonInfo.IsBusy OrElse .bwLoadEpInfo.IsBusy
                    Application.DoEvents()
                    Threading.Thread.Sleep(50)
                End While

                If Not IsNothing(.pbFanart.Image) Then
                    .pbFanart.Image.Dispose()
                    .pbFanart.Image = Nothing
                End If
                .MainFanart.Clear()

                If Not IsNothing(.pbPoster.Image) Then
                    .pbPoster.Image.Dispose()
                    .pbPoster.Image = Nothing
                End If
                .pnlPoster.Visible = False
                .MainPoster.Clear()

                If WithAllSeasons Then
                    If Not IsNothing(.pbAllSeason.Image) Then
                        .pbAllSeason.Image.Dispose()
                        .pbAllSeason.Image = Nothing
                    End If
                    .MainAllSeason.Clear()
                End If
                .pnlAllSeason.Visible = False

                'remove all the current genres
                Try
                    For iDel As Integer = UBound(.pnlGenre) To 0 Step -1
                        .scMain.Panel2.Controls.Remove(.pbGenre(iDel))
                        .scMain.Panel2.Controls.Remove(.pnlGenre(iDel))
                    Next
                Catch
                End Try

                If Not IsNothing(.pbMPAA.Image) Then
                    .pbMPAA.Image = Nothing
                End If
                .pnlMPAA.Visible = False

                .lblTitle.Text = String.Empty
                .lblVotes.Text = String.Empty
                .lblRuntime.Text = String.Empty
                .pnlTop250.Visible = False
                .lblTop250.Text = String.Empty
                .pbStar1.Image = Nothing
                .pbStar2.Image = Nothing
                .pbStar3.Image = Nothing
                .pbStar4.Image = Nothing
                .pbStar5.Image = Nothing
                ToolTips.SetToolTip(pbStar1, "")
                ToolTips.SetToolTip(pbStar2, "")
                ToolTips.SetToolTip(pbStar3, "")
                ToolTips.SetToolTip(pbStar4, "")
                ToolTips.SetToolTip(pbStar5, "")

                .lstActors.Items.Clear()
                If Not IsNothing(.alActors) Then
                    .alActors.Clear()
                    .alActors = Nothing
                End If
                If Not IsNothing(.pbActors.Image) Then
                    .pbActors.Image.Dispose()
                    .pbActors.Image = Nothing
                End If
                .lblDirector.Text = String.Empty
                .lblReleaseDate.Text = String.Empty
                .txtCerts.Text = String.Empty
                .txtIMDBID.Text = String.Empty
                .txtFilePath.Text = String.Empty
                .txtOutline.Text = String.Empty
                .txtPlot.Text = String.Empty
                .lblTagline.Text = String.Empty
                If Not IsNothing(.pbMPAA.Image) Then
                    .pbMPAA.Image.Dispose()
                    .pbMPAA.Image = Nothing
                End If
                .pbStudio.Image = Nothing
                .pbVideo.Image = Nothing
                .pbVType.Image = Nothing
                .pbAudio.Image = Nothing
                .pbResolution.Image = Nothing
                .pbChannels.Image = Nothing

                .txtMetaData.Text = String.Empty
                .pnlTop.Visible = False
                '.tslStatus.Text = String.Empty

                Application.DoEvents()
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub LoadMedia(ByVal Scan As Structures.Scans, Optional ByVal SourceName As String = "")
        Try
            Me.SetStatus(Master.eLang.GetString(116, "Performing Preliminary Tasks (Gathering Data)..."))
            Me.tspbLoading.ProgressBar.Style = ProgressBarStyle.Marquee
            Me.tspbLoading.Visible = True

            Application.DoEvents()

            Me.ClearInfo()
            Me.ClearFilters()
            Me.EnableFilters(False)

            Me.SetControlsEnabled(False)
            Me.tabMovies.Text = Master.eLang.GetString(36, "Movies")
            Me.tabTV.Text = Master.eLang.GetString(653, "TV")
            Me.txtSearch.Text = String.Empty

            Me.fScanner.CancelAndWait()

            If Scan.Movies Then
                Me.dgvMediaList.DataSource = Nothing
            End If

            If Scan.TV Then
                Me.dgvTVShows.DataSource = Nothing
                Me.dgvTVSeasons.DataSource = Nothing
                Me.dgvTVEpisodes.DataSource = Nothing
            End If

            Me.fScanner.Start(Scan, SourceName)

        Catch ex As Exception
            Me.LoadingDone = True
            Me.EnableFilters(True)
            Me.SetControlsEnabled(True)
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub SetListItemAfterEdit(ByVal iID As Integer, ByVal iRow As Integer)
        Try
            Dim dRow = From drvRow In dtMedia.Rows Where Convert.ToInt32(DirectCast(drvRow, DataRow).Item(0)) = iID Select drvRow

            Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                SQLcommand.CommandText = String.Concat("SELECT mark, SortTitle FROM movies WHERE id = ", iID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    DirectCast(dRow(0), DataRow).Item(11) = Convert.ToBoolean(SQLreader("mark"))
                    If Not DBNull.Value.Equals(SQLreader("SortTitle")) Then DirectCast(dRow(0), DataRow).Item(47) = SQLreader("SortTitle").ToString
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub SetShowListItemAfterEdit(ByVal iID As Integer, ByVal iRow As Integer)
        Try
            Dim dRow = From drvRow In dtShows.Rows Where Convert.ToInt32(DirectCast(drvRow, DataRow).Item(0)) = iID Select drvRow

            Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                SQLcommand.CommandText = String.Concat("SELECT Ordering FROM TVShows WHERE id = ", iID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    DirectCast(dRow(0), DataRow).Item(23) = Convert.ToInt32(SQLreader("Ordering"))
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Using dAbout As New dlgAbout
            dAbout.ShowDialog()
        End Using
    End Sub

    Private Sub AddGenreToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddGenreToolStripMenuItem.Click
        Try
            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parGenre As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parGenre", DbType.String, 0, "Genre")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE movies SET Genre = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                        If Not sRow.Cells(27).Value.ToString.Contains(Me.GenreListToolStripComboBox.Text) Then
                            If Not String.IsNullOrEmpty(sRow.Cells(27).Value.ToString) Then
                                parGenre.Value = String.Format("{0} / {1}", sRow.Cells(27).Value, Me.GenreListToolStripComboBox.Text).Trim
                            Else
                                parGenre.Value = Me.GenreListToolStripComboBox.Text.Trim
                            End If
                            parID.Value = sRow.Cells(0).Value
                            SQLcommand.ExecuteNonQuery()
                        End If
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    Me.RefreshMovie(Convert.ToInt64(sRow.Cells(0).Value), True, False, True)
                Next
                SQLtransaction.Commit()
            End Using

            Me.LoadInfo(Convert.ToInt32(Me.dgvMediaList.Item(0, Me.dgvMediaList.CurrentCell.RowIndex).Value), Me.dgvMediaList.Item(1, Me.dgvMediaList.CurrentCell.RowIndex).Value.ToString, True, False)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ApplyTheme(ByVal tType As Theming.ThemeType)
        Me.pnlInfoPanel.SuspendLayout()

        Me.currThemeType = tType

        tTheme.ApplyTheme(tType)

        Me.tmrAni.Stop()

        Select Case If(Me.tabsMain.SelectedIndex = 0, aniType, aniShowType)
            Case 1
                If Me.btnMid.Visible Then
                    Me.pnlInfoPanel.Height = Me._ipmid
                    Me.btnUp.Enabled = True
                    Me.btnMid.Enabled = False
                    Me.btnDown.Enabled = True
                ElseIf Me.btnUp.Visible Then
                    Me.pnlInfoPanel.Height = Me._ipup
                    If Me.tabsMain.SelectedIndex = 0 Then
                        aniType = 2
                    Else
                        aniShowType = 2
                    End If
                    Me.btnUp.Enabled = False
                    Me.btnMid.Enabled = True
                    Me.btnDown.Enabled = True
                Else
                    Me.pnlInfoPanel.Height = 25
                    If Me.tabsMain.SelectedIndex = 0 Then
                        aniType = 0
                    Else
                        aniShowType = 0
                    End If
                    Me.btnUp.Enabled = True
                    Me.btnMid.Enabled = True
                    Me.btnDown.Enabled = False
                End If
            Case 2
                If Me.btnUp.Visible Then
                    Me.pnlInfoPanel.Height = Me._ipup
                    Me.btnUp.Enabled = False
                    Me.btnMid.Enabled = True
                    Me.btnDown.Enabled = True
                ElseIf Me.btnMid.Visible Then
                    Me.pnlInfoPanel.Height = Me._ipmid

                    If Me.tabsMain.SelectedIndex = 0 Then
                        aniType = 1
                    Else
                        aniShowType = 1
                    End If

                    Me.btnUp.Enabled = True
                    Me.btnMid.Enabled = False
                    Me.btnDown.Enabled = True
                Else
                    Me.pnlInfoPanel.Height = 25
                    If Me.tabsMain.SelectedIndex = 0 Then
                        aniType = 0
                    Else
                        aniShowType = 0
                    End If
                    Me.btnUp.Enabled = True
                    Me.btnMid.Enabled = True
                    Me.btnDown.Enabled = False
                End If
            Case Else
                Me.pnlInfoPanel.Height = 25
                If Me.tabsMain.SelectedIndex = 0 Then
                    aniType = 0
                Else
                    aniShowType = 0
                End If

                Me.btnUp.Enabled = True
                Me.btnMid.Enabled = True
                Me.btnDown.Enabled = False
        End Select

        Me.pbActLoad.Visible = False
        Me.pbActors.Image = My.Resources.actor_silhouette
        Me.pbMILoading.Visible = False

        Me.pnlInfoPanel.ResumeLayout()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        btnCancel.Visible = False
        lblCanceling.Visible = True
        pbCanceling.Visible = True

        If Me.bwMovieScraper.IsBusy Then Me.bwMovieScraper.CancelAsync()
        If Me.bwRefreshMovies.IsBusy Then Me.bwRefreshMovies.CancelAsync()
        If Me.bwNonScrape.IsBusy Then Me.bwNonScrape.CancelAsync()
        While Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwNonScrape.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While
    End Sub

    Private Sub btnClearFilters_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearFilters.Click
        Me.ClearFilters(True)
    End Sub

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        '//
        ' Begin animation to lower panel all the way down
        '\\

        Me.tabsMain.Focus()
        If Me.tabsMain.SelectedIndex = 0 Then
            Me.aniType = 0
        Else
            Me.aniShowType = 0
        End If
        Me.aniRaise = False
        Me.tmrAni.Start()
    End Sub

    Private Sub btnFilterDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFilterDown.Click
        '//
        ' Begin animation to lower panel all the way down
        '\\
        Me.aniFilterRaise = False
        Me.tmrFilterAni.Start()
    End Sub

    Private Sub btnFilterUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFilterUp.Click
        '//
        ' Begin animation to raise panel all the way up
        '\\
        Me.aniFilterRaise = True
        Me.tmrFilterAni.Start()
    End Sub

    Private Sub btnIMDBRating_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnIMDBRating.Click
        If Me.dgvMediaList.RowCount > 0 Then
            If Me.btnIMDBRating.Tag.ToString = "DESC" Then
                Me.btnIMDBRating.Tag = "ASC"
                Me.btnIMDBRating.Image = My.Resources.desc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(18), ComponentModel.ListSortDirection.Descending)
            Else
                Me.btnIMDBRating.Tag = "DESC"
                Me.btnIMDBRating.Image = My.Resources.asc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(18), ComponentModel.ListSortDirection.Ascending)
            End If
        End If
    End Sub

    Private Sub btnMarkAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMarkAll.Click
        Try
            Dim MarkAll As Boolean = Not btnMarkAll.Text = Master.eLang.GetString(105, "Unmark All")
            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                    SQLcommand.CommandText = "UPDATE movies SET mark = (?);"
                    parMark.Value = MarkAll
                    SQLcommand.ExecuteNonQuery()
                End Using
                SQLtransaction.Commit()
            End Using
            For Each drvRow As DataRow In dtMedia.Rows
                drvRow.Item(11) = MarkAll
            Next
            dgvMediaList.Refresh()
            btnMarkAll.Text = If(Not MarkAll, Master.eLang.GetString(35, "Mark All"), Master.eLang.GetString(105, "Unmark All"))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnMid_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMid.Click
        '//
        ' Begin animation to raise/lower panel to mid point
        '\\

        Me.tabsMain.Focus()
        If Me.pnlInfoPanel.Height = Me.IPUp Then
            Me.aniRaise = False
        Else
            Me.aniRaise = True
        End If

        If Me.tabsMain.SelectedIndex = 0 Then
            Me.aniType = 1
        Else
            Me.aniShowType = 1
        End If

        Me.tmrAni.Start()
    End Sub

    Private Sub btnMIRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMetaDataRefresh.Click
        '//
        ' Refresh Media Info
        '\\

        If Me.tabsMain.SelectedIndex = 0 Then
            If Not String.IsNullOrEmpty(Master.currMovie.Filename) AndAlso Me.dgvMediaList.SelectedRows.Count > 0 Then
                Me.LoadInfo(Convert.ToInt32(Master.currMovie.ID), Master.currMovie.Filename, False, True, True)
            End If
        ElseIf Not String.IsNullOrEmpty(Master.currShow.Filename) AndAlso Me.dgvTVEpisodes.SelectedRows.Count > 0 Then
            Me.SetControlsEnabled(False, True)

            If Me.bwMediaInfo.IsBusy Then Me.bwMediaInfo.CancelAsync()

            Me.txtMetaData.Clear()
            Me.pbMILoading.Visible = True

            Me.bwMediaInfo = New System.ComponentModel.BackgroundWorker
            Me.bwMediaInfo.WorkerSupportsCancellation = True
            Me.bwMediaInfo.RunWorkerAsync(New Arguments With {.TVShow = Master.currShow, .IsTV = True, .setEnabled = True})
        End If
    End Sub

    Private Sub btnPlay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPlay.Click
        '//
        ' Launch video using system default player
        '\\

        Try
            If Not String.IsNullOrEmpty(Me.txtFilePath.Text) Then
                If File.Exists(Me.txtFilePath.Text) Then
                    If Master.isWindows Then
                        Process.Start(String.Concat("""", Me.txtFilePath.Text, """"))
                    Else
                        Using Explorer As New Process
                            Explorer.StartInfo.FileName = "xdg-open"
                            Explorer.StartInfo.Arguments = String.Format("""{0}""", Me.txtFilePath.Text)
                            Explorer.Start()
                        End Using
                    End If

                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnSortDate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSortDate.Click
        If Me.dgvMediaList.RowCount > 0 Then
            If Me.btnSortDate.Tag.ToString = "DESC" Then
                Me.btnSortDate.Tag = "ASC"
                Me.btnSortDate.Image = My.Resources.desc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(0), ComponentModel.ListSortDirection.Descending)
            Else
                Me.btnSortDate.Tag = "DESC"
                Me.btnSortDate.Image = My.Resources.asc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(0), ComponentModel.ListSortDirection.Ascending)
            End If
        End If
    End Sub

    Private Sub btnSortTitle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSortTitle.Click
        If Me.dgvMediaList.RowCount > 0 Then
            If Me.btnSortTitle.Tag.ToString = "DESC" Then
                Me.btnSortTitle.Tag = "ASC"
                Me.btnSortTitle.Image = My.Resources.desc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(47), ComponentModel.ListSortDirection.Descending)
            Else
                Me.btnSortTitle.Tag = "DESC"
                Me.btnSortTitle.Image = My.Resources.asc
                Me.dgvMediaList.Sort(Me.dgvMediaList.Columns(47), ComponentModel.ListSortDirection.Ascending)
            End If
        End If
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        '//
        ' Begin animation to raise panel all the way up
        '\\

        Me.tabsMain.Focus()
        If Me.tabsMain.SelectedIndex = 0 Then
            Me.aniType = 2
        Else
            Me.aniShowType = 2
        End If
        Me.aniRaise = True
        Me.tmrAni.Start()
    End Sub

    Private Sub BuildStars(ByVal sinRating As Single)
        '//
        ' Convert # rating to star images
        '\\

        Try
            With Me
                .pbStar1.Image = Nothing
                .pbStar2.Image = Nothing
                .pbStar3.Image = Nothing
                .pbStar4.Image = Nothing
                .pbStar5.Image = Nothing

                Dim tTip As String = String.Concat(Master.eLang.GetString(245, "Rating:"), String.Format(" {0:N}", sinRating))
                ToolTips.SetToolTip(.pbStar1, tTip)
                ToolTips.SetToolTip(.pbStar2, tTip)
                ToolTips.SetToolTip(.pbStar3, tTip)
                ToolTips.SetToolTip(.pbStar4, tTip)
                ToolTips.SetToolTip(.pbStar5, tTip)

                If sinRating >= 0.5 Then ' if rating is less than .5 out of ten, consider it a 0
                    Select Case (sinRating / 2)
                        Case Is <= 0.5
                            .pbStar1.Image = My.Resources.starhalf
                        Case Is <= 1
                            .pbStar1.Image = My.Resources.star
                        Case Is <= 1.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.starhalf
                        Case Is <= 2
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                        Case Is <= 2.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.starhalf
                        Case Is <= 3
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                        Case Is <= 3.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.starhalf
                        Case Is <= 4
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                        Case Is <= 4.5
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                            .pbStar5.Image = My.Resources.starhalf
                        Case Else
                            .pbStar1.Image = My.Resources.star
                            .pbStar2.Image = My.Resources.star
                            .pbStar3.Image = My.Resources.star
                            .pbStar4.Image = My.Resources.star
                            .pbStar5.Image = My.Resources.star
                    End Select
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwCleanDB_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwCleanDB.DoWork
        Master.DB.Clean(True, True)
    End Sub

    Private Sub bwCleanDB_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwCleanDB.RunWorkerCompleted
        Me.SetStatus(String.Empty)
        Me.tspbLoading.Visible = False

        Me.FillList(0)
    End Sub

    Private Sub bwDownloadPic_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadPic.DoWork
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)
        Try

            sHTTP.StartDownloadImage(Args.pURL)

            While sHTTP.IsDownloading
                Application.DoEvents()
                If Me.bwDownloadPic.CancellationPending Then
                    e.Cancel = True
                    sHTTP.Cancel()
                    Return
                End If
                Threading.Thread.Sleep(50)
            End While

            e.Result = New Results With {.Result = sHTTP.Image}
        Catch ex As Exception
            e.Result = New Results With {.Result = Nothing}
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwDownloadPic_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadPic.RunWorkerCompleted
        '//
        ' Thread finished: display pic if it was able to get one
        '\\

        Me.pbActLoad.Visible = False

        If e.Cancelled Then
            Me.pbActors.Image = My.Resources.actor_silhouette
        Else
            Dim Res As Results = DirectCast(e.Result, Results)

            If Not IsNothing(Res.Result) Then
                Me.pbActors.Image = Res.Result
            Else
                Me.pbActors.Image = My.Resources.actor_silhouette
            End If
        End If
    End Sub

    Private Sub bwLoadEpInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadEpInfo.DoWork
        Try

            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Me.MainPoster.Clear()
            Me.MainFanart.Clear()

            If bwLoadEpInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            Master.currShow = Master.DB.LoadTVEpFromDB(Args.ID, True)

            If bwLoadEpInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayPoster Then Me.MainPoster.FromFile(Master.currShow.EpPosterPath)

            If bwLoadEpInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayFanart Then
                Dim NeedsGS As Boolean = False
                If Not String.IsNullOrEmpty(Master.currShow.EpFanartPath) Then
                    Me.MainFanart.FromFile(Master.currShow.EpFanartPath)
                Else
                    Me.MainFanart.FromFile(Master.currShow.ShowFanartPath)
                    NeedsGS = True
                End If

                If Not IsNothing(Me.MainFanart.Image) Then
                    If String.IsNullOrEmpty(Master.currShow.Filename) Then
                        Me.MainFanart.Image = ImageUtils.AddMissingStamp(Me.MainFanart.Image)
                    ElseIf NeedsGS Then
                        Me.MainFanart.Image = ImageUtils.GrayScale(Me.MainFanart.Image)
                    End If
                End If
            End If

            'wait for mediainfo to update the nfo
            While bwMediaInfo.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If bwLoadEpInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwLoadEpInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadEpInfo.RunWorkerCompleted
        Try
            If Not e.Cancelled Then
                Me.fillScreenInfoWithEpisode()
            Else
                Me.SetControlsEnabled(True)
            End If

            Me.dgvTVEpisodes.ResumeLayout()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadInfo.DoWork
        Try

            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Me.MainFanart.Clear()
            Me.MainPoster.Clear()

            If bwLoadInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            Master.currMovie = Master.DB.LoadMovieFromDB(Args.ID)

            If bwLoadInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayFanart Then Me.MainFanart.FromFile(Master.currMovie.FanartPath)

            If bwLoadInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayPoster Then Me.MainPoster.FromFile(Master.currMovie.PosterPath)
            'read nfo if it's there

            'wait for mediainfo to update the nfo
            While bwMediaInfo.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If bwLoadInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwLoadInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadInfo.RunWorkerCompleted
        Try
            If Not e.Cancelled Then
                Me.fillScreenInfoWithMovie()
            Else
                If Not bwMovieScraper.IsBusy AndAlso Not bwRefreshMovies.IsBusy AndAlso Not bwCleanDB.IsBusy AndAlso Not bwNonScrape.IsBusy Then
                    Me.SetControlsEnabled(True)
                    Me.EnableFilters(True)
                Else
                    Me.dgvMediaList.Enabled = True
                End If
            End If
            Me.dgvMediaList.ResumeLayout()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadSeasonInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadSeasonInfo.DoWork
        Try

            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Me.MainPoster.Clear()
            Me.MainFanart.Clear()

            Master.currShow = Master.DB.LoadTVSeasonFromDB(Args.ID, Args.Season, True)

            If bwLoadSeasonInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayPoster Then Me.MainPoster.FromFile(Master.currShow.SeasonPosterPath)

            If bwLoadSeasonInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayFanart Then
                If Not String.IsNullOrEmpty(Master.currShow.SeasonFanartPath) Then
                    Me.MainFanart.FromFile(Master.currShow.SeasonFanartPath)
                Else
                    Me.MainFanart.FromFile(Master.currShow.ShowFanartPath)
                    If Not IsNothing(Me.MainFanart.Image) Then Me.MainFanart.Image = ImageUtils.GrayScale(Me.MainFanart.Image)
                End If
            End If

            If bwLoadSeasonInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwLoadSeasonInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadSeasonInfo.RunWorkerCompleted
        Try
            If Not e.Cancelled Then
                Me.fillScreenInfoWithSeason()
            Else
                Me.SetControlsEnabled(True)
            End If
            Me.dgvTVSeasons.ResumeLayout()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadShowInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadShowInfo.DoWork
        Try

            Dim Args As Arguments = DirectCast(e.Argument, Arguments)
            Me.MainFanart.Clear()
            Me.MainPoster.Clear()

            If bwLoadShowInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            Master.currShow = Master.DB.LoadTVFullShowFromDB(Args.ID)

            If bwLoadShowInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayFanart Then Me.MainFanart.FromFile(Master.currShow.ShowFanartPath)

            If bwLoadShowInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

            If Not Master.eSettings.NoDisplayPoster Then Me.MainPoster.FromFile(Master.currShow.ShowPosterPath)

            If Master.eSettings.DisplayAllSeason AndAlso Master.eSettings.AllSeasonPosterEnabled Then
                Me.MainAllSeason.FromFile(Master.currShow.SeasonPosterPath)
            End If

            If bwLoadShowInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwLoadShowInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadShowInfo.RunWorkerCompleted
        Try
            If Not e.Cancelled Then
                Me.fillScreenInfoWithShow()
            Else
                Me.SetControlsEnabled(True)
            End If
            Me.dgvTVShows.ResumeLayout()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwMediaInfo_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwMediaInfo.DoWork
        '//
        ' Thread to procure technical and tag information about media via MediaInfo.dll
        '\\
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)

        Try
            If Args.IsTV Then
                MediaInfo.UpdateTVMediaInfo(Args.TVShow)
                Master.DB.SaveTVEpToDB(Args.TVShow, False, False, False, True)
                e.Result = New Results With {.fileinfo = NFO.FIToString(Args.TVShow.TVEp.FileInfo, True), .TVShow = Args.TVShow, .IsTV = True, .setEnabled = Args.setEnabled}
            Else
                MediaInfo.UpdateMediaInfo(Args.Movie)
                Master.DB.SaveMovieToDB(Args.Movie, False, False, True)
                e.Result = New Results With {.fileinfo = NFO.FIToString(Args.Movie.Movie.FileInfo, False), .setEnabled = Args.setEnabled, .Path = Args.Path, .Movie = Args.Movie}
            End If

            If Me.bwMediaInfo.CancellationPending Then
                e.Cancel = True
                Return
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            e.Result = New Results With {.fileinfo = "error", .setEnabled = Args.setEnabled}
            e.Cancel = True
        End Try
    End Sub

    Private Sub bwMediaInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwMediaInfo.RunWorkerCompleted
        '//
        ' Thread finished: fill textbox with result
        '\\

        If Not e.Cancelled Then
            Dim Res As Results = DirectCast(e.Result, Results)

            Try
                If Not Res.fileInfo = "error" Then
                    Me.pbMILoading.Visible = False
                    Me.txtMetaData.Text = Res.fileInfo

                    If Res.IsTV Then
                        If Master.eSettings.ScanTVMediaInfo Then
                            Me.SetAVImages(APIXML.GetAVImages(Res.TVShow.TVEp.FileInfo, Res.TVShow.Filename, True, ""))
                            Me.pnlInfoIcons.Width = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + pbStudio.Width + 6
                            Me.pbStudio.Left = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + 5
                        Else
                            Me.pnlInfoIcons.Width = pbStudio.Width + 1
                            Me.pbStudio.Left = 0
                        End If
                    Else
                        If Master.eSettings.ScanMediaInfo Then
                            Me.SetAVImages(APIXML.GetAVImages(Res.Movie.Movie.FileInfo, Res.Movie.Filename, False, Res.Movie.FileSource))
                            Me.pnlInfoIcons.Width = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + pbStudio.Width + 6
                            Me.pbStudio.Left = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + 5
                        Else
                            Me.pnlInfoIcons.Width = pbStudio.Width + 1
                            Me.pbStudio.Left = 0
                        End If

                        If Master.eSettings.UseMIDuration Then
                            If Not String.IsNullOrEmpty(Res.Movie.Movie.Runtime) Then
                                Me.lblRuntime.Text = String.Format(Master.eLang.GetString(112, "Runtime: {0}"), Res.Movie.Movie.Runtime)
                            End If
                        End If
                    End If
                    Me.btnMetaDataRefresh.Focus()
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            If Res.setEnabled Then
                Me.tabsMain.Enabled = True
                Me.tsbRefreshMedia.Enabled = True
                Me.cmnuTrayIconUpdateMedia.Enabled = True
                If (Me.tabsMain.SelectedIndex = 0 AndAlso Me.dgvMediaList.RowCount > 0) OrElse _
                   (Me.tabsMain.SelectedIndex = 1 AndAlso Me.dgvTVShows.RowCount > 0) Then
                    Me.SetControlsEnabled(True)
                End If
            End If
        End If
    End Sub

    Private Sub bwMovieScraper_Completed(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwMovieScraper.RunWorkerCompleted
        Dim Res As Results = DirectCast(e.Result, Results)
        If isCL Then
            Me.ScraperDone = True
        End If

        If Res.scrapeType = Enums.ScrapeType.SingleScrape Then
            Me.MovieInfoDownloaded()
        Else
            If Me.dgvMediaList.SelectedRows.Count > 0 Then
                Me.SelectRow(Me.dgvMediaList.SelectedRows(0).Index)
            Else
                Me.ClearInfo()
            End If
            Me.tslLoading.Visible = False
            Me.tspbLoading.Visible = False
            Me.btnCancel.Visible = False
            Me.lblCanceling.Visible = False
            Me.pbCanceling.Visible = False
            Me.pnlCancel.Visible = False
            Me.SetControlsEnabled(True)
        End If
    End Sub

    Private Sub bwMovieScraper_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwMovieScraper.DoWork
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)
        Dim OldTitle As String = String.Empty
        Dim NewTitle As String = String.Empty

        AddHandler ModulesManager.Instance.MovieScraperEvent, AddressOf MovieScraperEvent

        For Each dRow As DataRow In ScrapeList
            Try
                If bwMovieScraper.CancellationPending Then Exit For
                OldTitle = dRow.Item(3).ToString
                bwMovieScraper.ReportProgress(1, OldTitle)

                dScrapeRow = dRow
                Dim DBScrapeMovie As Structures.DBMovie = Master.DB.LoadMovieFromDB(Convert.ToInt64(dRow.Item(0)))
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.BeforeEditMovie, Nothing, DBScrapeMovie)
                If Not ModulesManager.Instance.MovieScrapeOnly(DBScrapeMovie, Args.scrapeType, Args.Options) Then
                    If Master.eSettings.ScanMediaInfo AndAlso Master.GlobalScrapeMod.Meta Then
                        MediaInfo.UpdateMediaInfo(DBScrapeMovie)
                    End If
                    If bwMovieScraper.CancellationPending Then Exit For
                    If Not Args.scrapeType = Enums.ScrapeType.SingleScrape Then
                        MovieScraperEvent(Enums.MovieScraperEventType.NFOItem, True)

                        NewTitle = DBScrapeMovie.ListTitle

                        If Not NewTitle = OldTitle Then
                            bwMovieScraper.ReportProgress(0, String.Format(Master.eLang.GetString(812, "Old Title: {0} | New Title: {1}"), OldTitle, NewTitle))
                        End If

                        MovieScraperEvent(Enums.MovieScraperEventType.ListTitle, NewTitle)
                        MovieScraperEvent(Enums.MovieScraperEventType.SortTitle, DBScrapeMovie.Movie.SortTitle)

                        Dim didEts As Interfaces.ModuleResult = ModulesManager.Instance.MoviePostScrapeOnly(DBScrapeMovie, Args.scrapeType)

                        If bwMovieScraper.CancellationPending Then Exit For

                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieScraperRDYtoSave, Nothing, DBScrapeMovie)

                        If Master.GlobalScrapeMod.Extra Then
                            If Master.eSettings.AutoThumbs > 0 AndAlso DBScrapeMovie.isSingle Then
                                Dim params As New List(Of Object)(New Object() {DBScrapeMovie, Master.eSettings.AutoThumbs, False, ""})
                                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.RandomFrameExtrator, params, Nothing, True)
                                MovieScraperEvent(Enums.MovieScraperEventType.ThumbsItem, True)
                                Dim ETasFA As String = DirectCast(params(3), String)
                                If Not String.IsNullOrEmpty(ETasFA) Then
                                    DBScrapeMovie.ExtraPath = "TRUE"
                                    If Not ETasFA = "TRUE" Then
                                        MovieScraperEvent(Enums.MovieScraperEventType.FanartItem, True)
                                        DBScrapeMovie.FanartPath = ETasFA
                                    End If
                                End If
                            End If
                        End If

                        Master.DB.SaveMovieToDB(DBScrapeMovie, False, False, Not String.IsNullOrEmpty(DBScrapeMovie.Movie.IMDBID))
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieSync, Nothing, DBScrapeMovie)
                        bwMovieScraper.ReportProgress(-1, If(Not OldTitle = NewTitle, String.Format(Master.eLang.GetString(812, "Old Title: {0} | New Title: {1}"), OldTitle, NewTitle), NewTitle))
                        bwMovieScraper.ReportProgress(-2, dScrapeRow.Item(0).ToString)
                    Else
                        Master.tmpMovie = DBScrapeMovie.Movie
                    End If
                Else
                    Master.tmpMovie = DBScrapeMovie.Movie
                    Args.scrapeType = Enums.ScrapeType.None
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            End Try
        Next

        RemoveHandler ModulesManager.Instance.MovieScraperEvent, AddressOf MovieScraperEvent
        e.Result = New Results With {.scrapeType = Args.scrapeType}
    End Sub

    Private Sub bwMovieScraper_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwMovieScraper.ProgressChanged
        If e.ProgressPercentage = -1 Then
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"moviescraped", 3, Master.eLang.GetString(813, "Movie Scraped"), e.UserState.ToString, Nothing}))
        ElseIf e.ProgressPercentage = -2 Then
            If Me.dgvMediaList.SelectedRows.Count > 0 AndAlso Me.dgvMediaList.SelectedRows(0).Cells(0).Value.ToString = e.UserState.ToString Then
                If Me.dgvMediaList.CurrentCell Is Me.dgvMediaList.SelectedRows(0).Cells(3) Then
                    Me.SelectRow(Me.dgvMediaList.SelectedRows(0).Index)
                End If
            End If
        Else
            Me.tspbLoading.Value += e.ProgressPercentage
            Me.SetStatus(e.UserState.ToString)
        End If
    End Sub

    Private Sub bwNonScrape_Completed(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwNonScrape.RunWorkerCompleted
        Me.tslLoading.Visible = False
        Me.tspbLoading.Visible = False
        Me.btnCancel.Visible = False
        Me.lblCanceling.Visible = False
        Me.pbCanceling.Visible = False
        Me.pnlCancel.Visible = False
        Me.SetControlsEnabled(True)
        Me.EnableFilters(True)
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub bwNonScrape_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwNonScrape.DoWork
        Dim scrapeMovie As Structures.DBMovie
        Dim iCount As Integer = 0
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)
        Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()

            Try
                If Me.dtMedia.Rows.Count > 0 Then

                    Select Case Args.scrapeType
                        Case Enums.ScrapeType.CleanFolders
                            Dim fDeleter As New FileUtils.Delete
                            For Each drvRow As DataRow In Me.dtMedia.Rows
                                Try
                                    Me.bwNonScrape.ReportProgress(iCount, drvRow.Item(15))
                                    iCount += 1
                                    If Convert.ToBoolean(drvRow.Item(14)) Then Continue For

                                    If Me.bwNonScrape.CancellationPending Then GoTo doCancel

                                    scrapeMovie = Master.DB.LoadMovieFromDB(Convert.ToInt64(drvRow.Item(0)))

                                    fDeleter.GetItemsToDelete(True, scrapeMovie)

                                    Me.RefreshMovie(Convert.ToInt64(drvRow.Item(0)), True, True)

                                    Me.bwNonScrape.ReportProgress(iCount, String.Format("[[{0}]]", drvRow.Item(0).ToString))
                                Catch ex As Exception
                                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
                                End Try
                            Next
                        Case Enums.ScrapeType.CopyBD
                            Dim sPath As String = String.Empty
                            For Each drvRow As DataRow In Me.dtMedia.Rows
                                Try
                                    Me.bwNonScrape.ReportProgress(iCount, drvRow.Item(15).ToString)
                                    iCount += 1

                                    If Me.bwNonScrape.CancellationPending Then GoTo doCancel
                                    sPath = drvRow.Item(37).ToString
                                    If Not String.IsNullOrEmpty(sPath) Then
                                        If FileUtils.Common.isVideoTS(sPath) Then
                                            If Master.eSettings.VideoTSParent Then
                                                FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Directory.GetParent(Directory.GetParent(sPath).FullName).Name), "-fanart.jpg")))
                                            Else
                                                If Path.GetFileName(sPath).ToLower = "fanart.jpg" Then
                                                    FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(sPath).FullName).Name, "-fanart.jpg")))
                                                Else
                                                    FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, Path.GetFileName(sPath)))
                                                End If
                                            End If
                                        ElseIf FileUtils.Common.isBDRip(sPath) Then
                                            If Master.eSettings.VideoTSParent Then
                                                FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).FullName, Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).Name), "-fanart.jpg")))
                                            Else
                                                If Path.GetFileName(sPath).ToLower = "fanart.jpg" Then
                                                    FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName).Name, "-fanart.jpg")))
                                                Else
                                                    FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, Path.GetFileName(sPath)))
                                                End If
                                            End If
                                        Else
                                            If Path.GetFileName(sPath).ToLower = "fanart.jpg" Then
                                                FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, String.Concat(Path.GetFileNameWithoutExtension(drvRow.Item(1).ToString), "-fanart.jpg")))
                                            Else
                                                FileUtils.Common.MoveFileWithStream(sPath, Path.Combine(Master.eSettings.BDPath, Path.GetFileName(sPath)))
                                            End If

                                        End If
                                    End If
                                Catch ex As Exception
                                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
                                End Try
                            Next
                    End Select

doCancel:
                    If Not Args.scrapeType = Enums.ScrapeType.CopyBD Then
                        SQLtransaction.Commit()
                    End If
                End If
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
            End Try
        End Using
    End Sub

    Private Sub bwNonScrape_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwNonScrape.ProgressChanged
        If Not isCL Then
            If Regex.IsMatch(e.UserState.ToString, "\[\[[0-9]+\]\]") AndAlso Me.dgvMediaList.SelectedRows.Count > 0 Then
                Try
                    If Me.dgvMediaList.SelectedRows(0).Cells(0).Value.ToString = e.UserState.ToString.Replace("[[", String.Empty).Replace("]]", String.Empty).Trim Then
                        Me.SelectRow(Me.dgvMediaList.SelectedRows(0).Index)
                    End If
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
                End Try
            Else
                Me.SetStatus(e.UserState.ToString)
                Me.tspbLoading.Value = e.ProgressPercentage
            End If
        End If

        Me.dgvMediaList.Invalidate()
    End Sub

    Private Sub bwRefreshMovies_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwRefreshMovies.DoWork
        Dim iCount As Integer = 0
        Dim MovieIDs As New Dictionary(Of Long, String)

        For Each sRow As DataRow In Me.dtMedia.Rows
            MovieIDs.Add(Convert.ToInt64(sRow.Item(0)), sRow.Item(3).ToString)
        Next

        Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
            For Each KVP As KeyValuePair(Of Long, String) In MovieIDs
                Try
                    If Me.bwMovieScraper.CancellationPending Then Return
                    Me.bwRefreshMovies.ReportProgress(iCount, KVP.Value)
                    Me.RefreshMovie(KVP.Key, True)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
                End Try
                iCount += 1
            Next
            SQLtransaction.Commit()
        End Using
    End Sub

    Private Sub bwRefreshMovies_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwRefreshMovies.ProgressChanged
        Me.SetStatus(e.UserState.ToString)
        Me.tspbLoading.Value = e.ProgressPercentage
    End Sub

    Private Sub bwRefreshMovies_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwRefreshMovies.RunWorkerCompleted
        Me.tslLoading.Text = String.Empty
        Me.tspbLoading.Visible = False
        Me.tslLoading.Visible = False

        Me.FillList(0)
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub cbFilterFileSource_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFilterFileSource.SelectedIndexChanged
        Try
            While Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadInfo.IsBusy OrElse Me.bwDownloadPic.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwCleanDB.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            For i As Integer = Me.FilterArray.Count - 1 To 0 Step -1
                If Me.FilterArray(i).ToString.StartsWith("FileSource =") Then
                    Me.FilterArray.RemoveAt(i)
                End If
            Next

            If Not cbFilterFileSource.Text = Master.eLang.All Then
                Me.FilterArray.Add(String.Format("FileSource = '{0}'", If(cbFilterFileSource.Text = Master.eLang.None, String.Empty, cbFilterFileSource.Text)))
            End If

            Me.RunFilter()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cbFilterYearMod_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFilterYearMod.SelectedIndexChanged
        Try
            If Not String.IsNullOrEmpty(cbFilterYear.Text) AndAlso Not cbFilterYear.Text = Master.eLang.All Then
                Me.FilterArray.Remove(Me.filYear)
                Me.filYear = String.Empty

                Me.filYear = String.Concat("Year ", cbFilterYearMod.Text, " '", cbFilterYear.Text, "'")

                Me.FilterArray.Add(Me.filYear)
                Me.RunFilter()
            Else
                If Not String.IsNullOrEmpty(Me.filYear) Then
                    Me.FilterArray.Remove(Me.filYear)
                    Me.filYear = String.Empty
                    Me.RunFilter()
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub cbFilterYear_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFilterYear.SelectedIndexChanged
        Try
            If Not String.IsNullOrEmpty(cbFilterYearMod.Text) AndAlso Not cbFilterYear.Text = Master.eLang.All Then
                Me.FilterArray.Remove(Me.filYear)
                Me.filYear = String.Empty

                Me.filYear = String.Concat("Year ", cbFilterYearMod.Text, " '", cbFilterYear.Text, "'")

                Me.FilterArray.Add(Me.filYear)
                Me.RunFilter()
            Else
                If Not String.IsNullOrEmpty(Me.filYear) Then
                    Me.FilterArray.Remove(Me.filYear)
                    Me.filYear = String.Empty
                    Me.RunFilter()
                End If

                If cbFilterYear.Text = Master.eLang.All Then
                    Me.cbFilterYearMod.Text = String.Empty
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub cbSearch_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbSearch.SelectedIndexChanged
        Me.txtSearch.Text = String.Empty
    End Sub

    Private Sub chkFilterDupe_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFilterDupe.Click
        Try
            Me.RunFilter(True)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub chkFilterLock_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkFilterLock.Click
        Try
            If Me.chkFilterLock.Checked Then
                Me.FilterArray.Add("Lock = 1")
            Else
                Me.FilterArray.Remove("Lock = 1")
            End If
            Me.RunFilter()
        Catch
        End Try
    End Sub

    Private Sub chkFilterMark_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFilterMark.Click
        Try
            If Me.chkFilterMark.Checked Then
                Me.FilterArray.Add("mark = 1")
            Else
                Me.FilterArray.Remove("mark = 1")
            End If
            Me.RunFilter()
        Catch
        End Try
    End Sub

    Private Sub chkFilterMissing_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkFilterMissing.Click
        Try
            Dim MissingFilter As New List(Of String)
            If Me.chkFilterMissing.Checked Then
                With Master.eSettings
                    If .MissingFilterPoster Then MissingFilter.Add("HasPoster = 0")
                    If .MissingFilterFanart Then MissingFilter.Add("HasFanart = 0")
                    If .MissingFilterNFO Then MissingFilter.Add("HasNfo = 0")
                    If .MissingFilterTrailer Then MissingFilter.Add("HasTrailer = 0")
                    If .MissingFilterSubs Then MissingFilter.Add("HasSub = 0")
                    If .MissingFilterExtras Then MissingFilter.Add("HasExtra = 0")
                End With
                filMissing = Microsoft.VisualBasic.Strings.Join(MissingFilter.ToArray, " OR ")
                Me.FilterArray.Add(filMissing)
            Else
                Me.FilterArray.Remove(filMissing)
            End If
            Me.RunFilter()
        Catch
        End Try
    End Sub

    Private Sub chkFilterNew_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkFilterNew.Click
        Try
            If Me.chkFilterNew.Checked Then
                Me.FilterArray.Add("new = 1")
            Else
                Me.FilterArray.Remove("new = 1")
            End If
            Me.RunFilter()
        Catch
        End Try
    End Sub

    Private Sub chkFilterTolerance_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkFilterTolerance.Click
        Try
            If Me.chkFilterTolerance.Checked Then
                Me.FilterArray.Add("OutOfTolerance = 1")
            Else
                Me.FilterArray.Remove("OutOfTolerance = 1")
            End If
            Me.RunFilter()
        Catch
        End Try
    End Sub

    Private Sub clbFilterGenres_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles clbFilterGenres.LostFocus
        Try
            Me.pnlFilterGenre.Visible = False
            Me.pnlFilterGenre.Tag = "NO"

            If clbFilterGenres.CheckedItems.Count > 0 Then
                Me.txtFilterGenre.Text = String.Empty
                Me.FilterArray.Remove(Me.filGenre)

                Dim alGenres As New List(Of String)
                alGenres.AddRange(clbFilterGenres.CheckedItems.OfType(Of String).ToList)

                If rbFilterAnd.Checked Then
                    Me.txtFilterGenre.Text = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " AND ")
                Else
                    Me.txtFilterGenre.Text = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " OR ")
                End If

                For i As Integer = 0 To alGenres.Count - 1
                    alGenres.Item(i) = String.Format("Genre LIKE '%{0}%'", alGenres.Item(i))
                Next

                If rbFilterAnd.Checked Then
                    Me.filGenre = String.Format("({0})", Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " AND "))
                Else
                    Me.filGenre = String.Format("({0})", Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " OR "))
                End If

                Me.FilterArray.Add(Me.filGenre)
                Me.RunFilter()
            Else
                If Not String.IsNullOrEmpty(Me.filGenre) Then
                    Me.txtFilterGenre.Text = String.Empty
                    Me.FilterArray.Remove(Me.filGenre)
                    Me.filGenre = String.Empty
                    Me.RunFilter()
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub clbFilterSource_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles clbFilterSource.LostFocus
        Try
            Me.pnlFilterSource.Visible = False
            Me.pnlFilterSource.Tag = "NO"

            If clbFilterSource.CheckedItems.Count > 0 Then
                Me.txtFilterSource.Text = String.Empty
                Me.FilterArray.Remove(Me.filSource)

                Dim alSource As New List(Of String)
                alSource.AddRange(clbFilterSource.CheckedItems.OfType(Of String).ToList)

                Me.txtFilterSource.Text = Microsoft.VisualBasic.Strings.Join(alSource.ToArray, " | ")

                For i As Integer = 0 To alSource.Count - 1
                    alSource.Item(i) = String.Format("Source = '{0}'", alSource.Item(i))
                Next

                Me.filSource = String.Format("({0})", Microsoft.VisualBasic.Strings.Join(alSource.ToArray, " OR "))

                Me.FilterArray.Add(Me.filSource)
                Me.RunFilter()
            Else
                If Not String.IsNullOrEmpty(Me.filSource) Then
                    Me.txtFilterSource.Text = String.Empty
                    Me.FilterArray.Remove(Me.filSource)
                    Me.filSource = String.Empty
                    Me.RunFilter()
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub CleanDatabaseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CleanDatabaseToolStripMenuItem.Click, CleanDatabaseToolStripMenuItem1.Click
        Me.SetControlsEnabled(False, True)
        Me.tspbLoading.Style = ProgressBarStyle.Marquee
        Me.EnableFilters(False)

        Me.SetStatus(Master.eLang.GetString(644, "Cleaning Database..."))
        Me.tspbLoading.Visible = True

        Me.bwCleanDB.WorkerSupportsCancellation = True
        Me.bwCleanDB.RunWorkerAsync()
    End Sub

    Private Sub CleanFiles()
        Try
            Dim sWarning As String = String.Empty
            Dim sWarningFile As String = String.Empty
            With Master.eSettings
                If .ExpertCleaner Then
                    sWarning = String.Concat(Master.eLang.GetString(102, "WARNING: If you continue, all non-whitelisted file types will be deleted!"), vbNewLine, vbNewLine, Master.eLang.GetString(101, "Are you sure you want to continue?"))
                Else
                    If .CleanDotFanartJPG Then sWarningFile += String.Concat("<movie>.fanart.jpg", vbNewLine)
                    If .CleanFanartJPG Then sWarningFile += String.Concat("fanart.jpg", vbNewLine)
                    If .CleanFolderJPG Then sWarningFile += String.Concat("folder.jpg", vbNewLine)
                    If .CleanMovieFanartJPG Then sWarningFile += String.Concat("<movie>-fanart.jpg", vbNewLine)
                    If .CleanMovieJPG Then sWarningFile += String.Concat("movie.jpg", vbNewLine)
                    If .CleanMovieNameJPG Then sWarningFile += String.Concat("<movie>.jpg", vbNewLine)
                    If .CleanMovieNFO Then sWarningFile += String.Concat("movie.nfo", vbNewLine)
                    If .CleanMovieNFOB Then sWarningFile += String.Concat("<movie>.nfo", vbNewLine)
                    If .CleanMovieTBN Then sWarningFile += String.Concat("movie.tbn", vbNewLine)
                    If .CleanMovieTBNB Then sWarningFile += String.Concat("<movie>.tbn", vbNewLine)
                    If .CleanPosterJPG Then sWarningFile += String.Concat("poster.jpg", vbNewLine)
                    If .CleanPosterTBN Then sWarningFile += String.Concat("poster.tbn", vbNewLine)
                    If .CleanExtraThumbs Then sWarningFile += String.Concat("/extrathumbs/", vbNewLine)
                    sWarning = String.Concat(Master.eLang.GetString(103, "WARNING: If you continue, all files of the following types will be permanently deleted:"), vbNewLine, vbNewLine, sWarningFile, vbNewLine, Master.eLang.GetString(101, "Are you sure you want to continue?"))
                End If
            End With
            If MsgBox(sWarning, MsgBoxStyle.Critical Or MsgBoxStyle.YesNo, Master.eLang.GetString(104, "Are you sure?")) = MsgBoxResult.Yes Then
                Me.NonScrape(Enums.ScrapeType.CleanFolders, Nothing)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub CleanFoldersToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CleanFoldersToolStripMenuItem.Click, CleanFilesToolStripMenuItem.Click
        Me.CleanFiles()
    End Sub

    Private Sub ClearAllCachesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClearAllCachesToolStripMenuItem.Click, ClearAllCachesToolStripMenuItem1.Click
        Me.ClearCache()
    End Sub

    Private Sub ClearCache()
        If Directory.Exists(Master.TempPath) Then
            Dim dInfo As New DirectoryInfo(Master.TempPath)
            For Each dDir As DirectoryInfo In dInfo.GetDirectories.Where(Function(d) Not d.Name.ToLower = "shows" AndAlso Not d.Name.ToLower = "addons")
                FileUtils.Delete.DeleteDirectory(dDir.FullName)
            Next

            For Each fFile As FileInfo In dInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                fFile.Delete()
            Next
        Else
            Directory.CreateDirectory(Master.TempPath)
        End If
    End Sub

    Private Sub ClearFilters(Optional ByVal Reload As Boolean = False)
        Try
            Me.bsMedia.RemoveFilter()
            Me.FilterArray.Clear()
            Me.filSearch = String.Empty
            Me.filGenre = String.Empty
            Me.filYear = String.Empty
            Me.filSource = String.Empty

            RemoveHandler txtSearch.TextChanged, AddressOf txtSearch_TextChanged
            Me.txtSearch.Text = String.Empty
            AddHandler txtSearch.TextChanged, AddressOf txtSearch_TextChanged
            If Me.cbSearch.Items.Count > 0 Then
                Me.cbSearch.SelectedIndex = 0
            End If
            Me.chkFilterDupe.Checked = False
            Me.chkFilterTolerance.Checked = False
            Me.chkFilterMissing.Checked = False
            Me.chkFilterMark.Checked = False
            Me.chkFilterNew.Checked = False
            Me.chkFilterLock.Checked = False
            Me.rbFilterOr.Checked = False
            Me.rbFilterAnd.Checked = True
            Me.txtFilterGenre.Text = String.Empty
            For i As Integer = 0 To Me.clbFilterGenres.Items.Count - 1
                Me.clbFilterGenres.SetItemChecked(i, False)
            Next
            Me.txtFilterSource.Text = String.Empty
            For i As Integer = 0 To Me.clbFilterSource.Items.Count - 1
                Me.clbFilterSource.SetItemChecked(i, False)
            Next

            RemoveHandler cbFilterYear.SelectedIndexChanged, AddressOf cbFilterYear_SelectedIndexChanged
            If Me.cbFilterYear.Items.Count > 0 Then
                Me.cbFilterYear.SelectedIndex = 0
            End If
            AddHandler cbFilterYear.SelectedIndexChanged, AddressOf cbFilterYear_SelectedIndexChanged

            RemoveHandler cbFilterYearMod.SelectedIndexChanged, AddressOf cbFilterYearMod_SelectedIndexChanged
            If Me.cbFilterYearMod.Items.Count > 0 Then
                Me.cbFilterYearMod.SelectedIndex = 0
            End If
            AddHandler cbFilterYearMod.SelectedIndexChanged, AddressOf cbFilterYearMod_SelectedIndexChanged

            RemoveHandler cbFilterFileSource.SelectedIndexChanged, AddressOf cbFilterFileSource_SelectedIndexChanged
            If Me.cbFilterFileSource.Items.Count > 0 Then
                Me.cbFilterFileSource.SelectedIndex = 0
            End If
            AddHandler cbFilterFileSource.SelectedIndexChanged, AddressOf cbFilterFileSource_SelectedIndexChanged

            If Reload Then Me.FillList(0)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnShowOpenFolder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuShowOpenFolder.Click
        If Me.dgvTVShows.SelectedRows.Count > 0 Then
            Dim doOpen As Boolean = True
            If Me.dgvTVShows.SelectedRows.Count > 10 Then
                If Not MsgBox(String.Format(Master.eLang.GetString(635, "You have selected {0} folders to open. Are you sure you want to do this?"), Me.dgvTVShows.SelectedRows.Count), MsgBoxStyle.YesNo Or MsgBoxStyle.Question, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then doOpen = False
            End If

            If doOpen Then
                For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                    Using Explorer As New Diagnostics.Process

                        If Master.isWindows Then
                            Explorer.StartInfo.FileName = "explorer.exe"
                            Explorer.StartInfo.Arguments = String.Format("/select,""{0}""", sRow.Cells(7).Value.ToString)
                        Else
                            Explorer.StartInfo.FileName = "xdg-open"
                            Explorer.StartInfo.Arguments = String.Format("""{0}""", sRow.Cells(7).Value.ToString)
                        End If
                        Explorer.Start()
                    End Using
                Next
            End If
        End If
    End Sub

    Private Sub cmnuChangeEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuChangeEp.Click
        Me.SetControlsEnabled(False, True)
        Dim tEpisode As MediaContainers.EpisodeDetails = ModulesManager.Instance.ChangeEpisode(Convert.ToInt32(Master.currShow.ShowID), Me.tmpTVDB, Me.tmpLang)

        If Not IsNothing(tEpisode) Then
            Master.currShow.TVEp = tEpisode
            Master.currShow.EpPosterPath = tEpisode.Poster.SaveAsEpPoster(Master.currShow)

            Master.DB.SaveTVEpToDB(Master.currShow, False, True, False, True)

            Me.FillEpisodes(Convert.ToInt32(Master.currShow.ShowID), Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(2).Value))
        End If

        Me.SetControlsEnabled(True)
    End Sub

    Private Sub cmnuChangeShow_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmnuChangeShow.Click
        Me.SetControlsEnabled(False, True)
        Dim Lang As String = Me.dgvTVShows.Item(22, Me.dgvTVShows.SelectedRows(0).Index).Value.ToString
        ModulesManager.Instance.TVScrapeOnly(Convert.ToInt32(Me.dgvTVShows.Item(0, Me.dgvTVShows.SelectedRows(0).Index).Value), Me.dgvTVShows.Item(1, Me.dgvTVShows.SelectedRows(0).Index).Value.ToString, String.Empty, If(String.IsNullOrEmpty(Lang), Master.eSettings.TVDBLanguage, Lang), DirectCast(Convert.ToInt32(Me.dgvTVShows.Item(23, Me.dgvTVShows.SelectedRows(0).Index).Value), Enums.Ordering), Master.DefaultTVOptions, Enums.ScrapeType.FullAsk, False)
    End Sub

    Private Sub cmnuDeleteSeason_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmnuDeleteSeason.Click
        Try

            Dim SeasonsToDelete As New Dictionary(Of Long, Long)
            Dim ShowId As Long = -1
            Dim SeasonNum As Integer = -1

            For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                ShowId = Convert.ToInt64(sRow.Cells(0).Value)
                SeasonNum = Convert.ToInt32(sRow.Cells(2).Value)
                'seasonnum first... showid can't be key or else only one season will be deleted
                If Not SeasonsToDelete.ContainsKey(SeasonNum) Then
                    SeasonsToDelete.Add(SeasonNum, ShowId)
                End If
            Next

            If SeasonsToDelete.Count > 0 Then
                Using dlg As New dlgDeleteConfirm
                    If dlg.ShowDialog(SeasonsToDelete, Enums.DelType.Seasons) = DialogResult.OK Then
                        Me.FillSeasons(Convert.ToInt32(Me.dgvTVSeasons.Item(0, Me.currSeasonRow).Value))
                        Me.SetTVCount()
                    End If
                End Using
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuDeleteTVEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuDeleteTVEp.Click
        Try

            Dim EpsToDelete As New Dictionary(Of Long, Long)
            Dim EpId As Long = -1

            For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                EpId = Convert.ToInt64(sRow.Cells(0).Value)
                If Not EpsToDelete.ContainsKey(EpId) Then
                    EpsToDelete.Add(EpId, 0)
                End If
            Next

            If EpsToDelete.Count > 0 Then
                Using dlg As New dlgDeleteConfirm
                    If dlg.ShowDialog(EpsToDelete, Enums.DelType.Episodes) = DialogResult.OK Then
                        Me.FillEpisodes(Convert.ToInt32(Me.dgvTVSeasons.Item(0, Me.currSeasonRow).Value), Convert.ToInt32(Me.dgvTVSeasons.Item(2, Me.currSeasonRow).Value))
                        Me.SetTVCount()
                    End If
                End Using
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Private Sub cmnuDeleteTVShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuDeleteTVShow.Click
        Try

            Dim ShowsToDelete As New Dictionary(Of Long, Long)
            Dim ShowId As Long = -1

            For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                ShowId = Convert.ToInt64(sRow.Cells(0).Value)
                If Not ShowsToDelete.ContainsKey(ShowId) Then
                    ShowsToDelete.Add(ShowId, 0)
                End If
            Next

            If ShowsToDelete.Count > 0 Then
                Using dlg As New dlgDeleteConfirm
                    If dlg.ShowDialog(ShowsToDelete, Enums.DelType.Shows) = DialogResult.OK Then
                        Me.FillList(0)
                    End If
                End Using
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuEditEpisode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuEditEpisode.Click
        Try
            Dim indX As Integer = Me.dgvTVEpisodes.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvTVEpisodes.Item(0, indX).Value)
            Master.currShow = Master.DB.LoadTVEpFromDB(ID, True)

            Me.SetControlsEnabled(False)

            Using dEditEpisode As New dlgEditEpisode
                AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
                Select Case dEditEpisode.ShowDialog()
                    Case DialogResult.OK
                        If Me.RefreshEpisode(ID) Then
                            Me.FillEpisodes(Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVEp.Season)
                        End If
                End Select
                RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
            End Using

            Me.SetControlsEnabled(True)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuEditMovie_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuEditMovie.Click
        If Me.dgvMediaList.SelectedRows.Count > 1 Then Return
        Try
            Dim indX As Integer = Me.dgvMediaList.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvMediaList.Item(0, indX).Value)

            Me.SetControlsEnabled(False)

            Using dEditMovie As New dlgEditMovie(Me)
                AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                Select Case dEditMovie.ShowDialog()
                    Case DialogResult.OK
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieScraperRDYtoSave, Nothing, Master.currMovie)
                        Me.SetListItemAfterEdit(ID, indX)
                        If Me.RefreshMovie(ID) Then
                            Me.FillList(0)
                        Else
                            Me.SetControlsEnabled(True)
                        End If
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieSync, Nothing, Master.currMovie)
                    Case DialogResult.Retry
                        Functions.SetScraperMod(Enums.ModType.All, True, True)
                        Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                    Case DialogResult.Abort
                        Master.currMovie.ClearExtras = False
                        Functions.SetScraperMod(Enums.ModType.DoSearch, True)
                        Functions.SetScraperMod(Enums.ModType.All, True, False)
                        Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                    Case Else
                        If Me.InfoCleared Then
                            Me.LoadInfo(ID, Me.dgvMediaList.Item(1, indX).Value.ToString, True, False)
                        Else
                            Me.SetControlsEnabled(True)
                        End If
                End Select
                RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuEditShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuEditShow.Click
        Try
            Dim indX As Integer = Me.dgvTVShows.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvTVShows.Item(0, indX).Value)

            Master.currShow = Master.DB.LoadTVFullShowFromDB(ID)

            Me.SetControlsEnabled(False)

            Using dEditShow As New dlgEditShow

                Select Case dEditShow.ShowDialog()
                    Case DialogResult.OK
                        Me.SetShowListItemAfterEdit(ID, indX)
                        If Me.RefreshShow(ID, False, True, False, False) Then
                            Me.FillList(0)
                        Else
                            Me.SetControlsEnabled(True)
                        End If
                    Case Else
                        Me.SetControlsEnabled(True)
                End Select

            End Using

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuEpOpenFolder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuEpOpenFolder.Click
        If Me.dgvTVEpisodes.SelectedRows.Count > 0 Then
            Dim doOpen As Boolean = True
            Dim ePath As String = String.Empty

            If Me.dgvTVEpisodes.SelectedRows.Count > 10 Then
                If Not MsgBox(String.Format(Master.eLang.GetString(635, "You have selected {0} folders to open. Are you sure you want to do this?"), Me.dgvTVEpisodes.SelectedRows.Count), MsgBoxStyle.YesNo Or MsgBoxStyle.Question, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then doOpen = False
            End If

            If doOpen Then
                Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                        SQLCommand.CommandText = String.Concat("SELECT TVEpPath FROM TVEpPaths WHERE ID = ", sRow.Cells(9).Value.ToString, ";")
                        ePath = SQLCommand.ExecuteScalar.ToString

                        If Not String.IsNullOrEmpty(ePath) Then
                            Using Explorer As New Diagnostics.Process

                                If Master.isWindows Then
                                    Explorer.StartInfo.FileName = "explorer.exe"
                                    Explorer.StartInfo.Arguments = String.Format("/select,""{0}""", ePath)
                                Else
                                    Explorer.StartInfo.FileName = "xdg-open"
                                    Explorer.StartInfo.Arguments = String.Format("""{0}""", ePath)
                                End If
                                Explorer.Start()
                            End Using
                        End If
                    Next
                End Using
            End If
        End If
    End Sub

    Private Sub cmnuLockEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuLockEp.Click
        Try
            Dim setLock As Boolean = False
            If Me.dgvTVEpisodes.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                    'if any one item is set as unlocked, set menu to lock
                    'else they are all locked so set menu to unlock
                    If Not Convert.ToBoolean(sRow.Cells(11).Value) Then
                        setLock = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parLock As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parLock", DbType.Boolean, 0, "lock")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE TVShows SET lock = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                        parLock.Value = If(Me.dgvTVEpisodes.SelectedRows.Count > 1, setLock, Not Convert.ToBoolean(sRow.Cells(11).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(11).Value = parLock.Value
                    Next
                End Using

                'now check the status of all episodes in the season so we can update the season lock flag if needed
                Dim LockCount As Integer = 0
                Dim NotLockCount As Integer = 0
                For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                    If Convert.ToBoolean(sRow.Cells(11).Value) Then
                        LockCount += 1
                    Else
                        NotLockCount += 1
                    End If
                Next

                If LockCount = 0 OrElse NotLockCount = 0 Then
                    Using SQLSeacommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        Dim parSeaLock As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeaLock", DbType.Boolean, 0, "lock")
                        Dim parSeaID As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeaID", DbType.Int32, 0, "TVShowID")
                        Dim parSeason As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeason", DbType.Int32, 0, "Season")
                        SQLSeacommand.CommandText = "UPDATE TVSeason SET lock = (?) WHERE TVShowID = (?) AND Season = (?);"
                        If LockCount = 0 Then
                            parSeaLock.Value = False
                        ElseIf NotLockCount = 0 Then
                            parSeaLock.Value = True
                        End If
                        parSeaID.Value = Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(0).Value)
                        parSeason.Value = Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(2).Value)
                        SQLSeacommand.ExecuteNonQuery()
                        Me.dgvTVSeasons.SelectedRows(0).Cells(7).Value = parSeaLock.Value
                    End Using
                End If

                SQLtransaction.Commit()
            End Using

            Me.dgvTVEpisodes.Invalidate()
            Me.dgvTVSeasons.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuLockSeason_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuLockSeason.Click
        Try
            Dim setLock As Boolean = False
            If Me.dgvTVSeasons.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                    If Not Convert.ToBoolean(sRow.Cells(7).Value) Then
                        setLock = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parLock As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parLock", DbType.Boolean, 0, "mark")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "TVShowID")
                    Dim parSeason As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSeason", DbType.Int32, 0, "Season")
                    SQLcommand.CommandText = "UPDATE TVSeason SET Lock = (?) WHERE TVShowID = (?) AND Season = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                        parLock.Value = If(Me.dgvTVSeasons.SelectedRows.Count > 1, setLock, Not Convert.ToBoolean(sRow.Cells(7).Value))
                        parID.Value = sRow.Cells(0).Value
                        parSeason.Value = sRow.Cells(2).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(7).Value = parLock.Value

                        Using SQLECommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parELock As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parELock", DbType.Boolean, 0, "mark")
                            Dim parEID As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEID", DbType.Int32, 0, "TVShowID")
                            Dim parESeason As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parESeason", DbType.Int32, 0, "Season")
                            SQLECommand.CommandText = "UPDATE TVEps SET Lock = (?) WHERE TVShowID = (?) AND Season = (?);"
                            parELock.Value = parLock.Value
                            parEID.Value = parID.Value
                            parESeason.Value = parSeason.Value
                            SQLECommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                                eRow.Cells(11).Value = parLock.Value
                            Next
                        End Using
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Me.dgvTVSeasons.Invalidate()
            Me.dgvTVEpisodes.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuLockShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuLockShow.Click
        Try
            Dim setLock As Boolean = False
            If Me.dgvTVShows.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                    'if any one item is set as unlocked, set menu to lock
                    'else they are all locked so set menu to unlock
                    If Not Convert.ToBoolean(sRow.Cells(10).Value) Then
                        setLock = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parLock As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parLock", DbType.Boolean, 0, "lock")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE TVShows SET lock = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                        parLock.Value = If(Me.dgvTVShows.SelectedRows.Count > 1, setLock, Not Convert.ToBoolean(sRow.Cells(10).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(10).Value = parLock.Value

                        Using SQLSeaCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parSeaLock As SQLite.SQLiteParameter = SQLSeaCommand.Parameters.Add("parSeaLock", DbType.Boolean, 0, "lock")
                            Dim parSeaID As SQLite.SQLiteParameter = SQLSeaCommand.Parameters.Add("parSeaID", DbType.Int32, 0, "TVShowID")
                            SQLSeaCommand.CommandText = "UPDATE TVSeason SET lock = (?) WHERE TVShowID = (?);"
                            parSeaLock.Value = parLock.Value
                            parSeaID.Value = parID.Value
                            SQLSeaCommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVSeasons.Rows
                                eRow.Cells(7).Value = parLock.Value
                            Next
                        End Using

                        Using SQLECommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parELock As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parELock", DbType.Boolean, 0, "lock")
                            Dim parEID As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEID", DbType.Int32, 0, "TVShowID")
                            SQLECommand.CommandText = "UPDATE TVEps SET lock = (?) WHERE TVShowID = (?);"
                            parELock.Value = parLock.Value
                            parEID.Value = parID.Value
                            SQLECommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                                eRow.Cells(11).Value = parLock.Value
                            Next
                        End Using
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Me.dgvTVShows.Invalidate()
            Me.dgvTVSeasons.Invalidate()
            Me.dgvTVEpisodes.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuLock_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuLock.Click
        Try
            Dim setLock As Boolean = False
            If Me.dgvMediaList.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    'if any one item is set as unlocked, set menu to lock
                    'else they are all locked so set menu to unlock
                    If Not Convert.ToBoolean(sRow.Cells(14).Value) Then
                        setLock = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parLock As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parLock", DbType.Boolean, 0, "lock")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE movies SET lock = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                        parLock.Value = If(Me.dgvMediaList.SelectedRows.Count > 1, setLock, Not Convert.ToBoolean(sRow.Cells(14).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(14).Value = parLock.Value
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            If Me.chkFilterLock.Checked Then
                Me.dgvMediaList.ClearSelection()
                Me.dgvMediaList.CurrentCell = Nothing
                If Me.dgvMediaList.RowCount <= 0 Then Me.ClearInfo()
            End If

            Me.dgvMediaList.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuMarkEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuMarkEp.Click
        Try
            Dim setMark As Boolean = False
            If Me.dgvTVEpisodes.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                    'if any one item is set as unmarked, set menu to mark
                    'else they are all marked, so set menu to unmark
                    If Not Convert.ToBoolean(sRow.Cells(8).Value) Then
                        setMark = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE TVEps SET mark = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                        parMark.Value = If(Me.dgvTVEpisodes.SelectedRows.Count > 1, setMark, Not Convert.ToBoolean(sRow.Cells(8).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(8).Value = parMark.Value
                    Next
                End Using

                'now check the status of all episodes in the season so we can update the season mark flag if needed
                Dim MarkCount As Integer = 0
                Dim NotMarkCount As Integer = 0
                For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                    If Convert.ToBoolean(sRow.Cells(8).Value) Then
                        MarkCount += 1
                    Else
                        NotMarkCount += 1
                    End If
                Next

                If MarkCount = 0 OrElse NotMarkCount = 0 Then
                    Using SQLSeacommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        Dim parSeaMark As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeaMark", DbType.Boolean, 0, "Mark")
                        Dim parSeaID As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeaID", DbType.Int32, 0, "TVShowID")
                        Dim parSeason As SQLite.SQLiteParameter = SQLSeacommand.Parameters.Add("parSeason", DbType.Int32, 0, "Season")
                        SQLSeacommand.CommandText = "UPDATE TVSeason SET Mark = (?) WHERE TVShowID = (?) AND Season = (?);"
                        If MarkCount = 0 Then
                            parSeaMark.Value = False
                        ElseIf NotMarkCount = 0 Then
                            parSeaMark.Value = True
                        End If
                        parSeaID.Value = Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(0).Value)
                        parSeason.Value = Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(2).Value)
                        SQLSeacommand.ExecuteNonQuery()
                        Me.dgvTVSeasons.SelectedRows(0).Cells(8).Value = parSeaMark.Value
                    End Using
                End If

                SQLtransaction.Commit()
            End Using

            Me.dgvTVSeasons.Invalidate()
            Me.dgvTVEpisodes.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuMarkSeason_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuMarkSeason.Click
        Try
            Dim setMark As Boolean = False
            If Me.dgvTVSeasons.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                    If Not Convert.ToBoolean(sRow.Cells(8).Value) Then
                        setMark = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "TVShowID")
                    Dim parSeason As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSeason", DbType.Int32, 0, "Season")
                    SQLcommand.CommandText = "UPDATE TVSeason SET mark = (?) WHERE TVShowID = (?) AND Season = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                        parMark.Value = If(Me.dgvTVSeasons.SelectedRows.Count > 1, setMark, Not Convert.ToBoolean(sRow.Cells(8).Value))
                        parID.Value = sRow.Cells(0).Value
                        parSeason.Value = sRow.Cells(2).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(8).Value = parMark.Value

                        Using SQLECommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parEMark As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEMark", DbType.Boolean, 0, "mark")
                            Dim parEID As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEID", DbType.Int32, 0, "TVShowID")
                            Dim parESeason As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parESeason", DbType.Int32, 0, "Season")
                            SQLECommand.CommandText = "UPDATE TVEps SET mark = (?) WHERE TVShowID = (?) AND Season = (?);"
                            parEMark.Value = parMark.Value
                            parEID.Value = parID.Value
                            parESeason.Value = parSeason.Value
                            SQLECommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                                eRow.Cells(8).Value = parMark.Value
                            Next
                        End Using
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Me.dgvTVSeasons.Invalidate()
            Me.dgvTVEpisodes.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuMarkShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuMarkShow.Click
        Try
            Dim setMark As Boolean = False
            If Me.dgvTVShows.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                    'if any one item is set as unmarked, set menu to mark
                    'else they are all marked, so set menu to unmark
                    If Not Convert.ToBoolean(sRow.Cells(6).Value) Then
                        setMark = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE TVShows SET mark = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                        parMark.Value = If(Me.dgvTVShows.SelectedRows.Count > 1, setMark, Not Convert.ToBoolean(sRow.Cells(6).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(6).Value = parMark.Value

                        Using SQLSeaCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parSeaMark As SQLite.SQLiteParameter = SQLSeaCommand.Parameters.Add("parSeaMark", DbType.Boolean, 0, "mark")
                            Dim parSeaID As SQLite.SQLiteParameter = SQLSeaCommand.Parameters.Add("parSeaID", DbType.Int32, 0, "TVShowID")
                            SQLSeaCommand.CommandText = "UPDATE TVSeason SET mark = (?) WHERE TVShowID = (?);"
                            parSeaMark.Value = parMark.Value
                            parSeaID.Value = parID.Value
                            SQLSeaCommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVSeasons.Rows
                                eRow.Cells(8).Value = parMark.Value
                            Next
                        End Using

                        Using SQLECommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                            Dim parEMark As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEMark", DbType.Boolean, 0, "mark")
                            Dim parEID As SQLite.SQLiteParameter = SQLECommand.Parameters.Add("parEID", DbType.Int32, 0, "TVShowID")
                            SQLECommand.CommandText = "UPDATE TVEps SET mark = (?) WHERE TVShowID = (?);"
                            parEMark.Value = parMark.Value
                            parEID.Value = parID.Value
                            SQLECommand.ExecuteNonQuery()

                            For Each eRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                                eRow.Cells(8).Value = parMark.Value
                            Next
                        End Using
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Me.dgvTVShows.Invalidate()
            Me.dgvTVSeasons.Invalidate()
            Me.dgvTVEpisodes.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuMark_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuMark.Click
        Try
            Dim setMark As Boolean = False
            If Me.dgvMediaList.SelectedRows.Count > 1 Then
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    'if any one item is set as unmarked, set menu to mark
                    'else they are all marked, so set menu to unmark
                    If Not Convert.ToBoolean(sRow.Cells(11).Value) Then
                        setMark = True
                        Exit For
                    End If
                Next
            End If

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE movies SET mark = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                        parMark.Value = If(Me.dgvMediaList.SelectedRows.Count > 1, setMark, Not Convert.ToBoolean(sRow.Cells(11).Value))
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                        sRow.Cells(11).Value = parMark.Value
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            setMark = False
            For Each sRow As DataGridViewRow In Me.dgvMediaList.Rows
                If Convert.ToBoolean(sRow.Cells(11).Value) Then
                    setMark = True
                    Exit For
                End If
            Next
            Me.btnMarkAll.Text = If(setMark, Master.eLang.GetString(105, "Unmark All"), Master.eLang.GetString(35, "Mark All"))

            If Me.chkFilterMark.Checked Then
                Me.dgvMediaList.ClearSelection()
                Me.dgvMediaList.CurrentCell = Nothing
                If Me.dgvMediaList.RowCount <= 0 Then Me.ClearInfo()
            End If

            Me.dgvMediaList.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuMetaData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuMetaData.Click
        If Me.dgvMediaList.SelectedRows.Count > 1 Then Return
        Dim indX As Integer = Me.dgvMediaList.SelectedRows(0).Index
        Dim ID As Integer = Convert.ToInt32(Me.dgvMediaList.Item(0, indX).Value)
        Using dEditMeta As New dlgFileInfo
            Select Case dEditMeta.ShowDialog(False)
                Case DialogResult.OK
                    Me.SetListItemAfterEdit(ID, indX)
                    If Me.RefreshMovie(ID) Then
                        Me.FillList(0)
                    End If
            End Select
        End Using
    End Sub

    Private Sub cmnuRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRefresh.Click
        ReloadMovie()
    End Sub

    Private Sub cmnuReloadEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuReloadEp.Click
        Try
            Me.dgvTVShows.Cursor = Cursors.WaitCursor
            Me.dgvTVSeasons.Cursor = Cursors.WaitCursor
            Me.dgvTVEpisodes.Cursor = Cursors.WaitCursor
            Me.SetControlsEnabled(False, True)

            Dim doFill As Boolean = False
            Dim tFill As Boolean = False

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                    tFill = Me.RefreshEpisode(Convert.ToInt64(sRow.Cells(0).Value), True)
                    If tFill Then doFill = True
                Next

                Master.DB.CleanSeasons(True)

                SQLtransaction.Commit()
            End Using

            Me.dgvTVShows.Cursor = Cursors.Default
            Me.dgvTVSeasons.Cursor = Cursors.Default
            Me.dgvTVEpisodes.Cursor = Cursors.Default
            Me.SetControlsEnabled(True)

            If doFill Then FillEpisodes(Convert.ToInt32(Me.dgvTVEpisodes.SelectedRows(0).Cells(0).Value), Convert.ToInt32(Me.dgvTVEpisodes.SelectedRows(0).Cells(12).Value))
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuReloadSeason_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuReloadSeason.Click
        Me.dgvTVShows.Cursor = Cursors.WaitCursor
        Me.dgvTVSeasons.Cursor = Cursors.WaitCursor
        Me.dgvTVEpisodes.Cursor = Cursors.WaitCursor
        Me.SetControlsEnabled(False, True)

        Dim doFill As Boolean = False
        Dim tFill As Boolean = False

        If Me.dgvTVSeasons.SelectedRows.Count > 0 Then
            Using SQLTrans As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows

                    doFill = Me.RefreshSeason(Convert.ToInt32(sRow.Cells(0).Value), Convert.ToInt32(sRow.Cells(2).Value), True)

                    Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        SQLCommand.CommandText = String.Concat("SELECT ID FROM TVEps WHERE TVShowID = ", sRow.Cells(0).Value, " AND Season = ", sRow.Cells(2).Value, " AND Missing = 0;")
                        Using SQLReader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                            While SQLReader.Read
                                tFill = Me.RefreshEpisode(Convert.ToInt64(SQLReader("ID")), True)
                                If tFill Then doFill = True
                            End While
                        End Using
                    End Using
                Next

                Master.DB.CleanSeasons(True)

                SQLTrans.Commit()
            End Using
        End If

        Me.dgvTVShows.Cursor = Cursors.Default
        Me.dgvTVSeasons.Cursor = Cursors.Default
        Me.dgvTVEpisodes.Cursor = Cursors.Default
        Me.SetControlsEnabled(True)

        If doFill Then Me.FillSeasons(Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(0).Value))
    End Sub

    Private Sub cmnuReloadShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuReloadShow.Click
        Try
            Me.dgvTVShows.Cursor = Cursors.WaitCursor
            Me.dgvTVSeasons.Cursor = Cursors.WaitCursor
            Me.dgvTVEpisodes.Cursor = Cursors.WaitCursor
            Me.SetControlsEnabled(False, True)

            Dim doFill As Boolean = False
            Dim tFill As Boolean = False

            If Me.dgvTVShows.SelectedRows.Count > 1 Then
                Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                    For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                        tFill = Me.RefreshShow(Convert.ToInt64(sRow.Cells(0).Value), True, True, False, True)
                        If tFill Then doFill = True
                    Next
                    SQLtransaction.Commit()
                End Using
            ElseIf Me.dgvTVShows.SelectedRows.Count = 1 Then
                'seperate single refresh so we can have a progress bar
                tFill = Me.RefreshShow(Convert.ToInt64(Me.dgvTVShows.SelectedRows(0).Cells(0).Value), False, True, False, True)
                If tFill Then doFill = True
            End If

            Me.dgvTVShows.Cursor = Cursors.Default
            Me.dgvTVSeasons.Cursor = Cursors.Default
            Me.dgvTVEpisodes.Cursor = Cursors.Default
            Me.SetControlsEnabled(True)

            If doFill Then FillList(0)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub cmnuRemoveSeasonFromDB_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmnuRemoveSeasonFromDB.Click
        Me.ClearInfo(False)

        Using SQLTrans As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
            For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                Master.DB.DeleteTVSeasonFromDB(Convert.ToInt32(sRow.Cells(0).Value), Convert.ToInt32(sRow.Cells(2).Value), True)
            Next
            SQLTrans.Commit()
        End Using

        Me.FillSeasons(Convert.ToInt32(Me.dgvTVSeasons.SelectedRows(0).Cells(0).Value))

        Me.SetTVCount()
    End Sub

    Private Sub cmnuRemoveTVEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRemoveTVEp.Click
        Me.ClearInfo(False)

        Using SQLTrans As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
            For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                Master.DB.DeleteTVEpFromDB(Convert.ToInt32(sRow.Cells(0).Value), True, False, True)
            Next

            Master.DB.CleanSeasons(True)

            SQLTrans.Commit()
        End Using

        Dim cSeas As Integer = 0

        If Not Me.currSeasonRow = -1 Then
            cSeas = Me.currSeasonRow
        End If

        Me.FillEpisodes(Convert.ToInt32(Me.dgvTVSeasons.Item(0, cSeas).Value), Convert.ToInt32(Me.dgvTVSeasons.Item(2, cSeas).Value))

        Me.SetTVCount()
    End Sub

    Private Sub cmnuRemoveTVShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRemoveTVShow.Click
        Me.ClearInfo()

        Using SQLTrans As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
            For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                Master.DB.DeleteTVShowFromDB(Convert.ToInt32(sRow.Cells(0).Value), True)
            Next
            SQLTrans.Commit()
        End Using

        Me.FillList(0)
    End Sub

    Private Sub cmnuRescrapeEp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRescrapeEp.Click
        Me.SetControlsEnabled(False, True)
        ModulesManager.Instance.TVScrapeEpisode(Convert.ToInt32(Me.dgvTVEpisodes.Item(1, Me.dgvTVEpisodes.SelectedRows(0).Index).Value), Me.tmpTitle, Me.tmpTVDB, Convert.ToInt32(Me.dgvTVEpisodes.Item(2, Me.dgvTVEpisodes.SelectedRows(0).Index).Value), Convert.ToInt32(Me.dgvTVEpisodes.Item(12, Me.dgvTVEpisodes.SelectedRows(0).Index).Value), Me.tmpLang, Me.tmpOrdering, Master.DefaultTVOptions)
    End Sub

    Private Sub cmnuRescrapeShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRescrapeShow.Click
        Me.SetControlsEnabled(False, True)
        TVShowScrapeData()
    End Sub

    Sub TVShowScrapeData()
        Me.SetControlsEnabled(False)
        For Each s As DataGridViewRow In Me.dgvTVShows.SelectedRows
            ' Temporary Scratetype
            Dim ScrapeType As Enums.ScrapeType
            'If Me.dgvTVShows.SelectedRows.Count = 1 Then
            'ScrapeType = Enums.ScrapeType.FullAsk
            'Else
            'ScrapeType = Enums.ScrapeType.FullAuto
            'End If
            ScrapeType = Enums.ScrapeType.FullAsk
            Dim Lang As String = Me.dgvTVShows.Item(22, s.Index).Value.ToString
            ModulesManager.Instance.TVScrapeOnly(Convert.ToInt32(Me.dgvTVShows.Item(0, s.Index).Value), Me.dgvTVShows.Item(1, s.Index).Value.ToString, Me.dgvTVShows.Item(9, s.Index).Value.ToString, If(String.IsNullOrEmpty(Lang), Master.eSettings.TVDBLanguage, Lang), DirectCast(Convert.ToInt32(Me.dgvTVShows.Item(23, s.Index).Value), Enums.Ordering), Master.DefaultTVOptions, ScrapeType, True)
        Next
        Me.SetControlsEnabled(True)
    End Sub


    Private Sub cmnuRescrape_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuRescrape.Click
        If Me.dgvMediaList.SelectedRows.Count = 1 Then
            Functions.SetScraperMod(Enums.ModType.All, True, True)
            Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
        End If
    End Sub

    Private Sub cmnuSearchNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuSearchNew.Click
        If Me.dgvMediaList.SelectedRows.Count > 1 Then Return
        Functions.SetScraperMod(Enums.ModType.DoSearch, True)
        Functions.SetScraperMod(Enums.ModType.All, True, False)
        Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
    End Sub

    Private Sub cmnuSeasonChangeImages_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmnuSeasonChangeImages.Click
        Me.SetControlsEnabled(False)
        Using dEditSeason As New dlgEditSeason
            If dEditSeason.ShowDialog() = DialogResult.OK Then
                If Me.RefreshSeason(Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVEp.Season, False) Then
                    Me.FillSeasons(Convert.ToInt32(Master.currShow.ShowID))
                End If
            End If
        End Using
        Me.SetControlsEnabled(True)
    End Sub

    Private Sub cmnuSeasonOpenFolder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmnuSeasonOpenFolder.Click
        If Me.dgvTVSeasons.SelectedRows.Count > 0 Then
            Dim doOpen As Boolean = True
            Dim SeasonPath As String = String.Empty

            If Me.dgvTVSeasons.SelectedRows.Count > 10 Then
                If Not MsgBox(String.Format(Master.eLang.GetString(635, "You have selected {0} folders to open. Are you sure you want to do this?"), Me.dgvTVSeasons.SelectedRows.Count), MsgBoxStyle.YesNo Or MsgBoxStyle.Question, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then doOpen = False
            End If

            If doOpen Then
                For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                    SeasonPath = Functions.GetSeasonDirectoryFromShowPath(Master.currShow.ShowPath, Convert.ToInt32(sRow.Cells(2).Value))

                    Using Explorer As New Diagnostics.Process
                        If Master.isWindows Then
                            Explorer.StartInfo.FileName = "explorer.exe"
                            If String.IsNullOrEmpty(SeasonPath) Then
                                Explorer.StartInfo.Arguments = String.Format("/root,""{0}""", Master.currShow.ShowPath)
                            Else
                                Explorer.StartInfo.Arguments = String.Format("/select,""{0}""", SeasonPath)
                            End If

                        Else
                            Explorer.StartInfo.FileName = "xdg-open"
                            If String.IsNullOrEmpty(SeasonPath) Then
                                Explorer.StartInfo.Arguments = String.Format("""{0}""", Master.currShow.ShowPath)
                            Else
                                Explorer.StartInfo.Arguments = String.Format("""{0}""", SeasonPath)
                            End If
                        End If
                        Explorer.Start()
                    End Using
                Next
            End If
        End If
    End Sub

    Private Sub cmnuSeasonRescrape_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmnuSeasonRescrape.Click
        Me.SetControlsEnabled(False, True)
        ModulesManager.Instance.TVScrapeSeason(Convert.ToInt32(Me.dgvTVSeasons.Item(0, Me.dgvTVSeasons.SelectedRows(0).Index).Value), Me.tmpTitle, Me.tmpTVDB, Convert.ToInt32(Me.dgvTVSeasons.Item(2, Me.dgvTVSeasons.SelectedRows(0).Index).Value), Me.tmpLang, Me.tmpOrdering, Master.DefaultTVOptions)
    End Sub

    Private Sub ConvertFileSourceToFolderSourceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConvertFileSourceToFolderSourceToolStripMenuItem.Click, SortFilesIntoFoldersToolStripMenuItem.Click
        Me.SetControlsEnabled(False)
        Using dSortFiles As New dlgSortFiles
            If dSortFiles.ShowDialog() = DialogResult.OK Then
                Me.LoadMedia(New Structures.Scans With {.Movies = True})
            Else
                Me.SetControlsEnabled(True)
            End If
        End Using
    End Sub

    Private Sub CopyExistingFanartToBackdropsFolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyExistingFanartToBackdropsFolderToolStripMenuItem.Click, CopyExistingFanartToBackdropsFolderToolStripMenuItem1.Click
        Me.NonScrape(Enums.ScrapeType.CopyBD, Nothing)
    End Sub

    Private Sub createGenreThumbs(ByVal strGenres As String)
        '//
        ' Parse the genre string and create panels/pictureboxes for each one
        '\\

        Dim genreArray() As String

        Try
            genreArray = Microsoft.VisualBasic.Strings.Split(strGenres, " / ")
            For i As Integer = 0 To UBound(genreArray)
                ReDim Preserve Me.pnlGenre(i)
                ReDim Preserve Me.pbGenre(i)
                Me.pnlGenre(i) = New Panel()
                Me.pnlGenre(i).Visible = False
                Me.pbGenre(i) = New PictureBox()
                Me.pbGenre(i).Name = genreArray(i).Trim.ToUpper
                Me.pnlGenre(i).Size = New Size(68, 100)
                Me.pbGenre(i).Size = New Size(62, 94)
                Me.pnlGenre(i).BackColor = Me.GenrePanelColor
                Me.pbGenre(i).BackColor = Me.GenrePanelColor
                Me.pnlGenre(i).BorderStyle = BorderStyle.FixedSingle
                Me.pbGenre(i).SizeMode = PictureBoxSizeMode.StretchImage
                Me.pbGenre(i).Image = APIXML.GetGenreImage(genreArray(i).Trim)
                Me.pnlGenre(i).Left = ((Me.pnlInfoPanel.Right) - (i * 73)) - 73
                Me.pbGenre(i).Left = 2
                Me.pnlGenre(i).Top = Me.pnlInfoPanel.Top - 105
                Me.pbGenre(i).Top = 2
                Me.scMain.Panel2.Controls.Add(Me.pnlGenre(i))
                Me.pnlGenre(i).Controls.Add(Me.pbGenre(i))
                Me.pnlGenre(i).BringToFront()
                AddHandler Me.pbGenre(i).MouseEnter, AddressOf pbGenre_MouseEnter
                AddHandler Me.pbGenre(i).MouseLeave, AddressOf pbGenre_MouseLeave
                If Master.eSettings.AllwaysDisplayGenresText Then
                    Dim iLeft As Integer = 0
                    Me.GenreImage = pbGenre(i).Image
                    Dim bmGenre As New Bitmap(Me.GenreImage)
                    Dim grGenre As Graphics = Graphics.FromImage(bmGenre)
                    Dim drawString As String = pbGenre(i).Name
                    Dim drawFont As New Font("Microsoft Sans Serif", 14, FontStyle.Bold, GraphicsUnit.Pixel)
                    Dim drawBrush As New SolidBrush(Color.White)
                    Dim drawWidth As Single = grGenre.MeasureString(drawString, drawFont).Width
                    Dim drawSize As Integer = Convert.ToInt32((14 * (bmGenre.Width / drawWidth)) - 0.5)
                    drawFont = New Font("Microsoft Sans Serif", If(drawSize > 14, 14, drawSize), FontStyle.Bold, GraphicsUnit.Pixel)
                    Dim drawHeight As Single = grGenre.MeasureString(drawString, drawFont).Height
                    iLeft = Convert.ToInt32((bmGenre.Width - grGenre.MeasureString(drawString, drawFont).Width) / 2)
                    grGenre.DrawString(drawString, drawFont, drawBrush, iLeft, (bmGenre.Height - drawHeight))
                    pbGenre(i).Image = bmGenre
                End If
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub CustomUpdaterToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CustomUpdaterToolStripMenuItem.Click, TrayCustomUpdaterToolStripMenuItem.Click
        Me.SetControlsEnabled(False)
        Using dUpdate As New dlgUpdateMedia
            Dim CustomUpdater As Structures.CustomUpdaterStruct = Nothing
            CustomUpdater = dUpdate.ShowDialog()
            If Not CustomUpdater.Canceled Then
                Me.MovieScrapeData(False, CustomUpdater.ScrapeType, CustomUpdater.Options)
            Else
                Me.SetControlsEnabled(True)
            End If
        End Using
    End Sub

    Private Sub DeleteMovieToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeleteMovieToolStripMenuItem.Click
        Try
            Dim MoviesToDelete As New Dictionary(Of Long, Long)
            Dim MovieId As Int64 = -1

            For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                MovieId = Convert.ToInt64(sRow.Cells(0).Value)
                If Not MoviesToDelete.ContainsKey(MovieId) Then
                    MoviesToDelete.Add(MovieId, 0)
                End If
            Next

            If MoviesToDelete.Count > 0 Then
                Using dlg As New dlgDeleteConfirm
                    If dlg.ShowDialog(MoviesToDelete, Enums.DelType.Movies) = DialogResult.OK Then
                        Me.FillList(0)
                    End If
                End Using
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvMediaList.CellClick
        Try

            If e.ColumnIndex = 3 OrElse Not Master.eSettings.ClickScrape Then 'Title
                If Me.dgvMediaList.SelectedRows.Count > 0 Then
                    If Me.dgvMediaList.RowCount > 0 Then
                        If Me.dgvMediaList.SelectedRows.Count > 1 Then
                            Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvMediaList.SelectedRows.Count))
                        ElseIf Me.dgvMediaList.SelectedRows.Count = 1 Then
                            Me.SetStatus(Me.dgvMediaList.SelectedRows(0).Cells(1).Value.ToString)
                        End If
                    End If
                    Me.currRow = Me.dgvMediaList.SelectedRows(0).Index
                End If
            ElseIf Master.eSettings.ClickScrape AndAlso e.RowIndex >= 0 AndAlso e.ColumnIndex <> 8 AndAlso Not bwMovieScraper.IsBusy Then
                Dim movie As Int32 = CType(Me.dgvMediaList.Rows(e.RowIndex).Cells(0).Value, Int32)
                Dim objCell As DataGridViewCell = CType(Me.dgvMediaList.Rows(e.RowIndex).Cells(e.ColumnIndex), DataGridViewCell)

                'EMM not able to scrape subtitles yet.
                'So don't set status for it, but leave the option open for the future.
                Me.dgvMediaList.ClearSelection()
                Me.dgvMediaList.Rows(objCell.RowIndex).Selected = True
                Me.currRow = objCell.RowIndex
                Select Case e.ColumnIndex
                    Case 4 'Poster
                        Functions.SetScraperMod(Enums.ModType.Poster, True)
                    Case 5 'Fanart
                        Functions.SetScraperMod(Enums.ModType.Fanart, True)
                    Case 6 'Nfo
                        Functions.SetScraperMod(Enums.ModType.NFO, True)
                    Case 7 'Trailer
                        Functions.SetScraperMod(Enums.ModType.Trailer, True)
                    Case 8 'Subtitles
                        'Functions.SetScraperMod(Enums.ModType.Subtitles, True)
                    Case 9 'Extrathumbs
                        Functions.SetScraperMod(Enums.ModType.Extra, True)
                    Case 10 'Metadata - need to add this column to the view.
                        Functions.SetScraperMod(Enums.ModType.Meta, True)
                End Select
                If Master.eSettings.AskCheckboxScrape Then
                    MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
                Else
                    MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvMediaList.CellDoubleClick
        Try

            If e.RowIndex < 0 Then Exit Sub

            If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

            Dim indX As Integer = Me.dgvMediaList.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvMediaList.Item(0, indX).Value)
            Master.currMovie = Master.DB.LoadMovieFromDB(ID)

            Using dEditMovie As New dlgEditMovie(Me)
                AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                Select Case dEditMovie.ShowDialog()
                    Case DialogResult.OK
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieScraperRDYtoSave, Nothing, Master.currMovie)
                        Me.SetListItemAfterEdit(ID, indX)
                        If Me.RefreshMovie(ID) Then
                            Me.FillList(0)
                        End If
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieSync, Nothing, Master.currMovie)
                    Case DialogResult.Retry
                        Functions.SetScraperMod(Enums.ModType.All, True, True)
                        Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                    Case DialogResult.Abort
                        Functions.SetScraperMod(Enums.ModType.DoSearch, True)
                        Functions.SetScraperMod(Enums.ModType.All, True, False)
                        Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                    Case Else
                        If Me.InfoCleared Then Me.LoadInfo(ID, Me.dgvMediaList.Item(1, indX).Value.ToString, True, False)
                End Select
                RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_CellEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvMediaList.CellEnter
        Try
            If Not Me.tabsMain.SelectedIndex = 0 Then Return

            Me.tmrWaitShow.Stop()
            Me.tmrWaitSeason.Stop()
            Me.tmrWaitEp.Stop()
            Me.tmrWait.Stop()
            Me.tmrLoadShow.Stop()
            Me.tmrLoadSeason.Stop()
            Me.tmrLoadEp.Stop()
            Me.tmrLoad.Stop()

            Me.currRow = e.RowIndex
            Me.tmrWait.Start()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub


    Private Sub dgvMediaList_CellMouseEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvMediaList.CellMouseEnter
        'EMM not able to scrape subtitles yet.
        'So don't set status for it, but leave the option open for the future.
        If Master.eSettings.ClickScrape AndAlso e.RowIndex > 0 AndAlso e.ColumnIndex > 3 AndAlso e.ColumnIndex < 11 AndAlso e.ColumnIndex <> 8 AndAlso Not bwMovieScraper.IsBusy Then
            oldStatus = GetStatus()
            Dim movieName As String = Me.dgvMediaList.Rows(e.RowIndex).Cells(15).Value.ToString
            Dim scrapeFor As String = ""
            Dim scrapeType As String = ""
            Select Case e.ColumnIndex
                Case 4
                    scrapeFor = Master.eLang.GetString(72, "Poster Only")
                Case 5
                    scrapeFor = Master.eLang.GetString(73, "Fanart Only")
                Case 6
                    scrapeFor = Master.eLang.GetString(71, "NFO Only")
                Case 7
                    scrapeFor = Master.eLang.GetString(75, "Trailer Only")
                Case 8
                    'scrapeFor = Master.eLang.GetString(00, "Subtitles")
                Case 9
                    scrapeFor = Master.eLang.GetString(74, "Extrathumbs Only")
                Case 10
                    scrapeFor = Master.eLang.GetString(76, "Meta Data Only")
            End Select
            If Master.eSettings.AskCheckboxScrape Then
                scrapeType = Master.eLang.GetString(77, "Ask (Require Input If No Exact Match)")
            Else
                scrapeType = Master.eLang.GetString(69, "Automatic (Force Best Match)")
            End If
            Me.SetStatus(String.Format("Scrape ""{0}"" for {1} - {2}", movieName, scrapeFor, scrapeType))
        Else
            oldStatus = String.Empty
        End If
    End Sub

    Private Sub dgvMediaList_CellMouseLeave(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvMediaList.CellMouseLeave
        If Not String.IsNullOrEmpty(oldStatus) Then Me.SetStatus(oldStatus)
    End Sub

    Private Sub dgvMediaList_CellPainting(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles dgvMediaList.CellPainting
        Try

            If Master.isWindows AndAlso e.RowIndex >= 0 AndAlso Not Me.dgvMediaList.Item(e.ColumnIndex, e.RowIndex).Displayed Then
                e.Handled = True
                Return
            End If

            'icons
            If e.ColumnIndex >= 4 AndAlso e.ColumnIndex <= 9 AndAlso e.RowIndex = -1 Then
                e.PaintBackground(e.ClipBounds, False)

                Dim pt As Point = e.CellBounds.Location
                Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                pt.X += offset
                pt.Y = 3
                Me.ilColumnIcons.Draw(e.Graphics, pt, e.ColumnIndex - 4)

                e.Handled = True

            End If

            If e.ColumnIndex = 3 AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvMediaList.Item(11, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Crimson
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Crimson
                ElseIf Convert.ToBoolean(Me.dgvMediaList.Item(10, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Green
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Green
                Else
                    e.CellStyle.ForeColor = Color.Black
                    e.CellStyle.Font = New Font("Segoe UI", 8.25, FontStyle.Regular)
                    e.CellStyle.SelectionForeColor = Color.FromKnownColor(KnownColor.HighlightText)
                End If
            End If

            If e.ColumnIndex >= 3 AndAlso e.ColumnIndex <= 9 AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvMediaList.Item(14, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.LightSteelBlue
                    e.CellStyle.SelectionBackColor = Color.DarkTurquoise
                ElseIf Convert.ToBoolean(Me.dgvMediaList.Item(43, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.MistyRose
                    e.CellStyle.SelectionBackColor = Color.DarkMagenta
                Else
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.SelectionBackColor = Color.FromKnownColor(KnownColor.Highlight)
                End If

                If e.ColumnIndex >= 4 AndAlso e.ColumnIndex <= 9 Then
                    e.PaintBackground(e.ClipBounds, True)

                    Dim pt As Point = e.CellBounds.Location
                    Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                    pt.X += offset
                    pt.Y = e.CellBounds.Top + 3
                    Me.ilColumnIcons.Draw(e.Graphics, pt, If(Convert.ToBoolean(e.Value), 6, 7))
                    e.Handled = True
                End If
            End If

            Me.tabMovies.Text = String.Format("{0} ({1})", Master.eLang.GetString(36, "Movies"), Me.dgvMediaList.RowCount)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvMediaList.KeyDown
        'stop enter key from selecting next list item
        e.Handled = (e.KeyCode = Keys.Enter)
        If e.Modifiers = Keys.Control AndAlso e.KeyCode = Keys.S Then txtSearch.Focus()
    End Sub

    Private Sub dgvMediaList_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles dgvMediaList.KeyPress
        Try
            If Not StringUtils.AlphaNumericOnly(e.KeyChar) Then
                KeyBuffer = String.Concat(KeyBuffer, e.KeyChar.ToString.ToLower)
                tmrKeyBuffer.Start()
                For Each drvRow As DataGridViewRow In Me.dgvMediaList.Rows
                    If drvRow.Cells(3).Value.ToString.ToLower.StartsWith(KeyBuffer) Then
                        drvRow.Selected = True
                        Me.dgvMediaList.CurrentCell = drvRow.Cells(3)

                        Exit For
                    End If
                Next
            ElseIf e.KeyChar = Chr(13) Then
                If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadInfo.IsBusy OrElse _
                Me.bwDownloadPic.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy _
                OrElse Me.bwCleanDB.IsBusy Then Return

                Dim indX As Integer = Me.dgvMediaList.SelectedRows(0).Index
                Dim ID As Integer = Convert.ToInt32(Me.dgvMediaList.Item(0, indX).Value)
                Master.currMovie = Master.DB.LoadMovieFromDB(ID)
                Me.SetStatus(Master.currMovie.Filename)

                Using dEditMovie As New dlgEditMovie(Me)
                    AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                    Select Case dEditMovie.ShowDialog()
                        Case DialogResult.OK
                            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieScraperRDYtoSave, Nothing, Master.currMovie)
                            Me.SetListItemAfterEdit(ID, indX)
                            If Me.RefreshMovie(ID) Then
                                Me.FillList(0)
                            End If
                            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieSync, Nothing, Master.currMovie)
                        Case DialogResult.Retry
                            Functions.SetScraperMod(Enums.ModType.All, True, True)
                            Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                        Case DialogResult.Abort
                            Functions.SetScraperMod(Enums.ModType.DoSearch, True)
                            Functions.SetScraperMod(Enums.ModType.All, True, False)
                            Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                        Case Else
                            If Me.InfoCleared Then Me.LoadInfo(ID, Me.dgvMediaList.Item(1, indX).Value.ToString, True, False)
                    End Select
                    RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                End Using

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles dgvMediaList.MouseDown
        Try
            If e.Button = MouseButtons.Right And Me.dgvMediaList.RowCount > 0 Then
                If bwCleanDB.IsBusy OrElse bwMovieScraper.IsBusy OrElse bwNonScrape.IsBusy Then
                    Me.cmnuTitle.Text = Master.eLang.GetString(845, ">> No Item Selected <<")
                    Return
                End If

                Me.mnuMediaList.Enabled = False

                Dim dgvHTI As DataGridView.HitTestInfo = dgvMediaList.HitTest(e.X, e.Y)
                If dgvHTI.Type = DataGridViewHitTestType.Cell Then

                    If Me.dgvMediaList.SelectedRows.Count > 1 AndAlso Me.dgvMediaList.Rows(dgvHTI.RowIndex).Selected Then
                        Dim setMark As Boolean = False
                        Dim setLock As Boolean = False

                        Me.mnuMediaList.Enabled = True
                        Me.cmnuTitle.Text = Master.eLang.GetString(106, ">> Multiple <<")
                        Me.cmnuEditMovie.Visible = False
                        Me.ScrapingToolStripMenuItem.Visible = True
                        Me.cmnuRescrape.Visible = False
                        Me.cmnuSearchNew.Visible = False
                        'Me.cmuRenamer.Visible = False
                        Me.cmnuMetaData.Visible = False
                        Me.cmnuSep2.Visible = False

                        For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                            'if any one item is set as unmarked, set menu to mark
                            'else they are all marked, so set menu to unmark
                            If Not Convert.ToBoolean(sRow.Cells(11).Value) Then
                                setMark = True
                                If setLock Then Exit For
                            End If
                            'if any one item is set as unlocked, set menu to lock
                            'else they are all locked so set menu to unlock
                            If Not Convert.ToBoolean(sRow.Cells(14).Value) Then
                                setLock = True
                                If setMark Then Exit For
                            End If
                        Next

                        Me.cmnuMark.Text = If(setMark, Master.eLang.GetString(23, "Mark"), Master.eLang.GetString(107, "Unmark"))
                        Me.cmnuLock.Text = If(setLock, Master.eLang.GetString(24, "Lock"), Master.eLang.GetString(108, "Unlock"))

                        Me.GenreListToolStripComboBox.Items.Insert(0, Master.eLang.GetString(98, "Select Genre..."))
                        Me.GenreListToolStripComboBox.SelectedItem = Master.eLang.GetString(98, "Select Genre...")
                        Me.AddGenreToolStripMenuItem.Enabled = False
                        Me.SetGenreToolStripMenuItem.Enabled = False
                        Me.RemoveGenreToolStripMenuItem.Enabled = False
                    Else
                        Me.cmnuEditMovie.Visible = True
                        Me.ScrapingToolStripMenuItem.Visible = False
                        Me.cmnuRescrape.Visible = True
                        Me.cmnuSearchNew.Visible = True
                        Me.cmnuMetaData.Visible = True
                        Me.cmnuSep.Visible = True
                        Me.cmnuSep2.Visible = True

                        cmnuTitle.Text = String.Concat(">> ", Me.dgvMediaList.Item(3, dgvHTI.RowIndex).Value, " <<")

                        If Not Me.dgvMediaList.Rows(dgvHTI.RowIndex).Selected Then
                            Me.prevRow = -1
                            Me.dgvMediaList.CurrentCell = Nothing
                            Me.dgvMediaList.ClearSelection()
                            Me.dgvMediaList.Rows(dgvHTI.RowIndex).Selected = True
                            Me.dgvMediaList.CurrentCell = Me.dgvMediaList.Item(3, dgvHTI.RowIndex)
                        Else
                            Me.mnuMediaList.Enabled = True
                        End If

                        Me.cmnuMark.Text = If(Convert.ToBoolean(Me.dgvMediaList.Item(11, dgvHTI.RowIndex).Value), Master.eLang.GetString(107, "Unmark"), Master.eLang.GetString(23, "Mark"))
                        Me.cmnuLock.Text = If(Convert.ToBoolean(Me.dgvMediaList.Item(14, dgvHTI.RowIndex).Value), Master.eLang.GetString(108, "Unlock"), Master.eLang.GetString(24, "Lock"))

                        Me.GenreListToolStripComboBox.Tag = Me.dgvMediaList.Item(27, dgvHTI.RowIndex).Value
                        Me.GenreListToolStripComboBox.Items.Insert(0, Master.eLang.GetString(98, "Select Genre..."))
                        Me.GenreListToolStripComboBox.SelectedItem = Master.eLang.GetString(98, "Select Genre...")
                        Me.AddGenreToolStripMenuItem.Enabled = False
                        Me.SetGenreToolStripMenuItem.Enabled = False
                        Me.RemoveGenreToolStripMenuItem.Enabled = False
                    End If
                Else
                    Me.mnuMediaList.Enabled = False
                    Me.cmnuTitle.Text = Master.eLang.GetString(845, ">> No Item Selected <<")
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMediaList_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvMediaList.Resize
        ResizeMediaList()
    End Sub

    Private Sub dgvMediaList_Sorted(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvMediaList.Sorted
        Me.prevRow = -1
        If Me.dgvMediaList.RowCount > 0 Then
            Me.dgvMediaList.CurrentCell = Nothing
            Me.dgvMediaList.ClearSelection()
            Me.dgvMediaList.Rows(0).Selected = True
            Me.dgvMediaList.CurrentCell = Me.dgvMediaList.Rows(0).Cells(3)
        End If
    End Sub

    Private Sub dgvTVEpisodes_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVEpisodes.CellClick
        If Me.dgvTVEpisodes.SelectedRows.Count > 0 Then
            If Me.dgvTVEpisodes.RowCount > 0 Then
                If Me.dgvTVEpisodes.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVEpisodes.SelectedRows.Count))
                ElseIf Me.dgvTVEpisodes.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVEpisodes.SelectedRows(0).Cells(3).Value.ToString)
                End If
            End If

            Me.currEpRow = Me.dgvTVEpisodes.SelectedRows(0).Index
            If Not Me.currList = 2 Then
                Me.currList = 2
                Me.prevEpRow = -1
                Me.SelectEpisodeRow(Me.dgvTVEpisodes.SelectedRows(0).Index)
            End If
        End If
    End Sub

    Private Sub dgvTVEpisodes_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVEpisodes.CellDoubleClick
        Try

            If e.RowIndex < 0 Then Exit Sub

            If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

            Dim indX As Integer = Me.dgvTVEpisodes.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvTVEpisodes.Item(0, indX).Value)
            Master.currShow = Master.DB.LoadTVEpFromDB(ID, True)

            Using dEditEpisode As New dlgEditEpisode
                AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
                Select Case dEditEpisode.ShowDialog()
                    Case DialogResult.OK
                        If Me.RefreshEpisode(ID) Then
                            Me.FillEpisodes(Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVEp.Season)
                        End If
                End Select
                RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVEpisodes_CellEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVEpisodes.CellEnter
        Try
            If Not Me.tabsMain.SelectedIndex = 1 OrElse Not Me.currList = 2 Then Return

            Me.tmrWaitShow.Stop()
            Me.tmrWaitSeason.Stop()
            Me.tmrWait.Stop()
            Me.tmrWaitEp.Stop()
            Me.tmrLoadShow.Stop()
            Me.tmrLoadSeason.Stop()
            Me.tmrLoad.Stop()
            Me.tmrLoadEp.Stop()

            Me.currEpRow = e.RowIndex
            Me.tmrWaitEp.Start()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVEpisodes_CellPainting(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles dgvTVEpisodes.CellPainting
        Try

            If Master.isWindows AndAlso e.RowIndex >= 0 AndAlso Not Me.dgvTVEpisodes.Item(e.ColumnIndex, e.RowIndex).Displayed Then
                e.Handled = True
                Return
            End If

            'icons
            If e.ColumnIndex >= 4 AndAlso e.ColumnIndex <= 6 AndAlso e.RowIndex = -1 Then
                e.PaintBackground(e.ClipBounds, False)

                Dim pt As Point = e.CellBounds.Location
                Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                pt.X += offset
                pt.Y = 1

                Me.ilColumnIcons.Draw(e.Graphics, pt, e.ColumnIndex - 4)

                e.Handled = True
            End If

            If (e.ColumnIndex = 2 OrElse e.ColumnIndex = 3) AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvTVEpisodes.Item(22, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Gray
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Regular)
                    e.CellStyle.SelectionForeColor = Color.LightGray
                ElseIf Convert.ToBoolean(Me.dgvTVEpisodes.Item(8, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Crimson
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Crimson
                ElseIf Convert.ToBoolean(Me.dgvTVEpisodes.Item(7, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Green
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Green
                Else
                    e.CellStyle.ForeColor = Color.Black
                    e.CellStyle.Font = New Font("Segoe UI", 8.25, FontStyle.Regular)
                    e.CellStyle.SelectionForeColor = Color.FromKnownColor(KnownColor.HighlightText)
                End If
            End If

            If e.ColumnIndex >= 2 AndAlso e.ColumnIndex <= 6 AndAlso e.RowIndex >= 0 Then

                If Convert.ToBoolean(Me.dgvTVEpisodes.Item(22, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.SelectionBackColor = Color.DarkGray
                ElseIf Convert.ToBoolean(Me.dgvTVEpisodes.Item(11, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.LightSteelBlue
                    e.CellStyle.SelectionBackColor = Color.DarkTurquoise
                Else
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.SelectionBackColor = Color.FromKnownColor(KnownColor.Highlight)
                End If

                If e.ColumnIndex >= 4 AndAlso e.ColumnIndex <= 6 Then
                    e.PaintBackground(e.ClipBounds, True)

                    Dim pt As Point = e.CellBounds.Location
                    Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                    pt.X += offset
                    pt.Y = e.CellBounds.Top + 3
                    Me.ilColumnIcons.Draw(e.Graphics, pt, If(Convert.ToBoolean(e.Value), 6, 7))
                    e.Handled = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVEpisodes_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvTVEpisodes.KeyDown
        'stop enter key from selecting next list item
        e.Handled = e.KeyCode = Keys.Enter
    End Sub

    Private Sub dgvTVEpisodes_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles dgvTVEpisodes.KeyPress
        Try
            If Not StringUtils.AlphaNumericOnly(e.KeyChar) Then
                KeyBuffer = String.Concat(KeyBuffer, e.KeyChar.ToString.ToLower)
                tmrKeyBuffer.Start()
                For Each drvRow As DataGridViewRow In Me.dgvTVEpisodes.Rows
                    If drvRow.Cells(3).Value.ToString.ToLower.StartsWith(KeyBuffer) Then
                        drvRow.Selected = True
                        Me.dgvTVEpisodes.CurrentCell = drvRow.Cells(3)
                        Exit For
                    End If
                Next
            ElseIf e.KeyChar = Chr(13) Then
                If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

                Dim indX As Integer = Me.dgvTVEpisodes.SelectedRows(0).Index
                Dim ID As Integer = Convert.ToInt32(Me.dgvTVEpisodes.Item(0, indX).Value)
                Master.currShow = Master.DB.LoadTVEpFromDB(ID, True)

                Using dEditEpisode As New dlgEditEpisode
                    AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
                    Select Case dEditEpisode.ShowDialog()
                        Case DialogResult.OK
                            If Me.RefreshEpisode(ID) Then
                                Me.FillEpisodes(Convert.ToInt32(Master.currShow.ShowID), Master.currShow.TVEp.Season)
                            End If
                    End Select
                    RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEpisode.GenericRunCallBack
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ShowEpisodeMenuItems(ByVal Visible As Boolean)
        Dim cMnu As ToolStripMenuItem
        Dim cSep As ToolStripSeparator

        Try
            If Visible Then
                For Each cMnuItem As Object In Me.mnuEpisodes.Items
                    If TypeOf cMnuItem Is ToolStripMenuItem Then
                        DirectCast(cMnuItem, ToolStripMenuItem).Visible = True
                    ElseIf TypeOf cMnuItem Is ToolStripSeparator Then
                        DirectCast(cMnuItem, ToolStripSeparator).Visible = True
                    End If
                Next
                Me.cmnuDeleteTVEp.Visible = True
            Else
                For Each cMnuItem As Object In Me.mnuEpisodes.Items
                    If TypeOf cMnuItem Is ToolStripMenuItem Then
                        cMnu = DirectCast(cMnuItem, ToolStripMenuItem)
                        If Not cMnu.Name = "RemoveEpToolStripMenuItem" AndAlso Not cMnu.Name = "cmnuEpTitle" Then
                            cMnu.Visible = False
                        End If
                    ElseIf TypeOf cMnuItem Is ToolStripSeparator Then
                        cSep = DirectCast(cMnuItem, ToolStripSeparator)
                        If Not cSep.Name = "ToolStripSeparator6" Then
                            cSep.Visible = False
                        End If
                    End If
                    Me.cmnuDeleteTVEp.Visible = False
                Next
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Private Sub dgvTVEpisodes_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles dgvTVEpisodes.MouseDown
        Try
            Dim hasMissing As Boolean = False

            If e.Button = MouseButtons.Right And Me.dgvTVEpisodes.RowCount > 0 Then

                Me.mnuEpisodes.Enabled = False

                Dim dgvHTI As DataGridView.HitTestInfo = dgvTVEpisodes.HitTest(e.X, e.Y)
                If dgvHTI.Type = DataGridViewHitTestType.Cell Then

                    If Me.dgvTVEpisodes.SelectedRows.Count > 1 AndAlso Me.dgvTVEpisodes.Rows(dgvHTI.RowIndex).Selected Then

                        Me.mnuEpisodes.Enabled = True

                        For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                            If Convert.ToBoolean(sRow.Cells(22).Value) Then
                                hasMissing = True
                                Exit For
                            End If
                        Next

                        Me.cmnuEpTitle.Text = Master.eLang.GetString(106, ">> Multiple <<")

                        If hasMissing Then
                            Me.ShowEpisodeMenuItems(False)
                        Else
                            Dim setMark As Boolean = False
                            Dim setLock As Boolean = False

                            Me.ShowEpisodeMenuItems(True)

                            Me.ToolStripSeparator9.Visible = False
                            Me.cmnuEditEpisode.Visible = False
                            Me.ToolStripSeparator10.Visible = False
                            Me.cmnuRescrapeEp.Visible = False
                            Me.cmnuChangeEp.Visible = False
                            Me.ToolStripSeparator12.Visible = False
                            Me.cmnuEpOpenFolder.Visible = False

                            For Each sRow As DataGridViewRow In Me.dgvTVEpisodes.SelectedRows
                                'if any one item is set as unmarked, set menu to mark
                                'else they are all marked, so set menu to unmark
                                If Not Convert.ToBoolean(sRow.Cells(8).Value) Then
                                    setMark = True
                                    If setLock Then Exit For
                                End If
                                'if any one item is set as unlocked, set menu to lock
                                'else they are all locked so set menu to unlock
                                If Not Convert.ToBoolean(sRow.Cells(11).Value) Then
                                    setLock = True
                                    If setMark Then Exit For
                                End If
                            Next

                            Me.cmnuMarkEp.Text = If(setMark, Master.eLang.GetString(23, "Mark"), Master.eLang.GetString(107, "Unmark"))
                            Me.cmnuLockEp.Text = If(setLock, Master.eLang.GetString(24, "Lock"), Master.eLang.GetString(108, "Unlock"))
                        End If
                    Else
                        cmnuEpTitle.Text = String.Concat(">> ", Me.dgvTVEpisodes.Item(3, dgvHTI.RowIndex).Value, " <<")

                        If Not Me.dgvTVEpisodes.Rows(dgvHTI.RowIndex).Selected OrElse Not Me.currList = 2 Then
                            Me.prevEpRow = -1
                            Me.currList = 2
                            Me.dgvTVEpisodes.CurrentCell = Nothing
                            Me.dgvTVEpisodes.ClearSelection()
                            Me.dgvTVEpisodes.Rows(dgvHTI.RowIndex).Selected = True
                            Me.dgvTVEpisodes.CurrentCell = Me.dgvTVEpisodes.Item(3, dgvHTI.RowIndex)
                        Else
                            Me.mnuEpisodes.Enabled = True
                        End If

                        If Convert.ToBoolean(Me.dgvTVEpisodes.Item(22, dgvHTI.RowIndex).Value) Then hasMissing = True

                        If hasMissing Then
                            Me.ShowEpisodeMenuItems(False)
                        Else
                            Me.ShowEpisodeMenuItems(True)

                            Me.ToolStripSeparator9.Visible = True
                            Me.cmnuEditEpisode.Visible = True
                            Me.ToolStripSeparator10.Visible = True
                            Me.cmnuRescrapeEp.Visible = True
                            Me.cmnuChangeEp.Visible = True
                            Me.ToolStripSeparator12.Visible = True
                            Me.cmnuEpOpenFolder.Visible = True

                            Me.cmnuMarkEp.Text = If(Convert.ToBoolean(Me.dgvTVEpisodes.Item(8, dgvHTI.RowIndex).Value), Master.eLang.GetString(107, "Unmark"), Master.eLang.GetString(23, "Mark"))
                            Me.cmnuLockEp.Text = If(Convert.ToBoolean(Me.dgvTVEpisodes.Item(11, dgvHTI.RowIndex).Value), Master.eLang.GetString(108, "Unlock"), Master.eLang.GetString(24, "Lock"))
                        End If

                    End If
                Else
                    Me.mnuEpisodes.Enabled = False
                    Me.cmnuEpTitle.Text = Master.eLang.GetString(845, ">> No Item Selected <<")
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVEpisodes_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVEpisodes.Resize
        ResizeTVLists(3)
    End Sub

    Private Sub dgvTVEpisodes_Sorted(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVEpisodes.Sorted
        Me.prevEpRow = -1
        If Me.dgvTVEpisodes.RowCount > 0 Then
            Me.dgvTVEpisodes.CurrentCell = Nothing
            Me.dgvTVEpisodes.ClearSelection()
            Me.dgvTVEpisodes.Rows(0).Selected = True
            Me.dgvTVEpisodes.CurrentCell = Me.dgvTVEpisodes.Rows(0).Cells(3)
        End If
    End Sub

    Private Sub dgvTVSeasons_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVSeasons.CellClick
        If Me.dgvTVSeasons.SelectedRows.Count > 0 Then
            If Me.dgvTVSeasons.RowCount > 0 Then
                If Me.dgvTVSeasons.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVSeasons.SelectedRows.Count))
                ElseIf Me.dgvTVSeasons.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVSeasons.SelectedRows(0).Cells(1).Value.ToString)
                End If
            End If

            Me.currSeasonRow = Me.dgvTVSeasons.SelectedRows(0).Index
            If Not Me.currList = 1 Then
                Me.currList = 1
                Me.prevSeasonRow = -1
                Me.SelectSeasonRow(Me.dgvTVSeasons.SelectedRows(0).Index)
            End If
        End If
    End Sub

    Private Sub dgvTVSeasons_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVSeasons.CellDoubleClick
        Try

            If e.RowIndex < 0 Then Exit Sub

            If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadSeasonInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

            Dim indX As Integer = Me.dgvTVSeasons.SelectedRows(0).Index
            Dim ShowID As Integer = Convert.ToInt32(Me.dgvTVSeasons.Item(0, indX).Value)
            Dim Season As Integer = Convert.ToInt32(Me.dgvTVSeasons.Item(2, indX).Value)

            Master.currShow = Master.DB.LoadTVSeasonFromDB(ShowID, Season, True)

            Using dEditSeason As New dlgEditSeason
                If dEditSeason.ShowDialog() = DialogResult.OK Then
                    If Me.RefreshSeason(ShowID, Season, False) Then
                        Me.FillSeasons(ShowID)
                    End If
                End If
            End Using

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVSeasons_CellEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVSeasons.CellEnter
        Try

            If Not Me.tabsMain.SelectedIndex = 1 OrElse Not Me.currList = 1 Then Return

            Me.tmrWaitShow.Stop()
            Me.tmrWait.Stop()
            Me.tmrWaitEp.Stop()
            Me.tmrWaitSeason.Stop()
            Me.tmrLoadShow.Stop()
            Me.tmrLoad.Stop()
            Me.tmrLoadEp.Stop()
            Me.tmrLoadSeason.Stop()

            Me.currSeasonRow = e.RowIndex
            Me.tmrWaitSeason.Start()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVSeasons_CellPainting(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles dgvTVSeasons.CellPainting
        Try

            If Master.isWindows AndAlso e.RowIndex >= 0 AndAlso Not Me.dgvTVSeasons.Item(e.ColumnIndex, e.RowIndex).Displayed Then
                e.Handled = True
                Return
            End If

            'icons
            If (e.ColumnIndex = 3 OrElse e.ColumnIndex = 4) AndAlso e.RowIndex = -1 Then
                e.PaintBackground(e.ClipBounds, False)

                Dim pt As Point = e.CellBounds.Location
                Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                pt.X += offset
                pt.Y = 1
                Me.ilColumnIcons.Draw(e.Graphics, pt, e.ColumnIndex - 3)

                e.Handled = True

            End If

            If e.ColumnIndex = 1 AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvTVSeasons.Item(8, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Crimson
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Crimson
                ElseIf Convert.ToBoolean(Me.dgvTVSeasons.Item(9, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Green
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Green
                Else
                    e.CellStyle.ForeColor = Color.Black
                    e.CellStyle.Font = New Font("Segoe UI", 8.25, FontStyle.Regular)
                    e.CellStyle.SelectionForeColor = Color.FromKnownColor(KnownColor.HighlightText)
                End If
            End If

            If e.ColumnIndex >= 1 AndAlso e.ColumnIndex <= 4 AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvTVSeasons.Item(7, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.LightSteelBlue
                    e.CellStyle.SelectionBackColor = Color.DarkTurquoise
                Else
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.SelectionBackColor = Color.FromKnownColor(KnownColor.Highlight)
                End If

                If e.ColumnIndex = 3 OrElse e.ColumnIndex = 4 Then
                    e.PaintBackground(e.ClipBounds, True)

                    Dim pt As Point = e.CellBounds.Location
                    Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                    pt.X += offset
                    pt.Y = e.CellBounds.Top + 3
                    Me.ilColumnIcons.Draw(e.Graphics, pt, If(Convert.ToBoolean(e.Value), 6, 7))
                    e.Handled = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVSeasons_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvTVSeasons.KeyDown
        'stop enter key from selecting next list item
        e.Handled = e.KeyCode = Keys.Enter
    End Sub

    Private Sub dgvTVSeasons_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles dgvTVSeasons.KeyPress
        Try
            If Not StringUtils.AlphaNumericOnly(e.KeyChar) Then
                KeyBuffer = String.Concat(KeyBuffer, e.KeyChar.ToString.ToLower)
                tmrKeyBuffer.Start()
                For Each drvRow As DataGridViewRow In Me.dgvTVSeasons.Rows
                    If drvRow.Cells(2).Value.ToString.StartsWith(KeyBuffer) Then
                        drvRow.Selected = True
                        Me.dgvTVSeasons.CurrentCell = drvRow.Cells(1)
                        Exit For
                    End If
                Next
            ElseIf e.KeyChar = Chr(13) Then
                If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadSeasonInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

                Dim indX As Integer = Me.dgvTVSeasons.SelectedRows(0).Index
                Dim ShowID As Integer = Convert.ToInt32(Me.dgvTVSeasons.Item(0, indX).Value)
                Dim Season As Integer = Convert.ToInt32(Me.dgvTVSeasons.Item(2, indX).Value)

                Master.currShow = Master.DB.LoadTVSeasonFromDB(ShowID, Season, True)

                Using dEditSeason As New dlgEditSeason
                    If dEditSeason.ShowDialog() = DialogResult.OK Then
                        If Me.RefreshSeason(ShowID, Season, False) Then
                            Me.FillSeasons(ShowID)
                        End If
                    End If
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVSeasons_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles dgvTVSeasons.MouseDown
        Try
            If e.Button = MouseButtons.Right And Me.dgvTVSeasons.RowCount > 0 Then

                Me.mnuSeasons.Enabled = False

                Dim dgvHTI As DataGridView.HitTestInfo = dgvTVSeasons.HitTest(e.X, e.Y)
                If dgvHTI.Type = DataGridViewHitTestType.Cell Then

                    If Me.dgvTVSeasons.SelectedRows.Count > 1 AndAlso Me.dgvTVSeasons.Rows(dgvHTI.RowIndex).Selected Then
                        Dim setMark As Boolean = False
                        Dim setLock As Boolean = False

                        Me.mnuSeasons.Enabled = True
                        Me.cmnuSeasonTitle.Text = Master.eLang.GetString(106, ">> Multiple <<")
                        Me.ToolStripSeparator16.Visible = False
                        Me.cmnuSeasonChangeImages.Visible = False
                        Me.ToolStripSeparator14.Visible = False
                        Me.cmnuSeasonRescrape.Visible = False
                        Me.ToolStripSeparator15.Visible = False
                        Me.cmnuSeasonOpenFolder.Visible = False

                        For Each sRow As DataGridViewRow In Me.dgvTVSeasons.SelectedRows
                            'if any one item is set as unmarked, set menu to mark
                            'else they are all marked, so set menu to unmark
                            If Not Convert.ToBoolean(sRow.Cells(8).Value) Then
                                setMark = True
                                If setLock Then Exit For
                            End If
                            'if any one item is set as unlocked, set menu to lock
                            'else they are all locked so set menu to unlock
                            If Not Convert.ToBoolean(sRow.Cells(7).Value) Then
                                setLock = True
                                If setMark Then Exit For
                            End If
                        Next

                        Me.cmnuMarkSeason.Text = If(setMark, Master.eLang.GetString(23, "Mark"), Master.eLang.GetString(107, "Unmark"))
                        Me.cmnuLockSeason.Text = If(setLock, Master.eLang.GetString(24, "Lock"), Master.eLang.GetString(108, "Unlock"))

                    Else
                        Me.ToolStripSeparator16.Visible = True
                        Me.cmnuSeasonChangeImages.Visible = True
                        Me.ToolStripSeparator14.Visible = True
                        Me.cmnuSeasonRescrape.Visible = True
                        Me.ToolStripSeparator15.Visible = True
                        Me.cmnuSeasonOpenFolder.Visible = True

                        Me.cmnuSeasonTitle.Text = String.Concat(">> ", Me.dgvTVSeasons.Item(1, dgvHTI.RowIndex).Value, " <<")
                        Me.cmnuMarkSeason.Text = If(Convert.ToBoolean(Me.dgvTVSeasons.Item(8, dgvHTI.RowIndex).Value), Master.eLang.GetString(107, "Unmark"), Master.eLang.GetString(23, "Mark"))
                        Me.cmnuLockSeason.Text = If(Convert.ToBoolean(Me.dgvTVSeasons.Item(7, dgvHTI.RowIndex).Value), Master.eLang.GetString(108, "Unlock"), Master.eLang.GetString(24, "Lock"))
                        Me.cmnuSeasonChangeImages.Enabled = Convert.ToInt32(Me.dgvTVSeasons.Item(2, dgvHTI.RowIndex).Value) >= 0

                        If Not Me.dgvTVSeasons.Rows(dgvHTI.RowIndex).Selected OrElse Not Me.currList = 1 Then
                            Me.prevSeasonRow = -1
                            Me.currList = 1
                            Me.dgvTVSeasons.CurrentCell = Nothing
                            Me.dgvTVSeasons.ClearSelection()
                            Me.dgvTVSeasons.Rows(dgvHTI.RowIndex).Selected = True
                            Me.dgvTVSeasons.CurrentCell = Me.dgvTVSeasons.Item(1, dgvHTI.RowIndex)
                        Else
                            Me.mnuSeasons.Enabled = True
                        End If
                    End If
                Else
                    Me.mnuSeasons.Enabled = False
                    Me.cmnuSeasonTitle.Text = Master.eLang.GetString(845, ">> No Item Selected <<")
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVSeasons_Sorted(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVSeasons.Sorted
        Me.prevSeasonRow = -1
        If Me.dgvTVSeasons.RowCount > 0 Then
            Me.dgvTVSeasons.CurrentCell = Nothing
            Me.dgvTVSeasons.ClearSelection()
            Me.dgvTVSeasons.Rows(0).Selected = True
            Me.dgvTVSeasons.CurrentCell = Me.dgvTVSeasons.Rows(0).Cells(1)
        End If
    End Sub

    Private Sub dgvTVSeason_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVSeasons.Resize
        ResizeTVLists(2)
    End Sub

    Private Sub dgvTVShows_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVShows.CellClick
        If Me.dgvTVShows.SelectedRows.Count > 0 Then
            If Me.dgvTVShows.RowCount > 0 Then
                Me.tmpTitle = Me.dgvTVShows.SelectedRows(0).Cells(1).Value.ToString
                Me.tmpTVDB = Me.dgvTVShows.SelectedRows(0).Cells(9).Value.ToString
                Me.tmpLang = Me.dgvTVShows.SelectedRows(0).Cells(22).Value.ToString
                Me.tmpOrdering = DirectCast(Convert.ToInt32(Me.dgvTVShows.SelectedRows(0).Cells(23).Value), Enums.Ordering)
                If Me.dgvTVShows.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVShows.SelectedRows.Count))
                ElseIf Me.dgvTVShows.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVShows.SelectedRows(0).Cells(1).Value.ToString)
                End If
            End If

            Me.currShowRow = Me.dgvTVShows.SelectedRows(0).Index
            If Not Me.currList = 0 Then
                Me.currList = 0
                Me.prevShowRow = -1
                Me.SelectShowRow(Me.dgvTVShows.SelectedRows(0).Index)
            End If
        End If
    End Sub

    Private Sub dgvTVShows_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVShows.CellDoubleClick
        Try

            If e.RowIndex < 0 Then Exit Sub

            If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadSeasonInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

            Dim indX As Integer = Me.dgvTVShows.SelectedRows(0).Index
            Dim ID As Integer = Convert.ToInt32(Me.dgvTVShows.Item(0, indX).Value)

            Master.currShow = Master.DB.LoadTVFullShowFromDB(ID)

            Using dEditShow As New dlgEditShow

                Select Case dEditShow.ShowDialog()
                    Case DialogResult.OK
                        Me.SetShowListItemAfterEdit(ID, indX)
                        If Me.RefreshShow(ID, False, True, False, False) Then
                            Me.FillList(0)
                        End If
                End Select

            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVShows_CellEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvTVShows.CellEnter
        Try
            If Not Me.tabsMain.SelectedIndex = 1 OrElse Not Me.currList = 0 Then Return

            Me.tmrWait.Stop()
            Me.tmrWaitSeason.Stop()
            Me.tmrWaitEp.Stop()
            Me.tmrWaitShow.Stop()
            Me.tmrLoad.Stop()
            Me.tmrLoadSeason.Stop()
            Me.tmrLoadEp.Stop()
            Me.tmrLoadShow.Stop()

            Me.currShowRow = e.RowIndex
            Me.tmrWaitShow.Start()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVShows_CellPainting(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles dgvTVShows.CellPainting
        Try

            If Master.isWindows AndAlso e.RowIndex >= 0 AndAlso Not Me.dgvTVShows.Item(e.ColumnIndex, e.RowIndex).Displayed Then
                e.Handled = True
                Return
            End If

            'icons
            If e.ColumnIndex >= 2 AndAlso e.ColumnIndex <= 4 AndAlso e.RowIndex = -1 Then
                e.PaintBackground(e.ClipBounds, False)

                Dim pt As Point = e.CellBounds.Location
                Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                pt.X += offset
                pt.Y = 1
                Me.ilColumnIcons.Draw(e.Graphics, pt, e.ColumnIndex - 2)

                e.Handled = True

            End If

            If e.ColumnIndex = 1 AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(Me.dgvTVShows.Item(6, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Crimson
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Crimson
                ElseIf Convert.ToBoolean(Me.dgvTVShows.Item(5, e.RowIndex).Value) Then
                    e.CellStyle.ForeColor = Color.Green
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.SelectionForeColor = Color.Green
                Else
                    e.CellStyle.ForeColor = Color.Black
                    e.CellStyle.Font = New Font("Segoe UI", 8.25, FontStyle.Regular)
                    e.CellStyle.SelectionForeColor = Color.FromKnownColor(KnownColor.HighlightText)
                End If
            End If

            If e.ColumnIndex >= 1 AndAlso e.ColumnIndex <= 4 AndAlso e.RowIndex >= 0 Then

                If Convert.ToBoolean(Me.dgvTVShows.Item(10, e.RowIndex).Value) Then
                    e.CellStyle.BackColor = Color.LightSteelBlue
                    e.CellStyle.SelectionBackColor = Color.DarkTurquoise
                Else
                    e.CellStyle.BackColor = Color.White
                    e.CellStyle.SelectionBackColor = Color.FromKnownColor(KnownColor.Highlight)
                End If

                If e.ColumnIndex >= 2 AndAlso e.ColumnIndex <= 4 Then
                    e.PaintBackground(e.ClipBounds, True)

                    Dim pt As Point = e.CellBounds.Location
                    Dim offset As Integer = Convert.ToInt32((e.CellBounds.Width - Me.ilColumnIcons.ImageSize.Width) / 2)

                    pt.X += offset
                    pt.Y = e.CellBounds.Top + 3
                    Me.ilColumnIcons.Draw(e.Graphics, pt, If(Convert.ToBoolean(e.Value), 6, 7))
                    e.Handled = True
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVShows_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvTVShows.KeyDown
        'stop enter key from selecting next list item
        e.Handled = e.KeyCode = Keys.Enter
    End Sub

    Private Sub dgvTVShows_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles dgvTVShows.KeyPress
        Try
            If Not StringUtils.AlphaNumericOnly(e.KeyChar) Then
                KeyBuffer = String.Concat(KeyBuffer, e.KeyChar.ToString.ToLower)
                tmrKeyBuffer.Start()
                For Each drvRow As DataGridViewRow In Me.dgvTVShows.Rows
                    If drvRow.Cells(1).Value.ToString.ToLower.StartsWith(KeyBuffer) Then
                        drvRow.Selected = True
                        Me.dgvTVShows.CurrentCell = drvRow.Cells(1)
                        Exit For
                    End If
                Next
            ElseIf e.KeyChar = Chr(13) Then
                If Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadSeasonInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwCleanDB.IsBusy Then Return

                Dim indX As Integer = Me.dgvTVShows.SelectedRows(0).Index
                Dim ID As Integer = Convert.ToInt32(Me.dgvTVShows.Item(0, indX).Value)

                Master.currShow = Master.DB.LoadTVFullShowFromDB(ID)

                Using dEditShow As New dlgEditShow

                    Select Case dEditShow.ShowDialog()
                        Case DialogResult.OK
                            Me.SetShowListItemAfterEdit(ID, indX)
                            If Me.RefreshShow(ID, False, True, False, False) Then
                                Me.FillList(0)
                            End If
                    End Select

                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVShows_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles dgvTVShows.MouseDown
        Try
            If e.Button = MouseButtons.Right And Me.dgvTVShows.RowCount > 0 Then

                Me.mnuShows.Enabled = False

                Dim dgvHTI As DataGridView.HitTestInfo = dgvTVShows.HitTest(e.X, e.Y)
                If dgvHTI.Type = DataGridViewHitTestType.Cell Then

                    Me.tmpTitle = Me.dgvTVShows.Item(1, dgvHTI.RowIndex).Value.ToString
                    Me.tmpTVDB = Me.dgvTVShows.Item(9, dgvHTI.RowIndex).Value.ToString
                    Me.tmpLang = Me.dgvTVShows.Item(22, dgvHTI.RowIndex).Value.ToString
                    Me.tmpOrdering = DirectCast(Convert.ToInt32(Me.dgvTVShows.Item(23, dgvHTI.RowIndex).Value), Enums.Ordering)

                    If Me.dgvTVShows.SelectedRows.Count > 1 AndAlso Me.dgvTVShows.Rows(dgvHTI.RowIndex).Selected Then
                        Dim setMark As Boolean = False
                        Dim setLock As Boolean = False

                        Me.mnuShows.Enabled = True
                        Me.cmnuShowTitle.Text = Master.eLang.GetString(106, ">> Multiple <<")
                        Me.ToolStripSeparator8.Visible = False
                        Me.cmnuEditShow.Visible = False
                        Me.ToolStripSeparator7.Visible = False
                        ' Me.cmnuRescrapeShow.Visible = False
                        Me.cmnuChangeShow.Visible = False
                        Me.cmnuShowOpenFolder.Visible = False
                        Me.ToolStripSeparator20.Visible = False

                        For Each sRow As DataGridViewRow In Me.dgvTVShows.SelectedRows
                            'if any one item is set as unmarked, set menu to mark
                            'else they are all marked, so set menu to unmark
                            If Not Convert.ToBoolean(sRow.Cells(6).Value) Then
                                setMark = True
                                If setLock Then Exit For
                            End If
                            'if any one item is set as unlocked, set menu to lock
                            'else they are all locked so set menu to unlock
                            If Not Convert.ToBoolean(sRow.Cells(10).Value) Then
                                setLock = True
                                If setMark Then Exit For
                            End If
                        Next

                        Me.cmnuMarkShow.Text = If(setMark, Master.eLang.GetString(23, "Mark"), Master.eLang.GetString(107, "Unmark"))
                        Me.cmnuLockShow.Text = If(setLock, Master.eLang.GetString(24, "Lock"), Master.eLang.GetString(108, "Unlock"))

                    Else
                        Me.ToolStripSeparator8.Visible = True
                        Me.cmnuEditShow.Visible = True
                        Me.ToolStripSeparator7.Visible = True
                        Me.cmnuRescrapeShow.Visible = True
                        Me.cmnuChangeShow.Visible = True
                        Me.cmnuShowOpenFolder.Visible = True
                        Me.ToolStripSeparator20.Visible = True

                        Me.cmnuShowTitle.Text = String.Concat(">> ", Me.dgvTVShows.Item(1, dgvHTI.RowIndex).Value, " <<")
                        Me.cmnuMarkShow.Text = If(Convert.ToBoolean(Me.dgvTVShows.Item(6, dgvHTI.RowIndex).Value), Master.eLang.GetString(107, "Unmark"), Master.eLang.GetString(23, "Mark"))
                        Me.cmnuLockShow.Text = If(Convert.ToBoolean(Me.dgvTVShows.Item(10, dgvHTI.RowIndex).Value), Master.eLang.GetString(108, "Unlock"), Master.eLang.GetString(24, "Lock"))

                        If Not Me.dgvTVShows.Rows(dgvHTI.RowIndex).Selected OrElse Not Me.currList = 0 Then
                            Me.prevShowRow = -1
                            Me.currList = 0
                            Me.dgvTVShows.CurrentCell = Nothing
                            Me.dgvTVShows.ClearSelection()
                            Me.dgvTVShows.Rows(dgvHTI.RowIndex).Selected = True
                            Me.dgvTVShows.CurrentCell = Me.dgvTVShows.Item(3, dgvHTI.RowIndex)
                        Else
                            Me.mnuShows.Enabled = True
                        End If
                    End If
                Else
                    Me.mnuShows.Enabled = False
                    Me.cmnuShowTitle.Text = Master.eLang.GetString(845, ">> No Item Selected <<")
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvTVShows_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVShows.Resize
        ResizeTVLists(1)
    End Sub

    Private Sub dgvTVShows_Sorted(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvTVShows.Sorted
        Me.prevShowRow = -1
        If Me.dgvTVShows.RowCount > 0 Then
            Me.dgvTVShows.CurrentCell = Nothing
            Me.dgvTVShows.ClearSelection()
            Me.dgvTVShows.Rows(0).Selected = True
            Me.dgvTVShows.CurrentCell = Me.dgvTVShows.Rows(0).Cells(1)
        End If
    End Sub

    Private Sub DonateToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DonateToolStripMenuItem.Click
        If Master.isWindows Then
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G4WN5KRET4K48")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G4WN5KRET4K48"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub DoTitleCheck()
        Try

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    SQLcommand.CommandText = "UPDATE movies SET OutOfTolerance = (?) WHERE ID = (?);"
                    Dim parOutOfTolerance As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parOutOfTolerance", DbType.Boolean, 0, "OutOfTolerance")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "ID")
                    Dim LevFail As Boolean = False
                    Dim pTitle As String = String.Empty
                    For Each drvRow As DataGridViewRow In Me.dgvMediaList.Rows

                        If Master.eSettings.LevTolerance > 0 Then
                            If FileUtils.Common.isVideoTS(drvRow.Cells(1).Value.ToString) Then
                                pTitle = Directory.GetParent(Directory.GetParent(drvRow.Cells(1).Value.ToString).FullName).Name
                            ElseIf FileUtils.Common.isBDRip(drvRow.Cells(1).Value.ToString) Then
                                pTitle = Directory.GetParent(Directory.GetParent(Directory.GetParent(drvRow.Cells(1).Value.ToString).FullName).FullName).Name
                            Else
                                If Convert.ToBoolean(drvRow.Cells(42).Value) AndAlso Convert.ToBoolean(drvRow.Cells(2).Value) Then
                                    pTitle = Directory.GetParent(drvRow.Cells(1).Value.ToString).Name
                                Else
                                    pTitle = Path.GetFileNameWithoutExtension(drvRow.Cells(1).Value.ToString)
                                End If
                            End If

                            LevFail = StringUtils.ComputeLevenshtein(StringUtils.FilterName(drvRow.Cells(15).Value.ToString, False, True).ToLower, StringUtils.FilterName(pTitle, False, True).ToLower) > Master.eSettings.LevTolerance

                            parOutOfTolerance.Value = LevFail
                            drvRow.Cells(43).Value = LevFail
                            parID.Value = drvRow.Cells(0).Value
                        Else
                            parOutOfTolerance.Value = False
                            drvRow.Cells(43).Value = False
                            parID.Value = drvRow.Cells(0).Value
                        End If
                        SQLcommand.ExecuteNonQuery()
                    Next
                End Using

                SQLtransaction.Commit()
            End Using

            Me.dgvMediaList.Invalidate()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Sub dtListUpdate(ByVal drow As DataRow, ByVal i As Integer, ByVal v As Object)
        drow.Item(i) = v
    End Sub

    Private Sub EnableFilters(ByVal isEnabled As Boolean)
        Me.txtSearch.Enabled = isEnabled
        Me.cbSearch.Enabled = isEnabled
        Me.chkFilterDupe.Enabled = isEnabled
        Me.chkFilterTolerance.Enabled = If(Master.eSettings.LevTolerance > 0, isEnabled, False)
        Me.chkFilterMissing.Enabled = isEnabled
        Me.chkFilterMark.Enabled = isEnabled
        Me.chkFilterNew.Enabled = isEnabled
        Me.chkFilterLock.Enabled = isEnabled
        Me.rbFilterOr.Enabled = isEnabled
        Me.rbFilterAnd.Enabled = isEnabled
        Me.txtFilterSource.Enabled = isEnabled
        Me.cbFilterFileSource.Enabled = isEnabled
        Me.txtFilterGenre.Enabled = isEnabled
        Me.cbFilterYearMod.Enabled = isEnabled
        Me.cbFilterYear.Enabled = isEnabled
        Me.btnClearFilters.Enabled = isEnabled
    End Sub

    Private Sub ErrorOccurred()
        Me.ErrorToolStripMenuItem.Visible = True
        If dlgErrorViewer.Visible Then dlgErrorViewer.UpdateLog()
    End Sub

    Private Sub ErrorToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ErrorToolStripMenuItem.Click
        dlgErrorViewer.Show(Me)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click, cmnuTrayIconExit.Click
        If isCL Then
			'fLoading.SetLoadingMesg("Canceling ...")
			fLoading.SetLoadingMesg(Master.eLang.GetString(370, "Canceling Load..."))
            If Me.bwMovieScraper.IsBusy Then Me.bwMovieScraper.CancelAsync()
            If Me.bwRefreshMovies.IsBusy Then Me.bwRefreshMovies.CancelAsync()
            While Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwMovieScraper.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        Else
            Me.Close()
            Application.Exit()
        End If
    End Sub

    Private Sub FillEpisodes(ByVal ShowID As Integer, ByVal Season As Integer)
        Me.bsEpisodes.DataSource = Nothing
        Me.dgvTVEpisodes.DataSource = Nothing

        Application.DoEvents()

        Me.dgvTVEpisodes.Enabled = False

        Master.DB.FillDataTable(Me.dtEpisodes, String.Concat("SELECT * FROM TVEps WHERE TVShowID = ", ShowID, " AND Season = ", Season, " ORDER BY Episode;"))

        If Me.dtEpisodes.Rows.Count > 0 Then

            With Me
                .bsEpisodes.DataSource = .dtEpisodes
                .dgvTVEpisodes.DataSource = .bsEpisodes

                .dgvTVEpisodes.Columns(0).Visible = False
                .dgvTVEpisodes.Columns(1).Visible = False
                .dgvTVEpisodes.Columns(2).Resizable = DataGridViewTriState.False
                .dgvTVEpisodes.Columns(2).ReadOnly = True
                .dgvTVEpisodes.Columns(2).Width = 40
                .dgvTVEpisodes.Columns(2).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVEpisodes.Columns(2).ToolTipText = Master.eLang.GetString(755, "Episode #")
                .dgvTVEpisodes.Columns(2).HeaderText = "#"
                .dgvTVEpisodes.Columns(3).Resizable = DataGridViewTriState.True
                .dgvTVEpisodes.Columns(3).ReadOnly = True
                .dgvTVEpisodes.Columns(3).MinimumWidth = 83
                .dgvTVEpisodes.Columns(3).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVEpisodes.Columns(3).ToolTipText = Master.eLang.GetString(21, "Title")
                .dgvTVEpisodes.Columns(3).HeaderText = Master.eLang.GetString(21, "Title")
                .dgvTVEpisodes.Columns(4).Width = 20
                .dgvTVEpisodes.Columns(4).Resizable = DataGridViewTriState.False
                .dgvTVEpisodes.Columns(4).ReadOnly = True
                .dgvTVEpisodes.Columns(4).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVEpisodes.Columns(4).Visible = Not Master.eSettings.EpisodePosterCol
                .dgvTVEpisodes.Columns(4).ToolTipText = Master.eLang.GetString(148, "Poster")
                .dgvTVEpisodes.Columns(5).Width = 20
                .dgvTVEpisodes.Columns(5).Resizable = DataGridViewTriState.False
                .dgvTVEpisodes.Columns(5).ReadOnly = True
                .dgvTVEpisodes.Columns(5).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVEpisodes.Columns(5).Visible = Not Master.eSettings.EpisodeFanartCol
                .dgvTVEpisodes.Columns(5).ToolTipText = Master.eLang.GetString(149, "Fanart")
                .dgvTVEpisodes.Columns(6).Width = 20
                .dgvTVEpisodes.Columns(6).Resizable = DataGridViewTriState.False
                .dgvTVEpisodes.Columns(6).ReadOnly = True
                .dgvTVEpisodes.Columns(6).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVEpisodes.Columns(6).Visible = Not Master.eSettings.EpisodeNfoCol
                .dgvTVEpisodes.Columns(6).ToolTipText = Master.eLang.GetString(150, "Nfo")
                For i As Integer = 7 To .dgvTVEpisodes.Columns.Count - 1
                    .dgvTVEpisodes.Columns(i).Visible = False
                Next

                .dgvTVEpisodes.Columns(0).ValueType = GetType(Int32)
                .dgvTVEpisodes.Columns(2).ValueType = GetType(Int32)

                If Master.isWindows Then .dgvTVEpisodes.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                ResizeTVLists(3)

                .dgvTVEpisodes.Sort(.dgvTVEpisodes.Columns(2), ComponentModel.ListSortDirection.Ascending)

                .dgvTVEpisodes.ClearSelection()
                .dgvTVEpisodes.CurrentCell = Nothing

            End With
        End If

        Me.dgvTVEpisodes.Enabled = True
    End Sub

    Private Sub FillList(ByVal iIndex As Integer)
        Try
            Me.bsMedia.DataSource = Nothing
            Me.dgvMediaList.DataSource = Nothing
            Me.bsShows.DataSource = Nothing
            Me.dgvTVShows.DataSource = Nothing
            Me.bsSeasons.DataSource = Nothing
            Me.dgvTVSeasons.DataSource = Nothing
            Me.bsEpisodes.DataSource = Nothing
            Me.dgvTVEpisodes.DataSource = Nothing

            Me.ClearInfo()

            If Not String.IsNullOrEmpty(Me.filSearch) AndAlso Me.cbSearch.Text = Master.eLang.GetString(100, "Actor") Then
                Master.DB.FillDataTable(Me.dtMedia, String.Concat("SELECT * FROM movies WHERE ID IN (SELECT MovieID FROM MoviesActors WHERE ActorName LIKE '%", Me.filSearch, "%') ORDER BY ListTitle COLLATE NOCASE;"))
            Else
                If Me.chkFilterDupe.Checked Then
                    Master.DB.FillDataTable(Me.dtMedia, "SELECT * FROM movies WHERE imdb IN (SELECT imdb FROM movies WHERE imdb IS NOT NULL AND LENGTH(imdb) > 0 GROUP BY imdb HAVING ( COUNT(imdb) > 1 )) ORDER BY ListTitle COLLATE NOCASE;")
                Else
                    Master.DB.FillDataTable(Me.dtMedia, "SELECT * FROM movies ORDER BY ListTitle COLLATE NOCASE;")
                End If
            End If

            Master.DB.FillDataTable(Me.dtShows, "SELECT * FROM TVShows ORDER BY Title COLLATE NOCASE;")

            If isCL Then
                Me.LoadingDone = True
            Else
                If Me.dtMedia.Rows.Count > 0 Then

                    With Me
                        .bsMedia.DataSource = .dtMedia
                        .dgvMediaList.DataSource = .bsMedia

                        .dgvMediaList.Columns(0).Visible = False
                        .dgvMediaList.Columns(1).Visible = False
                        .dgvMediaList.Columns(2).Visible = False
                        .dgvMediaList.Columns(3).Resizable = DataGridViewTriState.True
                        .dgvMediaList.Columns(3).ReadOnly = True
                        .dgvMediaList.Columns(3).MinimumWidth = 83
                        .dgvMediaList.Columns(3).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(3).ToolTipText = Master.eLang.GetString(21, "Title")
                        .dgvMediaList.Columns(3).HeaderText = Master.eLang.GetString(21, "Title")
                        .dgvMediaList.Columns(4).Width = 20
                        .dgvMediaList.Columns(4).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(4).ReadOnly = True
                        .dgvMediaList.Columns(4).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(4).Visible = Not Master.eSettings.MoviePosterCol
                        .dgvMediaList.Columns(4).ToolTipText = Master.eLang.GetString(148, "Poster")
                        .dgvMediaList.Columns(5).Width = 20
                        .dgvMediaList.Columns(5).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(5).ReadOnly = True
                        .dgvMediaList.Columns(5).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(5).Visible = Not Master.eSettings.MovieFanartCol
                        .dgvMediaList.Columns(5).ToolTipText = Master.eLang.GetString(149, "Fanart")
                        .dgvMediaList.Columns(6).Width = 20
                        .dgvMediaList.Columns(6).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(6).ReadOnly = True
                        .dgvMediaList.Columns(6).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(6).Visible = Not Master.eSettings.MovieInfoCol
                        .dgvMediaList.Columns(6).ToolTipText = Master.eLang.GetString(150, "Nfo")
                        .dgvMediaList.Columns(7).Width = 20
                        .dgvMediaList.Columns(7).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(7).ReadOnly = True
                        .dgvMediaList.Columns(7).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(7).Visible = Not Master.eSettings.MovieTrailerCol
                        .dgvMediaList.Columns(7).ToolTipText = Master.eLang.GetString(151, "Trailer")
                        .dgvMediaList.Columns(8).Width = 20
                        .dgvMediaList.Columns(8).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(8).ReadOnly = True
                        .dgvMediaList.Columns(8).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(8).Visible = Not Master.eSettings.MovieSubCol
                        .dgvMediaList.Columns(8).ToolTipText = Master.eLang.GetString(152, "Subtitles")
                        .dgvMediaList.Columns(9).Width = 20
                        .dgvMediaList.Columns(9).Resizable = DataGridViewTriState.False
                        .dgvMediaList.Columns(9).ReadOnly = True
                        .dgvMediaList.Columns(9).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvMediaList.Columns(9).Visible = Not Master.eSettings.MovieExtraCol
                        .dgvMediaList.Columns(9).ToolTipText = Master.eLang.GetString(153, "Extrathumbs")
                        For i As Integer = 10 To .dgvMediaList.Columns.Count - 1
                            .dgvMediaList.Columns(i).Visible = False
                        Next

                        .dgvMediaList.Columns(0).ValueType = GetType(Int32)

                        If Master.isWindows Then .dgvMediaList.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        ResizeMediaList()

                        If .dgvMediaList.RowCount > 0 AndAlso Me.tabsMain.SelectedIndex = 0 Then
                            .dgvMediaList.Sort(.dgvMediaList.Columns(3), ComponentModel.ListSortDirection.Ascending)

                            .SetControlsEnabled(True)
                        End If

                    End With
                End If

                Me.dgvTVShows.Enabled = False

                If Me.dtShows.Rows.Count > 0 Then

                    With Me
                        .bsShows.DataSource = .dtShows
                        .dgvTVShows.DataSource = .bsShows

                        .dgvTVShows.Columns(0).Visible = False
                        .dgvTVShows.Columns(1).Resizable = DataGridViewTriState.True
                        .dgvTVShows.Columns(1).ReadOnly = True
                        .dgvTVShows.Columns(1).MinimumWidth = 83
                        .dgvTVShows.Columns(1).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvTVShows.Columns(1).ToolTipText = Master.eLang.GetString(21, "Title")
                        .dgvTVShows.Columns(1).HeaderText = Master.eLang.GetString(21, "Title")
                        .dgvTVShows.Columns(2).Width = 20
                        .dgvTVShows.Columns(2).Resizable = DataGridViewTriState.False
                        .dgvTVShows.Columns(2).ReadOnly = True
                        .dgvTVShows.Columns(2).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvTVShows.Columns(2).Visible = Not Master.eSettings.ShowPosterCol
                        .dgvTVShows.Columns(2).ToolTipText = Master.eLang.GetString(148, "Poster")
                        .dgvTVShows.Columns(3).Width = 20
                        .dgvTVShows.Columns(3).Resizable = DataGridViewTriState.False
                        .dgvTVShows.Columns(3).ReadOnly = True
                        .dgvTVShows.Columns(3).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvTVShows.Columns(3).Visible = Not Master.eSettings.ShowFanartCol
                        .dgvTVShows.Columns(3).ToolTipText = Master.eLang.GetString(149, "Fanart")
                        .dgvTVShows.Columns(4).Width = 20
                        .dgvTVShows.Columns(4).Resizable = DataGridViewTriState.False
                        .dgvTVShows.Columns(4).ReadOnly = True
                        .dgvTVShows.Columns(4).SortMode = DataGridViewColumnSortMode.Automatic
                        .dgvTVShows.Columns(4).Visible = Not Master.eSettings.ShowNfoCol
                        .dgvTVShows.Columns(4).ToolTipText = Master.eLang.GetString(150, "Nfo")
                        For i As Integer = 5 To .dgvTVShows.Columns.Count - 1
                            .dgvTVShows.Columns(i).Visible = False
                        Next

                        .dgvTVShows.Columns(0).ValueType = GetType(Int32)

                        If Master.isWindows Then .dgvTVShows.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        ResizeTVLists(1)

                        If .dgvTVShows.RowCount > 0 Then
                            .dgvTVShows.Sort(.dgvTVShows.Columns(1), ComponentModel.ListSortDirection.Ascending)

                            .SetControlsEnabled(True)
                        End If
                    End With
                End If
                Me.dgvTVShows.Enabled = True
            End If

            If Me.dtMedia.Rows.Count = 0 AndAlso Me.dtShows.Rows.Count = 0 Then
                Me.SetControlsEnabled(False, False, False)
                Me.SetStatus(String.Empty)
                Me.ClearInfo()
            End If
        Catch ex As Exception
            Me.LoadingDone = True
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        If Not isCL Then
            Me.tsbRefreshMedia.Enabled = True
            Me.cmnuTrayIconExit.Enabled = True
            Me.cmnuTrayIconSettings.Enabled = True
            Me.EditToolStripMenuItem.Enabled = True
            Me.cmnuTrayIconUpdateMedia.Enabled = True
            Me.HelpToolStripMenuItem.Enabled = True
            Me.tslLoading.Visible = False
            Me.tspbLoading.Visible = False
            Me.tspbLoading.Value = 0
            Me.tabsMain.Enabled = True
            Me.DoTitleCheck()
            Me.EnableFilters(True)
            Me.SetTVCount()
        End If
    End Sub

    Private Sub fillScreenInfoWithEpisode()
        Dim g As Graphics
        Dim strSize As String
        Dim lenSize As Integer
        Dim rect As Rectangle

        Try
            Me.SuspendLayout()
            Me.lblTitle.Text = If(String.IsNullOrEmpty(Master.currShow.Filename), String.Concat(Master.currShow.TVEp.Title, Master.eLang.GetString(689, " [MISSING]")), Master.currShow.TVEp.Title)
            Me.txtPlot.Text = Master.currShow.TVEp.Plot
            Me.lblDirector.Text = Master.currShow.TVEp.Director
            Me.txtFilePath.Text = Master.currShow.Filename
            Me.lblRuntime.Text = String.Format(Master.eLang.GetString(647, "Aired: {0}"), If(String.IsNullOrEmpty(Master.currShow.TVEp.Aired), "?", Master.currShow.TVEp.Aired))

            Me.lblTagline.Text = String.Format(Master.eLang.GetString(648, "Season: {0}, Episode: {1}"), _
                            If(String.IsNullOrEmpty(Master.currShow.TVEp.Season.ToString), "?", Master.currShow.TVEp.Season.ToString), _
                            If(String.IsNullOrEmpty(Master.currShow.TVEp.Episode.ToString), "?", Master.currShow.TVEp.Episode.ToString))

            Me.alActors = New List(Of String)

            If Master.currShow.TVEp.Actors.Count > 0 Then
                Me.pbActors.Image = My.Resources.actor_silhouette
                For Each imdbAct As MediaContainers.Person In Master.currShow.TVEp.Actors
                    If Not String.IsNullOrEmpty(imdbAct.Thumb) Then
                        If Not imdbAct.Thumb.ToLower.IndexOf("addtiny.gif") > 0 AndAlso Not imdbAct.Thumb.ToLower.IndexOf("no_photo") > 0 Then
                            Me.alActors.Add(imdbAct.Thumb)
                        Else
                            Me.alActors.Add("none")
                        End If
                    Else
                        Me.alActors.Add("none")
                    End If

                    If String.IsNullOrEmpty(imdbAct.Role.Trim) Then
                        Me.lstActors.Items.Add(imdbAct.Name.Trim)
                    Else
                        Me.lstActors.Items.Add(String.Format(Master.eLang.GetString(131, "{0} as {1}"), imdbAct.Name.Trim, imdbAct.Role.Trim))
                    End If
                Next
                Me.lstActors.SelectedIndex = 0
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.MPAA) Then
                Dim tmpRatingImg As Image = APIXML.GetTVRatingImage(Master.currShow.TVShow.MPAA)
                If Not IsNothing(tmpRatingImg) Then
                    Me.pbMPAA.Image = tmpRatingImg
                    Me.MoveMPAA()
                End If
            End If

            Dim tmpRating As Single = NumUtils.ConvertToSingle(Master.currShow.TVEp.Rating)
            If tmpRating > 0 Then
                Me.BuildStars(tmpRating)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Genre) Then
                Me.createGenreThumbs(Master.currShow.TVShow.Genre)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Studio) Then
                Me.pbStudio.Image = APIXML.GetStudioImage(Master.currShow.TVShow.Studio)
                Me.pbStudio.Tag = Master.currShow.TVShow.Studio
            Else
                Me.pbStudio.Image = APIXML.GetStudioImage("####")
                Me.pbStudio.Tag = String.Empty
            End If
            If AdvancedSettings.GetBooleanSetting("StudioTagAlwaysOn", False) Then
                lblStudio.Text = pbStudio.Tag.ToString
            End If
            If Master.eSettings.ScanTVMediaInfo AndAlso Not String.IsNullOrEmpty(Master.currShow.Filename) Then
                Me.SetAVImages(APIXML.GetAVImages(Master.currShow.TVEp.FileInfo, Master.currShow.Filename, True, APIXML.GetFileSource(Master.currShow.Filename)))
                Me.pnlInfoIcons.Width = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + pbStudio.Width + 6
                Me.pbStudio.Left = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + 5
            Else
                Me.pnlInfoIcons.Width = pbStudio.Width + 1
                Me.pbStudio.Left = 0
            End If

            Me.txtMetaData.Text = NFO.FIToString(Master.currShow.TVEp.FileInfo, True)

            If Not IsNothing(Me.MainPoster.Image) Then
                Me.pbPosterCache.Image = Me.MainPoster.Image
                ImageUtils.ResizePB(Me.pbPoster, Me.pbPosterCache, Me.PosterMaxHeight, Me.PosterMaxWidth)
                If AdvancedSettings.GetBooleanSetting("PosterGlassOverlay", True) Then ImageUtils.SetGlassOverlay(Me.pbPoster)
                Me.pnlPoster.Size = New Size(Me.pbPoster.Width + 10, Me.pbPoster.Height + 10)

                If Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbPoster.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainPoster.Image.Width, Me.MainPoster.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2 - 15), Me.pbPoster.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2), Me.pbPoster.Height - 20)
                End If

                Me.pbPoster.Location = New Point(4, 4)
            Else
                If Not IsNothing(Me.pbPoster.Image) Then
                    Me.pbPoster.Image.Dispose()
                    Me.pbPoster.Image = Nothing
                End If
            End If

            If Not IsNothing(Me.MainFanart.Image) Then
                Me.pbFanartCache.Image = Me.MainFanart.Image

                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)

                If Not IsNothing(pbFanart.Image) AndAlso Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbFanart.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainFanart.Image.Width, Me.MainFanart.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbFanart.Image.Width - lenSize) / 2 - 15), Me.pbFanart.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((Me.pbFanart.Image.Width - lenSize) / 2), Me.pbFanart.Height - 20)
                End If
            Else
                If Not IsNothing(Me.pbFanartCache.Image) Then
                    Me.pbFanartCache.Image.Dispose()
                    Me.pbFanartCache.Image = Nothing
                End If
                If Not IsNothing(Me.pbFanart.Image) Then
                    Me.pbFanart.Image.Dispose()
                    Me.pbFanart.Image = Nothing
                End If
            End If

            Me.InfoCleared = False

            If Not bwMovieScraper.IsBusy AndAlso Not bwRefreshMovies.IsBusy AndAlso Not bwCleanDB.IsBusy Then
                Me.SetControlsEnabled(True)
                Me.dgvTVEpisodes.Focus()
            Else
                Me.dgvTVEpisodes.Enabled = True
                Me.dgvTVSeasons.Enabled = True
                Me.dgvTVShows.Enabled = True
                Me.dgvTVEpisodes.Focus()
            End If

            Application.DoEvents()

            Me.pnlTop.Visible = True
            If Not IsNothing(Me.pbAllSeason.Image) Then Me.pnlAllSeason.Visible = True
            If Not IsNothing(Me.pbPoster.Image) Then Me.pnlPoster.Visible = True
            If Not IsNothing(Me.pbMPAA.Image) Then Me.pnlMPAA.Visible = True
            For i As Integer = 0 To UBound(Me.pnlGenre)
                Me.pnlGenre(i).Visible = True
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Me.ResumeLayout()
    End Sub

    Private Sub fillScreenInfoWithMovie()
        Dim g As Graphics
        Dim strSize As String
        Dim lenSize As Integer
        Dim rect As Rectangle

        Try
            Me.SuspendLayout()
            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Title) AndAlso Not String.IsNullOrEmpty(Master.currMovie.Movie.Year) Then
                Me.lblTitle.Text = String.Format("{0} ({1})", Master.currMovie.Movie.Title, Master.currMovie.Movie.Year)
            ElseIf Not String.IsNullOrEmpty(Master.currMovie.Movie.Title) AndAlso String.IsNullOrEmpty(Master.currMovie.Movie.Year) Then
                Me.lblTitle.Text = Master.currMovie.Movie.Title
            ElseIf String.IsNullOrEmpty(Master.currMovie.Movie.Title) AndAlso Not String.IsNullOrEmpty(Master.currMovie.Movie.Year) Then
                Me.lblTitle.Text = String.Format(Master.eLang.GetString(117, "Unknown Movie ({0})"), Master.currMovie.Movie.Year)
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.OriginalTitle) AndAlso Master.currMovie.Movie.OriginalTitle <> StringUtils.FilterTokens(Master.currMovie.Movie.Title) Then
                Me.lblOriginalTitle.Text = String.Format(String.Concat(Master.eLang.GetString(302, "Original Title"), ": {0}"), Master.currMovie.Movie.OriginalTitle)
            Else
                Me.lblOriginalTitle.Text = String.Empty
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Votes) Then
                Me.lblVotes.Text = String.Format(Master.eLang.GetString(118, "{0} Votes"), Master.currMovie.Movie.Votes)
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Runtime) Then
                Me.lblRuntime.Text = String.Format(Master.eLang.GetString(112, "Runtime: {0}"), If(Master.currMovie.Movie.Runtime.Contains("|"), Microsoft.VisualBasic.Strings.Left(Master.currMovie.Movie.Runtime, Master.currMovie.Movie.Runtime.IndexOf("|")), Master.currMovie.Movie.Runtime)).Trim
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Top250) AndAlso IsNumeric(Master.currMovie.Movie.Top250) AndAlso (IsNumeric(Master.currMovie.Movie.Top250) AndAlso Convert.ToInt32(Master.currMovie.Movie.Top250) > 0) Then
                Me.pnlTop250.Visible = True
                Me.lblTop250.Text = Master.currMovie.Movie.Top250
            Else
                Me.pnlTop250.Visible = False
            End If

            Me.txtOutline.Text = Master.currMovie.Movie.Outline
            Me.txtPlot.Text = Master.currMovie.Movie.Plot
            Me.lblTagline.Text = Master.currMovie.Movie.Tagline

            Me.alActors = New List(Of String)

            If Master.currMovie.Movie.Actors.Count > 0 Then
                Me.pbActors.Image = My.Resources.actor_silhouette
                For Each imdbAct As MediaContainers.Person In Master.currMovie.Movie.Actors
                    If Not String.IsNullOrEmpty(imdbAct.Thumb) Then
                        If Not imdbAct.Thumb.ToLower.IndexOf("addtiny.gif") > 0 AndAlso Not imdbAct.Thumb.ToLower.IndexOf("no_photo") > 0 Then
                            Me.alActors.Add(imdbAct.Thumb)
                        Else
                            Me.alActors.Add("none")
                        End If
                    Else
                        Me.alActors.Add("none")
                    End If

                    If String.IsNullOrEmpty(imdbAct.Role.Trim) Then
                        Me.lstActors.Items.Add(imdbAct.Name.Trim)
                    Else
                        Me.lstActors.Items.Add(String.Format(Master.eLang.GetString(131, "{0} as {1}"), imdbAct.Name.Trim, imdbAct.Role.Trim))
                    End If
                Next
                Me.lstActors.SelectedIndex = 0
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.MPAA) Then
                Dim tmpRatingImg As Image = APIXML.GetRatingImage(Master.currMovie.Movie.MPAA)
                If Not IsNothing(tmpRatingImg) Then
                    Me.pbMPAA.Image = tmpRatingImg
                    Me.MoveMPAA()
                End If
            End If

            Dim tmpRating As Single = NumUtils.ConvertToSingle(Master.currMovie.Movie.Rating)
            If tmpRating > 0 Then
                Me.BuildStars(tmpRating)
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Genre) Then
                Me.createGenreThumbs(Master.currMovie.Movie.Genre)
            End If

            If Not String.IsNullOrEmpty(Master.currMovie.Movie.Studio) Then
                Me.pbStudio.Image = APIXML.GetStudioImage(Master.currMovie.Movie.Studio.ToLower) 'ByDef all images file a lower case
                Me.pbStudio.Tag = Master.currMovie.Movie.Studio
            Else
                Me.pbStudio.Image = APIXML.GetStudioImage("####")
                Me.pbStudio.Tag = String.Empty
            End If
            If AdvancedSettings.GetBooleanSetting("StudioTagAlwaysOn", False) Then
                lblStudio.Text = pbStudio.Tag.ToString
            End If
            If Master.eSettings.ScanMediaInfo Then
                'Me.SetAVImages(APIXML.GetAVImages(Master.currMovie.Movie.FileInfo, Master.currMovie.Filename, False))
                Me.SetAVImages(APIXML.GetAVImages(Master.currMovie.Movie.FileInfo, Master.currMovie.Filename, False, Master.currMovie.FileSource))
                Me.pnlInfoIcons.Width = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + pbStudio.Width + 6
                Me.pbStudio.Left = pbVideo.Width + pbVType.Width + pbResolution.Width + pbAudio.Width + pbChannels.Width + 5
            Else
                Me.pnlInfoIcons.Width = pbStudio.Width + 1
                Me.pbStudio.Left = 0
            End If

            Me.lblDirector.Text = Master.currMovie.Movie.Director

            Me.txtIMDBID.Text = Master.currMovie.Movie.IMDBID

            Me.txtFilePath.Text = Master.currMovie.Filename

            Me.lblReleaseDate.Text = Master.currMovie.Movie.ReleaseDate
            Me.txtCerts.Text = Master.currMovie.Movie.Certification

            Me.txtMetaData.Text = NFO.FIToString(Master.currMovie.Movie.FileInfo, False)

            If Not IsNothing(Me.MainPoster.Image) Then
                Me.pbPosterCache.Image = Me.MainPoster.Image
                ImageUtils.ResizePB(Me.pbPoster, Me.pbPosterCache, Me.PosterMaxHeight, Me.PosterMaxWidth)
                If AdvancedSettings.GetBooleanSetting("PosterGlassOverlay", True) Then ImageUtils.SetGlassOverlay(Me.pbPoster)
                Me.pnlPoster.Size = New Size(Me.pbPoster.Width + 10, Me.pbPoster.Height + 10)

                If Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbPoster.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainPoster.Image.Width, Me.MainPoster.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2 - 15), Me.pbPoster.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2), Me.pbPoster.Height - 20)
                End If

                Me.pbPoster.Location = New Point(4, 4)
            Else
                If Not IsNothing(Me.pbPoster.Image) Then
                    Me.pbPoster.Image.Dispose()
                    Me.pbPoster.Image = Nothing
                End If
            End If

            If Not IsNothing(Me.MainFanart.Image) Then
                Me.pbFanartCache.Image = Me.MainFanart.Image

                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)

                If Not IsNothing(pbFanart.Image) AndAlso Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbFanart.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainFanart.Image.Width, Me.MainFanart.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbFanart.Image.Width - lenSize) / 2 - 15), Me.pbFanart.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((Me.pbFanart.Image.Width - lenSize) / 2), Me.pbFanart.Height - 20)
                End If
            Else
                If Not IsNothing(Me.pbFanartCache.Image) Then
                    Me.pbFanartCache.Image.Dispose()
                    Me.pbFanartCache.Image = Nothing
                End If
                If Not IsNothing(Me.pbFanart.Image) Then
                    Me.pbFanart.Image.Dispose()
                    Me.pbFanart.Image = Nothing
                End If
            End If

            Me.InfoCleared = False

            If Not bwMovieScraper.IsBusy AndAlso Not bwRefreshMovies.IsBusy AndAlso Not bwCleanDB.IsBusy AndAlso Not Me.bwNonScrape.IsBusy Then
                Me.SetControlsEnabled(True)
                Me.EnableFilters(True)
            Else
                Me.dgvMediaList.Enabled = True
            End If
            If bDoingSearch Then
                Me.txtSearch.Focus()
                bDoingSearch = False
            Else
                Me.dgvMediaList.Focus()
            End If


            Application.DoEvents()

            Me.pnlTop.Visible = True
            If Not IsNothing(Me.pbPoster.Image) Then Me.pnlPoster.Visible = True
            If Not IsNothing(Me.pbMPAA.Image) Then Me.pnlMPAA.Visible = True
            For i As Integer = 0 To UBound(Me.pnlGenre)
                Me.pnlGenre(i).Visible = True
            Next
            'Me.SetStatus(Master.currMovie.Filename)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Me.ResumeLayout()
    End Sub

    Private Sub fillScreenInfoWithSeason()
        Dim g As Graphics
        Dim strSize As String
        Dim lenSize As Integer
        Dim rect As Rectangle

        Try
            Me.SuspendLayout()
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Title) Then
                Me.lblTitle.Text = Master.currShow.TVShow.Title
            End If

            Me.txtPlot.Text = Master.currShow.TVShow.Plot
            Me.lblRuntime.Text = String.Format(Master.eLang.GetString(645, "Premiered: {0}"), If(String.IsNullOrEmpty(Master.currShow.TVShow.Premiered), "?", Master.currShow.TVShow.Premiered))

            Me.alActors = New List(Of String)

            If Master.currShow.TVShow.Actors.Count > 0 Then
                Me.pbActors.Image = My.Resources.actor_silhouette
                For Each imdbAct As MediaContainers.Person In Master.currShow.TVShow.Actors
                    If Not String.IsNullOrEmpty(imdbAct.Thumb) Then
                        If Not imdbAct.Thumb.ToLower.IndexOf("addtiny.gif") > 0 AndAlso Not imdbAct.Thumb.ToLower.IndexOf("no_photo") > 0 Then
                            Me.alActors.Add(imdbAct.Thumb)
                        Else
                            Me.alActors.Add("none")
                        End If
                    Else
                        Me.alActors.Add("none")
                    End If

                    If String.IsNullOrEmpty(imdbAct.Role.Trim) Then
                        Me.lstActors.Items.Add(imdbAct.Name.Trim)
                    Else
                        Me.lstActors.Items.Add(String.Format(Master.eLang.GetString(131, "{0} as {1}"), imdbAct.Name.Trim, imdbAct.Role.Trim))
                    End If
                Next
                Me.lstActors.SelectedIndex = 0
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.MPAA) Then
                Dim tmpRatingImg As Image = APIXML.GetTVRatingImage(Master.currShow.TVShow.MPAA)
                If Not IsNothing(tmpRatingImg) Then
                    Me.pbMPAA.Image = tmpRatingImg
                    Me.MoveMPAA()
                End If
            End If

            Dim tmpRating As Single = NumUtils.ConvertToSingle(Master.currShow.TVShow.Rating)
            If tmpRating > 0 Then
                Me.BuildStars(tmpRating)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Genre) Then
                Me.createGenreThumbs(Master.currShow.TVShow.Genre)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Studio) Then
                Me.pbStudio.Image = APIXML.GetStudioImage(Master.currShow.TVShow.Studio)
            Else
                Me.pbStudio.Image = APIXML.GetStudioImage("####")
            End If

            Me.pnlInfoIcons.Width = pbStudio.Width + 1
            Me.pbStudio.Left = 0

            If Not IsNothing(Me.MainPoster.Image) Then
                Me.pbPosterCache.Image = Me.MainPoster.Image
                ImageUtils.ResizePB(Me.pbPoster, Me.pbPosterCache, Me.PosterMaxHeight, Me.PosterMaxWidth)
                If AdvancedSettings.GetBooleanSetting("PosterGlassOverlay", True) Then ImageUtils.SetGlassOverlay(Me.pbPoster)
                Me.pnlPoster.Size = New Size(Me.pbPoster.Width + 10, Me.pbPoster.Height + 10)

                If Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbPoster.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainPoster.Image.Width, Me.MainPoster.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2 - 15), Me.pbPoster.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2), Me.pbPoster.Height - 20)
                End If

                Me.pbPoster.Location = New Point(4, 4)
            Else
                If Not IsNothing(Me.pbPoster.Image) Then
                    Me.pbPoster.Image.Dispose()
                    Me.pbPoster.Image = Nothing
                End If
            End If

            If Not IsNothing(Me.MainFanart.Image) Then
                Me.pbFanartCache.Image = Me.MainFanart.Image

                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)

                If Not IsNothing(pbFanart.Image) AndAlso Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbFanart.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainFanart.Image.Width, Me.MainFanart.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbFanart.Image.Width - lenSize) / 2 - 15), Me.pbFanart.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((Me.pbFanart.Image.Width - lenSize) / 2), Me.pbFanart.Height - 20)
                End If
            Else
                If Not IsNothing(Me.pbFanartCache.Image) Then
                    Me.pbFanartCache.Image.Dispose()
                    Me.pbFanartCache.Image = Nothing
                End If
                If Not IsNothing(Me.pbFanart.Image) Then
                    Me.pbFanart.Image.Dispose()
                    Me.pbFanart.Image = Nothing
                End If
            End If

            Me.InfoCleared = False

            If Not bwMovieScraper.IsBusy AndAlso Not bwRefreshMovies.IsBusy AndAlso Not bwCleanDB.IsBusy Then
                Me.SetControlsEnabled(True)
                Me.dgvTVSeasons.Focus()
            Else
                Me.dgvTVEpisodes.Enabled = True
                Me.dgvTVSeasons.Enabled = True
                Me.dgvTVShows.Enabled = True
                Me.dgvTVSeasons.Focus()
            End If

            Application.DoEvents()

            Me.pnlTop.Visible = True
            If Not IsNothing(Me.pbPoster.Image) Then Me.pnlPoster.Visible = True
            If Not IsNothing(Me.pbAllSeason.Image) Then Me.pnlAllSeason.Visible = True
            If Not IsNothing(Me.pbMPAA.Image) Then Me.pnlMPAA.Visible = True
            For i As Integer = 0 To UBound(Me.pnlGenre)
                Me.pnlGenre(i).Visible = True
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Me.ResumeLayout()
    End Sub

    Private Sub fillScreenInfoWithShow()
        Dim g As Graphics
        Dim strSize As String
        Dim lenSize As Integer
        Dim rect As Rectangle

        Try
            Me.SuspendLayout()
            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Title) Then
                Me.lblTitle.Text = Master.currShow.TVShow.Title
            End If

            Me.txtPlot.Text = Master.currShow.TVShow.Plot
            Me.lblRuntime.Text = String.Format(Master.eLang.GetString(645, "Premiered: {0}"), If(String.IsNullOrEmpty(Master.currShow.TVShow.Premiered), "?", Master.currShow.TVShow.Premiered))

            Me.alActors = New List(Of String)

            If Master.currShow.TVShow.Actors.Count > 0 Then
                Me.pbActors.Image = My.Resources.actor_silhouette
                For Each imdbAct As MediaContainers.Person In Master.currShow.TVShow.Actors
                    If Not String.IsNullOrEmpty(imdbAct.Thumb) Then
                        If Not imdbAct.Thumb.ToLower.IndexOf("addtiny.gif") > 0 AndAlso Not imdbAct.Thumb.ToLower.IndexOf("no_photo") > 0 Then
                            Me.alActors.Add(imdbAct.Thumb)
                        Else
                            Me.alActors.Add("none")
                        End If
                    Else
                        Me.alActors.Add("none")
                    End If

                    If String.IsNullOrEmpty(imdbAct.Role.Trim) Then
                        Me.lstActors.Items.Add(imdbAct.Name.Trim)
                    Else
                        Me.lstActors.Items.Add(String.Format(Master.eLang.GetString(131, "{0} as {1}"), imdbAct.Name.Trim, imdbAct.Role.Trim))
                    End If
                Next
                Me.lstActors.SelectedIndex = 0
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.MPAA) Then
                Dim tmpRatingImg As Image = APIXML.GetTVRatingImage(Master.currShow.TVShow.MPAA)
                If Not IsNothing(tmpRatingImg) Then
                    Me.pbMPAA.Image = tmpRatingImg
                    Me.MoveMPAA()
                End If
            End If

            Dim tmpRating As Single = NumUtils.ConvertToSingle(Master.currShow.TVShow.Rating)
            If tmpRating > 0 Then
                Me.BuildStars(tmpRating)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Genre) Then
                Me.createGenreThumbs(Master.currShow.TVShow.Genre)
            End If

            If Not String.IsNullOrEmpty(Master.currShow.TVShow.Studio) Then
                Me.pbStudio.Image = APIXML.GetStudioImage(Master.currShow.TVShow.Studio)
            Else
                Me.pbStudio.Image = APIXML.GetStudioImage("####")
            End If

            Me.pnlInfoIcons.Width = pbStudio.Width + 1
            Me.pbStudio.Left = 0

            If Not IsNothing(Me.MainPoster.Image) Then
                Me.pbPosterCache.Image = Me.MainPoster.Image
                ImageUtils.ResizePB(Me.pbPoster, Me.pbPosterCache, Me.PosterMaxHeight, Me.PosterMaxWidth)
                If AdvancedSettings.GetBooleanSetting("PosterGlassOverlay", True) Then ImageUtils.SetGlassOverlay(Me.pbPoster)
                Me.pnlPoster.Size = New Size(Me.pbPoster.Width + 10, Me.pbPoster.Height + 10)

                If Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbPoster.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainPoster.Image.Width, Me.MainPoster.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2 - 15), Me.pbPoster.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((pbPoster.Image.Width - lenSize) / 2), Me.pbPoster.Height - 20)
                End If

                Me.pbPoster.Location = New Point(4, 4)
            Else
                If Not IsNothing(Me.pbPoster.Image) Then
                    Me.pbPoster.Image.Dispose()
                    Me.pbPoster.Image = Nothing
                End If
            End If

            If Not IsNothing(Me.MainFanart.Image) Then
                Me.pbFanartCache.Image = Me.MainFanart.Image

                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)

                If Not IsNothing(pbFanart.Image) AndAlso Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(pbFanart.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainFanart.Image.Width, Me.MainFanart.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((pbFanart.Image.Width - lenSize) / 2 - 15), Me.pbFanart.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((Me.pbFanart.Image.Width - lenSize) / 2), Me.pbFanart.Height - 20)
                End If
            Else
                If Not IsNothing(Me.pbFanartCache.Image) Then
                    Me.pbFanartCache.Image.Dispose()
                    Me.pbFanartCache.Image = Nothing
                End If
                If Not IsNothing(Me.pbFanart.Image) Then
                    Me.pbFanart.Image.Dispose()
                    Me.pbFanart.Image = Nothing
                End If
            End If

            If Not IsNothing(Me.MainAllSeason.Image) Then
                Me.pbAllSeasonCache.Image = Me.MainAllSeason.Image
                ImageUtils.ResizePB(Me.pbAllSeason, Me.pbAllSeasonCache, Me.PosterMaxHeight, Me.PosterMaxWidth)
                If AdvancedSettings.GetBooleanSetting("PosterGlassOverlay", True) Then ImageUtils.SetGlassOverlay(Me.pbAllSeason)
                Me.pnlAllSeason.Size = New Size(Me.pbAllSeason.Width + 10, Me.pbAllSeason.Height + 10)

                If Master.eSettings.ShowDims Then
                    g = Graphics.FromImage(Me.pbAllSeason.Image)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    strSize = String.Format("{0} x {1}", Me.MainAllSeason.Image.Width, Me.MainAllSeason.Image.Height)
                    lenSize = Convert.ToInt32(g.MeasureString(strSize, New Font("Arial", 8, FontStyle.Bold)).Width)
                    rect = New Rectangle(Convert.ToInt32((Me.pbAllSeason.Image.Width - lenSize) / 2 - 15), Me.pbAllSeason.Height - 25, lenSize + 30, 25)
                    ImageUtils.DrawGradEllipse(g, rect, Color.FromArgb(250, 120, 120, 120), Color.FromArgb(0, 255, 255, 255))
                    g.DrawString(strSize, New Font("Arial", 8, FontStyle.Bold), New SolidBrush(Color.White), Convert.ToInt32((Me.pbAllSeason.Image.Width - lenSize) / 2), Me.pbAllSeason.Height - 20)
                End If

                Me.pbAllSeason.Location = New Point(4, 4)
                Me.pnlAllSeason.Location = New Point(Me.pbFanart.Width - Me.pnlAllSeason.Width - 9, 112)
            Else
                If Not IsNothing(Me.pbAllSeason.Image) Then
                    Me.pbAllSeason.Image.Dispose()
                    Me.pbAllSeason.Image = Nothing
                End If
            End If

            Me.InfoCleared = False

            If Not bwMovieScraper.IsBusy AndAlso Not bwRefreshMovies.IsBusy AndAlso Not bwCleanDB.IsBusy Then
                Me.SetControlsEnabled(True)
                Me.dgvTVShows.Focus()
            Else
                Me.dgvTVEpisodes.Enabled = True
                Me.dgvTVSeasons.Enabled = True
                Me.dgvTVShows.Enabled = True
                Me.dgvTVShows.Focus()
            End If

            Application.DoEvents()

            Me.pnlTop.Visible = True
            If Not IsNothing(Me.pbAllSeason.Image) Then Me.pnlAllSeason.Visible = True
            If Not IsNothing(Me.pbPoster.Image) Then Me.pnlPoster.Visible = True
            If Not IsNothing(Me.pbMPAA.Image) Then Me.pnlMPAA.Visible = True
            For i As Integer = 0 To UBound(Me.pnlGenre)
                Me.pnlGenre(i).Visible = True
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Me.ResumeLayout()
    End Sub

    Private Sub FillSeasons(ByVal ShowID As Integer)
        Me.bsSeasons.DataSource = Nothing
        Me.dgvTVSeasons.DataSource = Nothing
        Me.bsEpisodes.DataSource = Nothing
        Me.dgvTVEpisodes.DataSource = Nothing

        Application.DoEvents()

        Me.dgvTVSeasons.Enabled = False

        Master.DB.FillDataTable(Me.dtSeasons, String.Concat("SELECT * FROM TVSeason WHERE TVShowID = ", ShowID, " AND Season <> 999 ORDER BY Season;"))

        If Me.dtSeasons.Rows.Count > 0 Then

            With Me
                .bsSeasons.DataSource = .dtSeasons
                .dgvTVSeasons.DataSource = .bsSeasons

                .dgvTVSeasons.Columns(0).Visible = False
                .dgvTVSeasons.Columns(1).Resizable = DataGridViewTriState.True
                .dgvTVSeasons.Columns(1).ReadOnly = True
                .dgvTVSeasons.Columns(1).MinimumWidth = 83
                .dgvTVSeasons.Columns(1).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVSeasons.Columns(1).ToolTipText = Master.eLang.GetString(650, "Season")
                .dgvTVSeasons.Columns(1).HeaderText = Master.eLang.GetString(650, "Season")
                .dgvTVSeasons.Columns(2).Visible = False
                .dgvTVSeasons.Columns(3).Width = 20
                .dgvTVSeasons.Columns(3).Resizable = DataGridViewTriState.False
                .dgvTVSeasons.Columns(3).ReadOnly = True
                .dgvTVSeasons.Columns(3).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVSeasons.Columns(3).Visible = Not Master.eSettings.SeasonPosterCol
                .dgvTVSeasons.Columns(3).ToolTipText = Master.eLang.GetString(148, "Poster")
                .dgvTVSeasons.Columns(4).Width = 20
                .dgvTVSeasons.Columns(4).Resizable = DataGridViewTriState.False
                .dgvTVSeasons.Columns(4).ReadOnly = True
                .dgvTVSeasons.Columns(4).SortMode = DataGridViewColumnSortMode.Automatic
                .dgvTVSeasons.Columns(4).Visible = Not Master.eSettings.SeasonFanartCol
                .dgvTVSeasons.Columns(4).ToolTipText = Master.eLang.GetString(149, "Fanart")
                For i As Integer = 5 To .dgvTVSeasons.Columns.Count - 1
                    .dgvTVSeasons.Columns(i).Visible = False
                Next

                .dgvTVSeasons.Columns(0).ValueType = GetType(Int32)

                If Master.isWindows Then .dgvTVSeasons.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                ResizeTVLists(2)

                .dgvTVSeasons.Sort(.dgvTVSeasons.Columns(1), ComponentModel.ListSortDirection.Ascending)

                Me.FillEpisodes(ShowID, Convert.ToInt32(.dgvTVSeasons.Item(2, 0).Value))

            End With
        End If

        Me.dgvTVSeasons.Enabled = True
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        Try

            Dim doSave As Boolean = True

            Me.SetControlsEnabled(False, True)
            Me.EnableFilters(False)

            Master.eSettings.Version = String.Format("r{0}", My.Application.Info.Version.Revision)

            If Me.fScanner.IsBusy OrElse isCL Then
                doSave = False
            End If

            If Me.fScanner.IsBusy Then Me.fScanner.Cancel()
            If Me.bwMediaInfo.IsBusy Then Me.bwMediaInfo.CancelAsync()
            If Me.bwLoadInfo.IsBusy Then Me.bwLoadInfo.CancelAsync()
            If Me.bwLoadShowInfo.IsBusy Then Me.bwLoadShowInfo.CancelAsync()
            If Me.bwLoadSeasonInfo.IsBusy Then Me.bwLoadSeasonInfo.CancelAsync()
            If Me.bwLoadEpInfo.IsBusy Then Me.bwLoadEpInfo.CancelAsync()
            If Me.bwDownloadPic.IsBusy Then Me.bwDownloadPic.CancelAsync()
            If Me.bwRefreshMovies.IsBusy Then Me.bwRefreshMovies.CancelAsync()
            If Me.bwCleanDB.IsBusy Then Me.bwCleanDB.CancelAsync()
            If Me.bwMovieScraper.IsBusy Then Me.bwMovieScraper.CancelAsync()
            If ModulesManager.Instance.TVIsBusy Then ModulesManager.Instance.TVCancelAsync()

            lblCanceling.Text = Master.eLang.GetString(99, "Canceling All Processes...")
            btnCancel.Visible = False
            lblCanceling.Visible = True
            pbCanceling.Visible = True
            pnlCancel.Visible = True
            Me.Refresh()

            While Me.fScanner.IsBusy OrElse Me.bwMediaInfo.IsBusy OrElse Me.bwLoadInfo.IsBusy _
            OrElse Me.bwDownloadPic.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy _
            OrElse Me.bwCleanDB.IsBusy OrElse Me.bwLoadShowInfo.IsBusy OrElse Me.bwLoadEpInfo.IsBusy _
            OrElse Me.bwLoadSeasonInfo.IsBusy OrElse ModulesManager.Instance.TVIsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If doSave Then Master.DB.ClearNew()

            If Not isCL Then
                Master.DB.Close()
            End If
            If Not Master.eSettings.PersistImgCache Then
                Me.ClearCache()
            End If

            If Not isCL Then
                Master.eSettings.WindowLoc = Me.Location
                Master.eSettings.WindowSize = Me.Size
                Master.eSettings.WindowState = Me.WindowState
                Master.eSettings.InfoPanelState = Me.aniType
                Master.eSettings.ShowInfoPanelState = Me.aniShowType
                Master.eSettings.FilterPanelState = Me.aniFilterRaise
                Master.eSettings.SplitterPanelState = Me.scMain.SplitterDistance
                Me.pnlFilter.Visible = False
                Master.eSettings.SeasonSplitterPanelState = Me.SplitContainer2.SplitterDistance
                Master.eSettings.ShowSplitterPanelState = Me.scTV.SplitterDistance
            End If
            If Not Me.WindowState = FormWindowState.Minimized Then Master.eSettings.Save()

        Catch ex As Exception
            ' If we got here, then some of the above not run. Application.Exit can not be used.
            ' Because Exit will dispose object that are in use by BackgroundWorkers
            ' If any BackgroundWorker still running will raise exception
            ' "Collection was modified; enumeration operation may not execute."
            ' Application.Exit()
        End Try
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try

            Me.Visible = False
            fLoading = New frmSplash
            If Master.isWindows Then 'Dam mono on MacOSX don't have trayicon implemented yet
                Me.TrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
                Me.TrayIcon.Icon = Me.Icon
                Me.TrayIcon.ContextMenuStrip = Me.cmnuTrayIcon
                Me.TrayIcon.Text = "Ember Media Manager"
                Me.TrayIcon.Visible = True
            End If

            'Me.TrayIcon.Icon = Me.Icon
            Dim Args() As String = Environment.GetCommandLineArgs

            If Args.Count > 1 Then
                isCL = True
                fLoading.SetProgressBarSize(10)
            End If
            fLoading.Show(Me)
            Application.DoEvents()
			' Run InstallTask to see if any pending file needs to install
            ' Do this before loading modules/themes/etc
            If File.Exists(Path.Combine(Functions.AppPath, "InstallTasks.xml")) Then
                InstallNewFiles("InstallTasks.xml")
            End If

			'fLoading.SetLoadingMesg("Loading settings...")
			fLoading.SetLoadingMesg(Master.eLang.GetString(484, "Loading settings..."))

			Master.eSettings.Load()

			' Force initialization of languages for main
			Master.eLang.LoadAllLanguage(Master.eSettings.Language)
			Master.eLang.LoadLanguage(Master.eSettings.Language, "")
			fLoading.SetVersionMesg(Master.eLang.GetString(865, "Version {0}.{1}.{2}.{3}"))

			'fLoading.SetLoadingMesg("Basic setup...")
			fLoading.SetLoadingMesg(Master.eLang.GetString(854, "Basic setup"))

            Dim currentDomain As AppDomain = AppDomain.CurrentDomain
            ModulesManager.AssemblyList.Add(New ModulesManager.AssemblyListItem With {.AssemblyName = "EmberAPI", _
                    .Assembly = Assembly.LoadFile(Path.Combine(Functions.AppPath, "EmberAPI.dll"), Assembly.GetExecutingAssembly().Evidence)})
            AddHandler currentDomain.AssemblyResolve, AddressOf MyResolveEventHandler

            Dim sPath As String = String.Concat(Functions.AppPath, "Log", Path.DirectorySeparatorChar, "errlog.txt")
            If File.Exists(sPath) Then
                If File.Exists(sPath.Insert(sPath.LastIndexOf("."), "-old")) Then File.Delete(sPath.Insert(sPath.LastIndexOf("."), "-old"))
                FileUtils.Common.MoveFileWithStream(sPath, sPath.Insert(sPath.LastIndexOf("."), "-old"))
                File.Delete(sPath)
            End If
            AdvancedSettings.Start()
            'Create Modules Folders
            sPath = String.Concat(Functions.AppPath, "Modules")
            If Not Directory.Exists(sPath) Then
                Directory.CreateDirectory(sPath)
            End If
            sPath = String.Concat(Functions.AppPath, "Modules", Path.DirectorySeparatorChar, "Langs")
            If Not Directory.Exists(sPath) Then
                Directory.CreateDirectory(sPath)
            End If

			'fLoading.SetLoadingMesg("Creating default options...")
			fLoading.SetLoadingMesg(Master.eLang.GetString(855, "Creating default options..."))
			Functions.CreateDefaultOptions()
            '//
            ' Add our handlers, load settings, set form colors, and try to load movies at startup
            '\\
			'fLoading.SetLoadingMesg("Loading modules...")
			fLoading.SetLoadingMesg(Master.eLang.GetString(856, "Loading modules..."))
			'Setup/Load Modules Manager and set runtime objects (ember application) so they can be exposed to modules
            'ExternalModulesManager = New ModulesManager
            ModulesManager.Instance.RuntimeObjects.MenuMediaList = Me.mnuMediaList
            ModulesManager.Instance.RuntimeObjects.MenuTVShowList = Me.mnuShows
            ModulesManager.Instance.RuntimeObjects.MediaList = Me.dgvMediaList
            ModulesManager.Instance.RuntimeObjects.TopMenu = Me.MenuStrip
            ModulesManager.Instance.RuntimeObjects.MainTool = Me.tsMain
            ModulesManager.Instance.RuntimeObjects.TrayMenu = Me.cmnuTrayIcon
            ModulesManager.Instance.RuntimeObjects.DelegateLoadMedia(AddressOf LoadMedia)
            ModulesManager.Instance.RuntimeObjects.DelegateOpenImageViewer(AddressOf OpenImageViewer)
            ModulesManager.Instance.LoadAllModules()

			If Not isCL Then
				'fLoading.SetLoadingMesg("Creating GUI...")
				fLoading.SetLoadingMesg(Master.eLang.GetString(857, "Creating GUI..."))
			End If
			'setup some dummies so we don't get exceptions when resizing form/info panel
            ReDim Preserve Me.pnlGenre(0)
            ReDim Preserve Me.pbGenre(0)
            Me.pnlGenre(0) = New Panel()
            Me.pbGenre(0) = New PictureBox()

            AddHandler fScanner.ScannerUpdated, AddressOf ScannerUpdated
            AddHandler fScanner.ScanningCompleted, AddressOf ScanningCompleted
            AddHandler ModulesManager.Instance.TVScraperEvent, AddressOf TVScraperEvent
            AddHandler Master.eLog.ErrorOccurred, AddressOf ErrorOccurred
            AddHandler ModulesManager.Instance.GenericEvent, AddressOf Me.GenericRunCallBack

            Functions.DGVDoubleBuffer(Me.dgvMediaList)
            Functions.DGVDoubleBuffer(Me.dgvTVShows)
            Functions.DGVDoubleBuffer(Me.dgvTVSeasons)
            Functions.DGVDoubleBuffer(Me.dgvTVEpisodes)
            SetStyle(ControlStyles.DoubleBuffer, True)
            SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            SetStyle(ControlStyles.UserPaint, True)

            If TypeOf tsMain.Renderer Is ToolStripProfessionalRenderer Then
                CType(tsMain.Renderer, ToolStripProfessionalRenderer).RoundedEdges = False
            End If

            If Not Directory.Exists(Master.TempPath) Then Directory.CreateDirectory(Master.TempPath)

            If isCL Then ' Command Line
                Try
                    Dim MoviePath As String = String.Empty
                    Dim isSingle As Boolean = False
                    Dim hasSpec As Boolean = False
                    Dim clScrapeType As Enums.ScrapeType = Nothing
                    Dim clExport As Boolean = False
                    Dim clExportResizePoster As Integer = 0
                    Dim clExportTemplate As String = "template"
                    Dim clAsk As Boolean = False
                    Dim nowindow As Boolean = False
                    Dim RunModule As Boolean = False
                    Dim ModuleName As String = String.Empty
                    For i As Integer = 1 To Args.Count - 1

                        Select Case Args(i).ToLower
                            Case "-fullask"
                                clScrapeType = Enums.ScrapeType.FullAsk
                                clAsk = True
                            Case "-fullskip"
                                clScrapeType = Enums.ScrapeType.FullSkip
                                clAsk = False
                            Case "-fullauto"
                                clScrapeType = Enums.ScrapeType.FullAuto
                                clAsk = False
                            Case "-missask"
                                clScrapeType = Enums.ScrapeType.UpdateAsk
                                clAsk = True
                            Case "-missauto"
                                clScrapeType = Enums.ScrapeType.UpdateAuto
                                clAsk = False
                            Case "-newask"
                                clScrapeType = Enums.ScrapeType.NewAsk
                                clAsk = True
                            Case "-newauto"
                                clScrapeType = Enums.ScrapeType.NewAuto
                                clAsk = False
                            Case "-markask"
                                clScrapeType = Enums.ScrapeType.MarkAsk
                                clAsk = True
                            Case "-markauto"
                                clScrapeType = Enums.ScrapeType.MarkAuto
                                clAsk = False
                            Case "-file"
                                If Args.Count - 1 > i Then
                                    isSingle = False
                                    hasSpec = True
                                    clScrapeType = Enums.ScrapeType.SingleScrape
                                    If File.Exists(Args(i + 1).Replace("""", String.Empty)) Then
                                        MoviePath = Args(i + 1).Replace("""", String.Empty)
                                        i += 1
                                    End If
                                Else
                                    Exit For
                                End If
                            Case "-folder"
                                If Args.Count - 1 > i Then
                                    isSingle = True
                                    hasSpec = True
                                    clScrapeType = Enums.ScrapeType.SingleScrape
                                    If File.Exists(Args(i + 1).Replace("""", String.Empty)) Then
                                        MoviePath = Args(i + 1).Replace("""", String.Empty)
                                        i += 1
                                    End If
                                Else
                                    Exit For
                                End If
                            Case "-export"
                                If Args.Count - 1 > i Then
                                    MoviePath = Args(i + 1).Replace("""", String.Empty)
                                    clExport = True
                                Else
                                    Exit For
                                End If
                            Case "-template"
                                If Args.Count - 1 > i Then
                                    clExportTemplate = Args(i + 1).Replace("""", String.Empty)
                                Else
                                    Exit For
                                End If
                            Case "-resize"
                                If Args.Count - 1 > i Then
                                    clExportResizePoster = Convert.ToUInt16(Args(i + 1).Replace("""", String.Empty))
                                Else
                                    Exit For
                                End If
                            Case "-all"
                                Functions.SetScraperMod(Enums.ModType.All, True)
                            Case "-nfo"
                                Functions.SetScraperMod(Enums.ModType.NFO, True)
                            Case "-posters"
                                Functions.SetScraperMod(Enums.ModType.Poster, True)
                            Case "-fanart"
                                Functions.SetScraperMod(Enums.ModType.Fanart, True)
                            Case "-extra"
                                Functions.SetScraperMod(Enums.ModType.Extra, True)
                            Case "--verbose"
                                clAsk = True
                            Case "-nowindow"
                                nowindow = True
                            Case "-run"
                                If Args.Count - 1 > i Then
                                    ModuleName = Args(i + 1).Replace("""", String.Empty)
                                    RunModule = True
                                Else
                                    Exit For
                                End If
                            Case Else
                                'If File.Exists(Args(2).Replace("""", String.Empty)) Then
                                'MoviePath = Args(2).Replace("""", String.Empty)
                                'End If
                        End Select
                    Next
                    If nowindow Then fLoading.Hide()
                    APIXML.CacheXMLs()
					'fLoading.SetLoadingMesg("Loading database...")
					fLoading.SetLoadingMesg(Master.eLang.GetString(858, "Loading database..."))
					If Master.DB.CheckEssentials() Then
						Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
					End If
                    Master.DB.LoadMovieSourcesFromDB()
                    Master.DB.LoadTVSourcesFromDB()
                    If RunModule Then
                        fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
						'fLoading.SetLoadingMesg("Running Module...")
						fLoading.SetLoadingMesg(Master.eLang.GetString(859, "Running Module..."))
                        Dim gModule As ModulesManager._externalGenericModuleClass = ModulesManager.Instance.externalProcessorModules.FirstOrDefault(Function(y) y.ProcessorModule.ModuleName = ModuleName)
                        If Not IsNothing(gModule) Then
                            gModule.ProcessorModule.RunGeneric(Enums.ModuleEventType.CommandLine, Nothing, Nothing)
                        End If
                    End If
                    If clExport = True Then
                        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.CommandLine, New List(Of Object)(New Object() {MoviePath, clExportTemplate, clExportResizePoster}))
                        'dlgExportMovies.CLExport(MoviePath, clExportTemplate, clExportResizePoster)
                    End If

                    If Not IsNothing(clScrapeType) Then
                        Me.cmnuTrayIconExit.Enabled = True
                        Me.cmnuTrayIcon.Enabled = True
                        If Functions.HasModifier AndAlso Not clScrapeType = Enums.ScrapeType.SingleScrape Then
                            Try
                                fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
								'fLoading.SetLoadingMesg("Loading Media...")
								fLoading.SetLoadingMesg(Master.eLang.GetString(860, "Loading Media..."))
								LoadMedia(New Structures.Scans With {.Movies = True})
                                While Not Me.LoadingDone
                                    Application.DoEvents()
                                    Threading.Thread.Sleep(50)
                                End While
                                fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
								'fLoading.SetLoadingMesg("Command Line Scraping...")
								fLoading.SetLoadingMesg(Master.eLang.GetString(861, "Command Line Scraping..."))
                                MovieScrapeData(False, clScrapeType, Master.DefaultOptions)
                            Catch ex As Exception
                                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                            End Try
                        Else
                            Try
                                If Not String.IsNullOrEmpty(MoviePath) AndAlso hasSpec Then
                                    Master.currMovie = Master.DB.LoadMovieFromDB(MoviePath)
                                    Dim tmpTitle As String = String.Empty
                                    If FileUtils.Common.isVideoTS(MoviePath) Then
                                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(MoviePath).FullName).Name, False)
                                    ElseIf FileUtils.Common.isBDRip(MoviePath) Then
                                        tmpTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(MoviePath).FullName).FullName).Name, False)
                                    Else
                                        tmpTitle = StringUtils.FilterName(If(isSingle, Directory.GetParent(MoviePath).Name, Path.GetFileNameWithoutExtension(MoviePath)))
                                    End If
                                    If IsNothing(Master.currMovie.Movie) Then
                                        Master.currMovie.Movie = New MediaContainers.Movie
                                        Master.currMovie.Movie.Title = tmpTitle
                                        Dim sFile As New Scanner.MovieContainer
                                        sFile.Filename = MoviePath
                                        sFile.isSingle = isSingle
                                        sFile.UseFolder = If(isSingle, True, False)
                                        fScanner.GetMovieFolderContents(sFile)
                                        If Not String.IsNullOrEmpty(sFile.Nfo) Then
                                            Master.currMovie.Movie = NFO.LoadMovieFromNFO(sFile.Nfo, sFile.isSingle)
                                        Else
                                            Master.currMovie.Movie = NFO.LoadMovieFromNFO(sFile.Filename, sFile.isSingle)
                                        End If
                                        If String.IsNullOrEmpty(Master.currMovie.Movie.Title) Then
                                            'no title so assume it's an invalid nfo, clear nfo path if exists
                                            sFile.Nfo = String.Empty
                                            If FileUtils.Common.isVideoTS(sFile.Filename) Then
                                                Master.currMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(sFile.Filename).FullName).Name)
                                            ElseIf FileUtils.Common.isBDRip(sFile.Filename) Then
                                                Master.currMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(sFile.Filename).FullName).FullName).Name)
                                            Else
                                                If sFile.UseFolder AndAlso sFile.isSingle Then
                                                    Master.currMovie.ListTitle = StringUtils.FilterName(Directory.GetParent(sFile.Filename).Name)
                                                Else
                                                    Master.currMovie.ListTitle = StringUtils.FilterName(Path.GetFileNameWithoutExtension(sFile.Filename))
                                                End If
                                            End If
                                            If String.IsNullOrEmpty(Master.currMovie.Movie.SortTitle) Then Master.currMovie.Movie.SortTitle = Master.currMovie.ListTitle
                                        Else
                                            Dim tTitle As String = StringUtils.FilterTokens(Master.currMovie.Movie.Title)
                                            If String.IsNullOrEmpty(Master.currMovie.Movie.SortTitle) Then Master.currMovie.Movie.SortTitle = tTitle
                                            If Master.eSettings.DisplayYear AndAlso Not String.IsNullOrEmpty(Master.currMovie.Movie.Year) Then
                                                Master.currMovie.ListTitle = String.Format("{0} ({1})", tTitle, Master.currMovie.Movie.Year)
                                            Else
                                                Master.currMovie.ListTitle = tTitle
                                            End If
                                        End If

                                        If Not String.IsNullOrEmpty(Master.currMovie.ListTitle) Then
                                            Master.currMovie.NfoPath = sFile.Nfo
                                            Master.currMovie.PosterPath = sFile.Poster
                                            Master.currMovie.FanartPath = sFile.Fanart
                                            Master.currMovie.TrailerPath = sFile.Trailer
                                            Master.currMovie.SubPath = sFile.Subs
                                            Master.currMovie.ExtraPath = sFile.Extra
                                            Master.currMovie.Filename = sFile.Filename
                                            Master.currMovie.isSingle = sFile.isSingle
                                            Master.currMovie.UseFolder = sFile.UseFolder
                                            Master.currMovie.Source = sFile.Source
                                        End If
                                        Master.tmpMovie = Master.currMovie.Movie
                                    End If
									fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
									'fLoading.SetLoadingMesg("Command Line Scraping...")
									fLoading.SetLoadingMesg(Master.eLang.GetString(861, "Command Line Scraping..."))
									MovieScrapeData(False, Enums.ScrapeType.SingleScrape, Master.DefaultOptions)
                                Else
                                    Me.ScraperDone = True
                                End If
                            Catch ex As Exception
                                Me.ScraperDone = True
                                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                            End Try
                        End If

                        While Not Me.ScraperDone
                            Application.DoEvents()
                            Threading.Thread.Sleep(50)
                        End While
                    End If

                    frmSplash.Close()
                    Me.Close()
                Catch ex As Exception
                End Try

            Else 'Regular Run (GUI)
                Try
                    'If Master.eSettings.CheckUpdates Then
                    '    If Functions.CheckNeedUpdate() Then
                    '        Using dNewVer As New dlgNewVersion
                    '            fLoading.Hide()
                    '            If dNewVer.ShowDialog() = DialogResult.Abort Then
                    '                tmrAppExit.Enabled = True
                    '                CloseApp = True
                    '            End If
                    '        End Using
                    '    End If

                    'End If
                    If Not CloseApp Then
						'fLoading.SetLoadingMesg("Loading translations...")
						fLoading.SetLoadingMesg(Master.eLang.GetString(862, "Loading translations..."))
						APIXML.CacheXMLs()

                        Me.SetUp(True)
                        Me.cbSearch.SelectedIndex = 0

						'fLoading.SetLoadingMesg("Positioning controls...")
						fLoading.SetLoadingMesg(Master.eLang.GetString(863, "Positioning controls..."))
						Me.Location = Master.eSettings.WindowLoc
                        Me.Size = Master.eSettings.WindowSize
                        Me.WindowState = Master.eSettings.WindowState

                        Me.aniType = Master.eSettings.InfoPanelState
                        Select Case Me.aniType
                            Case 0
                                Me.pnlInfoPanel.Height = 25
                                Me.btnDown.Enabled = False
                                Me.btnMid.Enabled = True
                                Me.btnUp.Enabled = True
                            Case 1
                                Me.pnlInfoPanel.Height = Me.IPMid
                                Me.btnMid.Enabled = False
                                Me.btnDown.Enabled = True
                                Me.btnUp.Enabled = True
                            Case 2
                                Me.pnlInfoPanel.Height = Me.IPUp
                                Me.btnUp.Enabled = False
                                Me.btnDown.Enabled = True
                                Me.btnMid.Enabled = True
                        End Select

                        Me.aniShowType = Master.eSettings.ShowInfoPanelState

                        Me.aniFilterRaise = Master.eSettings.FilterPanelState
                        If Me.aniFilterRaise Then
                            Me.pnlFilter.Height = Functions.Quantize(Me.gbSpecific.Height + Me.lblFilter.Height + 15, 5)
                            Me.btnFilterDown.Enabled = True
                            Me.btnFilterUp.Enabled = False
                        Else
                            Me.pnlFilter.Height = 25
                            Me.btnFilterDown.Enabled = False
                            Me.btnFilterUp.Enabled = True
                        End If
                        Try ' On error just ignore this a let it use default
                            Me.scMain.SplitterDistance = Master.eSettings.SplitterPanelState
                            Me.scTV.SplitterDistance = Master.eSettings.ShowSplitterPanelState
                            Me.SplitContainer2.SplitterDistance = Master.eSettings.SeasonSplitterPanelState
                        Catch ex As Exception
                        End Try
                        Me.pnlFilter.Visible = True

                        Me.ClearInfo()

                        Application.DoEvents()
						'fLoading.SetLoadingMesg("Loading database...")
						fLoading.SetLoadingMesg(Master.eLang.GetString(858, "Loading database..."))
						If Master.eSettings.Version = String.Format("r{0}", My.Application.Info.Version.Revision) Then
							If Master.DB.CheckEssentials() Then
								Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
							End If
							Me.FillList(0)
							Me.Visible = True
						Else
							If Master.DB.CheckEssentials() Then
								Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
							End If
							If dlgWizard.ShowDialog = DialogResult.OK Then
								Application.DoEvents()
								Me.SetUp(False)	'just in case user changed languages
								Me.Visible = True
								Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
							Else
								Me.FillList(0)
								Me.Visible = True
							End If
						End If

                        Master.DB.LoadMovieSourcesFromDB()
                        Master.DB.LoadTVSourcesFromDB()
						'fLoading.SetLoadingMesg("Setting menus...")
						fLoading.SetLoadingMesg(Master.eLang.GetString(864, "Setting menus..."))

                        Me.SetMenus(True)
                        Functions.GetListOfSources()
                        Me.cmnuTrayIconExit.Enabled = True
                        Me.cmnuTrayIconSettings.Enabled = True
                        Me.EditToolStripMenuItem.Enabled = True
                        If tsbMediaCenters.DropDownItems.Count > 0 Then tsbMediaCenters.Enabled = True
                    End If
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try

            End If

            fLoading.Close()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub frmMain_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        Try
            If Me.Created Then
                Me.MoveMPAA()
                Me.MoveGenres()
                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)
                Me.pnlNoInfo.Location = New Point(Convert.ToInt32((Me.scMain.Panel2.Width - Me.pnlNoInfo.Width) / 2), Convert.ToInt32((Me.scMain.Panel2.Height - Me.pnlNoInfo.Height) / 2))
                Me.pnlCancel.Location = New Point(Convert.ToInt32((Me.scMain.Panel2.Width - Me.pnlNoInfo.Width) / 2), 100)
                Me.pnlFilterGenre.Location = New Point(Me.gbSpecific.Left + Me.txtFilterGenre.Left, (Me.pnlFilter.Top + Me.txtFilterGenre.Top + Me.gbSpecific.Top) - Me.pnlFilterGenre.Height)
                Me.pnlFilterSource.Location = New Point(Me.gbSpecific.Left + Me.txtFilterSource.Left, (Me.pnlFilter.Top + Me.txtFilterSource.Top + Me.gbSpecific.Top) - Me.pnlFilterSource.Height)
                Me.pnlLoadingSettings.Location = New Point(Convert.ToInt32((Me.Width - Me.pnlLoadingSettings.Width) / 2), Convert.ToInt32((Me.Height - Me.pnlLoadingSettings.Height) / 2))
                Me.pnlAllSeason.Location = New Point(Me.pbFanart.Width - Me.pnlAllSeason.Width - 9, 112)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        If Not CloseApp Then
            Me.BringToFront()
            Me.Activate()
            Me.cmnuTrayIcon.Enabled = True
            If Not Functions.CheckIfWindows Then Mono_Shown()
        End If
    End Sub

    Private Sub GenericRunCallBack(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        Select Case mType
            Case Enums.ModuleEventType.Generic
                Select Case _params(0).ToString
                    Case "controlsenabled"
                        Me.SetControlsEnabled(Convert.ToBoolean(_params(1)), If(_params.Count = 3, Convert.ToBoolean(_params(2)), False))
                End Select
            Case Enums.ModuleEventType.Notification
                Select Case _params(0).ToString
                    Case "error"
                        dlgErrorViewer.Show(Me)
                    Case Else
                        Me.Activate()
                End Select
            Case Enums.ModuleEventType.RenameMovie
                Try
                    Me.SetListItemAfterEdit(Convert.ToInt16(_params(0)), Convert.ToInt16(_params(1)))
                    If Me.RefreshMovie(Convert.ToInt16(_params(0))) Then
                        Me.FillList(0)
                    End If
                    Me.SetStatus(Master.currMovie.Filename)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Case Enums.ModuleEventType.RenameMovieManual
                Try
                    Me.SetListItemAfterEdit(Convert.ToInt16(_params(0)), Convert.ToInt16(_params(1)))
                    If Me.RefreshMovie(Convert.ToInt16(_params(0))) Then
                        Me.FillList(0)
                    End If
                    Me.SetStatus(Master.currMovie.Filename)
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
        End Select
    End Sub

    Private Sub GenreListToolStripComboBox_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles GenreListToolStripComboBox.DropDown
        Me.GenreListToolStripComboBox.Items.Remove(Master.eLang.GetString(98, "Select Genre..."))
    End Sub

    Private Sub GenreListToolStripComboBox_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles GenreListToolStripComboBox.SelectedIndexChanged
        If dgvMediaList.SelectedRows.Count > 1 Then
            RemoveGenreToolStripMenuItem.Enabled = True
            AddGenreToolStripMenuItem.Enabled = True
        Else
            RemoveGenreToolStripMenuItem.Enabled = GenreListToolStripComboBox.Tag.ToString.Contains(GenreListToolStripComboBox.Text)
            AddGenreToolStripMenuItem.Enabled = Not GenreListToolStripComboBox.Tag.ToString.Contains(GenreListToolStripComboBox.Text)
        End If
        SetGenreToolStripMenuItem.Enabled = True
    End Sub

    Private Sub lblGFilClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblGFilClose.Click
        Me.txtFilterGenre.Focus()
        Me.pnlFilterGenre.Tag = String.Empty
    End Sub

    Private Sub lblSFilClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblSFilClose.Click
        Me.txtFilterSource.Focus()
        Me.pnlFilterSource.Tag = String.Empty
    End Sub

    Private Sub LoadEpisodeInfo(ByVal ID As Integer)
        Try
            Me.dgvTVEpisodes.SuspendLayout()
            Me.SetControlsEnabled(False, True)
            Me.ShowNoInfo(False)

            If Not Me.currThemeType = Theming.ThemeType.Episode Then Me.ApplyTheme(Theming.ThemeType.Episode)

            Me.ClearInfo(False)

            Me.bwLoadEpInfo = New System.ComponentModel.BackgroundWorker
            Me.bwLoadEpInfo.WorkerSupportsCancellation = True
            Me.bwLoadEpInfo.RunWorkerAsync(New Arguments With {.ID = ID})

        Catch ex As Exception
            Me.SetControlsEnabled(True)
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub LoadInfo(ByVal ID As Integer, ByVal sPath As String, ByVal doInfo As Boolean, ByVal doMI As Boolean, Optional ByVal setEnabled As Boolean = False)
        Try
            Me.dgvMediaList.SuspendLayout()
            Me.SetControlsEnabled(False, True)
            Me.ShowNoInfo(False)

            If doMI Then
                If Me.bwMediaInfo.IsBusy Then Me.bwMediaInfo.CancelAsync()

                Me.txtMetaData.Clear()
                Me.pbMILoading.Visible = True

                Me.bwMediaInfo = New System.ComponentModel.BackgroundWorker
                Me.bwMediaInfo.WorkerSupportsCancellation = True
                Me.bwMediaInfo.RunWorkerAsync(New Arguments With {.setEnabled = setEnabled, .Path = sPath, .Movie = Master.currMovie})
            End If

            If doInfo Then
                Me.ClearInfo()

                Me.bwLoadInfo = New System.ComponentModel.BackgroundWorker
                Me.bwLoadInfo.WorkerSupportsCancellation = True
                Me.bwLoadInfo.RunWorkerAsync(New Arguments With {.ID = ID})
            End If
        Catch ex As Exception
            Me.SetControlsEnabled(True)
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub LoadSeasonInfo(ByVal ShowID As Integer, ByVal Season As Integer)
        Try
            Me.dgvTVSeasons.SuspendLayout()
            Me.SetControlsEnabled(False, True)
            Me.ShowNoInfo(False)

            If Not Me.currThemeType = Theming.ThemeType.Show Then
                Me.ApplyTheme(Theming.ThemeType.Show)
            End If

            Me.ClearInfo(False)

            Me.bwLoadSeasonInfo = New System.ComponentModel.BackgroundWorker
            Me.bwLoadSeasonInfo.WorkerSupportsCancellation = True
            Me.bwLoadSeasonInfo.RunWorkerAsync(New Arguments With {.ID = ShowID, .Season = Season})

            Me.FillEpisodes(ShowID, Season)

        Catch ex As Exception
            Me.SetControlsEnabled(True)
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub LoadShowInfo(ByVal ID As Integer)
        Try
            Me.dgvTVShows.SuspendLayout()
            Me.SetControlsEnabled(False, True)
            Me.ShowNoInfo(False)

            If Not Me.currThemeType = Theming.ThemeType.Show Then Me.ApplyTheme(Theming.ThemeType.Show)

            Me.ClearInfo()

            Me.bwLoadShowInfo = New System.ComponentModel.BackgroundWorker
            Me.bwLoadShowInfo.WorkerSupportsCancellation = True
            Me.bwLoadShowInfo.RunWorkerAsync(New Arguments With {.ID = ID})

            Me.FillSeasons(ID)

        Catch ex As Exception
            Me.SetControlsEnabled(True)
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub lstActors_SelectedValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstActors.SelectedValueChanged
        '//
        ' Begin thread to download actor image if one exists
        '\\
        Try
            If Me.lstActors.Items.Count > 0 AndAlso Me.lstActors.SelectedItems.Count > 0 AndAlso Not IsNothing(Me.alActors.Item(Me.lstActors.SelectedIndex)) AndAlso Not Me.alActors.Item(Me.lstActors.SelectedIndex).ToString = "none" Then

                If Not IsNothing(Me.pbActors.Image) Then
                    Me.pbActors.Image.Dispose()
                    Me.pbActors.Image = Nothing
                End If

                Me.pbActLoad.Visible = True

                If Me.bwDownloadPic.IsBusy Then
                    Me.bwDownloadPic.CancelAsync()
                    While Me.bwDownloadPic.IsBusy
                        Application.DoEvents()
                        Threading.Thread.Sleep(50)
                    End While
                End If

                Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                Me.bwDownloadPic.WorkerSupportsCancellation = True
                Me.bwDownloadPic.RunWorkerAsync(New Arguments With {.pURL = Me.alActors.Item(Me.lstActors.SelectedIndex).ToString})

            Else
                Me.pbActors.Image = My.Resources.actor_silhouette
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub mnuAllAskAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskAll.Click, mnuTrayAllAskAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllSkipAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllSkipAll.Click ', mnuTrayAllSkipAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullSkip, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskExtra.Click, mnuTrayAllAskExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskFanart.Click, mnuTrayAllAskFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskNfo.Click, mnuTrayAllAskNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskPoster.Click, mnuTrayAllAskPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAskTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAskTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoAll.Click, mnuTrayAllAutoAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoExtra.Click, mnuTrayAllAutoExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoFanart.Click, mnuTrayAllAutoFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoNfo.Click, mnuTrayAllAutoNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoPoster.Click, mnuTrayAllAutoPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuAllAutoTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAllAutoTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskAll.Click, mnuTrayFilterAskAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskExtra.Click, mnuTrayFilterAskExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskFanart.Click, mnuTrayFilterAskFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskMI.Click, mnuTrayFilterAskMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskNfo.Click, mnuTrayFilterAskNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskPoster.Click, mnuTrayFilterAskPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAskTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAskTrailer.Click, mnuTrayFilterAskTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoAll.Click, mnuTrayFilterAutoAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoExtra.Click, mnuTrayFilterAutoExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoFanart.Click, mnuTrayFilterAutoFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoMI.Click, mnuTrayFilterAutoMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoNfo.Click, mnuTrayFilterAutoNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoPoster.Click, mnuTrayFilterAutoPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuFilterAutoTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilterAutoTrailer.Click, mnuTrayFilterAutoTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.FilterAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskAll.Click, mnuTrayMarkAskAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskExtra.Click, mnuTrayMarkAskExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskFanart.Click, mnuTrayMarkAskFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskNfo.Click, mnuTrayMarkAskNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskPoster.Click, mnuTrayMarkAskPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAskTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAskTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoAll.Click, mnuTrayMarkAutoAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        'Me.ScrapeData(Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoExtra.Click, mnuTrayMarkAutoExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoFanart.Click, mnuTrayMarkAutoFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoNfo.Click, mnuTrayMarkAutoNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        'Me.ScrapeData(Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoPoster.Click, mnuTrayMarkAutoPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMarkAutoTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMarkAutoTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.MarkAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskAll.Click, mnuTrayMissAskAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskExtra.Click, mnuTrayMissAskExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskFanart.Click, mnuTrayMissAskFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskNfo.Click, mnuTrayMissAskNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskPoster.Click, mnuTrayMissAskPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAskTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAskTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoAll.Click, mnuTrayMissAutoAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoExtra.Click, mnuTrayMissAutoExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoFanart.Click, mnuTrayMissAutoFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoNfo.Click, mnuTrayMissAutoNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoPoster.Click, mnuTrayMissAutoPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuMissAutoTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuMissAutoTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.UpdateAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskAll.Click, mnuTrayNewAskAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskExtra.Click, mnuTrayNewAskExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        'Me.ScrapeData(Enums.ScrapeType.NewAsk, Master.DefaultOptions)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskFanart.Click, mnuTrayNewAskFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskNfo.Click, mnuTrayNewAskNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskPoster.Click, mnuTrayNewAskPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAskTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAskTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAsk, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoAll.Click, mnuTrayNewAutoAll.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoExtra_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoExtra.Click, mnuTrayNewAutoExtra.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoFanart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoFanart.Click, mnuTrayNewAutoFanart.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoMI.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoNfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoNfo.Click, mnuTrayNewAutoNfo.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoPoster_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoPoster.Click, mnuTrayNewAutoPoster.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub mnuNewAutoTrailer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNewAutoTrailer.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        Me.MovieScrapeData(False, Enums.ScrapeType.NewAuto, Master.DefaultOptions)
    End Sub

    Private Sub Mono_Shown()
        Me.pnlNoInfo.Location = New Point(Convert.ToInt32((Me.scMain.Panel2.Width - Me.pnlNoInfo.Width) / 2), Convert.ToInt32((Me.scMain.Panel2.Height - Me.pnlNoInfo.Height) / 2))
    End Sub

    Private Sub MoveGenres()
        '//
        ' Slide the genre images along with the panel and move with form resizing
        '\\

        Try
            For i As Integer = 0 To UBound(Me.pnlGenre)
                Me.pnlGenre(i).Left = ((Me.pnlInfoPanel.Right) - (i * 73)) - 73
                Me.pnlGenre(i).Top = Me.pnlInfoPanel.Top - 105
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub MoveMPAA()
        '//
        ' Slide the MPAA image along with the panel
        '\\

        Me.pnlMPAA.BringToFront()
        Me.pnlMPAA.Size = New Size(Me.pbMPAA.Width + 10, Me.pbMPAA.Height + 10)
        Me.pbMPAA.Location = New Point(4, 4)
        Me.pnlMPAA.Top = Me.pnlInfoPanel.Top - (Me.pnlMPAA.Height + 10)
    End Sub

    Private Sub MovieInfoDownloaded()
        Try
            If Not String.IsNullOrEmpty(Master.tmpMovie.Title) Then
                Master.currMovie.Movie = Master.tmpMovie
                If Master.eSettings.ScanMediaInfo Then
                    Me.tslLoading.Text = Master.eLang.GetString(140, "Scanning Meta Data:")
                    Me.tspbLoading.Value = Me.tspbLoading.Maximum
                    Me.tspbLoading.Style = ProgressBarStyle.Marquee
                    Application.DoEvents()
                    MediaInfo.UpdateMediaInfo(Master.currMovie)
                End If

                If Master.eSettings.SingleScrapeImages Then
                    Dim tmpImages As New Images
                    Dim AllowFA As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Fanart) AndAlso tmpImages.IsAllowedToDownload(Master.currMovie, Enums.ImageType.Fanart, True)

                    If ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Poster) AndAlso tmpImages.IsAllowedToDownload(Master.currMovie, Enums.ImageType.Posters, True) Then
                        Me.tslLoading.Text = Master.eLang.GetString(572, "Scraping Posters:")
                        Application.DoEvents()
                        Dim pResults As New Containers.ImgResult
                        ModulesManager.Instance.ScraperSelectImageOfType(Master.currMovie, Enums.ImageType.Posters, pResults, True, AllowFA)
                        If Not String.IsNullOrEmpty(pResults.ImagePath) Then
                            Master.currMovie.PosterPath = pResults.ImagePath
                            If Not Master.eSettings.NoSaveImagesToNfo AndAlso pResults.Posters.Count > 0 Then Master.currMovie.Movie.Thumb = pResults.Posters
                        End If
                        pResults = Nothing
                    End If

                    If AllowFA Then
                        Me.tslLoading.Text = Master.eLang.GetString(573, "Scraping Fanart:")
                        Application.DoEvents()
                        Dim fResults As New Containers.ImgResult
                        ModulesManager.Instance.ScraperSelectImageOfType(Master.currMovie, Enums.ImageType.Fanart, fResults, True, True)
                        If Not String.IsNullOrEmpty(fResults.ImagePath) Then
                            Master.currMovie.FanartPath = fResults.ImagePath
                            If Not Master.eSettings.NoSaveImagesToNfo AndAlso fResults.Fanart.Thumb.Count > 0 Then Master.currMovie.Movie.Fanart = fResults.Fanart
                        End If
                        fResults = Nothing
                    End If

                    tmpImages.Dispose()
                    tmpImages = Nothing
                End If

                If Master.eSettings.SingleScrapeTrailer AndAlso ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Trailer) Then
                    Me.tslLoading.Text = Master.eLang.GetString(574, "Scraping Trailers:")
                    Application.DoEvents()
                    Dim tURL As String = ModulesManager.Instance.ScraperDownloadTrailer(Master.currMovie)
                    ' If Not String.IsNullOrEmpty(tURL) AndAlso tURL.Contains("://") Then
                    If Not String.IsNullOrEmpty(tURL) AndAlso tURL.Substring(0, 7) = "http://" Then
                        Master.currMovie.Movie.Trailer = tURL
                    ElseIf Not String.IsNullOrEmpty(tURL) AndAlso tURL.Substring(0, 9) = "plugin://" Then
                        Master.currMovie.Movie.Trailer = tURL
                    End If
                End If

                If Master.eSettings.AutoThumbs > 0 AndAlso Master.currMovie.isSingle Then
                    Me.tslLoading.Text = Master.eLang.GetString(575, "Generating Extrathumbs:")
                    Application.DoEvents()
                    'ThumbGenerator.CreateRandomThumbs(Master.currMovie, Master.eSettings.AutoThumbs, True)
                    Dim params As New List(Of Object)(New Object() {Master.currMovie, Master.eSettings.AutoThumbs, True, ""})
                    ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.RandomFrameExtrator, params, Nothing, True)
                    MovieScraperEvent(Enums.MovieScraperEventType.ThumbsItem, True)
                    Dim ETasFA As String = DirectCast(params(3), String)
                    If Not String.IsNullOrEmpty(ETasFA) Then
                        Master.currMovie.ExtraPath = "TRUE"
                        If Not ETasFA = "TRUE" Then
                            Master.currMovie.FanartPath = ETasFA
                        End If
                    End If
                End If
                Me.tslLoading.Text = Master.eLang.GetString(568, "Generating Actors Cache:")
                If Master.GlobalScrapeMod.Actors AndAlso AdvancedSettings.GetBooleanSetting("ScrapeActorsThumbs", False) Then
                    For Each act As MediaContainers.Person In Master.currMovie.Movie.Actors
                        Dim img As New Images
                        img.FromWeb(act.Thumb)
                        img.SaveAsActorThumb(act, Directory.GetParent(Master.currMovie.Filename).FullName)
                    Next
                End If

                Dim indX As Integer = Me.dgvMediaList.SelectedRows(0).Index
                Dim ID As Integer = Convert.ToInt32(Me.dgvMediaList.Item(0, indX).Value)

                Me.tslLoading.Text = Master.eLang.GetString(576, "Verifying Movie Details:")
                Application.DoEvents()

                Using dEditMovie As New dlgEditMovie(Me)
                    AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                    Select Case dEditMovie.ShowDialog()
                        Case DialogResult.OK
                            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieScraperRDYtoSave, Nothing, Master.currMovie)
                            Me.SetListItemAfterEdit(ID, indX)
                            If Me.RefreshMovie(ID) Then
                                Me.FillList(0)
                            End If
                            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.MovieSync, Nothing, Master.currMovie)
                        Case DialogResult.Retry
                            Master.currMovie.ClearExtras = False
                            Functions.SetScraperMod(Enums.ModType.All, True, True)
                            Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions) ', ID)
                        Case DialogResult.Abort
                            Master.currMovie.ClearExtras = False
                            Functions.SetScraperMod(Enums.ModType.DoSearch, True)
                            Functions.SetScraperMod(Enums.ModType.All, True, False)
                            Me.MovieScrapeData(True, Enums.ScrapeType.SingleScrape, Master.DefaultOptions) ', ID, True)
                        Case Else
                            If Me.InfoCleared Then Me.LoadInfo(ID, Me.dgvMediaList.Item(1, indX).Value.ToString, True, False)
                    End Select
                    RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditMovie.GenericRunCallBack
                End Using

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Master.currMovie.ClearExtras = False

        Me.pnlCancel.Visible = False
        Me.tslLoading.Visible = False
        Me.tspbLoading.Visible = False
        Me.SetStatus(String.Empty)
        Me.SetControlsEnabled(True)
        Me.EnableFilters(True)
    End Sub

    Private Sub MovieInfoDownloadedPercent(ByVal iPercent As Integer)
        If Me.ReportDownloadPercent = True Then
            Me.tspbLoading.Value = iPercent
            Me.Refresh()
        End If
    End Sub

    Private Sub MovieScrapeData(ByVal selected As Boolean, ByVal sType As Enums.ScrapeType, ByVal Options As Structures.ScrapeOptions)
        ScrapeList.Clear()
        If selected Then
            'create snapshoot list of selected movies
            For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                ScrapeList.Add(DirectCast(sRow.DataBoundItem, DataRowView).Row)
            Next
        Else
            Dim PosterAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Poster)
            Dim FanartAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Fanart)
            Dim TrailerAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Trailer)

            'create list of movies acording to scrapetype
            For Each drvRow As DataRow In Me.dtMedia.Rows

                If Convert.ToBoolean(drvRow.Item(14)) Then Continue For

                Select Case sType
                    Case Enums.ScrapeType.NewAsk, Enums.ScrapeType.NewAuto
                        If Not Convert.ToBoolean(drvRow.Item(10)) Then Continue For
                    Case Enums.ScrapeType.MarkAsk, Enums.ScrapeType.MarkAuto
                        If Not Convert.ToBoolean(drvRow.Item(11)) Then Continue For
                    Case Enums.ScrapeType.FilterAsk, Enums.ScrapeType.FilterAuto
                        Dim index As Integer = Me.bsMedia.Find("id", drvRow.Item(0))
                        If Not index >= 0 Then Continue For
                    Case Enums.ScrapeType.UpdateAsk, Enums.ScrapeType.UpdateAuto
                        If Not ((Master.GlobalScrapeMod.Poster AndAlso Master.eSettings.MissingFilterPoster AndAlso PosterAllowed AndAlso Not Convert.ToBoolean(drvRow.Item(4))) OrElse _
                                (Master.GlobalScrapeMod.Fanart AndAlso Master.eSettings.MissingFilterFanart AndAlso FanartAllowed AndAlso Not Convert.ToBoolean(drvRow.Item(5))) OrElse _
                                (Master.GlobalScrapeMod.NFO AndAlso Master.eSettings.MissingFilterNFO AndAlso Not Convert.ToBoolean(drvRow.Item(6))) OrElse _
                                (Master.GlobalScrapeMod.Trailer AndAlso Master.eSettings.MissingFilterTrailer AndAlso TrailerAllowed AndAlso Not Convert.ToBoolean(drvRow.Item(7))) OrElse _
                                (Master.GlobalScrapeMod.Extra AndAlso Master.eSettings.MissingFilterExtras AndAlso (Master.eSettings.AutoET OrElse Master.eSettings.AutoThumbs > 0) AndAlso Not Convert.ToBoolean(drvRow.Item(9)))) Then
                            Continue For
                        End If
                End Select

                ScrapeList.Add(drvRow)
            Next
        End If

        Me.SetControlsEnabled(False)

        Me.tspbLoading.Value = 0
        If ScrapeList.Count > 1 Then
            Me.tspbLoading.Style = ProgressBarStyle.Continuous
            Me.tspbLoading.Maximum = ScrapeList.Count
        Else
            Me.tspbLoading.Maximum = 100
            Me.tspbLoading.Style = ProgressBarStyle.Marquee
        End If

        Select Case sType
            Case Enums.ScrapeType.FullAsk
                Me.tslLoading.Text = Master.eLang.GetString(127, "Scraping Media (All Movies - Ask):")
            Case Enums.ScrapeType.FullSkip
                Me.tslLoading.Text = Master.eLang.GetString(853, "Scraping Media (All Movies - Skip):")
            Case Enums.ScrapeType.FullAuto
                Me.tslLoading.Text = Master.eLang.GetString(128, "Scraping Media (All Movies - Auto):")
            Case Enums.ScrapeType.UpdateAuto
                Me.tslLoading.Text = Master.eLang.GetString(132, "Scraping Media (Movies Missing Items - Auto):")
            Case Enums.ScrapeType.UpdateAsk
                Me.tslLoading.Text = Master.eLang.GetString(133, "Scraping Media (Movies Missing Items - Ask):")
            Case Enums.ScrapeType.NewAsk
                Me.tslLoading.Text = Master.eLang.GetString(134, "Scraping Media (New Movies - Ask):")
            Case Enums.ScrapeType.NewAuto
                Me.tslLoading.Text = Master.eLang.GetString(135, "Scraping Media (New Movies - Auto):")
            Case Enums.ScrapeType.MarkAsk
                Me.tslLoading.Text = Master.eLang.GetString(136, "Scraping Media (Marked Movies - Ask):")
            Case Enums.ScrapeType.MarkAuto
                Me.tslLoading.Text = Master.eLang.GetString(137, "Scraping Media (Marked Movies - Auto):")
            Case Enums.ScrapeType.FilterAsk
                Me.tslLoading.Text = Master.eLang.GetString(622, "Scraping Media (Current Filter - Ask):")
            Case Enums.ScrapeType.FilterAuto
                Me.tslLoading.Text = Master.eLang.GetString(623, "Scraping Media (Current Filter - Auto):")
            Case Enums.ScrapeType.SingleScrape
                Me.tslLoading.Text = Master.eLang.GetString(139, "Scraping:")
        End Select

        If Not sType = Enums.ScrapeType.SingleScrape Then
            Me.btnCancel.Text = Master.eLang.GetString(126, "Cancel Scraper")
            Me.lblCanceling.Text = Master.eLang.GetString(125, "Canceling Scraper...")
            Me.btnCancel.Visible = True
            Me.lblCanceling.Visible = False
            Me.pbCanceling.Visible = False
            Me.pnlCancel.Visible = True
        End If

        Me.tslLoading.Visible = True
        Me.tspbLoading.Visible = True
        Application.DoEvents()
        bwMovieScraper.WorkerSupportsCancellation = True
        bwMovieScraper.WorkerReportsProgress = True
        bwMovieScraper.RunWorkerAsync(New Arguments With {.scrapeType = sType, .Options = Options})
    End Sub

    Private Sub MovieScraperEvent(ByVal eType As Enums.MovieScraperEventType, ByVal Parameter As Object)
        If (Me.InvokeRequired) Then
            Me.Invoke(New DelegateEvent(AddressOf MovieScraperEvent), New Object() {eType, Parameter})
        Else
            Select Case eType
                Case Enums.MovieScraperEventType.PosterItem
                    dScrapeRow.Item(4) = DirectCast(Parameter, Boolean)
                Case Enums.MovieScraperEventType.FanartItem
                    dScrapeRow.Item(5) = DirectCast(Parameter, Boolean)
                Case Enums.MovieScraperEventType.NFOItem
                    dScrapeRow.Item(6) = DirectCast(Parameter, Boolean)
                Case Enums.MovieScraperEventType.TrailerItem
                    dScrapeRow.Item(7) = DirectCast(Parameter, Boolean)
                Case Enums.MovieScraperEventType.ThumbsItem
                    dScrapeRow.Item(9) = DirectCast(Parameter, Boolean)
                Case Enums.MovieScraperEventType.SortTitle
                    dScrapeRow.Item(47) = DirectCast(Parameter, String)
                Case Enums.MovieScraperEventType.ListTitle
                    dScrapeRow.Item(3) = DirectCast(Parameter, String)

            End Select
            Me.dgvMediaList.Invalidate()
        End If
    End Sub

    Function MyResolveEventHandler(ByVal sender As Object, ByVal args As ResolveEventArgs) As [Assembly]
        Dim name As String = args.Name.Split(Convert.ToChar(","))(0)
        Dim asm As Assembly = ModulesManager.AssemblyList.FirstOrDefault(Function(y) y.AssemblyName = name).Assembly
        If asm Is Nothing Then
            asm = ModulesManager.AssemblyList.FirstOrDefault(Function(y) y.AssemblyName = name.Split(Convert.ToChar("."))(0)).Assembly
        End If
        Return asm
    End Function

    Private Sub NonScrape(ByVal sType As Enums.ScrapeType, ByVal Options As Structures.ScrapeOptions)
        Me.Cursor = Cursors.WaitCursor

        Select Case sType
            Case Enums.ScrapeType.CleanFolders
                Me.btnCancel.Text = Master.eLang.GetString(120, "Cancel Cleaner")
                Me.lblCanceling.Text = Master.eLang.GetString(119, "Canceling File Cleaner...")
                Me.tslLoading.Text = Master.eLang.GetString(129, "Cleaning Files:")
            Case Enums.ScrapeType.CopyBD
                Me.btnCancel.Text = Master.eLang.GetString(122, "Cancel Copy")
                Me.lblCanceling.Text = Master.eLang.GetString(121, "Canceling Backdrop Copy...")
                Me.tslLoading.Text = Master.eLang.GetString(130, "Copying Fanart to Backdrops Folder:")
        End Select

        btnCancel.Visible = True
        lblCanceling.Visible = False
        pbCanceling.Visible = False
        Me.pnlCancel.Visible = True
        Me.tslLoading.Visible = True
        Me.tspbLoading.Value = 0
        Me.tspbLoading.Maximum = Me.dtMedia.Rows.Count
        Me.tspbLoading.Visible = True
        Me.SetControlsEnabled(False, True)
        Me.EnableFilters(False)

        bwNonScrape.WorkerReportsProgress = True
        bwNonScrape.WorkerSupportsCancellation = True
        bwNonScrape.RunWorkerAsync(New Arguments With {.scrapeType = sType, .Options = Options})
    End Sub

    Private Sub OpenContainingFolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenContainingFolderToolStripMenuItem.Click
        If Me.dgvMediaList.SelectedRows.Count > 0 Then
            Dim doOpen As Boolean = True
            If Me.dgvMediaList.SelectedRows.Count > 10 Then
                If Not MsgBox(String.Format(Master.eLang.GetString(635, "You have selected {0} folders to open. Are you sure you want to do this?"), Me.dgvMediaList.SelectedRows.Count), MsgBoxStyle.YesNo Or MsgBoxStyle.Question, Master.eLang.GetString(104, "Are You Sure?")) = MsgBoxResult.Yes Then doOpen = False
            End If

            If doOpen Then
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    Using Explorer As New Diagnostics.Process

                        If Master.isWindows Then
                            Explorer.StartInfo.FileName = "explorer.exe"
                            Explorer.StartInfo.Arguments = String.Format("/select,""{0}""", sRow.Cells(1).Value)
                        Else
                            Explorer.StartInfo.FileName = "xdg-open"
                            Explorer.StartInfo.Arguments = String.Format("""{0}""", Path.GetDirectoryName(sRow.Cells(1).Value.ToString))
                        End If
                        Explorer.Start()
                    End Using
                Next
            End If
        End If
    End Sub

    Private Sub OpenImageViewer(ByVal _Image As Image)
        Using dImgView As New dlgImgView
            dImgView.ShowDialog(_Image)
        End Using
    End Sub

    Private Sub pbFanart_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbFanart.DoubleClick
        '//
        ' Show the Fanart in the Image Viewer
        '\\

        Try
            If Not IsNothing(Me.pbFanartCache.Image) Then
                Using dImgView As New dlgImgView
                    dImgView.ShowDialog(Me.pbFanartCache.Image)
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbGenre_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        '//
        ' Draw genre text over the image when mouse hovers
        '\\

        Try
            If Master.eSettings.AllwaysDisplayGenresText Then Return
            Dim iLeft As Integer = 0
            Me.GenreImage = DirectCast(sender, PictureBox).Image
            Dim bmGenre As New Bitmap(Me.GenreImage)
            Dim grGenre As Graphics = Graphics.FromImage(bmGenre)
            Dim drawString As String = DirectCast(sender, PictureBox).Name.ToString
            Dim drawFont As New Font("Microsoft Sans Serif", 14, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim drawBrush As New SolidBrush(Color.White)
            Dim drawWidth As Single = grGenre.MeasureString(drawString, drawFont).Width
            Dim drawSize As Integer = Convert.ToInt32((14 * (bmGenre.Width / drawWidth)) - 0.5)
            drawFont = New Font("Microsoft Sans Serif", If(drawSize > 14, 14, drawSize), FontStyle.Bold, GraphicsUnit.Pixel)
            Dim drawHeight As Single = grGenre.MeasureString(drawString, drawFont).Height
            iLeft = Convert.ToInt32((bmGenre.Width - grGenre.MeasureString(drawString, drawFont).Width) / 2)
            grGenre.DrawString(drawString, drawFont, drawBrush, iLeft, (bmGenre.Height - drawHeight))
            DirectCast(sender, PictureBox).Image = bmGenre
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbGenre_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        '//
        ' Reset genre image when mouse leaves to "clear" the text
        '\\

        Try
            If Master.eSettings.AllwaysDisplayGenresText Then Return
            DirectCast(sender, PictureBox).Image = GenreImage
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbPoster_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbPoster.DoubleClick
        Try
            If Not IsNothing(Me.pbPoster.Image) Then
                Using dImgView As New dlgImgView
                    dImgView.ShowDialog(Me.pbPosterCache.Image)
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbAllSeason_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbAllSeason.DoubleClick
        Try
            If Not IsNothing(Me.pbAllSeason.Image) Then
                Using dImgView As New dlgImgView
                    dImgView.ShowDialog(Me.pbAllSeasonCache.Image)
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub rbFilterAnd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbFilterAnd.Click
        If clbFilterGenres.CheckedItems.Count > 0 Then
            Me.txtFilterGenre.Text = String.Empty
            Me.FilterArray.Remove(Me.filGenre)

            Dim alGenres As New List(Of String)
            alGenres.AddRange(clbFilterGenres.CheckedItems.OfType(Of String).ToList)

            Me.txtFilterGenre.Text = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " AND ")

            For i As Integer = 0 To alGenres.Count - 1
                alGenres.Item(i) = String.Format("Genre LIKE '%{0}%'", alGenres.Item(i))
            Next

            Me.filGenre = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " AND ")

            Me.FilterArray.Add(Me.filGenre)
        End If

        If (Not String.IsNullOrEmpty(Me.cbFilterYear.Text) AndAlso Not Me.cbFilterYear.Text = Master.eLang.All) OrElse Me.clbFilterGenres.CheckedItems.Count > 0 OrElse _
        Me.chkFilterMark.Checked OrElse Me.chkFilterNew.Checked OrElse Me.chkFilterLock.Checked OrElse Not Me.clbFilterSource.CheckedItems.Count > 0 OrElse _
        Me.chkFilterDupe.Checked OrElse Me.chkFilterMissing.Checked OrElse Me.chkFilterTolerance.Checked OrElse Not Me.cbFilterFileSource.Text = Master.eLang.All Then Me.RunFilter()
    End Sub

    Private Sub rbFilterOr_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbFilterOr.Click
        If clbFilterGenres.CheckedItems.Count > 0 Then
            Me.txtFilterGenre.Text = String.Empty
            Me.FilterArray.Remove(Me.filGenre)

            Dim alGenres As New List(Of String)
            alGenres.AddRange(clbFilterGenres.CheckedItems.OfType(Of String).ToList)

            Me.txtFilterGenre.Text = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " OR ")

            For i As Integer = 0 To alGenres.Count - 1
                alGenres.Item(i) = String.Format("Genre LIKE '%{0}%'", alGenres.Item(i))
            Next

            Me.filGenre = Microsoft.VisualBasic.Strings.Join(alGenres.ToArray, " OR ")

            Me.FilterArray.Add(Me.filGenre)
        End If

        If (Not String.IsNullOrEmpty(Me.cbFilterYear.Text) AndAlso Not Me.cbFilterYear.Text = Master.eLang.All) OrElse Me.clbFilterGenres.CheckedItems.Count > 0 OrElse _
        Me.chkFilterMark.Checked OrElse Me.chkFilterNew.Checked OrElse Me.chkFilterLock.Checked OrElse Not Me.clbFilterSource.CheckedItems.Count > 0 OrElse _
        Me.chkFilterDupe.Checked OrElse Me.chkFilterMissing.Checked OrElse Me.chkFilterTolerance.Checked OrElse Not Me.cbFilterFileSource.Text = Master.eLang.All Then Me.RunFilter()
    End Sub

    Private Sub RefreshAllMovies()
        If Me.dtMedia.Rows.Count > 0 Then
            Me.Cursor = Cursors.WaitCursor
            Me.SetControlsEnabled(False, True)
            Me.tspbLoading.Style = ProgressBarStyle.Continuous
            Me.EnableFilters(False)

            Me.tspbLoading.Maximum = Me.dtMedia.Rows.Count + 1
            Me.tspbLoading.Value = 0
            Me.tslLoading.Text = Master.eLang.GetString(110, "Refreshing Media:")
            Me.tspbLoading.Visible = True
            Me.tslLoading.Visible = True

            Me.bwRefreshMovies.WorkerReportsProgress = True
            Me.bwRefreshMovies.WorkerSupportsCancellation = True
            Me.bwRefreshMovies.RunWorkerAsync()
        Else
            Me.SetControlsEnabled(True)
        End If
    End Sub

    Private Sub RefreshAllMoviesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshAllMoviesToolStripMenuItem.Click, ReloadAllMoviesToolStripMenuItem.Click
        RefreshAllMovies()
    End Sub

    Private Function RefreshEpisode(ByVal ID As Long, Optional ByVal BatchMode As Boolean = False, Optional ByVal FromNfo As Boolean = True, Optional ByVal ToNfo As Boolean = False) As Boolean
        Dim tmpShowDb As New Structures.DBTV
        Dim tmpEp As New MediaContainers.EpisodeDetails
        Dim SeasonChanged As Boolean = False
        Dim EpisodeChanged As Boolean = False
        Dim ShowID As Integer = -1
        Dim myDelegate As New MydtListUpdate(AddressOf dtListUpdate)

        Try

            tmpShowDb = Master.DB.LoadTVEpFromDB(ID, True)

            If Directory.Exists(tmpShowDb.ShowPath) Then

                If FromNfo Then
                    If String.IsNullOrEmpty(tmpShowDb.EpNfoPath) Then
                        Dim sNFO As String = NFO.GetEpNfoPath(tmpShowDb.Filename)
                        tmpShowDb.EpNfoPath = sNFO
                        tmpEp = NFO.LoadTVEpFromNFO(sNFO, tmpShowDb.TVEp.Season, tmpShowDb.TVEp.Episode)
                    Else
                        tmpEp = NFO.LoadTVEpFromNFO(tmpShowDb.EpNfoPath, tmpShowDb.TVEp.Season, tmpShowDb.TVEp.Episode)
                    End If
                    tmpShowDb.TVEp = tmpEp
                End If

                If String.IsNullOrEmpty(tmpShowDb.TVEp.Title) Then
                    tmpShowDb.TVEp.Title = StringUtils.FilterTVEpName(Path.GetFileNameWithoutExtension(tmpShowDb.Filename), tmpShowDb.TVShow.Title, False)
                End If

                Dim eContainer As New Scanner.EpisodeContainer With {.Filename = tmpShowDb.Filename}
                fScanner.GetEpFolderContents(eContainer)
                tmpShowDb.EpPosterPath = eContainer.Poster
                tmpShowDb.EpFanartPath = eContainer.Fanart
                'assume invalid nfo if no title
                tmpShowDb.EpNfoPath = If(String.IsNullOrEmpty(tmpShowDb.TVEp.Title), String.Empty, eContainer.Nfo)

                Dim dRow = From drvRow In dtEpisodes.Rows Where Convert.ToInt64(DirectCast(drvRow, DataRow).Item(0)) = ID Select drvRow

                If Not IsNothing(dRow(0)) Then
                    tmpShowDb.IsMarkEp = Convert.ToBoolean(DirectCast(dRow(0), DataRow).Item(8))
                    tmpShowDb.IsLockEp = Convert.ToBoolean(DirectCast(dRow(0), DataRow).Item(11))

                    If Not Convert.ToInt32(DirectCast(dRow(0), DataRow).Item(12)) = tmpShowDb.TVEp.Season Then
                        SeasonChanged = True
                        ShowID = Convert.ToInt32(tmpShowDb.ShowID)
                    End If

                    If Not Convert.ToInt32(DirectCast(dRow(0), DataRow).Item(2)) = tmpShowDb.TVEp.Episode Then
                        EpisodeChanged = True
                        ShowID = Convert.ToInt32(tmpShowDb.ShowID)
                    End If

                    If Me.InvokeRequired Then
                        Me.Invoke(myDelegate, New Object() {dRow(0), 3, tmpShowDb.TVEp.Title})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 4, If(String.IsNullOrEmpty(eContainer.Poster), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 5, If(String.IsNullOrEmpty(eContainer.Fanart), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 6, If(String.IsNullOrEmpty(tmpShowDb.EpNfoPath), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 7, False})
                    Else
                        DirectCast(dRow(0), DataRow).Item(3) = tmpShowDb.TVEp.Title
                        DirectCast(dRow(0), DataRow).Item(4) = If(String.IsNullOrEmpty(eContainer.Poster), False, True)
                        DirectCast(dRow(0), DataRow).Item(5) = If(String.IsNullOrEmpty(eContainer.Fanart), False, True)
                        DirectCast(dRow(0), DataRow).Item(6) = If(String.IsNullOrEmpty(tmpShowDb.EpNfoPath), False, True)
                        DirectCast(dRow(0), DataRow).Item(7) = False
                    End If
                End If

                Master.DB.SaveTVEpToDB(tmpShowDb, False, False, BatchMode, ToNfo)

            Else
                Master.DB.DeleteTVEpFromDB(ID, False, True, BatchMode)
                Return True
            End If

            If Not BatchMode Then
                If (SeasonChanged OrElse EpisodeChanged) AndAlso ShowID > -1 Then
                    Master.DB.CleanSeasons(BatchMode)
                    Me.FillSeasons(ShowID)
                Else
                    Me.LoadEpisodeInfo(Convert.ToInt32(ID))
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return False
    End Function

    Private Function RefreshMovie(ByVal ID As Long, Optional ByVal BatchMode As Boolean = False, Optional ByVal FromNfo As Boolean = True, Optional ByVal ToNfo As Boolean = False) As Boolean
        Dim tmpMovie As New MediaContainers.Movie
        Dim tmpMovieDb As New Structures.DBMovie
        Dim OldTitle As String = String.Empty
        Dim selRow As DataRow = Nothing

        Dim hasPoster As Boolean = False
        Dim hasFanart As Boolean = False
        Dim hasNfo As Boolean = False
        Dim hasTrailer As Boolean = False
        Dim hasSub As Boolean = False
        Dim hasExtra As Boolean = False

        Dim myDelegate As New MydtListUpdate(AddressOf dtListUpdate)

        Try

            tmpMovieDb = Master.DB.LoadMovieFromDB(ID)

            OldTitle = tmpMovieDb.Movie.Title

            If Directory.Exists(Directory.GetParent(tmpMovieDb.Filename).FullName) Then

                If FromNfo Then
                    If String.IsNullOrEmpty(tmpMovieDb.NfoPath) Then
                        Dim sNFO As String = NFO.GetNfoPath(tmpMovieDb.Filename, tmpMovieDb.isSingle)
                        tmpMovieDb.NfoPath = sNFO
                        tmpMovie = NFO.LoadMovieFromNFO(sNFO, tmpMovieDb.isSingle)
                    Else
                        tmpMovie = NFO.LoadMovieFromNFO(tmpMovieDb.NfoPath, tmpMovieDb.isSingle)
                    End If
                    'subsType and subsPath not in NFO , try to load it from DB
                    For x = 0 To tmpMovie.FileInfo.StreamDetails.Subtitle.Count - 1
                        If tmpMovieDb.Movie.FileInfo.StreamDetails.Subtitle.Count > 0 AndAlso Not tmpMovieDb.Movie.FileInfo.StreamDetails.Subtitle(x) Is Nothing AndAlso tmpMovieDb.Movie.FileInfo.StreamDetails.Subtitle(x).Language = tmpMovie.FileInfo.StreamDetails.Subtitle(x).Language Then
                            tmpMovie.FileInfo.StreamDetails.Subtitle(x).SubsType = tmpMovieDb.Movie.FileInfo.StreamDetails.Subtitle(x).SubsType
                            tmpMovie.FileInfo.StreamDetails.Subtitle(x).SubsPath = tmpMovieDb.Movie.FileInfo.StreamDetails.Subtitle(x).SubsPath
                        End If
                    Next
                    tmpMovieDb.Movie = tmpMovie
                End If

                If String.IsNullOrEmpty(tmpMovieDb.Movie.Title) Then
                    If FileUtils.Common.isVideoTS(tmpMovieDb.Filename) Then
                        tmpMovieDb.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(tmpMovieDb.Filename).FullName).Name)
                        tmpMovieDb.Movie.Title = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(tmpMovieDb.Filename).FullName).Name, False)
                    ElseIf FileUtils.Common.isBDRip(tmpMovieDb.Filename) Then
                        tmpMovieDb.ListTitle = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(tmpMovieDb.Filename).FullName).FullName).Name)
                        tmpMovieDb.Movie.Title = StringUtils.FilterName(Directory.GetParent(Directory.GetParent(Directory.GetParent(tmpMovieDb.Filename).FullName).FullName).Name, False)
                    Else
                        If tmpMovieDb.UseFolder AndAlso tmpMovieDb.isSingle Then
                            tmpMovieDb.ListTitle = StringUtils.FilterName(Directory.GetParent(tmpMovieDb.Filename).Name)
                            tmpMovieDb.Movie.Title = StringUtils.FilterName(Directory.GetParent(tmpMovieDb.Filename).Name, False)
                        Else
                            tmpMovieDb.ListTitle = StringUtils.FilterName(Path.GetFileNameWithoutExtension(tmpMovieDb.Filename))
                            tmpMovieDb.Movie.Title = StringUtils.FilterName(Path.GetFileNameWithoutExtension(tmpMovieDb.Filename), False)
                        End If
                    End If
                    If Not OldTitle = tmpMovieDb.Movie.Title OrElse String.IsNullOrEmpty(tmpMovieDb.Movie.SortTitle) Then tmpMovieDb.Movie.SortTitle = tmpMovieDb.ListTitle
                Else
                    Dim tTitle As String = StringUtils.FilterTokens(tmpMovieDb.Movie.Title)
                    If Not OldTitle = tmpMovieDb.Movie.Title OrElse String.IsNullOrEmpty(tmpMovieDb.Movie.SortTitle) Then tmpMovieDb.Movie.SortTitle = tTitle
                    If Master.eSettings.DisplayYear AndAlso Not String.IsNullOrEmpty(tmpMovieDb.Movie.Year) Then
                        tmpMovieDb.ListTitle = String.Format("{0} ({1})", tTitle, tmpMovieDb.Movie.Year)
                    Else
                        tmpMovieDb.ListTitle = tTitle
                    End If
                End If
                Dim fromFile As String = APIXML.GetFileSource(tmpMovieDb.Filename)
                If Not String.IsNullOrEmpty(fromFile) Then
                    tmpMovieDb.FileSource = fromFile
                ElseIf String.IsNullOrEmpty(tmpMovieDb.FileSource) AndAlso AdvancedSettings.GetBooleanSetting("MediaSourcesByExtension", False, "*EmberAPP") Then
                    tmpMovieDb.FileSource = AdvancedSettings.GetSetting(String.Concat("MediaSourcesByExtension:", Path.GetExtension(tmpMovieDb.Filename)), String.Empty, "*EmberAPP")
                End If

                Dim mContainer As New Scanner.MovieContainer With {.Filename = tmpMovieDb.Filename, .isSingle = tmpMovieDb.isSingle}
                fScanner.GetMovieFolderContents(mContainer)
                tmpMovieDb.PosterPath = mContainer.Poster
                tmpMovieDb.FanartPath = mContainer.Fanart
                'assume invalid nfo if no title
                tmpMovieDb.NfoPath = If(String.IsNullOrEmpty(tmpMovieDb.Movie.Title), String.Empty, mContainer.Nfo)
                tmpMovieDb.TrailerPath = mContainer.Trailer
                tmpMovieDb.SubPath = mContainer.Subs
                tmpMovieDb.ExtraPath = mContainer.Extra

                hasPoster = Not String.IsNullOrEmpty(mContainer.Poster)
                hasFanart = Not String.IsNullOrEmpty(mContainer.Fanart)
                hasNfo = Not String.IsNullOrEmpty(tmpMovieDb.NfoPath)
                hasTrailer = Not String.IsNullOrEmpty(mContainer.Trailer)
                hasSub = Not String.IsNullOrEmpty(mContainer.Subs)
                hasExtra = Not String.IsNullOrEmpty(mContainer.Extra)

                Dim dRow = From drvRow In dtMedia.Rows Where Convert.ToInt64(DirectCast(drvRow, DataRow).Item(0)) = ID Select drvRow

                If Not IsNothing(dRow(0)) Then
                    selRow = DirectCast(dRow(0), DataRow)
                    tmpMovieDb.IsMark = Convert.ToBoolean(selRow.Item(11))
                    tmpMovieDb.IsLock = Convert.ToBoolean(selRow.Item(14))

                    If Me.InvokeRequired Then
                        Me.Invoke(myDelegate, New Object() {dRow(0), 1, tmpMovieDb.Filename})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 3, tmpMovieDb.ListTitle})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 4, hasPoster})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 5, hasFanart})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 6, hasNfo})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 7, hasTrailer})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 8, hasSub})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 9, hasExtra})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 10, False})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 15, tmpMovieDb.Movie.Title})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 47, tmpMovieDb.Movie.SortTitle})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 27, tmpMovieDb.Movie.Genre})
                    Else
                        selRow.Item(1) = tmpMovieDb.Filename
                        selRow.Item(3) = tmpMovieDb.ListTitle
                        selRow.Item(4) = hasPoster
                        selRow.Item(5) = hasFanart
                        selRow.Item(6) = hasNfo
                        selRow.Item(7) = hasTrailer
                        selRow.Item(8) = hasSub
                        selRow.Item(9) = hasExtra
                        selRow.Item(10) = False
                        selRow.Item(15) = tmpMovieDb.Movie.Title
                        selRow.Item(47) = tmpMovieDb.Movie.SortTitle
                        selRow.Item(27) = tmpMovieDb.Movie.Genre
                    End If
                End If
                'Why on earth resave the movie if we just refreshed its data (causes issues with saving rescrapes_
                Master.DB.SaveMovieToDB(tmpMovieDb, False, BatchMode, ToNfo)

            Else
                Master.DB.DeleteFromDB(ID, BatchMode)
                Return True
            End If

            If Not BatchMode Then
                Me.DoTitleCheck()

                Dim selI As Integer = 0

                If Me.dgvMediaList.SelectedRows.Count > 0 Then selI = Me.dgvMediaList.SelectedRows(0).Index

                Me.dgvMediaList.ClearSelection()
                Me.dgvMediaList.CurrentCell = Nothing

                If Me.dgvMediaList.RowCount - 1 < selI Then selI = Me.dgvMediaList.RowCount

                Me.ClearInfo()
                Me.prevRow = -2
                Me.currRow = -1

                If Me.dgvMediaList.RowCount > 0 Then
                    Me.dgvMediaList.Rows(selI).Cells(3).Selected = True
                    Me.dgvMediaList.CurrentCell = Me.dgvMediaList.Rows(selI).Cells(3)
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return False
    End Function

    Private Function RefreshSeason(ByVal ShowID As Integer, ByVal Season As Integer, ByVal BatchMode As Boolean) As Boolean
        Dim tmpSeasonDb As New Structures.DBTV
        Dim tmpShow As New MediaContainers.TVShow

        Dim myDelegate As New MydtListUpdate(AddressOf dtListUpdate)

        Try
            Dim SQLtransaction As SQLite.SQLiteTransaction = Nothing
            If Not BatchMode Then SQLtransaction = Master.DB.MediaDBConn.BeginTransaction()

            tmpSeasonDb = Master.DB.LoadTVSeasonFromDB(ShowID, Season, True)

            Dim tPath As String = Functions.GetSeasonDirectoryFromShowPath(tmpSeasonDb.ShowPath, Season)

            If String.IsNullOrEmpty(tPath) Then
                tPath = tmpSeasonDb.ShowPath
            End If

            'fake file just for getting images
            tmpSeasonDb.Filename = Path.Combine(tPath, "file.ext")
            fScanner.GetSeasonImages(tmpSeasonDb, Season)

            Dim dRow = From drvRow In dtSeasons.Rows Where Convert.ToInt64(DirectCast(drvRow, DataRow).Item(0)) = ShowID AndAlso Convert.ToInt32(DirectCast(drvRow, DataRow).Item(2)) = Season Select drvRow

            If Not IsNothing(dRow(0)) Then
                If Me.InvokeRequired Then
                    Me.Invoke(myDelegate, New Object() {dRow(0), 3, If(String.IsNullOrEmpty(tmpSeasonDb.SeasonPosterPath), False, True)})
                    If Master.eSettings.SeasonFanartEnabled Then
                        Me.Invoke(myDelegate, New Object() {dRow(0), 4, If(String.IsNullOrEmpty(tmpSeasonDb.SeasonFanartPath), False, True)})
                    End If
                    Me.Invoke(myDelegate, New Object() {dRow(0), 9, False})
                Else
                    DirectCast(dRow(0), DataRow).Item(3) = If(String.IsNullOrEmpty(tmpSeasonDb.SeasonPosterPath), False, True)
                    If Master.eSettings.SeasonFanartEnabled Then
                        DirectCast(dRow(0), DataRow).Item(4) = If(String.IsNullOrEmpty(tmpSeasonDb.SeasonFanartPath), False, True)
                    End If
                    DirectCast(dRow(0), DataRow).Item(9) = False
                End If
            End If

            If Not BatchMode Then
                SQLtransaction.Commit()
                SQLtransaction = Nothing

                Me.LoadSeasonInfo(ShowID, Season)
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return False
    End Function

    Private Function RefreshShow(ByVal ID As Long, ByVal BatchMode As Boolean, ByVal FromNfo As Boolean, ByVal ToNfo As Boolean, ByVal WithEpisodes As Boolean) As Boolean
        If Not BatchMode Then
            Me.tspbLoading.Style = ProgressBarStyle.Continuous
            Me.tspbLoading.Value = 0

            Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                SQLCommand.CommandText = String.Concat("SELECT COUNT(ID) AS COUNT FROM TVEps WHERE TVShowID = ", ID, " AND Missing = 0;")
                Me.tspbLoading.Maximum = Convert.ToInt32(SQLCommand.ExecuteScalar) + 1
            End Using

            Me.tslLoading.Text = Master.eLang.GetString(731, "Refreshing Show:")
            Me.tslLoading.Visible = True
            Me.tspbLoading.Visible = True
            Application.DoEvents()
        End If

        Dim tmpShowDb As New Structures.DBTV
        Dim tmpShow As New MediaContainers.TVShow

        Dim myDelegate As New MydtListUpdate(AddressOf dtListUpdate)

        Try
            Dim SQLtransaction As SQLite.SQLiteTransaction = Nothing
            If Not BatchMode Then SQLtransaction = Master.DB.MediaDBConn.BeginTransaction()

            tmpShowDb = Master.DB.LoadTVFullShowFromDB(ID)

            If Directory.Exists(tmpShowDb.ShowPath) Then

                If FromNfo Then
                    If String.IsNullOrEmpty(tmpShowDb.ShowNfoPath) Then
                        Dim sNFO As String = NFO.GetShowNfoPath(tmpShowDb.ShowPath)
                        tmpShowDb.ShowNfoPath = sNFO
                        tmpShow = NFO.LoadTVShowFromNFO(sNFO)
                    Else
                        tmpShow = NFO.LoadTVShowFromNFO(tmpShowDb.ShowNfoPath)
                    End If
                    tmpShowDb.TVShow = tmpShow
                End If

                If String.IsNullOrEmpty(tmpShowDb.TVShow.Title) Then
                    tmpShowDb.TVShow.Title = StringUtils.FilterTVShowName(Path.GetFileNameWithoutExtension(tmpShowDb.ShowPath), False)
                End If

                Dim sContainer As New Scanner.TVShowContainer With {.ShowPath = tmpShowDb.ShowPath}
                fScanner.GetShowFolderContents(sContainer)
                tmpShowDb.ShowPosterPath = sContainer.Poster
                tmpShowDb.ShowFanartPath = sContainer.Fanart
                'assume invalid nfo if no title
                tmpShowDb.ShowNfoPath = If(String.IsNullOrEmpty(tmpShowDb.TVShow.Title), String.Empty, sContainer.Nfo)

                Dim dRow = From drvRow In dtShows.Rows Where Convert.ToInt64(DirectCast(drvRow, DataRow).Item(0)) = ID Select drvRow

                If Not IsNothing(dRow(0)) Then
                    tmpShowDb.IsMarkShow = Convert.ToBoolean(DirectCast(dRow(0), DataRow).Item(6))
                    tmpShowDb.IsLockShow = Convert.ToBoolean(DirectCast(dRow(0), DataRow).Item(10))

                    If Me.InvokeRequired Then
                        Me.Invoke(myDelegate, New Object() {dRow(0), 1, tmpShowDb.TVShow.Title})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 2, If(String.IsNullOrEmpty(sContainer.Poster), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 3, If(String.IsNullOrEmpty(sContainer.Fanart), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 4, If(String.IsNullOrEmpty(tmpShowDb.ShowNfoPath), False, True)})
                        Me.Invoke(myDelegate, New Object() {dRow(0), 5, False})
                    Else
                        DirectCast(dRow(0), DataRow).Item(1) = tmpShowDb.TVShow.Title
                        DirectCast(dRow(0), DataRow).Item(2) = If(String.IsNullOrEmpty(sContainer.Poster), False, True)
                        DirectCast(dRow(0), DataRow).Item(3) = If(String.IsNullOrEmpty(sContainer.Fanart), False, True)
                        DirectCast(dRow(0), DataRow).Item(4) = If(String.IsNullOrEmpty(tmpShowDb.ShowNfoPath), False, True)
                        DirectCast(dRow(0), DataRow).Item(5) = False
                    End If
                End If

                Master.DB.SaveTVShowToDB(tmpShowDb, False, WithEpisodes, ToNfo)

                If Not BatchMode Then
                    Me.tspbLoading.Value += 1
                    Application.DoEvents()
                End If

                If WithEpisodes Then
                    Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        SQLCommand.CommandText = String.Concat("SELECT ID FROM TVEps WHERE TVShowID = ", ID, " AND Missing = 0;")
                        Using SQLReader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                            While SQLReader.Read
                                Me.RefreshEpisode(Convert.ToInt64(SQLReader("ID")), True)
                                If Not BatchMode Then
                                    Me.tspbLoading.Value += 1
                                    Application.DoEvents()
                                    Threading.Thread.Sleep(50)
                                End If
                            End While
                        End Using
                    End Using

                    Master.DB.CleanSeasons(True)
                End If

            Else
                Master.DB.DeleteTVShowFromDB(ID, WithEpisodes)
                Return True
            End If

            If Not BatchMode Then
                SQLtransaction.Commit()
                SQLtransaction = Nothing

                Me.LoadShowInfo(Convert.ToInt32(ID))

                Me.tslLoading.Visible = False
                Me.tspbLoading.Visible = False
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return False
    End Function

    Private Sub ReloadMovie()
        Try
            Me.dgvMediaList.Cursor = Cursors.WaitCursor
            Me.SetControlsEnabled(False, True)

            Dim doFill As Boolean = False
            Dim tFill As Boolean = False

            Dim doBatch As Boolean = Not Me.dgvMediaList.SelectedRows.Count = 1

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    tFill = Me.RefreshMovie(Convert.ToInt64(sRow.Cells(0).Value), doBatch)
                    If tFill Then doFill = True
                Next
                SQLtransaction.Commit()
            End Using

            Me.dgvMediaList.Cursor = Cursors.Default
            Me.SetControlsEnabled(True)

            If doFill Then FillList(0) Else DoTitleCheck()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub RemoveFromDatabaseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveFromDatabaseToolStripMenuItem.Click
        Try
            Me.ClearInfo()

            For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                Master.DB.DeleteFromDB(Convert.ToInt64(sRow.Cells(0).Value))
            Next

            Me.FillList(0)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub RemoveGenreToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveGenreToolStripMenuItem.Click
        Try
            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parGenre As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parGenre", DbType.String, 0, "Genre")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE movies SET Genre = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                        If sRow.Cells(27).Value.ToString.Contains(Me.GenreListToolStripComboBox.Text) Then
                            parGenre.Value = sRow.Cells(27).Value.ToString.Replace(String.Concat(" / ", Me.GenreListToolStripComboBox.Text), String.Empty).Replace(String.Concat(Me.GenreListToolStripComboBox.Text, " / "), String.Empty).Replace(Me.GenreListToolStripComboBox.Text, String.Empty).Trim
                            parID.Value = sRow.Cells(0).Value
                            SQLcommand.ExecuteNonQuery()
                        End If
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    Me.RefreshMovie(Convert.ToInt64(sRow.Cells(0).Value), True, False, True)
                Next
                SQLtransaction.Commit()
            End Using

            Me.LoadInfo(Convert.ToInt32(Me.dgvMediaList.Item(0, Me.dgvMediaList.CurrentCell.RowIndex).Value), Me.dgvMediaList.Item(1, Me.dgvMediaList.CurrentCell.RowIndex).Value.ToString, True, False)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ResizeMediaList()
        If Not Master.isWindows Then
            If Me.dgvMediaList.ColumnCount > 0 Then
                Me.dgvMediaList.Columns(3).Width = Me.dgvMediaList.Width - _
                If(Master.eSettings.MoviePosterCol, 0, 20) - _
                If(Master.eSettings.MovieFanartCol, 0, 20) - _
                If(Master.eSettings.MovieInfoCol, 0, 20) - _
                If(Master.eSettings.MovieTrailerCol, 0, 20) - _
                If(Master.eSettings.MovieSubCol, 0, 20) - _
                If(Master.eSettings.MovieExtraCol, 0, 20) - _
                If(Me.dgvMediaList.DisplayRectangle.Height > Me.dgvMediaList.ClientRectangle.Height, 0, SystemInformation.VerticalScrollBarWidth)
            End If
        End If
    End Sub

    Private Sub ResizeTVLists(ByVal iType As Integer)
        '0 = all.... needed???

        If Not Master.isWindows Then
            If (iType = 0 OrElse iType = 1) AndAlso Me.dgvTVShows.ColumnCount > 0 Then
                Me.dgvTVShows.Columns(1).Width = Me.dgvTVShows.Width - _
                If(Master.eSettings.ShowPosterCol, 0, 20) - _
                If(Master.eSettings.ShowFanartCol, 0, 20) - _
                If(Master.eSettings.ShowNfoCol, 0, 20) - _
                If(Me.dgvTVShows.DisplayRectangle.Height > Me.dgvTVShows.ClientRectangle.Height, 0, SystemInformation.VerticalScrollBarWidth)
            End If

            If (iType = 0 OrElse iType = 2) AndAlso Me.dgvTVSeasons.ColumnCount > 0 Then
                Me.dgvTVSeasons.Columns(1).Width = Me.dgvTVSeasons.Width - _
                If(Master.eSettings.SeasonPosterCol, 0, 20) - _
                If(Master.eSettings.SeasonFanartCol, 0, 20) - _
                If(Me.dgvTVSeasons.DisplayRectangle.Height > Me.dgvTVSeasons.ClientRectangle.Height, 0, SystemInformation.VerticalScrollBarWidth)
            End If

            If (iType = 0 OrElse iType = 3) AndAlso Me.dgvTVEpisodes.ColumnCount > 0 Then
                Me.dgvTVEpisodes.Columns(2).Width = Me.dgvTVEpisodes.Width - 40 - _
                If(Master.eSettings.EpisodePosterCol, 0, 20) - _
                If(Master.eSettings.EpisodeFanartCol, 0, 20) - _
                If(Master.eSettings.EpisodeNfoCol, 0, 20) - _
                If(Me.dgvTVEpisodes.DisplayRectangle.Height > Me.dgvTVEpisodes.ClientRectangle.Height, 0, SystemInformation.VerticalScrollBarWidth)
            End If

        End If
    End Sub

    Private Sub RunFilter(Optional ByVal doFill As Boolean = False)
        Try
            If Me.Visible Then

                Me.ClearInfo()

                Me.prevRow = -2
                Me.currRow = -1
                Me.dgvMediaList.ClearSelection()
                Me.dgvMediaList.CurrentCell = Nothing

                If FilterArray.Count > 0 Then
                    Dim FilterString As String = String.Empty

                    If rbFilterAnd.Checked Then
                        FilterString = Microsoft.VisualBasic.Strings.Join(FilterArray.ToArray, " AND ")
                    Else
                        FilterString = Microsoft.VisualBasic.Strings.Join(FilterArray.ToArray, " OR ")
                    End If

                    bsMedia.Filter = FilterString
                Else
                    bsMedia.RemoveFilter()
                End If

                If doFill Then
                    Me.FillList(0)
                Else
                    Me.dgvMediaList.Focus()
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ScannerUpdated(ByVal iType As Integer, ByVal sText As String)
        Select Case iType
            Case 1
                Me.SetStatus(String.Concat(Master.eLang.GetString(814, "Added Episode: "), sText))
            Case 2
                Me.SetStatus(Master.eLang.GetString(116, "Performing Preliminary Tasks (Gathering Data)..."))
            Case 3
                Me.SetStatus(Master.eLang.GetString(644, "Cleaning Database..."))
            Case Else
                Me.SetStatus(String.Concat(Master.eLang.GetString(815, "Added Movie: "), sText))
        End Select
    End Sub

    Private Sub ScanningCompleted()
        If Not isCL Then
            Me.SetStatus(String.Empty)
            Me.FillList(0)
            Me.tspbLoading.Visible = False
            Me.tslLoading.Visible = False
        Else
            Me.FillList(0)
            LoadingDone = True
        End If
    End Sub

    Private Sub scMain_SplitterMoved(ByVal sender As System.Object, ByVal e As System.Windows.Forms.SplitterEventArgs) Handles scMain.SplitterMoved
        Try
            If Me.Created Then
                Me.SuspendLayout()
                Me.MoveMPAA()
                Me.MoveGenres()

                ImageUtils.ResizePB(Me.pbFanart, Me.pbFanartCache, Me.scMain.Panel2.Height - 90, Me.scMain.Panel2.Width)
                Me.pbFanart.Left = Convert.ToInt32((Me.scMain.Panel2.Width - Me.pbFanart.Width) / 2)
                Me.pnlNoInfo.Location = New Point(Convert.ToInt32((Me.scMain.Panel2.Width - Me.pnlNoInfo.Width) / 2), Convert.ToInt32((Me.scMain.Panel2.Height - Me.pnlNoInfo.Height) / 2))
                Me.pnlCancel.Location = New Point(Convert.ToInt32((Me.scMain.Panel2.Width - Me.pnlNoInfo.Width) / 2), 100)
                Me.pnlFilterGenre.Location = New Point(Me.gbSpecific.Left + Me.txtFilterGenre.Left, (Me.pnlFilter.Top + Me.txtFilterGenre.Top + Me.gbSpecific.Top) - Me.pnlFilterGenre.Height)
                Me.pnlFilterSource.Location = New Point(Me.gbSpecific.Left + Me.txtFilterSource.Left, (Me.pnlFilter.Top + Me.txtFilterSource.Top + Me.gbSpecific.Top) - Me.pnlFilterSource.Height)

                Select Case Me.tabsMain.SelectedIndex
                    Case 0
                        Me.dgvMediaList.Focus()
                    Case 1
                        Me.dgvTVShows.Focus()
                End Select

                Me.ResumeLayout(True)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectAllAskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllAskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectAllAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.All, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectEpisodeRow(ByVal iRow As Integer)
        Try
            If Not Convert.ToBoolean(Me.dgvTVEpisodes.Item(4, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvTVEpisodes.Item(5, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvTVEpisodes.Item(6, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvTVEpisodes.Item(22, iRow).Value) Then
                Me.ClearInfo(False)
                Me.ShowNoInfo(True, 2)

                Master.currShow = Master.DB.LoadTVEpFromDB(Convert.ToInt32(Me.dgvTVEpisodes.Item(0, iRow).Value), True)

                If Not Convert.ToBoolean(Me.dgvTVEpisodes.Item(22, iRow).Value) AndAlso Not Me.fScanner.IsBusy AndAlso Not Me.bwMediaInfo.IsBusy AndAlso Not Me.bwLoadInfo.IsBusy AndAlso Not Me.bwLoadShowInfo.IsBusy AndAlso Not Me.bwLoadSeasonInfo.IsBusy AndAlso Not Me.bwLoadEpInfo.IsBusy AndAlso Not Me.bwRefreshMovies.IsBusy AndAlso Not Me.bwCleanDB.IsBusy Then
                    Me.mnuEpisodes.Enabled = True
                End If
            Else
                Me.LoadEpisodeInfo(Convert.ToInt32(Me.dgvTVEpisodes.SelectedRows(0).Cells(0).Value))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectExtraAskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectExtraAskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectExtraAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectExtraAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Extra, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectFanartAskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectFanartAskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectFanartAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectFanartAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Fanart, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectMeEtaAskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectMeEtaAskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectMetaAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectMetaAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Meta, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectNfoAskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectNfoAskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectNfoAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectNfoAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.NFO, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectPosterAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectPosterAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SelectPosterÃskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectPosterÃskToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Poster, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub SelectRow(ByVal iRow As Integer)
        Try
            If Not Convert.ToBoolean(Me.dgvMediaList.Item(4, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvMediaList.Item(5, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvMediaList.Item(6, iRow).Value) Then
                Me.ClearInfo()
                Me.ShowNoInfo(True, 0)
                Master.currMovie = Master.DB.LoadMovieFromDB(Convert.ToInt64(Me.dgvMediaList.Item(0, iRow).Value))

                If Not Me.bwMovieScraper.IsBusy AndAlso Not Me.bwNonScrape.IsBusy AndAlso Not Me.fScanner.IsBusy AndAlso Not Me.bwMediaInfo.IsBusy AndAlso Not Me.bwLoadInfo.IsBusy AndAlso Not Me.bwLoadShowInfo.IsBusy AndAlso Not Me.bwLoadSeasonInfo.IsBusy AndAlso Not Me.bwLoadEpInfo.IsBusy AndAlso Not Me.bwRefreshMovies.IsBusy AndAlso Not Me.bwCleanDB.IsBusy Then
                    Me.mnuMediaList.Enabled = True
                End If
            Else
                Me.LoadInfo(Convert.ToInt32(Me.dgvMediaList.Item(0, iRow).Value), Me.dgvMediaList.Item(1, iRow).Value.ToString, True, False)
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectSeasonRow(ByVal iRow As Integer)
        Try
            Me.ClearInfo(False)
            If String.IsNullOrEmpty(Master.currShow.ShowPosterPath) AndAlso String.IsNullOrEmpty(Master.currShow.ShowFanartPath) AndAlso _
               String.IsNullOrEmpty(Master.currShow.ShowNfoPath) AndAlso Not Convert.ToBoolean(Me.dgvTVSeasons.Item(3, iRow).Value) AndAlso _
               Not Convert.ToBoolean(Me.dgvTVSeasons.Item(4, iRow).Value) Then
                If Not Me.currThemeType = Theming.ThemeType.Show Then Me.ApplyTheme(Theming.ThemeType.Show)
                Me.ShowNoInfo(True, 1)
                Master.currShow = Master.DB.LoadTVSeasonFromDB(Convert.ToInt32(Me.dgvTVSeasons.Item(0, iRow).Value), Convert.ToInt32(Me.dgvTVSeasons.Item(2, iRow).Value), True)

                Me.FillEpisodes(Convert.ToInt32(Me.dgvTVSeasons.Item(0, iRow).Value), Convert.ToInt32(Me.dgvTVSeasons.Item(2, iRow).Value))

                If Not Me.fScanner.IsBusy AndAlso Not Me.bwMediaInfo.IsBusy AndAlso Not Me.bwLoadInfo.IsBusy AndAlso Not Me.bwLoadShowInfo.IsBusy AndAlso Not Me.bwLoadSeasonInfo.IsBusy AndAlso Not Me.bwLoadEpInfo.IsBusy AndAlso Not Me.bwRefreshMovies.IsBusy AndAlso Not Me.bwCleanDB.IsBusy Then
                    Me.mnuSeasons.Enabled = True
                End If
            Else
                Me.LoadSeasonInfo(Convert.ToInt32(Me.dgvTVSeasons.Item(0, iRow).Value), Convert.ToInt32(Me.dgvTVSeasons.Item(2, iRow).Value))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectShowRow(ByVal iRow As Integer)
        Try
            Me.tmpTitle = Me.dgvTVShows.Item(1, iRow).Value.ToString
            Me.tmpTVDB = Me.dgvTVShows.Item(9, iRow).Value.ToString
            Me.tmpLang = Me.dgvTVShows.Item(22, iRow).Value.ToString
            Me.tmpOrdering = DirectCast(Convert.ToInt32(Me.dgvTVShows.Item(23, iRow).Value), Enums.Ordering)

            If Not Convert.ToBoolean(Me.dgvTVShows.Item(2, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvTVShows.Item(3, iRow).Value) AndAlso Not Convert.ToBoolean(Me.dgvTVShows.Item(4, iRow).Value) Then
                Me.ClearInfo()
                Me.ShowNoInfo(True, 1)

                Master.currShow = Master.DB.LoadTVFullShowFromDB(Convert.ToInt64(Me.dgvTVShows.Item(0, iRow).Value))

                Me.FillSeasons(Convert.ToInt32(Me.dgvTVShows.Item(0, iRow).Value))

                If Not Me.fScanner.IsBusy AndAlso Not Me.bwMediaInfo.IsBusy AndAlso Not Me.bwLoadInfo.IsBusy AndAlso Not Me.bwLoadShowInfo.IsBusy AndAlso Not Me.bwLoadSeasonInfo.IsBusy AndAlso Not Me.bwLoadEpInfo.IsBusy AndAlso Not Me.bwRefreshMovies.IsBusy AndAlso Not Me.bwCleanDB.IsBusy Then
                    Me.mnuShows.Enabled = True
                End If
            Else
                Me.LoadShowInfo(Convert.ToInt32(Me.dgvTVShows.Item(0, iRow).Value))
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SelectTrailerAutoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectTrailerAutoToolStripMenuItem.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAuto, Master.DefaultOptions)
    End Sub

    Private Sub SetAVImages(ByVal aImage As Image())
        Try
            Me.pbResolution.Image = aImage(0)
            Me.pbVideo.Image = aImage(1)
            Me.pbVType.Image = aImage(2)
            Me.pbAudio.Image = aImage(3)
            Me.pbChannels.Image = aImage(4)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetControlsEnabled(ByVal isEnabled As Boolean, Optional ByVal withLists As Boolean = False, Optional ByVal withTools As Boolean = True)
        'Me.ToolsToolStripMenuItem.Enabled = isEnabled AndAlso (Me.dgvMediaList.RowCount > 0 OrElse Me.dgvTVShows.RowCount > 0)
        For Each i As Object In Me.ToolsToolStripMenuItem.DropDownItems
            If TypeOf i Is ToolStripMenuItem Then
                Dim o As ToolStripMenuItem = DirectCast(i, ToolStripMenuItem)
                If o.Tag Is Nothing Then
                    o.Enabled = isEnabled AndAlso (Me.dgvMediaList.RowCount > 0 OrElse Me.dgvTVShows.RowCount > 0) AndAlso tabsMain.SelectedIndex = 0
                ElseIf TypeOf o.Tag Is Structures.ModulesMenus Then
                    Dim tagmenu As Structures.ModulesMenus = DirectCast(o.Tag, Structures.ModulesMenus)
                    o.Enabled = (isEnabled OrElse Not withTools) AndAlso (((Me.dgvMediaList.RowCount > 0 OrElse tagmenu.IfNoMovies) AndAlso tabsMain.SelectedIndex = 0) OrElse ((Me.dgvTVShows.RowCount > 0 OrElse tagmenu.IfNoTVShow) AndAlso tabsMain.SelectedIndex = 1))
                End If
            ElseIf TypeOf i Is ToolStripSeparator Then
                Dim o As ToolStripSeparator = DirectCast(i, ToolStripSeparator)
                o.Visible = (Me.ToolsToolStripMenuItem.DropDownItems.IndexOf(o) < Me.ToolsToolStripMenuItem.DropDownItems.Count - 1)
            End If
        Next
        With Master.eSettings
            If (Not .ExpertCleaner AndAlso (.CleanDotFanartJPG OrElse .CleanFanartJPG OrElse .CleanFolderJPG OrElse .CleanMovieFanartJPG OrElse _
            .CleanMovieJPG OrElse .CleanMovieNameJPG OrElse .CleanMovieNFO OrElse .CleanMovieNFOB OrElse _
            .CleanMovieTBN OrElse .CleanMovieTBNB OrElse .CleanPosterJPG OrElse .CleanPosterTBN OrElse .CleanExtraThumbs)) OrElse _
            (.ExpertCleaner AndAlso (.CleanWhitelistVideo OrElse .CleanWhitelistExts.Count > 0)) Then
                Me.CleanFoldersToolStripMenuItem.Enabled = isEnabled AndAlso Me.dgvMediaList.RowCount > 0 AndAlso Me.tabsMain.SelectedIndex = 0
            Else
                Me.CleanFoldersToolStripMenuItem.Enabled = False
            End If
        End With
        Me.EditToolStripMenuItem.Enabled = isEnabled
        Me.tsbAutoPilot.Enabled = isEnabled AndAlso Me.dgvMediaList.RowCount > 0 AndAlso Me.tabsMain.SelectedIndex = 0
        Me.tsbRefreshMedia.Enabled = isEnabled
        Me.tsbMediaCenters.Enabled = isEnabled
        Me.mnuMediaList.Enabled = isEnabled
        Me.mnuShows.Enabled = isEnabled
        Me.mnuSeasons.Enabled = isEnabled
        Me.mnuEpisodes.Enabled = isEnabled
        Me.txtSearch.Enabled = isEnabled
        Me.tabsMain.Enabled = isEnabled
        Me.btnMarkAll.Enabled = isEnabled
        Me.btnMetaDataRefresh.Enabled = isEnabled
        Me.scMain.IsSplitterFixed = Not isEnabled
        Me.scTV.IsSplitterFixed = Not isEnabled
        Me.SplitContainer2.IsSplitterFixed = Not isEnabled
        Me.HelpToolStripMenuItem.Enabled = isEnabled
        Me.cmnuTrayIconTools.Enabled = Me.ToolsToolStripMenuItem.Enabled
        Me.cmnuTrayIconScrapeMedia.Enabled = Me.tsbAutoPilot.Enabled
        Me.cmnuTrayIconUpdateMedia.Enabled = isEnabled
        Me.cmnuTrayIconMediaCenters.Enabled = isEnabled
        Me.cmnuTrayIconSettings.Enabled = isEnabled
        Me.cmnuTrayIconExit.Enabled = isEnabled

        If withLists OrElse isEnabled Then
            Me.dgvMediaList.TabStop = isEnabled
            Me.dgvTVShows.TabStop = isEnabled
            Me.dgvTVSeasons.TabStop = isEnabled
            Me.dgvTVEpisodes.TabStop = isEnabled
            Me.dgvMediaList.Enabled = isEnabled
            Me.dgvTVShows.Enabled = isEnabled
            Me.dgvTVSeasons.Enabled = isEnabled
            Me.dgvTVEpisodes.Enabled = isEnabled
        End If
    End Sub

    Private Sub SetGenreToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetGenreToolStripMenuItem.Click
        Try
            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    Dim parGenre As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parGenre", DbType.String, 0, "Genre")
                    Dim parID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parID", DbType.Int32, 0, "id")
                    SQLcommand.CommandText = "UPDATE movies SET Genre = (?) WHERE id = (?);"
                    For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                        parGenre.Value = Me.GenreListToolStripComboBox.Text.Trim
                        parID.Value = sRow.Cells(0).Value
                        SQLcommand.ExecuteNonQuery()
                    Next
                End Using
                SQLtransaction.Commit()
            End Using

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each sRow As DataGridViewRow In Me.dgvMediaList.SelectedRows
                    Me.RefreshMovie(Convert.ToInt64(sRow.Cells(0).Value), True, False, True)
                Next
                SQLtransaction.Commit()
            End Using

            Me.LoadInfo(Convert.ToInt32(Me.dgvMediaList.Item(0, Me.dgvMediaList.CurrentCell.RowIndex).Value), Me.dgvMediaList.Item(1, Me.dgvMediaList.CurrentCell.RowIndex).Value.ToString, True, False)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetMenus(ByVal ReloadFilters As Boolean)
        Dim mnuItem As ToolStripItem

        Try
            With Master.eSettings
                If (Not .ExpertCleaner AndAlso (.CleanDotFanartJPG OrElse .CleanFanartJPG OrElse .CleanFolderJPG OrElse .CleanMovieFanartJPG OrElse _
                .CleanMovieJPG OrElse .CleanMovieNameJPG OrElse .CleanMovieNFO OrElse .CleanMovieNFOB OrElse _
                .CleanMovieTBN OrElse .CleanMovieTBNB OrElse .CleanPosterJPG OrElse .CleanPosterTBN OrElse .CleanExtraThumbs)) OrElse _
                (.ExpertCleaner AndAlso (.CleanWhitelistVideo OrElse .CleanWhitelistExts.Count > 0)) Then
                    Me.CleanFoldersToolStripMenuItem.Enabled = True AndAlso Me.dgvMediaList.RowCount > 0 AndAlso Me.tabsMain.SelectedIndex = 0
                Else
                    Me.CleanFoldersToolStripMenuItem.Enabled = False
                End If

                Me.CopyExistingFanartToBackdropsFolderToolStripMenuItem.Enabled = Directory.Exists(.BDPath)

                Me.ClearAllCachesToolStripMenuItem.Enabled = .UseImgCache

                Me.mnuAllAutoExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuAllAskExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuMissAutoExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuMissAskExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuMarkAutoExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuMarkAskExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuNewAutoExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuNewAskExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuFilterAutoExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET
                Me.mnuFilterAskExtra.Enabled = .AutoThumbs > 0 OrElse .AutoET

                Dim PosterAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Poster)
                Me.mnuAllAutoPoster.Enabled = PosterAllowed
                Me.mnuAllAskPoster.Enabled = PosterAllowed
                Me.mnuMissAutoPoster.Enabled = PosterAllowed
                Me.mnuMissAskPoster.Enabled = PosterAllowed
                Me.mnuMarkAutoPoster.Enabled = PosterAllowed
                Me.mnuMarkAskPoster.Enabled = PosterAllowed
                Me.mnuNewAutoPoster.Enabled = PosterAllowed
                Me.mnuNewAskPoster.Enabled = PosterAllowed
                Me.mnuFilterAutoPoster.Enabled = PosterAllowed
                Me.mnuFilterAskPoster.Enabled = PosterAllowed

                Dim FanartAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Fanart)
                Me.mnuAllAutoFanart.Enabled = FanartAllowed
                Me.mnuAllAskFanart.Enabled = FanartAllowed
                Me.mnuMissAutoFanart.Enabled = FanartAllowed
                Me.mnuMissAskFanart.Enabled = FanartAllowed
                Me.mnuMarkAutoFanart.Enabled = FanartAllowed
                Me.mnuMarkAskFanart.Enabled = FanartAllowed
                Me.mnuNewAutoFanart.Enabled = FanartAllowed
                Me.mnuNewAskFanart.Enabled = FanartAllowed
                Me.mnuFilterAutoFanart.Enabled = FanartAllowed
                Me.mnuFilterAskFanart.Enabled = FanartAllowed

                Me.mnuAllAskMI.Enabled = .ScanMediaInfo
                Me.mnuAllAutoMI.Enabled = .ScanMediaInfo
                Me.mnuNewAskMI.Enabled = .ScanMediaInfo
                Me.mnuNewAutoMI.Enabled = .ScanMediaInfo
                Me.mnuMarkAskMI.Enabled = .ScanMediaInfo
                Me.mnuMarkAutoMI.Enabled = .ScanMediaInfo
                Me.mnuFilterAskMI.Enabled = .ScanMediaInfo
                Me.mnuFilterAutoMI.Enabled = .ScanMediaInfo

                Dim TrailerAllowed As Boolean = ModulesManager.Instance.QueryPostScraperCapabilities(Enums.PostScraperCapabilities.Trailer)
                Me.mnuAllAutoTrailer.Enabled = TrailerAllowed
                Me.mnuAllAskTrailer.Enabled = TrailerAllowed
                Me.mnuMissAutoTrailer.Enabled = TrailerAllowed
                Me.mnuMissAskTrailer.Enabled = TrailerAllowed
                Me.mnuNewAutoTrailer.Enabled = TrailerAllowed
                Me.mnuNewAskTrailer.Enabled = TrailerAllowed
                Me.mnuMarkAutoTrailer.Enabled = TrailerAllowed
                Me.mnuMarkAskTrailer.Enabled = TrailerAllowed
                Me.mnuFilterAutoTrailer.Enabled = TrailerAllowed
                Me.mnuFilterAskTrailer.Enabled = TrailerAllowed

                Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies WHERE mark = 1;")
                    Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                        If Convert.ToInt32(SQLcount("mcount")) > 0 Then
                            Me.btnMarkAll.Text = Master.eLang.GetString(105, "Unmark All")
                        Else
                            Me.btnMarkAll.Text = Master.eLang.GetString(35, "Mark All")
                        End If
                    End Using
                End Using

                Me.mnuMoviesUpdate.DropDownItems.Clear()
                Me.cmnuTrayIconUpdateMovies.DropDownItems.Clear()
                Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    SQLNewcommand.CommandText = "SELECT COUNT(ID) AS cID FROM Sources;"
                    If Convert.ToInt32(SQLNewcommand.ExecuteScalar) > 1 Then
                        mnuItem = Me.mnuMoviesUpdate.DropDownItems.Add(Master.eLang.GetString(649, "Update All"), Nothing, New System.EventHandler(AddressOf SourceSubClick))
                        mnuItem.Tag = String.Empty
                        mnuItem = Me.cmnuTrayIconUpdateMovies.DropDownItems.Add(Master.eLang.GetString(649, "Update All"), Nothing, New System.EventHandler(AddressOf SourceSubClick))
                        mnuItem.Tag = String.Empty
                    End If
                    SQLNewcommand.CommandText = "SELECT Name FROM Sources;"
                    Using SQLReader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                        While SQLReader.Read
                            mnuItem = Me.mnuMoviesUpdate.DropDownItems.Add(String.Format(Master.eLang.GetString(143, "Update {0} Only"), SQLReader("Name")), Nothing, New System.EventHandler(AddressOf SourceSubClick))
                            mnuItem.Tag = SQLReader("Name").ToString
                            mnuItem = Me.cmnuTrayIconUpdateMovies.DropDownItems.Add(String.Format(Master.eLang.GetString(143, "Update {0} Only"), SQLReader("Name")), Nothing, New System.EventHandler(AddressOf SourceSubClick))
                            mnuItem.Tag = SQLReader("Name").ToString
                        End While
                    End Using
                End Using

                Me.mnuTVShowUpdate.DropDownItems.Clear()
                Me.cmnuTrayIconUpdateTV.DropDownItems.Clear()
                Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                    SQLNewcommand.CommandText = "SELECT COUNT(ID) AS cID FROM TVSources;"
                    If Convert.ToInt32(SQLNewcommand.ExecuteScalar) > 1 Then
                        mnuItem = Me.mnuTVShowUpdate.DropDownItems.Add(Master.eLang.GetString(649, "Update All"), Nothing, New System.EventHandler(AddressOf TVSourceSubClick))
                        mnuItem.Tag = String.Empty
                        mnuItem = Me.cmnuTrayIconUpdateTV.DropDownItems.Add(Master.eLang.GetString(649, "Update All"), Nothing, New System.EventHandler(AddressOf TVSourceSubClick))
                        mnuItem.Tag = String.Empty
                    End If
                    SQLNewcommand.CommandText = "SELECT Name FROM TVSources;"
                    Using SQLReader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                        While SQLReader.Read
                            mnuItem = Me.mnuTVShowUpdate.DropDownItems.Add(String.Format(Master.eLang.GetString(143, "Update {0} Only"), SQLReader("Name")), Nothing, New System.EventHandler(AddressOf TVSourceSubClick))
                            mnuItem.Tag = SQLReader("Name").ToString
                            mnuItem = Me.cmnuTrayIconUpdateTV.DropDownItems.Add(String.Format(Master.eLang.GetString(143, "Update {0} Only"), SQLReader("Name")), Nothing, New System.EventHandler(AddressOf TVSourceSubClick))
                            mnuItem.Tag = SQLReader("Name").ToString
                        End While
                    End Using
                End Using

                GenreListToolStripComboBox.Items.Clear()
                Me.clbFilterGenres.Items.Clear()
                Dim lGenre() As Object = APIXML.GetGenreList
                GenreListToolStripComboBox.Items.AddRange(lGenre)
                clbFilterGenres.Items.AddRange(lGenre)

                'not technically a menu, but it's a good place to put it
                If ReloadFilters Then

                    clbFilterSource.Items.Clear()
                    Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        SQLNewcommand.CommandText = String.Concat("SELECT Name FROM Sources;")
                        Using SQLReader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                            While SQLReader.Read
                                clbFilterSource.Items.Add(SQLReader("Name"))
                            End While
                        End Using
                    End Using

                    RemoveHandler cbFilterYear.SelectedIndexChanged, AddressOf cbFilterYear_SelectedIndexChanged
                    Me.cbFilterYear.Items.Clear()
                    cbFilterYear.Items.Add(Master.eLang.All)
                    For i As Integer = (Year(Today) + 1) To 1888 Step -1
                        Me.cbFilterYear.Items.Add(i)
                    Next
                    cbFilterYear.SelectedIndex = 0
                    AddHandler cbFilterYear.SelectedIndexChanged, AddressOf cbFilterYear_SelectedIndexChanged

                    RemoveHandler cbFilterYearMod.SelectedIndexChanged, AddressOf cbFilterYearMod_SelectedIndexChanged
                    cbFilterYearMod.SelectedIndex = 0
                    AddHandler cbFilterYearMod.SelectedIndexChanged, AddressOf cbFilterYearMod_SelectedIndexChanged

                    RemoveHandler cbFilterFileSource.SelectedIndexChanged, AddressOf cbFilterFileSource_SelectedIndexChanged
                    cbFilterFileSource.Items.Clear()
                    cbFilterFileSource.Items.Add(Master.eLang.All)
                    cbFilterFileSource.Items.AddRange(APIXML.SourceList.ToArray)
                    cbFilterFileSource.Items.Add(Master.eLang.None)
                    cbFilterFileSource.SelectedIndex = 0
                    AddHandler cbFilterFileSource.SelectedIndexChanged, AddressOf cbFilterFileSource_SelectedIndexChanged

                End If

            End With
            Me.tsbAutoPilot.Enabled = (Me.dgvMediaList.RowCount > 0 AndAlso Me.tabsMain.SelectedIndex = 0)
            Me.cmnuTrayIconScrapeMedia.Enabled = Me.tsbAutoPilot.Enabled
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetsManagerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetsManagerToolStripMenuItem.Click, SetsManagerToolStripMenuItem1.Click
        Me.SetControlsEnabled(False)
        Using dSetsManager As New dlgSetsManager
            dSetsManager.ShowDialog()
        End Using
        Me.SetControlsEnabled(True)
    End Sub

    Private Sub SetStatus(ByVal sText As String)
        Me.tslStatus.Text = sText.Replace("&", "&&")
    End Sub

    Private Function GetStatus() As String
        Return Me.tslStatus.Text.Replace("&&", "&")
    End Function

    Sub HideLoadingSettings()
        If Not Me.pnlLoadingSettings.InvokeRequired Then
            Me.pnlLoadingSettings.Visible = False
        End If
    End Sub
    Sub SettingsShow(ByVal dlg As dlgSettings)
        AddHandler dlg.LoadEnd, AddressOf HideLoadingSettings
        Dim dresult As Structures.SettingsResult = dlg.ShowDialog()
        RemoveHandler dlg.LoadEnd, AddressOf HideLoadingSettings
        Me.SettingsToolStripMenuItem.Enabled = True
        Me.pnlLoadingSettings.Visible = False
        Me.cmnuTrayIconSettings.Enabled = True
        Me.cmnuTrayIconExit.Enabled = True
        If Not dresult.DidCancel Then

            If Not Master.eSettings.DisplayMissingEpisodes Then
                Using SQLTrans As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                    Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                        SQLCommand.CommandText = "DELETE FROM TVEps WHERE Missing = 1"
                        SQLCommand.ExecuteNonQuery()

                        Master.DB.CleanSeasons(True)
                    End Using
                    SQLTrans.Commit()
                End Using
            End If

            Me.SetUp(True)

            If Me.dgvMediaList.RowCount > 0 Then
                Me.dgvMediaList.Columns(4).Visible = Not Master.eSettings.MoviePosterCol
                Me.dgvMediaList.Columns(5).Visible = Not Master.eSettings.MovieFanartCol
                Me.dgvMediaList.Columns(6).Visible = Not Master.eSettings.MovieInfoCol
                Me.dgvMediaList.Columns(7).Visible = Not Master.eSettings.MovieTrailerCol
                Me.dgvMediaList.Columns(8).Visible = Not Master.eSettings.MovieSubCol
                Me.dgvMediaList.Columns(9).Visible = Not Master.eSettings.MovieExtraCol
            End If

            If Me.dgvTVShows.RowCount > 0 Then
                Me.dgvTVShows.Columns(2).Visible = Not Master.eSettings.ShowPosterCol
                Me.dgvTVShows.Columns(3).Visible = Not Master.eSettings.ShowFanartCol
                Me.dgvTVShows.Columns(4).Visible = Not Master.eSettings.ShowNfoCol
            End If

            If Me.dgvTVSeasons.RowCount > 0 Then
                Me.dgvTVSeasons.Columns(3).Visible = Not Master.eSettings.SeasonPosterCol
                Me.dgvTVSeasons.Columns(4).Visible = Not Master.eSettings.SeasonFanartCol
            End If

            If Me.dgvTVEpisodes.RowCount > 0 Then
                Me.dgvTVEpisodes.Columns(4).Visible = Not Master.eSettings.EpisodePosterCol
                Me.dgvTVEpisodes.Columns(5).Visible = Not Master.eSettings.EpisodeFanartCol
                Me.dgvTVEpisodes.Columns(6).Visible = Not Master.eSettings.EpisodeNfoCol
            End If

            'might as well wait for these
            While Me.bwMediaInfo.IsBusy OrElse Me.bwDownloadPic.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If dresult.NeedsRefresh OrElse dresult.NeedsUpdate Then
                If dresult.NeedsRefresh Then
                    If Not Me.fScanner.IsBusy Then
                        While Me.bwLoadInfo.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwCleanDB.IsBusy
                            Application.DoEvents()
                            Threading.Thread.Sleep(50)
                        End While
                        Me.RefreshAllMovies()
                    End If
                End If
                If dresult.NeedsUpdate Then
                    If Not Me.fScanner.IsBusy Then
                        While Me.bwLoadInfo.IsBusy OrElse Me.bwMovieScraper.IsBusy OrElse Me.bwRefreshMovies.IsBusy OrElse Me.bwCleanDB.IsBusy
                            Application.DoEvents()
                            Threading.Thread.Sleep(50)
                        End While
                        Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
                    End If
                End If
            Else
                If Not Me.fScanner.IsBusy AndAlso Not Me.bwLoadInfo.IsBusy AndAlso Not Me.bwMovieScraper.IsBusy AndAlso Not Me.bwRefreshMovies.IsBusy AndAlso Not Me.bwCleanDB.IsBusy Then
                    Me.FillList(0)
                End If
            End If

            Me.SetMenus(True)
            If dresult.NeedsRestart Then
                Using dRestart As New dlgRestart
                    If dRestart.ShowDialog = DialogResult.OK Then
                        Application.Restart()
                    End If
                End Using
            End If
        Else
            Me.SetMenus(False)
            Me.SetControlsEnabled(True)
        End If

    End Sub

    Private Sub SettingsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SettingsToolStripMenuItem.Click, cmnuTrayIconSettings.Click
        Try
            Me.SetControlsEnabled(False)
            Me.pnlLoadingSettings.Visible = True

            Dim dThread As Threading.Thread = New Threading.Thread(AddressOf ShowSettings)
            dThread.SetApartmentState(Threading.ApartmentState.STA)
            dThread.Start()
        Catch ex As Exception
            Me.SettingsToolStripMenuItem.Enabled = True
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetTVCount()
        Dim ShowCount As Integer = 0
        Dim EpCount As Integer = 0

        Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
            SQLCommand.CommandText = "SELECT COUNT(ID) AS COUNT FROM TVShows"
            ShowCount = Convert.ToInt32(SQLCommand.ExecuteScalar)

            SQLCommand.CommandText = "SELECT COUNT(ID) AS COUNT FROM TVEps WHERE Missing = 0"
            EpCount = Convert.ToInt32(SQLCommand.ExecuteScalar)
        End Using

        If ShowCount > 0 Then
            Me.tabTV.Text = String.Format("{0} ({1}/{2})", Master.eLang.GetString(653, "TV"), ShowCount, EpCount)
        Else
            Me.tabTV.Text = Master.eLang.GetString(653, "TV")
        End If
    End Sub

    Private Sub SetUp(ByVal doTheme As Boolean)
        Try
            With Me
                .MinimumSize = New Size(800, 600)

                .btnSortDate.Tag = String.Empty
                .pnlFilterGenre.Tag = String.Empty
                .pnlFilterSource.Tag = String.Empty
                .btnSortTitle.Tag = String.Empty
                .btnIMDBRating.Tag = String.Empty
                .FileToolStripMenuItem.Text = Master.eLang.GetString(1, "&File")
                .ExitToolStripMenuItem.Text = Master.eLang.GetString(2, "E&xit")
                .cmnuTrayIconExit.Text = Master.eLang.GetString(2, "E&xit")
                .EditToolStripMenuItem.Text = Master.eLang.GetString(3, "&Edit")
                .SettingsToolStripMenuItem.Text = Master.eLang.GetString(4, "&Settings...")
                .cmnuTrayIconSettings.Text = Master.eLang.GetString(4, "&Settings...")
                .HelpToolStripMenuItem.Text = Master.eLang.GetString(5, "&Help")
                .AboutToolStripMenuItem.Text = Master.eLang.GetString(6, "&About...")
                .tslLoading.Text = Master.eLang.GetString(7, "Loading Media:")
                .ToolsToolStripMenuItem.Text = Master.eLang.GetString(8, "&Tools")
                .cmnuTrayIconTools.Text = Master.eLang.GetString(8, "&Tools")
                .CleanFoldersToolStripMenuItem.Text = Master.eLang.GetString(9, "&Clean Files")
                .CleanFilesToolStripMenuItem.Text = .CleanFoldersToolStripMenuItem.Text
                .ConvertFileSourceToFolderSourceToolStripMenuItem.Text = Master.eLang.GetString(10, "&Sort Files Into Folders")
                .SortFilesIntoFoldersToolStripMenuItem.Text = .ConvertFileSourceToFolderSourceToolStripMenuItem.Text
                .CopyExistingFanartToBackdropsFolderToolStripMenuItem.Text = Master.eLang.GetString(11, "Copy Existing Fanart To &Backdrops Folder")
                .CopyExistingFanartToBackdropsFolderToolStripMenuItem1.Text = .CopyExistingFanartToBackdropsFolderToolStripMenuItem.Text
                .SetsManagerToolStripMenuItem.Text = Master.eLang.GetString(14, "Sets &Manager")
                .SetsManagerToolStripMenuItem1.Text = .SetsManagerToolStripMenuItem.Text
                .ClearAllCachesToolStripMenuItem.Text = Master.eLang.GetString(17, "Clear &All Caches")
                .ClearAllCachesToolStripMenuItem1.Text = .ClearAllCachesToolStripMenuItem.Text
                .RefreshAllMoviesToolStripMenuItem.Text = Master.eLang.GetString(18, "Re&load All Movies")
                .ReloadAllMoviesToolStripMenuItem.Text = .RefreshAllMoviesToolStripMenuItem.Text
                .lblGFilClose.Text = Master.eLang.GetString(19, "Close")
                .lblSFilClose.Text = Master.eLang.GetString(19, "Close")
                .Label4.Text = Master.eLang.GetString(20, "Genres")
                .Label8.Text = Master.eLang.GetString(602, "Sources")
                .cmnuTitle.Text = Master.eLang.GetString(21, "Title")
                .cmnuRefresh.Text = Master.eLang.GetString(22, "Reload")
                .cmnuMark.Text = Master.eLang.GetString(23, "Mark")
                .cmnuLock.Text = Master.eLang.GetString(24, "Lock")
                .cmnuEditMovie.Text = Master.eLang.GetString(25, "Edit Movie")
                .GenresToolStripMenuItem.Text = Master.eLang.GetString(20, "Genres")
                .LblGenreStripMenuItem2.Text = Master.eLang.GetString(27, ">> Select Genre <<")
                .AddGenreToolStripMenuItem.Text = Master.eLang.GetString(28, "Add")
                .SetGenreToolStripMenuItem.Text = Master.eLang.GetString(29, "Set")
                .RemoveGenreToolStripMenuItem.Text = Master.eLang.GetString(30, "Remove")
                .ScrapingToolStripMenuItem.Text = Master.eLang.GetString(31, "(Re)Scrape Selected Movies")
                .cmnuSearchNew.Text = Master.eLang.GetString(32, "Change Movie")
                .OpenContainingFolderToolStripMenuItem.Text = Master.eLang.GetString(33, "Open Containing Folder")
                .cmnuShowOpenFolder.Text = .OpenContainingFolderToolStripMenuItem.Text
                .cmnuSeasonOpenFolder.Text = .OpenContainingFolderToolStripMenuItem.Text
                .cmnuEpOpenFolder.Text = .OpenContainingFolderToolStripMenuItem.Text
                .RemoveToolStripMenuItem.Text = Master.eLang.GetString(30, "Remove")
                .DeleteMovieToolStripMenuItem.Text = Master.eLang.GetString(34, "Delete Movie")
                .RemoveFromDatabaseToolStripMenuItem.Text = Master.eLang.GetString(646, "Remove From Database")
                .btnMarkAll.Text = Master.eLang.GetString(35, "Mark All")
                .tabMovies.Text = Master.eLang.GetString(36, "Movies")
                .tabTV.Text = Master.eLang.GetString(653, "TV")
                .btnClearFilters.Text = Master.eLang.GetString(37, "Clear Filters")
                .GroupBox3.Text = Master.eLang.GetString(38, "General")
                .chkFilterTolerance.Text = Master.eLang.GetString(39, "Out of Tolerance")
                .chkFilterMissing.Text = Master.eLang.GetString(40, "Missing Items")
                .chkFilterDupe.Text = Master.eLang.GetString(41, "Duplicates")
                .gbSpecific.Text = Master.eLang.GetString(42, "Specific")
                .chkFilterLock.Text = Master.eLang.GetString(43, "Locked")
                .GroupBox2.Text = Master.eLang.GetString(44, "Modifier")
                .rbFilterAnd.Text = Master.eLang.GetString(45, "And")
                .rbFilterOr.Text = Master.eLang.GetString(46, "Or")
                .chkFilterNew.Text = Master.eLang.GetString(47, "New")
                .chkFilterMark.Text = Master.eLang.GetString(48, "Marked")
                .Label5.Text = Master.eLang.GetString(49, "Year:")
                .Label2.Text = Master.eLang.GetString(50, "Source:")
                .Label3.Text = Master.eLang.GetString(51, "Genre:")
                .lblFilter.Text = Master.eLang.GetString(52, "Filters")
                .lblCanceling.Text = Master.eLang.GetString(53, "Canceling Scraper...")
                .btnCancel.Text = Master.eLang.GetString(54, "Cancel Scraper")
                .lblCertsHeader.Text = Master.eLang.GetString(56, "Certification(s)")
                .lblReleaseDateHeader.Text = Master.eLang.GetString(57, "Release Date")
                .btnMetaDataRefresh.Text = Master.eLang.GetString(58, "Refresh")
                .lblMetaDataHeader.Text = Master.eLang.GetString(59, "Meta Data")
                .lblFilePathHeader.Text = Master.eLang.GetString(60, "File Path")
                .lblIMDBHeader.Text = Master.eLang.GetString(61, "IMDB ID")
                .lblDirectorHeader.Text = Master.eLang.GetString(62, "Director")
                .lblActorsHeader.Text = Master.eLang.GetString(63, "Cast")
                .lblOutlineHeader.Text = Master.eLang.GetString(64, "Plot Outline")
                .lblPlotHeader.Text = Master.eLang.GetString(65, "Plot")
                .lblInfoPanelHeader.Text = Master.eLang.GetString(66, "Info")
                .tsbAutoPilot.Text = Master.eLang.GetString(67, "Scrape Media")
                .cmnuTrayIconScrapeMedia.Text = Master.eLang.GetString(67, "Scrape Media")
                .FullToolStripMenuItem.Text = Master.eLang.GetString(68, "All Movies")
                .FullAutoToolStripMenuItem.Text = Master.eLang.GetString(69, "Automatic (Force Best Match)")
                .mnuAllAutoAll.Text = Master.eLang.GetString(70, "All Items")
                .mnuAllAutoNfo.Text = Master.eLang.GetString(71, "NFO Only")
                .mnuAllAutoPoster.Text = Master.eLang.GetString(72, "Poster Only")
                .mnuAllAutoFanart.Text = Master.eLang.GetString(73, "Fanart Only")
                .mnuAllAutoExtra.Text = Master.eLang.GetString(74, "Extrathumbs Only")
                .mnuAllAutoTrailer.Text = Master.eLang.GetString(75, "Trailer Only")
                .mnuAllAutoMI.Text = Master.eLang.GetString(76, "Meta Data Only")
                .FullAskToolStripMenuItem.Text = Master.eLang.GetString(77, "Ask (Require Input If No Exact Match)")
                .mnuAllAskAll.Text = mnuAllAutoAll.Text
                .mnuAllAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuAllAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuAllAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuAllAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuAllAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuAllAskMI.Text = .mnuAllAutoMI.Text
                .UpdateOnlyToolStripMenuItem.Text = Master.eLang.GetString(78, "Movies Missing Items")
                .UpdateAutoToolStripMenuItem.Text = .FullAutoToolStripMenuItem.Text
                .mnuMissAutoAll.Text = .mnuAllAutoAll.Text
                .mnuMissAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuMissAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuMissAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuMissAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuMissAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .UpdateAskToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuMissAskAll.Text = .mnuAllAutoAll.Text
                .mnuMissAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuMissAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuMissAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuMissAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuMissAskTrailer.Text = .mnuAllAutoTrailer.Text
                .NewMoviesToolStripMenuItem.Text = Master.eLang.GetString(79, "New Movies")
                .AutomaticForceBestMatchToolStripMenuItem.Text = .FullAutoToolStripMenuItem.Text
                .mnuNewAutoAll.Text = .mnuAllAutoAll.Text
                .mnuNewAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuNewAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuNewAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuNewAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuNewAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuNewAutoMI.Text = .mnuAllAutoMI.Text
                .AskRequireInputToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuNewAskAll.Text = .mnuAllAutoAll.Text
                .mnuNewAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuNewAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuNewAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuNewAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuNewAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuNewAskMI.Text = .mnuAllAutoMI.Text
                .MarkedMoviesToolStripMenuItem.Text = Master.eLang.GetString(80, "Marked Movies")
                .AutomaticForceBestMatchToolStripMenuItem1.Text = .FullAutoToolStripMenuItem.Text
                .mnuMarkAutoAll.Text = .mnuAllAutoAll.Text
                .mnuMarkAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuMarkAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuMarkAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuMarkAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuMarkAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuMarkAutoMI.Text = .mnuAllAutoMI.Text
                .AskRequireInputIfNoExactMatchToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuMarkAskAll.Text = .mnuAllAutoAll.Text
                .mnuMarkAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuMarkAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuMarkAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuMarkAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuMarkAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuMarkAskMI.Text = .mnuAllAutoMI.Text
                .CurrentFilterToolStripMenuItem.Text = Master.eLang.GetString(624, "Current Filter")
                .AutomaticForceBestMatchToolStripMenuItem2.Text = .FullAutoToolStripMenuItem.Text
                .mnuFilterAutoAll.Text = .mnuAllAutoAll.Text
                .mnuFilterAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuFilterAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuFilterAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuFilterAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuFilterAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuFilterAutoMI.Text = .mnuAllAutoMI.Text
                .AskRequireInputIfNoExactMatchToolStripMenuItem1.Text = .FullAskToolStripMenuItem.Text
                .mnuFilterAskAll.Text = .mnuAllAutoAll.Text
                .mnuFilterAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuFilterAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuFilterAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuFilterAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuFilterAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuFilterAskMI.Text = .mnuAllAutoMI.Text
                .TrayFullToolStripMenuItem.Text = .FullToolStripMenuItem.Text
                .TrayFullAutoToolStripMenuItem.Text = .FullAutoToolStripMenuItem.Text
                .mnuTrayAllAutoAll.Text = .mnuAllAutoAll.Text
                .mnuTrayAllAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayAllAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayAllAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayAllAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayAllAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayAllAutoMI.Text = .mnuAllAutoMI.Text
                .TrayFullAskToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuTrayAllAskAll.Text = mnuAllAutoAll.Text
                .mnuTrayAllAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayAllAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayAllAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayAllAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayAllAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayAllAskMI.Text = .mnuAllAutoMI.Text
                .UpdateOnlyToolStripMenuItem.Text = .UpdateOnlyToolStripMenuItem.Text
                .TrayUpdateAutoToolStripMenuItem.Text = .FullAutoToolStripMenuItem.Text
                .mnuTrayMissAutoAll.Text = .mnuAllAutoAll.Text
                .mnuTrayMissAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayMissAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayMissAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayMissAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayMissAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .UpdateAskToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuTrayMissAskAll.Text = .mnuAllAutoAll.Text
                .mnuTrayMissAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayMissAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayMissAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayMissAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayMissAskTrailer.Text = .mnuAllAutoTrailer.Text
                .TrayNewMoviesToolStripMenuItem.Text = .NewMoviesToolStripMenuItem.Text
                .TrayAutomaticForceBestMatchToolStripMenuItem.Text = .FullAutoToolStripMenuItem.Text
                .mnuTrayNewAutoAll.Text = .mnuAllAutoAll.Text
                .mnuTrayNewAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayNewAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayNewAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayNewAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayNewAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayNewAutoMI.Text = .mnuAllAutoMI.Text
                .TrayAskRequireInputToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuTrayNewAskAll.Text = .mnuAllAutoAll.Text
                .mnuTrayNewAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayNewAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayNewAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayNewAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayNewAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayNewAskMI.Text = .mnuAllAutoMI.Text
                .TrayMarkedMoviesToolStripMenuItem.Text = .MarkedMoviesToolStripMenuItem.Text
                .TrayAutomaticForceBestMatchToolStripMenuItem1.Text = .FullAutoToolStripMenuItem.Text
                .mnuTrayMarkAutoAll.Text = .mnuAllAutoAll.Text
                .mnuTrayMarkAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayMarkAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayMarkAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayMarkAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayMarkAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayMarkAutoMI.Text = .mnuAllAutoMI.Text
                .TrayAskRequireInputIfNoExactMatchToolStripMenuItem.Text = .FullAskToolStripMenuItem.Text
                .mnuTrayMarkAskAll.Text = .mnuAllAutoAll.Text
                .mnuTrayMarkAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayMarkAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayMarkAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayMarkAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayMarkAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayMarkAskMI.Text = .mnuAllAutoMI.Text
                .TrayCurrentFilterToolStripMenuItem.Text = .CurrentFilterToolStripMenuItem.Text
                .TrayAutomaticForceBestMatchToolStripMenuItem2.Text = .FullAutoToolStripMenuItem.Text
                .mnuTrayFilterAutoAll.Text = .mnuAllAutoAll.Text
                .mnuTrayFilterAutoNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayFilterAutoPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayFilterAutoFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayFilterAutoExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayFilterAutoTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayFilterAutoMI.Text = .mnuAllAutoMI.Text
                .TrayAskRequireInputIfNoExactMatchToolStripMenuItem1.Text = .FullAskToolStripMenuItem.Text
                .mnuTrayFilterAskAll.Text = .mnuAllAutoAll.Text
                .mnuTrayFilterAskNfo.Text = .mnuAllAutoNfo.Text
                .mnuTrayFilterAskPoster.Text = .mnuAllAutoPoster.Text
                .mnuTrayFilterAskFanart.Text = .mnuAllAutoFanart.Text
                .mnuTrayFilterAskExtra.Text = .mnuAllAutoExtra.Text
                .mnuTrayFilterAskTrailer.Text = .mnuAllAutoTrailer.Text
                .mnuTrayFilterAskMI.Text = .mnuAllAutoMI.Text
                .mnuMoviesUpdate.Text = Master.eLang.GetString(36, "Movies")
                .mnuTVShowUpdate.Text = Master.eLang.GetString(698, "TV Shows")
                .cmnuEditEpisode.Text = Master.eLang.GetString(656, "Edit Episode")
                .cmnuEditShow.Text = Master.eLang.GetString(663, "Edit Show")
                .CustomUpdaterToolStripMenuItem.Text = Master.eLang.GetString(81, "Custom Scraper...")
                .TrayCustomUpdaterToolStripMenuItem.Text = .CustomUpdaterToolStripMenuItem.Text
                .tsbRefreshMedia.Text = Master.eLang.GetString(82, "Update Library")
                .cmnuTrayIconUpdateMedia.Text = Master.eLang.GetString(82, "Update Library")
                .tsbMediaCenters.Text = Master.eLang.GetString(83, "Media Centers")
                .cmnuTrayIconMediaCenters.Text = .tsbMediaCenters.Text
                .Label6.Text = Master.eLang.GetString(579, "File Source:")
                .GroupBox1.Text = Master.eLang.GetString(600, "Extra Sorting")
                .btnSortDate.Text = Master.eLang.GetString(601, "Date Added")
                .cmnuMetaData.Text = Master.eLang.GetString(603, "Edit Meta Data")
                .btnSortTitle.Text = Master.eLang.GetString(642, "Sort Title")
                .btnIMDBRating.Text = Master.eLang.GetString(400, "Rating")
                .DonateToolStripMenuItem.Text = Master.eLang.GetString(708, "Donate")
                .CleanDatabaseToolStripMenuItem.Text = Master.eLang.GetString(709, "Clean Database")
                .cmnuReloadShow.Text = Master.eLang.GetString(22, "Reload")
                .cmnuMarkShow.Text = Master.eLang.GetString(23, "Mark")
                .cmnuLockShow.Text = Master.eLang.GetString(24, "Lock")
                .cmnuEditShow.Text = Master.eLang.GetString(663, "Edit Show")
                .cmnuRescrapeShow.Text = Master.eLang.GetString(766, "(Re)Scrape Show")
                .cmnuChangeShow.Text = Master.eLang.GetString(767, "Change Show")
                .RemoveShowToolStripMenuItem.Text = Master.eLang.GetString(30, "Remove")
                .cmnuRemoveTVShow.Text = Master.eLang.GetString(646, "Remove from Database")
                .cmnuDeleteTVShow.Text = Master.eLang.GetString(768, "Delete TV Show")
                .cmnuReloadSeason.Text = Master.eLang.GetString(22, "Reload")
                .cmnuMarkSeason.Text = Master.eLang.GetString(23, "Mark")
                .cmnuLockSeason.Text = Master.eLang.GetString(24, "Lock")
                .cmnuSeasonChangeImages.Text = Master.eLang.GetString(770, "Change Images")
                .cmnuSeasonRescrape.Text = Master.eLang.GetString(146, "(Re)Scrape Season")
                .cmnuSeasonRemove.Text = Master.eLang.GetString(30, "Remove")
                .cmnuRemoveSeasonFromDB.Text = Master.eLang.GetString(646, "Remove from Database")
                .cmnuDeleteSeason.Text = Master.eLang.GetString(771, "Delete Season")
                .cmnuReloadEp.Text = Master.eLang.GetString(22, "Reload")
                .cmnuMarkEp.Text = Master.eLang.GetString(23, "Mark")
                .cmnuLockEp.Text = Master.eLang.GetString(24, "Lock")
                .cmnuEditEpisode.Text = Master.eLang.GetString(656, "Edit Episode")
                .cmnuRescrapeEp.Text = Master.eLang.GetString(147, "(Re)Scrape Episode")
                .cmnuChangeEp.Text = Master.eLang.GetString(772, "Change Episode")
                .RemoveEpToolStripMenuItem.Text = Master.eLang.GetString(30, "Remove")
                .cmnuRemoveTVEp.Text = Master.eLang.GetString(646, "Remove from Database")
                .cmnuDeleteTVEp.Text = Master.eLang.GetString(773, "Delete Episode")
                .DonateToolStripMenuItem.Text = Master.eLang.GetString(792, "Donate")
                .VersionsToolStripMenuItem.Text = Master.eLang.GetString(793, "&Versions...")
                .CheckUpdatesToolStripMenuItem.Text = Master.eLang.GetString(850, "&Check For Updates...")
                .Label7.Text = Master.eLang.GetString(484, "Loading Settings...")
                .cmnuRescrape.Text = Master.eLang.GetString(163, "(Re)Scrape Movie")
				.ScrapingToolStripMenuItem.Text = Master.eLang.GetString(164, "(Re)Scrape Selected Movies")
				.WikiStripMenuItem.Text = Master.eLang.GetString(869, "EmberMM.com &Wiki...")

                Dim TT As ToolTip = New System.Windows.Forms.ToolTip(.components)
                .tsbAutoPilot.ToolTipText = Master.eLang.GetString(84, "Scrape/download data from the internet for multiple movies.")
                .tsbRefreshMedia.ToolTipText = Master.eLang.GetString(85, "Scans sources for new content and cleans database.")
                TT.SetToolTip(.btnMarkAll, Master.eLang.GetString(87, "Mark or Unmark all movies in the list."))
                TT.SetToolTip(.txtSearch, Master.eLang.GetString(88, "Search the movie titles by entering text here."))
                TT.SetToolTip(.btnPlay, Master.eLang.GetString(89, "Play the movie file with the system default media player."))
                TT.SetToolTip(.btnMetaDataRefresh, Master.eLang.GetString(90, "Rescan and save the meta data for the selected movie."))
                TT.SetToolTip(.chkFilterDupe, Master.eLang.GetString(91, "Display only movies that have duplicate IMDB IDs."))
                TT.SetToolTip(.chkFilterTolerance, Master.eLang.GetString(92, "Display only movies whose title matching is out of tolerance."))
                TT.SetToolTip(.chkFilterMissing, Master.eLang.GetString(93, "Display only movies that have items missing."))
                TT.SetToolTip(.chkFilterNew, Master.eLang.GetString(94, "Display only new movies."))
                TT.SetToolTip(.chkFilterMark, Master.eLang.GetString(95, "Display only marked movies."))
                TT.SetToolTip(.chkFilterLock, Master.eLang.GetString(96, "Display only locked movies."))
                TT.SetToolTip(.txtFilterSource, Master.eLang.GetString(97, "Display only movies from the selected source."))
                TT.SetToolTip(.cbFilterFileSource, Master.eLang.GetString(580, "Display only movies from the selected file source."))
                TT.Active = True

                .cbSearch.Items.Clear()
                .cbSearch.Items.AddRange(New Object() {Master.eLang.GetString(21, "Title"), Master.eLang.GetString(100, "Actor"), Master.eLang.GetString(62, "Director")})

                If doTheme Then
                    Me.tTheme = New Theming
                    .ApplyTheme(If(Me.tabsMain.SelectedIndex = 0, Theming.ThemeType.Movies, Theming.ThemeType.Show))
                End If

            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ShowNoInfo(ByVal ShowIt As Boolean, Optional ByVal tType As Integer = 0)
        If ShowIt Then
            Select Case tType
                Case 0
                    Me.Label1.Text = Master.eLang.GetString(55, "No Information is Available for This Movie")
                    If Not Me.currThemeType = Theming.ThemeType.Movies Then Me.ApplyTheme(Theming.ThemeType.Movies)
                Case 1
                    Me.Label1.Text = Master.eLang.GetString(651, "No Information is Available for This Show")
                    If Not Me.currThemeType = Theming.ThemeType.Show Then Me.ApplyTheme(Theming.ThemeType.Show)
                Case 2
                    Me.Label1.Text = Master.eLang.GetString(652, "No Information is Available for This Episode")
                    If Not Me.currThemeType = Theming.ThemeType.Episode Then Me.ApplyTheme(Theming.ThemeType.Episode)
            End Select
        End If

        Me.pnlNoInfo.Visible = ShowIt
    End Sub

    Private Sub ShowSettings()
        Using dSettings As New dlgSettings
            Me.Invoke(New MySettingsShow(AddressOf SettingsShow), dSettings)
        End Using
    End Sub

    Private Sub SourceSubClick(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim SourceName As String = DirectCast(sender, ToolStripItem).Tag.ToString
        Me.LoadMedia(New Structures.Scans With {.Movies = True}, SourceName)
    End Sub

    Private Sub tabsMain_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tabsMain.SelectedIndexChanged
        Me.ClearInfo()
        Me.ShowNoInfo(False)
        ModulesManager.Instance.RuntimeObjects.MediaTabSelected = tabsMain.SelectedIndex
        Select Case tabsMain.SelectedIndex
            Case 0
                Me.ToolsToolStripMenuItem.Enabled = True
                Me.cmnuTrayIconTools.Enabled = True
                Me.pnlFilter.Visible = True
                Me.pnlListTop.Height = 56
                Me.btnMarkAll.Visible = True
                Me.scTV.Visible = False
                Me.dgvMediaList.Visible = True
                Me.ApplyTheme(Theming.ThemeType.Movies)
                If Me.bwLoadEpInfo.IsBusy Then Me.bwLoadEpInfo.CancelAsync()
                If Me.bwLoadSeasonInfo.IsBusy Then Me.bwLoadSeasonInfo.CancelAsync()
                If Me.bwLoadShowInfo.IsBusy Then Me.bwLoadShowInfo.CancelAsync()
                If Me.bwDownloadPic.IsBusy Then Me.bwDownloadPic.CancelAsync()
                If Me.dgvMediaList.RowCount > 0 Then
                    Me.prevRow = -1

                    Me.dgvMediaList.CurrentCell = Nothing
                    Me.dgvMediaList.ClearSelection()
                    Me.dgvMediaList.Rows(0).Selected = True
                    Me.dgvMediaList.CurrentCell = Me.dgvMediaList.Rows(0).Cells(3)

                    Me.dgvMediaList.Focus()
                End If
            Case 1
                Me.ToolsToolStripMenuItem.Enabled = True
                Me.cmnuTrayIconTools.Enabled = False
                Me.tsbAutoPilot.Enabled = False
                Me.cmnuTrayIconScrapeMedia.Enabled = False
                Me.dgvMediaList.Visible = False
                Me.pnlFilter.Visible = False
                Me.pnlListTop.Height = 23
                Me.btnMarkAll.Visible = False
                Me.scTV.Visible = True
                Me.ApplyTheme(Theming.ThemeType.Show)
                If Me.bwLoadInfo.IsBusy Then Me.bwLoadInfo.CancelAsync()
                If Me.bwDownloadPic.IsBusy Then Me.bwDownloadPic.CancelAsync()
                If Me.dgvTVShows.RowCount > 0 Then
                    Me.prevShowRow = -1
                    Me.currList = 0

                    Me.dgvTVShows.CurrentCell = Nothing
                    Me.dgvTVShows.ClearSelection()
                    Me.dgvTVShows.Rows(0).Selected = True
                    Me.dgvTVShows.CurrentCell = Me.dgvTVShows.Rows(0).Cells(1)

                    Me.dgvTVShows.Focus()

                End If
        End Select
    End Sub

    Private Sub tmrAni_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrAni.Tick
        '//
        ' Just some crappy animation to make the GUI slightly more interesting
        '\\

        Try
            If Master.eSettings.InfoPanelAnim Then
                If Me.aniRaise Then
                    Me.pnlInfoPanel.Height += 5
                Else
                    Me.pnlInfoPanel.Height -= 5
                End If
            Else
                Select Case If(Me.tabsMain.SelectedIndex = 0, Me.aniType, Me.aniShowType)
                    Case 0
                        Me.pnlInfoPanel.Height = 25

                    Case 1
                        Me.pnlInfoPanel.Height = Me.IPMid

                    Case 2
                        Me.pnlInfoPanel.Height = Me.IPUp

                End Select
            End If

            Me.MoveGenres()
            Me.MoveMPAA()

            Dim aType As Integer = If(Me.tabsMain.SelectedIndex = 0, Me.aniType, Me.aniShowType)
            Select Case aType
                Case 0
                    If Me.pnlInfoPanel.Height = 25 Then
                        Me.tmrAni.Stop()
                        Me.btnDown.Enabled = False
                        Me.btnMid.Enabled = True
                        Me.btnUp.Enabled = True
                    End If
                Case 1
                    If Me.pnlInfoPanel.Height = Me.IPMid Then
                        Me.tmrAni.Stop()
                        Me.btnMid.Enabled = False
                        Me.btnDown.Enabled = True
                        Me.btnUp.Enabled = True
                    End If
                Case 2
                    If Me.pnlInfoPanel.Height = Me.IPUp Then
                        Me.tmrAni.Stop()
                        Me.btnUp.Enabled = False
                        Me.btnDown.Enabled = True
                        Me.btnMid.Enabled = True
                    End If
            End Select

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub tmrFilterAni_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrFilterAni.Tick
        '//
        ' Just some crappy animation to make the GUI slightly more interesting
        '\\

        Try

            Dim pHeight As Integer = Functions.Quantize(Me.gbSpecific.Height + Me.lblFilter.Height + 15, 5)

            If Master.eSettings.InfoPanelAnim Then
                If Me.aniFilterRaise Then
                    Me.pnlFilter.Height += 5
                Else
                    Me.pnlFilter.Height -= 5
                End If
            Else
                If Me.aniFilterRaise Then
                    Me.pnlFilter.Height = pHeight
                Else
                    Me.pnlFilter.Height = 25
                End If
            End If

            If Me.pnlFilter.Height = 25 Then
                Me.tmrFilterAni.Stop()
                Me.btnFilterUp.Enabled = True
                Me.btnFilterDown.Enabled = False
            ElseIf Me.pnlFilter.Height = pHeight Then
                Me.tmrFilterAni.Stop()
                Me.btnFilterUp.Enabled = False
                Me.btnFilterDown.Enabled = True
            End If

            Me.dgvMediaList.Invalidate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub tmrLoadEp_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrLoadEp.Tick
        Me.tmrWaitEp.Stop()
        Me.tmrLoadEp.Stop()
        Try

            If Me.dgvTVEpisodes.SelectedRows.Count > 0 Then

                If Me.dgvTVEpisodes.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVEpisodes.SelectedRows.Count))
                ElseIf Me.dgvTVEpisodes.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVEpisodes.SelectedRows(0).Cells(3).Value.ToString)
                End If

                Me.SelectEpisodeRow(Me.dgvTVEpisodes.SelectedRows(0).Index)
            End If
        Catch
        End Try
    End Sub

    Private Sub tmrLoadSeason_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrLoadSeason.Tick
        Me.tmrWaitSeason.Stop()
        Me.tmrLoadSeason.Stop()
        Try
            If Me.dgvTVSeasons.SelectedRows.Count > 0 Then

                If Me.dgvTVSeasons.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVSeasons.SelectedRows.Count))
                ElseIf Me.dgvMediaList.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVSeasons.SelectedRows(0).Cells(1).Value.ToString)
                End If

                Me.SelectSeasonRow(Me.dgvTVSeasons.SelectedRows(0).Index)
            End If
        Catch
        End Try
    End Sub

    Private Sub tmrLoadShow_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrLoadShow.Tick
        Me.tmrWaitShow.Stop()
        Me.tmrLoadShow.Stop()
        Try
            If Me.dgvTVShows.SelectedRows.Count > 0 Then

                If Me.dgvTVShows.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvTVShows.SelectedRows.Count))
                ElseIf Me.dgvTVShows.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvTVShows.SelectedRows(0).Cells(1).Value.ToString)
                End If

                Me.SelectShowRow(Me.dgvTVShows.SelectedRows(0).Index)
            End If
        Catch
        End Try
    End Sub

    Private Sub tmrLoad_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrLoad.Tick
        Me.tmrWait.Stop()
        Me.tmrLoad.Stop()
        Try
            If Me.dgvMediaList.SelectedRows.Count > 0 Then

                If Me.dgvMediaList.SelectedRows.Count > 1 Then
                    Me.SetStatus(String.Format(Master.eLang.GetString(627, "Selected Items: {0}"), Me.dgvMediaList.SelectedRows.Count))
                ElseIf Me.dgvMediaList.SelectedRows.Count = 1 Then
                    Me.SetStatus(Me.dgvMediaList.SelectedRows(0).Cells(1).Value.ToString)
                End If

                Me.SelectRow(Me.dgvMediaList.SelectedRows(0).Index)
            End If
        Catch
        End Try
    End Sub

    Private Sub tmrSearchWait_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSearchWait.Tick
        Me.tmrSearch.Enabled = False
        If Me.prevText = Me.currText Then
            Me.tmrSearch.Enabled = True
        Else
            Me.prevText = Me.currText
        End If
    End Sub

    Private Sub tmrSearch_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSearch.Tick
        Me.tmrSearchWait.Enabled = False
        Me.tmrSearch.Enabled = False
        bDoingSearch = True
        Try
            If Not String.IsNullOrEmpty(Me.txtSearch.Text) Then
                Me.FilterArray.Remove(Me.filSearch)
                Me.filSearch = String.Empty

                Select Case Me.cbSearch.Text
                    Case Master.eLang.GetString(21, "Title")
                        Me.filSearch = String.Concat("ListTitle LIKE '%", Me.txtSearch.Text, "%'")
                        Me.FilterArray.Add(Me.filSearch)
                    Case Master.eLang.GetString(100, "Actor")
                        Me.filSearch = Me.txtSearch.Text
                    Case Master.eLang.GetString(62, "Director")
                        Me.filSearch = String.Concat("Director LIKE '%", Me.txtSearch.Text, "%'")
                        Me.FilterArray.Add(Me.filSearch)
                End Select

                Me.RunFilter(Me.cbSearch.Text = Master.eLang.GetString(100, "Actor"))

            Else
                If Not String.IsNullOrEmpty(Me.filSearch) Then
                    Me.FilterArray.Remove(Me.filSearch)
                    Me.filSearch = String.Empty
                End If
                Me.RunFilter(True)
            End If
        Catch
        End Try
    End Sub

    Private Sub tmrWaitEp_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrWaitEp.Tick
        Me.tmrLoadSeason.Stop()
        Me.tmrLoadShow.Stop()
        Me.tmrWaitSeason.Stop()
        Me.tmrWaitShow.Stop()

        If Not Me.prevEpRow = Me.currEpRow Then
            Me.prevEpRow = Me.currEpRow
            Me.tmrWaitEp.Stop()
            Me.tmrLoadEp.Start()
        Else
            Me.tmrLoadEp.Stop()
            Me.tmrWaitEp.Stop()
        End If
    End Sub

    Private Sub tmrWaitSeason_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrWaitSeason.Tick
        Me.tmrLoadShow.Stop()
        Me.tmrLoadEp.Stop()
        Me.tmrWaitShow.Stop()
        Me.tmrWaitEp.Stop()

        If Not Me.prevSeasonRow = Me.currSeasonRow Then
            Me.prevSeasonRow = Me.currSeasonRow
            Me.tmrWaitSeason.Stop()
            Me.tmrLoadSeason.Start()
        Else
            Me.tmrLoadSeason.Stop()
            Me.tmrWaitSeason.Stop()
        End If
    End Sub

    Private Sub tmrWaitShow_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrWaitShow.Tick
        Me.tmrLoadSeason.Stop()
        Me.tmrLoadEp.Stop()
        Me.tmrWaitSeason.Stop()
        Me.tmrWaitEp.Stop()

        If Not Me.prevShowRow = Me.currShowRow Then
            Me.prevShowRow = Me.currShowRow
            Me.tmrWaitShow.Stop()
            Me.tmrLoadShow.Start()
        Else
            Me.tmrLoadShow.Stop()
            Me.tmrWaitShow.Stop()
        End If
    End Sub

    Private Sub tmrWait_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrWait.Tick
        If Not Me.prevRow = Me.currRow Then
            Me.prevRow = Me.currRow
            Me.tmrWait.Stop()
            Me.tmrLoad.Start()
        Else
            Me.tmrLoad.Stop()
            Me.tmrWait.Stop()
        End If
    End Sub

    Private Sub ToolStripAskMenuItem19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripAskMenuItem19.Click
        Functions.SetScraperMod(Enums.ModType.Trailer, True)
        MovieScrapeData(True, Enums.ScrapeType.FullAsk, Master.DefaultOptions)
    End Sub

    Private Sub tsbRefreshMedia_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbRefreshMedia.ButtonClick
        Me.LoadMedia(New Structures.Scans With {.Movies = True, .TV = True})
    End Sub

    Private Sub TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)
        Select Case eType
            Case Enums.TVScraperEventType.LoadingEpisodes
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Me.tslLoading.Text = Master.eLang.GetString(756, "Loading All Episodes:")
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True
            Case Enums.TVScraperEventType.SavingStarted
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Me.tslLoading.Text = Master.eLang.GetString(757, "Saving All Images:")
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True
            Case Enums.TVScraperEventType.ScraperDone
                Me.RefreshShow(Master.currShow.ShowID, False, False, False, True)

                Me.tspbLoading.Visible = False
                Me.tslLoading.Visible = False
                'Me.tslStatus.Visible = False

                Me.SetControlsEnabled(True)

            Case Enums.TVScraperEventType.Searching
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Me.tslLoading.Text = Master.eLang.GetString(758, "Searching theTVDB:")
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True
            Case Enums.TVScraperEventType.SelectImages
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Me.tslLoading.Text = Master.eLang.GetString(759, "Select Images:")
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True
            Case Enums.TVScraperEventType.StartingDownload
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Me.tslLoading.Text = Master.eLang.GetString(760, "Downloading Show Zip:")
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True
            Case Enums.TVScraperEventType.SaveAuto
                Me.tspbLoading.Style = ProgressBarStyle.Marquee
                Select Case iProgress
                    Case 0 ' show
                        Me.SetShowListItemAfterEdit(Convert.ToInt32(Master.currShow.ShowID), Me.dgvTVShows.SelectedRows(0).Index)
                        ModulesManager.Instance.TVSaveImages()
                End Select

            Case Enums.TVScraperEventType.Verifying
                Me.tspbLoading.Style = ProgressBarStyle.Marquee

                Select Case iProgress
                    Case 0 ' show
                        Me.tslLoading.Text = Master.eLang.GetString(761, "Verifying TV Show:")
                        Me.tspbLoading.Visible = True
                        Me.tslLoading.Visible = True
                        Using dEditShow As New dlgEditShow
                            If dEditShow.ShowDialog() = DialogResult.OK Then
                                Me.SetShowListItemAfterEdit(Convert.ToInt32(Master.currShow.ShowID), Me.dgvTVShows.SelectedRows(0).Index)
                                ModulesManager.Instance.TVSaveImages()
                            Else
                                Me.tspbLoading.Visible = False
                                Me.tslLoading.Visible = False

                                Me.LoadShowInfo(Convert.ToInt32(Master.currShow.ShowID))

                                Me.SetControlsEnabled(True)
                            End If
                        End Using
                    Case 1 ' episode
                        Me.tslLoading.Text = Master.eLang.GetString(762, "Verifying TV Episode:")
                        Me.tspbLoading.Visible = True
                        Me.tslLoading.Visible = True
                        Using dEditEp As New dlgEditEpisode
                            AddHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEp.GenericRunCallBack
                            If dEditEp.ShowDialog = DialogResult.OK Then
                                Me.RefreshEpisode(Master.currShow.EpID)
                            End If
                            RemoveHandler ModulesManager.Instance.GenericEvent, AddressOf dEditEp.GenericRunCallBack
                        End Using
                        Me.tspbLoading.Visible = False
                        Me.tslLoading.Visible = False
                        Me.SetControlsEnabled(True)
                End Select

            Case Enums.TVScraperEventType.Progress
                Select Case Parameter.ToString
                    Case "max"
                        Me.tspbLoading.Value = 0
                        Me.tspbLoading.Style = ProgressBarStyle.Continuous
                        Me.tspbLoading.Maximum = iProgress
                    Case "progress"
                        Me.tspbLoading.Value = iProgress
                End Select
                Me.tspbLoading.Visible = True
                Me.tslLoading.Visible = True

            Case Enums.TVScraperEventType.Cancelled
                Me.tspbLoading.Visible = False
                Me.tslLoading.Visible = False

                Me.LoadShowInfo(Convert.ToInt32(Master.currShow.ShowID))

                Me.SetControlsEnabled(True)
        End Select
    End Sub

    Private Sub TVSourceSubClick(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim SourceName As String = DirectCast(sender, ToolStripItem).Tag.ToString
        Me.LoadMedia(New Structures.Scans With {.TV = True}, SourceName)
    End Sub

    Private Sub txtFilterGenre_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtFilterGenre.Click
        Me.pnlFilterGenre.Location = New Point(Me.gbSpecific.Left + Me.txtFilterGenre.Left, (Me.pnlFilter.Top + Me.txtFilterGenre.Top + Me.gbSpecific.Top) - Me.pnlFilterGenre.Height)
        If Me.pnlFilterGenre.Visible Then
            Me.pnlFilterGenre.Visible = False
        ElseIf Not Me.pnlFilterGenre.Tag.ToString = "NO" Then
            Me.pnlFilterGenre.Tag = String.Empty
            Me.pnlFilterGenre.Visible = True
            Me.clbFilterGenres.Focus()
        Else
            Me.pnlFilterGenre.Tag = String.Empty
        End If
    End Sub

    Private Sub txtFilterSource_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtFilterSource.Click
        Me.pnlFilterSource.Location = New Point(Me.gbSpecific.Left + Me.txtFilterSource.Left, (Me.pnlFilter.Top + Me.txtFilterSource.Top + Me.gbSpecific.Top) - Me.pnlFilterSource.Height)
        If Me.pnlFilterSource.Visible Then
            Me.pnlFilterSource.Visible = False
        ElseIf Not Me.pnlFilterSource.Tag.ToString = "NO" Then
            Me.pnlFilterSource.Tag = String.Empty
            Me.pnlFilterSource.Visible = True
            Me.clbFilterSource.Focus()
        Else
            Me.pnlFilterSource.Tag = String.Empty
        End If
    End Sub

    Private Sub txtSearch_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSearch.KeyPress
        e.Handled = StringUtils.AlphaNumericOnly(e.KeyChar, True)
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            Me.dgvMediaList.Focus()
        End If
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        Me.currText = Me.txtSearch.Text

        Me.tmrSearchWait.Enabled = False
        Me.tmrSearch.Enabled = False
        Me.tmrSearchWait.Enabled = True
    End Sub

    Private Sub VersionsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VersionsToolStripMenuItem.Click
        ModulesManager.Instance.GetVersions()
    End Sub

    Private Sub WikiStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WikiStripMenuItem.Click
        If Master.isWindows Then
            Process.Start("https://sourceforge.net/apps/trac/emm-r/wiki")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "https://sourceforge.net/apps/trac/emm-r/wiki"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub tmrAppExit_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrAppExit.Tick
        tmrAppExit.Enabled = False
        Me.Close()
    End Sub

    Private Sub CheckUpdatesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckUpdatesToolStripMenuItem.Click
        If Functions.CheckNeedUpdate() Then
            Using dNewVer As New dlgNewVersion
                If dNewVer.ShowDialog() = DialogResult.Abort Then
                    tmrAppExit.Enabled = True
                    CloseApp = True
                End If
            End Using
        Else
            MsgBox(Master.eLang.GetString(851, "No Updates at this time"), MsgBoxStyle.OkOnly, "Updates")
        End If
    End Sub

    Private Sub lblIMDBHeader_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblIMDBHeader.Click
        If Not String.IsNullOrEmpty(txtIMDBID.Text) Then
            If Master.isWindows Then
                Process.Start(String.Format("http://www.imdb.com/title/tt{0}/", txtIMDBID.Text))
            Else
                Using Explorer As New Process
                    Explorer.StartInfo.FileName = "xdg-open"
                    Explorer.StartInfo.Arguments = String.Format("http://www.imdb.com/title/tt{0}/", txtIMDBID.Text)
                    Explorer.Start()
                End Using
            End If
        End If
    End Sub

    Private Sub lblIMDBHeader_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblIMDBHeader.MouseEnter
        If Not String.IsNullOrEmpty(txtIMDBID.Text) Then
            lblIMDBHeader.Tag = lblIMDBHeader.ForeColor
            lblIMDBHeader.ForeColor = Color.FromArgb(Not lblIMDBHeader.ForeColor.R, Not lblIMDBHeader.ForeColor.G, Not lblIMDBHeader.ForeColor.B)
            Me.Cursor = Cursors.Hand
        End If
    End Sub

    Private Sub lblIMDBHeader_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblIMDBHeader.MouseLeave
        If Not lblIMDBHeader.Tag Is Nothing Then
            lblIMDBHeader.ForeColor = DirectCast(lblIMDBHeader.Tag, Color)
            Me.Cursor = Cursors.Default
            lblIMDBHeader.Tag = Nothing
        End If
    End Sub

    Private Sub pbStudio_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStudio.MouseEnter

        If Not AdvancedSettings.GetBooleanSetting("StudioTagAlwaysOn", False) AndAlso Not String.IsNullOrEmpty(pbStudio.Tag.ToString) Then
            lblStudio.Text = pbStudio.Tag.ToString
        End If
    End Sub
    Private Sub pbStudio_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbStudio.MouseLeave
        If Not AdvancedSettings.GetBooleanSetting("StudioTagAlwaysOn", False) Then lblStudio.Text = String.Empty
    End Sub
#End Region 'Methods

#Region "Nested Types"

    Private Structure Arguments

#Region "Fields"

        Dim ID As Integer
        Dim IsTV As Boolean
        Dim Movie As Structures.DBMovie
        Dim Options As Structures.ScrapeOptions
        Dim Path As String
        Dim pURL As String
        Dim scrapeType As Enums.ScrapeType
        Dim Season As Integer
        Dim setEnabled As Boolean
        Dim TVShow As Structures.DBTV

#End Region 'Fields

    End Structure

    Private Structure Results

#Region "Fields"

        Dim fileInfo As String
        Dim IsTV As Boolean
        Dim Movie As Structures.DBMovie
        Dim Options As Structures.ScrapeOptions
        Dim Path As String
        Dim Result As Image
        Dim scrapeType As Enums.ScrapeType
        Dim setEnabled As Boolean
        Dim TVShow As Structures.DBTV

#End Region 'Fields

    End Structure

#End Region 'Nested Types


    Private Sub tmrKeyBuffer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrKeyBuffer.Tick
        tmrKeyBuffer.Enabled = False
        KeyBuffer = String.Empty
    End Sub
End Class