Imports System.Windows.Forms
Imports System.Xml.Serialization
Imports System.IO
Imports EmberAPI

Public Class frmGenresEditor
    Public Event ModuleSettingsChanged()
    Private xmlGenres As xGenres

    Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        SetUp()
        xmlGenres = xGenres.Load(Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}Genres.xml", Path.DirectorySeparatorChar)))
        GetLanguages()
        'xmlGenres.Save(Path.Combine(Functions.AppPath, "Images\Genres\Genres2.xml"))
    End Sub

    Public Sub SaveChanges()
        xmlGenres.Save(Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}Genres.xml", Path.DirectorySeparatorChar)))
        APIXML.GenreXML = XDocument.Load(Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}Genres.xml", Path.DirectorySeparatorChar)))
    End Sub


    Private Sub GetLanguages()
        Try
            cbLangs.Items.Clear()
            dgvLang.Rows.Clear()
            cbLangs.Items.Add(Master.eLang.GetString(2, "< All >"))
            cbLangs.Items.AddRange(xmlGenres.listOfLanguages.ToArray)
            cbLangs.SelectedIndex = 0
            For Each s As String In xmlGenres.listOfLanguages
                dgvLang.Rows.Add(New Object() {False, s})
            Next
            dgvLang.ClearSelection()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub PopulateGenres()
        Try
            dgvGenres.Rows.Clear()
            ClearLangSelection()
            If cbLangs.SelectedItem.ToString = Master.eLang.GetString(2, "< All >") Then
                For Each sett As xGenre In xmlGenres.listOfGenres
                    Dim i As Integer = dgvGenres.Rows.Add(New Object() {sett.searchstring})
                    dgvGenres.Rows(i).Tag = sett
                    If sett.Langs.Count = 0 Then
                        dgvGenres.Rows(i).DefaultCellStyle.ForeColor = Drawing.Color.Red
                    End If
                Next
            Else
                btnRemoveGenre.Enabled = False
                For Each sett As xGenre In xmlGenres.listOfGenres.Where(Function(y) y.Langs.Contains(cbLangs.SelectedItem.ToString))
                    Dim i As Integer = dgvGenres.Rows.Add(New Object() {sett.searchstring})
                    dgvGenres.Rows(i).Tag = sett
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ClearLangSelection()
        For Each r As DataGridViewRow In dgvLang.Rows
            r.Cells(0).Value = False
        Next
    End Sub

    Private Sub dgvGenres_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvGenres.CurrentCellDirtyStateChanged
        Try
            Dim g As xGenre = DirectCast(dgvGenres.CurrentRow.Tag, xGenre)
            If Not g Is Nothing Then
                dgvGenres.CommitEdit(DataGridViewDataErrorContexts.Commit)
                g.searchstring = dgvGenres.CurrentRow.Cells(0).Value.ToString
                RaiseEvent ModuleSettingsChanged()
            End If
        Catch ex As Exception
        End Try

    End Sub
    Private Sub dgvGenres_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dgvGenres.KeyDown
        e.Handled = (e.KeyCode = Keys.Enter)
    End Sub

    Private Sub dgvGenres_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvGenres.SelectionChanged
        btnRemoveGenre.Enabled = False
        btnChangeImg.Enabled = False
        dgvLang.Enabled = False
        pbIcon.Image = Nothing
        Try
            dgvLang.ClearSelection()
            If dgvGenres.SelectedCells.Count > 0 Then
                Dim g As xGenre = DirectCast(dgvGenres.CurrentRow.Tag, xGenre)
                If Not g Is Nothing Then
                    ClearLangSelection()
                    For Each r As DataGridViewRow In dgvLang.Rows
                        For Each s As String In g.Langs
                            r.Cells(0).Value = If(r.Cells(1).Value.ToString = s, True, r.Cells(0).Value)
                        Next
                    Next
                    btnRemoveGenre.Enabled = True
                    btnChangeImg.Enabled = True
                    dgvLang.Enabled = True
                    If g.icon Is Nothing Then
                        pbIcon.Image = Nothing
                    Else
                        pbIcon.Load(Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}{1}", Path.DirectorySeparatorChar, g.icon)))
                    End If

                Else
                    If dgvGenres.Rows.Count > 0 Then
                        dgvGenres.ClearSelection()
                    End If
                End If
            Else
                ClearLangSelection()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Private Sub dgvLang_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvLang.CurrentCellDirtyStateChanged
        Dim g As xGenre = DirectCast(dgvGenres.CurrentRow.Tag, xGenre)
        If Not g Is Nothing Then
            dgvLang.CommitEdit(DataGridViewDataErrorContexts.Commit)
            RaiseEvent ModuleSettingsChanged()
            If Convert.ToBoolean(dgvLang.CurrentRow.Cells(0).Value) Then
                If Not g.Langs.Contains(dgvLang.CurrentRow.Cells(1).Value.ToString) Then
                    g.Langs.Add(dgvLang.CurrentRow.Cells(1).Value.ToString)
                End If
            Else
                If g.Langs.Contains(dgvLang.CurrentRow.Cells(1).Value.ToString) Then
                    g.Langs.Remove(dgvLang.CurrentRow.Cells(1).Value.ToString)
                End If
            End If

            If g.Langs.Count > 0 AndAlso dgvLang.CurrentRow.DefaultCellStyle.ForeColor = Drawing.Color.Red Then
                dgvLang.CurrentRow.DefaultCellStyle.ForeColor = Drawing.Color.Black
            End If
        End If
    End Sub
    Private Sub dgvLang_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvLang.SelectionChanged
        If dgvLang.SelectedRows.Count > 0 AndAlso Not dgvLang.CurrentRow.Cells(1).Value Is Nothing Then
            btnRemoveLang.Enabled = True
        Else
            btnRemoveLang.Enabled = False
        End If
    End Sub

    Private Sub cbLangs_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbLangs.SelectedIndexChanged
        PopulateGenres()
    End Sub

    Private Sub btnAddGenre_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddGenre.Click
        Dim g As New xGenre
        Dim i As Integer = dgvGenres.Rows.Add(New Object() {String.Empty})
        dgvGenres.Rows(i).Tag = g
        xmlGenres.listOfGenres.Add(g)
        dgvGenres.CurrentCell = dgvGenres.Rows(i).Cells(0)
        pbIcon.Image = Nothing
        dgvLang.ClearSelection()
        ClearLangSelection()
        dgvGenres.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub

    Private Sub btnAddLang_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddLang.Click
        Dim s As String = InputBox(Master.eLang.GetString(3, "Enter the new Language"), Master.eLang.GetString(4, "New Language"))
        Dim i As Integer = dgvLang.Rows.Add(New Object() {False, s})
        dgvLang.CurrentCell = dgvLang.Rows(i).Cells(1)
        dgvLang.BeginEdit(True)
        RaiseEvent ModuleSettingsChanged()
    End Sub
    Private Sub btnRemoveGenre_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveGenre.Click
        If dgvGenres.SelectedCells.Count > 0 Then
            dgvGenres.Rows.RemoveAt(dgvGenres.SelectedCells(0).RowIndex)
            RaiseEvent ModuleSettingsChanged()
        End If
    End Sub

    Private Sub btnRemoveLang_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveLang.Click
        If MsgBox(Master.eLang.GetString(5, "This will remove the Language from all Genres. Are you sure?"), MsgBoxStyle.YesNo, Master.eLang.GetString(6, "Remove Language")) = MsgBoxResult.Yes Then
            If dgvLang.SelectedRows.Count > 0 AndAlso Not dgvLang.CurrentRow.Cells(1).Value Is Nothing Then
                Dim lang As String = dgvLang.SelectedRows(0).Cells(1).Value.ToString
                dgvLang.Rows.Remove(dgvLang.SelectedRows(0))
                xmlGenres.listOfLanguages.Remove(lang)
                GetLanguages()
                RaiseEvent ModuleSettingsChanged()
                For Each g As xGenre In xmlGenres.listOfGenres
                    If g.Langs.Contains(lang) Then
                        g.Langs.Remove(lang)
                        If g.Langs.Count = 0 Then
                            For Each d As DataGridViewRow In dgvGenres.Rows
                                If d.Cells(0).Value.ToString = g.searchstring Then
                                    d.DefaultCellStyle.ForeColor = Drawing.Color.Red
                                End If
                            Next
                        End If
                    End If
                Next
            End If
        End If
    End Sub

    Private Sub btnChangeImg_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChangeImg.Click
        Try
            Using fbImages As New OpenFileDialog
                fbImages.InitialDirectory = Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}", Path.DirectorySeparatorChar))
                fbImages.Filter = "Jpeg|*.jpg|PNG|*.png|GIF|*.gif"
                fbImages.ShowDialog()
                Dim g As xGenre = DirectCast(dgvGenres.CurrentRow.Tag, xGenre)
                g.icon = Path.GetFileName(fbImages.FileName)
                pbIcon.Load(Path.Combine(Functions.AppPath, String.Format("Images{0}Genres{0}{1}", Path.DirectorySeparatorChar, g.icon)))
                RaiseEvent ModuleSettingsChanged()
            End Using
        Catch ex As Exception
        End Try

    End Sub

    Private Sub SetUp()
        btnAddGenre.Text = Master.eLang.GetString(28, "Add", True)
        btnAddLang.Text = Master.eLang.GetString(28, "Add", True)
        btnRemoveGenre.Text = Master.eLang.GetString(30, "Remove", True)
        btnRemoveLang.Text = Master.eLang.GetString(30, "Remove", True)
        btnChangeImg.Text = Master.eLang.GetString(8, "Change")
        GroupBox1.Text = Master.eLang.GetString(9, "Images")
        Label1.Text = Master.eLang.GetString(10, "Genres Filter")
        Me.dgvGenres.Columns(0).HeaderText = Master.eLang.GetString(20, "Genres", True)
        Me.dgvLang.Columns(1).HeaderText = Master.eLang.GetString(7, "Languages")

    End Sub


#Region "Nested Types"
    <XmlRoot("genres")> _
    Class xGenres
        <XmlArray("supported")> _
        <XmlArrayItem("language")> _
        Public listOfLanguages As New List(Of String)
        <XmlElement("name")> _
        Public listOfGenres As New List(Of xGenre)
        <XmlElement("default")> _
        Public defaulticon As New xDefaulticon

        Public Shared Function Load(ByVal fpath As String) As xGenres
            Dim conf As xGenres = Nothing
            Try
                If Not File.Exists(fpath) Then Return New xGenres
                Dim xmlSer As XmlSerializer
                xmlSer = New XmlSerializer(GetType(xGenres))
                Using xmlSW As New StreamReader(Path.Combine(Functions.AppPath, fpath))
                    conf = DirectCast(xmlSer.Deserialize(xmlSW), xGenres)
                End Using
                For i As Integer = 0 To conf.listOfGenres.Count - 1
                    If Not IsNothing(conf.listOfGenres(i).language) Then
                        conf.listOfGenres(i).Langs.AddRange(conf.listOfGenres(i).language.Split(Convert.ToChar("|")))
                    End If
                Next
            Catch ex As Exception
            End Try
            Return conf
        End Function
        Public Sub Save(ByVal fpath As String)
            For i As Integer = 0 To Me.listOfGenres.Count - 1
                Me.listOfGenres(i).language = Strings.Join(Me.listOfGenres(i).Langs.ToArray, "|")
            Next
            Dim xmlSer As New XmlSerializer(GetType(xGenres))
            Using xmlSW As New StreamWriter(fpath)
                xmlSer.Serialize(xmlSW, Me)
            End Using
        End Sub
    End Class
    Class xDefaulticon
        <XmlText()> _
       <XmlElement()> _
        Public icon As String
    End Class
    Class xGenre
        <XmlIgnore()> _
        Public Langs As New List(Of String)
        <XmlAttribute("searchstring")> _
        Public searchstring As String
        <XmlAttribute("language")> _
        Public language As String
        <XmlText()> _
        <XmlElement()> _
        Public icon As String
    End Class
#End Region 'Nested Types

End Class
