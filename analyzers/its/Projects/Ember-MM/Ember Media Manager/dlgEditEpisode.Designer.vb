<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgEditEpisode
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditEpisode))
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.btnActorDown = New System.Windows.Forms.Button()
		Me.btnActorUp = New System.Windows.Forms.Button()
		Me.txtAired = New System.Windows.Forms.TextBox()
		Me.txtEpisode = New System.Windows.Forms.TextBox()
		Me.lblEpisode = New System.Windows.Forms.Label()
		Me.txtSeason = New System.Windows.Forms.TextBox()
		Me.lblSeason = New System.Windows.Forms.Label()
		Me.lblCredits = New System.Windows.Forms.Label()
		Me.txtCredits = New System.Windows.Forms.TextBox()
		Me.btnEditActor = New System.Windows.Forms.Button()
		Me.btnAddActor = New System.Windows.Forms.Button()
		Me.btnManual = New System.Windows.Forms.Button()
		Me.btnRemove = New System.Windows.Forms.Button()
		Me.lblActors = New System.Windows.Forms.Label()
		Me.lvActors = New System.Windows.Forms.ListView()
		Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colRole = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colThumb = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.lblDirector = New System.Windows.Forms.Label()
		Me.txtDirector = New System.Windows.Forms.TextBox()
		Me.lblPlot = New System.Windows.Forms.Label()
		Me.txtPlot = New System.Windows.Forms.TextBox()
		Me.pbStar5 = New System.Windows.Forms.PictureBox()
		Me.pbStar4 = New System.Windows.Forms.PictureBox()
		Me.pbStar3 = New System.Windows.Forms.PictureBox()
		Me.pbStar2 = New System.Windows.Forms.PictureBox()
		Me.pbStar1 = New System.Windows.Forms.PictureBox()
		Me.lblRating = New System.Windows.Forms.Label()
		Me.lblAired = New System.Windows.Forms.Label()
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
		Me.lblFanartSize = New System.Windows.Forms.Label()
		Me.btnSetFanartDL = New System.Windows.Forms.Button()
		Me.btnRemoveFanart = New System.Windows.Forms.Button()
		Me.btnSetFanartScrape = New System.Windows.Forms.Button()
		Me.btnSetFanart = New System.Windows.Forms.Button()
		Me.pbFanart = New System.Windows.Forms.PictureBox()
		Me.TabPage4 = New System.Windows.Forms.TabPage()
		Me.pnlFrameExtrator = New System.Windows.Forms.Panel()
		Me.TabPage5 = New System.Windows.Forms.TabPage()
		Me.pnlFileInfo = New System.Windows.Forms.Panel()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.ofdImage = New System.Windows.Forms.OpenFileDialog()
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
		Me.TabPage4.SuspendLayout()
		Me.TabPage5.SuspendLayout()
		Me.SuspendLayout()
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
		Me.Label2.Size = New System.Drawing.Size(214, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Edit the details for the selected episode."
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.BackColor = System.Drawing.Color.Transparent
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.ForeColor = System.Drawing.Color.White
		Me.Label1.Location = New System.Drawing.Point(58, 3)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(155, 32)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Edit Episode"
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
		Me.PictureBox1.ErrorImage = Nothing
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.InitialImage = Nothing
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
		Me.TabControl1.Controls.Add(Me.TabPage4)
		Me.TabControl1.Controls.Add(Me.TabPage5)
		Me.TabControl1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TabControl1.Location = New System.Drawing.Point(4, 70)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(844, 478)
		Me.TabControl1.TabIndex = 3
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.btnActorDown)
		Me.TabPage1.Controls.Add(Me.btnActorUp)
		Me.TabPage1.Controls.Add(Me.txtAired)
		Me.TabPage1.Controls.Add(Me.txtEpisode)
		Me.TabPage1.Controls.Add(Me.lblEpisode)
		Me.TabPage1.Controls.Add(Me.txtSeason)
		Me.TabPage1.Controls.Add(Me.lblSeason)
		Me.TabPage1.Controls.Add(Me.lblCredits)
		Me.TabPage1.Controls.Add(Me.txtCredits)
		Me.TabPage1.Controls.Add(Me.btnEditActor)
		Me.TabPage1.Controls.Add(Me.btnAddActor)
		Me.TabPage1.Controls.Add(Me.btnManual)
		Me.TabPage1.Controls.Add(Me.btnRemove)
		Me.TabPage1.Controls.Add(Me.lblActors)
		Me.TabPage1.Controls.Add(Me.lvActors)
		Me.TabPage1.Controls.Add(Me.lblDirector)
		Me.TabPage1.Controls.Add(Me.txtDirector)
		Me.TabPage1.Controls.Add(Me.lblPlot)
		Me.TabPage1.Controls.Add(Me.txtPlot)
		Me.TabPage1.Controls.Add(Me.pbStar5)
		Me.TabPage1.Controls.Add(Me.pbStar4)
		Me.TabPage1.Controls.Add(Me.pbStar3)
		Me.TabPage1.Controls.Add(Me.pbStar2)
		Me.TabPage1.Controls.Add(Me.pbStar1)
		Me.TabPage1.Controls.Add(Me.lblRating)
		Me.TabPage1.Controls.Add(Me.lblAired)
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
		'btnActorDown
		'
		Me.btnActorDown.Image = CType(resources.GetObject("btnActorDown.Image"), System.Drawing.Image)
		Me.btnActorDown.Location = New System.Drawing.Point(328, 423)
		Me.btnActorDown.Name = "btnActorDown"
		Me.btnActorDown.Size = New System.Drawing.Size(23, 23)
		Me.btnActorDown.TabIndex = 20
		Me.btnActorDown.UseVisualStyleBackColor = True
		'
		'btnActorUp
		'
		Me.btnActorUp.Image = CType(resources.GetObject("btnActorUp.Image"), System.Drawing.Image)
		Me.btnActorUp.Location = New System.Drawing.Point(304, 423)
		Me.btnActorUp.Name = "btnActorUp"
		Me.btnActorUp.Size = New System.Drawing.Size(23, 23)
		Me.btnActorUp.TabIndex = 19
		Me.btnActorUp.UseVisualStyleBackColor = True
		'
		'txtAired
		'
		Me.txtAired.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtAired.Location = New System.Drawing.Point(111, 67)
		Me.txtAired.Name = "txtAired"
		Me.txtAired.Size = New System.Drawing.Size(88, 22)
		Me.txtAired.TabIndex = 9
		'
		'txtEpisode
		'
		Me.txtEpisode.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtEpisode.Location = New System.Drawing.Point(59, 67)
		Me.txtEpisode.Name = "txtEpisode"
		Me.txtEpisode.Size = New System.Drawing.Size(46, 22)
		Me.txtEpisode.TabIndex = 7
		'
		'lblEpisode
		'
		Me.lblEpisode.AutoSize = True
		Me.lblEpisode.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblEpisode.Location = New System.Drawing.Point(59, 51)
		Me.lblEpisode.Name = "lblEpisode"
		Me.lblEpisode.Size = New System.Drawing.Size(51, 13)
		Me.lblEpisode.TabIndex = 6
		Me.lblEpisode.Text = "Episode:"
		'
		'txtSeason
		'
		Me.txtSeason.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtSeason.Location = New System.Drawing.Point(7, 67)
		Me.txtSeason.Name = "txtSeason"
		Me.txtSeason.Size = New System.Drawing.Size(46, 22)
		Me.txtSeason.TabIndex = 5
		'
		'lblSeason
		'
		Me.lblSeason.AutoSize = True
		Me.lblSeason.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblSeason.Location = New System.Drawing.Point(7, 51)
		Me.lblSeason.Name = "lblSeason"
		Me.lblSeason.Size = New System.Drawing.Size(47, 13)
		Me.lblSeason.TabIndex = 4
		Me.lblSeason.Text = "Season:"
		'
		'lblCredits
		'
		Me.lblCredits.AutoSize = True
		Me.lblCredits.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCredits.Location = New System.Drawing.Point(217, 139)
		Me.lblCredits.Name = "lblCredits"
		Me.lblCredits.Size = New System.Drawing.Size(46, 13)
		Me.lblCredits.TabIndex = 13
		Me.lblCredits.Text = "Credits:"
		'
		'txtCredits
		'
		Me.txtCredits.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtCredits.Location = New System.Drawing.Point(217, 155)
		Me.txtCredits.Name = "txtCredits"
		Me.txtCredits.Size = New System.Drawing.Size(408, 22)
		Me.txtCredits.TabIndex = 14
		'
		'btnEditActor
		'
		Me.btnEditActor.Image = CType(resources.GetObject("btnEditActor.Image"), System.Drawing.Image)
		Me.btnEditActor.Location = New System.Drawing.Point(35, 423)
		Me.btnEditActor.Name = "btnEditActor"
		Me.btnEditActor.Size = New System.Drawing.Size(23, 23)
		Me.btnEditActor.TabIndex = 18
		Me.btnEditActor.UseVisualStyleBackColor = True
		'
		'btnAddActor
		'
		Me.btnAddActor.Image = CType(resources.GetObject("btnAddActor.Image"), System.Drawing.Image)
		Me.btnAddActor.Location = New System.Drawing.Point(6, 423)
		Me.btnAddActor.Name = "btnAddActor"
		Me.btnAddActor.Size = New System.Drawing.Size(23, 23)
		Me.btnAddActor.TabIndex = 17
		Me.btnAddActor.UseVisualStyleBackColor = True
		'
		'btnManual
		'
		Me.btnManual.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnManual.Location = New System.Drawing.Point(738, 423)
		Me.btnManual.Name = "btnManual"
		Me.btnManual.Size = New System.Drawing.Size(92, 23)
		Me.btnManual.TabIndex = 22
		Me.btnManual.Text = "Manual Edit"
		Me.btnManual.UseVisualStyleBackColor = True
		'
		'btnRemove
		'
		Me.btnRemove.Image = CType(resources.GetObject("btnRemove.Image"), System.Drawing.Image)
		Me.btnRemove.Location = New System.Drawing.Point(602, 423)
		Me.btnRemove.Name = "btnRemove"
		Me.btnRemove.Size = New System.Drawing.Size(23, 23)
		Me.btnRemove.TabIndex = 21
		Me.btnRemove.UseVisualStyleBackColor = True
		'
		'lblActors
		'
		Me.lblActors.AutoSize = True
		Me.lblActors.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblActors.Location = New System.Drawing.Point(7, 188)
		Me.lblActors.Name = "lblActors"
		Me.lblActors.Size = New System.Drawing.Size(43, 13)
		Me.lblActors.TabIndex = 15
		Me.lblActors.Text = "Actors:"
		'
		'lvActors
		'
		Me.lvActors.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colRole, Me.colThumb})
		Me.lvActors.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lvActors.FullRowSelect = True
		Me.lvActors.Location = New System.Drawing.Point(7, 204)
		Me.lvActors.Name = "lvActors"
		Me.lvActors.Size = New System.Drawing.Size(618, 213)
		Me.lvActors.TabIndex = 16
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
		Me.colThumb.Width = 387
		'
		'lblDirector
		'
		Me.lblDirector.AutoSize = True
		Me.lblDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDirector.Location = New System.Drawing.Point(7, 139)
		Me.lblDirector.Name = "lblDirector"
		Me.lblDirector.Size = New System.Drawing.Size(51, 13)
		Me.lblDirector.TabIndex = 11
		Me.lblDirector.Text = "Director:"
		'
		'txtDirector
		'
		Me.txtDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtDirector.Location = New System.Drawing.Point(7, 155)
		Me.txtDirector.Name = "txtDirector"
		Me.txtDirector.Size = New System.Drawing.Size(192, 22)
		Me.txtDirector.TabIndex = 12
		'
		'lblPlot
		'
		Me.lblPlot.AutoSize = True
		Me.lblPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblPlot.Location = New System.Drawing.Point(217, 7)
		Me.lblPlot.Name = "lblPlot"
		Me.lblPlot.Size = New System.Drawing.Size(31, 13)
		Me.lblPlot.TabIndex = 2
		Me.lblPlot.Text = "Plot:"
		'
		'txtPlot
		'
		Me.txtPlot.AcceptsReturn = True
		Me.txtPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtPlot.Location = New System.Drawing.Point(217, 26)
		Me.txtPlot.Multiline = True
		Me.txtPlot.Name = "txtPlot"
		Me.txtPlot.Size = New System.Drawing.Size(611, 108)
		Me.txtPlot.TabIndex = 3
		'
		'pbStar5
		'
		Me.pbStar5.Location = New System.Drawing.Point(104, 110)
		Me.pbStar5.Name = "pbStar5"
		Me.pbStar5.Size = New System.Drawing.Size(24, 24)
		Me.pbStar5.TabIndex = 67
		Me.pbStar5.TabStop = False
		'
		'pbStar4
		'
		Me.pbStar4.Location = New System.Drawing.Point(80, 110)
		Me.pbStar4.Name = "pbStar4"
		Me.pbStar4.Size = New System.Drawing.Size(24, 24)
		Me.pbStar4.TabIndex = 66
		Me.pbStar4.TabStop = False
		'
		'pbStar3
		'
		Me.pbStar3.Location = New System.Drawing.Point(56, 110)
		Me.pbStar3.Name = "pbStar3"
		Me.pbStar3.Size = New System.Drawing.Size(24, 24)
		Me.pbStar3.TabIndex = 65
		Me.pbStar3.TabStop = False
		'
		'pbStar2
		'
		Me.pbStar2.Location = New System.Drawing.Point(32, 110)
		Me.pbStar2.Name = "pbStar2"
		Me.pbStar2.Size = New System.Drawing.Size(24, 24)
		Me.pbStar2.TabIndex = 64
		Me.pbStar2.TabStop = False
		'
		'pbStar1
		'
		Me.pbStar1.Location = New System.Drawing.Point(8, 110)
		Me.pbStar1.Name = "pbStar1"
		Me.pbStar1.Size = New System.Drawing.Size(24, 24)
		Me.pbStar1.TabIndex = 63
		Me.pbStar1.TabStop = False
		'
		'lblRating
		'
		Me.lblRating.AutoSize = True
		Me.lblRating.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblRating.Location = New System.Drawing.Point(7, 94)
		Me.lblRating.Name = "lblRating"
		Me.lblRating.Size = New System.Drawing.Size(44, 13)
		Me.lblRating.TabIndex = 10
		Me.lblRating.Text = "Rating:"
		'
		'lblAired
		'
		Me.lblAired.AutoSize = True
		Me.lblAired.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblAired.Location = New System.Drawing.Point(111, 51)
		Me.lblAired.Name = "lblAired"
		Me.lblAired.Size = New System.Drawing.Size(38, 13)
		Me.lblAired.TabIndex = 8
		Me.lblAired.Text = "Aired:"
		'
		'lblTitle
		'
		Me.lblTitle.AutoSize = True
		Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTitle.Location = New System.Drawing.Point(7, 7)
		Me.lblTitle.Name = "lblTitle"
		Me.lblTitle.Size = New System.Drawing.Size(32, 13)
		Me.lblTitle.TabIndex = 0
		Me.lblTitle.Text = "Title:"
		'
		'txtTitle
		'
		Me.txtTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtTitle.Location = New System.Drawing.Point(7, 26)
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
		Me.btnSetPosterDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.btnRemovePoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.lblPosterSize.Size = New System.Drawing.Size(104, 23)
		Me.lblPosterSize.TabIndex = 0
		Me.lblPosterSize.Text = "Size: (XXXXxXXXX)"
		Me.lblPosterSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblPosterSize.Visible = False
		'
		'btnSetPosterScrape
		'
		Me.btnSetPosterScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.btnSetPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetPoster.Image = CType(resources.GetObject("btnSetPoster.Image"), System.Drawing.Image)
		Me.btnSetPoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPoster.Location = New System.Drawing.Point(735, 6)
		Me.btnSetPoster.Name = "btnSetPoster"
		Me.btnSetPoster.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPoster.TabIndex = 1
		Me.btnSetPoster.Text = "Change Poster (Local)"
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
		Me.TabPage3.Controls.Add(Me.lblFanartSize)
		Me.TabPage3.Controls.Add(Me.btnSetFanartDL)
		Me.TabPage3.Controls.Add(Me.btnRemoveFanart)
		Me.TabPage3.Controls.Add(Me.btnSetFanartScrape)
		Me.TabPage3.Controls.Add(Me.btnSetFanart)
		Me.TabPage3.Controls.Add(Me.pbFanart)
		Me.TabPage3.Location = New System.Drawing.Point(4, 22)
		Me.TabPage3.Name = "TabPage3"
		Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage3.Size = New System.Drawing.Size(836, 452)
		Me.TabPage3.TabIndex = 6
		Me.TabPage3.Text = "Fanart"
		Me.TabPage3.UseVisualStyleBackColor = True
		'
		'lblFanartSize
		'
		Me.lblFanartSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblFanartSize.Location = New System.Drawing.Point(8, 8)
		Me.lblFanartSize.Name = "lblFanartSize"
		Me.lblFanartSize.Size = New System.Drawing.Size(104, 23)
		Me.lblFanartSize.TabIndex = 0
		Me.lblFanartSize.Text = "Size: (XXXXxXXXX)"
		Me.lblFanartSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblFanartSize.Visible = False
		'
		'btnSetFanartDL
		'
		Me.btnSetFanartDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.btnRemoveFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		'btnSetFanartScrape
		'
		Me.btnSetFanartScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.btnSetFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetFanart.Image = CType(resources.GetObject("btnSetFanart.Image"), System.Drawing.Image)
		Me.btnSetFanart.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanart.Location = New System.Drawing.Point(735, 6)
		Me.btnSetFanart.Name = "btnSetFanart"
		Me.btnSetFanart.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanart.TabIndex = 1
		Me.btnSetFanart.Text = "Change Fanart (Local)"
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
		Me.pbFanart.TabIndex = 30
		Me.pbFanart.TabStop = False
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
		Me.pnlFrameExtrator.Location = New System.Drawing.Point(1, 0)
		Me.pnlFrameExtrator.Name = "pnlFrameExtrator"
		Me.pnlFrameExtrator.Size = New System.Drawing.Size(834, 452)
		Me.pnlFrameExtrator.TabIndex = 0
		'
		'TabPage5
		'
		Me.TabPage5.Controls.Add(Me.pnlFileInfo)
		Me.TabPage5.Location = New System.Drawing.Point(4, 22)
		Me.TabPage5.Name = "TabPage5"
		Me.TabPage5.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage5.Size = New System.Drawing.Size(836, 452)
		Me.TabPage5.TabIndex = 5
		Me.TabPage5.Text = "Meta Data"
		Me.TabPage5.UseVisualStyleBackColor = True
		'
		'pnlFileInfo
		'
		Me.pnlFileInfo.Location = New System.Drawing.Point(-4, 0)
		Me.pnlFileInfo.Name = "pnlFileInfo"
		Me.pnlFileInfo.Size = New System.Drawing.Size(844, 452)
		Me.pnlFileInfo.TabIndex = 0
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Location = New System.Drawing.Point(781, 553)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'OK_Button
		'
		Me.OK_Button.Location = New System.Drawing.Point(708, 553)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'dlgEditEpisode
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(854, 582)
		Me.Controls.Add(Me.TabControl1)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.pnlTop)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgEditEpisode"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Edit Episode"
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
		Me.TabPage4.ResumeLayout(False)
		Me.TabPage5.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents lblCredits As System.Windows.Forms.Label
    Friend WithEvents txtCredits As System.Windows.Forms.TextBox
    Friend WithEvents btnEditActor As System.Windows.Forms.Button
    Friend WithEvents btnAddActor As System.Windows.Forms.Button
    Friend WithEvents btnManual As System.Windows.Forms.Button
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents lblActors As System.Windows.Forms.Label
    Friend WithEvents lvActors As System.Windows.Forms.ListView
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colRole As System.Windows.Forms.ColumnHeader
    Friend WithEvents colThumb As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblDirector As System.Windows.Forms.Label
    Friend WithEvents txtDirector As System.Windows.Forms.TextBox
    Friend WithEvents lblPlot As System.Windows.Forms.Label
    Friend WithEvents txtPlot As System.Windows.Forms.TextBox
    Friend WithEvents pbStar5 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar4 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar3 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar2 As System.Windows.Forms.PictureBox
    Friend WithEvents pbStar1 As System.Windows.Forms.PictureBox
    Friend WithEvents lblRating As System.Windows.Forms.Label
    Friend WithEvents lblAired As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents txtTitle As System.Windows.Forms.TextBox
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents btnSetPosterDL As System.Windows.Forms.Button
    Friend WithEvents btnRemovePoster As System.Windows.Forms.Button
    Friend WithEvents lblPosterSize As System.Windows.Forms.Label
    Friend WithEvents btnSetPosterScrape As System.Windows.Forms.Button
    Friend WithEvents btnSetPoster As System.Windows.Forms.Button
    Friend WithEvents pbPoster As System.Windows.Forms.PictureBox
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents pnlFileInfo As System.Windows.Forms.Panel
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents txtEpisode As System.Windows.Forms.TextBox
    Friend WithEvents lblEpisode As System.Windows.Forms.Label
    Friend WithEvents txtSeason As System.Windows.Forms.TextBox
    Friend WithEvents lblSeason As System.Windows.Forms.Label
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents btnSetFanartDL As System.Windows.Forms.Button
    Friend WithEvents btnRemoveFanart As System.Windows.Forms.Button
    Friend WithEvents btnSetFanartScrape As System.Windows.Forms.Button
    Friend WithEvents btnSetFanart As System.Windows.Forms.Button
    Friend WithEvents pbFanart As System.Windows.Forms.PictureBox
    Friend WithEvents lblFanartSize As System.Windows.Forms.Label
    Friend WithEvents txtAired As System.Windows.Forms.TextBox
    Friend WithEvents btnActorDown As System.Windows.Forms.Button
    Friend WithEvents btnActorUp As System.Windows.Forms.Button
    Friend WithEvents pnlFrameExtrator As System.Windows.Forms.Panel
    Friend WithEvents ofdImage As System.Windows.Forms.OpenFileDialog

End Class
