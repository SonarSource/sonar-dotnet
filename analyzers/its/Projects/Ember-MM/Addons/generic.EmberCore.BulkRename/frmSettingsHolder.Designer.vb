<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSettingsHolder
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
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.gbRenamerPatterns = New System.Windows.Forms.GroupBox()
		Me.chkRenameSingle = New System.Windows.Forms.CheckBox()
		Me.chkRenameMulti = New System.Windows.Forms.CheckBox()
		Me.lblFilePattern = New System.Windows.Forms.Label()
		Me.lblFolderPattern = New System.Windows.Forms.Label()
		Me.txtFilePattern = New System.Windows.Forms.TextBox()
		Me.txtFolderPattern = New System.Windows.Forms.TextBox()
		Me.chkBulkRenamer = New System.Windows.Forms.CheckBox()
		Me.chkGenericModule = New System.Windows.Forms.CheckBox()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.chkEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings.SuspendLayout()
		Me.gbRenamerPatterns.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.BackColor = System.Drawing.Color.White
		Me.pnlSettings.Controls.Add(Me.Label1)
		Me.pnlSettings.Controls.Add(Me.gbRenamerPatterns)
		Me.pnlSettings.Controls.Add(Me.chkBulkRenamer)
		Me.pnlSettings.Controls.Add(Me.chkGenericModule)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(13, 15)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 84
		'
		'Label1
		'
		Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.Label1.Location = New System.Drawing.Point(224, 54)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(372, 263)
		Me.Label1.TabIndex = 4
		Me.Label1.Text = "Label1"
		'
		'gbRenamerPatterns
		'
		Me.gbRenamerPatterns.Controls.Add(Me.chkRenameSingle)
		Me.gbRenamerPatterns.Controls.Add(Me.chkRenameMulti)
		Me.gbRenamerPatterns.Controls.Add(Me.lblFilePattern)
		Me.gbRenamerPatterns.Controls.Add(Me.lblFolderPattern)
		Me.gbRenamerPatterns.Controls.Add(Me.txtFilePattern)
		Me.gbRenamerPatterns.Controls.Add(Me.txtFolderPattern)
		Me.gbRenamerPatterns.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.gbRenamerPatterns.Location = New System.Drawing.Point(21, 54)
		Me.gbRenamerPatterns.Name = "gbRenamerPatterns"
		Me.gbRenamerPatterns.Size = New System.Drawing.Size(197, 191)
		Me.gbRenamerPatterns.TabIndex = 3
		Me.gbRenamerPatterns.TabStop = False
		Me.gbRenamerPatterns.Text = "Default Renaming Patterns"
		'
		'chkRenameSingle
		'
		Me.chkRenameSingle.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkRenameSingle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRenameSingle.Location = New System.Drawing.Point(8, 146)
		Me.chkRenameSingle.Name = "chkRenameSingle"
		Me.chkRenameSingle.Size = New System.Drawing.Size(176, 30)
		Me.chkRenameSingle.TabIndex = 5
		Me.chkRenameSingle.Text = "Automatically Rename Files During Single-Scraper"
		Me.chkRenameSingle.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkRenameSingle.UseVisualStyleBackColor = True
		'
		'chkRenameMulti
		'
		Me.chkRenameMulti.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkRenameMulti.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRenameMulti.Location = New System.Drawing.Point(8, 108)
		Me.chkRenameMulti.Name = "chkRenameMulti"
		Me.chkRenameMulti.Size = New System.Drawing.Size(179, 30)
		Me.chkRenameMulti.TabIndex = 4
		Me.chkRenameMulti.Text = "Automatically Rename Files During Multi-Scraper"
		Me.chkRenameMulti.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkRenameMulti.UseVisualStyleBackColor = True
		'
		'lblFilePattern
		'
		Me.lblFilePattern.AutoSize = True
		Me.lblFilePattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblFilePattern.Location = New System.Drawing.Point(7, 56)
		Me.lblFilePattern.Name = "lblFilePattern"
		Me.lblFilePattern.Size = New System.Drawing.Size(70, 13)
		Me.lblFilePattern.TabIndex = 2
		Me.lblFilePattern.Text = "Files Pattern"
		'
		'lblFolderPattern
		'
		Me.lblFolderPattern.AutoSize = True
		Me.lblFolderPattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblFolderPattern.Location = New System.Drawing.Point(6, 17)
		Me.lblFolderPattern.Name = "lblFolderPattern"
		Me.lblFolderPattern.Size = New System.Drawing.Size(85, 13)
		Me.lblFolderPattern.TabIndex = 0
		Me.lblFolderPattern.Text = "Folders Pattern"
		'
		'txtFilePattern
		'
		Me.txtFilePattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtFilePattern.Location = New System.Drawing.Point(8, 71)
		Me.txtFilePattern.Name = "txtFilePattern"
		Me.txtFilePattern.Size = New System.Drawing.Size(176, 22)
		Me.txtFilePattern.TabIndex = 3
		'
		'txtFolderPattern
		'
		Me.txtFolderPattern.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtFolderPattern.Location = New System.Drawing.Point(8, 32)
		Me.txtFolderPattern.Name = "txtFolderPattern"
		Me.txtFolderPattern.Size = New System.Drawing.Size(177, 22)
		Me.txtFolderPattern.TabIndex = 1
		'
		'chkBulkRenamer
		'
		Me.chkBulkRenamer.AutoSize = True
		Me.chkBulkRenamer.Location = New System.Drawing.Point(391, 31)
		Me.chkBulkRenamer.Name = "chkBulkRenamer"
		Me.chkBulkRenamer.Size = New System.Drawing.Size(160, 17)
		Me.chkBulkRenamer.TabIndex = 2
		Me.chkBulkRenamer.Text = "Enable Bulk Renamer Tool"
		Me.chkBulkRenamer.UseVisualStyleBackColor = True
		'
		'chkGenericModule
		'
		Me.chkGenericModule.AutoSize = True
		Me.chkGenericModule.Location = New System.Drawing.Point(10, 31)
		Me.chkGenericModule.Name = "chkGenericModule"
		Me.chkGenericModule.Size = New System.Drawing.Size(190, 17)
		Me.chkGenericModule.TabIndex = 1
		Me.chkGenericModule.Text = "Enable Generic Rename Module"
		Me.chkGenericModule.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.chkEnabled)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(617, 25)
		Me.Panel1.TabIndex = 0
		'
		'chkEnabled
		'
		Me.chkEnabled.AutoSize = True
		Me.chkEnabled.Location = New System.Drawing.Point(10, 5)
		Me.chkEnabled.Name = "chkEnabled"
		Me.chkEnabled.Size = New System.Drawing.Size(68, 17)
		Me.chkEnabled.TabIndex = 0
		Me.chkEnabled.Text = "Enabled"
		Me.chkEnabled.UseVisualStyleBackColor = True
		'
		'frmSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(643, 356)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Name = "frmSettingsHolder"
		Me.Text = "frmSettingsHolder"
		Me.pnlSettings.ResumeLayout(False)
		Me.pnlSettings.PerformLayout()
		Me.gbRenamerPatterns.ResumeLayout(False)
		Me.gbRenamerPatterns.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents chkEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents chkBulkRenamer As System.Windows.Forms.CheckBox
    Friend WithEvents chkGenericModule As System.Windows.Forms.CheckBox
    Friend WithEvents gbRenamerPatterns As System.Windows.Forms.GroupBox
    Friend WithEvents chkRenameSingle As System.Windows.Forms.CheckBox
    Friend WithEvents chkRenameMulti As System.Windows.Forms.CheckBox
    Friend WithEvents lblFilePattern As System.Windows.Forms.Label
    Friend WithEvents lblFolderPattern As System.Windows.Forms.Label
    Friend WithEvents txtFilePattern As System.Windows.Forms.TextBox
    Friend WithEvents txtFolderPattern As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
