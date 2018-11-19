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
Imports ICSharpCode.SharpZipLib.Zip
Imports System.Xml.Serialization
Imports EmberAPI

Public Class frmSettingsHolder
    Private confs As New List(Of NMTExporterModule.Config)
    Private conf As NMTExporterModule.Config
    Private sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
#Region "Events"

    Public Event ModuleEnabledChanged(ByVal State As Boolean)

    Public Event ModuleSettingsChanged()

#End Region 'Events

#Region "Methods"

    Private Sub cbEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbEnabled.CheckedChanged
        RaiseEvent ModuleEnabledChanged(cbEnabled.Checked)
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.SetUp()
        LoadTemplates(True)

    End Sub

    Sub LoadTemplates(Optional ByVal withNew As Boolean = False)
        Dim fxml As String
        lstTemplates.Items.Clear()
        lstTemplates.ShowItemToolTips = True
        Dim di As DirectoryInfo = New DirectoryInfo(Path.Combine(sBasePath, "Templates"))
        For Each i As DirectoryInfo In di.GetDirectories
            If Not (i.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then
                fxml = Path.Combine(sBasePath, String.Concat("Templates", Path.DirectorySeparatorChar, i.Name))
                conf = NMTExporterModule.Config.Load(Path.Combine(fxml, "config.xml"))
                If Not conf Is Nothing AndAlso Not String.IsNullOrEmpty(conf.Name) Then
                    conf.TemplatePath = fxml
                    conf.ReadMe = File.Exists(Path.Combine(conf.TemplatePath, "readme.txt"))
                    confs.Add(conf)
                    Dim li As New ListViewItem(conf.Name)
                    Dim status As String
                    Try
                        If Convert.ToSingle(conf.DesignVersion.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)) < NMTExporterModule.MinDesignVersion Then
                            status = Master.eLang.GetString(22, "Outdated")
                        Else
                            status = Master.eLang.GetString(18, "Installed")
                        End If
                    Catch ex As Exception
                        status = Master.eLang.GetString(22, "Outdated")
                    End Try

                    li.SubItems.AddRange(New String() {conf.Version.ToString, conf.Author.ToString, status})
                    li.ToolTipText = conf.Description
                    li.Tag = conf
                    lstTemplates.Items.Add(li)
                End If
            End If
        Next
        If withNew Then
            For Each i As FileInfo In di.GetFiles
                If i.Extension = ".zip" Then
                    conf = ScanZip(i.FullName)
                    If Not conf Is Nothing Then
                        fxml = Path.Combine(sBasePath, String.Concat("Templates", Path.DirectorySeparatorChar, Path.GetFileName(i.Name)))
                        conf.TemplatePath = fxml
                        confs.Add(conf)
                        Dim li As New ListViewItem(conf.Name)
                        li.SubItems.AddRange(New String() {conf.Version.ToString, conf.Author.ToString, Master.eLang.GetString(19, "New")})
                        li.ToolTipText = conf.Description
                        li.Tag = conf
                        lstTemplates.Items.Add(li)
                    End If
                End If
            Next
        End If
    End Sub

    Private Function ScanZip(ByVal fname As String) As NMTExporterModule.Config
        Dim conf As NMTExporterModule.Config = Nothing
        Dim haveReadMe As Boolean = False
        Using fStream As FileStream = New FileStream(fname, FileMode.Open, FileAccess.Read)
            Dim fZip As Byte() = Functions.ReadStreamToEnd(fStream)
            Try
                Using zStream As ZipInputStream = New ZipInputStream(New MemoryStream(fZip))
                    Dim zEntry As ZipEntry = zStream.GetNextEntry
                    While Not IsNothing(zEntry)
                        Select Case zEntry.Name
                            Case "config.xml"
                                Dim xmlSer As XmlSerializer
                                xmlSer = New XmlSerializer(GetType(NMTExporterModule.Config))
                                Try
                                    conf = DirectCast(xmlSer.Deserialize(zStream), NMTExporterModule.Config)
                                Catch ex As Exception
                                End Try
                            Case "readme.txt"
                                haveReadMe = True
                        End Select
                        zEntry = zStream.GetNextEntry
                    End While
                End Using
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Using
        If Not conf Is Nothing Then
            conf.ReadMe = haveReadMe
        End If


        Return conf
    End Function

    Private Function GetReadmeZip(ByVal fname As String) As String
        Dim readme As String = String.Empty

        Using fStream As FileStream = New FileStream(fname, FileMode.Open, FileAccess.Read)
            Dim fZip As Byte() = Functions.ReadStreamToEnd(fStream)
            Try
                Using zStream As ZipInputStream = New ZipInputStream(New MemoryStream(fZip))
                    Dim zEntry As ZipEntry = zStream.GetNextEntry
                    While Not IsNothing(zEntry)
                        Select Case zEntry.Name
                            Case "readme.txt"
                                readme = System.Text.Encoding.UTF8.GetString(Functions.ReadStreamToEnd(zStream))
                                Exit While
                        End Select
                        zEntry = zStream.GetNextEntry
                    End While
                End Using
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End Using
        Return readme
    End Function

    Private Function UnZip(ByVal fname As String) As Boolean
        Dim baseFolder As String = Path.Combine(sBasePath, String.Concat("Templates", Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(fname)))
        If Directory.Exists(baseFolder) Then
            If MsgBox(Master.eLang.GetString(37, "Folder already exist. Overwrite?"), MsgBoxStyle.YesNo, Master.eLang.GetString(36, "Install Template")) = MsgBoxResult.No Then
                Return False
            Else
                Try
                    Directory.Delete(baseFolder, True)
                Catch ex As Exception
                    MsgBox(Master.eLang.GetString(38, "File/Folder in use! Can Not overwrite"), MsgBoxStyle.OkOnly, Master.eLang.GetString(36, "Install Template"))
                End Try
            End If
        End If
        Using fStream As FileStream = New FileStream(fname, FileMode.Open, FileAccess.Read)
            Dim fZip As Byte() = Functions.ReadStreamToEnd(fStream)
            Try
                Using zStream As ZipInputStream = New ZipInputStream(New MemoryStream(fZip))
                    Dim zEntry As ZipEntry = zStream.GetNextEntry
                    While Not IsNothing(zEntry)
                        Dim folderName As String = Path.Combine(baseFolder, Path.GetDirectoryName(zEntry.Name))
                        Dim fileName As String = Path.GetFileName(zEntry.Name)
                        Dim fFile As Byte() = Functions.ReadStreamToEnd(zStream)
                        If Not Directory.Exists(folderName) Then
                            Directory.CreateDirectory(folderName)
                        End If
                        If Not String.IsNullOrEmpty(fileName) Then File.WriteAllBytes(Path.Combine(folderName, fileName), fFile)
                        zEntry = zStream.GetNextEntry
                    End While
                End Using
            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                Return False
            End Try
        End Using
        Return True
    End Function

    Private Sub SetUp()
		Me.cbEnabled.Text = Master.eLang.GetString(774, "Enabled", True)
		Me.lstTemplates.Columns(0).Text = Master.eLang.GetString(4, "Template")
		Me.lstTemplates.Columns(1).Text = Master.eLang.GetString(43, "Version")
		Me.lstTemplates.Columns(2).Text = Master.eLang.GetString(44, "Author")
		Me.lstTemplates.Columns(3).Text = Master.eLang.GetString(45, "Status")
		Me.btnInstall.Text = Master.eLang.GetString(46, "Install")
		Me.btnRemove.Text = Master.eLang.GetString(47, "Remove")
		Me.GroupBox1.Text = Master.eLang.GetString(48, "Details")
	End Sub

    Sub RemoveTemplate()
        If lstTemplates.SelectedItems.Count > 0 Then
            Dim conf As NMTExporterModule.Config = DirectCast(lstTemplates.SelectedItems(0).Tag, NMTExporterModule.Config)
            If MsgBox(Master.eLang.GetString(27, "Removing Template can not be undone. Are you sure?"), MsgBoxStyle.YesNo, Master.eLang.GetString(28, "Remove Template")) = MsgBoxResult.Yes Then
                Select Case lstTemplates.SelectedItems(0).SubItems(3).Text
                    Case Master.eLang.GetString(18, "Installed")
                        Directory.Delete(conf.TemplatePath, True)
                    Case Master.eLang.GetString(19, "New")
                        File.Delete(conf.TemplatePath)
                End Select
            End If
        End If
    End Sub

    Sub InstallTemplate()
        If lstTemplates.SelectedItems.Count > 0 Then
            Dim conf As NMTExporterModule.Config = DirectCast(lstTemplates.SelectedItems(0).Tag, NMTExporterModule.Config)
            Select Case lstTemplates.SelectedItems(0).SubItems(3).Text
                Case Master.eLang.GetString(19, "New")
                    If UnZip(conf.TemplatePath) Then
                        File.Delete(conf.TemplatePath)
                        Dim wn As String = Path.Combine(Path.Combine(Path.GetDirectoryName(conf.TemplatePath), Path.GetFileNameWithoutExtension(conf.TemplatePath)), "WhatsNew.txt")
                        If conf.WhatsNew AndAlso File.Exists(wn) Then
                            Using dWN As New frmWhatsNew
                                dWN.txtWhatsNew.Text = File.ReadAllText(wn)
                                dWN.ShowDialog()
                            End Using
                        End If
                    End If
            End Select
        End If
    End Sub

    Private Sub lstTemplates_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstTemplates.SelectedIndexChanged
        Dim readme As String = String.Empty
        If lstTemplates.SelectedItems.Count > 0 Then
            Dim conf As NMTExporterModule.Config = DirectCast(lstTemplates.SelectedItems(0).Tag, NMTExporterModule.Config)

            Select Case lstTemplates.SelectedItems(0).SubItems(3).Text
                Case Master.eLang.GetString(18, "Installed")
                    btnRemove.Enabled = True
                    btnInstall.Enabled = False
                    If conf.ReadMe AndAlso File.Exists(Path.Combine(conf.TemplatePath, "readme.txt")) Then
                        readme = File.ReadAllText(Path.Combine(conf.TemplatePath, "readme.txt"))
                    End If
                Case Master.eLang.GetString(19, "New")
                    btnRemove.Enabled = True
                    btnInstall.Enabled = True
                    If conf.ReadMe Then readme = GetReadmeZip(conf.TemplatePath)
                Case Else
                    btnRemove.Enabled = False
                    btnInstall.Enabled = False
            End Select
        Else
            btnRemove.Enabled = False
            btnInstall.Enabled = False
            txtDetails.Text = String.Empty
        End If
        txtDetails.Text = readme
    End Sub


    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        RemoveTemplate()
        LoadTemplates(True)
    End Sub
    Private Sub btnInstall_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInstall.Click
        InstallTemplate()
        LoadTemplates(True)
    End Sub
#End Region 'Methods


End Class