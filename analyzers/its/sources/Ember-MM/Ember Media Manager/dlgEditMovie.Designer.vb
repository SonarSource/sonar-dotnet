<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgEditMovie
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditMovie))
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.txtOriginalTitle = New System.Windows.Forms.TextBox()
		Me.lblOriginalTitle = New System.Windows.Forms.Label()
		Me.txtCountry = New System.Windows.Forms.TextBox()
		Me.lblCountry = New System.Windows.Forms.Label()
		Me.txtFileSource = New System.Windows.Forms.TextBox()
		Me.lblFileSource = New System.Windows.Forms.Label()
		Me.btnActorDown = New System.Windows.Forms.Button()
		Me.btnActorUp = New System.Windows.Forms.Button()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.txtSortTitle = New System.Windows.Forms.TextBox()
		Me.lblLocalTrailer = New System.Windows.Forms.Label()
		Me.btnPlayTrailer = New System.Windows.Forms.Button()
		Me.btnDLTrailer = New System.Windows.Forms.Button()
		Me.lbGenre = New System.Windows.Forms.CheckedListBox()
		Me.btnStudio = New System.Windows.Forms.Button()
		Me.lblStudio = New System.Windows.Forms.Label()
		Me.txtStudio = New System.Windows.Forms.TextBox()
		Me.lblTrailer = New System.Windows.Forms.Label()
		Me.txtTrailer = New System.Windows.Forms.TextBox()
		Me.txtReleaseDate = New System.Windows.Forms.TextBox()
		Me.lblReleaseDate = New System.Windows.Forms.Label()
		Me.lblCredits = New System.Windows.Forms.Label()
		Me.txtCredits = New System.Windows.Forms.TextBox()
		Me.lblCerts = New System.Windows.Forms.Label()
		Me.txtCerts = New System.Windows.Forms.TextBox()
		Me.lblRuntime = New System.Windows.Forms.Label()
		Me.txtRuntime = New System.Windows.Forms.TextBox()
		Me.lblMPAADesc = New System.Windows.Forms.Label()
		Me.txtMPAADesc = New System.Windows.Forms.TextBox()
		Me.btnEditActor = New System.Windows.Forms.Button()
		Me.btnAddActor = New System.Windows.Forms.Button()
		Me.btnManual = New System.Windows.Forms.Button()
		Me.btnRemove = New System.Windows.Forms.Button()
		Me.lblActors = New System.Windows.Forms.Label()
		Me.lvActors = New System.Windows.Forms.ListView()
		Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colRole = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colThumb = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.lbMPAA = New System.Windows.Forms.ListBox()
		Me.lblGenre = New System.Windows.Forms.Label()
		Me.lblMPAA = New System.Windows.Forms.Label()
		Me.lblDirector = New System.Windows.Forms.Label()
		Me.txtDirector = New System.Windows.Forms.TextBox()
		Me.txtTop250 = New System.Windows.Forms.TextBox()
		Me.lblTop250 = New System.Windows.Forms.Label()
		Me.lblPlot = New System.Windows.Forms.Label()
		Me.txtPlot = New System.Windows.Forms.TextBox()
		Me.lblOutline = New System.Windows.Forms.Label()
		Me.txtOutline = New System.Windows.Forms.TextBox()
		Me.lblTagline = New System.Windows.Forms.Label()
		Me.txtTagline = New System.Windows.Forms.TextBox()
		Me.pbStar5 = New System.Windows.Forms.PictureBox()
		Me.pbStar4 = New System.Windows.Forms.PictureBox()
		Me.pbStar3 = New System.Windows.Forms.PictureBox()
		Me.pbStar2 = New System.Windows.Forms.PictureBox()
		Me.pbStar1 = New System.Windows.Forms.PictureBox()
		Me.txtVotes = New System.Windows.Forms.TextBox()
		Me.lblVotes = New System.Windows.Forms.Label()
		Me.lblRating = New System.Windows.Forms.Label()
		Me.mtxtYear = New System.Windows.Forms.MaskedTextBox()
		Me.lblYear = New System.Windows.Forms.Label()
		Me.lblTitle = New System.Windows.Forms.Label()
		Me.txtTitle = New System.Windows.Forms.TextBox()
		Me.TabPage2 = New System.Windows.Forms.TabPage()
		Me.btnSetPosterDL = New System.Windows.Forms.Button()
		Me.btnRemovePoster = New System.Windows.Forms.Button()
		Me.lblPosterSize = New System.Windows.Forms.Label()
		Me.btnSetPosterScrape = New System.Windows.Forms.Button()
		Me.btnSetPoster = New System.Windows.Forms.Button()
		Me.pbPoster = New System.Windows.Forms.PictureBox()
		Me.TabPage3 = New System.Windows.Forms.TabPage()
		Me.btnSetFanartDL = New System.Windows.Forms.Button()
		Me.btnRemoveFanart = New System.Windows.Forms.Button()
		Me.lblFanartSize = New System.Windows.Forms.Label()
		Me.btnSetFanartScrape = New System.Windows.Forms.Button()
		Me.btnSetFanart = New System.Windows.Forms.Button()
		Me.pbFanart = New System.Windows.Forms.PictureBox()
		Me.TabPage5 = New System.Windows.Forms.TabPage()
		Me.pnlETQueue = New System.Windows.Forms.Panel()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.btnTransferNow = New System.Windows.Forms.Button()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.btnSetAsFanart = New System.Windows.Forms.Button()
		Me.btnThumbsRefresh = New System.Windows.Forms.Button()
		Me.btnRemoveThumb = New System.Windows.Forms.Button()
		Me.btnDown = New System.Windows.Forms.Button()
		Me.btnUp = New System.Windows.Forms.Button()
		Me.lvThumbs = New System.Windows.Forms.ListView()
		Me.ilThumbs = New System.Windows.Forms.ImageList(Me.components)
		Me.pbExtraThumbs = New System.Windows.Forms.PictureBox()
		Me.TabPage4 = New System.Windows.Forms.TabPage()
		Me.pnlFrameExtrator = New System.Windows.Forms.Panel()
		Me.TabPage6 = New System.Windows.Forms.TabPage()
		Me.pnlFileInfo = New System.Windows.Forms.Panel()
		Me.ofdImage = New System.Windows.Forms.OpenFileDialog()
		Me.chkMark = New System.Windows.Forms.CheckBox()
		Me.btnRescrape = New System.Windows.Forms.Button()
		Me.btnChangeMovie = New System.Windows.Forms.Button()
		Me.btnClearCache = New System.Windows.Forms.Button()
		Me.DelayTimer = New System.Windows.Forms.Timer(Me.components)
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabControl1.SuspendLayout()
		Me.TabPage1.SuspendLayout()
		CType(Me.pbStar5, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbStar4, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbStar3, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbStar2, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbStar1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage2.SuspendLayout()
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage3.SuspendLayout()
		CType(Me.pbFanart, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage5.SuspendLayout()
		Me.pnlETQueue.SuspendLayout()
		Me.Panel1.SuspendLayout()
		CType(Me.pbExtraThumbs, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage4.SuspendLayout()
		Me.TabPage6.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.OK_Button.Location = New System.Drawing.Point(708, 553)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(781, 553)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'pnlTop
		'
		Me.pnlTop.BackColor = System.Drawing.Color.LightSteelBlue
		Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlTop.Controls.Add(Me.Label2)
		Me.pnlTop.Controls.Add(Me.Label1)
		Me.pnlTop.Controls.Add(Me.PictureBox1)
		Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
		Me.pnlTop.Location = New System.Drawing.Point(0, 0)
		Me.pnlTop.Name = "pnlTop"
		Me.pnlTop.Size = New System.Drawing.Size(854, 64)
		Me.pnlTop.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.BackColor = System.Drawing.Color.Transparent
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.ForeColor = System.Drawing.Color.White
		Me.Label2.Location = New System.Drawing.Point(61, 38)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(205, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Edit the details for the selected movie."
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.BackColor = System.Drawing.Color.Transparent
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.ForeColor = System.Drawing.Color.White
		Me.Label1.Location = New System.Drawing.Point(58, 3)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(137, 32)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Edit Movie"
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(7, 8)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(48, 48)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
		Me.PictureBox1.TabIndex = 0
		Me.PictureBox1.TabStop = False
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Controls.Add(Me.TabPage2)
		Me.TabControl1.Controls.Add(Me.TabPage3)
		Me.TabControl1.Controls.Add(Me.TabPage5)
		Me.TabControl1.Controls.Add(Me.TabPage4)
		Me.TabControl1.Controls.Add(Me.TabPage6)
		Me.TabControl1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.TabControl1.Location = New System.Drawing.Point(4, 70)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(844, 478)
		Me.TabControl1.TabIndex = 3
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.txtOriginalTitle)
		Me.TabPage1.Controls.Add(Me.lblOriginalTitle)
		Me.TabPage1.Controls.Add(Me.txtCountry)
		Me.TabPage1.Controls.Add(Me.lblCountry)
		Me.TabPage1.Controls.Add(Me.txtFileSource)
		Me.TabPage1.Controls.Add(Me.lblFileSource)
		Me.TabPage1.Controls.Add(Me.btnActorDown)
		Me.TabPage1.Controls.Add(Me.btnActorUp)
		Me.TabPage1.Controls.Add(Me.Label6)
		Me.TabPage1.Controls.Add(Me.txtSortTitle)
		Me.TabPage1.Controls.Add(Me.lblLocalTrailer)
		Me.TabPage1.Controls.Add(Me.btnPlayTrailer)
		Me.TabPage1.Controls.Add(Me.btnDLTrailer)
		Me.TabPage1.Controls.Add(Me.lbGenre)
		Me.TabPage1.Controls.Add(Me.btnStudio)
		Me.TabPage1.Controls.Add(Me.lblStudio)
		Me.TabPage1.Controls.Add(Me.txtStudio)
		Me.TabPage1.Controls.Add(Me.lblTrailer)
		Me.TabPage1.Controls.Add(Me.txtTrailer)
		Me.TabPage1.Controls.Add(Me.txtReleaseDate)
		Me.TabPage1.Controls.Add(Me.lblReleaseDate)
		Me.TabPage1.Controls.Add(Me.lblCredits)
		Me.TabPage1.Controls.Add(Me.txtCredits)
		Me.TabPage1.Controls.Add(Me.lblCerts)
		Me.TabPage1.Controls.Add(Me.txtCerts)
		Me.TabPage1.Controls.Add(Me.lblRuntime)
		Me.TabPage1.Controls.Add(Me.txtRuntime)
		Me.TabPage1.Controls.Add(Me.lblMPAADesc)
		Me.TabPage1.Controls.Add(Me.txtMPAADesc)
		Me.TabPage1.Controls.Add(Me.btnEditActor)
		Me.TabPage1.Controls.Add(Me.btnAddActor)
		Me.TabPage1.Controls.Add(Me.btnManual)
		Me.TabPage1.Controls.Add(Me.btnRemove)
		Me.TabPage1.Controls.Add(Me.lblActors)
		Me.TabPage1.Controls.Add(Me.lvActors)
		Me.TabPage1.Controls.Add(Me.lbMPAA)
		Me.TabPage1.Controls.Add(Me.lblGenre)
		Me.TabPage1.Controls.Add(Me.lblMPAA)
		Me.TabPage1.Controls.Add(Me.lblDirector)
		Me.TabPage1.Controls.Add(Me.txtDirector)
		Me.TabPage1.Controls.Add(Me.txtTop250)
		Me.TabPage1.Controls.Add(Me.lblTop250)
		Me.TabPage1.Controls.Add(Me.lblPlot)
		Me.TabPage1.Controls.Add(Me.txtPlot)
		Me.TabPage1.Controls.Add(Me.lblOutline)
		Me.TabPage1.Controls.Add(Me.txtOutline)
		Me.TabPage1.Controls.Add(Me.lblTagline)
		Me.TabPage1.Controls.Add(Me.txtTagline)
		Me.TabPage1.Controls.Add(Me.pbStar5)
		Me.TabPage1.Controls.Add(Me.pbStar4)
		Me.TabPage1.Controls.Add(Me.pbStar3)
		Me.TabPage1.Controls.Add(Me.pbStar2)
		Me.TabPage1.Controls.Add(Me.pbStar1)
		Me.TabPage1.Controls.Add(Me.txtVotes)
		Me.TabPage1.Controls.Add(Me.lblVotes)
		Me.TabPage1.Controls.Add(Me.lblRating)
		Me.TabPage1.Controls.Add(Me.mtxtYear)
		Me.TabPage1.Controls.Add(Me.lblYear)
		Me.TabPage1.Controls.Add(Me.lblTitle)
		Me.TabPage1.Controls.Add(Me.txtTitle)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage1.Size = New System.Drawing.Size(836, 452)
		Me.TabPage1.TabIndex = 0
		Me.TabPage1.Text = "Details"
		Me.TabPage1.UseVisualStyleBackColor = True
		'
		'txtOriginalTitle
		'
		Me.txtOriginalTitle.BackColor = System.Drawing.SystemColors.Window
		Me.txtOriginalTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtOriginalTitle.Location = New System.Drawing.Point(7, 63)
		Me.txtOriginalTitle.Name = "txtOriginalTitle"
		Me.txtOriginalTitle.Size = New System.Drawing.Size(192, 22)
		Me.txtOriginalTitle.TabIndex = 3
		'
		'lblOriginalTitle
		'
		Me.lblOriginalTitle.AutoSize = True
		Me.lblOriginalTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblOriginalTitle.Location = New System.Drawing.Point(5, 47)
		Me.lblOriginalTitle.Name = "lblOriginalTitle"
		Me.lblOriginalTitle.Size = New System.Drawing.Size(76, 13)
		Me.lblOriginalTitle.TabIndex = 2
		Me.lblOriginalTitle.Text = "Original Title:"
		'
		'txtCountry
		'
		Me.txtCountry.BackColor = System.Drawing.SystemColors.Window
		Me.txtCountry.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtCountry.Location = New System.Drawing.Point(8, 224)
		Me.txtCountry.Name = "txtCountry"
		Me.txtCountry.Size = New System.Drawing.Size(89, 22)
		Me.txtCountry.TabIndex = 12
		'
		'lblCountry
		'
		Me.lblCountry.AutoSize = True
		Me.lblCountry.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCountry.Location = New System.Drawing.Point(5, 209)
		Me.lblCountry.Name = "lblCountry"
		Me.lblCountry.Size = New System.Drawing.Size(52, 13)
		Me.lblCountry.TabIndex = 11
		Me.lblCountry.Text = "Country:"
		'
		'txtFileSource
		'
		Me.txtFileSource.BackColor = System.Drawing.SystemColors.Window
		Me.txtFileSource.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtFileSource.Location = New System.Drawing.Point(636, 385)
		Me.txtFileSource.Name = "txtFileSource"
		Me.txtFileSource.Size = New System.Drawing.Size(167, 22)
		Me.txtFileSource.TabIndex = 48
		'
		'lblFileSource
		'
		Me.lblFileSource.AutoSize = True
		Me.lblFileSource.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblFileSource.Location = New System.Drawing.Point(633, 370)
		Me.lblFileSource.Name = "lblFileSource"
		Me.lblFileSource.Size = New System.Drawing.Size(78, 13)
		Me.lblFileSource.TabIndex = 47
		Me.lblFileSource.Text = "Video Source:"
		'
		'btnActorDown
		'
		Me.btnActorDown.Image = CType(resources.GetObject("btnActorDown.Image"), System.Drawing.Image)
		Me.btnActorDown.Location = New System.Drawing.Point(430, 304)
		Me.btnActorDown.Name = "btnActorDown"
		Me.btnActorDown.Size = New System.Drawing.Size(23, 23)
		Me.btnActorDown.TabIndex = 34
		Me.btnActorDown.UseVisualStyleBackColor = True
		'
		'btnActorUp
		'
		Me.btnActorUp.Image = CType(resources.GetObject("btnActorUp.Image"), System.Drawing.Image)
		Me.btnActorUp.Location = New System.Drawing.Point(406, 304)
		Me.btnActorUp.Name = "btnActorUp"
		Me.btnActorUp.Size = New System.Drawing.Size(23, 23)
		Me.btnActorUp.TabIndex = 33
		Me.btnActorUp.UseVisualStyleBackColor = True
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label6.Location = New System.Drawing.Point(5, 87)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(56, 13)
		Me.Label6.TabIndex = 4
		Me.Label6.Text = "Sort Title:"
		'
		'txtSortTitle
		'
		Me.txtSortTitle.BackColor = System.Drawing.SystemColors.Window
		Me.txtSortTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtSortTitle.Location = New System.Drawing.Point(7, 102)
		Me.txtSortTitle.Name = "txtSortTitle"
		Me.txtSortTitle.Size = New System.Drawing.Size(192, 22)
		Me.txtSortTitle.TabIndex = 5
		'
		'lblLocalTrailer
		'
		Me.lblLocalTrailer.AutoSize = True
		Me.lblLocalTrailer.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblLocalTrailer.ForeColor = System.Drawing.Color.Green
		Me.lblLocalTrailer.Location = New System.Drawing.Point(487, 414)
		Me.lblLocalTrailer.Name = "lblLocalTrailer"
		Me.lblLocalTrailer.Size = New System.Drawing.Size(90, 9)
		Me.lblLocalTrailer.TabIndex = 51
		Me.lblLocalTrailer.Text = "Local Trailer Found"
		Me.lblLocalTrailer.Visible = False
		'
		'btnPlayTrailer
		'
		Me.btnPlayTrailer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnPlayTrailer.Image = Global.Ember_Media_Manager.My.Resources.Resources.Play_Icon
		Me.btnPlayTrailer.Location = New System.Drawing.Point(579, 423)
		Me.btnPlayTrailer.Name = "btnPlayTrailer"
		Me.btnPlayTrailer.Size = New System.Drawing.Size(23, 23)
		Me.btnPlayTrailer.TabIndex = 52
		Me.btnPlayTrailer.UseVisualStyleBackColor = True
		'
		'btnDLTrailer
		'
		Me.btnDLTrailer.Image = CType(resources.GetObject("btnDLTrailer.Image"), System.Drawing.Image)
		Me.btnDLTrailer.Location = New System.Drawing.Point(602, 423)
		Me.btnDLTrailer.Name = "btnDLTrailer"
		Me.btnDLTrailer.Size = New System.Drawing.Size(23, 23)
		Me.btnDLTrailer.TabIndex = 53
		Me.btnDLTrailer.UseVisualStyleBackColor = True
		'
		'lbGenre
		'
		Me.lbGenre.BackColor = System.Drawing.SystemColors.Window
		Me.lbGenre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lbGenre.CheckOnClick = True
		Me.lbGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lbGenre.FormattingEnabled = True
		Me.lbGenre.IntegralHeight = False
		Me.lbGenre.Location = New System.Drawing.Point(8, 345)
		Me.lbGenre.Name = "lbGenre"
		Me.lbGenre.Size = New System.Drawing.Size(192, 100)
		Me.lbGenre.Sorted = True
		Me.lbGenre.TabIndex = 24
		'
		'btnStudio
		'
		Me.btnStudio.Image = CType(resources.GetObject("btnStudio.Image"), System.Drawing.Image)
		Me.btnStudio.Location = New System.Drawing.Point(805, 343)
		Me.btnStudio.Name = "btnStudio"
		Me.btnStudio.Size = New System.Drawing.Size(23, 23)
		Me.btnStudio.TabIndex = 44
		Me.btnStudio.UseVisualStyleBackColor = True
		'
		'lblStudio
		'
		Me.lblStudio.AutoSize = True
		Me.lblStudio.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblStudio.Location = New System.Drawing.Point(633, 330)
		Me.lblStudio.Name = "lblStudio"
		Me.lblStudio.Size = New System.Drawing.Size(44, 13)
		Me.lblStudio.TabIndex = 42
		Me.lblStudio.Text = "Studio:"
		'
		'txtStudio
		'
		Me.txtStudio.BackColor = System.Drawing.SystemColors.Window
		Me.txtStudio.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtStudio.Location = New System.Drawing.Point(635, 345)
		Me.txtStudio.Name = "txtStudio"
		Me.txtStudio.Size = New System.Drawing.Size(167, 22)
		Me.txtStudio.TabIndex = 43
		'
		'lblTrailer
		'
		Me.lblTrailer.AutoSize = True
		Me.lblTrailer.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTrailer.Location = New System.Drawing.Point(215, 410)
		Me.lblTrailer.Name = "lblTrailer"
		Me.lblTrailer.Size = New System.Drawing.Size(66, 13)
		Me.lblTrailer.TabIndex = 49
		Me.lblTrailer.Text = "Trailer URL:"
		'
		'txtTrailer
		'
		Me.txtTrailer.BackColor = System.Drawing.SystemColors.Window
		Me.txtTrailer.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTrailer.Location = New System.Drawing.Point(217, 425)
		Me.txtTrailer.Name = "txtTrailer"
		Me.txtTrailer.Size = New System.Drawing.Size(360, 22)
		Me.txtTrailer.TabIndex = 50
		'
		'txtReleaseDate
		'
		Me.txtReleaseDate.BackColor = System.Drawing.SystemColors.Window
		Me.txtReleaseDate.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtReleaseDate.Location = New System.Drawing.Point(103, 224)
		Me.txtReleaseDate.Name = "txtReleaseDate"
		Me.txtReleaseDate.Size = New System.Drawing.Size(96, 22)
		Me.txtReleaseDate.TabIndex = 14
		'
		'lblReleaseDate
		'
		Me.lblReleaseDate.AutoSize = True
		Me.lblReleaseDate.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblReleaseDate.Location = New System.Drawing.Point(100, 209)
		Me.lblReleaseDate.Name = "lblReleaseDate"
		Me.lblReleaseDate.Size = New System.Drawing.Size(76, 13)
		Me.lblReleaseDate.TabIndex = 13
		Me.lblReleaseDate.Text = "Release Date:"
		'
		'lblCredits
		'
		Me.lblCredits.AutoSize = True
		Me.lblCredits.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCredits.Location = New System.Drawing.Point(215, 330)
		Me.lblCredits.Name = "lblCredits"
		Me.lblCredits.Size = New System.Drawing.Size(46, 13)
		Me.lblCredits.TabIndex = 40
		Me.lblCredits.Text = "Credits:"
		'
		'txtCredits
		'
		Me.txtCredits.BackColor = System.Drawing.SystemColors.Window
		Me.txtCredits.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtCredits.Location = New System.Drawing.Point(217, 345)
		Me.txtCredits.Name = "txtCredits"
		Me.txtCredits.Size = New System.Drawing.Size(408, 22)
		Me.txtCredits.TabIndex = 41
		'
		'lblCerts
		'
		Me.lblCerts.AutoSize = True
		Me.lblCerts.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCerts.Location = New System.Drawing.Point(215, 370)
		Me.lblCerts.Name = "lblCerts"
		Me.lblCerts.Size = New System.Drawing.Size(86, 13)
		Me.lblCerts.TabIndex = 45
		Me.lblCerts.Text = "Certification(s):"
		'
		'txtCerts
		'
		Me.txtCerts.BackColor = System.Drawing.SystemColors.Window
		Me.txtCerts.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtCerts.Location = New System.Drawing.Point(217, 385)
		Me.txtCerts.Name = "txtCerts"
		Me.txtCerts.Size = New System.Drawing.Size(408, 22)
		Me.txtCerts.TabIndex = 46
		'
		'lblRuntime
		'
		Me.lblRuntime.AutoSize = True
		Me.lblRuntime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblRuntime.Location = New System.Drawing.Point(5, 249)
		Me.lblRuntime.Name = "lblRuntime"
		Me.lblRuntime.Size = New System.Drawing.Size(54, 13)
		Me.lblRuntime.TabIndex = 15
		Me.lblRuntime.Text = "Runtime:"
		'
		'txtRuntime
		'
		Me.txtRuntime.BackColor = System.Drawing.SystemColors.Window
		Me.txtRuntime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtRuntime.Location = New System.Drawing.Point(7, 264)
		Me.txtRuntime.Name = "txtRuntime"
		Me.txtRuntime.Size = New System.Drawing.Size(66, 22)
		Me.txtRuntime.TabIndex = 16
		'
		'lblMPAADesc
		'
		Me.lblMPAADesc.AutoSize = True
		Me.lblMPAADesc.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblMPAADesc.Location = New System.Drawing.Point(633, 245)
		Me.lblMPAADesc.Name = "lblMPAADesc"
		Me.lblMPAADesc.Size = New System.Drawing.Size(143, 13)
		Me.lblMPAADesc.TabIndex = 38
		Me.lblMPAADesc.Text = "MPAA Rating Description:"
		'
		'txtMPAADesc
		'
		Me.txtMPAADesc.BackColor = System.Drawing.SystemColors.Window
		Me.txtMPAADesc.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtMPAADesc.Location = New System.Drawing.Point(635, 260)
		Me.txtMPAADesc.Multiline = True
		Me.txtMPAADesc.Name = "txtMPAADesc"
		Me.txtMPAADesc.Size = New System.Drawing.Size(193, 64)
		Me.txtMPAADesc.TabIndex = 39
		'
		'btnEditActor
		'
		Me.btnEditActor.Image = CType(resources.GetObject("btnEditActor.Image"), System.Drawing.Image)
		Me.btnEditActor.Location = New System.Drawing.Point(246, 303)
		Me.btnEditActor.Name = "btnEditActor"
		Me.btnEditActor.Size = New System.Drawing.Size(23, 23)
		Me.btnEditActor.TabIndex = 32
		Me.btnEditActor.UseVisualStyleBackColor = True
		'
		'btnAddActor
		'
		Me.btnAddActor.Image = CType(resources.GetObject("btnAddActor.Image"), System.Drawing.Image)
		Me.btnAddActor.Location = New System.Drawing.Point(217, 303)
		Me.btnAddActor.Name = "btnAddActor"
		Me.btnAddActor.Size = New System.Drawing.Size(23, 23)
		Me.btnAddActor.TabIndex = 31
		Me.btnAddActor.UseVisualStyleBackColor = True
		'
		'btnManual
		'
		Me.btnManual.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnManual.Location = New System.Drawing.Point(738, 423)
		Me.btnManual.Name = "btnManual"
		Me.btnManual.Size = New System.Drawing.Size(92, 23)
		Me.btnManual.TabIndex = 54
		Me.btnManual.Text = "Manual Edit"
		Me.btnManual.UseVisualStyleBackColor = True
		'
		'btnRemove
		'
		Me.btnRemove.Image = CType(resources.GetObject("btnRemove.Image"), System.Drawing.Image)
		Me.btnRemove.Location = New System.Drawing.Point(602, 303)
		Me.btnRemove.Name = "btnRemove"
		Me.btnRemove.Size = New System.Drawing.Size(23, 23)
		Me.btnRemove.TabIndex = 35
		Me.btnRemove.UseVisualStyleBackColor = True
		'
		'lblActors
		'
		Me.lblActors.AutoSize = True
		Me.lblActors.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblActors.Location = New System.Drawing.Point(215, 141)
		Me.lblActors.Name = "lblActors"
		Me.lblActors.Size = New System.Drawing.Size(43, 13)
		Me.lblActors.TabIndex = 29
		Me.lblActors.Text = "Actors:"
		'
		'lvActors
		'
		Me.lvActors.BackColor = System.Drawing.SystemColors.Window
		Me.lvActors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lvActors.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colRole, Me.colThumb})
		Me.lvActors.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lvActors.FullRowSelect = True
		Me.lvActors.Location = New System.Drawing.Point(217, 155)
		Me.lvActors.Name = "lvActors"
		Me.lvActors.Size = New System.Drawing.Size(408, 147)
		Me.lvActors.TabIndex = 30
		Me.lvActors.UseCompatibleStateImageBehavior = False
		Me.lvActors.View = System.Windows.Forms.View.Details
		'
		'colName
		'
		Me.colName.Text = "Name"
		Me.colName.Width = 110
		'
		'colRole
		'
		Me.colRole.Text = "Role"
		Me.colRole.Width = 100
		'
		'colThumb
		'
		Me.colThumb.Text = "Thumb"
		Me.colThumb.Width = 174
		'
		'lbMPAA
		'
		Me.lbMPAA.BackColor = System.Drawing.SystemColors.Window
		Me.lbMPAA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lbMPAA.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lbMPAA.FormattingEnabled = True
		Me.lbMPAA.Location = New System.Drawing.Point(635, 155)
		Me.lbMPAA.Name = "lbMPAA"
		Me.lbMPAA.Size = New System.Drawing.Size(193, 80)
		Me.lbMPAA.TabIndex = 37
		'
		'lblGenre
		'
		Me.lblGenre.AutoSize = True
		Me.lblGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblGenre.Location = New System.Drawing.Point(5, 330)
		Me.lblGenre.Name = "lblGenre"
		Me.lblGenre.Size = New System.Drawing.Size(41, 13)
		Me.lblGenre.TabIndex = 23
		Me.lblGenre.Text = "Genre:"
		'
		'lblMPAA
		'
		Me.lblMPAA.AutoSize = True
		Me.lblMPAA.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblMPAA.Location = New System.Drawing.Point(633, 141)
		Me.lblMPAA.Name = "lblMPAA"
		Me.lblMPAA.Size = New System.Drawing.Size(81, 13)
		Me.lblMPAA.TabIndex = 36
		Me.lblMPAA.Text = "MPAA Rating:"
		'
		'lblDirector
		'
		Me.lblDirector.AutoSize = True
		Me.lblDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDirector.Location = New System.Drawing.Point(5, 289)
		Me.lblDirector.Name = "lblDirector"
		Me.lblDirector.Size = New System.Drawing.Size(51, 13)
		Me.lblDirector.TabIndex = 21
		Me.lblDirector.Text = "Director:"
		'
		'txtDirector
		'
		Me.txtDirector.BackColor = System.Drawing.SystemColors.Window
		Me.txtDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtDirector.Location = New System.Drawing.Point(7, 304)
		Me.txtDirector.Name = "txtDirector"
		Me.txtDirector.Size = New System.Drawing.Size(192, 22)
		Me.txtDirector.TabIndex = 22
		'
		'txtTop250
		'
		Me.txtTop250.BackColor = System.Drawing.SystemColors.Window
		Me.txtTop250.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTop250.Location = New System.Drawing.Point(156, 264)
		Me.txtTop250.Name = "txtTop250"
		Me.txtTop250.Size = New System.Drawing.Size(43, 22)
		Me.txtTop250.TabIndex = 20
		'
		'lblTop250
		'
		Me.lblTop250.AutoSize = True
		Me.lblTop250.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTop250.Location = New System.Drawing.Point(153, 249)
		Me.lblTop250.Name = "lblTop250"
		Me.lblTop250.Size = New System.Drawing.Size(51, 13)
		Me.lblTop250.TabIndex = 19
		Me.lblTop250.Text = "Top 250:"
		'
		'lblPlot
		'
		Me.lblPlot.AutoSize = True
		Me.lblPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblPlot.Location = New System.Drawing.Point(441, 7)
		Me.lblPlot.Name = "lblPlot"
		Me.lblPlot.Size = New System.Drawing.Size(31, 13)
		Me.lblPlot.TabIndex = 27
		Me.lblPlot.Text = "Plot:"
		'
		'txtPlot
		'
		Me.txtPlot.AcceptsReturn = True
		Me.txtPlot.BackColor = System.Drawing.SystemColors.Window
		Me.txtPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtPlot.Location = New System.Drawing.Point(443, 22)
		Me.txtPlot.Multiline = True
		Me.txtPlot.Name = "txtPlot"
		Me.txtPlot.Size = New System.Drawing.Size(385, 112)
		Me.txtPlot.TabIndex = 28
		'
		'lblOutline
		'
		Me.lblOutline.AutoSize = True
		Me.lblOutline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblOutline.Location = New System.Drawing.Point(215, 7)
		Me.lblOutline.Name = "lblOutline"
		Me.lblOutline.Size = New System.Drawing.Size(48, 13)
		Me.lblOutline.TabIndex = 25
		Me.lblOutline.Text = "Outline:"
		'
		'txtOutline
		'
		Me.txtOutline.AcceptsReturn = True
		Me.txtOutline.BackColor = System.Drawing.SystemColors.Window
		Me.txtOutline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtOutline.Location = New System.Drawing.Point(217, 22)
		Me.txtOutline.Multiline = True
		Me.txtOutline.Name = "txtOutline"
		Me.txtOutline.Size = New System.Drawing.Size(220, 112)
		Me.txtOutline.TabIndex = 26
		'
		'lblTagline
		'
		Me.lblTagline.AutoSize = True
		Me.lblTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTagline.Location = New System.Drawing.Point(5, 127)
		Me.lblTagline.Name = "lblTagline"
		Me.lblTagline.Size = New System.Drawing.Size(48, 13)
		Me.lblTagline.TabIndex = 6
		Me.lblTagline.Text = "Tagline:"
		'
		'txtTagline
		'
		Me.txtTagline.BackColor = System.Drawing.SystemColors.Window
		Me.txtTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTagline.Location = New System.Drawing.Point(7, 142)
		Me.txtTagline.Name = "txtTagline"
		Me.txtTagline.Size = New System.Drawing.Size(192, 22)
		Me.txtTagline.TabIndex = 7
		'
		'pbStar5
		'
		Me.pbStar5.Location = New System.Drawing.Point(175, 180)
		Me.pbStar5.Name = "pbStar5"
		Me.pbStar5.Size = New System.Drawing.Size(24, 24)
		Me.pbStar5.TabIndex = 67
		Me.pbStar5.TabStop = False
		'
		'pbStar4
		'
		Me.pbStar4.Location = New System.Drawing.Point(151, 180)
		Me.pbStar4.Name = "pbStar4"
		Me.pbStar4.Size = New System.Drawing.Size(24, 24)
		Me.pbStar4.TabIndex = 66
		Me.pbStar4.TabStop = False
		'
		'pbStar3
		'
		Me.pbStar3.Location = New System.Drawing.Point(127, 180)
		Me.pbStar3.Name = "pbStar3"
		Me.pbStar3.Size = New System.Drawing.Size(24, 24)
		Me.pbStar3.TabIndex = 65
		Me.pbStar3.TabStop = False
		'
		'pbStar2
		'
		Me.pbStar2.Location = New System.Drawing.Point(103, 180)
		Me.pbStar2.Name = "pbStar2"
		Me.pbStar2.Size = New System.Drawing.Size(24, 24)
		Me.pbStar2.TabIndex = 64
		Me.pbStar2.TabStop = False
		'
		'pbStar1
		'
		Me.pbStar1.Location = New System.Drawing.Point(79, 180)
		Me.pbStar1.Name = "pbStar1"
		Me.pbStar1.Size = New System.Drawing.Size(24, 24)
		Me.pbStar1.TabIndex = 63
		Me.pbStar1.TabStop = False
		'
		'txtVotes
		'
		Me.txtVotes.BackColor = System.Drawing.SystemColors.Window
		Me.txtVotes.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtVotes.Location = New System.Drawing.Point(81, 264)
		Me.txtVotes.Name = "txtVotes"
		Me.txtVotes.Size = New System.Drawing.Size(66, 22)
		Me.txtVotes.TabIndex = 18
		'
		'lblVotes
		'
		Me.lblVotes.AutoSize = True
		Me.lblVotes.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblVotes.Location = New System.Drawing.Point(78, 249)
		Me.lblVotes.Name = "lblVotes"
		Me.lblVotes.Size = New System.Drawing.Size(39, 13)
		Me.lblVotes.TabIndex = 17
		Me.lblVotes.Text = "Votes:"
		'
		'lblRating
		'
		Me.lblRating.AutoSize = True
		Me.lblRating.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblRating.Location = New System.Drawing.Point(77, 167)
		Me.lblRating.Name = "lblRating"
		Me.lblRating.Size = New System.Drawing.Size(44, 13)
		Me.lblRating.TabIndex = 10
		Me.lblRating.Text = "Rating:"
		'
		'mtxtYear
		'
		Me.mtxtYear.BackColor = System.Drawing.SystemColors.Window
		Me.mtxtYear.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.mtxtYear.Location = New System.Drawing.Point(7, 182)
		Me.mtxtYear.Mask = "####"
		Me.mtxtYear.Name = "mtxtYear"
		Me.mtxtYear.PromptChar = Global.Microsoft.VisualBasic.ChrW(32)
		Me.mtxtYear.Size = New System.Drawing.Size(50, 22)
		Me.mtxtYear.TabIndex = 9
		'
		'lblYear
		'
		Me.lblYear.AutoSize = True
		Me.lblYear.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblYear.Location = New System.Drawing.Point(5, 167)
		Me.lblYear.Name = "lblYear"
		Me.lblYear.Size = New System.Drawing.Size(33, 13)
		Me.lblYear.TabIndex = 8
		Me.lblYear.Text = "Year:"
		'
		'lblTitle
		'
		Me.lblTitle.AutoSize = True
		Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTitle.Location = New System.Drawing.Point(5, 7)
		Me.lblTitle.Name = "lblTitle"
		Me.lblTitle.Size = New System.Drawing.Size(32, 13)
		Me.lblTitle.TabIndex = 0
		Me.lblTitle.Text = "Title:"
		'
		'txtTitle
		'
		Me.txtTitle.BackColor = System.Drawing.SystemColors.Window
		Me.txtTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTitle.Location = New System.Drawing.Point(7, 22)
		Me.txtTitle.Name = "txtTitle"
		Me.txtTitle.Size = New System.Drawing.Size(192, 22)
		Me.txtTitle.TabIndex = 1
		'
		'TabPage2
		'
		Me.TabPage2.Controls.Add(Me.btnSetPosterDL)
		Me.TabPage2.Controls.Add(Me.btnRemovePoster)
		Me.TabPage2.Controls.Add(Me.lblPosterSize)
		Me.TabPage2.Controls.Add(Me.btnSetPosterScrape)
		Me.TabPage2.Controls.Add(Me.btnSetPoster)
		Me.TabPage2.Controls.Add(Me.pbPoster)
		Me.TabPage2.Location = New System.Drawing.Point(4, 22)
		Me.TabPage2.Name = "TabPage2"
		Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage2.Size = New System.Drawing.Size(836, 452)
		Me.TabPage2.TabIndex = 1
		Me.TabPage2.Text = "Poster"
		Me.TabPage2.UseVisualStyleBackColor = True
		'
		'btnSetPosterDL
		'
		Me.btnSetPosterDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetPosterDL.Image = CType(resources.GetObject("btnSetPosterDL.Image"), System.Drawing.Image)
		Me.btnSetPosterDL.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPosterDL.Location = New System.Drawing.Point(735, 180)
		Me.btnSetPosterDL.Name = "btnSetPosterDL"
		Me.btnSetPosterDL.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPosterDL.TabIndex = 3
		Me.btnSetPosterDL.Text = "Change Poster (Download)"
		Me.btnSetPosterDL.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPosterDL.UseVisualStyleBackColor = True
		'
		'btnRemovePoster
		'
		Me.btnRemovePoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnRemovePoster.Image = CType(resources.GetObject("btnRemovePoster.Image"), System.Drawing.Image)
		Me.btnRemovePoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnRemovePoster.Location = New System.Drawing.Point(735, 363)
		Me.btnRemovePoster.Name = "btnRemovePoster"
		Me.btnRemovePoster.Size = New System.Drawing.Size(96, 83)
		Me.btnRemovePoster.TabIndex = 4
		Me.btnRemovePoster.Text = "Remove Poster"
		Me.btnRemovePoster.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnRemovePoster.UseVisualStyleBackColor = True
		'
		'lblPosterSize
		'
		Me.lblPosterSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblPosterSize.Location = New System.Drawing.Point(8, 8)
		Me.lblPosterSize.Name = "lblPosterSize"
		Me.lblPosterSize.Size = New System.Drawing.Size(105, 23)
		Me.lblPosterSize.TabIndex = 0
		Me.lblPosterSize.Text = "Size: (XXXXxXXXX)"
		Me.lblPosterSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblPosterSize.Visible = False
		'
		'btnSetPosterScrape
		'
		Me.btnSetPosterScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetPosterScrape.Image = CType(resources.GetObject("btnSetPosterScrape.Image"), System.Drawing.Image)
		Me.btnSetPosterScrape.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPosterScrape.Location = New System.Drawing.Point(735, 93)
		Me.btnSetPosterScrape.Name = "btnSetPosterScrape"
		Me.btnSetPosterScrape.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPosterScrape.TabIndex = 2
		Me.btnSetPosterScrape.Text = "Change Poster (Scrape)"
		Me.btnSetPosterScrape.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPosterScrape.UseVisualStyleBackColor = True
		'
		'btnSetPoster
		'
		Me.btnSetPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetPoster.Image = CType(resources.GetObject("btnSetPoster.Image"), System.Drawing.Image)
		Me.btnSetPoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPoster.Location = New System.Drawing.Point(735, 6)
		Me.btnSetPoster.Name = "btnSetPoster"
		Me.btnSetPoster.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPoster.TabIndex = 1
		Me.btnSetPoster.Text = "Change Poster (Local Browse)"
		Me.btnSetPoster.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPoster.UseVisualStyleBackColor = True
		'
		'pbPoster
		'
		Me.pbPoster.BackColor = System.Drawing.Color.DimGray
		Me.pbPoster.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbPoster.Location = New System.Drawing.Point(6, 6)
		Me.pbPoster.Name = "pbPoster"
		Me.pbPoster.Size = New System.Drawing.Size(724, 440)
		Me.pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbPoster.TabIndex = 0
		Me.pbPoster.TabStop = False
		'
		'TabPage3
		'
		Me.TabPage3.Controls.Add(Me.btnSetFanartDL)
		Me.TabPage3.Controls.Add(Me.btnRemoveFanart)
		Me.TabPage3.Controls.Add(Me.lblFanartSize)
		Me.TabPage3.Controls.Add(Me.btnSetFanartScrape)
		Me.TabPage3.Controls.Add(Me.btnSetFanart)
		Me.TabPage3.Controls.Add(Me.pbFanart)
		Me.TabPage3.Location = New System.Drawing.Point(4, 22)
		Me.TabPage3.Name = "TabPage3"
		Me.TabPage3.Size = New System.Drawing.Size(836, 452)
		Me.TabPage3.TabIndex = 2
		Me.TabPage3.Text = "Fanart"
		Me.TabPage3.UseVisualStyleBackColor = True
		'
		'btnSetFanartDL
		'
		Me.btnSetFanartDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetFanartDL.Image = CType(resources.GetObject("btnSetFanartDL.Image"), System.Drawing.Image)
		Me.btnSetFanartDL.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanartDL.Location = New System.Drawing.Point(735, 180)
		Me.btnSetFanartDL.Name = "btnSetFanartDL"
		Me.btnSetFanartDL.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanartDL.TabIndex = 3
		Me.btnSetFanartDL.Text = "Change Fanart (Download)"
		Me.btnSetFanartDL.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanartDL.UseVisualStyleBackColor = True
		'
		'btnRemoveFanart
		'
		Me.btnRemoveFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnRemoveFanart.Image = CType(resources.GetObject("btnRemoveFanart.Image"), System.Drawing.Image)
		Me.btnRemoveFanart.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnRemoveFanart.Location = New System.Drawing.Point(735, 363)
		Me.btnRemoveFanart.Name = "btnRemoveFanart"
		Me.btnRemoveFanart.Size = New System.Drawing.Size(96, 83)
		Me.btnRemoveFanart.TabIndex = 4
		Me.btnRemoveFanart.Text = "Remove Fanart"
		Me.btnRemoveFanart.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnRemoveFanart.UseVisualStyleBackColor = True
		'
		'lblFanartSize
		'
		Me.lblFanartSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblFanartSize.Location = New System.Drawing.Point(8, 8)
		Me.lblFanartSize.Name = "lblFanartSize"
		Me.lblFanartSize.Size = New System.Drawing.Size(105, 23)
		Me.lblFanartSize.TabIndex = 0
		Me.lblFanartSize.Text = "Size: (XXXXxXXXX)"
		Me.lblFanartSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblFanartSize.Visible = False
		'
		'btnSetFanartScrape
		'
		Me.btnSetFanartScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetFanartScrape.Image = CType(resources.GetObject("btnSetFanartScrape.Image"), System.Drawing.Image)
		Me.btnSetFanartScrape.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanartScrape.Location = New System.Drawing.Point(735, 93)
		Me.btnSetFanartScrape.Name = "btnSetFanartScrape"
		Me.btnSetFanartScrape.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanartScrape.TabIndex = 2
		Me.btnSetFanartScrape.Text = "Change Fanart (Scrape)"
		Me.btnSetFanartScrape.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanartScrape.UseVisualStyleBackColor = True
		'
		'btnSetFanart
		'
		Me.btnSetFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetFanart.Image = CType(resources.GetObject("btnSetFanart.Image"), System.Drawing.Image)
		Me.btnSetFanart.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanart.Location = New System.Drawing.Point(735, 6)
		Me.btnSetFanart.Name = "btnSetFanart"
		Me.btnSetFanart.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanart.TabIndex = 1
		Me.btnSetFanart.Text = "Change Fanart (Local Browse)"
		Me.btnSetFanart.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanart.UseVisualStyleBackColor = True
		'
		'pbFanart
		'
		Me.pbFanart.BackColor = System.Drawing.Color.DimGray
		Me.pbFanart.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbFanart.Location = New System.Drawing.Point(6, 6)
		Me.pbFanart.Name = "pbFanart"
		Me.pbFanart.Size = New System.Drawing.Size(724, 440)
		Me.pbFanart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbFanart.TabIndex = 1
		Me.pbFanart.TabStop = False
		'
		'TabPage5
		'
		Me.TabPage5.Controls.Add(Me.pnlETQueue)
		Me.TabPage5.Controls.Add(Me.Panel1)
		Me.TabPage5.Controls.Add(Me.btnThumbsRefresh)
		Me.TabPage5.Controls.Add(Me.btnRemoveThumb)
		Me.TabPage5.Controls.Add(Me.btnDown)
		Me.TabPage5.Controls.Add(Me.btnUp)
		Me.TabPage5.Controls.Add(Me.lvThumbs)
		Me.TabPage5.Controls.Add(Me.pbExtraThumbs)
		Me.TabPage5.Location = New System.Drawing.Point(4, 22)
		Me.TabPage5.Name = "TabPage5"
		Me.TabPage5.Size = New System.Drawing.Size(836, 452)
		Me.TabPage5.TabIndex = 4
		Me.TabPage5.Text = "Extrathumbs"
		Me.TabPage5.UseVisualStyleBackColor = True
		'
		'pnlETQueue
		'
		Me.pnlETQueue.BackColor = System.Drawing.Color.LightGray
		Me.pnlETQueue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlETQueue.Controls.Add(Me.Label4)
		Me.pnlETQueue.Controls.Add(Me.btnTransferNow)
		Me.pnlETQueue.Location = New System.Drawing.Point(626, 11)
		Me.pnlETQueue.Name = "pnlETQueue"
		Me.pnlETQueue.Size = New System.Drawing.Size(201, 69)
		Me.pnlETQueue.TabIndex = 1
		Me.pnlETQueue.Visible = False
		'
		'Label4
		'
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.Location = New System.Drawing.Point(3, 3)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(193, 26)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "You have extrathumbs queued to be transferred to the movie directory."
		Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'btnTransferNow
		'
		Me.btnTransferNow.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnTransferNow.Image = CType(resources.GetObject("btnTransferNow.Image"), System.Drawing.Image)
		Me.btnTransferNow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnTransferNow.Location = New System.Drawing.Point(53, 32)
		Me.btnTransferNow.Name = "btnTransferNow"
		Me.btnTransferNow.Size = New System.Drawing.Size(103, 32)
		Me.btnTransferNow.TabIndex = 1
		Me.btnTransferNow.Text = "Transfer Now"
		Me.btnTransferNow.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnTransferNow.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.LightGray
		Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Panel1.Controls.Add(Me.btnSetAsFanart)
		Me.Panel1.Location = New System.Drawing.Point(718, 403)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(109, 39)
		Me.Panel1.TabIndex = 6
		'
		'btnSetAsFanart
		'
		Me.btnSetAsFanart.Enabled = False
		Me.btnSetAsFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSetAsFanart.Image = CType(resources.GetObject("btnSetAsFanart.Image"), System.Drawing.Image)
		Me.btnSetAsFanart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnSetAsFanart.Location = New System.Drawing.Point(2, 3)
		Me.btnSetAsFanart.Name = "btnSetAsFanart"
		Me.btnSetAsFanart.Size = New System.Drawing.Size(103, 32)
		Me.btnSetAsFanart.TabIndex = 0
		Me.btnSetAsFanart.Text = "Set As Fanart"
		Me.btnSetAsFanart.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnSetAsFanart.UseVisualStyleBackColor = True
		'
		'btnThumbsRefresh
		'
		Me.btnThumbsRefresh.Image = CType(resources.GetObject("btnThumbsRefresh.Image"), System.Drawing.Image)
		Me.btnThumbsRefresh.Location = New System.Drawing.Point(87, 422)
		Me.btnThumbsRefresh.Name = "btnThumbsRefresh"
		Me.btnThumbsRefresh.Size = New System.Drawing.Size(23, 23)
		Me.btnThumbsRefresh.TabIndex = 4
		Me.btnThumbsRefresh.UseVisualStyleBackColor = True
		'
		'btnRemoveThumb
		'
		Me.btnRemoveThumb.Image = CType(resources.GetObject("btnRemoveThumb.Image"), System.Drawing.Image)
		Me.btnRemoveThumb.Location = New System.Drawing.Point(147, 422)
		Me.btnRemoveThumb.Name = "btnRemoveThumb"
		Me.btnRemoveThumb.Size = New System.Drawing.Size(23, 23)
		Me.btnRemoveThumb.TabIndex = 5
		Me.btnRemoveThumb.UseVisualStyleBackColor = True
		'
		'btnDown
		'
		Me.btnDown.Image = CType(resources.GetObject("btnDown.Image"), System.Drawing.Image)
		Me.btnDown.Location = New System.Drawing.Point(28, 422)
		Me.btnDown.Name = "btnDown"
		Me.btnDown.Size = New System.Drawing.Size(23, 23)
		Me.btnDown.TabIndex = 3
		Me.btnDown.UseVisualStyleBackColor = True
		'
		'btnUp
		'
		Me.btnUp.Image = CType(resources.GetObject("btnUp.Image"), System.Drawing.Image)
		Me.btnUp.Location = New System.Drawing.Point(4, 422)
		Me.btnUp.Name = "btnUp"
		Me.btnUp.Size = New System.Drawing.Size(23, 23)
		Me.btnUp.TabIndex = 2
		Me.btnUp.UseVisualStyleBackColor = True
		'
		'lvThumbs
		'
		Me.lvThumbs.AutoArrange = False
		Me.lvThumbs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lvThumbs.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lvThumbs.FullRowSelect = True
		Me.lvThumbs.HideSelection = False
		Me.lvThumbs.Location = New System.Drawing.Point(5, 8)
		Me.lvThumbs.Name = "lvThumbs"
		Me.lvThumbs.Size = New System.Drawing.Size(165, 408)
		Me.lvThumbs.SmallImageList = Me.ilThumbs
		Me.lvThumbs.Sorting = System.Windows.Forms.SortOrder.Ascending
		Me.lvThumbs.TabIndex = 0
		Me.lvThumbs.UseCompatibleStateImageBehavior = False
		Me.lvThumbs.View = System.Windows.Forms.View.SmallIcon
		'
		'ilThumbs
		'
		Me.ilThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
		Me.ilThumbs.ImageSize = New System.Drawing.Size(96, 54)
		Me.ilThumbs.TransparentColor = System.Drawing.Color.Transparent
		'
		'pbExtraThumbs
		'
		Me.pbExtraThumbs.BackColor = System.Drawing.Color.DimGray
		Me.pbExtraThumbs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbExtraThumbs.Location = New System.Drawing.Point(176, 8)
		Me.pbExtraThumbs.Name = "pbExtraThumbs"
		Me.pbExtraThumbs.Size = New System.Drawing.Size(653, 437)
		Me.pbExtraThumbs.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbExtraThumbs.TabIndex = 2
		Me.pbExtraThumbs.TabStop = False
		'
		'TabPage4
		'
		Me.TabPage4.Controls.Add(Me.pnlFrameExtrator)
		Me.TabPage4.Location = New System.Drawing.Point(4, 22)
		Me.TabPage4.Name = "TabPage4"
		Me.TabPage4.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage4.Size = New System.Drawing.Size(836, 452)
		Me.TabPage4.TabIndex = 3
		Me.TabPage4.Text = "Frame Extraction"
		Me.TabPage4.UseVisualStyleBackColor = True
		'
		'pnlFrameExtrator
		'
		Me.pnlFrameExtrator.Location = New System.Drawing.Point(0, 0)
		Me.pnlFrameExtrator.Name = "pnlFrameExtrator"
		Me.pnlFrameExtrator.Size = New System.Drawing.Size(834, 452)
		Me.pnlFrameExtrator.TabIndex = 0
		'
		'TabPage6
		'
		Me.TabPage6.Controls.Add(Me.pnlFileInfo)
		Me.TabPage6.Location = New System.Drawing.Point(4, 22)
		Me.TabPage6.Name = "TabPage6"
		Me.TabPage6.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage6.Size = New System.Drawing.Size(836, 452)
		Me.TabPage6.TabIndex = 5
		Me.TabPage6.Text = "Meta Data"
		Me.TabPage6.UseVisualStyleBackColor = True
		'
		'pnlFileInfo
		'
		Me.pnlFileInfo.Location = New System.Drawing.Point(-4, 0)
		Me.pnlFileInfo.Name = "pnlFileInfo"
		Me.pnlFileInfo.Size = New System.Drawing.Size(844, 452)
		Me.pnlFileInfo.TabIndex = 0
		'
		'chkMark
		'
		Me.chkMark.AutoSize = True
		Me.chkMark.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkMark.Location = New System.Drawing.Point(4, 559)
		Me.chkMark.Name = "chkMark"
		Me.chkMark.Size = New System.Drawing.Size(86, 17)
		Me.chkMark.TabIndex = 5
		Me.chkMark.Text = "Mark Movie"
		Me.chkMark.UseVisualStyleBackColor = True
		'
		'btnRescrape
		'
		Me.btnRescrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnRescrape.Image = CType(resources.GetObject("btnRescrape.Image"), System.Drawing.Image)
		Me.btnRescrape.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnRescrape.Location = New System.Drawing.Point(310, 554)
		Me.btnRescrape.Name = "btnRescrape"
		Me.btnRescrape.Size = New System.Drawing.Size(98, 23)
		Me.btnRescrape.TabIndex = 6
		Me.btnRescrape.Text = "Re-scrape"
		Me.btnRescrape.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnRescrape.UseVisualStyleBackColor = True
		'
		'btnChangeMovie
		'
		Me.btnChangeMovie.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnChangeMovie.Image = CType(resources.GetObject("btnChangeMovie.Image"), System.Drawing.Image)
		Me.btnChangeMovie.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnChangeMovie.Location = New System.Drawing.Point(429, 554)
		Me.btnChangeMovie.Name = "btnChangeMovie"
		Me.btnChangeMovie.Size = New System.Drawing.Size(107, 23)
		Me.btnChangeMovie.TabIndex = 7
		Me.btnChangeMovie.Text = "Change Movie"
		Me.btnChangeMovie.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnChangeMovie.UseVisualStyleBackColor = True
		'
		'btnClearCache
		'
		Me.btnClearCache.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnClearCache.Image = CType(resources.GetObject("btnClearCache.Image"), System.Drawing.Image)
		Me.btnClearCache.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnClearCache.Location = New System.Drawing.Point(751, 66)
		Me.btnClearCache.Name = "btnClearCache"
		Me.btnClearCache.Size = New System.Drawing.Size(96, 23)
		Me.btnClearCache.TabIndex = 4
		Me.btnClearCache.Text = "Clear Cache"
		Me.btnClearCache.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnClearCache.UseVisualStyleBackColor = True
		Me.btnClearCache.Visible = False
		'
		'DelayTimer
		'
		Me.DelayTimer.Interval = 250
		'
		'dlgEditMovie
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(854, 582)
		Me.Controls.Add(Me.btnClearCache)
		Me.Controls.Add(Me.btnChangeMovie)
		Me.Controls.Add(Me.btnRescrape)
		Me.Controls.Add(Me.chkMark)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.pnlTop)
		Me.Controls.Add(Me.TabControl1)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgEditMovie"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Edit Movie"
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage1.ResumeLayout(False)
		Me.TabPage1.PerformLayout()
		CType(Me.pbStar5, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbStar4, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbStar3, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbStar2, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbStar1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage2.ResumeLayout(False)
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage3.ResumeLayout(False)
		CType(Me.pbFanart, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage5.ResumeLayout(False)
		Me.pnlETQueue.ResumeLayout(False)
		Me.Panel1.ResumeLayout(False)
		CType(Me.pbExtraThumbs, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage4.ResumeLayout(False)
		Me.TabPage6.ResumeLayout(False)
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents lblMPAADesc As System.Windows.Forms.Label
    Friend WithEvents txtMPAADesc As System.Windows.Forms.TextBox
    Friend WithEvents btnEditActor As System.Windows.Forms.Button
    Friend WithEvents btnAddActor As System.Windows.Forms.Button
    Friend WithEvents btnManual As System.Windows.Forms.Button
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents lblActors As System.Windows.Forms.Label
    Friend WithEvents lvActors As System.Windows.Forms.ListView
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colRole As System.Windows.Forms.ColumnHeader
    Friend WithEvents colThumb As System.Windows.Forms.ColumnHeader
    Friend WithEvents lbMPAA As System.Windows.Forms.ListBox
    Friend WithEvents lblGenre As System.Windows.Forms.Label
    Friend WithEvents lblMPAA As System.Windows.Forms.Label
    Friend WithEvents lblDirector As System.Windows.Forms.Label
    Friend WithEvents txtDirector As System.Windows.Forms.TextBox
    Friend WithEvents txtTop250 As System.Windows.Forms.TextBox
    Friend WithEvents lblTop250 As System.Windows.Forms.Label
    Friend WithEvents lblPlot As System.Windows.Forms.Label
    Friend WithEvents txtPlot As System.Windows.Forms.TextBox
    Friend WithEvents lblOutline As System.Windows.Forms.Label
    Friend WithEvents txtOutline As System.Windows.Forms.TextBox
    Friend WithEvents lblTagline As System.Windows.Forms.Label
    Friend WithEvents txtTagline As System.Windows.Forms.TextBox
    Friend WithEvents pbStar5 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar4 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar3 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar2 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar1 As System.Windows.Forms.PictureBox
    Friend WithEvents txtVotes As System.Windows.Forms.TextBox
    Friend WithEvents lblVotes As System.Windows.Forms.Label
    Friend WithEvents lblRating As System.Windows.Forms.Label
    Friend WithEvents mtxtYear As System.Windows.Forms.MaskedTextBox
    Friend WithEvents lblYear As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents txtTitle As System.Windows.Forms.TextBox
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents btnSetPoster As System.Windows.Forms.Button
    Friend WithEvents pbPoster As System.Windows.Forms.PictureBox
    Friend WithEvents btnSetFanart As System.Windows.Forms.Button
    Friend WithEvents pbFanart As System.Windows.Forms.PictureBox
    Friend WithEvents ofdImage As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblRuntime As System.Windows.Forms.Label
    Friend WithEvents txtRuntime As System.Windows.Forms.TextBox
    Friend WithEvents txtReleaseDate As System.Windows.Forms.TextBox
    Friend WithEvents lblReleaseDate As System.Windows.Forms.Label
    Friend WithEvents lblCredits As System.Windows.Forms.Label
    Friend WithEvents txtCredits As System.Windows.Forms.TextBox
    Friend WithEvents lblCerts As System.Windows.Forms.Label
    Friend WithEvents txtCerts As System.Windows.Forms.TextBox
    Friend WithEvents lblTrailer As System.Windows.Forms.Label
    Friend WithEvents txtTrailer As System.Windows.Forms.TextBox
    Friend WithEvents btnSetPosterScrape As System.Windows.Forms.Button
    Friend WithEvents btnSetFanartScrape As System.Windows.Forms.Button
    Friend WithEvents lblPosterSize As System.Windows.Forms.Label
    Friend WithEvents lblFanartSize As System.Windows.Forms.Label
    Friend WithEvents lblStudio As System.Windows.Forms.Label
    Friend WithEvents txtStudio As System.Windows.Forms.TextBox
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents chkMark As System.Windows.Forms.CheckBox
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents pbExtraThumbs As System.Windows.Forms.PictureBox
    Friend WithEvents lvThumbs As System.Windows.Forms.ListView
    Friend WithEvents ilThumbs As System.Windows.Forms.ImageList
    Friend WithEvents btnDown As System.Windows.Forms.Button
    Friend WithEvents btnUp As System.Windows.Forms.Button
    Friend WithEvents btnRemoveThumb As System.Windows.Forms.Button
    Friend WithEvents btnRescrape As System.Windows.Forms.Button
    Friend WithEvents btnChangeMovie As System.Windows.Forms.Button
    Friend WithEvents btnRemovePoster As System.Windows.Forms.Button
    Friend WithEvents btnRemoveFanart As System.Windows.Forms.Button
    Friend WithEvents btnThumbsRefresh As System.Windows.Forms.Button
    Friend WithEvents btnStudio As System.Windows.Forms.Button
    Friend WithEvents lbGenre As System.Windows.Forms.CheckedListBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents btnSetAsFanart As System.Windows.Forms.Button
    Friend WithEvents btnClearCache As System.Windows.Forms.Button
    Friend WithEvents btnDLTrailer As System.Windows.Forms.Button
    Friend WithEvents btnPlayTrailer As System.Windows.Forms.Button
    Friend WithEvents lblLocalTrailer As System.Windows.Forms.Label
    Friend WithEvents pnlETQueue As System.Windows.Forms.Panel
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents btnTransferNow As System.Windows.Forms.Button
    Friend WithEvents btnSetPosterDL As System.Windows.Forms.Button
    Friend WithEvents btnSetFanartDL As System.Windows.Forms.Button
    Friend WithEvents TabPage6 As System.Windows.Forms.TabPage
    Friend WithEvents pnlFileInfo As System.Windows.Forms.Panel
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtSortTitle As System.Windows.Forms.TextBox
    Friend WithEvents DelayTimer As System.Windows.Forms.Timer
    Friend WithEvents btnActorDown As System.Windows.Forms.Button
    Friend WithEvents btnActorUp As System.Windows.Forms.Button
    Friend WithEvents pnlFrameExtrator As System.Windows.Forms.Panel
    Friend WithEvents txtFileSource As System.Windows.Forms.TextBox
    Friend WithEvents lblFileSource As System.Windows.Forms.Label
    Friend WithEvents lblCountry As System.Windows.Forms.Label
    Friend WithEvents txtCountry As System.Windows.Forms.TextBox
    Friend WithEvents txtOriginalTitle As System.Windows.Forms.TextBox
    Friend WithEvents lblOriginalTitle As System.Windows.Forms.Label

End Class
