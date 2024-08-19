<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMediaSettingsHolder
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMediaSettingsHolder))
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.GroupBox3 = New System.Windows.Forms.GroupBox()
		Me.GroupBox4 = New System.Windows.Forms.GroupBox()
		Me.cbManualETSize = New System.Windows.Forms.ComboBox()
		Me.grpSaveFanart = New System.Windows.Forms.GroupBox()
		Me.optFanartFolderExtraFanart = New System.Windows.Forms.RadioButton()
		Me.optFanartFolderExtraThumbs = New System.Windows.Forms.RadioButton()
		Me.GroupBox9 = New System.Windows.Forms.GroupBox()
		Me.chkUseMPDB = New System.Windows.Forms.CheckBox()
		Me.chkUseTMDB = New System.Windows.Forms.CheckBox()
		Me.chkUseIMPA = New System.Windows.Forms.CheckBox()
		Me.chkScrapePoster = New System.Windows.Forms.CheckBox()
		Me.chkScrapeFanart = New System.Windows.Forms.CheckBox()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.GroupBox5 = New System.Windows.Forms.GroupBox()
		Me.cbTrailerTMDBPref = New System.Windows.Forms.ComboBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.chkDownloadTrailer = New System.Windows.Forms.CheckBox()
		Me.Label23 = New System.Windows.Forms.Label()
		Me.txtTimeout = New System.Windows.Forms.TextBox()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.chkTrailerTMDBXBMC = New System.Windows.Forms.CheckBox()
		Me.chkTrailerIMDB = New System.Windows.Forms.CheckBox()
		Me.chkTrailerTMDB = New System.Windows.Forms.CheckBox()
		Me.Panel2 = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.btnDown = New System.Windows.Forms.Button()
		Me.btnUp = New System.Windows.Forms.Button()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox3.SuspendLayout()
		Me.GroupBox4.SuspendLayout()
		Me.grpSaveFanart.SuspendLayout()
		Me.GroupBox9.SuspendLayout()
		Me.GroupBox1.SuspendLayout()
		Me.GroupBox5.SuspendLayout()
		Me.GroupBox2.SuspendLayout()
		Me.Panel2.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.Label1)
		Me.pnlSettings.Controls.Add(Me.PictureBox1)
		Me.pnlSettings.Controls.Add(Me.GroupBox3)
		Me.pnlSettings.Controls.Add(Me.GroupBox1)
		Me.pnlSettings.Controls.Add(Me.Panel2)
		Me.pnlSettings.Location = New System.Drawing.Point(12, 4)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 369)
		Me.pnlSettings.TabIndex = 0
		'
		'Label1
		'
		Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.ForeColor = System.Drawing.Color.Blue
		Me.Label1.Location = New System.Drawing.Point(37, 337)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(225, 31)
		Me.Label1.TabIndex = 3
		Me.Label1.Text = "These settings are specific to this module." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Please refer to the global settings " & _
		  "for more options."
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'PictureBox1
		'
		Me.PictureBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(3, 335)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(30, 31)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
		Me.PictureBox1.TabIndex = 94
		Me.PictureBox1.TabStop = False
		'
		'GroupBox3
		'
		Me.GroupBox3.Controls.Add(Me.GroupBox4)
		Me.GroupBox3.Controls.Add(Me.grpSaveFanart)
		Me.GroupBox3.Controls.Add(Me.GroupBox9)
		Me.GroupBox3.Controls.Add(Me.chkScrapePoster)
		Me.GroupBox3.Controls.Add(Me.chkScrapeFanart)
		Me.GroupBox3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox3.Location = New System.Drawing.Point(15, 31)
		Me.GroupBox3.Name = "GroupBox3"
		Me.GroupBox3.Size = New System.Drawing.Size(587, 131)
		Me.GroupBox3.TabIndex = 1
		Me.GroupBox3.TabStop = False
		Me.GroupBox3.Text = "Images"
		'
		'GroupBox4
		'
		Me.GroupBox4.Controls.Add(Me.cbManualETSize)
		Me.GroupBox4.Location = New System.Drawing.Point(374, 11)
		Me.GroupBox4.Name = "GroupBox4"
		Me.GroupBox4.Size = New System.Drawing.Size(160, 80)
		Me.GroupBox4.TabIndex = 4
		Me.GroupBox4.TabStop = False
		Me.GroupBox4.Text = "TMDB Extrathumbs Size:"
		'
		'cbManualETSize
		'
		Me.cbManualETSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbManualETSize.Enabled = False
		Me.cbManualETSize.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.cbManualETSize.FormattingEnabled = True
		Me.cbManualETSize.Items.AddRange(New Object() {"original", "w1280", "poster", "thumb"})
		Me.cbManualETSize.Location = New System.Drawing.Point(21, 35)
		Me.cbManualETSize.Name = "cbManualETSize"
		Me.cbManualETSize.Size = New System.Drawing.Size(121, 21)
		Me.cbManualETSize.TabIndex = 0
		'
		'grpSaveFanart
		'
		Me.grpSaveFanart.Controls.Add(Me.optFanartFolderExtraFanart)
		Me.grpSaveFanart.Controls.Add(Me.optFanartFolderExtraThumbs)
		Me.grpSaveFanart.Enabled = False
		Me.grpSaveFanart.Location = New System.Drawing.Point(24, 54)
		Me.grpSaveFanart.Name = "grpSaveFanart"
		Me.grpSaveFanart.Size = New System.Drawing.Size(123, 66)
		Me.grpSaveFanart.TabIndex = 2
		Me.grpSaveFanart.TabStop = False
		Me.grpSaveFanart.Text = "Save Fanart In:"
		'
		'optFanartFolderExtraFanart
		'
		Me.optFanartFolderExtraFanart.AutoSize = True
		Me.optFanartFolderExtraFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.optFanartFolderExtraFanart.Location = New System.Drawing.Point(22, 38)
		Me.optFanartFolderExtraFanart.Name = "optFanartFolderExtraFanart"
		Me.optFanartFolderExtraFanart.Size = New System.Drawing.Size(85, 17)
		Me.optFanartFolderExtraFanart.TabIndex = 1
		Me.optFanartFolderExtraFanart.TabStop = True
		Me.optFanartFolderExtraFanart.Text = "\extrafanart"
		Me.optFanartFolderExtraFanart.UseVisualStyleBackColor = True
		'
		'optFanartFolderExtraThumbs
		'
		Me.optFanartFolderExtraThumbs.AutoSize = True
		Me.optFanartFolderExtraThumbs.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.optFanartFolderExtraThumbs.Location = New System.Drawing.Point(22, 19)
		Me.optFanartFolderExtraThumbs.Name = "optFanartFolderExtraThumbs"
		Me.optFanartFolderExtraThumbs.Size = New System.Drawing.Size(93, 17)
		Me.optFanartFolderExtraThumbs.TabIndex = 0
		Me.optFanartFolderExtraThumbs.TabStop = True
		Me.optFanartFolderExtraThumbs.Text = "\extrathumbs"
		Me.optFanartFolderExtraThumbs.UseVisualStyleBackColor = True
		'
		'GroupBox9
		'
		Me.GroupBox9.Controls.Add(Me.chkUseMPDB)
		Me.GroupBox9.Controls.Add(Me.chkUseTMDB)
		Me.GroupBox9.Controls.Add(Me.chkUseIMPA)
		Me.GroupBox9.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox9.Location = New System.Drawing.Point(165, 11)
		Me.GroupBox9.Name = "GroupBox9"
		Me.GroupBox9.Size = New System.Drawing.Size(160, 80)
		Me.GroupBox9.TabIndex = 3
		Me.GroupBox9.TabStop = False
		Me.GroupBox9.Text = "Get Images From:"
		'
		'chkUseMPDB
		'
		Me.chkUseMPDB.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseMPDB.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkUseMPDB.Location = New System.Drawing.Point(6, 56)
		Me.chkUseMPDB.Name = "chkUseMPDB"
		Me.chkUseMPDB.Size = New System.Drawing.Size(150, 22)
		Me.chkUseMPDB.TabIndex = 2
		Me.chkUseMPDB.Text = "MoviePosterDB.com"
		Me.chkUseMPDB.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseMPDB.UseVisualStyleBackColor = True
		'
		'chkUseTMDB
		'
		Me.chkUseTMDB.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseTMDB.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkUseTMDB.Location = New System.Drawing.Point(6, 18)
		Me.chkUseTMDB.Name = "chkUseTMDB"
		Me.chkUseTMDB.Size = New System.Drawing.Size(149, 19)
		Me.chkUseTMDB.TabIndex = 0
		Me.chkUseTMDB.Text = "themoviedb.org"
		Me.chkUseTMDB.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseTMDB.UseVisualStyleBackColor = True
		'
		'chkUseIMPA
		'
		Me.chkUseIMPA.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseIMPA.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkUseIMPA.Location = New System.Drawing.Point(6, 37)
		Me.chkUseIMPA.Name = "chkUseIMPA"
		Me.chkUseIMPA.Size = New System.Drawing.Size(149, 20)
		Me.chkUseIMPA.TabIndex = 1
		Me.chkUseIMPA.Text = "IMPAwards.com"
		Me.chkUseIMPA.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseIMPA.UseVisualStyleBackColor = True
		'
		'chkScrapePoster
		'
		Me.chkScrapePoster.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkScrapePoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkScrapePoster.Location = New System.Drawing.Point(6, 19)
		Me.chkScrapePoster.Name = "chkScrapePoster"
		Me.chkScrapePoster.Size = New System.Drawing.Size(114, 15)
		Me.chkScrapePoster.TabIndex = 0
		Me.chkScrapePoster.Text = "Get Posters"
		Me.chkScrapePoster.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkScrapePoster.UseVisualStyleBackColor = True
		'
		'chkScrapeFanart
		'
		Me.chkScrapeFanart.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkScrapeFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkScrapeFanart.Location = New System.Drawing.Point(6, 37)
		Me.chkScrapeFanart.Name = "chkScrapeFanart"
		Me.chkScrapeFanart.Size = New System.Drawing.Size(84, 16)
		Me.chkScrapeFanart.TabIndex = 1
		Me.chkScrapeFanart.Text = "Get Fanart"
		Me.chkScrapeFanart.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkScrapeFanart.UseVisualStyleBackColor = True
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.GroupBox5)
		Me.GroupBox1.Controls.Add(Me.chkDownloadTrailer)
		Me.GroupBox1.Controls.Add(Me.Label23)
		Me.GroupBox1.Controls.Add(Me.txtTimeout)
		Me.GroupBox1.Controls.Add(Me.GroupBox2)
		Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(15, 168)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(587, 112)
		Me.GroupBox1.TabIndex = 2
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Trailers"
		Me.GroupBox1.UseCompatibleTextRendering = True
		'
		'GroupBox5
		'
		Me.GroupBox5.Controls.Add(Me.cbTrailerTMDBPref)
		Me.GroupBox5.Controls.Add(Me.Label2)
		Me.GroupBox5.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold)
		Me.GroupBox5.Location = New System.Drawing.Point(374, 12)
		Me.GroupBox5.Name = "GroupBox5"
		Me.GroupBox5.Size = New System.Drawing.Size(161, 94)
		Me.GroupBox5.TabIndex = 4
		Me.GroupBox5.TabStop = False
		Me.GroupBox5.Text = "Youtube/TMDB Trailer:"
		'
		'cbTrailerTMDBPref
		'
		Me.cbTrailerTMDBPref.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbTrailerTMDBPref.Enabled = False
		Me.cbTrailerTMDBPref.Font = New System.Drawing.Font("Segoe UI", 8.25!)
		Me.cbTrailerTMDBPref.FormattingEnabled = True
		Me.cbTrailerTMDBPref.Items.AddRange(New Object() {"bg", "cs", "da", "de", "el", "en", "es", "fi", "fr", "he", "hu", "it", "nb", "nl", "no", "pl", "pt", "ru", "sk", "sv", "ta", "tr", "uk", "vi", "xx", "zh"})
		Me.cbTrailerTMDBPref.Location = New System.Drawing.Point(21, 51)
		Me.cbTrailerTMDBPref.Name = "cbTrailerTMDBPref"
		Me.cbTrailerTMDBPref.Size = New System.Drawing.Size(121, 21)
		Me.cbTrailerTMDBPref.TabIndex = 1
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!)
		Me.Label2.Location = New System.Drawing.Point(26, 26)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(111, 13)
		Me.Label2.TabIndex = 0
		Me.Label2.Text = "Preferred Language:"
		'
		'chkDownloadTrailer
		'
		Me.chkDownloadTrailer.AutoSize = True
		Me.chkDownloadTrailer.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkDownloadTrailer.Location = New System.Drawing.Point(6, 19)
		Me.chkDownloadTrailer.Name = "chkDownloadTrailer"
		Me.chkDownloadTrailer.Size = New System.Drawing.Size(140, 17)
		Me.chkDownloadTrailer.TabIndex = 0
		Me.chkDownloadTrailer.Text = "Enable Trailer Support"
		Me.chkDownloadTrailer.UseVisualStyleBackColor = True
		'
		'Label23
		'
		Me.Label23.AutoSize = True
		Me.Label23.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label23.Location = New System.Drawing.Point(21, 43)
		Me.Label23.Name = "Label23"
		Me.Label23.Size = New System.Drawing.Size(51, 13)
		Me.Label23.TabIndex = 1
		Me.Label23.Text = "Timeout:"
		'
		'txtTimeout
		'
		Me.txtTimeout.Enabled = False
		Me.txtTimeout.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtTimeout.Location = New System.Drawing.Point(82, 39)
		Me.txtTimeout.Name = "txtTimeout"
		Me.txtTimeout.Size = New System.Drawing.Size(50, 22)
		Me.txtTimeout.TabIndex = 2
		'
		'GroupBox2
		'
		Me.GroupBox2.Controls.Add(Me.chkTrailerTMDBXBMC)
		Me.GroupBox2.Controls.Add(Me.chkTrailerIMDB)
		Me.GroupBox2.Controls.Add(Me.chkTrailerTMDB)
		Me.GroupBox2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox2.Location = New System.Drawing.Point(165, 12)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(161, 94)
		Me.GroupBox2.TabIndex = 3
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Supported Sites:"
		'
		'chkTrailerTMDBXBMC
		'
		Me.chkTrailerTMDBXBMC.AutoSize = True
		Me.chkTrailerTMDBXBMC.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTrailerTMDBXBMC.Location = New System.Drawing.Point(26, 44)
		Me.chkTrailerTMDBXBMC.Name = "chkTrailerTMDBXBMC"
		Me.chkTrailerTMDBXBMC.Size = New System.Drawing.Size(95, 17)
		Me.chkTrailerTMDBXBMC.TabIndex = 1
		Me.chkTrailerTMDBXBMC.Text = "XBMC Format"
		Me.chkTrailerTMDBXBMC.UseVisualStyleBackColor = True
		'
		'chkTrailerIMDB
		'
		Me.chkTrailerIMDB.AutoSize = True
		Me.chkTrailerIMDB.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTrailerIMDB.Location = New System.Drawing.Point(6, 67)
		Me.chkTrailerIMDB.Name = "chkTrailerIMDB"
		Me.chkTrailerIMDB.Size = New System.Drawing.Size(54, 17)
		Me.chkTrailerIMDB.TabIndex = 2
		Me.chkTrailerIMDB.Text = "IMDB"
		Me.chkTrailerIMDB.UseVisualStyleBackColor = True
		'
		'chkTrailerTMDB
		'
		Me.chkTrailerTMDB.AutoSize = True
		Me.chkTrailerTMDB.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkTrailerTMDB.Location = New System.Drawing.Point(6, 21)
		Me.chkTrailerTMDB.Name = "chkTrailerTMDB"
		Me.chkTrailerTMDB.Size = New System.Drawing.Size(103, 17)
		Me.chkTrailerTMDB.TabIndex = 0
		Me.chkTrailerTMDB.Text = "Youtube/TMDB"
		Me.chkTrailerTMDB.UseVisualStyleBackColor = True
		'
		'Panel2
		'
		Me.Panel2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
				  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Panel2.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel2.Controls.Add(Me.Label3)
		Me.Panel2.Controls.Add(Me.btnDown)
		Me.Panel2.Controls.Add(Me.btnUp)
		Me.Panel2.Controls.Add(Me.cbEnabled)
		Me.Panel2.Location = New System.Drawing.Point(0, 0)
		Me.Panel2.Name = "Panel2"
		Me.Panel2.Size = New System.Drawing.Size(1125, 25)
		Me.Panel2.TabIndex = 0
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label3.Location = New System.Drawing.Point(500, 7)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(58, 12)
		Me.Label3.TabIndex = 1
		Me.Label3.Text = "Scraper order"
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
		'frmMediaSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackColor = System.Drawing.Color.White
		Me.ClientSize = New System.Drawing.Size(652, 388)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmMediaSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Scraper Setup"
		Me.pnlSettings.ResumeLayout(False)
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox3.ResumeLayout(False)
		Me.GroupBox4.ResumeLayout(False)
		Me.grpSaveFanart.ResumeLayout(False)
		Me.grpSaveFanart.PerformLayout()
		Me.GroupBox9.ResumeLayout(False)
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.GroupBox5.ResumeLayout(False)
		Me.GroupBox5.PerformLayout()
		Me.GroupBox2.ResumeLayout(False)
		Me.GroupBox2.PerformLayout()
		Me.Panel2.ResumeLayout(False)
		Me.Panel2.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents chkScrapeFanart As System.Windows.Forms.CheckBox
    Friend WithEvents chkScrapePoster As System.Windows.Forms.CheckBox
    Friend WithEvents chkDownloadTrailer As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox9 As System.Windows.Forms.GroupBox
    Friend WithEvents chkUseMPDB As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseTMDB As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseIMPA As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents Label23 As System.Windows.Forms.Label
    Friend WithEvents txtTimeout As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnDown As System.Windows.Forms.Button
    Friend WithEvents btnUp As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents grpSaveFanart As System.Windows.Forms.GroupBox
    Friend WithEvents optFanartFolderExtraFanart As System.Windows.Forms.RadioButton
    Friend WithEvents optFanartFolderExtraThumbs As System.Windows.Forms.RadioButton
    Friend WithEvents chkTrailerIMDB As System.Windows.Forms.CheckBox
    Friend WithEvents chkTrailerTMDB As System.Windows.Forms.CheckBox
    Friend WithEvents chkTrailerTMDBXBMC As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents cbManualETSize As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents cbTrailerTMDBPref As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label

End Class