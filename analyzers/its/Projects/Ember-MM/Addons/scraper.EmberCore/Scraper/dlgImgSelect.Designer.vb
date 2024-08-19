<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgImgSelect
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgImgSelect))
		Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.pnlBG = New System.Windows.Forms.Panel()
		Me.pnlBottomMain = New System.Windows.Forms.Panel()
		Me.pnlSize = New System.Windows.Forms.Panel()
		Me.btnPreview = New System.Windows.Forms.Button()
		Me.rbSmall = New System.Windows.Forms.RadioButton()
		Me.rbMedium = New System.Windows.Forms.RadioButton()
		Me.rbLarge = New System.Windows.Forms.RadioButton()
		Me.rbXLarge = New System.Windows.Forms.RadioButton()
		Me.pnlFanart = New System.Windows.Forms.Panel()
		Me.chkThumb = New System.Windows.Forms.CheckBox()
		Me.chkMid = New System.Windows.Forms.CheckBox()
		Me.chkOriginal = New System.Windows.Forms.CheckBox()
		Me.lblInfo = New System.Windows.Forms.Label()
		Me.pnlDLStatus = New System.Windows.Forms.Panel()
		Me.pnlMPDB = New System.Windows.Forms.Panel()
		Me.lblDL3Status = New System.Windows.Forms.Label()
		Me.lblDL3 = New System.Windows.Forms.Label()
		Me.pbDL3 = New System.Windows.Forms.ProgressBar()
		Me.pnlIMPA = New System.Windows.Forms.Panel()
		Me.lblDL2Status = New System.Windows.Forms.Label()
		Me.lblDL2 = New System.Windows.Forms.Label()
		Me.pbDL2 = New System.Windows.Forms.ProgressBar()
		Me.lblDL1Status = New System.Windows.Forms.Label()
		Me.lblDL1 = New System.Windows.Forms.Label()
		Me.pbDL1 = New System.Windows.Forms.ProgressBar()
		Me.pnlSinglePic = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.TableLayoutPanel1.SuspendLayout()
		Me.pnlBottomMain.SuspendLayout()
		Me.pnlSize.SuspendLayout()
		Me.pnlFanart.SuspendLayout()
		Me.pnlDLStatus.SuspendLayout()
		Me.pnlMPDB.SuspendLayout()
		Me.pnlIMPA.SuspendLayout()
		Me.pnlSinglePic.SuspendLayout()
		Me.SuspendLayout()
		'
		'TableLayoutPanel1
		'
		Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TableLayoutPanel1.ColumnCount = 2
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
		Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
		Me.TableLayoutPanel1.Location = New System.Drawing.Point(687, 11)
		Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
		Me.TableLayoutPanel1.RowCount = 1
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
		Me.TableLayoutPanel1.TabIndex = 0
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.OK_Button.Enabled = False
		Me.OK_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.OK_Button.Location = New System.Drawing.Point(3, 3)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'pnlBG
		'
		Me.pnlBG.AutoScroll = True
		Me.pnlBG.Dock = System.Windows.Forms.DockStyle.Fill
		Me.pnlBG.Location = New System.Drawing.Point(0, 0)
		Me.pnlBG.Name = "pnlBG"
		Me.pnlBG.Size = New System.Drawing.Size(836, 495)
		Me.pnlBG.TabIndex = 1
		Me.pnlBG.Visible = False
		'
		'pnlBottomMain
		'
		Me.pnlBottomMain.Controls.Add(Me.pnlSize)
		Me.pnlBottomMain.Controls.Add(Me.pnlFanart)
		Me.pnlBottomMain.Controls.Add(Me.lblInfo)
		Me.pnlBottomMain.Controls.Add(Me.TableLayoutPanel1)
		Me.pnlBottomMain.Dock = System.Windows.Forms.DockStyle.Bottom
		Me.pnlBottomMain.Location = New System.Drawing.Point(0, 495)
		Me.pnlBottomMain.Name = "pnlBottomMain"
		Me.pnlBottomMain.Size = New System.Drawing.Size(836, 50)
		Me.pnlBottomMain.TabIndex = 0
		'
		'pnlSize
		'
		Me.pnlSize.BackColor = System.Drawing.Color.White
		Me.pnlSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.pnlSize.Controls.Add(Me.btnPreview)
		Me.pnlSize.Controls.Add(Me.rbSmall)
		Me.pnlSize.Controls.Add(Me.rbMedium)
		Me.pnlSize.Controls.Add(Me.rbLarge)
		Me.pnlSize.Controls.Add(Me.rbXLarge)
		Me.pnlSize.Location = New System.Drawing.Point(8, 8)
		Me.pnlSize.Name = "pnlSize"
		Me.pnlSize.Size = New System.Drawing.Size(673, 34)
		Me.pnlSize.TabIndex = 1
		Me.pnlSize.Visible = False
		'
		'btnPreview
		'
		Me.btnPreview.Enabled = False
		Me.btnPreview.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnPreview.Image = CType(resources.GetObject("btnPreview.Image"), System.Drawing.Image)
		Me.btnPreview.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnPreview.Location = New System.Drawing.Point(593, 5)
		Me.btnPreview.Name = "btnPreview"
		Me.btnPreview.Size = New System.Drawing.Size(75, 23)
		Me.btnPreview.TabIndex = 4
		Me.btnPreview.Text = "Preview"
		Me.btnPreview.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnPreview.UseVisualStyleBackColor = True
		'
		'rbSmall
		'
		Me.rbSmall.AutoSize = True
		Me.rbSmall.Enabled = False
		Me.rbSmall.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbSmall.Location = New System.Drawing.Point(475, 8)
		Me.rbSmall.Name = "rbSmall"
		Me.rbSmall.Size = New System.Drawing.Size(53, 17)
		Me.rbSmall.TabIndex = 3
		Me.rbSmall.TabStop = True
		Me.rbSmall.Text = "Small"
		Me.rbSmall.UseVisualStyleBackColor = True
		'
		'rbMedium
		'
		Me.rbMedium.AutoSize = True
		Me.rbMedium.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbMedium.Location = New System.Drawing.Point(201, 8)
		Me.rbMedium.Name = "rbMedium"
		Me.rbMedium.Size = New System.Drawing.Size(69, 17)
		Me.rbMedium.TabIndex = 1
		Me.rbMedium.TabStop = True
		Me.rbMedium.Text = "Medium"
		Me.rbMedium.UseVisualStyleBackColor = True
		'
		'rbLarge
		'
		Me.rbLarge.AutoSize = True
		Me.rbLarge.Enabled = False
		Me.rbLarge.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbLarge.Location = New System.Drawing.Point(343, 8)
		Me.rbLarge.Name = "rbLarge"
		Me.rbLarge.Size = New System.Drawing.Size(55, 17)
		Me.rbLarge.TabIndex = 2
		Me.rbLarge.TabStop = True
		Me.rbLarge.Text = "Cover"
		Me.rbLarge.UseVisualStyleBackColor = True
		'
		'rbXLarge
		'
		Me.rbXLarge.AutoSize = True
		Me.rbXLarge.Enabled = False
		Me.rbXLarge.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.rbXLarge.Location = New System.Drawing.Point(26, 8)
		Me.rbXLarge.Name = "rbXLarge"
		Me.rbXLarge.Size = New System.Drawing.Size(66, 17)
		Me.rbXLarge.TabIndex = 0
		Me.rbXLarge.TabStop = True
		Me.rbXLarge.Text = "Original"
		Me.rbXLarge.UseVisualStyleBackColor = True
		'
		'pnlFanart
		'
		Me.pnlFanart.BackColor = System.Drawing.Color.White
		Me.pnlFanart.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlFanart.Controls.Add(Me.chkThumb)
		Me.pnlFanart.Controls.Add(Me.chkMid)
		Me.pnlFanart.Controls.Add(Me.chkOriginal)
		Me.pnlFanart.Location = New System.Drawing.Point(8, 8)
		Me.pnlFanart.Name = "pnlFanart"
		Me.pnlFanart.Size = New System.Drawing.Size(339, 34)
		Me.pnlFanart.TabIndex = 5
		Me.pnlFanart.Visible = False
		'
		'chkThumb
		'
		Me.chkThumb.AutoSize = True
		Me.chkThumb.Location = New System.Drawing.Point(231, 8)
		Me.chkThumb.Name = "chkThumb"
		Me.chkThumb.Size = New System.Drawing.Size(111, 17)
		Me.chkThumb.TabIndex = 2
		Me.chkThumb.Text = "Check All Thumb"
		Me.chkThumb.UseVisualStyleBackColor = True
		'
		'chkMid
		'
		Me.chkMid.AutoSize = True
		Me.chkMid.Location = New System.Drawing.Point(131, 8)
		Me.chkMid.Name = "chkMid"
		Me.chkMid.Size = New System.Drawing.Size(96, 17)
		Me.chkMid.TabIndex = 1
		Me.chkMid.Text = "Check All Mid"
		Me.chkMid.UseVisualStyleBackColor = True
		'
		'chkOriginal
		'
		Me.chkOriginal.AutoSize = True
		Me.chkOriginal.Location = New System.Drawing.Point(7, 8)
		Me.chkOriginal.Name = "chkOriginal"
		Me.chkOriginal.Size = New System.Drawing.Size(118, 17)
		Me.chkOriginal.TabIndex = 0
		Me.chkOriginal.Text = "Check All Original"
		Me.chkOriginal.UseVisualStyleBackColor = True
		'
		'lblInfo
		'
		Me.lblInfo.Location = New System.Drawing.Point(402, 10)
		Me.lblInfo.Name = "lblInfo"
		Me.lblInfo.Size = New System.Drawing.Size(240, 31)
		Me.lblInfo.TabIndex = 1
		Me.lblInfo.Text = "Selected item will be set as fanart. All checked items will be saved to \extrathu" & _
		  "mbs."
		Me.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblInfo.Visible = False
		'
		'pnlDLStatus
		'
		Me.pnlDLStatus.BackColor = System.Drawing.Color.White
		Me.pnlDLStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlDLStatus.Controls.Add(Me.pnlMPDB)
		Me.pnlDLStatus.Controls.Add(Me.pnlIMPA)
		Me.pnlDLStatus.Controls.Add(Me.lblDL1Status)
		Me.pnlDLStatus.Controls.Add(Me.lblDL1)
		Me.pnlDLStatus.Controls.Add(Me.pbDL1)
		Me.pnlDLStatus.Location = New System.Drawing.Point(257, 132)
		Me.pnlDLStatus.Name = "pnlDLStatus"
		Me.pnlDLStatus.Size = New System.Drawing.Size(321, 223)
		Me.pnlDLStatus.TabIndex = 2
		Me.pnlDLStatus.Visible = False
		'
		'pnlMPDB
		'
		Me.pnlMPDB.Controls.Add(Me.lblDL3Status)
		Me.pnlMPDB.Controls.Add(Me.lblDL3)
		Me.pnlMPDB.Controls.Add(Me.pbDL3)
		Me.pnlMPDB.Location = New System.Drawing.Point(0, 147)
		Me.pnlMPDB.Name = "pnlMPDB"
		Me.pnlMPDB.Size = New System.Drawing.Size(321, 75)
		Me.pnlMPDB.TabIndex = 4
		'
		'lblDL3Status
		'
		Me.lblDL3Status.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDL3Status.Location = New System.Drawing.Point(5, 34)
		Me.lblDL3Status.Name = "lblDL3Status"
		Me.lblDL3Status.Size = New System.Drawing.Size(310, 13)
		Me.lblDL3Status.TabIndex = 1
		'
		'lblDL3
		'
		Me.lblDL3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDL3.Location = New System.Drawing.Point(5, 10)
		Me.lblDL3.Name = "lblDL3"
		Me.lblDL3.Size = New System.Drawing.Size(310, 13)
		Me.lblDL3.TabIndex = 0
		Me.lblDL3.Text = "Performing Preliminary Tasks..."
		'
		'pbDL3
		'
		Me.pbDL3.Location = New System.Drawing.Point(6, 53)
		Me.pbDL3.Name = "pbDL3"
		Me.pbDL3.Size = New System.Drawing.Size(309, 19)
		Me.pbDL3.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbDL3.TabIndex = 2
		'
		'pnlIMPA
		'
		Me.pnlIMPA.Controls.Add(Me.pnlSinglePic)
		Me.pnlIMPA.Controls.Add(Me.lblDL2Status)
		Me.pnlIMPA.Controls.Add(Me.lblDL2)
		Me.pnlIMPA.Controls.Add(Me.pbDL2)
		Me.pnlIMPA.Location = New System.Drawing.Point(0, 73)
		Me.pnlIMPA.Name = "pnlIMPA"
		Me.pnlIMPA.Size = New System.Drawing.Size(321, 75)
		Me.pnlIMPA.TabIndex = 3
		'
		'lblDL2Status
		'
		Me.lblDL2Status.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDL2Status.Location = New System.Drawing.Point(5, 34)
		Me.lblDL2Status.Name = "lblDL2Status"
		Me.lblDL2Status.Size = New System.Drawing.Size(310, 13)
		Me.lblDL2Status.TabIndex = 1
		'
		'lblDL2
		'
		Me.lblDL2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblDL2.Location = New System.Drawing.Point(5, 10)
		Me.lblDL2.Name = "lblDL2"
		Me.lblDL2.Size = New System.Drawing.Size(310, 13)
		Me.lblDL2.TabIndex = 0
		Me.lblDL2.Text = "Performing Preliminary Tasks..."
		'
		'pbDL2
		'
		Me.pbDL2.Location = New System.Drawing.Point(6, 52)
		Me.pbDL2.Name = "pbDL2"
		Me.pbDL2.Size = New System.Drawing.Size(309, 19)
		Me.pbDL2.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbDL2.TabIndex = 2
		'
		'lblDL1Status
		'
		Me.lblDL1Status.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDL1Status.Location = New System.Drawing.Point(5, 29)
		Me.lblDL1Status.Name = "lblDL1Status"
		Me.lblDL1Status.Size = New System.Drawing.Size(310, 13)
		Me.lblDL1Status.TabIndex = 1
		'
		'lblDL1
		'
		Me.lblDL1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblDL1.Location = New System.Drawing.Point(5, 6)
		Me.lblDL1.Name = "lblDL1"
		Me.lblDL1.Size = New System.Drawing.Size(310, 13)
		Me.lblDL1.TabIndex = 0
		Me.lblDL1.Text = "Performing Preliminary Tasks..."
		'
		'pbDL1
		'
		Me.pbDL1.Location = New System.Drawing.Point(6, 49)
		Me.pbDL1.Name = "pbDL1"
		Me.pbDL1.Size = New System.Drawing.Size(309, 19)
		Me.pbDL1.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbDL1.TabIndex = 2
		'
		'pnlSinglePic
		'
		Me.pnlSinglePic.BackColor = System.Drawing.Color.White
		Me.pnlSinglePic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlSinglePic.Controls.Add(Me.Label2)
		Me.pnlSinglePic.Controls.Add(Me.ProgressBar1)
		Me.pnlSinglePic.Location = New System.Drawing.Point(0, 1)
		Me.pnlSinglePic.Name = "pnlSinglePic"
		Me.pnlSinglePic.Size = New System.Drawing.Size(321, 75)
		Me.pnlSinglePic.TabIndex = 1
		Me.pnlSinglePic.Visible = False
		'
		'Label2
		'
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.Location = New System.Drawing.Point(5, 10)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(310, 13)
		Me.Label2.TabIndex = 0
		Me.Label2.Text = "Downloading Selected Image..."
		'
		'ProgressBar1
		'
		Me.ProgressBar1.Location = New System.Drawing.Point(6, 53)
		Me.ProgressBar1.MarqueeAnimationSpeed = 25
		Me.ProgressBar1.Name = "ProgressBar1"
		Me.ProgressBar1.Size = New System.Drawing.Size(309, 19)
		Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.ProgressBar1.TabIndex = 1
		'
		'dlgImgSelect
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.AutoScroll = True
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(836, 545)
		Me.ControlBox = False
		Me.Controls.Add(Me.pnlDLStatus)
		Me.Controls.Add(Me.pnlBG)
		Me.Controls.Add(Me.pnlBottomMain)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgImgSelect"
		Me.ShowIcon = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Select Poster"
		Me.TableLayoutPanel1.ResumeLayout(False)
		Me.pnlBottomMain.ResumeLayout(False)
		Me.pnlSize.ResumeLayout(False)
		Me.pnlSize.PerformLayout()
		Me.pnlFanart.ResumeLayout(False)
		Me.pnlFanart.PerformLayout()
		Me.pnlDLStatus.ResumeLayout(False)
		Me.pnlMPDB.ResumeLayout(False)
		Me.pnlIMPA.ResumeLayout(False)
		Me.pnlSinglePic.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents pnlBG As System.Windows.Forms.Panel
    Friend WithEvents pnlBottomMain As System.Windows.Forms.Panel
    Friend WithEvents pnlDLStatus As System.Windows.Forms.Panel
    Friend WithEvents lblDL1Status As System.Windows.Forms.Label
    Friend WithEvents lblDL1 As System.Windows.Forms.Label
    Friend WithEvents pbDL1 As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlIMPA As System.Windows.Forms.Panel
    Friend WithEvents lblDL2Status As System.Windows.Forms.Label
    Friend WithEvents lblDL2 As System.Windows.Forms.Label
    Friend WithEvents pbDL2 As System.Windows.Forms.ProgressBar
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents pnlSize As System.Windows.Forms.Panel
    Friend WithEvents rbSmall As System.Windows.Forms.RadioButton
    Friend WithEvents rbMedium As System.Windows.Forms.RadioButton
    Friend WithEvents rbLarge As System.Windows.Forms.RadioButton
    Friend WithEvents rbXLarge As System.Windows.Forms.RadioButton
    Friend WithEvents pnlSinglePic As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlMPDB As System.Windows.Forms.Panel
    Friend WithEvents lblDL3Status As System.Windows.Forms.Label
    Friend WithEvents lblDL3 As System.Windows.Forms.Label
    Friend WithEvents pbDL3 As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlFanart As System.Windows.Forms.Panel
    Friend WithEvents chkThumb As System.Windows.Forms.CheckBox
    Friend WithEvents chkMid As System.Windows.Forms.CheckBox
    Friend WithEvents chkOriginal As System.Windows.Forms.CheckBox
    Friend WithEvents btnPreview As System.Windows.Forms.Button

End Class
