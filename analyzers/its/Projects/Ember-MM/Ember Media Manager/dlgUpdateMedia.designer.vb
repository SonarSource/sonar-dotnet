<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated> _
Partial Class dlgUpdateMedia
    Inherits System.Windows.Forms.Form

    #Region "Fields"

    Friend  WithEvents chkAllMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkCast As System.Windows.Forms.CheckBox
    Friend  WithEvents chkCert As System.Windows.Forms.CheckBox
    Friend  WithEvents chkCrew As System.Windows.Forms.CheckBox
    Friend  WithEvents chkDirector As System.Windows.Forms.CheckBox
    Friend  WithEvents chkExtraMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkFanartMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkGenre As System.Windows.Forms.CheckBox
    Friend  WithEvents chkMetaMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkMPAA As System.Windows.Forms.CheckBox
    Friend  WithEvents chkMusicBy As System.Windows.Forms.CheckBox
    Friend  WithEvents chkNFOMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkOutline As System.Windows.Forms.CheckBox
    Friend  WithEvents chkPlot As System.Windows.Forms.CheckBox
    Friend  WithEvents chkPosterMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkProducers As System.Windows.Forms.CheckBox
    Friend  WithEvents chkRating As System.Windows.Forms.CheckBox
    Friend  WithEvents chkRelease As System.Windows.Forms.CheckBox
    Friend  WithEvents chkRuntime As System.Windows.Forms.CheckBox
    Friend  WithEvents chkStudio As System.Windows.Forms.CheckBox
    Friend  WithEvents chkTagline As System.Windows.Forms.CheckBox
    Friend  WithEvents chkTitle As System.Windows.Forms.CheckBox
    Friend  WithEvents chkTop250 As System.Windows.Forms.CheckBox
    Friend  WithEvents chkCountry As System.Windows.Forms.CheckBox
    Friend  WithEvents chkTrailer As System.Windows.Forms.CheckBox
    Friend  WithEvents chkTrailerMod As System.Windows.Forms.CheckBox
    Friend  WithEvents chkVotes As System.Windows.Forms.CheckBox
    Friend  WithEvents chkWriters As System.Windows.Forms.CheckBox
    Friend  WithEvents chkYear As System.Windows.Forms.CheckBox
    Friend  WithEvents gbOptions As System.Windows.Forms.GroupBox
    Friend  WithEvents gbUpdateItems As System.Windows.Forms.GroupBox
    Friend  WithEvents gbUpdateModifier As System.Windows.Forms.GroupBox
    Friend  WithEvents gbUpdateType As System.Windows.Forms.GroupBox
    Friend  WithEvents Label2 As System.Windows.Forms.Label
    Friend  WithEvents Label4 As System.Windows.Forms.Label
    Friend  WithEvents OK_Button As System.Windows.Forms.Button
    Friend  WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend  WithEvents pnlTop As System.Windows.Forms.Panel
    Friend  WithEvents rbUpdateModifier_All As System.Windows.Forms.RadioButton
    Friend  WithEvents rbUpdateModifier_Marked As System.Windows.Forms.RadioButton
    Friend  WithEvents rbUpdateModifier_Missing As System.Windows.Forms.RadioButton
    Friend  WithEvents rbUpdateModifier_New As System.Windows.Forms.RadioButton
    Friend  WithEvents rbUpdate_Ask As System.Windows.Forms.RadioButton
    Friend  WithEvents rbUpdate_Auto As System.Windows.Forms.RadioButton
    Friend  WithEvents Update_Button As System.Windows.Forms.Button

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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgUpdateMedia))
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.rbUpdateModifier_All = New System.Windows.Forms.RadioButton()
		Me.gbUpdateModifier = New System.Windows.Forms.GroupBox()
		Me.rbUpdateModifier_Marked = New System.Windows.Forms.RadioButton()
		Me.rbUpdateModifier_New = New System.Windows.Forms.RadioButton()
		Me.rbUpdateModifier_Missing = New System.Windows.Forms.RadioButton()
		Me.gbUpdateType = New System.Windows.Forms.GroupBox()
		Me.rbUpdate_Ask = New System.Windows.Forms.RadioButton()
		Me.rbUpdate_Auto = New System.Windows.Forms.RadioButton()
		Me.gbUpdateItems = New System.Windows.Forms.GroupBox()
		Me.chkTrailerMod = New System.Windows.Forms.CheckBox()
		Me.chkExtraMod = New System.Windows.Forms.CheckBox()
		Me.chkMetaMod = New System.Windows.Forms.CheckBox()
		Me.chkFanartMod = New System.Windows.Forms.CheckBox()
		Me.chkPosterMod = New System.Windows.Forms.CheckBox()
		Me.chkNFOMod = New System.Windows.Forms.CheckBox()
		Me.chkAllMod = New System.Windows.Forms.CheckBox()
		Me.Update_Button = New System.Windows.Forms.Button()
		Me.gbOptions = New System.Windows.Forms.GroupBox()
		Me.chkCert = New System.Windows.Forms.CheckBox()
		Me.chkCountry = New System.Windows.Forms.CheckBox()
		Me.chkTop250 = New System.Windows.Forms.CheckBox()
		Me.chkCrew = New System.Windows.Forms.CheckBox()
		Me.chkMusicBy = New System.Windows.Forms.CheckBox()
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
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.gbUpdateModifier.SuspendLayout()
		Me.gbUpdateType.SuspendLayout()
		Me.gbUpdateItems.SuspendLayout()
		Me.gbOptions.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.OK_Button.Location = New System.Drawing.Point(489, 333)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(80, 23)
		Me.OK_Button.TabIndex = 1
		Me.OK_Button.Text = "Cancel"
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
		Me.pnlTop.Size = New System.Drawing.Size(576, 64)
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
		Me.Label2.Size = New System.Drawing.Size(130, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Create a custom scraper"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.BackColor = System.Drawing.Color.Transparent
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.ForeColor = System.Drawing.Color.White
		Me.Label4.Location = New System.Drawing.Point(61, 3)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(195, 32)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "Custom Scraper"
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
		'rbUpdateModifier_All
		'
		Me.rbUpdateModifier_All.AutoSize = True
		Me.rbUpdateModifier_All.Checked = True
		Me.rbUpdateModifier_All.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdateModifier_All.Location = New System.Drawing.Point(6, 19)
		Me.rbUpdateModifier_All.Name = "rbUpdateModifier_All"
		Me.rbUpdateModifier_All.Size = New System.Drawing.Size(77, 17)
		Me.rbUpdateModifier_All.TabIndex = 0
		Me.rbUpdateModifier_All.TabStop = True
		Me.rbUpdateModifier_All.Text = "All Movies"
		Me.rbUpdateModifier_All.UseVisualStyleBackColor = True
		'
		'gbUpdateModifier
		'
		Me.gbUpdateModifier.Controls.Add(Me.rbUpdateModifier_Marked)
		Me.gbUpdateModifier.Controls.Add(Me.rbUpdateModifier_New)
		Me.gbUpdateModifier.Controls.Add(Me.rbUpdateModifier_Missing)
		Me.gbUpdateModifier.Controls.Add(Me.rbUpdateModifier_All)
		Me.gbUpdateModifier.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.gbUpdateModifier.Location = New System.Drawing.Point(6, 2)
		Me.gbUpdateModifier.Name = "gbUpdateModifier"
		Me.gbUpdateModifier.Size = New System.Drawing.Size(275, 68)
		Me.gbUpdateModifier.TabIndex = 0
		Me.gbUpdateModifier.TabStop = False
		Me.gbUpdateModifier.Text = "Selection Filter"
		'
		'rbUpdateModifier_Marked
		'
		Me.rbUpdateModifier_Marked.AutoSize = True
		Me.rbUpdateModifier_Marked.Enabled = False
		Me.rbUpdateModifier_Marked.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdateModifier_Marked.Location = New System.Drawing.Point(126, 42)
		Me.rbUpdateModifier_Marked.Name = "rbUpdateModifier_Marked"
		Me.rbUpdateModifier_Marked.Size = New System.Drawing.Size(103, 17)
		Me.rbUpdateModifier_Marked.TabIndex = 3
		Me.rbUpdateModifier_Marked.Text = "Marked Movies"
		Me.rbUpdateModifier_Marked.UseVisualStyleBackColor = True
		'
		'rbUpdateModifier_New
		'
		Me.rbUpdateModifier_New.AutoSize = True
		Me.rbUpdateModifier_New.Enabled = False
		Me.rbUpdateModifier_New.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdateModifier_New.Location = New System.Drawing.Point(6, 42)
		Me.rbUpdateModifier_New.Name = "rbUpdateModifier_New"
		Me.rbUpdateModifier_New.Size = New System.Drawing.Size(87, 17)
		Me.rbUpdateModifier_New.TabIndex = 1
		Me.rbUpdateModifier_New.Text = "New Movies"
		Me.rbUpdateModifier_New.UseVisualStyleBackColor = True
		'
		'rbUpdateModifier_Missing
		'
		Me.rbUpdateModifier_Missing.AutoSize = True
		Me.rbUpdateModifier_Missing.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdateModifier_Missing.Location = New System.Drawing.Point(126, 20)
		Me.rbUpdateModifier_Missing.Name = "rbUpdateModifier_Missing"
		Me.rbUpdateModifier_Missing.Size = New System.Drawing.Size(134, 17)
		Me.rbUpdateModifier_Missing.TabIndex = 2
		Me.rbUpdateModifier_Missing.Text = "Movies Missing Items"
		Me.rbUpdateModifier_Missing.UseVisualStyleBackColor = True
		'
		'gbUpdateType
		'
		Me.gbUpdateType.Controls.Add(Me.rbUpdate_Ask)
		Me.gbUpdateType.Controls.Add(Me.rbUpdate_Auto)
		Me.gbUpdateType.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.gbUpdateType.Location = New System.Drawing.Point(6, 79)
		Me.gbUpdateType.Name = "gbUpdateType"
		Me.gbUpdateType.Size = New System.Drawing.Size(275, 75)
		Me.gbUpdateType.TabIndex = 1
		Me.gbUpdateType.TabStop = False
		Me.gbUpdateType.Text = "Update Mode"
		'
		'rbUpdate_Ask
		'
		Me.rbUpdate_Ask.AutoSize = True
		Me.rbUpdate_Ask.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdate_Ask.Location = New System.Drawing.Point(6, 41)
		Me.rbUpdate_Ask.Name = "rbUpdate_Ask"
		Me.rbUpdate_Ask.Size = New System.Drawing.Size(215, 17)
		Me.rbUpdate_Ask.TabIndex = 1
		Me.rbUpdate_Ask.Text = "Ask (Require Input If No Exact Match)"
		Me.rbUpdate_Ask.UseVisualStyleBackColor = True
		'
		'rbUpdate_Auto
		'
		Me.rbUpdate_Auto.AutoSize = True
		Me.rbUpdate_Auto.Checked = True
		Me.rbUpdate_Auto.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbUpdate_Auto.Location = New System.Drawing.Point(6, 18)
		Me.rbUpdate_Auto.Name = "rbUpdate_Auto"
		Me.rbUpdate_Auto.Size = New System.Drawing.Size(174, 17)
		Me.rbUpdate_Auto.TabIndex = 0
		Me.rbUpdate_Auto.TabStop = True
		Me.rbUpdate_Auto.Text = "Automatic (Force Best Match)"
		Me.rbUpdate_Auto.UseVisualStyleBackColor = True
		'
		'gbUpdateItems
		'
		Me.gbUpdateItems.Controls.Add(Me.chkTrailerMod)
		Me.gbUpdateItems.Controls.Add(Me.chkExtraMod)
		Me.gbUpdateItems.Controls.Add(Me.chkMetaMod)
		Me.gbUpdateItems.Controls.Add(Me.chkFanartMod)
		Me.gbUpdateItems.Controls.Add(Me.chkPosterMod)
		Me.gbUpdateItems.Controls.Add(Me.chkNFOMod)
		Me.gbUpdateItems.Controls.Add(Me.chkAllMod)
		Me.gbUpdateItems.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.gbUpdateItems.Location = New System.Drawing.Point(6, 160)
		Me.gbUpdateItems.Name = "gbUpdateItems"
		Me.gbUpdateItems.Size = New System.Drawing.Size(275, 96)
		Me.gbUpdateItems.TabIndex = 2
		Me.gbUpdateItems.TabStop = False
		Me.gbUpdateItems.Text = "Modifiers"
		'
		'chkTrailerMod
		'
		Me.chkTrailerMod.AutoSize = True
		Me.chkTrailerMod.Checked = True
		Me.chkTrailerMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkTrailerMod.Enabled = False
		Me.chkTrailerMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkTrailerMod.Location = New System.Drawing.Point(135, 54)
		Me.chkTrailerMod.Name = "chkTrailerMod"
		Me.chkTrailerMod.Size = New System.Drawing.Size(57, 17)
		Me.chkTrailerMod.TabIndex = 5
		Me.chkTrailerMod.Text = "Trailer"
		Me.chkTrailerMod.UseVisualStyleBackColor = True
		'
		'chkExtraMod
		'
		Me.chkExtraMod.AutoSize = True
		Me.chkExtraMod.Checked = True
		Me.chkExtraMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkExtraMod.Enabled = False
		Me.chkExtraMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkExtraMod.Location = New System.Drawing.Point(135, 36)
		Me.chkExtraMod.Name = "chkExtraMod"
		Me.chkExtraMod.Size = New System.Drawing.Size(90, 17)
		Me.chkExtraMod.TabIndex = 4
		Me.chkExtraMod.Text = "Extrathumbs"
		Me.chkExtraMod.UseVisualStyleBackColor = True
		'
		'chkMetaMod
		'
		Me.chkMetaMod.AutoSize = True
		Me.chkMetaMod.Checked = True
		Me.chkMetaMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkMetaMod.Enabled = False
		Me.chkMetaMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkMetaMod.Location = New System.Drawing.Point(135, 18)
		Me.chkMetaMod.Name = "chkMetaMod"
		Me.chkMetaMod.Size = New System.Drawing.Size(79, 17)
		Me.chkMetaMod.TabIndex = 3
		Me.chkMetaMod.Text = "Meta Data"
		Me.chkMetaMod.UseVisualStyleBackColor = True
		'
		'chkFanartMod
		'
		Me.chkFanartMod.AutoSize = True
		Me.chkFanartMod.Checked = True
		Me.chkFanartMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkFanartMod.Enabled = False
		Me.chkFanartMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkFanartMod.Location = New System.Drawing.Point(14, 72)
		Me.chkFanartMod.Name = "chkFanartMod"
		Me.chkFanartMod.Size = New System.Drawing.Size(59, 17)
		Me.chkFanartMod.TabIndex = 2
		Me.chkFanartMod.Text = "Fanart"
		Me.chkFanartMod.UseVisualStyleBackColor = True
		'
		'chkPosterMod
		'
		Me.chkPosterMod.AutoSize = True
		Me.chkPosterMod.Checked = True
		Me.chkPosterMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkPosterMod.Enabled = False
		Me.chkPosterMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkPosterMod.Location = New System.Drawing.Point(14, 54)
		Me.chkPosterMod.Name = "chkPosterMod"
		Me.chkPosterMod.Size = New System.Drawing.Size(58, 17)
		Me.chkPosterMod.TabIndex = 2
		Me.chkPosterMod.Text = "Poster"
		Me.chkPosterMod.UseVisualStyleBackColor = True
		'
		'chkNFOMod
		'
		Me.chkNFOMod.AutoSize = True
		Me.chkNFOMod.Checked = True
		Me.chkNFOMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkNFOMod.Enabled = False
		Me.chkNFOMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkNFOMod.Location = New System.Drawing.Point(14, 36)
		Me.chkNFOMod.Name = "chkNFOMod"
		Me.chkNFOMod.Size = New System.Drawing.Size(49, 17)
		Me.chkNFOMod.TabIndex = 1
		Me.chkNFOMod.Text = "NFO"
		Me.chkNFOMod.UseVisualStyleBackColor = True
		'
		'chkAllMod
		'
		Me.chkAllMod.AutoSize = True
		Me.chkAllMod.Checked = True
		Me.chkAllMod.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkAllMod.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkAllMod.Location = New System.Drawing.Point(14, 18)
		Me.chkAllMod.Name = "chkAllMod"
		Me.chkAllMod.Size = New System.Drawing.Size(69, 17)
		Me.chkAllMod.TabIndex = 0
		Me.chkAllMod.Text = "All Items"
		Me.chkAllMod.UseVisualStyleBackColor = True
		'
		'Update_Button
		'
		Me.Update_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.Update_Button.Enabled = False
		Me.Update_Button.Location = New System.Drawing.Point(401, 333)
		Me.Update_Button.Name = "Update_Button"
		Me.Update_Button.Size = New System.Drawing.Size(80, 23)
		Me.Update_Button.TabIndex = 0
		Me.Update_Button.Text = "Begin"
		'
		'gbOptions
		'
		Me.gbOptions.Controls.Add(Me.chkCert)
		Me.gbOptions.Controls.Add(Me.chkCountry)
		Me.gbOptions.Controls.Add(Me.chkTop250)
		Me.gbOptions.Controls.Add(Me.chkCrew)
		Me.gbOptions.Controls.Add(Me.chkMusicBy)
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
		Me.gbOptions.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.gbOptions.Location = New System.Drawing.Point(287, 2)
		Me.gbOptions.Name = "gbOptions"
		Me.gbOptions.Size = New System.Drawing.Size(274, 254)
		Me.gbOptions.TabIndex = 3
		Me.gbOptions.TabStop = False
		Me.gbOptions.Text = "Options"
		'
		'chkCert
		'
		Me.chkCert.AutoSize = True
		Me.chkCert.Checked = True
		Me.chkCert.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkCert.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkCert.Location = New System.Drawing.Point(6, 80)
		Me.chkCert.Name = "chkCert"
		Me.chkCert.Size = New System.Drawing.Size(89, 17)
		Me.chkCert.TabIndex = 3
		Me.chkCert.Text = "Certification"
		Me.chkCert.UseVisualStyleBackColor = True
		'
		'chkCountry
		'
		Me.chkCountry.AutoSize = True
		Me.chkCountry.Checked = True
		Me.chkCountry.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkCountry.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkCountry.Location = New System.Drawing.Point(147, 220)
		Me.chkCountry.Name = "chkCountry"
		Me.chkCountry.Size = New System.Drawing.Size(67, 17)
		Me.chkCountry.TabIndex = 21
		Me.chkCountry.Text = "Country"
		Me.chkCountry.UseVisualStyleBackColor = True
		'
		'chkTop250
		'
		Me.chkTop250.AutoSize = True
		Me.chkTop250.Checked = True
		Me.chkTop250.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkTop250.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkTop250.Location = New System.Drawing.Point(147, 200)
		Me.chkTop250.Name = "chkTop250"
		Me.chkTop250.Size = New System.Drawing.Size(66, 17)
		Me.chkTop250.TabIndex = 20
		Me.chkTop250.Text = "Top 250"
		Me.chkTop250.UseVisualStyleBackColor = True
		'
		'chkCrew
		'
		Me.chkCrew.AutoSize = True
		Me.chkCrew.Checked = True
		Me.chkCrew.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkCrew.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkCrew.Location = New System.Drawing.Point(147, 180)
		Me.chkCrew.Name = "chkCrew"
		Me.chkCrew.Size = New System.Drawing.Size(85, 17)
		Me.chkCrew.TabIndex = 19
		Me.chkCrew.Text = "Other Crew"
		Me.chkCrew.UseVisualStyleBackColor = True
		'
		'chkMusicBy
		'
		Me.chkMusicBy.AutoSize = True
		Me.chkMusicBy.Checked = True
		Me.chkMusicBy.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkMusicBy.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkMusicBy.Location = New System.Drawing.Point(147, 160)
		Me.chkMusicBy.Name = "chkMusicBy"
		Me.chkMusicBy.Size = New System.Drawing.Size(71, 17)
		Me.chkMusicBy.TabIndex = 18
		Me.chkMusicBy.Text = "Music By"
		Me.chkMusicBy.UseVisualStyleBackColor = True
		'
		'chkProducers
		'
		Me.chkProducers.AutoSize = True
		Me.chkProducers.Checked = True
		Me.chkProducers.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkProducers.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkProducers.Location = New System.Drawing.Point(147, 140)
		Me.chkProducers.Name = "chkProducers"
		Me.chkProducers.Size = New System.Drawing.Size(77, 17)
		Me.chkProducers.TabIndex = 17
		Me.chkProducers.Text = "Producers"
		Me.chkProducers.UseVisualStyleBackColor = True
		'
		'chkWriters
		'
		Me.chkWriters.AutoSize = True
		Me.chkWriters.Checked = True
		Me.chkWriters.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkWriters.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkWriters.Location = New System.Drawing.Point(147, 120)
		Me.chkWriters.Name = "chkWriters"
		Me.chkWriters.Size = New System.Drawing.Size(63, 17)
		Me.chkWriters.TabIndex = 16
		Me.chkWriters.Text = "Writers"
		Me.chkWriters.UseVisualStyleBackColor = True
		'
		'chkStudio
		'
		Me.chkStudio.AutoSize = True
		Me.chkStudio.Checked = True
		Me.chkStudio.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkStudio.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkStudio.Location = New System.Drawing.Point(6, 180)
		Me.chkStudio.Name = "chkStudio"
		Me.chkStudio.Size = New System.Drawing.Size(60, 17)
		Me.chkStudio.TabIndex = 8
		Me.chkStudio.Text = "Studio"
		Me.chkStudio.UseVisualStyleBackColor = True
		'
		'chkRuntime
		'
		Me.chkRuntime.AutoSize = True
		Me.chkRuntime.Checked = True
		Me.chkRuntime.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkRuntime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkRuntime.Location = New System.Drawing.Point(6, 120)
		Me.chkRuntime.Name = "chkRuntime"
		Me.chkRuntime.Size = New System.Drawing.Size(69, 17)
		Me.chkRuntime.TabIndex = 5
		Me.chkRuntime.Text = "Runtime"
		Me.chkRuntime.UseVisualStyleBackColor = True
		'
		'chkPlot
		'
		Me.chkPlot.AutoSize = True
		Me.chkPlot.Checked = True
		Me.chkPlot.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkPlot.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkPlot.Location = New System.Drawing.Point(147, 60)
		Me.chkPlot.Name = "chkPlot"
		Me.chkPlot.Size = New System.Drawing.Size(46, 17)
		Me.chkPlot.TabIndex = 13
		Me.chkPlot.Text = "Plot"
		Me.chkPlot.UseVisualStyleBackColor = True
		'
		'chkOutline
		'
		Me.chkOutline.AutoSize = True
		Me.chkOutline.Checked = True
		Me.chkOutline.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkOutline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkOutline.Location = New System.Drawing.Point(147, 40)
		Me.chkOutline.Name = "chkOutline"
		Me.chkOutline.Size = New System.Drawing.Size(65, 17)
		Me.chkOutline.TabIndex = 12
		Me.chkOutline.Text = "Outline"
		Me.chkOutline.UseVisualStyleBackColor = True
		'
		'chkGenre
		'
		Me.chkGenre.AutoSize = True
		Me.chkGenre.Checked = True
		Me.chkGenre.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkGenre.Location = New System.Drawing.Point(6, 200)
		Me.chkGenre.Name = "chkGenre"
		Me.chkGenre.Size = New System.Drawing.Size(57, 17)
		Me.chkGenre.TabIndex = 9
		Me.chkGenre.Text = "Genre"
		Me.chkGenre.UseVisualStyleBackColor = True
		'
		'chkDirector
		'
		Me.chkDirector.AutoSize = True
		Me.chkDirector.Checked = True
		Me.chkDirector.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkDirector.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkDirector.Location = New System.Drawing.Point(147, 100)
		Me.chkDirector.Name = "chkDirector"
		Me.chkDirector.Size = New System.Drawing.Size(67, 17)
		Me.chkDirector.TabIndex = 15
		Me.chkDirector.Text = "Director"
		Me.chkDirector.UseVisualStyleBackColor = True
		'
		'chkTagline
		'
		Me.chkTagline.AutoSize = True
		Me.chkTagline.Checked = True
		Me.chkTagline.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkTagline.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkTagline.Location = New System.Drawing.Point(147, 19)
		Me.chkTagline.Name = "chkTagline"
		Me.chkTagline.Size = New System.Drawing.Size(63, 17)
		Me.chkTagline.TabIndex = 11
		Me.chkTagline.Text = "Tagline"
		Me.chkTagline.UseVisualStyleBackColor = True
		'
		'chkCast
		'
		Me.chkCast.AutoSize = True
		Me.chkCast.Checked = True
		Me.chkCast.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkCast.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkCast.Location = New System.Drawing.Point(147, 80)
		Me.chkCast.Name = "chkCast"
		Me.chkCast.Size = New System.Drawing.Size(48, 17)
		Me.chkCast.TabIndex = 14
		Me.chkCast.Text = "Cast"
		Me.chkCast.UseVisualStyleBackColor = True
		'
		'chkVotes
		'
		Me.chkVotes.AutoSize = True
		Me.chkVotes.Checked = True
		Me.chkVotes.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkVotes.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkVotes.Location = New System.Drawing.Point(6, 160)
		Me.chkVotes.Name = "chkVotes"
		Me.chkVotes.Size = New System.Drawing.Size(55, 17)
		Me.chkVotes.TabIndex = 7
		Me.chkVotes.Text = "Votes"
		Me.chkVotes.UseVisualStyleBackColor = True
		'
		'chkTrailer
		'
		Me.chkTrailer.AutoSize = True
		Me.chkTrailer.Checked = True
		Me.chkTrailer.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkTrailer.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkTrailer.Location = New System.Drawing.Point(6, 220)
		Me.chkTrailer.Name = "chkTrailer"
		Me.chkTrailer.Size = New System.Drawing.Size(57, 17)
		Me.chkTrailer.TabIndex = 10
		Me.chkTrailer.Text = "Trailer"
		Me.chkTrailer.UseVisualStyleBackColor = True
		'
		'chkRating
		'
		Me.chkRating.AutoSize = True
		Me.chkRating.Checked = True
		Me.chkRating.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkRating.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkRating.Location = New System.Drawing.Point(6, 140)
		Me.chkRating.Name = "chkRating"
		Me.chkRating.Size = New System.Drawing.Size(60, 17)
		Me.chkRating.TabIndex = 6
		Me.chkRating.Text = "Rating"
		Me.chkRating.UseVisualStyleBackColor = True
		'
		'chkRelease
		'
		Me.chkRelease.AutoSize = True
		Me.chkRelease.Checked = True
		Me.chkRelease.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkRelease.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkRelease.Location = New System.Drawing.Point(6, 100)
		Me.chkRelease.Name = "chkRelease"
		Me.chkRelease.Size = New System.Drawing.Size(92, 17)
		Me.chkRelease.TabIndex = 4
		Me.chkRelease.Text = "Release Date"
		Me.chkRelease.UseVisualStyleBackColor = True
		'
		'chkMPAA
		'
		Me.chkMPAA.AutoSize = True
		Me.chkMPAA.Checked = True
		Me.chkMPAA.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkMPAA.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkMPAA.Location = New System.Drawing.Point(6, 60)
		Me.chkMPAA.Name = "chkMPAA"
		Me.chkMPAA.Size = New System.Drawing.Size(56, 17)
		Me.chkMPAA.TabIndex = 2
		Me.chkMPAA.Text = "MPAA"
		Me.chkMPAA.UseVisualStyleBackColor = True
		'
		'chkYear
		'
		Me.chkYear.AutoSize = True
		Me.chkYear.Checked = True
		Me.chkYear.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkYear.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkYear.Location = New System.Drawing.Point(6, 40)
		Me.chkYear.Name = "chkYear"
		Me.chkYear.Size = New System.Drawing.Size(47, 17)
		Me.chkYear.TabIndex = 1
		Me.chkYear.Text = "Year"
		Me.chkYear.UseVisualStyleBackColor = True
		'
		'chkTitle
		'
		Me.chkTitle.AutoSize = True
		Me.chkTitle.Checked = True
		Me.chkTitle.CheckState = System.Windows.Forms.CheckState.Checked
		Me.chkTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkTitle.Location = New System.Drawing.Point(6, 19)
		Me.chkTitle.Name = "chkTitle"
		Me.chkTitle.Size = New System.Drawing.Size(47, 17)
		Me.chkTitle.TabIndex = 0
		Me.chkTitle.Text = "Title"
		Me.chkTitle.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.gbOptions)
		Me.Panel1.Controls.Add(Me.gbUpdateItems)
		Me.Panel1.Controls.Add(Me.gbUpdateType)
		Me.Panel1.Controls.Add(Me.gbUpdateModifier)
		Me.Panel1.Location = New System.Drawing.Point(4, 68)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(568, 262)
		Me.Panel1.TabIndex = 3
		'
		'dlgUpdateMedia
		'
		Me.AcceptButton = Me.Update_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.OK_Button
		Me.ClientSize = New System.Drawing.Size(576, 358)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.Update_Button)
		Me.Controls.Add(Me.pnlTop)
		Me.Controls.Add(Me.OK_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgUpdateMedia"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Custom Scraper"
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.gbUpdateModifier.ResumeLayout(False)
		Me.gbUpdateModifier.PerformLayout()
		Me.gbUpdateType.ResumeLayout(False)
		Me.gbUpdateType.PerformLayout()
		Me.gbUpdateItems.ResumeLayout(False)
		Me.gbUpdateItems.PerformLayout()
		Me.gbOptions.ResumeLayout(False)
		Me.gbOptions.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

    #End Region 'Methods

End Class