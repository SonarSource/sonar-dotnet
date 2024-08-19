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

Public Class dlgTVDBSearchResults

#Region "Fields"

    Friend WithEvents bwDownloadPic As New System.ComponentModel.BackgroundWorker

    Private lvResultsSorter As New ListViewColumnSorter
    Private sHTTP As New HTTP
    Private sInfo As Structures.ScrapeInfo
    Private _manualresult As Scraper.TVSearchResults = Nothing
    Private _skipdownload As Boolean = False

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal _sInfo As Structures.ScrapeInfo) As Windows.Forms.DialogResult
        Me.sInfo = _sInfo
        Me.Text = String.Concat(Master.eLang.GetString(85, "TV Search Results"), " - ", sInfo.ShowTitle)
        Scraper.sObject.GetSearchResultsAsync(Me.sInfo)

        Return MyBase.ShowDialog()
    End Function

    Public Overloads Function ShowDialog(ByVal _sinfo As Structures.ScrapeInfo, ByVal SkipDownload As Boolean) As Structures.ScrapeInfo
        Me.sInfo = _sinfo
        Me._skipdownload = SkipDownload

        Me.Text = String.Concat(Master.eLang.GetString(85, "TV Search Results"), " - ", sInfo.ShowTitle)
        Scraper.sObject.GetSearchResultsAsync(Me.sInfo)

        If MyBase.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Return Me.sInfo
        Else
            Return _sinfo
        End If
    End Function

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If Not String.IsNullOrEmpty(Me.txtSearch.Text) Then
            Me.lvSearchResults.Enabled = False
            Me.sInfo.ShowTitle = Me.txtSearch.Text
            Me.ClearInfo()
            Me.chkManual.Enabled = False
            Me.chkManual.Checked = False
            Me.txtSearch.Text = String.Empty
            Me.btnVerify.Enabled = False
            Scraper.sObject.GetSearchResultsAsync(Me.sInfo)
            Me.pnlLoading.Visible = True
        End If
    End Sub

    Private Sub btnVerify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVerify.Click
        If IsNumeric(Me.txtTVDBID.Text) AndAlso Me.txtTVDBID.Text.Length >= 5 Then
            Dim tmpXML As XDocument = Nothing
            Dim sLang As String = String.Empty

            Me.ClearInfo()
            Me.pnlLoading.Visible = True
            Application.DoEvents()

            Dim forceXML As String = sHTTP.DownloadData(String.Format("http://{0}/api/{1}/series/{2}/{3}.xml", Master.eSettings.TVDBMirror, Scraper.APIKey, Me.txtTVDBID.Text, Master.eSettings.TVDBLanguage))

            If Not String.IsNullOrEmpty(forceXML) Then
                Try
                    tmpXML = XDocument.Parse(forceXML)
                Catch
                End Try

                If Not IsNothing(tmpXML) Then
                    Dim tSer As XElement = tmpXML.Descendants("Series").FirstOrDefault(Function(s) s.HasElements)

                    If Not IsNothing(tSer) Then
                        Me._manualresult = New Scraper.TVSearchResults
                        Me._manualresult.ID = Convert.ToInt32(tSer.Element("id").Value)
                        Me._manualresult.Name = If(Not IsNothing(tSer.Element("SeriesName")), tSer.Element("SeriesName").Value, String.Empty)
                        If Not IsNothing(tSer.Element("Language")) AndAlso Master.eSettings.TVDBLanguages.Count > 0 Then
                            sLang = tSer.Element("Language").Value
                            Me._manualresult.Language = Master.eSettings.TVDBLanguages.FirstOrDefault(Function(s) s.ShortLang = sLang)
                        ElseIf Not IsNothing(tSer.Element("Language")) Then
                            sLang = tSer.Element("Language").Value
                            Me._manualresult.Language = New Containers.TVLanguage With {.LongLang = String.Format("Unknown ({0})", sLang), .ShortLang = sLang}
                        End If
                        Me._manualresult.Aired = If(Not IsNothing(tSer.Element("FirstAired")), tSer.Element("FirstAired").Value, String.Empty)
                        Me._manualresult.Overview = If(Not IsNothing(tSer.Element("Overview")), tSer.Element("Overview").Value, String.Empty)
                        Me._manualresult.Banner = If(Not IsNothing(tSer.Element("banner")), tSer.Element("banner").Value, String.Empty)
                        If Not String.IsNullOrEmpty(Me._manualresult.Name) AndAlso Not String.IsNullOrEmpty(sLang) Then
                            If Not String.IsNullOrEmpty(Me._manualresult.Banner) Then
                                If Me.bwDownloadPic.IsBusy Then
                                    Me.bwDownloadPic.CancelAsync()
                                End If

                                Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                                Me.bwDownloadPic.WorkerSupportsCancellation = True
                                Me.bwDownloadPic.RunWorkerAsync(New Arguments With {.pURL = Me._manualresult.Banner})
                            End If

                            Me.OK_Button.Tag = Me._manualresult.ID
                            Me.lblTitle.Text = Me._manualresult.Name
                            Me.txtOutline.Text = Me._manualresult.Overview
                            Me.lblAired.Text = Me._manualresult.Aired
                            Me.OK_Button.Enabled = True
                            Me.pnlLoading.Visible = False
                            Me.ControlsVisible(True)
                        Else
                            Me.pnlLoading.Visible = False
                        End If
                    Else
                        Me.pnlLoading.Visible = False
                    End If
                Else
                    Me.pnlLoading.Visible = False
                End If
            Else
                Me.pnlLoading.Visible = False
            End If

        Else
            MsgBox(Master.eLang.GetString(83, "The ID you entered is not a valid TVDB ID."), MsgBoxStyle.Exclamation, Master.eLang.GetString(292, "Invalid Entry", True))
        End If
    End Sub

    Private Sub bwDownloadPic_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadPic.DoWork
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)
        sHTTP.StartDownloadImage(String.Format("http://{0}/banners/_cache/{1}", Master.eSettings.TVDBMirror, Args.pURL))

        While sHTTP.IsDownloading
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        e.Result = New Results With {.Result = sHTTP.Image()}
    End Sub

    Private Sub bwDownloadPic_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadPic.RunWorkerCompleted
        Dim Res As Results = DirectCast(e.Result, Results)

        Try
            Me.pbBanner.Image = Res.Result
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub chkManual_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkManual.CheckedChanged
        Me.ClearInfo()
        Me.OK_Button.Enabled = False
        Me.txtTVDBID.Enabled = Me.chkManual.Checked
        Me.btnVerify.Enabled = Me.chkManual.Checked
        Me.lvSearchResults.Enabled = Not Me.chkManual.Checked

        If Not Me.chkManual.Checked Then
            txtTVDBID.Text = String.Empty
        Else
            If Me.lvSearchResults.SelectedItems.Count > 0 Then Me.lvSearchResults.SelectedItems(0).Selected = False
        End If
    End Sub

    Private Sub ClearInfo()
        Me.ControlsVisible(False)
        Me.lblTitle.Text = String.Empty
        Me.lblAired.Text = String.Empty
        Me.pbBanner.Image = Nothing
        Scraper.sObject.CancelAsync()
    End Sub

    Private Sub ControlsVisible(ByVal areVisible As Boolean)
        Me.pbBanner.Visible = areVisible
        Me.lblTitle.Visible = areVisible
        Me.lblAiredHeader.Visible = areVisible
        Me.lblAired.Visible = areVisible
        Me.lblPlotHeader.Visible = areVisible
        Me.txtOutline.Visible = areVisible
    End Sub

    Private Sub dlgTVDBSearchResults_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            AddHandler ModulesManager.Instance.TVScraperEvent, AddressOf TVScraperEvent
            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Using g As Graphics = Graphics.FromImage(iBackground)
                g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
                Me.pnlTop.BackgroundImage = iBackground
            End Using

            Me.lvSearchResults.ListViewItemSorter = Me.lvResultsSorter

            Me.SetUp()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub lvSearchResults_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lvSearchResults.ColumnClick
        ' Determine if the clicked column is already the column that is
        ' being sorted.
        Try
            If (e.Column = Me.lvResultsSorter.SortColumn) Then
                ' Reverse the current sort direction for this column.
                If (Me.lvResultsSorter.Order = SortOrder.Ascending) Then
                    Me.lvResultsSorter.Order = SortOrder.Descending
                Else
                    Me.lvResultsSorter.Order = SortOrder.Ascending
                End If
            Else
                ' Set the column number that is to be sorted; default to ascending.
                Me.lvResultsSorter.SortColumn = e.Column
                Me.lvResultsSorter.Order = SortOrder.Ascending
            End If

            ' Perform the sort with these new sort options.
            Me.lvSearchResults.Sort()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub lvSearchResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvSearchResults.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub lvSearchResults_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvSearchResults.SelectedIndexChanged
        Me.ClearInfo()
        If Me.lvSearchResults.SelectedItems.Count > 0 AndAlso Not Me.chkManual.Checked Then
            Dim SelectedShow As Scraper.TVSearchResults = DirectCast(Me.lvSearchResults.SelectedItems(0).Tag, Scraper.TVSearchResults)
            If Not String.IsNullOrEmpty(SelectedShow.Banner) Then
                If Me.bwDownloadPic.IsBusy Then
                    Me.bwDownloadPic.CancelAsync()
                End If

                Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                Me.bwDownloadPic.WorkerSupportsCancellation = True
                Me.bwDownloadPic.RunWorkerAsync(New Arguments With {.pURL = SelectedShow.Banner})
            End If

            Me.OK_Button.Tag = SelectedShow.ID
            Me.lblTitle.Text = SelectedShow.Name
            Me.txtOutline.Text = SelectedShow.Overview
            Me.lblAired.Text = SelectedShow.Aired
            Me.OK_Button.Enabled = True
            Me.ControlsVisible(True)
        End If
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If Me.lvSearchResults.SelectedItems.Count > 0 Then
            Dim sResults As Scraper.TVSearchResults = DirectCast(Me.lvSearchResults.SelectedItems(0).Tag, Scraper.TVSearchResults)
            Me.sInfo.TVDBID = sResults.ID.ToString
            Me.sInfo.SelectedLang = sResults.Language.ShortLang

            If Not _skipdownload Then
                Me.Label3.Text = Master.eLang.GetString(84, "Downloading show info...")
                Me.pnlLoading.Visible = True
                Scraper.sObject.DownloadSeriesAsync(sInfo)
            Else
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
            End If
        ElseIf Me.chkManual.Checked AndAlso Not IsNothing(Me._manualresult) Then
            Me.sInfo.TVDBID = Me._manualresult.ID.ToString
            Me.sInfo.SelectedLang = Me._manualresult.Language.ShortLang

            If Not _skipdownload Then
                Me.Label3.Text = Master.eLang.GetString(84, "Downloading show info...")
                Me.pnlLoading.Visible = True
                Scraper.sObject.DownloadSeriesAsync(sInfo)
            Else
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
            End If
        End If
    End Sub

    Private Sub SetUp()
        Me.Label1.Text = Master.eLang.GetString(85, "TV Search Results")
        Me.Label2.Text = Master.eLang.GetString(86, "View details of each result to find the proper TV show.")
        Me.lblAiredHeader.Text = Master.eLang.GetString(658, "Aired:", True)
        Me.lblPlotHeader.Text = Master.eLang.GetString(783, "Plot Summary:", True)

        Me.lvSearchResults.Columns(0).Text = Master.eLang.GetString(21, "Title", True)
        Me.lvSearchResults.Columns(1).Text = Master.eLang.GetString(610, "Language", True)

        Me.OK_Button.Text = Master.eLang.GetString(179, "OK", True)
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel", True)
    End Sub

    Private Sub TVScraperEvent(ByVal eType As Enums.TVScraperEventType, ByVal iProgress As Integer, ByVal Parameter As Object)
        Select Case eType
            Case Enums.TVScraperEventType.SearchResultsDownloaded
                Dim lItem As ListViewItem
                Dim sResults As List(Of Scraper.TVSearchResults) = DirectCast(Parameter, List(Of Scraper.TVSearchResults))

                Me.lvSearchResults.Items.Clear()

                If Not IsNothing(sResults) AndAlso sResults.Count > 0 Then
                    For Each sRes As Scraper.TVSearchResults In sResults.OrderBy(Function(r) r.Lev)
                        lItem = New ListViewItem(sRes.Name)
                        lItem.SubItems.Add(sRes.Language.LongLang)
                        lItem.SubItems.Add(sRes.Lev.ToString)
                        lItem.SubItems.Add(sRes.ID.ToString)
                        lItem.SubItems.Add(sRes.Language.ShortLang)
                        lItem.Tag = sRes
                        Me.lvSearchResults.Items.Add(lItem)
                    Next
                End If

                Me.pnlLoading.Visible = False

                If Me.lvSearchResults.Items.Count > 0 Then
                    If sResults.Select(Function(s) s.ID).Distinct.Count = 1 Then
                        'they're all for the same show... try to find one with the preferred language
                        For Each fItem As ListViewItem In Me.lvSearchResults.Items
                            If fItem.SubItems(4).Text = Master.eSettings.TVDBLanguage Then
                                fItem.Selected = True
                                fItem.EnsureVisible()
                                Exit For
                            End If
                        Next
                    Else
                        'we've got a bunch of different shows... try to find a "best match" title with the preferred language
                        If sResults.Where(Function(s) s.Lev <= 5).Count > 0 Then
                            For Each fItem As ListViewItem In Me.lvSearchResults.Items
                                If Convert.ToInt32(fItem.SubItems(2).Text) <= 5 AndAlso fItem.SubItems(4).Text = Master.eSettings.TVDBLanguage Then
                                    fItem.Selected = True
                                    fItem.EnsureVisible()
                                    Exit For
                                End If
                            Next

                            If Me.lvSearchResults.SelectedItems.Count = 0 Then
                                'get the id for the best english match and see if we have one for the preferred language with same id
                                Dim tID As Integer = sResults.OrderBy(Function(s) s.Lev).FirstOrDefault(Function(s) s.Language.ShortLang = "en").ID
                                If tID > 0 Then
                                    For Each fItem As ListViewItem In Me.lvSearchResults.Items
                                        If Convert.ToInt32(fItem.SubItems(3).Text) = tID AndAlso fItem.SubItems(4).Text = Master.eSettings.TVDBLanguage Then
                                            fItem.Selected = True
                                            fItem.EnsureVisible()
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If

                    If Me.lvSearchResults.SelectedItems.Count = 0 Then
                        Me.lvSearchResults.Items(0).Selected = True
                    End If
                    Me.lvSearchResults.Select()
                End If

                Me.chkManual.Enabled = True
                If Not Me.chkManual.Checked Then Me.lvSearchResults.Enabled = True

            Case Enums.TVScraperEventType.ShowDownloaded
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
        End Select
    End Sub

    Private Sub txtSearch_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSearch.GotFocus
        Me.AcceptButton = Me.btnSearch
    End Sub

    Private Sub txtTVDBID_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTVDBID.GotFocus
        Me.AcceptButton = Me.btnVerify
    End Sub

    Private Sub txtTVDBID_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtTVDBID.KeyPress
        e.Handled = StringUtils.NumericOnly(e.KeyChar, True)
    End Sub

    Private Sub txtTVDBID_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTVDBID.TextChanged
        If String.IsNullOrEmpty(Me.txtTVDBID.Text) Then
            Me.btnVerify.Enabled = False
            Me.OK_Button.Enabled = False
        Else
            If Me.chkManual.Checked Then
                Me.btnVerify.Enabled = True
                Me.OK_Button.Enabled = False
            End If
        End If
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
