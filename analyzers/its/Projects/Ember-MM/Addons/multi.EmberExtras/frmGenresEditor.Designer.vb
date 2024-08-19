<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGenresEditor
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmGenresEditor))
		Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
		Me.pnlGenres = New System.Windows.Forms.Panel()
		Me.btnRemoveGenre = New System.Windows.Forms.Button()
		Me.btnAddGenre = New System.Windows.Forms.Button()
		Me.btnRemoveLang = New System.Windows.Forms.Button()
		Me.btnAddLang = New System.Windows.Forms.Button()
		Me.cbLangs = New System.Windows.Forms.ComboBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.btnChangeImg = New System.Windows.Forms.Button()
		Me.pbIcon = New System.Windows.Forms.PictureBox()
		Me.dgvGenres = New System.Windows.Forms.DataGridView()
		Me.searchstring = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.dgvLang = New System.Windows.Forms.DataGridView()
		Me.Column1 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
		Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.pnlGenres.SuspendLayout()
		Me.GroupBox1.SuspendLayout()
		CType(Me.pbIcon, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.dgvGenres, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.dgvLang, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'pnlGenres
		'
		Me.pnlGenres.Controls.Add(Me.btnRemoveGenre)
		Me.pnlGenres.Controls.Add(Me.btnAddGenre)
		Me.pnlGenres.Controls.Add(Me.btnRemoveLang)
		Me.pnlGenres.Controls.Add(Me.btnAddLang)
		Me.pnlGenres.Controls.Add(Me.cbLangs)
		Me.pnlGenres.Controls.Add(Me.Label1)
		Me.pnlGenres.Controls.Add(Me.GroupBox1)
		Me.pnlGenres.Controls.Add(Me.dgvGenres)
		Me.pnlGenres.Controls.Add(Me.dgvLang)
		Me.pnlGenres.Location = New System.Drawing.Point(0, 0)
		Me.pnlGenres.Name = "pnlGenres"
		Me.pnlGenres.Size = New System.Drawing.Size(627, 367)
		Me.pnlGenres.TabIndex = 0
		'
		'btnRemoveGenre
		'
		Me.btnRemoveGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnRemoveGenre.Image = CType(resources.GetObject("btnRemoveGenre.Image"), System.Drawing.Image)
		Me.btnRemoveGenre.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnRemoveGenre.Location = New System.Drawing.Point(114, 248)
		Me.btnRemoveGenre.Name = "btnRemoveGenre"
		Me.btnRemoveGenre.Size = New System.Drawing.Size(72, 23)
		Me.btnRemoveGenre.TabIndex = 4
		Me.btnRemoveGenre.Text = "Remove"
		Me.btnRemoveGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnRemoveGenre.UseVisualStyleBackColor = True
		'
		'btnAddGenre
		'
		Me.btnAddGenre.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnAddGenre.Image = CType(resources.GetObject("btnAddGenre.Image"), System.Drawing.Image)
		Me.btnAddGenre.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnAddGenre.Location = New System.Drawing.Point(22, 248)
		Me.btnAddGenre.Name = "btnAddGenre"
		Me.btnAddGenre.Size = New System.Drawing.Size(72, 23)
		Me.btnAddGenre.TabIndex = 3
		Me.btnAddGenre.Text = "Add"
		Me.btnAddGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnAddGenre.UseVisualStyleBackColor = True
		'
		'btnRemoveLang
		'
		Me.btnRemoveLang.Enabled = False
		Me.btnRemoveLang.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnRemoveLang.Image = CType(resources.GetObject("btnRemoveLang.Image"), System.Drawing.Image)
		Me.btnRemoveLang.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnRemoveLang.Location = New System.Drawing.Point(331, 248)
		Me.btnRemoveLang.Name = "btnRemoveLang"
		Me.btnRemoveLang.Size = New System.Drawing.Size(72, 23)
		Me.btnRemoveLang.TabIndex = 7
		Me.btnRemoveLang.Text = "Remove"
		Me.btnRemoveLang.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnRemoveLang.UseVisualStyleBackColor = True
		'
		'btnAddLang
		'
		Me.btnAddLang.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnAddLang.Image = CType(resources.GetObject("btnAddLang.Image"), System.Drawing.Image)
		Me.btnAddLang.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnAddLang.Location = New System.Drawing.Point(239, 248)
		Me.btnAddLang.Name = "btnAddLang"
		Me.btnAddLang.Size = New System.Drawing.Size(72, 23)
		Me.btnAddLang.TabIndex = 6
		Me.btnAddLang.Text = "Add"
		Me.btnAddLang.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnAddLang.UseVisualStyleBackColor = True
		'
		'cbLangs
		'
		Me.cbLangs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbLangs.FormattingEnabled = True
		Me.cbLangs.Location = New System.Drawing.Point(8, 25)
		Me.cbLangs.Name = "cbLangs"
		Me.cbLangs.Size = New System.Drawing.Size(188, 21)
		Me.cbLangs.TabIndex = 1
		'
		'Label1
		'
		Me.Label1.Location = New System.Drawing.Point(9, 9)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(90, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Genres Filter"
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.btnChangeImg)
		Me.GroupBox1.Controls.Add(Me.pbIcon)
		Me.GroupBox1.Location = New System.Drawing.Point(431, 49)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(180, 195)
		Me.GroupBox1.TabIndex = 8
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Image"
		'
		'btnChangeImg
		'
		Me.btnChangeImg.Enabled = False
		Me.btnChangeImg.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnChangeImg.Image = Global.multi.EmberExtras.My.Resources.Resources.image
		Me.btnChangeImg.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnChangeImg.Location = New System.Drawing.Point(87, 19)
		Me.btnChangeImg.Name = "btnChangeImg"
		Me.btnChangeImg.Size = New System.Drawing.Size(81, 23)
		Me.btnChangeImg.TabIndex = 0
		Me.btnChangeImg.Text = "Change"
		Me.btnChangeImg.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnChangeImg.UseVisualStyleBackColor = True
		'
		'pbIcon
		'
		Me.pbIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbIcon.Location = New System.Drawing.Point(8, 19)
		Me.pbIcon.Name = "pbIcon"
		Me.pbIcon.Size = New System.Drawing.Size(68, 102)
		Me.pbIcon.TabIndex = 6
		Me.pbIcon.TabStop = False
		'
		'dgvGenres
		'
		Me.dgvGenres.AllowUserToAddRows = False
		Me.dgvGenres.AllowUserToDeleteRows = False
		Me.dgvGenres.AllowUserToResizeColumns = False
		Me.dgvGenres.AllowUserToResizeRows = False
		Me.dgvGenres.BackgroundColor = System.Drawing.Color.White
		Me.dgvGenres.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvGenres.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.searchstring})
		Me.dgvGenres.Location = New System.Drawing.Point(7, 52)
		Me.dgvGenres.MultiSelect = False
		Me.dgvGenres.Name = "dgvGenres"
		Me.dgvGenres.RowHeadersVisible = False
		Me.dgvGenres.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvGenres.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
		Me.dgvGenres.ShowCellErrors = False
		Me.dgvGenres.ShowCellToolTips = False
		Me.dgvGenres.ShowRowErrors = False
		Me.dgvGenres.Size = New System.Drawing.Size(202, 192)
		Me.dgvGenres.TabIndex = 2
		'
		'searchstring
		'
		DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		Me.searchstring.DefaultCellStyle = DataGridViewCellStyle1
		Me.searchstring.FillWeight = 180.0!
		Me.searchstring.HeaderText = "Genre"
		Me.searchstring.Name = "searchstring"
		Me.searchstring.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
		Me.searchstring.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
		Me.searchstring.Width = 180
		'
		'dgvLang
		'
		Me.dgvLang.AllowUserToAddRows = False
		Me.dgvLang.AllowUserToDeleteRows = False
		Me.dgvLang.AllowUserToResizeColumns = False
		Me.dgvLang.AllowUserToResizeRows = False
		Me.dgvLang.BackgroundColor = System.Drawing.Color.White
		Me.dgvLang.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvLang.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.DataGridViewTextBoxColumn1})
		Me.dgvLang.Location = New System.Drawing.Point(239, 52)
		Me.dgvLang.MultiSelect = False
		Me.dgvLang.Name = "dgvLang"
		Me.dgvLang.RowHeadersVisible = False
		Me.dgvLang.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.dgvLang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.dgvLang.ShowCellErrors = False
		Me.dgvLang.ShowCellToolTips = False
		Me.dgvLang.ShowEditingIcon = False
		Me.dgvLang.ShowRowErrors = False
		Me.dgvLang.Size = New System.Drawing.Size(164, 192)
		Me.dgvLang.TabIndex = 5
		'
		'Column1
		'
		DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		DataGridViewCellStyle2.NullValue = False
		DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White
		DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black
		Me.Column1.DefaultCellStyle = DataGridViewCellStyle2
		Me.Column1.FillWeight = 22.0!
		Me.Column1.HeaderText = ""
		Me.Column1.Name = "Column1"
		Me.Column1.Width = 22
		'
		'DataGridViewTextBoxColumn1
		'
		DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
		Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle3
		Me.DataGridViewTextBoxColumn1.FillWeight = 120.0!
		Me.DataGridViewTextBoxColumn1.HeaderText = "Languages"
		Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
		Me.DataGridViewTextBoxColumn1.ReadOnly = True
		Me.DataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
		Me.DataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
		Me.DataGridViewTextBoxColumn1.Width = 120
		'
		'frmGenresEditor
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(628, 366)
		Me.Controls.Add(Me.pnlGenres)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmGenresEditor"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "frmGenresEditor"
		Me.pnlGenres.ResumeLayout(False)
		Me.GroupBox1.ResumeLayout(False)
		CType(Me.pbIcon, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.dgvGenres, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.dgvLang, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlGenres As System.Windows.Forms.Panel
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents btnChangeImg As System.Windows.Forms.Button
    Friend WithEvents pbIcon As System.Windows.Forms.PictureBox
    Friend WithEvents dgvGenres As System.Windows.Forms.DataGridView
    Friend WithEvents dgvLang As System.Windows.Forms.DataGridView
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cbLangs As System.Windows.Forms.ComboBox
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnRemoveGenre As System.Windows.Forms.Button
    Friend WithEvents btnAddGenre As System.Windows.Forms.Button
    Friend WithEvents btnRemoveLang As System.Windows.Forms.Button
    Friend WithEvents btnAddLang As System.Windows.Forms.Button
    Friend WithEvents searchstring As System.Windows.Forms.DataGridViewTextBoxColumn

End Class
