<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
        Partial Class dlgTVImageSelect
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgTVImageSelect))
		Me.tvList = New System.Windows.Forms.TreeView()
		Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
		Me.pnlImages = New System.Windows.Forms.Panel()
		Me.pbCurrent = New System.Windows.Forms.PictureBox()
		Me.pnlStatus = New System.Windows.Forms.Panel()
		Me.lblStatus = New System.Windows.Forms.Label()
		Me.pbStatus = New System.Windows.Forms.ProgressBar()
		Me.btnOK = New System.Windows.Forms.Button()
		Me.btnCancel = New System.Windows.Forms.Button()
		Me.pbDelete = New System.Windows.Forms.PictureBox()
		Me.pbUndo = New System.Windows.Forms.PictureBox()
		Me.lblCurrentImage = New System.Windows.Forms.Label()
		CType(Me.pbCurrent, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.pnlStatus.SuspendLayout()
		CType(Me.pbDelete, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbUndo, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'tvList
		'
		Me.tvList.Enabled = False
		Me.tvList.ImageIndex = 0
		Me.tvList.ImageList = Me.ImageList1
		Me.tvList.Location = New System.Drawing.Point(3, 4)
		Me.tvList.Name = "tvList"
		Me.tvList.SelectedImageIndex = 0
		Me.tvList.Size = New System.Drawing.Size(214, 262)
		Me.tvList.TabIndex = 2
		Me.tvList.Visible = False
		'
		'ImageList1
		'
		Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
		Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
		Me.ImageList1.Images.SetKeyName(0, "new_page.png")
		Me.ImageList1.Images.SetKeyName(1, "image.png")
		Me.ImageList1.Images.SetKeyName(2, "artwork.png")
		Me.ImageList1.Images.SetKeyName(3, "star_full.png")
		'
		'pnlImages
		'
		Me.pnlImages.AutoScroll = True
		Me.pnlImages.BackColor = System.Drawing.SystemColors.Control
		Me.pnlImages.Location = New System.Drawing.Point(222, 4)
		Me.pnlImages.Name = "pnlImages"
		Me.pnlImages.Size = New System.Drawing.Size(622, 421)
		Me.pnlImages.TabIndex = 3
		'
		'pbCurrent
		'
		Me.pbCurrent.BackColor = System.Drawing.SystemColors.Control
		Me.pbCurrent.Location = New System.Drawing.Point(3, 293)
		Me.pbCurrent.Name = "pbCurrent"
		Me.pbCurrent.Size = New System.Drawing.Size(214, 157)
		Me.pbCurrent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbCurrent.TabIndex = 2
		Me.pbCurrent.TabStop = False
		Me.pbCurrent.Visible = False
		'
		'pnlStatus
		'
		Me.pnlStatus.BackColor = System.Drawing.Color.White
		Me.pnlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlStatus.Controls.Add(Me.lblStatus)
		Me.pnlStatus.Controls.Add(Me.pbStatus)
		Me.pnlStatus.Location = New System.Drawing.Point(264, 192)
		Me.pnlStatus.Name = "pnlStatus"
		Me.pnlStatus.Size = New System.Drawing.Size(321, 75)
		Me.pnlStatus.TabIndex = 5
		'
		'lblStatus
		'
		Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblStatus.Location = New System.Drawing.Point(5, 10)
		Me.lblStatus.Name = "lblStatus"
		Me.lblStatus.Size = New System.Drawing.Size(310, 13)
		Me.lblStatus.TabIndex = 0
		Me.lblStatus.Text = "Gathering Data..."
		'
		'pbStatus
		'
		Me.pbStatus.Location = New System.Drawing.Point(6, 52)
		Me.pbStatus.MarqueeAnimationSpeed = 25
		Me.pbStatus.Name = "pbStatus"
		Me.pbStatus.Size = New System.Drawing.Size(309, 19)
		Me.pbStatus.Step = 1
		Me.pbStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbStatus.TabIndex = 1
		'
		'btnOK
		'
		Me.btnOK.Enabled = False
		Me.btnOK.Location = New System.Drawing.Point(708, 431)
		Me.btnOK.Name = "btnOK"
		Me.btnOK.Size = New System.Drawing.Size(65, 22)
		Me.btnOK.TabIndex = 0
		Me.btnOK.Text = "OK"
		Me.btnOK.UseVisualStyleBackColor = True
		'
		'btnCancel
		'
		Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.btnCancel.Location = New System.Drawing.Point(779, 431)
		Me.btnCancel.Name = "btnCancel"
		Me.btnCancel.Size = New System.Drawing.Size(65, 22)
		Me.btnCancel.TabIndex = 1
		Me.btnCancel.Text = "Cancel"
		Me.btnCancel.UseVisualStyleBackColor = True
		'
		'pbDelete
		'
		Me.pbDelete.Image = CType(resources.GetObject("pbDelete.Image"), System.Drawing.Image)
		Me.pbDelete.Location = New System.Drawing.Point(3, 293)
		Me.pbDelete.Name = "pbDelete"
		Me.pbDelete.Size = New System.Drawing.Size(16, 16)
		Me.pbDelete.TabIndex = 13
		Me.pbDelete.TabStop = False
		Me.pbDelete.Visible = False
		'
		'pbUndo
		'
		Me.pbUndo.Image = CType(resources.GetObject("pbUndo.Image"), System.Drawing.Image)
		Me.pbUndo.Location = New System.Drawing.Point(201, 293)
		Me.pbUndo.Name = "pbUndo"
		Me.pbUndo.Size = New System.Drawing.Size(16, 16)
		Me.pbUndo.TabIndex = 14
		Me.pbUndo.TabStop = False
		Me.pbUndo.Visible = False
		'
		'lblCurrentImage
		'
		Me.lblCurrentImage.AutoSize = True
		Me.lblCurrentImage.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblCurrentImage.Location = New System.Drawing.Point(0, 274)
		Me.lblCurrentImage.Name = "lblCurrentImage"
		Me.lblCurrentImage.Size = New System.Drawing.Size(100, 17)
		Me.lblCurrentImage.TabIndex = 4
		Me.lblCurrentImage.Text = "Current Image:"
		Me.lblCurrentImage.Visible = False
		'
		'dlgTVImageSelect
		'
		Me.AcceptButton = Me.btnOK
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.btnCancel
		Me.ClientSize = New System.Drawing.Size(849, 481)
		Me.ControlBox = False
		Me.Controls.Add(Me.lblCurrentImage)
		Me.Controls.Add(Me.pbUndo)
		Me.Controls.Add(Me.pbDelete)
		Me.Controls.Add(Me.btnCancel)
		Me.Controls.Add(Me.btnOK)
		Me.Controls.Add(Me.pnlStatus)
		Me.Controls.Add(Me.pbCurrent)
		Me.Controls.Add(Me.pnlImages)
		Me.Controls.Add(Me.tvList)
		Me.DoubleBuffered = True
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.MinimumSize = New System.Drawing.Size(855, 491)
		Me.Name = "dlgTVImageSelect"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "TV Image Selection"
		CType(Me.pbCurrent, System.ComponentModel.ISupportInitialize).EndInit()
		Me.pnlStatus.ResumeLayout(False)
		CType(Me.pbDelete, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbUndo, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents tvList As System.Windows.Forms.TreeView
    Friend WithEvents pnlImages As System.Windows.Forms.Panel
    Friend WithEvents pbCurrent As System.Windows.Forms.PictureBox
    Friend WithEvents pnlStatus As System.Windows.Forms.Panel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents pbStatus As System.Windows.Forms.ProgressBar
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents pbDelete As System.Windows.Forms.PictureBox
    Friend WithEvents pbUndo As System.Windows.Forms.PictureBox
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents lblCurrentImage As System.Windows.Forms.Label
End Class