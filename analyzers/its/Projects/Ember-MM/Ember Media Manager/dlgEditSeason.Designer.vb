<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgEditSeason
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgEditSeason))
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage2 = New System.Windows.Forms.TabPage()
		Me.btnSetPosterDL = New System.Windows.Forms.Button()
		Me.btnRemovePoster = New System.Windows.Forms.Button()
		Me.lblPosterSize = New System.Windows.Forms.Label()
		Me.btnSetPosterScrape = New System.Windows.Forms.Button()
		Me.btnSetPoster = New System.Windows.Forms.Button()
		Me.pbPoster = New System.Windows.Forms.PictureBox()
		Me.TabPage3 = New System.Windows.Forms.TabPage()
		Me.btnSetFanartDL = New System.Windows.Forms.Button()
		Me.btnRemoveFanart = New System.Windows.Forms.Button()
		Me.lblFanartSize = New System.Windows.Forms.Label()
		Me.btnSetFanartScrape = New System.Windows.Forms.Button()
		Me.btnSetFanart = New System.Windows.Forms.Button()
		Me.pbFanart = New System.Windows.Forms.PictureBox()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.ofdImage = New System.Windows.Forms.OpenFileDialog()
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabControl1.SuspendLayout()
		Me.TabPage2.SuspendLayout()
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TabPage3.SuspendLayout()
		CType(Me.pbFanart, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'pnlTop
		'
		Me.pnlTop.BackColor = System.Drawing.Color.LightSteelBlue
		Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlTop.Controls.Add(Me.Label2)
		Me.pnlTop.Controls.Add(Me.Label1)
		Me.pnlTop.Controls.Add(Me.PictureBox1)
		Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
		Me.pnlTop.Location = New System.Drawing.Point(0, 0)
		Me.pnlTop.Name = "pnlTop"
		Me.pnlTop.Size = New System.Drawing.Size(854, 64)
		Me.pnlTop.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.BackColor = System.Drawing.Color.Transparent
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.ForeColor = System.Drawing.Color.White
		Me.Label2.Location = New System.Drawing.Point(61, 38)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(209, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Edit the details for the selected season."
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.BackColor = System.Drawing.Color.Transparent
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.ForeColor = System.Drawing.Color.White
		Me.Label1.Location = New System.Drawing.Point(58, 3)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(146, 32)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Edit Season"
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(7, 8)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(48, 48)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
		Me.PictureBox1.TabIndex = 0
		Me.PictureBox1.TabStop = False
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage2)
		Me.TabControl1.Controls.Add(Me.TabPage3)
		Me.TabControl1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TabControl1.Location = New System.Drawing.Point(4, 70)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(844, 478)
		Me.TabControl1.TabIndex = 3
		'
		'TabPage2
		'
		Me.TabPage2.Controls.Add(Me.btnSetPosterDL)
		Me.TabPage2.Controls.Add(Me.btnRemovePoster)
		Me.TabPage2.Controls.Add(Me.lblPosterSize)
		Me.TabPage2.Controls.Add(Me.btnSetPosterScrape)
		Me.TabPage2.Controls.Add(Me.btnSetPoster)
		Me.TabPage2.Controls.Add(Me.pbPoster)
		Me.TabPage2.Location = New System.Drawing.Point(4, 22)
		Me.TabPage2.Name = "TabPage2"
		Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage2.Size = New System.Drawing.Size(836, 452)
		Me.TabPage2.TabIndex = 1
		Me.TabPage2.Text = "Poster"
		Me.TabPage2.UseVisualStyleBackColor = True
		'
		'btnSetPosterDL
		'
		Me.btnSetPosterDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetPosterDL.Image = CType(resources.GetObject("btnSetPosterDL.Image"), System.Drawing.Image)
		Me.btnSetPosterDL.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPosterDL.Location = New System.Drawing.Point(735, 180)
		Me.btnSetPosterDL.Name = "btnSetPosterDL"
		Me.btnSetPosterDL.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPosterDL.TabIndex = 3
		Me.btnSetPosterDL.Text = "Change Poster (Download)"
		Me.btnSetPosterDL.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPosterDL.UseVisualStyleBackColor = True
		'
		'btnRemovePoster
		'
		Me.btnRemovePoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnRemovePoster.Image = CType(resources.GetObject("btnRemovePoster.Image"), System.Drawing.Image)
		Me.btnRemovePoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnRemovePoster.Location = New System.Drawing.Point(735, 363)
		Me.btnRemovePoster.Name = "btnRemovePoster"
		Me.btnRemovePoster.Size = New System.Drawing.Size(96, 83)
		Me.btnRemovePoster.TabIndex = 4
		Me.btnRemovePoster.Text = "Remove Poster"
		Me.btnRemovePoster.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnRemovePoster.UseVisualStyleBackColor = True
		'
		'lblPosterSize
		'
		Me.lblPosterSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblPosterSize.Location = New System.Drawing.Point(8, 8)
		Me.lblPosterSize.Name = "lblPosterSize"
		Me.lblPosterSize.Size = New System.Drawing.Size(105, 23)
		Me.lblPosterSize.TabIndex = 0
		Me.lblPosterSize.Text = "Size: (XXXXxXXXX)"
		Me.lblPosterSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblPosterSize.Visible = False
		'
		'btnSetPosterScrape
		'
		Me.btnSetPosterScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetPosterScrape.Image = CType(resources.GetObject("btnSetPosterScrape.Image"), System.Drawing.Image)
		Me.btnSetPosterScrape.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPosterScrape.Location = New System.Drawing.Point(735, 93)
		Me.btnSetPosterScrape.Name = "btnSetPosterScrape"
		Me.btnSetPosterScrape.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPosterScrape.TabIndex = 2
		Me.btnSetPosterScrape.Text = "Change Poster (Scrape)"
		Me.btnSetPosterScrape.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPosterScrape.UseVisualStyleBackColor = True
		'
		'btnSetPoster
		'
		Me.btnSetPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetPoster.Image = CType(resources.GetObject("btnSetPoster.Image"), System.Drawing.Image)
		Me.btnSetPoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetPoster.Location = New System.Drawing.Point(735, 6)
		Me.btnSetPoster.Name = "btnSetPoster"
		Me.btnSetPoster.Size = New System.Drawing.Size(96, 83)
		Me.btnSetPoster.TabIndex = 1
		Me.btnSetPoster.Text = "Change Poster (Local)"
		Me.btnSetPoster.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetPoster.UseVisualStyleBackColor = True
		'
		'pbPoster
		'
		Me.pbPoster.BackColor = System.Drawing.Color.DimGray
		Me.pbPoster.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbPoster.Location = New System.Drawing.Point(6, 6)
		Me.pbPoster.Name = "pbPoster"
		Me.pbPoster.Size = New System.Drawing.Size(724, 440)
		Me.pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbPoster.TabIndex = 0
		Me.pbPoster.TabStop = False
		'
		'TabPage3
		'
		Me.TabPage3.Controls.Add(Me.btnSetFanartDL)
		Me.TabPage3.Controls.Add(Me.btnRemoveFanart)
		Me.TabPage3.Controls.Add(Me.lblFanartSize)
		Me.TabPage3.Controls.Add(Me.btnSetFanartScrape)
		Me.TabPage3.Controls.Add(Me.btnSetFanart)
		Me.TabPage3.Controls.Add(Me.pbFanart)
		Me.TabPage3.Location = New System.Drawing.Point(4, 22)
		Me.TabPage3.Name = "TabPage3"
		Me.TabPage3.Size = New System.Drawing.Size(836, 452)
		Me.TabPage3.TabIndex = 2
		Me.TabPage3.Text = "Fanart"
		Me.TabPage3.UseVisualStyleBackColor = True
		'
		'btnSetFanartDL
		'
		Me.btnSetFanartDL.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetFanartDL.Image = CType(resources.GetObject("btnSetFanartDL.Image"), System.Drawing.Image)
		Me.btnSetFanartDL.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanartDL.Location = New System.Drawing.Point(735, 180)
		Me.btnSetFanartDL.Name = "btnSetFanartDL"
		Me.btnSetFanartDL.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanartDL.TabIndex = 3
		Me.btnSetFanartDL.Text = "Change Fanart (Download)"
		Me.btnSetFanartDL.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanartDL.UseVisualStyleBackColor = True
		'
		'btnRemoveFanart
		'
		Me.btnRemoveFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnRemoveFanart.Image = CType(resources.GetObject("btnRemoveFanart.Image"), System.Drawing.Image)
		Me.btnRemoveFanart.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnRemoveFanart.Location = New System.Drawing.Point(735, 363)
		Me.btnRemoveFanart.Name = "btnRemoveFanart"
		Me.btnRemoveFanart.Size = New System.Drawing.Size(96, 83)
		Me.btnRemoveFanart.TabIndex = 4
		Me.btnRemoveFanart.Text = "Remove Fanart"
		Me.btnRemoveFanart.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnRemoveFanart.UseVisualStyleBackColor = True
		'
		'lblFanartSize
		'
		Me.lblFanartSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.lblFanartSize.Location = New System.Drawing.Point(8, 8)
		Me.lblFanartSize.Name = "lblFanartSize"
		Me.lblFanartSize.Size = New System.Drawing.Size(105, 23)
		Me.lblFanartSize.TabIndex = 0
		Me.lblFanartSize.Text = "Size: (XXXXxXXXX)"
		Me.lblFanartSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblFanartSize.Visible = False
		'
		'btnSetFanartScrape
		'
		Me.btnSetFanartScrape.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetFanartScrape.Image = CType(resources.GetObject("btnSetFanartScrape.Image"), System.Drawing.Image)
		Me.btnSetFanartScrape.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanartScrape.Location = New System.Drawing.Point(735, 93)
		Me.btnSetFanartScrape.Name = "btnSetFanartScrape"
		Me.btnSetFanartScrape.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanartScrape.TabIndex = 2
		Me.btnSetFanartScrape.Text = "Change Fanart (Scrape)"
		Me.btnSetFanartScrape.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanartScrape.UseVisualStyleBackColor = True
		'
		'btnSetFanart
		'
		Me.btnSetFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSetFanart.Image = CType(resources.GetObject("btnSetFanart.Image"), System.Drawing.Image)
		Me.btnSetFanart.ImageAlign = System.Drawing.ContentAlignment.TopCenter
		Me.btnSetFanart.Location = New System.Drawing.Point(735, 6)
		Me.btnSetFanart.Name = "btnSetFanart"
		Me.btnSetFanart.Size = New System.Drawing.Size(96, 83)
		Me.btnSetFanart.TabIndex = 1
		Me.btnSetFanart.Text = "Change Fanart (Local)"
		Me.btnSetFanart.TextAlign = System.Drawing.ContentAlignment.BottomCenter
		Me.btnSetFanart.UseVisualStyleBackColor = True
		'
		'pbFanart
		'
		Me.pbFanart.BackColor = System.Drawing.Color.DimGray
		Me.pbFanart.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbFanart.Location = New System.Drawing.Point(6, 6)
		Me.pbFanart.Name = "pbFanart"
		Me.pbFanart.Size = New System.Drawing.Size(724, 440)
		Me.pbFanart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbFanart.TabIndex = 1
		Me.pbFanart.TabStop = False
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Location = New System.Drawing.Point(781, 553)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'OK_Button
		'
		Me.OK_Button.Location = New System.Drawing.Point(708, 553)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'dlgEditSeason
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(854, 582)
		Me.Controls.Add(Me.TabControl1)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.pnlTop)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgEditSeason"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Edit Season"
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage2.ResumeLayout(False)
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TabPage3.ResumeLayout(False)
		CType(Me.pbFanart, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents btnSetPosterDL As System.Windows.Forms.Button
    Friend WithEvents btnRemovePoster As System.Windows.Forms.Button
    Friend WithEvents lblPosterSize As System.Windows.Forms.Label
    Friend WithEvents btnSetPosterScrape As System.Windows.Forms.Button
    Friend WithEvents btnSetPoster As System.Windows.Forms.Button
    Friend WithEvents pbPoster As System.Windows.Forms.PictureBox
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents btnSetFanartDL As System.Windows.Forms.Button
    Friend WithEvents btnRemoveFanart As System.Windows.Forms.Button
    Friend WithEvents lblFanartSize As System.Windows.Forms.Label
    Friend WithEvents btnSetFanartScrape As System.Windows.Forms.Button
    Friend WithEvents btnSetFanart As System.Windows.Forms.Button
    Friend WithEvents pbFanart As System.Windows.Forms.PictureBox
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents ofdImage As System.Windows.Forms.OpenFileDialog

End Class
