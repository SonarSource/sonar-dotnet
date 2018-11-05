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
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.txtDetails = New System.Windows.Forms.TextBox()
		Me.btnRemove = New System.Windows.Forms.Button()
		Me.btnInstall = New System.Windows.Forms.Button()
		Me.lstTemplates = New System.Windows.Forms.ListView()
		Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.Panel1.SuspendLayout()
		Me.pnlSettings.SuspendLayout()
		Me.GroupBox1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.cbEnabled)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(617, 25)
		Me.Panel1.TabIndex = 0
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
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.GroupBox1)
		Me.pnlSettings.Controls.Add(Me.btnRemove)
		Me.pnlSettings.Controls.Add(Me.btnInstall)
		Me.pnlSettings.Controls.Add(Me.lstTemplates)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(3, 12)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 370)
		Me.pnlSettings.TabIndex = 0
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.txtDetails)
		Me.GroupBox1.Location = New System.Drawing.Point(10, 229)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(600, 133)
		Me.GroupBox1.TabIndex = 4
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Details"
		'
		'txtDetails
		'
		Me.txtDetails.BackColor = System.Drawing.Color.White
		Me.txtDetails.Location = New System.Drawing.Point(6, 12)
		Me.txtDetails.Multiline = True
		Me.txtDetails.Name = "txtDetails"
		Me.txtDetails.ReadOnly = True
		Me.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtDetails.Size = New System.Drawing.Size(588, 121)
		Me.txtDetails.TabIndex = 0
		'
		'btnRemove
		'
		Me.btnRemove.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnRemove.Enabled = False
		Me.btnRemove.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnRemove.Location = New System.Drawing.Point(95, 200)
		Me.btnRemove.Name = "btnRemove"
		Me.btnRemove.Size = New System.Drawing.Size(79, 23)
		Me.btnRemove.TabIndex = 3
		Me.btnRemove.Text = "Remove"
		'
		'btnInstall
		'
		Me.btnInstall.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnInstall.Enabled = False
		Me.btnInstall.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnInstall.Location = New System.Drawing.Point(10, 200)
		Me.btnInstall.Name = "btnInstall"
		Me.btnInstall.Size = New System.Drawing.Size(79, 23)
		Me.btnInstall.TabIndex = 2
		Me.btnInstall.Text = "Install"
		'
		'lstTemplates
		'
		Me.lstTemplates.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4})
		Me.lstTemplates.FullRowSelect = True
		Me.lstTemplates.HideSelection = False
		Me.lstTemplates.Location = New System.Drawing.Point(10, 41)
		Me.lstTemplates.Name = "lstTemplates"
		Me.lstTemplates.Size = New System.Drawing.Size(600, 153)
		Me.lstTemplates.TabIndex = 1
		Me.lstTemplates.UseCompatibleStateImageBehavior = False
		Me.lstTemplates.View = System.Windows.Forms.View.Details
		'
		'ColumnHeader1
		'
		Me.ColumnHeader1.Text = "Template"
		Me.ColumnHeader1.Width = 195
		'
		'ColumnHeader2
		'
		Me.ColumnHeader2.Text = "Version"
		Me.ColumnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
		'
		'ColumnHeader3
		'
		Me.ColumnHeader3.Text = "Author"
		Me.ColumnHeader3.Width = 252
		'
		'ColumnHeader4
		'
		Me.ColumnHeader4.Text = "Status"
		Me.ColumnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
		Me.ColumnHeader4.Width = 69
		'
		'frmSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackColor = System.Drawing.Color.White
		Me.ClientSize = New System.Drawing.Size(625, 386)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "frmSettingsHolder"
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.pnlSettings.ResumeLayout(False)
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents lstTemplates As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents btnInstall As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtDetails As System.Windows.Forms.TextBox

End Class
