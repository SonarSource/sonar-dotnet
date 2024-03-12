<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgNMTMovies
    Inherits System.Windows.Forms.Form

#Region "Fields"

    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Close_Button As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblCanceling As System.Windows.Forms.Label
    Friend WithEvents lblCompiling As System.Windows.Forms.Label
    Friend WithEvents pbCompile As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlCancel As System.Windows.Forms.Panel

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

#End Region 'Fields

#Region "Methods"

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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgNMTMovies))
		Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Me.Close_Button = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.pnlCancel = New System.Windows.Forms.Panel()
		Me.btnCancel = New System.Windows.Forms.Button()
		Me.pbCompile = New System.Windows.Forms.ProgressBar()
		Me.lblCompiling = New System.Windows.Forms.Label()
		Me.lblCanceling = New System.Windows.Forms.Label()
		Me.cbTemplate = New System.Windows.Forms.ComboBox()
		Me.btnBuild = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.txtOutputFolder = New System.Windows.Forms.TextBox()
		Me.Panel2 = New System.Windows.Forms.Panel()
		Me.gbHelp = New System.Windows.Forms.GroupBox()
		Me.PictureBox2 = New System.Windows.Forms.PictureBox()
		Me.lblHelpa = New System.Windows.Forms.Label()
		Me.dgvSources = New System.Windows.Forms.DataGridView()
		Me.export = New System.Windows.Forms.DataGridViewCheckBoxColumn()
		Me.EmberSource = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column1 = New System.Windows.Forms.DataGridViewImageColumn()
		Me.Value = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.sourcetype = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.dgvSettings = New System.Windows.Forms.DataGridView()
		Me.Setting = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.DataGridViewComboBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.ValidatedToBuild = New System.Windows.Forms.Timer(Me.components)
		Me.lblTemplateInfo = New System.Windows.Forms.Label()
		Me.btnBrowse = New System.Windows.Forms.Button()
		Me.btnSave = New System.Windows.Forms.Button()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage2 = New System.Windows.Forms.TabPage()
		Me.dgvProperties = New System.Windows.Forms.DataGridView()
		Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.pbTemplateLogo = New System.Windows.Forms.PictureBox()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.lblWarning = New System.Windows.Forms.Label()
		Me.pbWarning = New System.Windows.Forms.PictureBox()
		Me.ilNMT = New System.Windows.Forms.ImageList(Me.components)
		Me.chHighPriority = New System.Windows.Forms.CheckBox()
		Me.pbHelp = New System.Windows.Forms.PictureBox()
		Me.pnlCancel.SuspendLayout()
		Me.Panel2.SuspendLayout()
		Me.gbHelp.SuspendLayout()
		CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.dgvSources, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.dgvSettings, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabControl1.SuspendLayout()
		Me.TabPage2.SuspendLayout()
		CType(Me.dgvProperties, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage1.SuspendLayout()
		CType(Me.pbTemplateLogo, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.Panel1.SuspendLayout()
		CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbHelp, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'Close_Button
		'
		Me.Close_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Close_Button.DialogResult = System.Windows.Forms.DialogResult.OK
		Me.Close_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Close_Button.Location = New System.Drawing.Point(778, 368)
		Me.Close_Button.Name = "Close_Button"
		Me.Close_Button.Size = New System.Drawing.Size(67, 23)
		Me.Close_Button.TabIndex = 1
		Me.Close_Button.Text = "Close"
		'
		'Label2
		'
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.Location = New System.Drawing.Point(11, 6)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(100, 18)
		Me.Label2.TabIndex = 2
		Me.Label2.Text = "Template"
		Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'pnlCancel
		'
		Me.pnlCancel.BackColor = System.Drawing.Color.LightGray
		Me.pnlCancel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlCancel.Controls.Add(Me.btnCancel)
		Me.pnlCancel.Controls.Add(Me.pbCompile)
		Me.pnlCancel.Controls.Add(Me.lblCompiling)
		Me.pnlCancel.Controls.Add(Me.lblCanceling)
		Me.pnlCancel.Location = New System.Drawing.Point(250, 137)
		Me.pnlCancel.Name = "pnlCancel"
		Me.pnlCancel.Size = New System.Drawing.Size(403, 76)
		Me.pnlCancel.TabIndex = 0
		Me.pnlCancel.Visible = False
		'
		'btnCancel
		'
		Me.btnCancel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnCancel.Image = CType(resources.GetObject("btnCancel.Image"), System.Drawing.Image)
		Me.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnCancel.Location = New System.Drawing.Point(298, 3)
		Me.btnCancel.Name = "btnCancel"
		Me.btnCancel.Size = New System.Drawing.Size(100, 30)
		Me.btnCancel.TabIndex = 2
		Me.btnCancel.Text = "Cancel"
		Me.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnCancel.UseVisualStyleBackColor = True
		'
		'pbCompile
		'
		Me.pbCompile.Location = New System.Drawing.Point(8, 36)
		Me.pbCompile.Name = "pbCompile"
		Me.pbCompile.Size = New System.Drawing.Size(388, 18)
		Me.pbCompile.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbCompile.TabIndex = 3
		'
		'lblCompiling
		'
		Me.lblCompiling.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCompiling.Location = New System.Drawing.Point(3, 11)
		Me.lblCompiling.Name = "lblCompiling"
		Me.lblCompiling.Size = New System.Drawing.Size(395, 20)
		Me.lblCompiling.TabIndex = 0
		Me.lblCompiling.Text = "Compiling Movie List..."
		Me.lblCompiling.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.lblCompiling.Visible = False
		'
		'lblCanceling
		'
		Me.lblCanceling.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblCanceling.Location = New System.Drawing.Point(110, 12)
		Me.lblCanceling.Name = "lblCanceling"
		Me.lblCanceling.Size = New System.Drawing.Size(186, 20)
		Me.lblCanceling.TabIndex = 1
		Me.lblCanceling.Text = "Canceling Compilation..."
		Me.lblCanceling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCanceling.Visible = False
		'
		'cbTemplate
		'
		Me.cbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbTemplate.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.cbTemplate.FormattingEnabled = True
		Me.cbTemplate.Location = New System.Drawing.Point(113, 3)
		Me.cbTemplate.Name = "cbTemplate"
		Me.cbTemplate.Size = New System.Drawing.Size(196, 21)
		Me.cbTemplate.TabIndex = 3
		'
		'btnBuild
		'
		Me.btnBuild.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnBuild.Enabled = False
		Me.btnBuild.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnBuild.Location = New System.Drawing.Point(705, 368)
		Me.btnBuild.Name = "btnBuild"
		Me.btnBuild.Size = New System.Drawing.Size(67, 23)
		Me.btnBuild.TabIndex = 0
		Me.btnBuild.Text = "Build"
		'
		'Label1
		'
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.Location = New System.Drawing.Point(7, 33)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(100, 19)
		Me.Label1.TabIndex = 4
		Me.Label1.Text = "Output Folder"
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
		'
		'txtOutputFolder
		'
		Me.txtOutputFolder.Location = New System.Drawing.Point(113, 30)
		Me.txtOutputFolder.Name = "txtOutputFolder"
		Me.txtOutputFolder.Size = New System.Drawing.Size(168, 22)
		Me.txtOutputFolder.TabIndex = 5
		'
		'Panel2
		'
		Me.Panel2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.Panel2.BackColor = System.Drawing.Color.White
		Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Panel2.Controls.Add(Me.gbHelp)
		Me.Panel2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Panel2.Location = New System.Drawing.Point(7, 292)
		Me.Panel2.Name = "Panel2"
		Me.Panel2.Size = New System.Drawing.Size(692, 102)
		Me.Panel2.TabIndex = 13
		'
		'gbHelp
		'
		Me.gbHelp.BackColor = System.Drawing.Color.White
		Me.gbHelp.Controls.Add(Me.PictureBox2)
		Me.gbHelp.Controls.Add(Me.lblHelpa)
		Me.gbHelp.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.gbHelp.Location = New System.Drawing.Point(3, 3)
		Me.gbHelp.Name = "gbHelp"
		Me.gbHelp.Size = New System.Drawing.Size(684, 94)
		Me.gbHelp.TabIndex = 0
		Me.gbHelp.TabStop = False
		Me.gbHelp.Text = "     Help"
		'
		'PictureBox2
		'
		Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
		Me.PictureBox2.Location = New System.Drawing.Point(6, -2)
		Me.PictureBox2.Name = "PictureBox2"
		Me.PictureBox2.Size = New System.Drawing.Size(16, 16)
		Me.PictureBox2.TabIndex = 1
		Me.PictureBox2.TabStop = False
		'
		'lblHelpa
		'
		Me.lblHelpa.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblHelpa.Location = New System.Drawing.Point(3, 17)
		Me.lblHelpa.Name = "lblHelpa"
		Me.lblHelpa.Size = New System.Drawing.Size(675, 66)
		Me.lblHelpa.TabIndex = 0
		'
		'dgvSources
		'
		Me.dgvSources.AllowUserToAddRows = False
		Me.dgvSources.AllowUserToDeleteRows = False
		Me.dgvSources.AllowUserToResizeColumns = False
		Me.dgvSources.AllowUserToResizeRows = False
		Me.dgvSources.BackgroundColor = System.Drawing.Color.White
		Me.dgvSources.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvSources.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.export, Me.EmberSource, Me.Column1, Me.Value, Me.sourcetype})
		Me.dgvSources.Location = New System.Drawing.Point(370, 86)
		Me.dgvSources.MultiSelect = False
		Me.dgvSources.Name = "dgvSources"
		Me.dgvSources.RowHeadersVisible = False
		Me.dgvSources.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvSources.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
		Me.dgvSources.ShowCellErrors = False
		Me.dgvSources.ShowCellToolTips = False
		Me.dgvSources.ShowRowErrors = False
		Me.dgvSources.Size = New System.Drawing.Size(472, 168)
		Me.dgvSources.TabIndex = 10
		'
		'export
		'
		DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		DataGridViewCellStyle1.NullValue = False
		DataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White
		DataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black
		Me.export.DefaultCellStyle = DataGridViewCellStyle1
		Me.export.FillWeight = 20.0!
		Me.export.HeaderText = ""
		Me.export.Name = "export"
		Me.export.Width = 20
		'
		'EmberSource
		'
		DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White
		DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black
		Me.EmberSource.DefaultCellStyle = DataGridViewCellStyle2
		Me.EmberSource.FillWeight = 130.0!
		Me.EmberSource.HeaderText = "Ember Source"
		Me.EmberSource.Name = "EmberSource"
		Me.EmberSource.ReadOnly = True
		Me.EmberSource.Width = 130
		'
		'Column1
		'
		Me.Column1.FillWeight = 18.0!
		Me.Column1.HeaderText = ""
		Me.Column1.Name = "Column1"
		Me.Column1.Width = 18
		'
		'Value
		'
		DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
		Me.Value.DefaultCellStyle = DataGridViewCellStyle3
		Me.Value.FillWeight = 280.0!
		Me.Value.HeaderText = "NMT Path"
		Me.Value.Name = "Value"
		Me.Value.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
		Me.Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
		Me.Value.Width = 280
		'
		'sourcetype
		'
		Me.sourcetype.HeaderText = ""
		Me.sourcetype.Name = "sourcetype"
		Me.sourcetype.Visible = False
		'
		'dgvSettings
		'
		Me.dgvSettings.AllowUserToAddRows = False
		Me.dgvSettings.AllowUserToDeleteRows = False
		Me.dgvSettings.AllowUserToResizeColumns = False
		Me.dgvSettings.AllowUserToResizeRows = False
		Me.dgvSettings.BackgroundColor = System.Drawing.Color.White
		Me.dgvSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvSettings.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Setting, Me.DataGridViewComboBoxColumn1})
		Me.dgvSettings.Location = New System.Drawing.Point(0, 2)
		Me.dgvSettings.MultiSelect = False
		Me.dgvSettings.Name = "dgvSettings"
		Me.dgvSettings.RowHeadersVisible = False
		Me.dgvSettings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvSettings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
		Me.dgvSettings.ShowCellErrors = False
		Me.dgvSettings.ShowCellToolTips = False
		Me.dgvSettings.ShowRowErrors = False
		Me.dgvSettings.Size = New System.Drawing.Size(347, 166)
		Me.dgvSettings.TabIndex = 0
		'
		'Setting
		'
		DataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White
		DataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black
		Me.Setting.DefaultCellStyle = DataGridViewCellStyle4
		Me.Setting.FillWeight = 145.0!
		Me.Setting.HeaderText = "Setting"
		Me.Setting.Name = "Setting"
		Me.Setting.ReadOnly = True
		Me.Setting.Width = 145
		'
		'DataGridViewComboBoxColumn1
		'
		DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		Me.DataGridViewComboBoxColumn1.DefaultCellStyle = DataGridViewCellStyle5
		Me.DataGridViewComboBoxColumn1.FillWeight = 180.0!
		Me.DataGridViewComboBoxColumn1.HeaderText = "Value"
		Me.DataGridViewComboBoxColumn1.Name = "DataGridViewComboBoxColumn1"
		Me.DataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
		Me.DataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
		Me.DataGridViewComboBoxColumn1.Width = 180
		'
		'ValidatedToBuild
		'
		Me.ValidatedToBuild.Interval = 300
		'
		'lblTemplateInfo
		'
		Me.lblTemplateInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblTemplateInfo.Location = New System.Drawing.Point(458, 3)
		Me.lblTemplateInfo.Name = "lblTemplateInfo"
		Me.lblTemplateInfo.Size = New System.Drawing.Size(384, 72)
		Me.lblTemplateInfo.TabIndex = 8
		'
		'btnBrowse
		'
		Me.btnBrowse.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnBrowse.Location = New System.Drawing.Point(283, 28)
		Me.btnBrowse.Name = "btnBrowse"
		Me.btnBrowse.Size = New System.Drawing.Size(26, 23)
		Me.btnBrowse.TabIndex = 7
		Me.btnBrowse.Text = "..."
		Me.btnBrowse.UseVisualStyleBackColor = True
		'
		'btnSave
		'
		Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnSave.Enabled = False
		Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnSave.Location = New System.Drawing.Point(705, 263)
		Me.btnSave.Name = "btnSave"
		Me.btnSave.Size = New System.Drawing.Size(136, 23)
		Me.btnSave.TabIndex = 12
		Me.btnSave.Text = "Save Settings"
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage2)
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Location = New System.Drawing.Point(7, 64)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(357, 194)
		Me.TabControl1.TabIndex = 9
		'
		'TabPage2
		'
		Me.TabPage2.Controls.Add(Me.dgvProperties)
		Me.TabPage2.Location = New System.Drawing.Point(4, 22)
		Me.TabPage2.Name = "TabPage2"
		Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage2.Size = New System.Drawing.Size(349, 168)
		Me.TabPage2.TabIndex = 1
		Me.TabPage2.Text = "Skin Properties"
		Me.TabPage2.UseVisualStyleBackColor = True
		'
		'dgvProperties
		'
		Me.dgvProperties.AllowUserToAddRows = False
		Me.dgvProperties.AllowUserToDeleteRows = False
		Me.dgvProperties.AllowUserToResizeColumns = False
		Me.dgvProperties.AllowUserToResizeRows = False
		Me.dgvProperties.BackgroundColor = System.Drawing.Color.White
		Me.dgvProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvProperties.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2})
		Me.dgvProperties.Location = New System.Drawing.Point(1, 1)
		Me.dgvProperties.MultiSelect = False
		Me.dgvProperties.Name = "dgvProperties"
		Me.dgvProperties.RowHeadersVisible = False
		Me.dgvProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvProperties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
		Me.dgvProperties.ShowCellErrors = False
		Me.dgvProperties.ShowCellToolTips = False
		Me.dgvProperties.ShowRowErrors = False
		Me.dgvProperties.Size = New System.Drawing.Size(347, 166)
		Me.dgvProperties.TabIndex = 0
		'
		'DataGridViewTextBoxColumn1
		'
		DataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.White
		DataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black
		Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle6
		Me.DataGridViewTextBoxColumn1.FillWeight = 225.0!
		Me.DataGridViewTextBoxColumn1.HeaderText = "Property"
		Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
		Me.DataGridViewTextBoxColumn1.ReadOnly = True
		Me.DataGridViewTextBoxColumn1.Width = 225
		'
		'DataGridViewTextBoxColumn2
		'
		DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		Me.DataGridViewTextBoxColumn2.DefaultCellStyle = DataGridViewCellStyle7
		Me.DataGridViewTextBoxColumn2.HeaderText = "Value"
		Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
		Me.DataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
		Me.DataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.dgvSettings)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage1.Size = New System.Drawing.Size(349, 168)
		Me.TabPage1.TabIndex = 0
		Me.TabPage1.Text = "Template Settings"
		Me.TabPage1.UseVisualStyleBackColor = True
		'
		'pbTemplateLogo
		'
		Me.pbTemplateLogo.Location = New System.Drawing.Point(324, 5)
		Me.pbTemplateLogo.Name = "pbTemplateLogo"
		Me.pbTemplateLogo.Size = New System.Drawing.Size(130, 70)
		Me.pbTemplateLogo.TabIndex = 94
		Me.pbTemplateLogo.TabStop = False
		'
		'Panel1
		'
		Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Panel1.Controls.Add(Me.lblWarning)
		Me.Panel1.Controls.Add(Me.pbWarning)
		Me.Panel1.Location = New System.Drawing.Point(7, 261)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(692, 28)
		Me.Panel1.TabIndex = 11
		'
		'lblWarning
		'
		Me.lblWarning.Location = New System.Drawing.Point(25, 4)
		Me.lblWarning.Name = "lblWarning"
		Me.lblWarning.Size = New System.Drawing.Size(654, 18)
		Me.lblWarning.TabIndex = 0
		Me.lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'pbWarning
		'
		Me.pbWarning.Location = New System.Drawing.Point(3, 5)
		Me.pbWarning.Name = "pbWarning"
		Me.pbWarning.Size = New System.Drawing.Size(18, 18)
		Me.pbWarning.TabIndex = 0
		Me.pbWarning.TabStop = False
		'
		'ilNMT
		'
		Me.ilNMT.ImageStream = CType(resources.GetObject("ilNMT.ImageStream"), System.Windows.Forms.ImageListStreamer)
		Me.ilNMT.TransparentColor = System.Drawing.Color.Transparent
		Me.ilNMT.Images.SetKeyName(0, "green")
		Me.ilNMT.Images.SetKeyName(1, "red")
		Me.ilNMT.Images.SetKeyName(2, "block")
		'
		'chHighPriority
		'
		Me.chHighPriority.Location = New System.Drawing.Point(705, 346)
		Me.chHighPriority.Name = "chHighPriority"
		Me.chHighPriority.Size = New System.Drawing.Size(129, 18)
		Me.chHighPriority.TabIndex = 14
		Me.chHighPriority.Text = "High Priority"
		Me.chHighPriority.UseVisualStyleBackColor = True
		'
		'pbHelp
		'
		Me.pbHelp.Image = CType(resources.GetObject("pbHelp.Image"), System.Drawing.Image)
		Me.pbHelp.Location = New System.Drawing.Point(824, 5)
		Me.pbHelp.Name = "pbHelp"
		Me.pbHelp.Size = New System.Drawing.Size(16, 16)
		Me.pbHelp.TabIndex = 97
		Me.pbHelp.TabStop = False
		'
		'dlgNMTMovies
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.AutoScroll = True
		Me.CancelButton = Me.Close_Button
		Me.ClientSize = New System.Drawing.Size(853, 396)
		Me.Controls.Add(Me.pbHelp)
		Me.Controls.Add(Me.chHighPriority)
		Me.Controls.Add(Me.pbTemplateLogo)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.pnlCancel)
		Me.Controls.Add(Me.TabControl1)
		Me.Controls.Add(Me.btnSave)
		Me.Controls.Add(Me.btnBrowse)
		Me.Controls.Add(Me.lblTemplateInfo)
		Me.Controls.Add(Me.dgvSources)
		Me.Controls.Add(Me.Panel2)
		Me.Controls.Add(Me.txtOutputFolder)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.btnBuild)
		Me.Controls.Add(Me.Close_Button)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.cbTemplate)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgNMTMovies"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "NMT Jukebox Builder"
		Me.pnlCancel.ResumeLayout(False)
		Me.Panel2.ResumeLayout(False)
		Me.gbHelp.ResumeLayout(False)
		CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.dgvSources, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.dgvSettings, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage2.ResumeLayout(False)
		CType(Me.dgvProperties, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage1.ResumeLayout(False)
		CType(Me.pbTemplateLogo, System.ComponentModel.ISupportInitialize).EndInit()
		Me.Panel1.ResumeLayout(False)
		CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbHelp, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents cbTemplate As System.Windows.Forms.ComboBox
    Friend WithEvents btnBuild As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtOutputFolder As System.Windows.Forms.TextBox
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents gbHelp As System.Windows.Forms.GroupBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents lblHelpa As System.Windows.Forms.Label
    Friend WithEvents dgvSources As System.Windows.Forms.DataGridView
    Friend WithEvents dgvSettings As System.Windows.Forms.DataGridView
    Friend WithEvents ValidatedToBuild As System.Windows.Forms.Timer
    Friend WithEvents lblTemplateInfo As System.Windows.Forms.Label
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents export As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents EmberSource As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents Value As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents sourcetype As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Setting As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents dgvProperties As System.Windows.Forms.DataGridView
    Friend WithEvents pbTemplateLogo As System.Windows.Forms.PictureBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents pbWarning As System.Windows.Forms.PictureBox
    Friend WithEvents ilNMT As System.Windows.Forms.ImageList
    Friend WithEvents lblWarning As System.Windows.Forms.Label
    Friend WithEvents chHighPriority As System.Windows.Forms.CheckBox
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents pbHelp As System.Windows.Forms.PictureBox

#End Region 'Methods

End Class