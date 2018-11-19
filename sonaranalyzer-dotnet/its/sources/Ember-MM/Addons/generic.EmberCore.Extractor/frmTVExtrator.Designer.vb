<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTVExtrator
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTVExtrator))
		Me.pnlExtrator = New System.Windows.Forms.Panel()
		Me.btnFrameSave = New System.Windows.Forms.Button()
		Me.pnlFrameProgress = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.lblTime = New System.Windows.Forms.Label()
		Me.tbFrame = New System.Windows.Forms.TrackBar()
		Me.btnFrameLoad = New System.Windows.Forms.Button()
		Me.pbFrame = New System.Windows.Forms.PictureBox()
		Me.pnlExtrator.SuspendLayout()
		Me.pnlFrameProgress.SuspendLayout()
		CType(Me.tbFrame, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'pnlExtrator
		'
		Me.pnlExtrator.BackColor = System.Drawing.Color.White
		Me.pnlExtrator.Controls.Add(Me.btnFrameSave)
		Me.pnlExtrator.Controls.Add(Me.pnlFrameProgress)
		Me.pnlExtrator.Controls.Add(Me.lblTime)
		Me.pnlExtrator.Controls.Add(Me.tbFrame)
		Me.pnlExtrator.Controls.Add(Me.btnFrameLoad)
		Me.pnlExtrator.Controls.Add(Me.pbFrame)
		Me.pnlExtrator.Location = New System.Drawing.Point(0, 0)
		Me.pnlExtrator.Name = "pnlExtrator"
		Me.pnlExtrator.Size = New System.Drawing.Size(836, 452)
		Me.pnlExtrator.TabIndex = 0
		'
		'btnFrameSave
		'
		Me.btnFrameSave.Enabled = False
		Me.btnFrameSave.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnFrameSave.Image = CType(resources.GetObject("btnFrameSave.Image"), System.Drawing.Image)
		Me.btnFrameSave.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnFrameSave.Location = New System.Drawing.Point(736, 363)
		Me.btnFrameSave.Name = "btnFrameSave"
		Me.btnFrameSave.Size = New System.Drawing.Size(96, 83)
		Me.btnFrameSave.TabIndex = 3
		Me.btnFrameSave.Text = "Save as Poster"
		Me.btnFrameSave.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnFrameSave.UseVisualStyleBackColor = True
		'
		'pnlFrameProgress
		'
		Me.pnlFrameProgress.BackColor = System.Drawing.Color.White
		Me.pnlFrameProgress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlFrameProgress.Controls.Add(Me.Label3)
		Me.pnlFrameProgress.Controls.Add(Me.ProgressBar1)
		Me.pnlFrameProgress.Location = New System.Drawing.Point(242, 176)
		Me.pnlFrameProgress.Name = "pnlFrameProgress"
		Me.pnlFrameProgress.Size = New System.Drawing.Size(252, 51)
		Me.pnlFrameProgress.TabIndex = 4
		Me.pnlFrameProgress.Visible = False
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(2, 7)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(101, 13)
		Me.Label3.TabIndex = 0
		Me.Label3.Text = "Extracting Frame..."
		'
		'ProgressBar1
		'
		Me.ProgressBar1.Location = New System.Drawing.Point(4, 26)
		Me.ProgressBar1.MarqueeAnimationSpeed = 25
		Me.ProgressBar1.Name = "ProgressBar1"
		Me.ProgressBar1.Size = New System.Drawing.Size(242, 16)
		Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.ProgressBar1.TabIndex = 1
		'
		'lblTime
		'
		Me.lblTime.Location = New System.Drawing.Point(672, 423)
		Me.lblTime.Name = "lblTime"
		Me.lblTime.Size = New System.Drawing.Size(59, 23)
		Me.lblTime.TabIndex = 2
		Me.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'tbFrame
		'
		Me.tbFrame.BackColor = System.Drawing.Color.White
		Me.tbFrame.Enabled = False
		Me.tbFrame.Location = New System.Drawing.Point(7, 423)
		Me.tbFrame.Name = "tbFrame"
		Me.tbFrame.Size = New System.Drawing.Size(659, 45)
		Me.tbFrame.TabIndex = 1
		Me.tbFrame.TickStyle = System.Windows.Forms.TickStyle.None
		'
		'btnFrameLoad
		'
		Me.btnFrameLoad.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnFrameLoad.Image = CType(resources.GetObject("btnFrameLoad.Image"), System.Drawing.Image)
		Me.btnFrameLoad.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnFrameLoad.Location = New System.Drawing.Point(736, 7)
		Me.btnFrameLoad.Name = "btnFrameLoad"
		Me.btnFrameLoad.Size = New System.Drawing.Size(96, 83)
		Me.btnFrameLoad.TabIndex = 0
		Me.btnFrameLoad.Text = "Load Episode"
		Me.btnFrameLoad.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnFrameLoad.UseVisualStyleBackColor = True
		'
		'pbFrame
		'
		Me.pbFrame.BackColor = System.Drawing.Color.DimGray
		Me.pbFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbFrame.Location = New System.Drawing.Point(7, 7)
		Me.pbFrame.Name = "pbFrame"
		Me.pbFrame.Size = New System.Drawing.Size(724, 414)
		Me.pbFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbFrame.TabIndex = 14
		Me.pbFrame.TabStop = False
		'
		'frmTVExtrator
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(841, 457)
		Me.Controls.Add(Me.pnlExtrator)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmTVExtrator"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "frmTVExtrator"
		Me.pnlExtrator.ResumeLayout(False)
		Me.pnlExtrator.PerformLayout()
		Me.pnlFrameProgress.ResumeLayout(False)
		Me.pnlFrameProgress.PerformLayout()
		CType(Me.tbFrame, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlExtrator As System.Windows.Forms.Panel
    Friend WithEvents btnFrameSave As System.Windows.Forms.Button
    Friend WithEvents pnlFrameProgress As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents tbFrame As System.Windows.Forms.TrackBar
    Friend WithEvents btnFrameLoad As System.Windows.Forms.Button
    Friend WithEvents pbFrame As System.Windows.Forms.PictureBox

End Class
