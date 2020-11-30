Imports EmberAPI

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

Public Class dlgSetsManager

#Region "Fields"

    Friend WithEvents bwLoadMovies As New System.ComponentModel.BackgroundWorker

    Private alSets As New List(Of String)
    Private currSet As New Sets
    Private lMovies As New List(Of Movies)
    Private needsSave As Boolean = False
    Private sListTitle As String = String.Empty

#End Region 'Fields

#Region "Methods"

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        Try
            Dim lMov As New Movies

            If Me.lbSets.SelectedItems.Count > 0 Then
                While lbMovies.SelectedItems.Count > 0
                    If Not Me.lbMoviesInSet.Items.Contains(lbMovies.SelectedItems(0)) Then
                        Me.sListTitle = lbMovies.SelectedItems(0).ToString
                        lMov = lMovies.Find(AddressOf FindMovie)
                        If Not IsNothing(lMov) Then
                            Me.currSet.AddMovie(lMov, Me.currSet.Movies.Count + 1)
                            needsSave = True
                            Me.lbMovies.Items.Remove(lbMovies.SelectedItems(0))
                        End If
                    End If
                End While

                lbMovies.SelectedIndex = -1
                Me.LoadCurrSet()

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Try
            If Me.lbMoviesInSet.Items.Count > 0 AndAlso Not IsNothing(Me.lbMoviesInSet.SelectedItem) AndAlso Me.lbMoviesInSet.SelectedIndex < (Me.lbMoviesInSet.Items.Count - 1) Then
                needsSave = True
                Dim iIndex As Integer = Me.lbMoviesInSet.SelectedIndex
                Me.currSet.Movies(iIndex).Order += 1
                Me.currSet.Movies(iIndex + 1).Order -= 1
                Me.LoadCurrSet()
                Me.lbMoviesInSet.SelectedIndex = iIndex + 1
                Me.lbMoviesInSet.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnEditSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditSet.Click
        Me.EditSet()
    End Sub

    Private Sub btnNewSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewSet.Click
        Try
            Using dNewSet As New dlgNewSet
                Dim strSet As String = dNewSet.ShowDialog
                If Not String.IsNullOrEmpty(strSet) AndAlso Not Me.alSets.Contains(strSet) Then
                    Me.alSets.Add(strSet)
                    Me.LoadSets()
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnRemoveSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveSet.Click
        Me.RemoveSet()
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.DeleteFromSet()
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Try
            If Me.lbMoviesInSet.Items.Count > 0 AndAlso Not IsNothing(Me.lbMoviesInSet.SelectedItem) AndAlso Me.lbMoviesInSet.SelectedIndex > 0 Then
                needsSave = True
                Dim iIndex As Integer = Me.lbMoviesInSet.SelectedIndex
                Me.currSet.Movies(iIndex).Order -= 1
                Me.currSet.Movies(iIndex - 1).Order += 1
                Me.LoadCurrSet()
                Me.lbMoviesInSet.SelectedIndex = iIndex - 1
                Me.lbMoviesInSet.Focus()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadMovies_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadMovies.DoWork
        '//
        ' Start thread to load movie information from nfo
        '\\

        Try

            For Each sSet As String In Master.eSettings.Sets
                If Not String.IsNullOrEmpty(sSet) Then alSets.Add(sSet)
            Next

            Using SQLcommand As SQLite.SQLiteCommand = Master.DB.MediaDBConn.CreateCommand()
                Dim tmpMovie As New Structures.DBMovie
                Dim iProg As Integer = 0
                SQLcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies;")
                Using SQLcount As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Me.bwLoadMovies.ReportProgress(-1, SQLcount("mcount"))
                End Using
                SQLcommand.CommandText = String.Concat("SELECT ID FROM movies ORDER BY ListTitle ASC;")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    If SQLreader.HasRows Then
                        While SQLreader.Read()
                            If bwLoadMovies.CancellationPending Then Return
                            tmpMovie = Master.DB.LoadMovieFromDB(Convert.ToInt64(SQLreader("ID")))
                            If Not String.IsNullOrEmpty(tmpMovie.Movie.Title) Then
                                lMovies.Add(New Movies With {.DBMovie = tmpMovie, .ListTitle = String.Concat(StringUtils.FilterTokens(tmpMovie.Movie.Title), If(Not String.IsNullOrEmpty(tmpMovie.Movie.Year), String.Format(" ({0})", tmpMovie.Movie.Year), String.Empty))})
                                If tmpMovie.Movie.Sets.Count > 0 Then
                                    For Each mSet As MediaContainers.Set In tmpMovie.Movie.Sets
                                        If Not alSets.Contains(mSet.Set) AndAlso Not String.IsNullOrEmpty(mSet.Set) Then
                                            alSets.Add(mSet.Set)
                                        End If
                                    Next
                                End If
                            End If
                            Me.bwLoadMovies.ReportProgress(iProg, tmpMovie.Movie.Title)
                            iProg += 1
                        End While
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwLoadMovies_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadMovies.ProgressChanged
        If e.ProgressPercentage >= 0 Then
            Me.pbCompile.Value = e.ProgressPercentage
            Me.lblFile.Text = e.UserState.ToString
        Else
            Me.pbCompile.Maximum = Convert.ToInt32(e.UserState)
        End If
    End Sub

    Private Sub bwLoadMovies_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadMovies.RunWorkerCompleted
        '//
        ' Thread finished: fill movie and sets lists
        '\\

        Me.LoadSets()

        Me.FillMovies()

        Me.pnlCancel.Visible = False

        Me.lbSets.Enabled = True
        Me.lbMoviesInSet.Enabled = True
        Me.lbMovies.Enabled = True
        Me.btnNewSet.Enabled = True
        'Me.btnUp.Enabled = True
        'Me.btnDown.Enabled = True
        'Me.btnRemove.Enabled = True
        Me.btnAdd.Enabled = True
        'Me.btnEditSet.Enabled = True
        'Me.btnRemoveSet.Enabled = True
    End Sub

    Private Sub DeleteFromSet()
        Dim lMov As New Movies

        If Me.lbMoviesInSet.SelectedItems.Count > 0 Then
            Me.SetControlsEnabled(False)
            While Me.lbMoviesInSet.SelectedItems.Count > 0
                Me.sListTitle = Me.lbMoviesInSet.SelectedItems(0).ToString
                lMov = Me.currSet.Movies.Find(AddressOf FindMovie)
                If Not IsNothing(lMov) Then
                    Me.RemoveFromSet(lMov, False)
                Else
                    Me.lbMoviesInSet.Items.Remove(Me.lbMoviesInSet.SelectedItems(0))
                End If
            End While
            Me.LoadCurrSet()
            Me.FillMovies()
            Me.SetControlsEnabled(True)
            Me.btnUp.Enabled = False
            Me.btnDown.Enabled = False
            Me.btnRemove.Enabled = False
        End If
    End Sub

    Private Sub dlgSetsManager_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.bwLoadMovies.IsBusy Then
            Me.DoCancel()
            While Me.bwLoadMovies.IsBusy
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
        End If
    End Sub

    Private Sub dlgSetsManager_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()
        Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
        Using g As Graphics = Graphics.FromImage(iBackground)
            g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
            Me.pnlTop.BackgroundImage = iBackground
        End Using
    End Sub

    Private Sub dlgSetsManager_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()

        ' Show Cancel Panel
        btnCancel.Visible = True
        lblCompiling.Visible = True
        pbCompile.Visible = True
        pbCompile.Style = ProgressBarStyle.Continuous
        lblCanceling.Visible = False
        pnlCancel.Visible = True
        Application.DoEvents()

        Me.bwLoadMovies.WorkerSupportsCancellation = True
        Me.bwLoadMovies.WorkerReportsProgress = True
        Me.bwLoadMovies.RunWorkerAsync()
    End Sub

    Private Sub DoCancel()
        Me.bwLoadMovies.CancelAsync()
        btnCancel.Visible = False
        lblCompiling.Visible = False
        pbCompile.Style = ProgressBarStyle.Marquee
        pbCompile.MarqueeAnimationSpeed = 25
        lblCanceling.Visible = True
        lblFile.Visible = False
    End Sub

    Private Sub EditSet()
        Try
            If Me.lbSets.SelectedItems.Count > 0 Then
                Using dNewSet As New dlgNewSet
                    Dim strSet As String = dNewSet.ShowDialog(Me.lbSets.SelectedItem.ToString)
                    If Not String.IsNullOrEmpty(strSet) AndAlso Not Me.alSets.Contains(strSet) Then
                        Me.SetControlsEnabled(False)
                        For i As Integer = 0 To Me.alSets.Count - 1
                            If Me.alSets(i).ToString = Me.lbSets.SelectedItem.ToString Then
                                'remove the old set from each movie.
                                If lbMoviesInSet.Items.Count > 0 Then
                                    For Each lMov As Movies In Me.currSet.Movies
                                        Me.RemoveFromSet(lMov, True)
                                    Next
                                End If
                                'set the currset to have the updated title
                                currSet.Set = strSet
                                'save the set to update each movie with the new set name
                                Me.SaveSet(currSet)
                                'change the name in alSets
                                Me.alSets(i) = strSet
                                Exit For
                            End If
                        Next
                        Me.LoadSets()
                    End If
                End Using
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub FillMovies()
        Try
            Me.lbMovies.SuspendLayout()

            Me.lbMovies.Items.Clear()
            For Each lMov As Movies In lMovies
                If Not Me.lbMoviesInSet.Items.Contains(lMov.ListTitle) Then Me.lbMovies.Items.Add(lMov.ListTitle)
            Next

            Me.lbMovies.ResumeLayout()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function FindMovie(ByVal lMov As Movies) As Boolean
        If lMov.ListTitle = sListTitle Then Return True Else  : Return False
    End Function

    Private Sub lbMoviesInSet_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lbMoviesInSet.KeyDown
        If e.KeyCode = Keys.Delete Then Me.DeleteFromSet()
    End Sub

    Private Sub lbSets_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lbSets.DoubleClick
        Me.EditSet()
    End Sub

    Private Sub lbSets_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lbSets.KeyDown
        If e.KeyCode = Keys.Delete Then Me.RemoveSet()
    End Sub

    Private Sub lbSets_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbSets.SelectedIndexChanged
        Try
            Dim tOrder As Integer = 0

            If Me.currSet.Movies.Count > 0 AndAlso needsSave Then SaveSet(currSet)

            Me.currSet.Clear()
            Me.lbMoviesInSet.Items.Clear()

            If Me.lbSets.SelectedItems.Count > 0 Then

                Me.lblCurrentSet.Text = Me.lbSets.SelectedItem.ToString
                Me.currSet.Set = Me.lbSets.SelectedItem.ToString

                For Each tMovie As Movies In lMovies
                    For Each mSet As MediaContainers.Set In tMovie.DBMovie.Movie.Sets
                        If mSet.Set = Me.currSet.Set Then
                            If Not String.IsNullOrEmpty(mSet.Order) Then
                                tOrder = Convert.ToInt32(mSet.Order)
                            End If
                            Me.currSet.AddMovie(tMovie, tOrder)
                        End If
                    Next
                Next

                If Me.currSet.Movies.Count > 0 Then
                    Me.LoadCurrSet()
                End If
                Me.btnEditSet.Enabled = True
                Me.btnRemoveSet.Enabled = True
            Else
                Me.lblCurrentSet.Text = Master.eLang.GetString(368, "None Selected")
                needsSave = False
                Me.btnEditSet.Enabled = False
                Me.btnRemoveSet.Enabled = False
            End If
            Me.btnUp.Enabled = False
            Me.btnDown.Enabled = False
            Me.btnRemove.Enabled = False
            Me.FillMovies()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub LoadCurrSet()
        Try
            Me.lbMoviesInSet.SuspendLayout()

            Me.lbMoviesInSet.Items.Clear()
            Me.currSet.Movies.Sort()
            For Each tMovie As Movies In Me.currSet.Movies
                Me.lbMoviesInSet.Items.Add(tMovie.ListTitle)
                tMovie.Order = Me.lbMoviesInSet.Items.Count
            Next

            Me.lbMoviesInSet.ResumeLayout()
            Me.btnUp.Enabled = False
            Me.btnDown.Enabled = False
            Me.btnRemove.Enabled = False
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub LoadSets()
        Me.lbSets.SuspendLayout()

        Me.lbSets.Items.Clear()
        Me.lbMoviesInSet.Items.Clear()
        Me.lblCurrentSet.Text = Master.eLang.GetString(368, "None Selected")

        For Each mSet As String In alSets
            Me.lbSets.Items.Add(mSet)
        Next

        Me.lbSets.ResumeLayout()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Master.eSettings.Sets = alSets
        Master.eSettings.Save()

        If Me.currSet.Movies.Count > 0 AndAlso needsSave Then SaveSet(currSet)

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub RemoveFromSet(ByVal lMov As Movies, ByVal isEdit As Boolean)
        Try
            lMov.DBMovie.Movie.RemoveSet(Me.currSet.Set)
            Master.DB.SaveMovieToDB(lMov.DBMovie, False, False, True)
            If Not isEdit Then Me.currSet.Movies.Remove(lMov)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub RemoveSet()
        Try
            If lbMoviesInSet.Items.Count > 0 Then

                For x As Integer = Me.currSet.Movies.Count - 1 To 0 Step -1
                    Me.RemoveFromSet(Me.currSet.Movies(x), False)
                Next
            End If

            Me.alSets.Remove(Me.lbSets.SelectedItem.ToString)

            Me.LoadSets()
            Me.btnEditSet.Enabled = False
            Me.btnRemoveSet.Enabled = False
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SaveSet(ByVal mSet As Sets)
        Try
            Me.SetControlsEnabled(False)

            Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MediaDBConn.BeginTransaction()
                For Each tMovie As Movies In mSet.Movies
                    If Not Master.eSettings.YAMJSetsCompatible Then
                        tMovie.DBMovie.Movie.AddSet(mSet.Set, 0)
                    Else
                        tMovie.DBMovie.Movie.AddSet(mSet.Set, tMovie.Order)
                    End If
                    Master.DB.SaveMovieToDB(tMovie.DBMovie, False, True, True)
                Next
                SQLtransaction.Commit()
                needsSave = False
            End Using

            Me.SetControlsEnabled(True)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetControlsEnabled(ByVal isEnabled As Boolean)
        Me.pnlSaving.Visible = Not isEnabled
        Me.lbSets.Enabled = isEnabled
        Me.lbMoviesInSet.Enabled = isEnabled
        Me.lbMovies.Enabled = isEnabled
        Me.btnNewSet.Enabled = isEnabled
        'Me.btnEditSet.Enabled = isEnabled
        'Me.btnRemoveSet.Enabled = isEnabled
        'Me.btnUp.Enabled = isEnabled
        'Me.btnDown.Enabled = isEnabled
        'Me.btnRemove.Enabled = isEnabled
        Me.btnAdd.Enabled = isEnabled
        Me.OK_Button.Enabled = isEnabled
        Application.DoEvents()
    End Sub

    Private Sub SetUp()
        If Not Master.eSettings.YAMJSetsCompatible Then
            btnUp.Visible = False
            btnDown.Visible = False
        End If
        Me.Text = Master.eLang.GetString(365, "Sets Manager")
        Me.OK_Button.Text = Master.eLang.GetString(19, "Close")
        Me.GroupBox1.Text = Master.eLang.GetString(36, "Movies")
        Me.GroupBox2.Text = Master.eLang.GetString(366, "Sets")
        Me.GroupBox3.Text = Master.eLang.GetString(367, "Movies In Set")
        Me.lblCurrentSet.Text = Master.eLang.GetString(368, "None Selected")
        Me.lblCompiling.Text = Master.eLang.GetString(369, "Loading Movies and Sets...")
        Me.lblCanceling.Text = Master.eLang.GetString(370, "Canceling Load...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label2.Text = Master.eLang.GetString(371, "Add and configure movie boxed sets.")
        Me.Label4.Text = Me.Text
    End Sub

    Private Sub lbMoviesInSet_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbMoviesInSet.SelectedIndexChanged
        ValidateMoviesInSet()
    End Sub

    Sub ValidateMoviesInSet()
        If Me.currSet.Movies.Count > 0 Then
            Dim canMove As Boolean = (Me.currSet.Movies.Count > 1)
            Me.btnUp.Enabled = canMove
            Me.btnDown.Enabled = canMove
            Me.btnRemove.Enabled = True
        Else
            Me.btnUp.Enabled = False
            Me.btnDown.Enabled = False
            Me.btnRemove.Enabled = False
        End If
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Friend Class Movies
        Implements IComparable(Of Movies)

#Region "Fields"

        Private _dbmovie As Structures.DBMovie
        Private _listtitle As String
        Private _order As Integer

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property DBMovie() As Structures.DBMovie
            Get
                Return Me._dbmovie
            End Get
            Set(ByVal value As Structures.DBMovie)
                Me._dbmovie = value
            End Set
        End Property

        Public Property ListTitle() As String
            Get
                Return Me._listtitle
            End Get
            Set(ByVal value As String)
                Me._listtitle = value
            End Set
        End Property

        Public Property Order() As Integer
            Get
                Return Me._order
            End Get
            Set(ByVal value As Integer)
                Me._order = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._dbmovie = New Structures.DBMovie
            Me._order = 0
            Me._listtitle = String.Empty
        End Sub

        Public Function CompareTo(ByVal other As Movies) As Integer Implements IComparable(Of Movies).CompareTo
            Return (Me.Order).CompareTo(other.Order)
        End Function

#End Region 'Methods

    End Class

    Friend Class Sets

#Region "Fields"

        Private _movies As New List(Of Movies)
        Private _order As Integer
        Private _set As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property Movies() As List(Of Movies)
            Get
                Return _movies
            End Get
            Set(ByVal value As List(Of Movies))
                _movies = value
            End Set
        End Property

        Public Property [Set]() As String
            Get
                Return _set
            End Get
            Set(ByVal value As String)
                _set = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub AddMovie(ByVal sMovie As Movies, ByVal Order As Integer)
            sMovie.Order = Order
            _movies.Add(sMovie)
        End Sub

        Public Sub Clear()
            Me._set = String.Empty
            Me._order = 0
            Me._movies.Clear()
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types


End Class