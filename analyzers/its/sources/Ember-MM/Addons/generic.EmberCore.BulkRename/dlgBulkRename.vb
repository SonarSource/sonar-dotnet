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

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports EmberAPI

Public Class dlgBulkRenamer

#Region "Fields"

    Friend WithEvents bwDoRename As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadInfo As New System.ComponentModel.BackgroundWorker

    Private bsMovies As New BindingSource
    Private CancelRename As Boolean = False
    Private DoneRename As Boolean = False
    Private FFRenamer As FileFolderRenamer
    Private isLoaded As Boolean = False
    Private run_once As Boolean = True
    Private _columnsize(9) As Integer
    Private dHelpTips As dlgHelpTips
#End Region 'Fields

#Region "Delegates"

    Delegate Sub MyFinish()

    Delegate Sub MyStart()

#End Region 'Delegates

#Region "Methods"

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If DoneRename Then
            CancelRename = True
        Else
            DoCancel()
        End If
    End Sub

    Private Sub bwbwDoRename_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDoRename.RunWorkerCompleted
        pnlCancel.Visible = False
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub bwDoRename_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDoRename.DoWork
        FFRenamer.DoRename(AddressOf ShowProgressRename)
    End Sub

    Private Sub bwDoRename_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwDoRename.ProgressChanged
        pbCompile.Value = e.ProgressPercentage
        lblFile.Text = e.UserState.ToString
    End Sub

    Private Sub bwLoadInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadInfo.DoWork
        '//
        ' Thread to load movieinformation (from nfo)
        '\\
        Try
            Dim MovieFile As New FileFolderRenamer.FileRename
            Dim _curMovie As New Structures.DBMovie

            ' Load nfo movies using path from DB
            Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                Dim _tmpPath As String = String.Empty
                Dim iProg As Integer = 0
                SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies;")
                Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    Me.bwLoadInfo.ReportProgress(-1, SQLcount("mcount")) ' set maximum
                End Using
                SQLNewcommand.CommandText = String.Concat("SELECT NfoPath ,id FROM movies ORDER BY ListTitle ASC;")
                Using SQLreader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    If SQLreader.HasRows Then
                        While SQLreader.Read()
                            Try
                                If Not DBNull.Value.Equals(SQLreader("NfoPath")) AndAlso Not DBNull.Value.Equals(SQLreader("id")) Then
                                    _tmpPath = SQLreader("NfoPath").ToString
                                    If Not String.IsNullOrEmpty(_tmpPath) Then

                                        MovieFile = New FileFolderRenamer.FileRename
                                        MovieFile.ID = Convert.ToInt32(SQLreader("id"))
                                        _curMovie = Master.DB.LoadMovieFromDB(MovieFile.ID)
                                        If Not _curMovie.ID = -1 AndAlso Not String.IsNullOrEmpty(_curMovie.Filename) Then
                                            If String.IsNullOrEmpty(_curMovie.Movie.Title) Then
                                                MovieFile.Title = _curMovie.ListTitle
                                            Else
                                                MovieFile.Title = _curMovie.Movie.Title
                                            End If
                                            If String.IsNullOrEmpty(_curMovie.Movie.SortTitle) Then
                                                MovieFile.SortTitle = MovieFile.Title
                                            Else
                                                MovieFile.SortTitle = _curMovie.Movie.SortTitle
                                            End If
                                            MovieFile.ListTitle = _curMovie.ListTitle
                                            MovieFile.MPAARate = FileFolderRenamer.SelectMPAA(_curMovie.Movie)
                                            MovieFile.OriginalTitle = _curMovie.Movie.OriginalTitle
                                            MovieFile.Year = _curMovie.Movie.Year
                                            MovieFile.IsLocked = _curMovie.IsLock
                                            MovieFile.IsSingle = _curMovie.isSingle
                                            MovieFile.IMDBID = _curMovie.Movie.IMDBID
                                            MovieFile.Genre = _curMovie.Movie.Genre
                                            MovieFile.Director = _curMovie.Movie.Director
                                            MovieFile.FileSource = _curMovie.FileSource

                                            If Not IsNothing(_curMovie.Movie.FileInfo) Then
                                                Try
                                                    If _curMovie.Movie.FileInfo.StreamDetails.Video.Count > 0 Then
                                                        Dim tVid As MediaInfo.Video = NFO.GetBestVideo(_curMovie.Movie.FileInfo)
                                                        Dim tRes As String = NFO.GetResFromDimensions(tVid)
                                                        MovieFile.Resolution = String.Format("{0}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes))
                                                    Else
                                                        MovieFile.Resolution = String.Empty
                                                    End If

                                                    If _curMovie.Movie.FileInfo.StreamDetails.Audio.Count > 0 Then
                                                        Dim tAud As MediaInfo.Audio = NFO.GetBestAudio(_curMovie.Movie.FileInfo, False)
                                                        MovieFile.Audio = String.Format("{0}-{1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels))
                                                    Else
                                                        MovieFile.Audio = String.Empty
                                                    End If
                                                Catch ex As Exception
                                                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error FileInfo")
                                                End Try
                                            Else
                                                MovieFile.Resolution = String.Empty
                                                MovieFile.Audio = String.Empty
                                            End If

                                            For Each i As String In FFRenamer.MovieFolders
                                                If _curMovie.Filename.StartsWith(i, StringComparison.OrdinalIgnoreCase) Then
                                                    MovieFile.BasePath = If(i.EndsWith(Path.DirectorySeparatorChar.ToString), i.Substring(0, i.Length - 1), i)
                                                    If FileUtils.Common.isVideoTS(_curMovie.Filename) Then
                                                        MovieFile.Parent = Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).Name
                                                        If MovieFile.BasePath = Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName Then
                                                            MovieFile.OldPath = String.Empty
                                                            MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                                                        Else
                                                            MovieFile.OldPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName).FullName.Replace(MovieFile.BasePath, String.Empty)
                                                        End If
                                                        MovieFile.IsVideo_TS = True
                                                    ElseIf FileUtils.Common.isBDRip(_curMovie.Filename) Then
                                                        MovieFile.Parent = Directory.GetParent(Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName).Name
                                                        If MovieFile.BasePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName).FullName Then
                                                            MovieFile.OldPath = String.Empty
                                                            MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                                                        Else
                                                            MovieFile.OldPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName).FullName).FullName.Replace(MovieFile.BasePath, String.Empty)
                                                        End If
                                                        MovieFile.IsBDMV = True
                                                    Else
                                                        MovieFile.Parent = Directory.GetParent(_curMovie.Filename).Name
                                                        If MovieFile.BasePath = Directory.GetParent(_curMovie.Filename).FullName Then
                                                            MovieFile.OldPath = String.Empty
                                                            MovieFile.BasePath = Directory.GetParent(MovieFile.BasePath).FullName
                                                        Else
                                                            MovieFile.OldPath = Directory.GetParent(Directory.GetParent(_curMovie.Filename).FullName).FullName.Replace(MovieFile.BasePath, String.Empty)
                                                        End If
                                                    End If
                                                End If
                                            Next

                                            If Not MovieFile.IsVideo_TS AndAlso Not MovieFile.IsBDMV Then
                                                MovieFile.FileName = StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(_curMovie.Filename))
                                                Dim stackMark As String = Path.GetFileNameWithoutExtension(_curMovie.Filename).Replace(MovieFile.FileName, String.Empty).ToLower
                                                If Not stackMark = String.Empty AndAlso _curMovie.Movie.Title.ToLower.EndsWith(stackMark) Then
                                                    MovieFile.FileName = Path.GetFileNameWithoutExtension(_curMovie.Filename)
                                                End If
                                            ElseIf MovieFile.IsBDMV Then
                                                MovieFile.FileName = String.Concat("BDMV", Path.DirectorySeparatorChar, "STREAM")
                                            Else
                                                MovieFile.FileName = "VIDEO_TS"
                                            End If

                                            FFRenamer.AddMovie(MovieFile)

                                            Me.bwLoadInfo.ReportProgress(iProg, _curMovie.ListTitle)
                                        End If
                                    End If
                                End If
                            Catch ex As Exception
                                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                            End Try
                            iProg += 1

                            If bwLoadInfo.CancellationPending Then
                                e.Cancel = True
                                Return
                            End If
                        End While
                        e.Result = True
                    Else
                        e.Cancel = True
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadInfo_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadInfo.ProgressChanged
        If e.ProgressPercentage >= 0 Then
            Me.pbCompile.Value = e.ProgressPercentage
            Me.lblFile.Text = e.UserState.ToString
        Else
            Me.pbCompile.Maximum = Convert.ToInt32(e.UserState)
        End If
    End Sub

    Private Sub bwLoadInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadInfo.RunWorkerCompleted
        '//
        ' Thread finished: display it if not cancelled
        '\\
        Try
            If Not e.Cancelled Then
                Rename_Button.Enabled = True
                isLoaded = True
                tmrSimul.Enabled = True
            Else
            End If
            Me.pnlCancel.Visible = False
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub chkRenamedOnly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRenamedOnly.CheckedChanged
        If chkRenamedOnly.Checked Then
            bsMovies.Filter = "IsRenamed = True AND IsLocked = False"
        Else
            bsMovies.RemoveFilter()
        End If
    End Sub

    Private Sub Close_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Close_Button.Click
        If DoneRename Then
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Else
            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        End If

        Me.Close()
    End Sub

    Private Sub cmsMovieList_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles cmsMovieList.Opening
        Dim count As Integer = FFRenamer.GetCount
        Dim lockcount As Integer = FFRenamer.GetCountLocked
        If count > 0 Then
            If lockcount > 0 Then
                tsmUnlockAll.Visible = True
                If lockcount < count Then
                    tsmLockAll.Visible = True
                Else
                    tsmLockAll.Visible = False
                End If
                If lockcount = count Then
                    tsmLockAll.Visible = False
                End If

            Else
                tsmLockAll.Visible = True
                tsmUnlockAll.Visible = False
            End If
        Else
            tsmUnlockAll.Visible = False
            tsmLockAll.Visible = False
        End If
        tsmLockMovie.Visible = False
        tsmUnlockMovie.Visible = False
        For Each row As DataGridViewRow In dgvMoviesList.SelectedRows
            If Convert.ToBoolean(row.Cells(5).Value) Then
                tsmUnlockMovie.Visible = True
            Else
                tsmLockMovie.Visible = True
            End If
        Next
    End Sub

    Private Sub dgvMoviesList_CellPainting(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellPaintingEventArgs) Handles dgvMoviesList.CellPainting
        Try

            If (e.ColumnIndex = 3 OrElse e.ColumnIndex = 4) AndAlso e.RowIndex >= 0 Then
                If Convert.ToBoolean(dgvMoviesList.Rows(e.RowIndex).Cells(5).Value) Then
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    e.CellStyle.ForeColor = Color.Red
                ElseIf Not IsNothing(e.Value) AndAlso Not dgvMoviesList.Rows(e.RowIndex).Cells(e.ColumnIndex - 2).Value.ToString = e.Value.ToString Then
                    e.CellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                    If (Convert.ToBoolean(dgvMoviesList.Rows(e.RowIndex).Cells(6).Value) AndAlso e.ColumnIndex = 3) OrElse (Convert.ToBoolean(dgvMoviesList.Rows(e.RowIndex).Cells(7).Value) AndAlso e.ColumnIndex = 4) Then
                        e.CellStyle.ForeColor = Color.Purple
                    Else
                        e.CellStyle.ForeColor = Color.Blue
                    End If
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvMoviesList_ColumnHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvMoviesList.ColumnHeaderMouseClick
        Me.dgvMoviesList.Sort(Me.dgvMoviesList.Columns(e.ColumnIndex), System.ComponentModel.ListSortDirection.Ascending)
    End Sub

    Private Sub dgvMoviesList_ColumnWidthChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewColumnEventArgs) Handles dgvMoviesList.ColumnWidthChanged
        If Not dgvMoviesList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None OrElse dgvMoviesList.Columns.Count < 9 OrElse Convert.ToBoolean(dgvMoviesList.Tag) Then Return
        Dim sum As Integer = 0
        For Each c As DataGridViewColumn In dgvMoviesList.Columns
            If c.Visible Then sum += c.Width
        Next
        If sum < dgvMoviesList.Width Then
            e.Column.Width = dgvMoviesList.Width - (sum - e.Column.Width)
        End If
    End Sub

    Private Sub dlgBulkRename_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.bwLoadInfo.IsBusy Then
            Me.DoCancel()
            While Me.bwLoadInfo.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End If
    End Sub

    Private Sub dlgBulkRename_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()

        FFRenamer = New FileFolderRenamer
        Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
        Using g As Graphics = Graphics.FromImage(iBackground)
            g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
            Me.pnlTop.BackgroundImage = iBackground
        End Using

    End Sub

    Private Sub dlgBulkRename_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()

        Try
            ' Show Cancel Panel
            btnCancel.Visible = True
            lblCompiling.Visible = True
            pbCompile.Visible = True
            pbCompile.Style = ProgressBarStyle.Continuous
            lblCanceling.Visible = False
            pnlCancel.Visible = True
            Application.DoEvents()

            'Start worker
            Me.bwLoadInfo = New System.ComponentModel.BackgroundWorker
            Me.bwLoadInfo.WorkerSupportsCancellation = True
            Me.bwLoadInfo.WorkerReportsProgress = True
            Me.bwLoadInfo.RunWorkerAsync()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub DoCancel()
        Try
            Me.bwLoadInfo.CancelAsync()
            btnCancel.Visible = False
            lblCompiling.Visible = False
            pbCompile.Style = ProgressBarStyle.Marquee
            pbCompile.MarqueeAnimationSpeed = 25
            lblCanceling.Visible = True
            lblFile.Visible = False
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub DoProcess()
        Dim Sta As MyStart = New MyStart(AddressOf Start)
        Dim Fin As MyFinish = New MyFinish(AddressOf Finish)
        Me.Invoke(Sta)
        FFRenamer.ProccessFiles(txtFolder.Text, txtFile.Text, txtFolderNotSingle.Text)
        Me.Invoke(Fin)
    End Sub

    Private Sub Finish()
        Simulate()
        Me.pnlCancel.Visible = False
    End Sub

    Private Sub Rename_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Rename_Button.Click
        DoneRename = True
        pnlCancel.Visible = True
        lblCompiling.Text = Master.eLang.GetString(1, "Renaming...")
        lblFile.Visible = True
        pbCompile.Style = ProgressBarStyle.Continuous
        pbCompile.Maximum = FFRenamer.GetMoviesCount
        pbCompile.Value = 0
        Application.DoEvents()
        'Start worker
        Me.bwDoRename = New System.ComponentModel.BackgroundWorker
        Me.bwDoRename.WorkerSupportsCancellation = True
        Me.bwDoRename.WorkerReportsProgress = True
        Me.bwDoRename.RunWorkerAsync()
    End Sub

    Sub setLock(ByVal lock As Boolean)
        For Each row As DataGridViewRow In dgvMoviesList.SelectedRows
            FFRenamer.SetIsLocked(row.Cells(1).Value.ToString, row.Cells(2).Value.ToString, lock)
            row.Cells(5).Value = lock
        Next

        If Me.chkRenamedOnly.Checked AndAlso lock Then
            Me.dgvMoviesList.ClearSelection()
            Me.dgvMoviesList.CurrentCell = Nothing
        End If

        dgvMoviesList.Refresh()
    End Sub

    Sub setLockAll(ByVal lock As Boolean)
        Try
            FFRenamer.SetIsLocked(String.Empty, String.Empty, False)
            For Each row As DataGridViewRow In dgvMoviesList.Rows
                row.Cells(5).Value = lock
            Next

            If Me.chkRenamedOnly.Checked AndAlso lock Then
                Me.dgvMoviesList.ClearSelection()
                Me.dgvMoviesList.CurrentCell = Nothing
            End If

            dgvMoviesList.Refresh()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(2, "Bulk Renamer")
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.Label2.Text = Master.eLang.GetString(3, "Rename movies and files")
        Me.Label4.Text = Me.Text
        Me.lblCompiling.Text = Master.eLang.GetString(4, "Compiling Movie List...")
        Me.lblCanceling.Text = Master.eLang.GetString(5, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Rename_Button.Text = Master.eLang.GetString(6, "Rename")
        Me.tsmLockMovie.Text = Master.eLang.GetString(24, "Lock", True)
        Me.tsmUnlockMovie.Text = Master.eLang.GetString(108, "Unlock", True)
        Me.tsmLockAll.Text = Master.eLang.GetString(169, "Lock All", True)
        Me.tsmUnlockAll.Text = Master.eLang.GetString(170, "Unlock All", True)
        Me.lblFolderPattern.Text = Master.eLang.GetString(7, "Folder Pattern (for Single movie in Folder)")
        Me.lblFilePattern.Text = Master.eLang.GetString(8, "File Pattern")
        Me.Label1.Text = Master.eLang.GetString(9, "Folder Pattern (for Multiple movies in Folder)")
        Me.chkRenamedOnly.Text = Master.eLang.GetString(10, "Display Only Movies That Will Be Renamed")

        Dim frmToolTip As New ToolTip()
        Dim s As String = String.Format(Master.eLang.GetString(11, "$1 = First Letter of the Title{0}$A = Audio{0}$B = Base Path{0}$C = Director{0}$D = Directory{0}$E = Sort Title{0}$F = File Name{0}$G = Genre (Follow with a space, dot or hyphen to change separator){0}$I = IMDB ID{0}$L = List Title{0}$M = MPAA{0}$O = OriginalTitle{0}$R = Resolution{0}$S = Source{0}$T = Title{0}$Y = Year{0}$X. (Replace Space with .){0}{{}} = Optional{0}$?aaa?bbb? = Replace aaa with bbb{0}$- = Remove previous char if next pattern does not have a value{0}$+ = Remove next char if previous pattern does not have a value{0}$^ = Remove previous and next char if next pattern does not have a value"), vbNewLine)
        frmToolTip.SetToolTip(Me.txtFolder, s)
        frmToolTip.SetToolTip(Me.txtFile, s)
        frmToolTip.SetToolTip(Me.txtFolderNotSingle, s)
    End Sub

    Private Function ShowProgressRename(ByVal mov As String, ByVal iProg As Integer) As Boolean
        Me.bwDoRename.ReportProgress(iProg, mov.ToString)
        If CancelRename Then Return False
        Return True
    End Function

    Private Sub Simulate()
        Try
            With Me.dgvMoviesList
                If Not run_once Then
                    For Each c As DataGridViewColumn In .Columns
                        _columnsize(c.Index) = c.Width
                    Next
                End If
                .DataSource = Nothing
                .Rows.Clear()
                .AutoGenerateColumns = True
                If run_once Then
                    .Tag = False
                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                End If
                bsMovies.DataSource = FFRenamer.GetMovies
                .DataSource = bsMovies
                .Columns(5).Visible = False
                .Columns(6).Visible = False
                .Columns(7).Visible = False
                .Columns(8).Visible = False
                .Columns(9).Visible = False
                If run_once Then
                    For Each c As DataGridViewColumn In .Columns
                        c.MinimumWidth = Convert.ToInt32(.Width / 5)
                    Next
                    .AutoResizeColumns()
                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                    For Each c As DataGridViewColumn In .Columns
                        c.MinimumWidth = 20
                    Next
                    run_once = False
                Else
                    .Tag = True
                    For Each c As DataGridViewColumn In .Columns
                        c.Width = _columnsize(c.Index)
                    Next
                    .Tag = False
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub Start()
        Me.btnCancel.Visible = False
        Me.lblFile.Visible = False
        Me.pbCompile.Style = ProgressBarStyle.Marquee
        Me.pnlCancel.Visible = True
    End Sub

    Private Sub tmrSimul_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSimul.Tick
        Try
            'Need to make simulate thread safe
            tmrSimul.Enabled = False
            If isLoaded Then
                Dim tThread As Threading.Thread = New Threading.Thread(AddressOf DoProcess)
                tThread.Start()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub tsmLockAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmLockAll.Click
        setLockAll(True)
    End Sub

    Private Sub tsmLockMovie_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmLockMovie.Click
        setLock(True)
    End Sub

    Private Sub tsmUnlockAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmUnlockAll.Click
        setLockAll(False)
    End Sub

    Private Sub tsmUnlockMovie_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsmUnlockMovie.Click
        setLock(False)
    End Sub

    Private Sub txtFile_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFile.TextChanged
        Try
            If String.IsNullOrEmpty(txtFile.Text) Then txtFile.Text = "$F"
            tmrSimul.Enabled = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub txtFolderNotSingle_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFolderNotSingle.TextChanged
        Try
            If String.IsNullOrEmpty(txtFolderNotSingle.Text) Then txtFolderNotSingle.Text = "$D"
            tmrSimul.Enabled = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub txtFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFolder.TextChanged
        Try
            If String.IsNullOrEmpty(txtFolder.Text) Then txtFolder.Text = "$D"
            tmrSimul.Enabled = True
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub
    Sub LoadHelpTips()
        If dHelpTips Is Nothing OrElse dHelpTips.IsDisposed Then
            dHelpTips = New dlgHelpTips
        End If
        Dim s As String = String.Format(Master.eLang.GetString(11, "$1 = First Letter of the Title{0}$A = Audio{0}$B = Base Path{0}$C = Director{0}$D = Directory{0}$E = Sort Title{0}$F = File Name{0}$G = Genre (Follow with a space, dot or hyphen to change separator){0}$I = IMDB ID{0}$L = List Title{0}$M = MPAA{0}$O = OriginalTitle{0}$R = Resolution{0}$S = Source{0}$T = Title{0}$Y = Year{0}$X. (Replace Space with .){0}{{}} = Optional{0}$?aaa?bbb? = Replace aaa with bbb{0}$- = Remove previous char if next pattern does not have a value{0}$+ = Remove next char if previous pattern does not have a value{0}$^ = Remove previous and next char if next pattern does not have a value"), vbNewLine)
        dHelpTips.lblTips.Text = s
        dHelpTips.Width = dHelpTips.lblTips.Width + 5
        dHelpTips.Height = dHelpTips.lblTips.Height + 35
        dHelpTips.Top = Me.Top + 10
        dHelpTips.Left = Me.Right - dHelpTips.Width - 10
        If dHelpTips.Visible Then
            dHelpTips.Hide()
        Else
            dHelpTips.Show(Me)
        End If
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        LoadHelpTips()
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        LoadHelpTips()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        LoadHelpTips()
    End Sub
#End Region 'Methods
End Class