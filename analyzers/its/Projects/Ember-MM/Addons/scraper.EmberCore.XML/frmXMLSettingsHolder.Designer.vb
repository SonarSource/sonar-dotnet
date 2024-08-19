<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmXMLSettingsHolder
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmXMLSettingsHolder))
		Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.gbOptions = New System.Windows.Forms.GroupBox()
		Me.chkCertification = New System.Windows.Forms.CheckBox()
		Me.chkCountry = New System.Windows.Forms.CheckBox()
		Me.chkTop250 = New System.Windows.Forms.CheckBox()
		Me.chkCrew = New System.Windows.Forms.CheckBox()
		Me.chkMusicBy = New System.Windows.Forms.CheckBox()
		Me.chkFullCrew = New System.Windows.Forms.CheckBox()
		Me.chkFullCast = New System.Windows.Forms.CheckBox()
		Me.chkProducers = New System.Windows.Forms.CheckBox()
		Me.chkWriters = New System.Windows.Forms.CheckBox()
		Me.chkStudio = New System.Windows.Forms.CheckBox()
		Me.chkRuntime = New System.Windows.Forms.CheckBox()
		Me.chkPlot = New System.Windows.Forms.CheckBox()
		Me.chkOutline = New System.Windows.Forms.CheckBox()
		Me.chkGenre = New System.Windows.Forms.CheckBox()
		Me.chkDirector = New System.Windows.Forms.CheckBox()
		Me.chkTagline = New System.Windows.Forms.CheckBox()
		Me.chkCast = New System.Windows.Forms.CheckBox()
		Me.chkVotes = New System.Windows.Forms.CheckBox()
		Me.chkTrailer = New System.Windows.Forms.CheckBox()
		Me.chkRating = New System.Windows.Forms.CheckBox()
		Me.chkRelease = New System.Windows.Forms.CheckBox()
		Me.chkMPAA = New System.Windows.Forms.CheckBox()
		Me.chkYear = New System.Windows.Forms.CheckBox()
		Me.chkTitle = New System.Windows.Forms.CheckBox()
		Me.pnlLoading = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.lblLanguage = New System.Windows.Forms.Label()
		Me.pbPoster = New System.Windows.Forms.PictureBox()
		Me.dgvSettings = New System.Windows.Forms.DataGridView()
		Me.Setting = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Value = New System.Windows.Forms.DataGridViewComboBoxColumn()
		Me.btnPopulate = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.cbScraper = New System.Windows.Forms.ComboBox()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.btnDown = New System.Windows.Forms.Button()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.btnUp = New System.Windows.Forms.Button()
		Me.pnlSettings.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.gbOptions.SuspendLayout()
		Me.pnlLoading.SuspendLayout()
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.dgvSettings, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.Label4)
		Me.pnlSettings.Controls.Add(Me.PictureBox1)
		Me.pnlSettings.Controls.Add(Me.gbOptions)
		Me.pnlSettings.Controls.Add(Me.pnlLoading)
		Me.pnlSettings.Controls.Add(Me.lblLanguage)
		Me.pnlSettings.Controls.Add(Me.pbPoster)
		Me.pnlSettings.Controls.Add(Me.dgvSettings)
		Me.pnlSettings.Controls.Add(Me.btnPopulate)
		Me.pnlSettings.Controls.Add(Me.Label1)
		Me.pnlSettings.Controls.Add(Me.cbScraper)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(12, 1)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 392)
		Me.pnlSettings.TabIndex = 0
		'
		'Label4
		'
		Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.ForeColor = System.Drawing.Color.Blue
		Me.Label4.Location = New System.Drawing.Point(37, 354)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(225, 31)
		Me.Label4.TabIndex = 8
		Me.Label4.Text = "These settings are specific to this module." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Please refer to the global settings " & _
		  "for more options."
		Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'PictureBox1
		'
		Me.PictureBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(3, 352)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(30, 31)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
		Me.PictureBox1.TabIndex = 98
		Me.PictureBox1.TabStop = False
		'
		'gbOptions
		'
		Me.gbOptions.Controls.Add(Me.chkCertification)
		Me.gbOptions.Controls.Add(Me.chkCountry)
		Me.gbOptions.Controls.Add(Me.chkTop250)
		Me.gbOptions.Controls.Add(Me.chkCrew)
		Me.gbOptions.Controls.Add(Me.chkMusicBy)
		Me.gbOptions.Controls.Add(Me.chkFullCrew)
		Me.gbOptions.Controls.Add(Me.chkFullCast)
		Me.gbOptions.Controls.Add(Me.chkProducers)
		Me.gbOptions.Controls.Add(Me.chkWriters)
		Me.gbOptions.Controls.Add(Me.chkStudio)
		Me.gbOptions.Controls.Add(Me.chkRuntime)
		Me.gbOptions.Controls.Add(Me.chkPlot)
		Me.gbOptions.Controls.Add(Me.chkOutline)
		Me.gbOptions.Controls.Add(Me.chkGenre)
		Me.gbOptions.Controls.Add(Me.chkDirector)
		Me.gbOptions.Controls.Add(Me.chkTagline)
		Me.gbOptions.Controls.Add(Me.chkCast)
		Me.gbOptions.Controls.Add(Me.chkVotes)
		Me.gbOptions.Controls.Add(Me.chkTrailer)
		Me.gbOptions.Controls.Add(Me.chkRating)
		Me.gbOptions.Controls.Add(Me.chkRelease)
		Me.gbOptions.Controls.Add(Me.chkMPAA)
		Me.gbOptions.Controls.Add(Me.chkYear)
		Me.gbOptions.Controls.Add(Me.chkTitle)
		Me.gbOptions.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.gbOptions.Location = New System.Drawing.Point(10, 240)
		Me.gbOptions.Name = "gbOptions"
		Me.gbOptions.Size = New System.Drawing.Size(591, 111)
		Me.gbOptions.TabIndex = 7
		Me.gbOptions.TabStop = False
		Me.gbOptions.Text = "Scraper Fields"
		'
		'chkCertification
		'
		Me.chkCertification.AutoSize = True
		Me.chkCertification.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkCertification.Location = New System.Drawing.Point(6, 70)
		Me.chkCertification.Name = "chkCertification"
		Me.chkCertification.Size = New System.Drawing.Size(89, 17)
		Me.chkCertification.TabIndex = 2
		Me.chkCertification.Text = "Certification"
		Me.chkCertification.UseVisualStyleBackColor = True
		'
		'chkCountry
		'
		Me.chkCountry.AutoSize = True
		Me.chkCountry.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkCountry.Location = New System.Drawing.Point(329, 87)
		Me.chkCountry.Name = "chkCountry"
		Me.chkCountry.Size = New System.Drawing.Size(67, 17)
		Me.chkCountry.TabIndex = 18
		Me.chkCountry.Text = "Country"
		Me.chkCountry.UseVisualStyleBackColor = True
		'
		'chkTop250
		'
		Me.chkTop250.AutoSize = True
		Me.chkTop250.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTop250.Location = New System.Drawing.Point(329, 36)
		Me.chkTop250.Name = "chkTop250"
		Me.chkTop250.Size = New System.Drawing.Size(66, 17)
		Me.chkTop250.TabIndex = 15
		Me.chkTop250.Text = "Top 250"
		Me.chkTop250.UseVisualStyleBackColor = True
		'
		'chkCrew
		'
		Me.chkCrew.AutoSize = True
		Me.chkCrew.Enabled = False
		Me.chkCrew.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkCrew.Location = New System.Drawing.Point(476, 36)
		Me.chkCrew.Name = "chkCrew"
		Me.chkCrew.Size = New System.Drawing.Size(85, 17)
		Me.chkCrew.TabIndex = 20
		Me.chkCrew.Text = "Other Crew"
		Me.chkCrew.UseVisualStyleBackColor = True
		'
		'chkMusicBy
		'
		Me.chkMusicBy.AutoSize = True
		Me.chkMusicBy.Enabled = False
		Me.chkMusicBy.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkMusicBy.Location = New System.Drawing.Point(476, 53)
		Me.chkMusicBy.Name = "chkMusicBy"
		Me.chkMusicBy.Size = New System.Drawing.Size(71, 17)
		Me.chkMusicBy.TabIndex = 21
		Me.chkMusicBy.Text = "Music By"
		Me.chkMusicBy.UseVisualStyleBackColor = True
		'
		'chkFullCrew
		'
		Me.chkFullCrew.AutoSize = True
		Me.chkFullCrew.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkFullCrew.Location = New System.Drawing.Point(468, 19)
		Me.chkFullCrew.Name = "chkFullCrew"
		Me.chkFullCrew.Size = New System.Drawing.Size(111, 17)
		Me.chkFullCrew.TabIndex = 19
		Me.chkFullCrew.Text = "Scrape Full Crew"
		Me.chkFullCrew.UseVisualStyleBackColor = True
		'
		'chkFullCast
		'
		Me.chkFullCast.AutoSize = True
		Me.chkFullCast.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkFullCast.Location = New System.Drawing.Point(329, 53)
		Me.chkFullCast.Name = "chkFullCast"
		Me.chkFullCast.Size = New System.Drawing.Size(107, 17)
		Me.chkFullCast.TabIndex = 16
		Me.chkFullCast.Text = "Scrape Full Cast"
		Me.chkFullCast.UseVisualStyleBackColor = True
		'
		'chkProducers
		'
		Me.chkProducers.AutoSize = True
		Me.chkProducers.Enabled = False
		Me.chkProducers.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkProducers.Location = New System.Drawing.Point(476, 70)
		Me.chkProducers.Name = "chkProducers"
		Me.chkProducers.Size = New System.Drawing.Size(77, 17)
		Me.chkProducers.TabIndex = 22
		Me.chkProducers.Text = "Producers"
		Me.chkProducers.UseVisualStyleBackColor = True
		'
		'chkWriters
		'
		Me.chkWriters.AutoSize = True
		Me.chkWriters.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkWriters.Location = New System.Drawing.Point(227, 87)
		Me.chkWriters.Name = "chkWriters"
		Me.chkWriters.Size = New System.Drawing.Size(63, 17)
		Me.chkWriters.TabIndex = 13
		Me.chkWriters.Text = "Writers"
		Me.chkWriters.UseVisualStyleBackColor = True
		'
		'chkStudio
		'
		Me.chkStudio.AutoSize = True
		Me.chkStudio.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkStudio.Location = New System.Drawing.Point(125, 70)
		Me.chkStudio.Name = "chkStudio"
		Me.chkStudio.Size = New System.Drawing.Size(60, 17)
		Me.chkStudio.TabIndex = 7
		Me.chkStudio.Text = "Studio"
		Me.chkStudio.UseVisualStyleBackColor = True
		'
		'chkRuntime
		'
		Me.chkRuntime.AutoSize = True
		Me.chkRuntime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRuntime.Location = New System.Drawing.Point(125, 19)
		Me.chkRuntime.Name = "chkRuntime"
		Me.chkRuntime.Size = New System.Drawing.Size(69, 17)
		Me.chkRuntime.TabIndex = 4
		Me.chkRuntime.Text = "Runtime"
		Me.chkRuntime.UseVisualStyleBackColor = True
		'
		'chkPlot
		'
		Me.chkPlot.AutoSize = True
		Me.chkPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkPlot.Location = New System.Drawing.Point(227, 36)
		Me.chkPlot.Name = "chkPlot"
		Me.chkPlot.Size = New System.Drawing.Size(46, 17)
		Me.chkPlot.TabIndex = 10
		Me.chkPlot.Text = "Plot"
		Me.chkPlot.UseVisualStyleBackColor = True
		'
		'chkOutline
		'
		Me.chkOutline.AutoSize = True
		Me.chkOutline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkOutline.Location = New System.Drawing.Point(227, 19)
		Me.chkOutline.Name = "chkOutline"
		Me.chkOutline.Size = New System.Drawing.Size(65, 17)
		Me.chkOutline.TabIndex = 9
		Me.chkOutline.Text = "Outline"
		Me.chkOutline.UseVisualStyleBackColor = True
		'
		'chkGenre
		'
		Me.chkGenre.AutoSize = True
		Me.chkGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkGenre.Location = New System.Drawing.Point(329, 19)
		Me.chkGenre.Name = "chkGenre"
		Me.chkGenre.Size = New System.Drawing.Size(57, 17)
		Me.chkGenre.TabIndex = 14
		Me.chkGenre.Text = "Genre"
		Me.chkGenre.UseVisualStyleBackColor = True
		'
		'chkDirector
		'
		Me.chkDirector.AutoSize = True
		Me.chkDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkDirector.Location = New System.Drawing.Point(227, 70)
		Me.chkDirector.Name = "chkDirector"
		Me.chkDirector.Size = New System.Drawing.Size(67, 17)
		Me.chkDirector.TabIndex = 12
		Me.chkDirector.Text = "Director"
		Me.chkDirector.UseVisualStyleBackColor = True
		'
		'chkTagline
		'
		Me.chkTagline.AutoSize = True
		Me.chkTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTagline.Location = New System.Drawing.Point(125, 87)
		Me.chkTagline.Name = "chkTagline"
		Me.chkTagline.Size = New System.Drawing.Size(63, 17)
		Me.chkTagline.TabIndex = 8
		Me.chkTagline.Text = "Tagline"
		Me.chkTagline.UseVisualStyleBackColor = True
		'
		'chkCast
		'
		Me.chkCast.AutoSize = True
		Me.chkCast.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkCast.Location = New System.Drawing.Point(227, 53)
		Me.chkCast.Name = "chkCast"
		Me.chkCast.Size = New System.Drawing.Size(48, 17)
		Me.chkCast.TabIndex = 11
		Me.chkCast.Text = "Cast"
		Me.chkCast.UseVisualStyleBackColor = True
		'
		'chkVotes
		'
		Me.chkVotes.AutoSize = True
		Me.chkVotes.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkVotes.Location = New System.Drawing.Point(125, 53)
		Me.chkVotes.Name = "chkVotes"
		Me.chkVotes.Size = New System.Drawing.Size(55, 17)
		Me.chkVotes.TabIndex = 6
		Me.chkVotes.Text = "Votes"
		Me.chkVotes.UseVisualStyleBackColor = True
		'
		'chkTrailer
		'
		Me.chkTrailer.AutoSize = True
		Me.chkTrailer.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTrailer.Location = New System.Drawing.Point(329, 70)
		Me.chkTrailer.Name = "chkTrailer"
		Me.chkTrailer.Size = New System.Drawing.Size(57, 17)
		Me.chkTrailer.TabIndex = 17
		Me.chkTrailer.Text = "Trailer"
		Me.chkTrailer.UseVisualStyleBackColor = True
		'
		'chkRating
		'
		Me.chkRating.AutoSize = True
		Me.chkRating.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRating.Location = New System.Drawing.Point(125, 36)
		Me.chkRating.Name = "chkRating"
		Me.chkRating.Size = New System.Drawing.Size(60, 17)
		Me.chkRating.TabIndex = 5
		Me.chkRating.Text = "Rating"
		Me.chkRating.UseVisualStyleBackColor = True
		'
		'chkRelease
		'
		Me.chkRelease.AutoSize = True
		Me.chkRelease.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRelease.Location = New System.Drawing.Point(6, 87)
		Me.chkRelease.Name = "chkRelease"
		Me.chkRelease.Size = New System.Drawing.Size(92, 17)
		Me.chkRelease.TabIndex = 3
		Me.chkRelease.Text = "Release Date"
		Me.chkRelease.UseVisualStyleBackColor = True
		'
		'chkMPAA
		'
		Me.chkMPAA.AutoSize = True
		Me.chkMPAA.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkMPAA.Location = New System.Drawing.Point(6, 53)
		Me.chkMPAA.Name = "chkMPAA"
		Me.chkMPAA.Size = New System.Drawing.Size(56, 17)
		Me.chkMPAA.TabIndex = 1
		Me.chkMPAA.Text = "MPAA"
		Me.chkMPAA.UseVisualStyleBackColor = True
		'
		'chkYear
		'
		Me.chkYear.AutoSize = True
		Me.chkYear.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkYear.Location = New System.Drawing.Point(6, 36)
		Me.chkYear.Name = "chkYear"
		Me.chkYear.Size = New System.Drawing.Size(47, 17)
		Me.chkYear.TabIndex = 1
		Me.chkYear.Text = "Year"
		Me.chkYear.UseVisualStyleBackColor = True
		'
		'chkTitle
		'
		Me.chkTitle.AutoSize = True
		Me.chkTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTitle.Location = New System.Drawing.Point(6, 19)
		Me.chkTitle.Name = "chkTitle"
		Me.chkTitle.Size = New System.Drawing.Size(47, 17)
		Me.chkTitle.TabIndex = 0
		Me.chkTitle.Text = "Title"
		Me.chkTitle.UseVisualStyleBackColor = True
		'
		'pnlLoading
		'
		Me.pnlLoading.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlLoading.Controls.Add(Me.Label3)
		Me.pnlLoading.Location = New System.Drawing.Point(104, 105)
		Me.pnlLoading.Name = "pnlLoading"
		Me.pnlLoading.Size = New System.Drawing.Size(200, 40)
		Me.pnlLoading.TabIndex = 5
		Me.pnlLoading.Visible = False
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label3.ForeColor = System.Drawing.Color.DarkRed
		Me.Label3.Location = New System.Drawing.Point(40, 12)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(117, 15)
		Me.Label3.TabIndex = 0
		Me.Label3.Text = "Loading Please Wait"
		'
		'lblLanguage
		'
		Me.lblLanguage.Location = New System.Drawing.Point(433, 84)
		Me.lblLanguage.Name = "lblLanguage"
		Me.lblLanguage.Size = New System.Drawing.Size(177, 15)
		Me.lblLanguage.TabIndex = 6
		Me.lblLanguage.Text = "Language"
		Me.lblLanguage.TextAlign = System.Drawing.ContentAlignment.TopCenter
		'
		'pbPoster
		'
		Me.pbPoster.Location = New System.Drawing.Point(432, 31)
		Me.pbPoster.Name = "pbPoster"
		Me.pbPoster.Size = New System.Drawing.Size(178, 50)
		Me.pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbPoster.TabIndex = 89
		Me.pbPoster.TabStop = False
		'
		'dgvSettings
		'
		Me.dgvSettings.AllowUserToAddRows = False
		Me.dgvSettings.AllowUserToDeleteRows = False
		Me.dgvSettings.AllowUserToResizeColumns = False
		Me.dgvSettings.AllowUserToResizeRows = False
		Me.dgvSettings.BackgroundColor = System.Drawing.Color.White
		Me.dgvSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvSettings.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Setting, Me.Value})
		Me.dgvSettings.Location = New System.Drawing.Point(10, 68)
		Me.dgvSettings.MultiSelect = False
		Me.dgvSettings.Name = "dgvSettings"
		Me.dgvSettings.RowHeadersVisible = False
		Me.dgvSettings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvSettings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
		Me.dgvSettings.ShowCellErrors = False
		Me.dgvSettings.ShowCellToolTips = False
		Me.dgvSettings.ShowRowErrors = False
		Me.dgvSettings.Size = New System.Drawing.Size(402, 166)
		Me.dgvSettings.TabIndex = 4
		'
		'Setting
		'
		Me.Setting.FillWeight = 280.0!
		Me.Setting.HeaderText = "Setting"
		Me.Setting.Name = "Setting"
		Me.Setting.ReadOnly = True
		Me.Setting.Width = 280
		'
		'Value
		'
		DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		Me.Value.DefaultCellStyle = DataGridViewCellStyle1
		Me.Value.HeaderText = "Value"
		Me.Value.Name = "Value"
		'
		'btnPopulate
		'
		Me.btnPopulate.Location = New System.Drawing.Point(261, 39)
		Me.btnPopulate.Name = "btnPopulate"
		Me.btnPopulate.Size = New System.Drawing.Size(129, 23)
		Me.btnPopulate.TabIndex = 3
		Me.btnPopulate.Text = "Populate Scrapers"
		Me.btnPopulate.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(7, 44)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(45, 13)
		Me.Label1.TabIndex = 1
		Me.Label1.Text = "Scraper"
		'
		'cbScraper
		'
		Me.cbScraper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbScraper.FormattingEnabled = True
		Me.cbScraper.Location = New System.Drawing.Point(58, 41)
		Me.cbScraper.Name = "cbScraper"
		Me.cbScraper.Size = New System.Drawing.Size(183, 21)
		Me.cbScraper.TabIndex = 2
		'
		'Panel1
		'
		Me.Panel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
				  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.Label2)
		Me.Panel1.Controls.Add(Me.btnDown)
		Me.Panel1.Controls.Add(Me.cbEnabled)
		Me.Panel1.Controls.Add(Me.btnUp)
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(1125, 25)
		Me.Panel1.TabIndex = 0
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label2.Location = New System.Drawing.Point(500, 7)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(58, 12)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Scraper order"
		'
		'btnDown
		'
		Me.btnDown.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.btnDown.Image = CType(resources.GetObject("btnDown.Image"), System.Drawing.Image)
		Me.btnDown.Location = New System.Drawing.Point(591, 1)
		Me.btnDown.Name = "btnDown"
		Me.btnDown.Size = New System.Drawing.Size(23, 23)
		Me.btnDown.TabIndex = 3
		Me.btnDown.UseVisualStyleBackColor = True
		'
		'cbEnabled
		'
		Me.cbEnabled.AutoSize = True
		Me.cbEnabled.Location = New System.Drawing.Point(10, 5)
		Me.cbEnabled.Name = "cbEnabled"
		Me.cbEnabled.Size = New System.Drawing.Size(68, 17)
		Me.cbEnabled.TabIndex = 0
		Me.cbEnabled.Text = "Enabled"
		Me.cbEnabled.UseVisualStyleBackColor = True
		'
		'btnUp
		'
		Me.btnUp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.btnUp.Image = CType(resources.GetObject("btnUp.Image"), System.Drawing.Image)
		Me.btnUp.Location = New System.Drawing.Point(566, 1)
		Me.btnUp.Name = "btnUp"
		Me.btnUp.Size = New System.Drawing.Size(23, 23)
		Me.btnUp.TabIndex = 2
		Me.btnUp.UseVisualStyleBackColor = True
		'
		'frmXMLSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackColor = System.Drawing.Color.White
		Me.ClientSize = New System.Drawing.Size(652, 405)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmXMLSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Setup"
		Me.pnlSettings.ResumeLayout(False)
		Me.pnlSettings.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.gbOptions.ResumeLayout(False)
		Me.gbOptions.PerformLayout()
		Me.pnlLoading.ResumeLayout(False)
		Me.pnlLoading.PerformLayout()
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.dgvSettings, System.ComponentModel.ISupportInitialize).EndInit()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnDown As System.Windows.Forms.Button
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents btnUp As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cbScraper As System.Windows.Forms.ComboBox
    Friend WithEvents btnPopulate As System.Windows.Forms.Button
    Friend WithEvents pnlLoading As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents dgvSettings As System.Windows.Forms.DataGridView
    Friend WithEvents pbPoster As System.Windows.Forms.PictureBox
    Friend WithEvents Setting As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Value As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents lblLanguage As System.Windows.Forms.Label
    Friend WithEvents gbOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkCertification As System.Windows.Forms.CheckBox
    Friend WithEvents chkCountry As System.Windows.Forms.CheckBox
    Friend WithEvents chkTop250 As System.Windows.Forms.CheckBox
    Friend WithEvents chkCrew As System.Windows.Forms.CheckBox
    Friend WithEvents chkMusicBy As System.Windows.Forms.CheckBox
    Friend WithEvents chkFullCrew As System.Windows.Forms.CheckBox
    Friend WithEvents chkFullCast As System.Windows.Forms.CheckBox
    Friend WithEvents chkProducers As System.Windows.Forms.CheckBox
    Friend WithEvents chkWriters As System.Windows.Forms.CheckBox
    Friend WithEvents chkStudio As System.Windows.Forms.CheckBox
    Friend WithEvents chkRuntime As System.Windows.Forms.CheckBox
    Friend WithEvents chkPlot As System.Windows.Forms.CheckBox
    Friend WithEvents chkOutline As System.Windows.Forms.CheckBox
    Friend WithEvents chkGenre As System.Windows.Forms.CheckBox
    Friend WithEvents chkDirector As System.Windows.Forms.CheckBox
    Friend WithEvents chkTagline As System.Windows.Forms.CheckBox
    Friend WithEvents chkCast As System.Windows.Forms.CheckBox
    Friend WithEvents chkVotes As System.Windows.Forms.CheckBox
    Friend WithEvents chkTrailer As System.Windows.Forms.CheckBox
    Friend WithEvents chkRating As System.Windows.Forms.CheckBox
    Friend WithEvents chkRelease As System.Windows.Forms.CheckBox
    Friend WithEvents chkMPAA As System.Windows.Forms.CheckBox
    Friend WithEvents chkYear As System.Windows.Forms.CheckBox
    Friend WithEvents chkTitle As System.Windows.Forms.CheckBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox

End Class
