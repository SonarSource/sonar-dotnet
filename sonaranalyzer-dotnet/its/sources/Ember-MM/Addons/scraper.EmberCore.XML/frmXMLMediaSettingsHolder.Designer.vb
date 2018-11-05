<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmXMLMediaSettingsHolder
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmXMLMediaSettingsHolder))
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.btnDown = New System.Windows.Forms.Button()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.btnUp = New System.Windows.Forms.Button()
		Me.pnlSettings.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.Label1)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(12, 1)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 369)
		Me.pnlSettings.TabIndex = 0
		'
		'Label1
		'
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label1.Location = New System.Drawing.Point(39, 52)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(519, 20)
		Me.Label1.TabIndex = 1
		Me.Label1.Text = "Use the Settings in Scrapers - Data"
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
		'frmXMLMediaSettingsHolder
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
		Me.Name = "frmXMLMediaSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Setup"
		Me.pnlSettings.ResumeLayout(False)
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

End Class
