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
Imports EmberAPI
Imports System.Windows.Forms

Public Class dlgImgSelect

#Region "Fields"

    Friend WithEvents bwDownload As New System.ComponentModel.BackgroundWorker

    Private CachePath As String = String.Empty
    Private chkImage() As CheckBox
    Private DLType As Enums.ImageType
    Private ETHashes As New List(Of String)
    Private iCounter As Integer = 0
    Private iLeft As Integer = 5
    Private MovieImages As New List(Of MediaContainers.Image)
    Private isEdit As Boolean = False
    Private isShown As Boolean = False
    Private iTop As Integer = 5
    Private lblImage() As Label
    Private noImages As Boolean = False
    Private pbImage() As PictureBox
    Private pnlImage() As Panel
    Private PreDL As Boolean = False
    Private Results As New Containers.ImgResult
    Private selIndex As Integer = -1
    Private tMovie As New Structures.DBMovie
    Private tmpImage As New Images

#End Region 'Fields

#Region "Events"

    Private Event EventImagesDone()

#End Region 'Events

#Region "Methods"

    Public Sub PreLoad(ByVal mMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, Optional ByVal _isEdit As Boolean = False)
        tMovie = mMovie
        DLType = _DLType
        isEdit = _isEdit
        PreDL = True
        SetUp()
        StartDownload()
    End Sub

    Public Overloads Function ShowDialog(ByVal mMovie As Structures.DBMovie, ByVal _DLType As Enums.ImageType, Optional ByVal _isEdit As Boolean = False) As Containers.ImgResult
        '//
        ' Overload to pass data
        '\\

        tMovie = mMovie
        DLType = _DLType
        isEdit = _isEdit
        isShown = True

        MyBase.ShowDialog()
        Return Results
    End Function

    Public Overloads Function ShowDialog() As Containers.ImgResult
        isShown = True
        MyBase.ShowDialog()

        Return Results
    End Function

    Private Sub AddImage(ByVal iImage As Image, ByVal sDescription As String, ByVal iIndex As Integer, ByVal sURL As String, ByVal isChecked As Boolean)
        Try
            ReDim Preserve pnlImage(iIndex)
            ReDim Preserve pbImage(iIndex)
            pnlImage(iIndex) = New Panel()
            pbImage(iIndex) = New PictureBox()
            pbImage(iIndex).Name = iIndex.ToString
            pnlImage(iIndex).Name = iIndex.ToString
            pnlImage(iIndex).Size = New Size(256, 286)
            pbImage(iIndex).Size = New Size(250, 250)
            pnlImage(iIndex).BackColor = Color.White
            pnlImage(iIndex).BorderStyle = BorderStyle.FixedSingle
            pbImage(iIndex).SizeMode = PictureBoxSizeMode.Zoom
            pnlImage(iIndex).Tag = sURL
            pbImage(iIndex).Tag = sURL
            pbImage(iIndex).Image = iImage
            pnlImage(iIndex).Left = iLeft
            pbImage(iIndex).Left = 3
            pnlImage(iIndex).Top = iTop
            pbImage(iIndex).Top = 3
            pnlBG.Controls.Add(pnlImage(iIndex))
            pnlImage(iIndex).Controls.Add(pbImage(iIndex))
            pnlImage(iIndex).BringToFront()
            AddHandler pbImage(iIndex).Click, AddressOf pbImage_Click
            AddHandler pbImage(iIndex).DoubleClick, AddressOf pbImage_DoubleClick
            AddHandler pnlImage(iIndex).Click, AddressOf pnlImage_Click

            AddHandler pbImage(iIndex).MouseWheel, AddressOf MouseWheelEvent
            AddHandler pnlImage(iIndex).MouseWheel, AddressOf MouseWheelEvent

            If DLType = Enums.ImageType.Fanart Then
                ReDim Preserve chkImage(iIndex)
                chkImage(iIndex) = New CheckBox()
                chkImage(iIndex).Name = iIndex.ToString
                chkImage(iIndex).Size = New Size(250, 30)
                chkImage(iIndex).AutoSize = False
                chkImage(iIndex).BackColor = Color.White
                chkImage(iIndex).TextAlign = ContentAlignment.MiddleCenter
                chkImage(iIndex).Text = String.Format("{0}x{1} ({2})", pbImage(iIndex).Image.Width.ToString, pbImage(iIndex).Image.Height.ToString, sDescription)
                chkImage(iIndex).Left = 0
                chkImage(iIndex).Top = 250
                chkImage(iIndex).Checked = isChecked
                pnlImage(iIndex).Controls.Add(chkImage(iIndex))
                AddHandler pnlImage(iIndex).MouseWheel, AddressOf MouseWheelEvent
            Else
                ReDim Preserve lblImage(iIndex)
                lblImage(iIndex) = New Label()
                lblImage(iIndex).Name = iIndex.ToString
                lblImage(iIndex).Size = New Size(250, 30)
                lblImage(iIndex).AutoSize = False
                lblImage(iIndex).BackColor = Color.White
                lblImage(iIndex).TextAlign = ContentAlignment.MiddleCenter
                'lblImage(iIndex).Text = Master.eLang.GetString(55, "Multiple")
                lblImage(iIndex).Text = String.Format("{0}x{1} ({2})", pbImage(iIndex).Image.Width.ToString, pbImage(iIndex).Image.Height.ToString, sDescription)

                lblImage(iIndex).Tag = sURL
                lblImage(iIndex).Left = 0
                lblImage(iIndex).Top = 250
                pnlImage(iIndex).Controls.Add(lblImage(iIndex))
                AddHandler lblImage(iIndex).Click, AddressOf lblImage_Click
                AddHandler lblImage(iIndex).MouseWheel, AddressOf MouseWheelEvent
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        iCounter += 1

        If iCounter = 3 Then
            iCounter = 0
            iLeft = 5
            iTop += 301
        Else
            iLeft += 271
        End If
    End Sub

    Private Sub AllDoneDownloading()
        ' If _impaDone AndAlso _tmdbDone AndAlso _mpdbDone Then
        pnlDLStatus.Visible = False
        'TMDBPosters.AddRange(Posters)
        'TMDBPosters.AddRange(MPDBPosters)
        ProcessPics(MovieImages)
        pnlBG.Visible = True
        'End If
    End Sub

    Private Sub btnPreview_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles btnPreview.Click
        Try
            Dim tImage As New Images

            pnlSinglePic.Visible = True
            Application.DoEvents()

            Select Case True
                Case rbXLarge.Checked
                    If Master.eSettings.UseImgCache Then
                        tImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(original)_(url=", rbXLarge.Tag, ").jpg")))
                    Else
                        tImage.FromWeb(rbXLarge.Tag.ToString)
                    End If
                Case rbLarge.Checked
                    If Master.eSettings.UseImgCache Then
                        tImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(mid)_(url=", rbLarge.Tag, ").jpg")))
                    Else
                        tImage.FromWeb(rbLarge.Tag.ToString)
                    End If
                Case rbMedium.Checked
                    If Master.eSettings.UseImgCache Then
                        tImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(cover)_(url=", rbMedium.Tag, ").jpg")))
                    Else
                        tImage.FromWeb(rbMedium.Tag.ToString)
                    End If
                Case rbSmall.Checked
                    If Master.eSettings.UseImgCache Then
                        tImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(thumb)_(url=", rbSmall.Tag, ").jpg")))
                    Else
                        tImage.FromWeb(rbSmall.Tag.ToString)
                    End If
            End Select

            pnlSinglePic.Visible = False

            If Not IsNothing(tImage.Image) Then

                ModulesManager.Instance.RuntimeObjects.InvokeOpenImageViewer(tImage.Image)

            End If

            tImage.Dispose()
        Catch ex As Exception
            pnlSinglePic.Visible = False
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwDownload_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownload.DoWork
        '//
        ' Thread to download impa posters from the internet (multi-threaded because sometimes
        ' the web server is slow to respond or not reachable, hanging the GUI)
        '\\

        For i As Integer = 0 To MovieImages.Count - 1
            If bwDownload.CancellationPending Then
                e.Cancel = True
                Return
            End If
            bwDownload.ReportProgress(i + 1, MovieImages.Item(i).URL)
            Try
                MovieImages.Item(i).WebImage.FromWeb(MovieImages.Item(i).URL)
                If Not Master.eSettings.NoSaveImagesToNfo Then Results.Posters.Add(MovieImages.Item(i).URL)
                If Master.eSettings.UseImgCache Then
                    Try
                        MovieImages.Item(i).URL = StringUtils.CleanURL(MovieImages.Item(i).URL)
                        MovieImages.Item(i).WebImage.Save(Path.Combine(CachePath, String.Concat("poster_(", MovieImages.Item(i).Description, ")_(url=", MovieImages.Item(i).URL, ").jpg")))
                    Catch
                    End Try
                End If
            Catch
            End Try
        Next
    End Sub

    Private Sub bwDownload_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwDownload.ProgressChanged
        '//
        ' Update the status bar with the name of the current media name and increase progress bar
        '\\
        Try
            Dim sStatus As String = e.UserState.ToString
            lblDL2Status.Text = String.Format(Master.eLang.GetString(27, "Downloading {0}"), If(sStatus.Length > 40, StringUtils.TruncateURL(sStatus, 40), sStatus))
            pbDL2.Value = e.ProgressPercentage
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwDownload_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownload.RunWorkerCompleted
        '//
        ' Thread finished: process the pics
        '\\
        If Not e.Cancelled Then
            RaiseEvent EventImagesDone()
        End If
    End Sub


    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles Cancel_Button.Click
        If bwDownload.IsBusy Then bwDownload.CancelAsync()
        While bwDownload.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub CheckAll(ByVal sType As String, ByVal Checked As Boolean)
        For i As Integer = 0 To UBound(chkImage)
            If chkImage(i).Text.ToLower.Contains(sType) Then
                chkImage(i).Checked = Checked
            End If
        Next
    End Sub

    Private Sub chkMid_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles chkMid.CheckedChanged
        CheckAll("(mid)", chkMid.Checked)
    End Sub

    Private Sub chkOriginal_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles chkOriginal.CheckedChanged
        CheckAll("(original)", chkOriginal.Checked)
    End Sub

    Private Sub chkThumb_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles chkThumb.CheckedChanged
        CheckAll("(thumb)", chkThumb.Checked)
    End Sub

    Private Sub dlgImgSelect_Disposed(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Disposed
        'tmpImage.Dispose()
        MovieImages = Nothing
    End Sub

    Private Sub dlgImgSelect_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        If Master.eSettings.AutoET AndAlso Not Master.eSettings.UseImgCache Then FileUtils.Delete.DeleteDirectory(CachePath)
    End Sub

    Private Sub dlgImgSelect_Load(ByVal sender As System.Object, ByVal e As EventArgs) Handles MyBase.Load
        If Not PreDL Then SetUp()
    End Sub

    Private Sub dlgImgSelect_Shown(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Shown
        Try
            Application.DoEvents()
            If Not PreDL Then
                StartDownload()
            ElseIf noImages Then
                If DLType = Enums.ImageType.Fanart Then
                    MsgBox(Master.eLang.GetString(28, "No Fanart found for this movie."), MsgBoxStyle.Information, Master.eLang.GetString(29, "No Fanart Found"))
                Else
                    MsgBox(Master.eLang.GetString(30, "No Posters found for this movie."), MsgBoxStyle.Information, Master.eLang.GetString(31, "No Posters Found"))
                End If
                DialogResult = DialogResult.Cancel
                Close()
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub DoSelect(ByVal iIndex As Integer, ByVal sURL As String)
        Try
            'set all pnl colors to white first
            'remove all the current genres
            For i As Integer = 0 To UBound(pnlImage)
                pnlImage(i).BackColor = Color.White

                If DLType = Enums.ImageType.Fanart Then
                    chkImage(i).BackColor = Color.White
                    chkImage(i).ForeColor = Color.Black
                Else
                    lblImage(i).BackColor = Color.White
                    lblImage(i).ForeColor = Color.Black
                End If
            Next

            'set selected pnl color to blue
            pnlImage(iIndex).BackColor = Color.Blue

            If DLType = Enums.ImageType.Fanart Then
                chkImage(iIndex).BackColor = Color.Blue
                chkImage(iIndex).ForeColor = Color.White
            Else
                lblImage(iIndex).BackColor = Color.Blue
                lblImage(iIndex).ForeColor = Color.White
            End If

            selIndex = iIndex

            pnlSize.Visible = False

            If Not DLType = Enums.ImageType.Fanart AndAlso sURL.ToLower.Contains("themoviedb.org") Then
                SetupSizes(sURL)
                If Not rbLarge.Checked AndAlso Not rbMedium.Checked AndAlso Not rbSmall.Checked AndAlso Not rbXLarge.Checked Then
                    OK_Button.Enabled = False
                Else
                    OK_Button.Focus()
                End If
                tmpImage.Clear()
            Else
                rbXLarge.Checked = False
                rbLarge.Checked = False
                rbMedium.Checked = False
                rbSmall.Checked = False
                OK_Button.Enabled = True
                OK_Button.Focus()
                tmpImage.Image = pbImage(iIndex).Image
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub GetFanart()
        Try
            Dim NoneFound As Boolean = True

            If Master.eSettings.UseImgCache Then
                Dim di As New DirectoryInfo(CachePath)
                Dim lFi As New List(Of FileInfo)

                If Not Directory.Exists(CachePath) Then
                    Directory.CreateDirectory(CachePath)
                Else
                    Try
                        lFi.AddRange(di.GetFiles("*.jpg"))
                    Catch
                    End Try
                End If

                If lFi.Count > 0 Then
                    pnlDLStatus.Visible = True
                    Application.DoEvents()
                    NoneFound = False
                    Dim tImage As MediaContainers.Image
                    For Each sFile As FileInfo In lFi
                        tImage = New MediaContainers.Image
                        tImage.WebImage.FromFile(sFile.FullName)
                        If Not IsNothing(tImage.WebImage.Image) Then
                            Select Case True
                                Case sFile.Name.Contains("(original)")
                                    tImage.Description = "original"
                                    If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Lrg Then
                                        If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                            tImage.isChecked = True
                                        End If
                                    End If
                                Case sFile.Name.Contains("(mid)")
                                    tImage.Description = "mid"
                                    If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Mid Then
                                        If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                            tImage.isChecked = True
                                        End If
                                    End If
                                Case sFile.Name.Contains("(thumb)")
                                    tImage.Description = "thumb"
                                    If Master.eSettings.AutoET AndAlso Master.eSettings.AutoETSize = Enums.FanartSize.Small Then
                                        If Not ETHashes.Contains(HashFile.HashCalcFile(sFile.FullName)) Then
                                            tImage.isChecked = True
                                        End If
                                    End If
                            End Select
                            tImage.URL = Regex.Match(sFile.Name, "\(url=(.*?)\)").Groups(1).ToString
                            'TMDBPosters.Add(tImage)
                        End If
                    Next
                    'ProcessPics(TMDBPosters)
                    pnlDLStatus.Visible = False
                    pnlBG.Visible = True
                    pnlFanart.Visible = True
                    lblInfo.Visible = True
                End If
            End If

            If NoneFound Then
                If Master.eSettings.AutoET AndAlso Not Directory.Exists(CachePath) Then
                    Directory.CreateDirectory(CachePath)
                End If

                lblDL2.Text = Master.eLang.GetString(32, "Retrieving images ...")
                lblDL2Status.Text = String.Empty
                pbDL2.Maximum = 3
                pnlDLStatus.Visible = True
                Refresh()
                Application.DoEvents()
                Dim _images As List(Of MediaContainers.Image) = (From t In tMovie.Movie.Fanart.Thumb Select New MediaContainers.Image With {.URL = t.Text}).ToList()
                ImagesDownloaded(_images)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub GetPosters()
        Try
            Dim NoneFound As Boolean = True

            If Master.eSettings.UseImgCache Then
                Dim lFi As New List(Of FileInfo)
                Dim di As New DirectoryInfo(CachePath)

                If Not Directory.Exists(CachePath) Then
                    Directory.CreateDirectory(CachePath)
                Else
                    Try
                        lFi.AddRange(di.GetFiles("*.jpg"))
                    Catch
                    End Try
                End If

                If lFi.Count > 0 Then
                    pnlDLStatus.Height = 75
                    pnlDLStatus.Top = 207
                    pnlDLStatus.Visible = True
                    Application.DoEvents()
                    NoneFound = False
                    Dim tImage As MediaContainers.Image
                    For Each sFile As FileInfo In lFi
                        tImage = New MediaContainers.Image
                        tImage.WebImage.FromFile(sFile.FullName)
                        Select Case True
                            Case sFile.Name.Contains("(original)")
                                tImage.Description = "original"
                            Case sFile.Name.Contains("(mid)")
                                tImage.Description = "mid"
                            Case sFile.Name.Contains("(cover)")
                                tImage.Description = "cover"
                            Case sFile.Name.Contains("(thumb)")
                                tImage.Description = "thumb"
                            Case sFile.Name.Contains("(poster)")
                                tImage.Description = "poster"
                        End Select
                        tImage.URL = Regex.Match(sFile.Name, "\(url=(.*?)\)").Groups(1).ToString
                        'TMDBPosters.Add(tImage)
                    Next
                    'ProcessPics(TMDBPosters)
                    pnlDLStatus.Visible = False
                    'pnlBG.Visible = True
                End If
            End If

            If NoneFound Then

                lblDL2.Text = Master.eLang.GetString(34, "Retrieving images ...")
                lblDL2Status.Text = String.Empty
                pbDL2.Maximum = 3
                pnlDLStatus.Visible = True
                Refresh()
                Application.DoEvents()
                Dim _images As List(Of MediaContainers.Image) = (From t In tMovie.Movie.Thumb Select New MediaContainers.Image With {.URL = t}).ToList()
                ImagesDownloaded(_images)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub DoneDownloading()
        Try
            AllDoneDownloading()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ImagesDownloaded(ByVal _images As List(Of MediaContainers.Image))
        Try
            pbDL2.Value = 0

            lblDL2.Text = Master.eLang.GetString(38, "Preparing images...")
            lblDL2Status.Text = String.Empty
            pbDL2.Maximum = _images.Count

            MovieImages = _images

            bwDownload.WorkerSupportsCancellation = True
            bwDownload.WorkerReportsProgress = True
            bwDownload.RunWorkerAsync()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub lblImage_Click(ByVal sender As Object, ByVal e As EventArgs)
        DoSelect(Convert.ToInt32(DirectCast(sender, Label).Name), DirectCast(sender, Label).Tag.ToString)
    End Sub

    Private Sub MouseWheelEvent(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Delta < 0 Then
            If (pnlBG.VerticalScroll.Value + 50) <= pnlBG.VerticalScroll.Maximum Then
                pnlBG.VerticalScroll.Value += 50
            Else
                pnlBG.VerticalScroll.Value = pnlBG.VerticalScroll.Maximum
            End If
        Else
            If (pnlBG.VerticalScroll.Value - 50) >= pnlBG.VerticalScroll.Minimum Then
                pnlBG.VerticalScroll.Value -= 50
            Else
                pnlBG.VerticalScroll.Value = pnlBG.VerticalScroll.Minimum
            End If
        End If
    End Sub
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles OK_Button.Click
        Try
            Dim tmpPathPlus As String

            If DLType = Enums.ImageType.Fanart Then
                tmpPathPlus = Path.Combine(Master.TempPath, "fanart.jpg")
            Else
                tmpPathPlus = Path.Combine(Master.TempPath, "poster.jpg")
            End If

            If Not IsNothing(tmpImage.Image) Then
                If isEdit Then
                    tmpImage.Save(tmpPathPlus)
                    Results.ImagePath = tmpPathPlus
                Else
                    If DLType = Enums.ImageType.Fanart Then
                        Results.ImagePath = tmpImage.SaveAsFanart(tMovie)
                    Else
                        Results.ImagePath = tmpImage.SaveAsPoster(tMovie)
                    End If
                End If
            Else
                pnlBG.Visible = False
                pnlSinglePic.Visible = True
                Refresh()
                Application.DoEvents()
                Select Case True
                    Case rbXLarge.Checked
                        If Master.eSettings.UseImgCache Then
                            tmpImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(original)_(url=", rbXLarge.Tag, ").jpg")))
                        Else
                            tmpImage.FromWeb(rbXLarge.Tag.ToString)
                        End If
                    Case rbLarge.Checked
                        If Master.eSettings.UseImgCache Then
                            tmpImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(mid)_(url=", rbLarge.Tag, ").jpg")))
                        Else
                            tmpImage.FromWeb(rbLarge.Tag.ToString)
                        End If
                    Case rbMedium.Checked
                        tmpImage.Image = pbImage(selIndex).Image
                    Case rbSmall.Checked
                        If Master.eSettings.UseImgCache Then
                            tmpImage.FromFile(Path.Combine(CachePath, String.Concat("poster_(thumb)_(url=", rbSmall.Tag, ").jpg")))
                        Else
                            tmpImage.FromWeb(rbSmall.Tag.ToString)
                        End If
                End Select

                If Not IsNothing(tmpImage.Image) Then
                    If isEdit Then
                        tmpImage.Save(tmpPathPlus)
                        Results.ImagePath = tmpPathPlus
                    Else
                        If DLType = Enums.ImageType.Fanart Then
                            Results.ImagePath = tmpImage.SaveAsFanart(tMovie)
                        Else
                            Results.ImagePath = tmpImage.SaveAsPoster(tMovie)
                        End If
                    End If
                End If
                pnlSinglePic.Visible = False
            End If

            If DLType = Enums.ImageType.Fanart Then
                Dim iMod As Integer
                Dim iVal As Integer = 1
                Dim extraPath As String
                Dim isChecked As Boolean = False

                For i As Integer = 0 To UBound(chkImage)
                    If chkImage(i).Checked Then
                        isChecked = True
                        Exit For
                    End If
                Next

                If isChecked Then

                    If isEdit Then
                        extraPath = Path.Combine(Master.TempPath, "extrathumbs")
                    Else
                        If Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isVideoTS(tMovie.Filename) Then
                            extraPath = Path.Combine(Directory.GetParent(Directory.GetParent(tMovie.Filename).FullName).FullName, "extrathumbs")
                        ElseIf Master.eSettings.VideoTSParent AndAlso FileUtils.Common.isBDRip(tMovie.Filename) Then
                            extraPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(tMovie.Filename).FullName).FullName).FullName, "extrathumbs")
                        Else
                            extraPath = Path.Combine(Directory.GetParent(tMovie.Filename).FullName, "extrathumbs")
                        End If
                        iMod = Functions.GetExtraModifier(extraPath)
                        iVal = iMod + 1
                    End If

                    If Not Directory.Exists(extraPath) Then
                        Directory.CreateDirectory(extraPath)
                    End If

                    Dim fsET As FileStream
                    For i As Integer = 0 To UBound(chkImage)
                        If chkImage(i).Checked Then
                            fsET = New FileStream(Path.Combine(extraPath, String.Concat("thumb", iVal, ".jpg")), FileMode.Create, FileAccess.ReadWrite)
                            pbImage(i).Image.Save(fsET, Imaging.ImageFormat.Jpeg)
                            fsET.Close()
                            iVal += 1
                        End If
                    Next                    
                End If
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub pbImage_Click(ByVal sender As Object, ByVal e As EventArgs)
        DoSelect(Convert.ToInt32(DirectCast(sender, PictureBox).Name), DirectCast(sender, PictureBox).Tag.ToString)
    End Sub

    Private Sub pbImage_DoubleClick(ByVal sender As System.Object, ByVal e As EventArgs)
        Try
            If DLType = Enums.ImageType.Fanart Then
                ModulesManager.Instance.RuntimeObjects.InvokeOpenImageViewer(DirectCast(sender, PictureBox).Image)
            End If
        Catch
        End Try
    End Sub

    Private Sub pnlImage_Click(ByVal sender As Object, ByVal e As EventArgs)
        DoSelect(Convert.ToInt32(DirectCast(sender, Panel).Name), DirectCast(sender, Panel).Tag.ToString)
    End Sub

    Private Sub ProcessPics(ByVal posters As List(Of MediaContainers.Image))
        Try
            Dim iIndex As Integer = 0

            'remove all entries with invalid images
            If Master.eSettings.UseImgCache Then
                For i As Integer = posters.Count - 1 To 0 Step -1
                    If IsNothing(posters.Item(i).WebImage.Image) Then
                        posters.RemoveAt(i)
                    End If
                Next
            End If

            If posters.Count > 0 Then
                For Each xPoster As MediaContainers.Image In posters.OrderBy(Function(p) p.URL)
                    If Not IsNothing(xPoster.WebImage.Image) AndAlso (DLType = Enums.ImageType.Fanart OrElse Not (xPoster.URL.ToLower.Contains("themoviedb.org") AndAlso Not xPoster.Description = "cover")) Then
                        AddImage(xPoster.WebImage.Image, xPoster.Description, iIndex, xPoster.URL, xPoster.isChecked)
                        iIndex += 1
                    End If
                Next
            Else
                If Not PreDL OrElse isShown Then
                    If DLType = Enums.ImageType.Fanart Then
                        MsgBox(Master.eLang.GetString(28, "No Fanart found for this movie."), MsgBoxStyle.Information, Master.eLang.GetString(29, "No Fanart Found"))
                    Else
                        MsgBox(Master.eLang.GetString(30, "No Posters found for this movie."), MsgBoxStyle.Information, Master.eLang.GetString(31, "No Posters Found"))
                    End If
                    DialogResult = DialogResult.Cancel
                    Close()
                Else
                    noImages = True
                End If
            End If

            Activate()

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub rbLarge_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles rbLarge.CheckedChanged
        OK_Button.Enabled = True
        btnPreview.Enabled = True
    End Sub

    Private Sub rbMedium_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles rbMedium.CheckedChanged
        OK_Button.Enabled = True
        btnPreview.Enabled = True
    End Sub

    Private Sub rbSmall_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles rbSmall.CheckedChanged
        OK_Button.Enabled = True
        btnPreview.Enabled = True
    End Sub

    Private Sub rbXLarge_CheckedChanged(ByVal sender As System.Object, ByVal e As EventArgs) Handles rbXLarge.CheckedChanged
        OK_Button.Enabled = True
        btnPreview.Enabled = True
    End Sub

    Private Sub SetUp()
        Try
            AddHandler EventImagesDone, AddressOf DoneDownloading

            AddHandler MouseWheel, AddressOf MouseWheelEvent
            AddHandler pnlBG.MouseWheel, AddressOf MouseWheelEvent

            Functions.PNLDoubleBuffer(pnlBG)

            If DLType = Enums.ImageType.Posters Then
                Text = String.Concat(Master.eLang.GetString(39, "Select Poster - "), If(Not String.IsNullOrEmpty(tMovie.Movie.Title), tMovie.Movie.Title, tMovie.ListTitle))
            Else
                Text = String.Concat(Master.eLang.GetString(40, "Select Fanart - "), If(Not String.IsNullOrEmpty(tMovie.Movie.Title), tMovie.Movie.Title, tMovie.ListTitle))
                pnlDLStatus.Height = 75
                pnlDLStatus.Top = 207

                If Master.eSettings.AutoET Then
                    ETHashes = HashFile.CurrentETHashes(tMovie.Filename)
                End If
            End If

            CachePath = String.Concat(Master.TempPath, Path.DirectorySeparatorChar, tMovie.Movie.IMDBID, Path.DirectorySeparatorChar, If(DLType = Enums.ImageType.Posters, "posters", "fanart"))

            OK_Button.Text = Master.eLang.GetString(179, "OK", True)
            Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
            btnPreview.Text = Master.eLang.GetString(180, "Preview", True)
            chkThumb.Text = Master.eLang.GetString(41, "Check All Thumb")
            chkMid.Text = Master.eLang.GetString(42, "Check All Mid")
            chkOriginal.Text = Master.eLang.GetString(43, "Check All Original")
            lblInfo.Text = Master.eLang.GetString(44, "Selected item will be set as fanart. All checked items will be saved to \extrathumbs.")
            lblDL2.Text = Master.eLang.GetString(45, "Performing Preliminary Tasks...")
            Label2.Text = Master.eLang.GetString(46, "Downloading Selected Image...")
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetupSizes(ByVal sURL As String)
        Try
            Strings.Left(sURL, sURL.Length - 10)

            rbXLarge.Checked = False
            rbXLarge.Enabled = False
            rbXLarge.Text = Master.eLang.GetString(47, "X-Large")
            rbLarge.Checked = False
            rbLarge.Enabled = False
            rbLarge.Text = Master.eLang.GetString(48, "Large")
            rbMedium.Checked = False
            rbMedium.Text = Master.eLang.GetString(49, "Medium")
            rbSmall.Checked = False
            rbSmall.Enabled = False
            rbSmall.Text = Master.eLang.GetString(50, "Small")

            rbMedium.Tag = sURL

            'For i As Integer = 0 To TMDBPosters.Count - 1
            'Select Case True
            '   Case TMDBPosters.Item(i).URL = String.Concat(sLeft, ".jpg")
            ' xlarge
            'If Not Master.eSettings.UseImgCache OrElse Not IsNothing(TMDBPosters.Item(i).WebImage.Image) Then
            'rbXLarge.Enabled = True
            'rbXLarge.Tag = TMDBPosters.Item(i).URL
            'If Master.eSettings.UseImgCache Then rbXLarge.Text = String.Format(Master.eLang.GetString(51, "X-Large ({0}x{1})"), TMDBPosters.Item(i).WebImage.Image.Width, TMDBPosters.Item(i).WebImage.Image.Height)
            'End If
            '   Case TMDBPosters.Item(i).URL = String.Concat(sLeft, "_mid.jpg")
            ' large
            'If Not Master.eSettings.UseImgCache OrElse Not IsNothing(TMDBPosters.Item(i).WebImage.Image) Then
            'rbLarge.Enabled = True
            'rbLarge.Tag = TMDBPosters.Item(i).URL
            'If Master.eSettings.UseImgCache Then rbLarge.Text = String.Format(Master.eLang.GetString(52, "Large ({0}x{1})"), TMDBPosters.Item(i).WebImage.Image.Width, TMDBPosters.Item(i).WebImage.Image.Height)
            'End If
            '   Case TMDBPosters.Item(i).URL = String.Concat(sLeft, "_thumb.jpg")
            ' small
            'If Not Master.eSettings.UseImgCache OrElse Not IsNothing(TMDBPosters.Item(i).WebImage.Image) Then
            'rbSmall.Enabled = True
            'rbSmall.Tag = TMDBPosters.Item(i).URL
            'If Master.eSettings.UseImgCache Then rbSmall.Text = String.Format(Master.eLang.GetString(53, "Small ({0}x{1})"), TMDBPosters.Item(i).WebImage.Image.Width, TMDBPosters.Item(i).WebImage.Image.Height)
            'End If
            '    Case TMDBPosters.Item(i).URL = sURL
            'If Master.eSettings.UseImgCache Then rbMedium.Text = String.Format(Master.eLang.GetString(54, "Medium ({0}x{1})"), TMDBPosters.Item(i).WebImage.Image.Width, TMDBPosters.Item(i).WebImage.Image.Height)
            'End Select
            'Next

            Select Case Master.eSettings.PreferredPosterSize
                Case Enums.PosterSize.Small
                    rbSmall.Checked = rbSmall.Enabled
                Case Enums.PosterSize.Mid
                    rbMedium.Checked = rbMedium.Enabled
                Case Enums.PosterSize.Lrg
                    rbLarge.Checked = rbLarge.Enabled
                Case Enums.PosterSize.Xlrg
                    rbXLarge.Checked = rbXLarge.Enabled
            End Select

            pnlSize.Visible = True

            Invalidate()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub StartDownload()
        Try
            Select Case DLType
                Case Enums.ImageType.Posters
                    GetPosters()
                Case Enums.ImageType.Fanart
                    GetFanart()
            End Select
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

#End Region 'Methods

End Class