<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated> _
Partial Class dlgOfflineHolder
    Inherits System.Windows.Forms.Form

    #Region "Fields"

    Friend  WithEvents btnBackgroundColor As System.Windows.Forms.Button
    Friend  WithEvents btnFont As System.Windows.Forms.Button
    Friend  WithEvents btnTextColor As System.Windows.Forms.Button
    Friend  WithEvents Bulk_Button As System.Windows.Forms.Button
    Friend  WithEvents cbFormat As System.Windows.Forms.ComboBox
    Friend  WithEvents cbSources As System.Windows.Forms.ComboBox
    Friend  WithEvents cdColor As System.Windows.Forms.ColorDialog
    Friend  WithEvents cdFont As System.Windows.Forms.FontDialog
    Friend  WithEvents chkBackground As System.Windows.Forms.CheckBox
    Friend  WithEvents chkOverlay As System.Windows.Forms.CheckBox
    Friend  WithEvents chkUseFanart As System.Windows.Forms.CheckBox
    Friend  WithEvents CLOSE_Button As System.Windows.Forms.Button
    Friend  WithEvents colCondition As System.Windows.Forms.ColumnHeader
    Friend  WithEvents colStatus As System.Windows.Forms.ColumnHeader
    Friend  WithEvents Create_Button As System.Windows.Forms.Button
    Friend  WithEvents GetIMDB_Button As System.Windows.Forms.Button
    Friend  WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend  WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend  WithEvents Label1 As System.Windows.Forms.Label
    Friend  WithEvents Label2 As System.Windows.Forms.Label
    Friend  WithEvents Label3 As System.Windows.Forms.Label
    Friend  WithEvents Label4 As System.Windows.Forms.Label
    Friend  WithEvents Label5 As System.Windows.Forms.Label
    Friend  WithEvents Label6 As System.Windows.Forms.Label
    Friend  WithEvents lblMovie As System.Windows.Forms.Label
    Friend  WithEvents lblSources As System.Windows.Forms.Label
    Friend  WithEvents lblTagline As System.Windows.Forms.Label
    Friend  WithEvents lvStatus As System.Windows.Forms.ListView
    Friend  WithEvents pbPreview As System.Windows.Forms.PictureBox
    Friend  WithEvents pbProgress As System.Windows.Forms.ProgressBar
    Friend  WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend  WithEvents pnlTop As System.Windows.Forms.Panel
    Friend  WithEvents tbTagLine As System.Windows.Forms.TrackBar
    Friend  WithEvents tmrName As System.Windows.Forms.Timer
    Friend  WithEvents tmrNameWait As System.Windows.Forms.Timer
    Friend  WithEvents txtMovieName As System.Windows.Forms.TextBox
    Friend  WithEvents txtTagline As System.Windows.Forms.TextBox
    Friend  WithEvents txtTop As System.Windows.Forms.TextBox

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    #End Region 'Fields

    #Region "Methods"

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgOfflineHolder))
		Me.CLOSE_Button = New System.Windows.Forms.Button()
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.cbSources = New System.Windows.Forms.ComboBox()
		Me.lblSources = New System.Windows.Forms.Label()
		Me.txtMovieName = New System.Windows.Forms.TextBox()
		Me.lblMovie = New System.Windows.Forms.Label()
		Me.GetIMDB_Button = New System.Windows.Forms.Button()
		Me.Bulk_Button = New System.Windows.Forms.Button()
		Me.pbProgress = New System.Windows.Forms.ProgressBar()
		Me.lvStatus = New System.Windows.Forms.ListView()
		Me.colCondition = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colStatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.Create_Button = New System.Windows.Forms.Button()
		Me.chkUseFanart = New System.Windows.Forms.CheckBox()
		Me.lblTagline = New System.Windows.Forms.Label()
		Me.txtTagline = New System.Windows.Forms.TextBox()
		Me.btnTextColor = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.cdColor = New System.Windows.Forms.ColorDialog()
		Me.pbPreview = New System.Windows.Forms.PictureBox()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.cbFormat = New System.Windows.Forms.ComboBox()
		Me.chkBackground = New System.Windows.Forms.CheckBox()
		Me.btnBackgroundColor = New System.Windows.Forms.Button()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.chkOverlay = New System.Windows.Forms.CheckBox()
		Me.btnFont = New System.Windows.Forms.Button()
		Me.txtTop = New System.Windows.Forms.TextBox()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.tmrName = New System.Windows.Forms.Timer(Me.components)
		Me.cdFont = New System.Windows.Forms.FontDialog()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.tbTagLine = New System.Windows.Forms.TrackBar()
		Me.tmrNameWait = New System.Windows.Forms.Timer(Me.components)
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbPreview, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox1.SuspendLayout()
		Me.GroupBox2.SuspendLayout()
		CType(Me.tbTagLine, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'CLOSE_Button
		'
		Me.CLOSE_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.CLOSE_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.CLOSE_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.CLOSE_Button.Location = New System.Drawing.Point(649, 518)
		Me.CLOSE_Button.Name = "CLOSE_Button"
		Me.CLOSE_Button.Size = New System.Drawing.Size(80, 23)
		Me.CLOSE_Button.TabIndex = 1
		Me.CLOSE_Button.Text = "Close"
		'
		'pnlTop
		'
		Me.pnlTop.BackColor = System.Drawing.Color.SteelBlue
		Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlTop.Controls.Add(Me.Label2)
		Me.pnlTop.Controls.Add(Me.Label4)
		Me.pnlTop.Controls.Add(Me.PictureBox1)
		Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
		Me.pnlTop.Location = New System.Drawing.Point(0, 0)
		Me.pnlTop.Name = "pnlTop"
		Me.pnlTop.Size = New System.Drawing.Size(734, 64)
		Me.pnlTop.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.BackColor = System.Drawing.Color.Transparent
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.ForeColor = System.Drawing.Color.White
		Me.Label2.Location = New System.Drawing.Point(64, 38)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(102, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Add Offline movie"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.BackColor = System.Drawing.Color.Transparent
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.ForeColor = System.Drawing.Color.White
		Me.Label4.Location = New System.Drawing.Point(61, 3)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(280, 32)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "Offline Media Manager"
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(12, 7)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(48, 48)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
		Me.PictureBox1.TabIndex = 0
		Me.PictureBox1.TabStop = False
		'
		'cbSources
		'
		Me.cbSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbSources.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.cbSources.FormattingEnabled = True
		Me.cbSources.Location = New System.Drawing.Point(9, 19)
		Me.cbSources.Name = "cbSources"
		Me.cbSources.Size = New System.Drawing.Size(313, 21)
		Me.cbSources.TabIndex = 1
		'
		'lblSources
		'
		Me.lblSources.AutoSize = True
		Me.lblSources.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblSources.Location = New System.Drawing.Point(7, 4)
		Me.lblSources.Name = "lblSources"
		Me.lblSources.Size = New System.Drawing.Size(84, 13)
		Me.lblSources.TabIndex = 0
		Me.lblSources.Text = "Add to Source:"
		'
		'txtMovieName
		'
		Me.txtMovieName.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtMovieName.Location = New System.Drawing.Point(9, 65)
		Me.txtMovieName.Name = "txtMovieName"
		Me.txtMovieName.Size = New System.Drawing.Size(313, 22)
		Me.txtMovieName.TabIndex = 3
		'
		'lblMovie
		'
		Me.lblMovie.AutoSize = True
		Me.lblMovie.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblMovie.Location = New System.Drawing.Point(7, 50)
		Me.lblMovie.Name = "lblMovie"
		Me.lblMovie.Size = New System.Drawing.Size(183, 13)
		Me.lblMovie.TabIndex = 2
		Me.lblMovie.Text = "Place Holder Folder/Movie Name:"
		'
		'GetIMDB_Button
		'
		Me.GetIMDB_Button.BackColor = System.Drawing.SystemColors.Control
		Me.GetIMDB_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GetIMDB_Button.Location = New System.Drawing.Point(9, 90)
		Me.GetIMDB_Button.Name = "GetIMDB_Button"
		Me.GetIMDB_Button.Size = New System.Drawing.Size(80, 21)
		Me.GetIMDB_Button.TabIndex = 4
		Me.GetIMDB_Button.Text = "Search IMDB"
		Me.GetIMDB_Button.UseVisualStyleBackColor = True
		'
		'Bulk_Button
		'
		Me.Bulk_Button.BackColor = System.Drawing.SystemColors.Control
		Me.Bulk_Button.Enabled = False
		Me.Bulk_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Bulk_Button.Location = New System.Drawing.Point(95, 90)
		Me.Bulk_Button.Name = "Bulk_Button"
		Me.Bulk_Button.Size = New System.Drawing.Size(80, 21)
		Me.Bulk_Button.TabIndex = 5
		Me.Bulk_Button.Text = "Bulk Creator"
		Me.Bulk_Button.UseVisualStyleBackColor = True
		Me.Bulk_Button.Visible = False
		'
		'pbProgress
		'
		Me.pbProgress.Location = New System.Drawing.Point(6, 19)
		Me.pbProgress.MarqueeAnimationSpeed = 25
		Me.pbProgress.Name = "pbProgress"
		Me.pbProgress.Size = New System.Drawing.Size(301, 20)
		Me.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.pbProgress.TabIndex = 0
		Me.pbProgress.Visible = False
		'
		'lvStatus
		'
		Me.lvStatus.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colCondition, Me.colStatus})
		Me.lvStatus.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lvStatus.FullRowSelect = True
		Me.lvStatus.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
		Me.lvStatus.Location = New System.Drawing.Point(5, 44)
		Me.lvStatus.MultiSelect = False
		Me.lvStatus.Name = "lvStatus"
		Me.lvStatus.Size = New System.Drawing.Size(303, 229)
		Me.lvStatus.TabIndex = 1
		Me.lvStatus.UseCompatibleStateImageBehavior = False
		Me.lvStatus.View = System.Windows.Forms.View.Details
		'
		'colCondition
		'
		Me.colCondition.Text = "Condition"
		Me.colCondition.Width = 236
		'
		'colStatus
		'
		Me.colStatus.Text = "Status"
		Me.colStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
		'
		'Create_Button
		'
		Me.Create_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.Create_Button.Enabled = False
		Me.Create_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Create_Button.Location = New System.Drawing.Point(563, 518)
		Me.Create_Button.Name = "Create_Button"
		Me.Create_Button.Size = New System.Drawing.Size(80, 23)
		Me.Create_Button.TabIndex = 0
		Me.Create_Button.Text = "Create"
		'
		'chkUseFanart
		'
		Me.chkUseFanart.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkUseFanart.Enabled = False
		Me.chkUseFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkUseFanart.Location = New System.Drawing.Point(9, 364)
		Me.chkUseFanart.Name = "chkUseFanart"
		Me.chkUseFanart.Size = New System.Drawing.Size(192, 22)
		Me.chkUseFanart.TabIndex = 9
		Me.chkUseFanart.Text = "Use Fanart for Place Holder Video"
		Me.chkUseFanart.UseVisualStyleBackColor = True
		'
		'lblTagline
		'
		Me.lblTagline.AutoSize = True
		Me.lblTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblTagline.Location = New System.Drawing.Point(7, 297)
		Me.lblTagline.Name = "lblTagline"
		Me.lblTagline.Size = New System.Drawing.Size(149, 13)
		Me.lblTagline.TabIndex = 0
		Me.lblTagline.Text = "Place Holder Video Tagline:"
		'
		'txtTagline
		'
		Me.txtTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTagline.Location = New System.Drawing.Point(9, 312)
		Me.txtTagline.Name = "txtTagline"
		Me.txtTagline.Size = New System.Drawing.Size(220, 22)
		Me.txtTagline.TabIndex = 1
		Me.txtTagline.Text = "Insert DVD"
		'
		'btnTextColor
		'
		Me.btnTextColor.BackColor = System.Drawing.Color.White
		Me.btnTextColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup
		Me.btnTextColor.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnTextColor.ForeColor = System.Drawing.SystemColors.ControlText
		Me.btnTextColor.Location = New System.Drawing.Point(337, 320)
		Me.btnTextColor.Name = "btnTextColor"
		Me.btnTextColor.Size = New System.Drawing.Size(24, 22)
		Me.btnTextColor.TabIndex = 5
		Me.btnTextColor.UseVisualStyleBackColor = False
		'
		'Label1
		'
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.Location = New System.Drawing.Point(235, 325)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(100, 13)
		Me.Label1.TabIndex = 4
		Me.Label1.Text = "Text Color:"
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'pbPreview
		'
		Me.pbPreview.Location = New System.Drawing.Point(6, 17)
		Me.pbPreview.Name = "pbPreview"
		Me.pbPreview.Size = New System.Drawing.Size(359, 274)
		Me.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
		Me.pbPreview.TabIndex = 73
		Me.pbPreview.TabStop = False
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.Label6)
		Me.GroupBox1.Controls.Add(Me.cbFormat)
		Me.GroupBox1.Controls.Add(Me.chkBackground)
		Me.GroupBox1.Controls.Add(Me.btnBackgroundColor)
		Me.GroupBox1.Controls.Add(Me.Label5)
		Me.GroupBox1.Controls.Add(Me.chkOverlay)
		Me.GroupBox1.Controls.Add(Me.btnFont)
		Me.GroupBox1.Controls.Add(Me.chkUseFanart)
		Me.GroupBox1.Controls.Add(Me.lblTagline)
		Me.GroupBox1.Controls.Add(Me.btnTextColor)
		Me.GroupBox1.Controls.Add(Me.txtTagline)
		Me.GroupBox1.Controls.Add(Me.Label1)
		Me.GroupBox1.Controls.Add(Me.txtTop)
		Me.GroupBox1.Controls.Add(Me.Label3)
		Me.GroupBox1.Controls.Add(Me.pbPreview)
		Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(332, 3)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(371, 437)
		Me.GroupBox1.TabIndex = 7
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Preview"
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label6.Location = New System.Drawing.Point(6, 343)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(148, 13)
		Me.Label6.TabIndex = 7
		Me.Label6.Text = "Place Holder Video Format:"
		'
		'cbFormat
		'
		Me.cbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbFormat.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.cbFormat.FormattingEnabled = True
		Me.cbFormat.Items.AddRange(New Object() {"Standard", "HDV", "DV PAL"})
		Me.cbFormat.Location = New System.Drawing.Point(156, 338)
		Me.cbFormat.Name = "cbFormat"
		Me.cbFormat.Size = New System.Drawing.Size(73, 21)
		Me.cbFormat.TabIndex = 8
		'
		'chkBackground
		'
		Me.chkBackground.Checked = True
		Me.chkBackground.CheckState = System.Windows.Forms.CheckState.Indeterminate
		Me.chkBackground.Enabled = False
		Me.chkBackground.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkBackground.Location = New System.Drawing.Point(9, 408)
		Me.chkBackground.Name = "chkBackground"
		Me.chkBackground.Size = New System.Drawing.Size(176, 22)
		Me.chkBackground.TabIndex = 11
		Me.chkBackground.Text = "Use Tagline background"
		Me.chkBackground.UseVisualStyleBackColor = True
		'
		'btnBackgroundColor
		'
		Me.btnBackgroundColor.BackColor = System.Drawing.SystemColors.InactiveCaptionText
		Me.btnBackgroundColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup
		Me.btnBackgroundColor.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnBackgroundColor.ForeColor = System.Drawing.SystemColors.ControlText
		Me.btnBackgroundColor.Location = New System.Drawing.Point(337, 405)
		Me.btnBackgroundColor.Name = "btnBackgroundColor"
		Me.btnBackgroundColor.Size = New System.Drawing.Size(24, 22)
		Me.btnBackgroundColor.TabIndex = 13
		Me.btnBackgroundColor.UseVisualStyleBackColor = False
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label5.Location = New System.Drawing.Point(191, 410)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(145, 13)
		Me.Label5.TabIndex = 12
		Me.Label5.Text = "Tagline background Color:"
		'
		'chkOverlay
		'
		Me.chkOverlay.Checked = True
		Me.chkOverlay.CheckState = System.Windows.Forms.CheckState.Indeterminate
		Me.chkOverlay.Enabled = False
		Me.chkOverlay.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkOverlay.Location = New System.Drawing.Point(9, 386)
		Me.chkOverlay.Name = "chkOverlay"
		Me.chkOverlay.Size = New System.Drawing.Size(192, 22)
		Me.chkOverlay.TabIndex = 10
		Me.chkOverlay.Text = "Use Ember Overlay"
		Me.chkOverlay.UseVisualStyleBackColor = True
		'
		'btnFont
		'
		Me.btnFont.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnFont.Location = New System.Drawing.Point(258, 345)
		Me.btnFont.Name = "btnFont"
		Me.btnFont.Size = New System.Drawing.Size(104, 23)
		Me.btnFont.TabIndex = 6
		Me.btnFont.Text = "Select Font..."
		Me.btnFont.UseVisualStyleBackColor = True
		'
		'txtTop
		'
		Me.txtTop.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtTop.Location = New System.Drawing.Point(325, 295)
		Me.txtTop.Name = "txtTop"
		Me.txtTop.Size = New System.Drawing.Size(36, 22)
		Me.txtTop.TabIndex = 3
		Me.txtTop.Text = "470"
		Me.txtTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
		'
		'Label3
		'
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label3.Location = New System.Drawing.Point(238, 297)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(88, 13)
		Me.Label3.TabIndex = 2
		Me.Label3.Text = "Tagline Top:"
		Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'tmrName
		'
		Me.tmrName.Interval = 250
		'
		'cdFont
		'
		Me.cdFont.Font = New System.Drawing.Font("Arial", 21.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		'
		'GroupBox2
		'
		Me.GroupBox2.Controls.Add(Me.pbProgress)
		Me.GroupBox2.Controls.Add(Me.lvStatus)
		Me.GroupBox2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GroupBox2.Location = New System.Drawing.Point(9, 161)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(313, 279)
		Me.GroupBox2.TabIndex = 6
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Information"
		'
		'tbTagLine
		'
		Me.tbTagLine.Location = New System.Drawing.Point(680, 13)
		Me.tbTagLine.Maximum = 576
		Me.tbTagLine.Name = "tbTagLine"
		Me.tbTagLine.Orientation = System.Windows.Forms.Orientation.Vertical
		Me.tbTagLine.RightToLeft = System.Windows.Forms.RightToLeft.Yes
		Me.tbTagLine.RightToLeftLayout = True
		Me.tbTagLine.Size = New System.Drawing.Size(45, 281)
		Me.tbTagLine.TabIndex = 8
		Me.tbTagLine.TickStyle = System.Windows.Forms.TickStyle.None
		'
		'tmrNameWait
		'
		Me.tmrNameWait.Interval = 250
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.GroupBox2)
		Me.Panel1.Controls.Add(Me.GroupBox1)
		Me.Panel1.Controls.Add(Me.Bulk_Button)
		Me.Panel1.Controls.Add(Me.GetIMDB_Button)
		Me.Panel1.Controls.Add(Me.lblMovie)
		Me.Panel1.Controls.Add(Me.txtMovieName)
		Me.Panel1.Controls.Add(Me.lblSources)
		Me.Panel1.Controls.Add(Me.cbSources)
		Me.Panel1.Controls.Add(Me.tbTagLine)
		Me.Panel1.Location = New System.Drawing.Point(4, 68)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(726, 446)
		Me.Panel1.TabIndex = 3
		'
		'dlgOfflineHolder
		'
		Me.AcceptButton = Me.Create_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.CLOSE_Button
		Me.ClientSize = New System.Drawing.Size(734, 545)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.Create_Button)
		Me.Controls.Add(Me.pnlTop)
		Me.Controls.Add(Me.CLOSE_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgOfflineHolder"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Offline Media Manager"
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbPreview, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.GroupBox2.ResumeLayout(False)
		CType(Me.tbTagLine, System.ComponentModel.ISupportInitialize).EndInit()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

    #End Region 'Methods

End Class