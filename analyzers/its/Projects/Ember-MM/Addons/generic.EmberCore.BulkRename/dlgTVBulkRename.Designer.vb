<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgTVBulkRenamer
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgTVBulkRenamer))
		Me.tsmUnlockMovie = New System.Windows.Forms.ToolStripMenuItem()
		Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
		Me.chkRenamedOnly = New System.Windows.Forms.CheckBox()
		Me.txtFolder = New System.Windows.Forms.TextBox()
		Me.tsmLockAll = New System.Windows.Forms.ToolStripMenuItem()
		Me.lblFolderPattern = New System.Windows.Forms.Label()
		Me.lblFilePattern = New System.Windows.Forms.Label()
		Me.tsmUnlockAll = New System.Windows.Forms.ToolStripMenuItem()
		Me.txtFile = New System.Windows.Forms.TextBox()
		Me.tsmLockMovie = New System.Windows.Forms.ToolStripMenuItem()
		Me.dgvMoviesList = New System.Windows.Forms.DataGridView()
		Me.cmsMovieList = New System.Windows.Forms.ContextMenuStrip(Me.components)
		Me.Label2 = New System.Windows.Forms.Label()
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.Close_Button = New System.Windows.Forms.Button()
		Me.pnlCancel = New System.Windows.Forms.Panel()
		Me.pbCompile = New System.Windows.Forms.ProgressBar()
		Me.lblCompiling = New System.Windows.Forms.Label()
		Me.lblFile = New System.Windows.Forms.Label()
		Me.lblCanceling = New System.Windows.Forms.Label()
		Me.btnCancel = New System.Windows.Forms.Button()
		Me.tmrSimul = New System.Windows.Forms.Timer(Me.components)
		Me.Rename_Button = New System.Windows.Forms.Button()
		Me.Button2 = New System.Windows.Forms.Button()
		Me.Button1 = New System.Windows.Forms.Button()
		CType(Me.dgvMoviesList, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.cmsMovieList.SuspendLayout()
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.pnlCancel.SuspendLayout()
		Me.SuspendLayout()
		'
		'tsmUnlockMovie
		'
		Me.tsmUnlockMovie.Name = "tsmUnlockMovie"
		Me.tsmUnlockMovie.Size = New System.Drawing.Size(173, 22)
		Me.tsmUnlockMovie.Text = "Unlock TV Show(s)"
		'
		'ToolStripSeparator1
		'
		Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
		Me.ToolStripSeparator1.Size = New System.Drawing.Size(170, 6)
		'
		'chkRenamedOnly
		'
		Me.chkRenamedOnly.AutoSize = True
		Me.chkRenamedOnly.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.chkRenamedOnly.Location = New System.Drawing.Point(668, 442)
		Me.chkRenamedOnly.Name = "chkRenamedOnly"
		Me.chkRenamedOnly.Size = New System.Drawing.Size(231, 17)
		Me.chkRenamedOnly.TabIndex = 11
		Me.chkRenamedOnly.Text = "Display Only Files That Will Be Renamed"
		Me.chkRenamedOnly.UseVisualStyleBackColor = True
		'
		'txtFolder
		'
		Me.txtFolder.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtFolder.Location = New System.Drawing.Point(316, 416)
		Me.txtFolder.Name = "txtFolder"
		Me.txtFolder.Size = New System.Drawing.Size(200, 22)
		Me.txtFolder.TabIndex = 6
		Me.txtFolder.Text = "$T ($Y)"
		'
		'tsmLockAll
		'
		Me.tsmLockAll.Name = "tsmLockAll"
		Me.tsmLockAll.Size = New System.Drawing.Size(173, 22)
		Me.tsmLockAll.Text = "Lock All"
		'
		'lblFolderPattern
		'
		Me.lblFolderPattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblFolderPattern.Location = New System.Drawing.Point(12, 419)
		Me.lblFolderPattern.Name = "lblFolderPattern"
		Me.lblFolderPattern.Size = New System.Drawing.Size(298, 13)
		Me.lblFolderPattern.TabIndex = 5
		Me.lblFolderPattern.Text = "Folder Pattern"
		Me.lblFolderPattern.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'lblFilePattern
		'
		Me.lblFilePattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblFilePattern.Location = New System.Drawing.Point(522, 419)
		Me.lblFilePattern.Name = "lblFilePattern"
		Me.lblFilePattern.Size = New System.Drawing.Size(140, 13)
		Me.lblFilePattern.TabIndex = 8
		Me.lblFilePattern.Text = "File Pattern"
		Me.lblFilePattern.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'tsmUnlockAll
		'
		Me.tsmUnlockAll.Name = "tsmUnlockAll"
		Me.tsmUnlockAll.Size = New System.Drawing.Size(173, 22)
		Me.tsmUnlockAll.Text = "Unlock All"
		'
		'txtFile
		'
		Me.txtFile.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtFile.Location = New System.Drawing.Point(668, 416)
		Me.txtFile.Name = "txtFile"
		Me.txtFile.Size = New System.Drawing.Size(224, 22)
		Me.txtFile.TabIndex = 9
		Me.txtFile.Text = "$T"
		'
		'tsmLockMovie
		'
		Me.tsmLockMovie.Name = "tsmLockMovie"
		Me.tsmLockMovie.Size = New System.Drawing.Size(173, 22)
		Me.tsmLockMovie.Text = "Lock TV Show(s)"
		'
		'dgvMoviesList
		'
		Me.dgvMoviesList.AllowUserToAddRows = False
		Me.dgvMoviesList.AllowUserToDeleteRows = False
		Me.dgvMoviesList.AllowUserToResizeRows = False
		Me.dgvMoviesList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
				  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.dgvMoviesList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvMoviesList.ContextMenuStrip = Me.cmsMovieList
		Me.dgvMoviesList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
		Me.dgvMoviesList.Location = New System.Drawing.Point(12, 94)
		Me.dgvMoviesList.Name = "dgvMoviesList"
		Me.dgvMoviesList.RowHeadersVisible = False
		Me.dgvMoviesList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.dgvMoviesList.ShowEditingIcon = False
		Me.dgvMoviesList.Size = New System.Drawing.Size(966, 316)
		Me.dgvMoviesList.TabIndex = 3
		'
		'cmsMovieList
		'
		Me.cmsMovieList.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsmLockMovie, Me.tsmUnlockMovie, Me.ToolStripSeparator1, Me.tsmLockAll, Me.tsmUnlockAll})
		Me.cmsMovieList.Name = "cmsMovieList"
		Me.cmsMovieList.Size = New System.Drawing.Size(174, 98)
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.BackColor = System.Drawing.Color.Transparent
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.ForeColor = System.Drawing.Color.White
		Me.Label2.Location = New System.Drawing.Point(64, 38)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(149, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Rename TV Shows and files"
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
		Me.pnlTop.Size = New System.Drawing.Size(1000, 64)
		Me.pnlTop.TabIndex = 2
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.BackColor = System.Drawing.Color.Transparent
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.ForeColor = System.Drawing.Color.White
		Me.Label4.Location = New System.Drawing.Point(61, 3)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(211, 32)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "TV Bulk Renamer"
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
		'Close_Button
		'
		Me.Close_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.Close_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Close_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Close_Button.Location = New System.Drawing.Point(898, 463)
		Me.Close_Button.Name = "Close_Button"
		Me.Close_Button.Size = New System.Drawing.Size(80, 23)
		Me.Close_Button.TabIndex = 0
		Me.Close_Button.Text = "Close"
		'
		'pnlCancel
		'
		Me.pnlCancel.BackColor = System.Drawing.Color.LightGray
		Me.pnlCancel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlCancel.Controls.Add(Me.pbCompile)
		Me.pnlCancel.Controls.Add(Me.lblCompiling)
		Me.pnlCancel.Controls.Add(Me.lblFile)
		Me.pnlCancel.Controls.Add(Me.lblCanceling)
		Me.pnlCancel.Controls.Add(Me.btnCancel)
		Me.pnlCancel.Location = New System.Drawing.Point(295, 214)
		Me.pnlCancel.Name = "pnlCancel"
		Me.pnlCancel.Size = New System.Drawing.Size(403, 76)
		Me.pnlCancel.TabIndex = 4
		Me.pnlCancel.Visible = False
		'
		'pbCompile
		'
		Me.pbCompile.Location = New System.Drawing.Point(8, 36)
		Me.pbCompile.MarqueeAnimationSpeed = 25
		Me.pbCompile.Name = "pbCompile"
		Me.pbCompile.Size = New System.Drawing.Size(388, 18)
		Me.pbCompile.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbCompile.TabIndex = 3
		'
		'lblCompiling
		'
		Me.lblCompiling.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCompiling.Location = New System.Drawing.Point(3, 12)
		Me.lblCompiling.Name = "lblCompiling"
		Me.lblCompiling.Size = New System.Drawing.Size(186, 20)
		Me.lblCompiling.TabIndex = 0
		Me.lblCompiling.Text = "Compiling Tv Shows List..."
		Me.lblCompiling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCompiling.Visible = False
		'
		'lblFile
		'
		Me.lblFile.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblFile.Location = New System.Drawing.Point(6, 57)
		Me.lblFile.Name = "lblFile"
		Me.lblFile.Size = New System.Drawing.Size(390, 13)
		Me.lblFile.TabIndex = 4
		Me.lblFile.Text = "File ..."
		'
		'lblCanceling
		'
		Me.lblCanceling.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCanceling.Location = New System.Drawing.Point(110, 12)
		Me.lblCanceling.Name = "lblCanceling"
		Me.lblCanceling.Size = New System.Drawing.Size(186, 20)
		Me.lblCanceling.TabIndex = 1
		Me.lblCanceling.Text = "Canceling Compilation..."
		Me.lblCanceling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCanceling.Visible = False
		'
		'btnCancel
		'
		Me.btnCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		'tmrSimul
		'
		Me.tmrSimul.Interval = 250
		'
		'Rename_Button
		'
		Me.Rename_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
		Me.Rename_Button.Enabled = False
		Me.Rename_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Rename_Button.Location = New System.Drawing.Point(812, 463)
		Me.Rename_Button.Name = "Rename_Button"
		Me.Rename_Button.Size = New System.Drawing.Size(80, 23)
		Me.Rename_Button.TabIndex = 1
		Me.Rename_Button.Text = "Rename"
		'
		'Button2
		'
		Me.Button2.Image = CType(resources.GetObject("Button2.Image"), System.Drawing.Image)
		Me.Button2.Location = New System.Drawing.Point(895, 416)
		Me.Button2.Name = "Button2"
		Me.Button2.Size = New System.Drawing.Size(17, 19)
		Me.Button2.TabIndex = 10
		Me.Button2.UseVisualStyleBackColor = True
		'
		'Button1
		'
		Me.Button1.Image = CType(resources.GetObject("Button1.Image"), System.Drawing.Image)
		Me.Button1.Location = New System.Drawing.Point(517, 418)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(17, 19)
		Me.Button1.TabIndex = 7
		Me.Button1.UseVisualStyleBackColor = True
		'
		'dlgTVBulkRenamer
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(1000, 505)
		Me.Controls.Add(Me.Button2)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.chkRenamedOnly)
		Me.Controls.Add(Me.txtFolder)
		Me.Controls.Add(Me.lblFolderPattern)
		Me.Controls.Add(Me.lblFilePattern)
		Me.Controls.Add(Me.txtFile)
		Me.Controls.Add(Me.pnlTop)
		Me.Controls.Add(Me.Close_Button)
		Me.Controls.Add(Me.pnlCancel)
		Me.Controls.Add(Me.Rename_Button)
		Me.Controls.Add(Me.dgvMoviesList)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgTVBulkRenamer"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "TV BulkRename"
		CType(Me.dgvMoviesList, System.ComponentModel.ISupportInitialize).EndInit()
		Me.cmsMovieList.ResumeLayout(False)
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.pnlCancel.ResumeLayout(False)
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents tsmUnlockMovie As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents chkRenamedOnly As System.Windows.Forms.CheckBox
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents tsmLockAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblFolderPattern As System.Windows.Forms.Label
    Friend WithEvents lblFilePattern As System.Windows.Forms.Label
    Friend WithEvents tsmUnlockAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents txtFile As System.Windows.Forms.TextBox
    Friend WithEvents tsmLockMovie As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents dgvMoviesList As System.Windows.Forms.DataGridView
    Friend WithEvents cmsMovieList As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents Close_Button As System.Windows.Forms.Button
    Friend WithEvents pnlCancel As System.Windows.Forms.Panel
    Friend WithEvents pbCompile As System.Windows.Forms.ProgressBar
    Friend WithEvents lblCompiling As System.Windows.Forms.Label
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents lblCanceling As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents tmrSimul As System.Windows.Forms.Timer
    Friend WithEvents Rename_Button As System.Windows.Forms.Button

End Class
