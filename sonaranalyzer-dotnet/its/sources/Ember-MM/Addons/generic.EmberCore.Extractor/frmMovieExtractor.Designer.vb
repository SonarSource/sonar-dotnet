<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMovieExtractor
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMovieExtractor))
		Me.pnlExtrator = New System.Windows.Forms.Panel()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.txtThumbCount = New System.Windows.Forms.TextBox()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.btnAutoGen = New System.Windows.Forms.Button()
		Me.btnFrameSave = New System.Windows.Forms.Button()
		Me.pnlFrameProgress = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.lblTime = New System.Windows.Forms.Label()
		Me.tbFrame = New System.Windows.Forms.TrackBar()
		Me.btnFrameLoad = New System.Windows.Forms.Button()
		Me.pbFrame = New System.Windows.Forms.PictureBox()
		Me.DelayTimer = New System.Windows.Forms.Timer(Me.components)
		Me.pnlExtrator.SuspendLayout()
		Me.GroupBox1.SuspendLayout()
		Me.pnlFrameProgress.SuspendLayout()
		CType(Me.tbFrame, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'pnlExtrator
		'
		Me.pnlExtrator.BackColor = System.Drawing.Color.White
		Me.pnlExtrator.Controls.Add(Me.GroupBox1)
		Me.pnlExtrator.Controls.Add(Me.btnFrameSave)
		Me.pnlExtrator.Controls.Add(Me.pnlFrameProgress)
		Me.pnlExtrator.Controls.Add(Me.lblTime)
		Me.pnlExtrator.Controls.Add(Me.tbFrame)
		Me.pnlExtrator.Controls.Add(Me.btnFrameLoad)
		Me.pnlExtrator.Controls.Add(Me.pbFrame)
		Me.pnlExtrator.Location = New System.Drawing.Point(2, 2)
		Me.pnlExtrator.Name = "pnlExtrator"
		Me.pnlExtrator.Size = New System.Drawing.Size(836, 452)
		Me.pnlExtrator.TabIndex = 0
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.txtThumbCount)
		Me.GroupBox1.Controls.Add(Me.Label5)
		Me.GroupBox1.Controls.Add(Me.btnAutoGen)
		Me.GroupBox1.Location = New System.Drawing.Point(734, 161)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(99, 100)
		Me.GroupBox1.TabIndex = 1
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Auto-Generate"
		'
		'txtThumbCount
		'
		Me.txtThumbCount.Location = New System.Drawing.Point(68, 18)
		Me.txtThumbCount.Name = "txtThumbCount"
		Me.txtThumbCount.Size = New System.Drawing.Size(25, 22)
		Me.txtThumbCount.TabIndex = 1
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label5.Location = New System.Drawing.Point(2, 23)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(67, 13)
		Me.Label5.TabIndex = 0
		Me.Label5.Text = "# to Create:"
		'
		'btnAutoGen
		'
		Me.btnAutoGen.Enabled = False
		Me.btnAutoGen.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnAutoGen.Image = CType(resources.GetObject("btnAutoGen.Image"), System.Drawing.Image)
		Me.btnAutoGen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnAutoGen.Location = New System.Drawing.Point(5, 49)
		Me.btnAutoGen.Name = "btnAutoGen"
		Me.btnAutoGen.Size = New System.Drawing.Size(89, 45)
		Me.btnAutoGen.TabIndex = 2
		Me.btnAutoGen.Text = "Auto-Gen"
		Me.btnAutoGen.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnAutoGen.UseVisualStyleBackColor = True
		'
		'btnFrameSave
		'
		Me.btnFrameSave.Enabled = False
		Me.btnFrameSave.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnFrameSave.Image = CType(resources.GetObject("btnFrameSave.Image"), System.Drawing.Image)
		Me.btnFrameSave.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnFrameSave.Location = New System.Drawing.Point(735, 361)
		Me.btnFrameSave.Name = "btnFrameSave"
		Me.btnFrameSave.Size = New System.Drawing.Size(96, 83)
		Me.btnFrameSave.TabIndex = 2
		Me.btnFrameSave.Text = "Save Extrathumb"
		Me.btnFrameSave.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnFrameSave.UseVisualStyleBackColor = True
		'
		'pnlFrameProgress
		'
		Me.pnlFrameProgress.BackColor = System.Drawing.Color.White
		Me.pnlFrameProgress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlFrameProgress.Controls.Add(Me.Label3)
		Me.pnlFrameProgress.Controls.Add(Me.ProgressBar1)
		Me.pnlFrameProgress.Location = New System.Drawing.Point(241, 173)
		Me.pnlFrameProgress.Name = "pnlFrameProgress"
		Me.pnlFrameProgress.Size = New System.Drawing.Size(252, 51)
		Me.pnlFrameProgress.TabIndex = 0
		Me.pnlFrameProgress.Visible = False
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label3.Location = New System.Drawing.Point(2, 7)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(103, 13)
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
		Me.lblTime.Location = New System.Drawing.Point(671, 420)
		Me.lblTime.Name = "lblTime"
		Me.lblTime.Size = New System.Drawing.Size(59, 23)
		Me.lblTime.TabIndex = 4
		Me.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'tbFrame
		'
		Me.tbFrame.BackColor = System.Drawing.Color.White
		Me.tbFrame.Enabled = False
		Me.tbFrame.Location = New System.Drawing.Point(6, 420)
		Me.tbFrame.Name = "tbFrame"
		Me.tbFrame.Size = New System.Drawing.Size(659, 45)
		Me.tbFrame.TabIndex = 3
		Me.tbFrame.TickStyle = System.Windows.Forms.TickStyle.None
		'
		'btnFrameLoad
		'
		Me.btnFrameLoad.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnFrameLoad.Image = CType(resources.GetObject("btnFrameLoad.Image"), System.Drawing.Image)
		Me.btnFrameLoad.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnFrameLoad.Location = New System.Drawing.Point(735, 4)
		Me.btnFrameLoad.Name = "btnFrameLoad"
		Me.btnFrameLoad.Size = New System.Drawing.Size(96, 83)
		Me.btnFrameLoad.TabIndex = 0
		Me.btnFrameLoad.Text = "Load Movie"
		Me.btnFrameLoad.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnFrameLoad.UseVisualStyleBackColor = True
		'
		'pbFrame
		'
		Me.pbFrame.BackColor = System.Drawing.Color.DimGray
		Me.pbFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbFrame.Location = New System.Drawing.Point(6, 4)
		Me.pbFrame.Name = "pbFrame"
		Me.pbFrame.Size = New System.Drawing.Size(724, 414)
		Me.pbFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbFrame.TabIndex = 16
		Me.pbFrame.TabStop = False
		'
		'DelayTimer
		'
		Me.DelayTimer.Interval = 250
		'
		'frmMovieExtractor
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(843, 455)
		Me.Controls.Add(Me.pnlExtrator)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmMovieExtractor"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "frmExtrator"
		Me.pnlExtrator.ResumeLayout(False)
		Me.pnlExtrator.PerformLayout()
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.pnlFrameProgress.ResumeLayout(False)
		Me.pnlFrameProgress.PerformLayout()
		CType(Me.tbFrame, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.pbFrame, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlExtrator As System.Windows.Forms.Panel
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtThumbCount As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents btnAutoGen As System.Windows.Forms.Button
    Friend WithEvents btnFrameSave As System.Windows.Forms.Button
    Friend WithEvents pnlFrameProgress As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents tbFrame As System.Windows.Forms.TrackBar
    Friend WithEvents btnFrameLoad As System.Windows.Forms.Button
    Friend WithEvents pbFrame As System.Windows.Forms.PictureBox
    Friend WithEvents DelayTimer As System.Windows.Forms.Timer

End Class
