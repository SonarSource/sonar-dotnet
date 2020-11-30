<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgImgView
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgImgView))
		Me.pnlBG = New System.Windows.Forms.Panel()
		Me.pbCache = New System.Windows.Forms.PictureBox()
		Me.pbPicture = New System.Windows.Forms.PictureBox()
		Me.tsImgView = New System.Windows.Forms.ToolStrip()
		Me.tsbFit = New System.Windows.Forms.ToolStripButton()
		Me.tsbFull = New System.Windows.Forms.ToolStripButton()
		Me.pnlBG.SuspendLayout()
		CType(Me.pbCache, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbPicture, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.tsImgView.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlBG
		'
		Me.pnlBG.AutoScroll = True
		Me.pnlBG.Controls.Add(Me.pbCache)
		Me.pnlBG.Controls.Add(Me.pbPicture)
		Me.pnlBG.Dock = System.Windows.Forms.DockStyle.Fill
		Me.pnlBG.Location = New System.Drawing.Point(0, 0)
		Me.pnlBG.Name = "pnlBG"
		Me.pnlBG.Size = New System.Drawing.Size(489, 361)
		Me.pnlBG.TabIndex = 1
		'
		'pbCache
		'
		Me.pbCache.Location = New System.Drawing.Point(194, 155)
		Me.pbCache.Name = "pbCache"
		Me.pbCache.Size = New System.Drawing.Size(100, 50)
		Me.pbCache.TabIndex = 1
		Me.pbCache.TabStop = False
		Me.pbCache.Visible = False
		'
		'pbPicture
		'
		Me.pbPicture.Location = New System.Drawing.Point(0, 25)
		Me.pbPicture.Name = "pbPicture"
		Me.pbPicture.Size = New System.Drawing.Size(100, 50)
		Me.pbPicture.TabIndex = 0
		Me.pbPicture.TabStop = False
		'
		'tsImgView
		'
		Me.tsImgView.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbFit, Me.tsbFull})
		Me.tsImgView.Location = New System.Drawing.Point(0, 0)
		Me.tsImgView.Name = "tsImgView"
		Me.tsImgView.Size = New System.Drawing.Size(489, 25)
		Me.tsImgView.TabIndex = 0
		Me.tsImgView.Text = "ToolStrip1"
		'
		'tsbFit
		'
		Me.tsbFit.Image = CType(resources.GetObject("tsbFit.Image"), System.Drawing.Image)
		Me.tsbFit.ImageTransparentColor = System.Drawing.Color.Magenta
		Me.tsbFit.Name = "tsbFit"
		Me.tsbFit.Size = New System.Drawing.Size(40, 22)
		Me.tsbFit.Text = "Fit"
		'
		'tsbFull
		'
		Me.tsbFull.Image = CType(resources.GetObject("tsbFull.Image"), System.Drawing.Image)
		Me.tsbFull.ImageTransparentColor = System.Drawing.Color.Magenta
		Me.tsbFull.Name = "tsbFull"
		Me.tsbFull.Size = New System.Drawing.Size(69, 22)
		Me.tsbFull.Text = "Full Size"
		'
		'dlgImgView
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(489, 361)
		Me.Controls.Add(Me.tsImgView)
		Me.Controls.Add(Me.pnlBG)
		Me.DoubleBuffered = True
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgImgView"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Image Viewer"
		Me.pnlBG.ResumeLayout(False)
		CType(Me.pbCache, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbPicture, System.ComponentModel.ISupportInitialize).EndInit()
		Me.tsImgView.ResumeLayout(False)
		Me.tsImgView.PerformLayout()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents pnlBG As System.Windows.Forms.Panel
    Friend WithEvents pbPicture As System.Windows.Forms.PictureBox
    Friend WithEvents pbCache As System.Windows.Forms.PictureBox
    Friend WithEvents tsImgView As System.Windows.Forms.ToolStrip
    Friend WithEvents tsbFit As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsbFull As System.Windows.Forms.ToolStripButton

End Class
