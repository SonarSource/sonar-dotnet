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
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Schema
Imports System.Text
Imports EmberAPI

Public Class dlgManualEdit

#Region "Fields"

    Private Changed As Boolean = False
    Private currFile As String
    Private DtdDt As DataTable
    Private ErrStr As String
    Private IsValid As Boolean
    Private lineInf As IXmlLineInfo
    Private ReturnOK As Boolean = False
    Private TagStack As New Stack()

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal nfoPath As String) As Windows.Forms.DialogResult
        Me.currFile = nfoPath

        Return MyBase.ShowDialog()
    End Function

    Private Function ConstructTag(ByVal ElementNameParam As String) As String
        Dim ElementName As String
        ElementName = ElementNameParam
        Dim myRow As DataRow

        Try

            Dim currRows() As DataRow = DtdDt.Select(Nothing, Nothing, DataViewRowState.CurrentRows)

            If (currRows.Length < 1) Then
                RichTextBox1.Text += "No Current Rows Found"
            Else

                For Each myRow In currRows
                    If myRow(2).ToString = "Att" Then

                        If myRow(0).ToString = ElementNameParam.Trim Then
                            ElementName = ElementName + " " + myRow(1).ToString + "="" """
                        End If
                    End If

                Next
            End If

            ElementName += ">"

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return ElementName
    End Function

    Private Sub Editor_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        RichTextBox1.Focus()
    End Sub

    Private Sub Editor_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If Changed = True Then
            Dim DResult As MsgBoxResult
            DResult = MsgBox(Master.eLang.GetString(196, "Do you want to save changes?"), MsgBoxStyle.YesNoCancel, Master.eLang.GetString(197, "Save?"))
            If DResult = MsgBoxResult.Yes Then
                File.WriteAllText(currFile, RichTextBox1.Text, Encoding.UTF8)
                'RichTextBox1.SaveFile(currFile, RichTextBoxStreamType.PlainText)
                Me.DialogResult = Windows.Forms.DialogResult.OK

            ElseIf DResult = MsgBoxResult.Cancel Then

                e.Cancel = True

            End If

        Else

            If ReturnOK Then Me.DialogResult = Windows.Forms.DialogResult.OK

        End If
    End Sub

    Private Sub Editor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()

        If File.Exists(currFile) Then
            RichTextBox1.Text = File.ReadAllText(currFile, Encoding.UTF8)
            'RichTextBox1.LoadFile(currFile, RichTextBoxStreamType.PlainText)
        End If
        Me.Text = String.Concat(Master.eLang.GetString(190, "Manual NFO Editor | "), currFile.Substring(currFile.LastIndexOf(Path.DirectorySeparatorChar) + 1))

        Changed = False

        Me.Activate()
    End Sub

    Private Sub IndentFormat()
        Try
            Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

            Dim tempfile As String = Path.GetTempPath + "nfo-uf.tmp"
            File.WriteAllText(tempfile, RichTextBox1.Text, Encoding.UTF8)
            'RichTextBox1.SaveFile(tempfile, RichTextBoxStreamType.PlainText)

            Dim IfErr As Boolean = False
            Dim StrR As New StreamReader(tempfile)
            Dim StrW As New StreamWriter(Path.GetTempPath + "nfo.tmp", False)
            Dim AllData As String = StrR.ReadToEnd
            Dim m As Match

            Dim TagS As String, i As Integer

            'Converting entire file to a single line

            AllData = AllData.Replace(vbNewLine, String.Empty)
            AllData = AllData.Replace(vbCrLf, String.Empty)
            AllData = AllData.Replace(vbLf, String.Empty)
            AllData = AllData.Replace(vbCr, String.Empty)
            AllData = AllData.Replace(vbTab, String.Empty).Trim

            'Looking for Processing Instruction and DTD declaration

            For i = 0 To 3 'We assume only first 4 lines have Processing Instruction and DTD declaration
                m = Regex.Match(AllData, "^\<\?([^>]+)\>", RegexOptions.IgnoreCase) 'go to MSDN for RegularExpression Help
                TagS = String.Empty
                If m.Success Then
                    TagS = Regex.Replace(AllData, "^\<\?([^>]+)\>(.*)", "<?$1>", RegexOptions.IgnoreCase)
                    AllData = Regex.Replace(AllData, "^\<\?([^>]+)\>(.*)", "$2", RegexOptions.IgnoreCase)
                    StrW.WriteLine(TagS)
                Else
                    m = Regex.Match(AllData, "^\<\!DOCTYPE([^>]+)\>", RegexOptions.IgnoreCase)
                    If m.Success Then
                        TagS = Regex.Replace(AllData, "^\<\!DOCTYPE([^>]+)\>(.*)", "<!DOCTYPE$1>", RegexOptions.IgnoreCase)
                        AllData = Regex.Replace(AllData, "^\<\!DOCTYPE([^>]+)\>(.*)", "$2", RegexOptions.IgnoreCase)
                        StrW.WriteLine(TagS)
                    End If

                End If

            Next

            Dim LevelX, j As Integer, TabC As String

            Do
                TagS = String.Empty
                TabC = String.Empty

                m = Regex.Match(AllData, "^\<([^>]+) \/\>") 'Single Tag
                If m.Success Then

                    TagS = Regex.Replace(AllData, "^\<([^>]+) \/\>(.*)", "<$1 />").Trim
                    AllData = Regex.Replace(AllData, "^\<([^>]+) \/\>(.*)", "$2").Trim

                    For j = 1 To LevelX
                        TabC += vbTab
                    Next
                    StrW.WriteLine(String.Concat(TabC, TagS))
                Else
                    m = Regex.Match(AllData, "^\<([^>/]+)\>([^<]+)\<\/([^>/]+)\>") 'Opening Tag
                    If m.Success Then
                        TagS = Regex.Replace(AllData, "^\<([^>/]+)\>([^<]+)\<\/([^>/]+)\>(.*)", "<$1>$2</$3>").Trim

                        AllData = Regex.Replace(AllData, "^\<([^>/]+)\>([^<]+)\<\/([^>/]+)\>(.*)", "$4").Trim

                        For j = 1 To LevelX 'Calculating depth of tag
                            TabC += vbTab
                        Next
                        StrW.WriteLine(String.Concat(TabC, TagS))
                    Else
                        m = Regex.Match(AllData, "^\<\/([^>]+)\>(.*)") 'Closing Tag
                        If m.Success Then

                            TagS = Regex.Replace(AllData, "^\<\/([^>]+)\>(.*)", "</$1>").Trim
                            AllData = Regex.Replace(AllData, "^\<\/([^>]+)\>(.*)", "$2").Trim
                            LevelX -= 1

                            For j = 1 To LevelX
                                TabC += vbTab
                            Next

                            StrW.WriteLine(String.Concat(TabC, TagS))

                        Else
                            m = Regex.Match(AllData, "^\<([^>]+)\>(.*)")
                            If m.Success Then
                                TagS = Regex.Replace(AllData, "^\<([^>]+)\>(.*)", "<$1>").Trim
                                AllData = Regex.Replace(AllData, "^\<([^>]+)\>(.*)", "$2").Trim
                                LevelX += 1
                                For j = 1 To LevelX - 1
                                    TabC += vbTab
                                Next

                                StrW.WriteLine(String.Concat(TabC, TagS))

                            Else
                                m = Regex.Match(AllData, "^([^<]+)\<")
                                If m.Success Then
                                    TagS = Regex.Replace(AllData, "^([^<]+)\<(.*)", "$1").Trim

                                    AllData = Regex.Replace(AllData, "^([^<]+)\<(.*)", "<$2").Trim

                                    For j = 0 To LevelX - 1
                                        TabC += vbTab
                                    Next

                                    StrW.WriteLine(String.Concat(TabC, TagS))

                                Else
                                    MsgBox(Master.eLang.GetString(191, "This is not a proper XML document"), MsgBoxStyle.Information)
                                    IfErr = True
                                    Exit Do

                                End If

                            End If

                        End If

                    End If

                End If

                If AllData.Length < 2 Then
                    Exit Do
                End If

            Loop While True

            StrR.Close()
            StrW.Close()

            If IfErr = False Then
                RichTextBox1.LoadFile(Path.GetTempPath + "nfo.tmp", RichTextBoxStreamType.PlainText)
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Cursor = System.Windows.Forms.Cursors.Default
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        Dim SelItem As String
        Dim linN, colN As Integer

        SelItem = ListBox1.SelectedItem.ToString

        If Not String.IsNullOrEmpty(SelItem) Then

            linN = CType(Regex.Replace(SelItem, "([0-9]+): ([0-9]+)(.*)", "$1"), Integer)
            colN = CType(Regex.Replace(SelItem, "([0-9]+): ([0-9]+)(.*)", "$2"), Integer)

            Dim mc As MatchCollection
            Dim i As Integer = 0

            mc = Regex.Matches(RichTextBox1.Text, "\n", RegexOptions.Singleline)

            Try
                RichTextBox1.Select(mc(linN - 2).Index + colN, 2)
                RichTextBox1.SelectionColor = Color.Blue
                RichTextBox1.Focus()

            Catch ex As Exception
                RichTextBox1.Focus()
            End Try
        End If
    End Sub

    Private Sub mnuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExit.Click
        Me.Close()
    End Sub

    Private Sub mnuFormat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFormat.Click
        IndentFormat()
    End Sub

    Private Sub mnuParse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuParse.Click
        ParseFile()
    End Sub

    Private Sub mnuSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSave.Click
        File.WriteAllText(currFile, RichTextBox1.Text, Encoding.UTF8)
        'RichTextBox1.SaveFile(currFile, RichTextBoxStreamType.PlainText)
        ReturnOK = True
        Changed = False
    End Sub

    Private Sub ParseFile()
        If currFile Is Nothing Then
            Exit Sub
        End If

        Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

        Dim tempFile As String = Path.GetTempPath + "nfo.tmp"
        File.WriteAllText(tempFile, RichTextBox1.Text, Encoding.UTF8)
        'RichTextBox1.SaveFile(tempFile, RichTextBoxStreamType.PlainText)

        Dim xmlP As New XmlTextReader(tempFile)
        ' Set the validation settings.
        Dim settings As XmlReaderSettings = New XmlReaderSettings()
        settings.ValidationType = ValidationType.Schema
        settings.ValidationFlags = settings.ValidationFlags Or XmlSchemaValidationFlags.ProcessInlineSchema
        settings.ValidationFlags = settings.ValidationFlags Or XmlSchemaValidationFlags.ReportValidationWarnings

        Dim xmlV As XmlReader = XmlReader.Create(xmlP, settings)
        ErrStr = String.Empty
        ListBox1.Items.Clear()

        IsValid = True

        Do
            Try
                If xmlV.Read() Then
                    lineInf = CType(xmlV, IXmlLineInfo)
                End If

            Catch exx As Exception

                Try

                    IsValid = False

                    If lineInf.HasLineInfo Then
                        ErrStr = lineInf.LineNumber.ToString + ": " + lineInf.LinePosition.ToString + " " + exx.Message
                    End If

                    If exx.Message.IndexOf("EndElement") > 1 Then
                        Exit Do
                    End If

                    ListBox1.Items.Add(ErrStr)

                Catch ex As Exception
                    Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                    Exit Do
                End Try

            End Try

        Loop While Not xmlP.EOF

        xmlV.Close()
        xmlP.Close()

        Me.Cursor = System.Windows.Forms.Cursors.Default

        If IsValid = False Then
            MsgBox(Master.eLang.GetString(192, "File is not valid."), MsgBoxStyle.Exclamation, Master.eLang.GetString(194, "Not Valid"))
        Else
            MsgBox(Master.eLang.GetString(193, "File is valid."), MsgBoxStyle.Information, Master.eLang.GetString(195, "Valid"))
        End If
    End Sub

    Private Sub RichTextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RichTextBox1.TextChanged
        Changed = True
    End Sub

    Private Sub SetUp()
        Me.mnuFormat.Text = Master.eLang.GetString(187, "&Format / Indent")
        Me.mnuParse.Text = Master.eLang.GetString(188, "&Parse")
        Me.MenuItem19.Text = Master.eLang.GetString(8, "&Tools")
        Me.mnuFile.Text = Master.eLang.GetString(1, "&File")
        Me.mnuSave.Text = Master.eLang.GetString(189, "&Save")
        Me.mnuExit.Text = Master.eLang.GetString(2, "E&xit")
    End Sub

    Private Sub WriteErrorLog(ByVal sender As Object, ByVal args As ValidationEventArgs)
        IsValid = False
        ErrStr = lineInf.LineNumber.ToString + ": " + lineInf.LinePosition.ToString + " " + args.Message
        ListBox1.Items.Add(ErrStr)
    End Sub

#End Region 'Methods

End Class