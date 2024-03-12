Imports System.Windows.Forms
Imports EmberAPI
Imports System.IO

Public Class dlgXBMCHost
    Public XComs As New List(Of XBMCxCom.XBMCCom)
    Public hostid As String = Nothing
    Private xCom As New XBMCxCom.XBMCCom
    Private xc As New XBMCxCom.XBMCCom
    Private XBMCSources As New List(Of String)
    Private RemotePathSeparator As String = String.Empty
    Private Paths As New Hashtable
    Friend WithEvents bwLoadInfo As New System.ComponentModel.BackgroundWorker

    Structure EmberSource
        Dim Path As String
        Dim ElemCounts As Integer
        Dim XBMCSource As Hashtable
    End Structure
    Private EmberSources As New List(Of EmberSource)

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Try

            If Not String.IsNullOrEmpty(txtName.Text) Then
                For Each row As DataGridViewRow In dgvSources.Rows
                    If Not Paths.ContainsKey(row.Cells(0).Value.ToString) Then
                        Paths.Add(row.Cells(0).Value.ToString, If(row.Cells(1).Value Is Nothing, String.Empty, row.Cells(1).Value.ToString))
                    Else
                        Paths(row.Cells(0).Value.ToString) = If(row.Cells(1).Value Is Nothing, String.Empty, row.Cells(1).Value.ToString)
                    End If

                Next
                'have to iterate the list instead of using .comtains so we can convert each to lower case

                For i As Integer = 0 To XComs.Count - 1
                    If XComs(i).Name.ToLower = Me.txtName.Text.ToLower AndAlso Not XComs(i) Is xCom Then
                        MsgBox(Master.eLang.GetString(1, "The name you are attempting to use for this XBMC installation is already in use. Please choose another."), MsgBoxStyle.Exclamation, Master.eLang.GetString(2, "Each name must be unique"))
                        txtName.Focus()
                        Exit Sub
                    End If
                Next
                If Not String.IsNullOrEmpty(txtIP.Text) Then
                    If Not String.IsNullOrEmpty(txtPort.Text) Then
                        Me.xCom.Name = Me.txtName.Text
                        Me.xCom.IP = Me.txtIP.Text
                        Me.xCom.Port = Me.txtPort.Text
                        Me.xCom.Username = Me.txtUsername.Text
                        Me.xCom.Password = Me.txtPassword.Text
                        Me.xCom.Paths = Paths
                        Me.xCom.RemotePathSeparator = If(rbWindows.Checked, Path.DirectorySeparatorChar, "/")
                        Me.xCom.RealTime = chkRealTime.Checked
                        'XComs.Add(xCom)
                    Else
                        MsgBox(Master.eLang.GetString(3, "You must enter a port for this XBMC installation."), MsgBoxStyle.Exclamation, Master.eLang.GetString(4, "Please Enter a Port"))
                        txtPort.Focus()
                        Return
                    End If
                Else
                    MsgBox(Master.eLang.GetString(5, "You must enter an IP for this XBMC installation."), MsgBoxStyle.Exclamation, Master.eLang.GetString(6, "Please Enter an IP"))
                    txtIP.Focus()
                    Return
                End If
            Else
                MsgBox(Master.eLang.GetString(7, "You must enter a name for this XBMC installation."), MsgBoxStyle.Exclamation, Master.eLang.GetString(8, "Please Enter a Unique Name"))
                txtName.Focus()
                Return
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()

    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Sub Setup()
        Me.Text = Master.eLang.GetString(29, "XBMC Host")
        Me.Label16.Text = Master.eLang.GetString(11, "Name:")
        Me.Label13.Text = Master.eLang.GetString(425, "Username:", True)
        Me.Label14.Text = Master.eLang.GetString(426, "Password:", True)
        Me.Label7.Text = Master.eLang.GetString(13, "XBMC IP:")
        Me.Label6.Text = Master.eLang.GetString(14, "XBMC Port:")
        Me.chkRealTime.Text = Master.eLang.GetString(21, "Real Time synchronization")
        Me.Label3.Text = Master.eLang.GetString(22, "Searching ...")
        Me.btnPopulate.Text = Master.eLang.GetString(23, "Populate Sources")
        Me.dgvSources.Columns(0).HeaderText = Master.eLang.GetString(24, "Ember Source")
        Me.dgvSources.Columns(1).HeaderText = Master.eLang.GetString(25, "XBMC Source")
        Me.GroupBox1.Text = Master.eLang.GetString(26, "XBMC Source type")
        Me.rbWindows.Text = Master.eLang.GetString(27, "Windows Drive Letter (X:\)")
        Me.rbLinux.Text = Master.eLang.GetString(28, "Windows UNC/Linux/MacOS X")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
    End Sub

    Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.


    End Sub

    Sub PopulateSources()
        Try

            dgvSources.Rows.Clear()
            Dim sPath As String
            For Each s As Structures.MovieSource In Master.MovieSources
                Try
                    sPath = s.Path
                    Dim i As Integer = dgvSources.Rows.Add(sPath)
                    Dim dcb As DataGridViewComboBoxCell = DirectCast(dgvSources.Rows(i).Cells(1), DataGridViewComboBoxCell)
                    Dim l As New List(Of String)
                    l.Add("") 'Empty Entrie for combo
                    If Not xCom.Paths Is Nothing Then
                        For Each sp As Object In xCom.Paths.Values
                            If Not String.IsNullOrEmpty(sp.ToString) AndAlso Not l.Contains(sp.ToString) Then l.Add(sp.ToString)
                        Next
                    End If
                    dcb.DataSource = l.ToArray
                    If Not xCom.Paths Is Nothing AndAlso xCom.Paths.Count > 0 AndAlso Not sPath Is Nothing AndAlso Not xCom.Paths(sPath) Is Nothing Then
                        dcb.Value = xCom.Paths(sPath).ToString
                    End If

                Catch ex As Exception
                    'Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub


    Private Sub dlgXBMCHost_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Setup()
        xCom = XComs.FirstOrDefault(Function(y) y.Name = hostid)
        If Not xCom Is Nothing Then
            Try
                Paths = xCom.Paths
                Me.txtName.Text = xCom.Name
                Me.txtIP.Text = xCom.IP
                Me.txtPort.Text = xCom.Port
                Me.txtUsername.Text = xCom.Username
                Me.txtPassword.Text = xCom.Password
                If xCom.RemotePathSeparator = Path.DirectorySeparatorChar Then
                    Me.rbWindows.Checked = True
                Else
                    Me.rbLinux.Checked = True
                End If
                RemotePathSeparator = xCom.RemotePathSeparator
                chkRealTime.Checked = xCom.RealTime
                PopulateSources()
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            xCom = New XBMCxCom.XBMCCom
            XComs.Add(xCom)
            PopulateSources()
        End If

        dgvSources.Enabled = True
    End Sub

    Public Shared Function XBMCGetSources(ByVal xc As XBMCxCom.XBMCCom) As List(Of String)
        Dim cmd As String = "command=fileDownload(special://profile/sources.xml)"
        Dim updateXML As String
        Dim listSources As New List(Of String)
        Try
            updateXML = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(XBMCxCom.SendCmd(xc, cmd)))
            If updateXML.Length > 0 Then
                Dim n As String = String.Empty
                Dim xmlUpdate As XDocument
                Try
                    xmlUpdate = XDocument.Parse(updateXML)
                Catch
                    Return listSources
                End Try
                Dim xUdpate = From xUp In xmlUpdate...<video>...<source>...<path> Select xUp.Value
                Try
                    For Each x As String In xUdpate
                        listSources.Add(x)
                    Next
                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return listSources
    End Function

    Private Sub btnPopulate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPopulate.Click
        pnlLoading.Visible = True
        GroupBox1.Enabled = False
        btnPopulate.Enabled = False
        OK_Button.Enabled = False
        Cancel_Button.Enabled = False
        txtIP.Enabled = False
        txtName.Enabled = False
        txtPassword.Enabled = False
        txtPort.Enabled = False
        txtUsername.Enabled = False
        dgvSources.Enabled = False
        EmberSources.Clear()
        If Paths Is Nothing Then Paths = New Hashtable
        Paths.Clear()

        xc = New XBMCxCom.XBMCCom With {.Name = txtName.Text, .IP = txtIP.Text, .Port = txtPort.Text, .Username = txtUsername.Text, .Password = txtPassword.Text, .RealTime = chkRealTime.Checked}
        bwLoadInfo.RunWorkerAsync()
        While bwLoadInfo.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While
        If XBMCSources.Count > 0 Then
            Try
                dgvSources.Rows.Clear()
                For Each s As Structures.MovieSource In Master.MovieSources
                    Dim sPath As String = s.Path
                    Dim i As Integer = dgvSources.Rows.Add(sPath)
                    Dim dcb As DataGridViewComboBoxCell = DirectCast(dgvSources.Rows(i).Cells(1), DataGridViewComboBoxCell)
                    Dim tmp As List(Of String) = New List(Of String)(New String() {""})
                    tmp.AddRange(XBMCSources)
                    dcb.DataSource = tmp.ToArray
                    Dim es As EmberSource = EmberSources.FirstOrDefault(Function(y) y.Path = sPath)
                    ' If it match > 90% of the movies
                    If Convert.ToInt32(es.XBMCSource(getMaxSourceCount(es.XBMCSource))) > es.ElemCounts * 0.9 Then
                        dcb.Value = getMaxSourceCount(es.XBMCSource)
                        Paths.Add(es.Path, dcb.Value)
                    Else
                        Paths.Add(es.Path, "")
                    End If
                Next
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

        End If
        pnlLoading.Visible = False
        btnPopulate.Enabled = True
        OK_Button.Enabled = True
        Cancel_Button.Enabled = True
        txtIP.Enabled = True
        txtName.Enabled = True
        txtPassword.Enabled = True
        txtPort.Enabled = True
        txtUsername.Enabled = True
        dgvSources.Enabled = True
        GroupBox1.Enabled = True

    End Sub
    Private Sub bwLoadInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadInfo.DoWork
        SourceGuessing()
    End Sub
    Function getMaxSourceCount(ByVal h As Hashtable) As String
        Dim cur As String = String.Empty
        Dim curV As Integer = 0

        For Each s As String In h.Keys
            If Convert.ToInt32(h(s)) > curV Then
                cur = s
                curV = Convert.ToInt32(h(s))
            End If
        Next
        Return cur
    End Function

    Sub SourceGuessing()
        Dim files As List(Of String()) = Nothing
        XBMCSources = XBMCGetSources(xc)
        If XBMCSources.Count = 0 Then Return
        '"command=queryvideodatabase(select movie.*,path.strpath,files.strfilename,path.strcontent,path.strHash from movie inner join files on movie.idfile=files.idfile inner join path on files.idpath = path.idpath)"
        Dim cmd As String = "command=queryvideodatabase(select movie.*,path.strpath,files.strfilename from movie inner join files on movie.idfile=files.idfile inner join path on files.idpath = path.idpath)"
        files = XBMCxCom.SplitResponse(XBMCxCom.SendCmd(xc, cmd))

        For Each s As Structures.MovieSource In Master.MovieSources
            Dim myPaths As New List(Of String)
            For Each di As String In Master.DB.GetMoviePathsBySource(s.Path)
                myPaths.Add(GetFilePath(s.Path, di))
            Next

            Dim es As New EmberSource With {.Path = s.Path, .XBMCSource = New Hashtable, .ElemCounts = myPaths.Count}
            For Each xs As String In XBMCSources
                es.XBMCSource.Add(xs, 0) 'Populate Hashtable
            Next

            For Each f As String() In files.Where(Function(y) y.Count >= 24)
                Dim fPath As String = String.Concat(f(23), f(24))
                Dim xs As String = XBMCSources.FirstOrDefault(Function(y) fPath.StartsWith(y))
                If fPath.StartsWith(xs) AndAlso myPaths.Contains(GetFilePath(xs, fPath, RemotePathSeparator)) Then
                    es.XBMCSource(xs) = Convert.ToInt32(es.XBMCSource(xs)) + 1
                End If
            Next
            EmberSources.Add(es)
        Next
    End Sub
    Function GetFilePath(ByVal base As String, ByVal mypath As String, Optional ByVal sep As String = "") As String
        If String.IsNullOrEmpty(sep) Then sep = Path.DirectorySeparatorChar
        If mypath.StartsWith(base) Then mypath = mypath.Substring(base.Length).Replace(sep, Path.DirectorySeparatorChar)
        If mypath.StartsWith(Path.DirectorySeparatorChar) Then mypath = mypath.Substring(1)
        Return mypath
    End Function

    Private Sub rbLinux_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbLinux.CheckedChanged
        Me.xCom.RemotePathSeparator = If(rbWindows.Checked, Path.DirectorySeparatorChar, "/")
        RemotePathSeparator = Me.xCom.RemotePathSeparator
    End Sub

    Private Sub rbWindows_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbWindows.CheckedChanged
        Me.xCom.RemotePathSeparator = If(rbWindows.Checked, Path.DirectorySeparatorChar, "/")
        RemotePathSeparator = Me.xCom.RemotePathSeparator
    End Sub

    Private Sub dgvSources_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.CurrentCellDirtyStateChanged
        If dgvSources.IsCurrentCellDirty Then
            dgvSources.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub
End Class
