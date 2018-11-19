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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSettingsHolder))
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.GroupBox11 = New System.Windows.Forms.GroupBox()
		Me.chkNotification = New System.Windows.Forms.CheckBox()
		Me.cbPlayCountHost = New System.Windows.Forms.ComboBox()
		Me.chkPlayCount = New System.Windows.Forms.CheckBox()
		Me.chkRealTime = New System.Windows.Forms.CheckBox()
		Me.btnEditCom = New System.Windows.Forms.Button()
		Me.btnRemoveCom = New System.Windows.Forms.Button()
		Me.lbXBMCCom = New System.Windows.Forms.ListBox()
		Me.btnAddCom = New System.Windows.Forms.Button()
		Me.Panel1.SuspendLayout()
		Me.pnlSettings.SuspendLayout()
		Me.GroupBox11.SuspendLayout()
		Me.SuspendLayout()
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.cbEnabled)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(617, 25)
		Me.Panel1.TabIndex = 0
		'
		'cbEnabled
		'
		Me.cbEnabled.AutoSize = True
		Me.cbEnabled.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.cbEnabled.Location = New System.Drawing.Point(10, 5)
		Me.cbEnabled.Name = "cbEnabled"
		Me.cbEnabled.Size = New System.Drawing.Size(68, 17)
		Me.cbEnabled.TabIndex = 0
		Me.cbEnabled.Text = "Enabled"
		Me.cbEnabled.UseVisualStyleBackColor = True
		'
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.GroupBox11)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.pnlSettings.Location = New System.Drawing.Point(13, 15)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 0
		'
		'GroupBox11
		'
		Me.GroupBox11.Controls.Add(Me.chkNotification)
		Me.GroupBox11.Controls.Add(Me.cbPlayCountHost)
		Me.GroupBox11.Controls.Add(Me.chkPlayCount)
		Me.GroupBox11.Controls.Add(Me.chkRealTime)
		Me.GroupBox11.Controls.Add(Me.btnEditCom)
		Me.GroupBox11.Controls.Add(Me.btnRemoveCom)
		Me.GroupBox11.Controls.Add(Me.lbXBMCCom)
		Me.GroupBox11.Controls.Add(Me.btnAddCom)
		Me.GroupBox11.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox11.Location = New System.Drawing.Point(3, 28)
		Me.GroupBox11.Name = "GroupBox11"
		Me.GroupBox11.Size = New System.Drawing.Size(611, 299)
		Me.GroupBox11.TabIndex = 1
		Me.GroupBox11.TabStop = False
		Me.GroupBox11.Text = "XBMC Communication"
		'
		'chkNotification
		'
		Me.chkNotification.AutoSize = True
		Me.chkNotification.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkNotification.Location = New System.Drawing.Point(319, 92)
		Me.chkNotification.Name = "chkNotification"
		Me.chkNotification.Size = New System.Drawing.Size(121, 17)
		Me.chkNotification.TabIndex = 7
		Me.chkNotification.Text = "Send Notifications"
		Me.chkNotification.UseVisualStyleBackColor = True
		'
		'cbPlayCountHost
		'
		Me.cbPlayCountHost.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbPlayCountHost.FormattingEnabled = True
		Me.cbPlayCountHost.Location = New System.Drawing.Point(337, 61)
		Me.cbPlayCountHost.Name = "cbPlayCountHost"
		Me.cbPlayCountHost.Size = New System.Drawing.Size(121, 21)
		Me.cbPlayCountHost.TabIndex = 6
		'
		'chkPlayCount
		'
		Me.chkPlayCount.AutoSize = True
		Me.chkPlayCount.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkPlayCount.Location = New System.Drawing.Point(319, 38)
		Me.chkPlayCount.Name = "chkPlayCount"
		Me.chkPlayCount.Size = New System.Drawing.Size(152, 17)
		Me.chkPlayCount.TabIndex = 5
		Me.chkPlayCount.Text = "Retrieve PlayCount from:"
		Me.chkPlayCount.UseVisualStyleBackColor = True
		'
		'chkRealTime
		'
		Me.chkRealTime.AutoSize = True
		Me.chkRealTime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRealTime.Location = New System.Drawing.Point(319, 15)
		Me.chkRealTime.Name = "chkRealTime"
		Me.chkRealTime.Size = New System.Drawing.Size(200, 17)
		Me.chkRealTime.TabIndex = 4
		Me.chkRealTime.Text = "Enable Real Time synchronization "
		Me.chkRealTime.UseVisualStyleBackColor = True
		'
		'btnEditCom
		'
		Me.btnEditCom.Enabled = False
		Me.btnEditCom.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnEditCom.Image = CType(resources.GetObject("btnEditCom.Image"), System.Drawing.Image)
		Me.btnEditCom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnEditCom.Location = New System.Drawing.Point(199, 182)
		Me.btnEditCom.Name = "btnEditCom"
		Me.btnEditCom.Size = New System.Drawing.Size(91, 23)
		Me.btnEditCom.TabIndex = 3
		Me.btnEditCom.Text = "Edit"
		Me.btnEditCom.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnEditCom.UseVisualStyleBackColor = True
		'
		'btnRemoveCom
		'
		Me.btnRemoveCom.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnRemoveCom.Image = CType(resources.GetObject("btnRemoveCom.Image"), System.Drawing.Image)
		Me.btnRemoveCom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnRemoveCom.Location = New System.Drawing.Point(106, 182)
		Me.btnRemoveCom.Name = "btnRemoveCom"
		Me.btnRemoveCom.Size = New System.Drawing.Size(87, 23)
		Me.btnRemoveCom.TabIndex = 2
		Me.btnRemoveCom.Text = "Remove"
		Me.btnRemoveCom.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnRemoveCom.UseVisualStyleBackColor = True
		'
		'lbXBMCCom
		'
		Me.lbXBMCCom.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lbXBMCCom.FormattingEnabled = True
		Me.lbXBMCCom.Location = New System.Drawing.Point(13, 15)
		Me.lbXBMCCom.Name = "lbXBMCCom"
		Me.lbXBMCCom.Size = New System.Drawing.Size(283, 160)
		Me.lbXBMCCom.Sorted = True
		Me.lbXBMCCom.TabIndex = 0
		'
		'btnAddCom
		'
		Me.btnAddCom.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnAddCom.Image = CType(resources.GetObject("btnAddCom.Image"), System.Drawing.Image)
		Me.btnAddCom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnAddCom.Location = New System.Drawing.Point(13, 182)
		Me.btnAddCom.Name = "btnAddCom"
		Me.btnAddCom.Size = New System.Drawing.Size(87, 23)
		Me.btnAddCom.TabIndex = 1
		Me.btnAddCom.Text = "Add"
		Me.btnAddCom.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnAddCom.UseVisualStyleBackColor = True
		'
		'frmSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackColor = System.Drawing.Color.White
		Me.ClientSize = New System.Drawing.Size(653, 366)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "frmSettingsHolder"
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.pnlSettings.ResumeLayout(False)
		Me.GroupBox11.ResumeLayout(False)
		Me.GroupBox11.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents GroupBox11 As System.Windows.Forms.GroupBox
    Friend WithEvents btnEditCom As System.Windows.Forms.Button
    Friend WithEvents btnRemoveCom As System.Windows.Forms.Button
    Friend WithEvents lbXBMCCom As System.Windows.Forms.ListBox
    Friend WithEvents btnAddCom As System.Windows.Forms.Button
    Friend WithEvents chkRealTime As System.Windows.Forms.CheckBox
    Friend WithEvents cbPlayCountHost As System.Windows.Forms.ComboBox
    Friend WithEvents chkPlayCount As System.Windows.Forms.CheckBox
    Friend WithEvents chkNotification As System.Windows.Forms.CheckBox

End Class
