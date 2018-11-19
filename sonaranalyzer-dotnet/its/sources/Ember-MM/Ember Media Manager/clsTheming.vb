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

Public Class Theming

#Region "Fields"

    Private rProcs(3) As Regex
    Private _availablecontrols As New List(Of Controls)
    Private _eptheme As New Theme
    Private _movietheme As New Theme
    Private _showtheme As New Theme

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Const AvailCalcs As String = "*/+-"

        For i As Integer = 0 To 3
            rProcs(i) = New Regex(Strings.Replace("(\d+(?:[.,]\d+)?) *(\#) *([+-]?\d+(?:[.,]\d+)?)", "#", AvailCalcs.Substring(i, 1)))
        Next

        BuildControlList()

        ParseThemes(_movietheme, "movie", Master.eSettings.MovieTheme)
        ParseThemes(_showtheme, "tvshow", Master.eSettings.TVShowTheme)
        ParseThemes(_eptheme, "tvep", Master.eSettings.TVEpTheme)
    End Sub

#End Region 'Constructors

#Region "Enumerations"

    Public Enum ThemeType As Integer
        Movies = 0
        Show = 1
        Episode = 2
    End Enum

#End Region 'Enumerations

#Region "Methods"

    Public Sub ApplyTheme(ByVal tType As ThemeType)
        Dim xTheme As New Theme
        Dim xControl As New Control
        Select Case tType
            Case ThemeType.Movies
                xTheme = _movietheme
            Case ThemeType.Show
                xTheme = _showtheme
            Case ThemeType.Episode
                xTheme = _eptheme
        End Select
        frmMain.pnlTop.BackColor = xTheme.TopPanelBackColor
        frmMain.pnlInfoIcons.BackColor = xTheme.TopPanelBackColor
        frmMain.pnlRating.BackColor = xTheme.TopPanelBackColor
        frmMain.pbVideo.BackColor = xTheme.TopPanelBackColor
        frmMain.pbResolution.BackColor = xTheme.TopPanelBackColor
        frmMain.pbAudio.BackColor = xTheme.TopPanelBackColor
        frmMain.pbChannels.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStudio.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStar1.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStar2.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStar3.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStar4.BackColor = xTheme.TopPanelBackColor
        frmMain.pbStar5.BackColor = xTheme.TopPanelBackColor

        frmMain.lblTitle.ForeColor = xTheme.TopPanelForeColor
        frmMain.lblVotes.ForeColor = xTheme.TopPanelForeColor
        frmMain.lblRuntime.ForeColor = xTheme.TopPanelForeColor
        frmMain.lblTagline.ForeColor = xTheme.TopPanelForeColor

        frmMain.pbFanart.BackColor = xTheme.FanartBackColor
        frmMain.scMain.Panel2.BackColor = xTheme.FanartBackColor
        frmMain.pnlPoster.BackColor = xTheme.PosterBackColor
        frmMain.pbPoster.BackColor = xTheme.PosterBackColor
        frmMain.pnlMPAA.BackColor = xTheme.MPAABackColor
        frmMain.pbMPAA.BackColor = xTheme.MPAABackColor

        frmMain.GenrePanelColor = xTheme.GenreBackColor
        frmMain.PosterMaxWidth = xTheme.PosterMaxWidth
        frmMain.PosterMaxHeight = xTheme.PosterMaxHeight
        frmMain.IPUp = xTheme.IPUp
        frmMain.IPMid = xTheme.IPMid

        For Each xCon As Controls In xTheme.Controls
            Select Case xCon.Control
                Case "pnlInfoPanel"
                    xControl = frmMain.pnlInfoPanel
                Case "pbTop250", "lblTop250"
                    xControl = frmMain.pnlTop250.Controls(xCon.Control)
                Case "lblActorsHeader", "lstActors", "pbActors", "pbActLoad"
                    xControl = frmMain.pnlActors.Controls(xCon.Control)
                Case Else
                    xControl = frmMain.pnlInfoPanel.Controls(xCon.Control)
            End Select

            If Not xCon.Control = "pnlInfoPanel" Then xControl.Visible = xCon.Visible
            If xCon.Visible Then
                If Not xCon.Control = "pnlInfoPanel" AndAlso Not String.IsNullOrEmpty(xCon.Width) Then xControl.Width = EvaluateFormula(xCon.Width)
                If Not xCon.Control = "pnlInfoPanel" AndAlso Not String.IsNullOrEmpty(xCon.Height) Then xControl.Height = EvaluateFormula(xCon.Height)
                If Not xCon.Control = "pnlInfoPanel" AndAlso Not String.IsNullOrEmpty(xCon.Left) Then xControl.Left = EvaluateFormula(xCon.Left)
                If Not xCon.Control = "pnlInfoPanel" AndAlso Not String.IsNullOrEmpty(xCon.Top) Then xControl.Top = EvaluateFormula(xCon.Top)
                If Not xCon.Control = "btnUp" AndAlso Not xCon.Control = "btnMid" AndAlso Not xCon.Control = "btnDown" AndAlso Not xCon.Control = "btnMetaDataRefresh" Then xControl.BackColor = xCon.BackColor
                xControl.ForeColor = xCon.ForeColor
                xControl.Font = xCon.Font
                If Not xCon.Control = "pnlInfoPanel" Then xControl.Anchor = xCon.Anchor
            End If
        Next
    End Sub

    Public Sub BuildControlList()
        Try
            _availablecontrols.Clear()
            Const PossibleControls As String = "pnlInfoPanel,lblInfoPanelHeader,btnUp,btnMid,btnDown,lblDirectorHeader,lblDirector,lblReleaseDateHeader,lblReleaseDate,pnlTop250,pbTop250,lblTop250,lblOutlineHeader,txtOutline,lblIMDBHeader,txtIMDBID,lblCertsHeader,txtCerts,lblFilePathHeader,txtFilePath,btnPlay,pnlActors,lblActorsHeader,lstActors,pbActors,pbActLoad,lblPlotHeader,txtPlot,lblMetaDataHeader,btnMetaDataRefresh,txtMetaData,pbMILoading"
            For Each sCon As String In PossibleControls.Split(Convert.ToChar(","))
                _availablecontrols.Add(New Controls With {.Control = sCon})
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Function EvaluateFormula(ByVal sFormula As String) As Integer
        Dim mReg As Match
        Dim rResult As Double = 0.0
        Dim dFirst As Double = 0.0
        Dim dSecond As Double = 0.0

        If IsNumeric(sFormula) Then Return Convert.ToInt32(sFormula)

        ReplaceControlVars(sFormula)

        Try

            'check for invalid characters
            If Regex.IsMatch(sFormula, "^[().,1234567890 ^*/+-]+$") Then

                'check for equal number of ()
                If sFormula.Replace("(", String.Empty).Length = sFormula.Replace(")", String.Empty).Length Then

                    'step 2: all code between brackets
                    For Each mReg In Regex.Matches(sFormula, "\((.+)\)")
                        rResult = EvaluateFormula(mReg.Groups(1).ToString())
                        sFormula = sFormula.Replace(mReg.ToString(), rResult.ToString("0.00"))
                    Next

                    'step 2 operators
                    For Each rMatch As Regex In rProcs
                        Do
                            mReg = rMatch.Match(sFormula)
                            If Not mReg.Success Then Exit Do
                            dFirst = Double.Parse(mReg.Groups(1).ToString())
                            dSecond = Double.Parse(mReg.Groups(3).ToString())

                            Select Case mReg.Groups(2).ToString.Trim
                                Case "*"
                                    rResult = dFirst * dSecond
                                Case "/"
                                    rResult = dFirst / dSecond
                                Case "+"
                                    rResult = dFirst + dSecond
                                Case "-"
                                    rResult = dFirst - dSecond
                            End Select
                            If mReg.ToString().Length = sFormula.Length Then Return Convert.ToInt32(rResult)
                            sFormula = sFormula.Replace(mReg.ToString(), rResult.ToString("0.00"))
                        Loop
                    Next
                End If
            End If

            Return Convert.ToInt32(sFormula)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(String.Format("{0} ({1})", ex.Message, sFormula), ex.StackTrace, "Error")
        End Try

        Return 0
    End Function

    Public Sub ParseThemes(ByRef tTheme As Theme, ByVal tType As String, ByVal sTheme As String)
        Dim ThemeXML As New XDocument
        Dim cControl As Controls
        Dim cName As String = String.Empty
        Dim cFont As String = "Microsoft Sans Serif"
        Dim cFontSize As Integer = 8
        Dim cFontStyle As FontStyle = FontStyle.Bold

        Dim tPath As String = String.Concat(Functions.AppPath, "Themes", Path.DirectorySeparatorChar, String.Format("{0}-{1}.xml", tType, sTheme))
        If File.Exists(tPath) Then
            ThemeXML = XDocument.Load(tPath)
        Else
            Select Case tType
                Case "movie"
                    ThemeXML = XDocument.Parse(My.Resources.movie_Default)
                Case "tvshow"
                    ThemeXML = XDocument.Parse(My.Resources.tvshow_Default)
                Case "tvep"
                    ThemeXML = XDocument.Parse(My.Resources.tvep_Default)
            End Select
        End If

        'top panel
        Try
            Dim xTop = From xTheme In ThemeXML...<theme>...<toppanel>
            If xTop.Count > 0 Then
                If Not String.IsNullOrEmpty(xTop.<backcolor>.Value) Then tTheme.TopPanelBackColor = Color.FromArgb(Convert.ToInt32(xTop.<backcolor>.Value))
                If Not String.IsNullOrEmpty(xTop.<forecolor>.Value) Then tTheme.TopPanelForeColor = Color.FromArgb(Convert.ToInt32(xTop.<forecolor>.Value))
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        'images
        Try
            Dim xImages = From xTheme In ThemeXML...<theme>...<images>
            If xImages.Count > 0 Then
                If Not String.IsNullOrEmpty(xImages.<fanartbackcolor>.Value) Then tTheme.FanartBackColor = Color.FromArgb(Convert.ToInt32(xImages.<fanartbackcolor>.Value))
                If Not String.IsNullOrEmpty(xImages.<posterbackcolor>.Value) Then tTheme.PosterBackColor = Color.FromArgb(Convert.ToInt32(xImages.<posterbackcolor>.Value))
                If Not String.IsNullOrEmpty(xImages.<postermaxheight>.Value) Then tTheme.PosterMaxHeight = Convert.ToInt32(xImages.<postermaxheight>.Value)
                If Not String.IsNullOrEmpty(xImages.<postermaxwidth>.Value) Then tTheme.PosterMaxWidth = Convert.ToInt32(xImages.<postermaxwidth>.Value)
                If Not String.IsNullOrEmpty(xImages.<mpaabackcolor>.Value) Then tTheme.MPAABackColor = Color.FromArgb(Convert.ToInt32(xImages.<mpaabackcolor>.Value))
                If Not String.IsNullOrEmpty(xImages.<genrebackcolor>.Value) Then tTheme.GenreBackColor = Color.FromArgb(Convert.ToInt32(xImages.<genrebackcolor>.Value))
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Try
            'info panel
            Dim xIPMain = From xTheme In ThemeXML...<theme>...<infopanel> Select xTheme.<backcolor>.Value, xTheme.<ipup>.Value, xTheme.<ipmid>.Value
            If xIPMain.Count > 0 Then
                If Not String.IsNullOrEmpty(xIPMain(0).backcolor) Then tTheme.InfoPanelBackColor = Color.FromArgb(Convert.ToInt32(xIPMain(0).backcolor))
                If Not String.IsNullOrEmpty(xIPMain(0).ipup) Then tTheme.IPUp = Convert.ToInt32(xIPMain(0).ipup)
                If Not String.IsNullOrEmpty(xIPMain(0).ipmid) Then tTheme.IPMid = Convert.ToInt32(xIPMain(0).ipmid)
            End If

            tTheme.Controls.Clear()
            For Each xIP As XElement In ThemeXML...<theme>...<infopanel>...<object>
                If Not String.IsNullOrEmpty(xIP.@name) Then
                    cName = xIP.@name
                    Dim xControl = From xCons As Controls In _availablecontrols Where xCons.Control = cName
                    If xControl.Count > 0 Then
                        cControl = New Controls
                        cControl.Control = cName
                        If Not String.IsNullOrEmpty(xIP.<width>.Value) Then cControl.Width = xIP.<width>.Value
                        If Not String.IsNullOrEmpty(xIP.<height>.Value) Then cControl.Height = xIP.<height>.Value
                        If Not String.IsNullOrEmpty(xIP.<left>.Value) Then cControl.Left = xIP.<left>.Value
                        If Not String.IsNullOrEmpty(xIP.<top>.Value) Then cControl.Top = xIP.<top>.Value
                        If Not String.IsNullOrEmpty(xIP.<backcolor>.Value) Then cControl.BackColor = Color.FromArgb(Convert.ToInt32(xIP.<backcolor>.Value))
                        If Not String.IsNullOrEmpty(xIP.<forecolor>.Value) Then cControl.ForeColor = Color.FromArgb(Convert.ToInt32(xIP.<forecolor>.Value))
                        If Not String.IsNullOrEmpty(xIP.<anchor>.Value) Then cControl.Anchor = DirectCast(Convert.ToInt32(xIP.<anchor>.Value), AnchorStyles)
                        If Not String.IsNullOrEmpty(xIP.<visible>.Value) Then cControl.Visible = Convert.ToBoolean(xIP.<visible>.Value)

                        cFont = "Microsoft Sans Serif"
                        cFontSize = 8
                        cFontStyle = FontStyle.Regular

                        If Not String.IsNullOrEmpty(xIP.<font>.Value) Then cFont = xIP.<font>.Value
                        If Not String.IsNullOrEmpty(xIP.<fontsize>.Value) Then cFontSize = Convert.ToInt32(xIP.<fontsize>.Value)
                        If Not String.IsNullOrEmpty(xIP.<fontstyle>.Value) Then cFontStyle = DirectCast(Convert.ToInt32(xIP.<fontstyle>.Value), FontStyle)
                        cControl.Font = New Font(cFont, cFontSize, cFontStyle)
                        tTheme.Controls.Add(cControl)
                    End If
                End If

            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub ReplaceControlVars(ByRef sFormula As String)
        Dim xControl As New Control
        Dim cName As String

        Try

            For Each xCon As Match In Regex.Matches(sFormula, "(?<control>[a-z]+)\.(?<value>[a-z]+)", RegexOptions.IgnoreCase)
                cName = xCon.Groups("control").Value
                Dim aCon = From bCon As Controls In _availablecontrols Where bCon.Control.ToLower = cName.ToLower

                If aCon.Count > 0 Then
                    Select Case aCon(0).Control
                        Case "pnlInfoPanel"
                            xControl = frmMain.pnlInfoPanel
                        Case "pbTop250", "lblTop250"
                            xControl = frmMain.pnlTop250.Controls(aCon(0).Control)
                        Case "lblActorsHeader", "lstActors", "pbActors", "pbActLoad"
                            xControl = frmMain.pnlActors.Controls(aCon(0).Control)
                        Case Else
                            xControl = frmMain.pnlInfoPanel.Controls(aCon(0).Control)
                    End Select

                    Select Case xCon.Groups("value").Value.ToLower
                        Case "width"
                            sFormula = sFormula.Replace(xCon.ToString, xControl.Width.ToString)
                        Case "height"
                            sFormula = sFormula.Replace(xCon.ToString, xControl.Height.ToString)
                        Case "top"
                            sFormula = sFormula.Replace(xCon.ToString, xControl.Top.ToString)
                        Case "left"
                            sFormula = sFormula.Replace(xCon.ToString, xControl.Left.ToString)
                    End Select
                End If
            Next

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Class Controls

#Region "Fields"

        Dim _anchor As AnchorStyles
        Dim _backcolor As Color
        Dim _control As String
        Dim _font As Font
        Dim _forecolor As Color
        Dim _height As String
        Dim _left As String
        Dim _top As String
        Dim _visible As Boolean
        Dim _width As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property Anchor() As AnchorStyles
            Get
                Return _anchor
            End Get
            Set(ByVal value As AnchorStyles)
                _anchor = value
            End Set
        End Property

        Public Property BackColor() As Color
            Get
                Return _backcolor
            End Get
            Set(ByVal value As Color)
                _backcolor = value
            End Set
        End Property

        Public Property Control() As String
            Get
                Return _control
            End Get
            Set(ByVal value As String)
                _control = value
            End Set
        End Property

        Public Property ForeColor() As Color
            Get
                Return _forecolor
            End Get
            Set(ByVal value As Color)
                _forecolor = value
            End Set
        End Property

        Public Property Height() As String
            Get
                Return _height
            End Get
            Set(ByVal value As String)
                _height = value
            End Set
        End Property

        Public Property Left() As String
            Get
                Return _left
            End Get
            Set(ByVal value As String)
                _left = value
            End Set
        End Property

        Public Property Top() As String
            Get
                Return _top
            End Get
            Set(ByVal value As String)
                _top = value
            End Set
        End Property

        Public Property Visible() As Boolean
            Get
                Return _visible
            End Get
            Set(ByVal value As Boolean)
                _visible = value
            End Set
        End Property

        Public Property Width() As String
            Get
                Return _width
            End Get
            Set(ByVal value As String)
                _width = value
            End Set
        End Property

        Public Property [Font]() As Font
            Get
                Return _font
            End Get
            Set(ByVal value As Font)
                _font = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _control = String.Empty
            _width = String.Empty
            _height = String.Empty
            _left = String.Empty
            _top = String.Empty
            _backcolor = Color.Gainsboro
            _forecolor = Color.Black
            _anchor = AnchorStyles.None
            _visible = True
            _font = New Font("Microsoft Sans Serif", 8, FontStyle.Regular)
        End Sub

#End Region 'Methods

    End Class

    Public Class Theme

#Region "Fields"

        Dim _controls As List(Of Controls)
        Dim _fanartbackcolor As Color
        Dim _genrebackcolor As Color
        Dim _infopanelbackcolor As Color
        Dim _ipmid As Integer
        Dim _ipup As Integer
        Dim _mpaabackcolor As Color
        Dim _posterbackcolor As Color
        Dim _postermaxheight As Integer
        Dim _postermaxwidth As Integer
        Dim _toppanelbackcolor As Color
        Dim _toppanelforecolor As Color

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property Controls() As List(Of Controls)
            Get
                Return _controls
            End Get
            Set(ByVal value As List(Of Controls))
                _controls = value
            End Set
        End Property

        Public Property FanartBackColor() As Color
            Get
                Return _fanartbackcolor
            End Get
            Set(ByVal value As Color)
                _fanartbackcolor = value
            End Set
        End Property

        Public Property GenreBackColor() As Color
            Get
                Return _genrebackcolor
            End Get
            Set(ByVal value As Color)
                _genrebackcolor = value
            End Set
        End Property

        Public Property InfoPanelBackColor() As Color
            Get
                Return _infopanelbackcolor
            End Get
            Set(ByVal value As Color)
                _infopanelbackcolor = value
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

        Public Property MPAABackColor() As Color
            Get
                Return _mpaabackcolor
            End Get
            Set(ByVal value As Color)
                _mpaabackcolor = value
            End Set
        End Property

        Public Property PosterBackColor() As Color
            Get
                Return _posterbackcolor
            End Get
            Set(ByVal value As Color)
                _posterbackcolor = value
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

        Public Property TopPanelBackColor() As Color
            Get
                Return _toppanelbackcolor
            End Get
            Set(ByVal value As Color)
                _toppanelbackcolor = value
            End Set
        End Property

        Public Property TopPanelForeColor() As Color
            Get
                Return _toppanelforecolor
            End Get
            Set(ByVal value As Color)
                _toppanelforecolor = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _toppanelbackcolor = Color.Gainsboro
            _toppanelforecolor = Color.Black
            _fanartbackcolor = Color.Gainsboro
            _posterbackcolor = Color.Gainsboro
            _postermaxwidth = 160
            _postermaxheight = 160
            _mpaabackcolor = Color.Gainsboro
            _genrebackcolor = Color.Gainsboro
            _infopanelbackcolor = Color.Gainsboro
            _ipup = 500
            _ipmid = 280
            _controls = New List(Of Controls)
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class