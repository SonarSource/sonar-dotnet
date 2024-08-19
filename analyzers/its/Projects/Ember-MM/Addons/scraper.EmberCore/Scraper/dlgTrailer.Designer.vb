<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgTrailer
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgTrailer))
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.lbTrailers = New System.Windows.Forms.ListBox()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.pnlStatus = New System.Windows.Forms.Panel()
		Me.lblStatus = New System.Windows.Forms.Label()
		Me.pbStatus = New System.Windows.Forms.ProgressBar()
		Me.btnGetTrailers = New System.Windows.Forms.Button()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.btnBrowse = New System.Windows.Forms.Button()
		Me.txtManual = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.txtYouTube = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.btnPlayTrailer = New System.Windows.Forms.Button()
		Me.btnSetNfo = New System.Windows.Forms.Button()
		Me.ofdTrailer = New System.Windows.Forms.OpenFileDialog()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.GroupBox1.SuspendLayout()
		Me.pnlStatus.SuspendLayout()
		Me.GroupBox2.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Enabled = False
		Me.OK_Button.Location = New System.Drawing.Point(290, 339)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(74, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "Download"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Location = New System.Drawing.Point(369, 339)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'lbTrailers
		'
		Me.lbTrailers.Enabled = False
		Me.lbTrailers.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lbTrailers.FormattingEnabled = True
		Me.lbTrailers.HorizontalScrollbar = True
		Me.lbTrailers.Location = New System.Drawing.Point(6, 19)
		Me.lbTrailers.Name = "lbTrailers"
		Me.lbTrailers.Size = New System.Drawing.Size(411, 173)
		Me.lbTrailers.TabIndex = 0
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.pnlStatus)
		Me.GroupBox1.Controls.Add(Me.btnGetTrailers)
		Me.GroupBox1.Controls.Add(Me.GroupBox2)
		Me.GroupBox1.Controls.Add(Me.lbTrailers)
		Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(9, 9)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(424, 319)
		Me.GroupBox1.TabIndex = 0
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Select Trailer to Scrape"
		'
		'pnlStatus
		'
		Me.pnlStatus.BackColor = System.Drawing.Color.White
		Me.pnlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlStatus.Controls.Add(Me.lblStatus)
		Me.pnlStatus.Controls.Add(Me.pbStatus)
		Me.pnlStatus.Location = New System.Drawing.Point(112, 82)
		Me.pnlStatus.Name = "pnlStatus"
		Me.pnlStatus.Size = New System.Drawing.Size(200, 54)
		Me.pnlStatus.TabIndex = 1
		Me.pnlStatus.Visible = False
		'
		'lblStatus
		'
		Me.lblStatus.AutoSize = True
		Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblStatus.Location = New System.Drawing.Point(3, 10)
		Me.lblStatus.Name = "lblStatus"
		Me.lblStatus.Size = New System.Drawing.Size(121, 13)
		Me.lblStatus.TabIndex = 0
		Me.lblStatus.Text = "Compiling trailer list..."
		Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'pbStatus
		'
		Me.pbStatus.Location = New System.Drawing.Point(3, 32)
		Me.pbStatus.MarqueeAnimationSpeed = 25
		Me.pbStatus.Name = "pbStatus"
		Me.pbStatus.Size = New System.Drawing.Size(192, 17)
		Me.pbStatus.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.pbStatus.TabIndex = 1
		'
		'btnGetTrailers
		'
		Me.btnGetTrailers.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnGetTrailers.Image = CType(resources.GetObject("btnGetTrailers.Image"), System.Drawing.Image)
		Me.btnGetTrailers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnGetTrailers.Location = New System.Drawing.Point(120, 82)
		Me.btnGetTrailers.Name = "btnGetTrailers"
		Me.btnGetTrailers.Size = New System.Drawing.Size(184, 23)
		Me.btnGetTrailers.TabIndex = 2
		Me.btnGetTrailers.Text = "Download Trailer List"
		Me.btnGetTrailers.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnGetTrailers.UseVisualStyleBackColor = True
		'
		'GroupBox2
		'
		Me.GroupBox2.Controls.Add(Me.btnBrowse)
		Me.GroupBox2.Controls.Add(Me.txtManual)
		Me.GroupBox2.Controls.Add(Me.Label2)
		Me.GroupBox2.Controls.Add(Me.txtYouTube)
		Me.GroupBox2.Controls.Add(Me.Label1)
		Me.GroupBox2.Location = New System.Drawing.Point(6, 201)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(411, 111)
		Me.GroupBox2.TabIndex = 3
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Manual Trailer Entry"
		'
		'btnBrowse
		'
		Me.btnBrowse.Location = New System.Drawing.Point(376, 82)
		Me.btnBrowse.Name = "btnBrowse"
		Me.btnBrowse.Size = New System.Drawing.Size(25, 23)
		Me.btnBrowse.TabIndex = 4
		Me.btnBrowse.Text = "..."
		Me.btnBrowse.UseVisualStyleBackColor = True
		'
		'txtManual
		'
		Me.txtManual.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.txtManual.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtManual.Location = New System.Drawing.Point(9, 82)
		Me.txtManual.Name = "txtManual"
		Me.txtManual.Size = New System.Drawing.Size(365, 22)
		Me.txtManual.TabIndex = 3
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label2.Location = New System.Drawing.Point(6, 68)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(72, 13)
		Me.Label2.TabIndex = 2
		Me.Label2.Text = "Local Trailer:"
		'
		'txtYouTube
		'
		Me.txtYouTube.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.txtYouTube.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtYouTube.Location = New System.Drawing.Point(9, 28)
		Me.txtYouTube.Name = "txtYouTube"
		Me.txtYouTube.Size = New System.Drawing.Size(392, 22)
		Me.txtYouTube.TabIndex = 1
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label1.Location = New System.Drawing.Point(6, 14)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(153, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Direct Link or YouTube URL:"
		'
		'btnPlayTrailer
		'
		Me.btnPlayTrailer.Enabled = False
		Me.btnPlayTrailer.Image = CType(resources.GetObject("btnPlayTrailer.Image"), System.Drawing.Image)
		Me.btnPlayTrailer.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnPlayTrailer.Location = New System.Drawing.Point(12, 339)
		Me.btnPlayTrailer.Name = "btnPlayTrailer"
		Me.btnPlayTrailer.Size = New System.Drawing.Size(106, 23)
		Me.btnPlayTrailer.TabIndex = 3
		Me.btnPlayTrailer.Text = "Preview Trailer"
		Me.btnPlayTrailer.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnPlayTrailer.UseVisualStyleBackColor = True
		'
		'btnSetNfo
		'
		Me.btnSetNfo.Enabled = False
		Me.btnSetNfo.Location = New System.Drawing.Point(207, 339)
		Me.btnSetNfo.Name = "btnSetNfo"
		Me.btnSetNfo.Size = New System.Drawing.Size(77, 23)
		Me.btnSetNfo.TabIndex = 4
		Me.btnSetNfo.Text = "Set To Nfo"
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.GroupBox1)
		Me.Panel1.Location = New System.Drawing.Point(3, 3)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(444, 331)
		Me.Panel1.TabIndex = 2
		'
		'dlgTrailer
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(450, 366)
		Me.ControlBox = False
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.btnSetNfo)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.btnPlayTrailer)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgTrailer"
		Me.ShowIcon = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Select Trailer"
		Me.GroupBox1.ResumeLayout(False)
		Me.pnlStatus.ResumeLayout(False)
		Me.pnlStatus.PerformLayout()
		Me.GroupBox2.ResumeLayout(False)
		Me.GroupBox2.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents lbTrailers As System.Windows.Forms.ListBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents pnlStatus As System.Windows.Forms.Panel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents pbStatus As System.Windows.Forms.ProgressBar
    Friend WithEvents btnPlayTrailer As System.Windows.Forms.Button
    Friend WithEvents btnSetNfo As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtYouTube As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents txtManual As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ofdTrailer As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnGetTrailers As System.Windows.Forms.Button
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
