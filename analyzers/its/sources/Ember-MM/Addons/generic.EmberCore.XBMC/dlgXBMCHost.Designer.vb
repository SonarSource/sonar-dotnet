<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgXBMCHost
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
		Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.txtName = New System.Windows.Forms.TextBox()
		Me.Label16 = New System.Windows.Forms.Label()
		Me.txtPassword = New System.Windows.Forms.TextBox()
		Me.txtUsername = New System.Windows.Forms.TextBox()
		Me.Label13 = New System.Windows.Forms.Label()
		Me.Label14 = New System.Windows.Forms.Label()
		Me.Label7 = New System.Windows.Forms.Label()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.txtPort = New System.Windows.Forms.TextBox()
		Me.txtIP = New System.Windows.Forms.TextBox()
		Me.btnPopulate = New System.Windows.Forms.Button()
		Me.dgvSources = New System.Windows.Forms.DataGridView()
		Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
		Me.Column2 = New System.Windows.Forms.DataGridViewComboBoxColumn()
		Me.pnlLoading = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.chkRealTime = New System.Windows.Forms.CheckBox()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.rbLinux = New System.Windows.Forms.RadioButton()
		Me.rbWindows = New System.Windows.Forms.RadioButton()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.TableLayoutPanel1.SuspendLayout()
		CType(Me.dgvSources, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.pnlLoading.SuspendLayout()
		Me.GroupBox1.SuspendLayout()
		Me.Panel1.SuspendLayout()
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
		Me.TableLayoutPanel1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.TableLayoutPanel1.Location = New System.Drawing.Point(315, 321)
		Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
		Me.TableLayoutPanel1.RowCount = 1
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
		Me.TableLayoutPanel1.TabIndex = 0
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.OK_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'txtName
		'
		Me.txtName.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtName.Location = New System.Drawing.Point(70, 2)
		Me.txtName.Name = "txtName"
		Me.txtName.Size = New System.Drawing.Size(227, 22)
		Me.txtName.TabIndex = 1
		'
		'Label16
		'
		Me.Label16.AutoSize = True
		Me.Label16.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label16.Location = New System.Drawing.Point(26, 6)
		Me.Label16.Name = "Label16"
		Me.Label16.Size = New System.Drawing.Size(41, 13)
		Me.Label16.TabIndex = 0
		Me.Label16.Text = "Name:"
		'
		'txtPassword
		'
		Me.txtPassword.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtPassword.Location = New System.Drawing.Point(227, 58)
		Me.txtPassword.Name = "txtPassword"
		Me.txtPassword.Size = New System.Drawing.Size(70, 22)
		Me.txtPassword.TabIndex = 10
		Me.txtPassword.UseSystemPasswordChar = True
		'
		'txtUsername
		'
		Me.txtUsername.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtUsername.Location = New System.Drawing.Point(70, 58)
		Me.txtUsername.Name = "txtUsername"
		Me.txtUsername.Size = New System.Drawing.Size(70, 22)
		Me.txtUsername.TabIndex = 8
		'
		'Label13
		'
		Me.Label13.AutoSize = True
		Me.Label13.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label13.Location = New System.Drawing.Point(4, 63)
		Me.Label13.Name = "Label13"
		Me.Label13.Size = New System.Drawing.Size(62, 13)
		Me.Label13.TabIndex = 7
		Me.Label13.Text = "Username:"
		'
		'Label14
		'
		Me.Label14.AutoSize = True
		Me.Label14.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label14.Location = New System.Drawing.Point(167, 65)
		Me.Label14.Name = "Label14"
		Me.Label14.Size = New System.Drawing.Size(60, 13)
		Me.Label14.TabIndex = 9
		Me.Label14.Text = "Password:"
		'
		'Label7
		'
		Me.Label7.AutoSize = True
		Me.Label7.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label7.Location = New System.Drawing.Point(15, 35)
		Me.Label7.Name = "Label7"
		Me.Label7.Size = New System.Drawing.Size(55, 13)
		Me.Label7.TabIndex = 3
		Me.Label7.Text = "XBMC IP:"
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label6.Location = New System.Drawing.Point(179, 33)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(67, 13)
		Me.Label6.TabIndex = 5
		Me.Label6.Text = "XBMC Port:"
		'
		'txtPort
		'
		Me.txtPort.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtPort.Location = New System.Drawing.Point(246, 30)
		Me.txtPort.Name = "txtPort"
		Me.txtPort.Size = New System.Drawing.Size(51, 22)
		Me.txtPort.TabIndex = 6
		'
		'txtIP
		'
		Me.txtIP.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtIP.Location = New System.Drawing.Point(70, 30)
		Me.txtIP.Name = "txtIP"
		Me.txtIP.Size = New System.Drawing.Size(103, 22)
		Me.txtIP.TabIndex = 4
		'
		'btnPopulate
		'
		Me.btnPopulate.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.btnPopulate.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnPopulate.Location = New System.Drawing.Point(344, 123)
		Me.btnPopulate.Name = "btnPopulate"
		Me.btnPopulate.Size = New System.Drawing.Size(108, 23)
		Me.btnPopulate.TabIndex = 12
		Me.btnPopulate.Text = "Populate Sources"
		Me.btnPopulate.UseVisualStyleBackColor = True
		'
		'dgvSources
		'
		Me.dgvSources.AllowUserToAddRows = False
		Me.dgvSources.AllowUserToDeleteRows = False
		Me.dgvSources.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvSources.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2})
		Me.dgvSources.Enabled = False
		Me.dgvSources.Location = New System.Drawing.Point(12, 152)
		Me.dgvSources.MultiSelect = False
		Me.dgvSources.Name = "dgvSources"
		Me.dgvSources.RowHeadersVisible = False
		Me.dgvSources.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
		Me.dgvSources.ShowCellErrors = False
		Me.dgvSources.ShowCellToolTips = False
		Me.dgvSources.ShowEditingIcon = False
		Me.dgvSources.ShowRowErrors = False
		Me.dgvSources.Size = New System.Drawing.Size(440, 150)
		Me.dgvSources.TabIndex = 13
		'
		'Column1
		'
		Me.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
		Me.Column1.FillWeight = 150.0!
		Me.Column1.HeaderText = "Ember Source"
		Me.Column1.Name = "Column1"
		Me.Column1.ReadOnly = True
		Me.Column1.Width = 150
		'
		'Column2
		'
		Me.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
		Me.Column2.FillWeight = 270.0!
		Me.Column2.HeaderText = "XBMC Source"
		Me.Column2.Name = "Column2"
		Me.Column2.Width = 270
		'
		'pnlLoading
		'
		Me.pnlLoading.BackColor = System.Drawing.Color.White
		Me.pnlLoading.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlLoading.Controls.Add(Me.Label3)
		Me.pnlLoading.Controls.Add(Me.ProgressBar1)
		Me.pnlLoading.Location = New System.Drawing.Point(131, 186)
		Me.pnlLoading.Name = "pnlLoading"
		Me.pnlLoading.Size = New System.Drawing.Size(200, 54)
		Me.pnlLoading.TabIndex = 14
		Me.pnlLoading.Visible = False
		'
		'Label3
		'
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label3.Location = New System.Drawing.Point(3, 5)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(192, 15)
		Me.Label3.TabIndex = 0
		Me.Label3.Text = "Searching ..."
		Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'ProgressBar1
		'
		Me.ProgressBar1.Location = New System.Drawing.Point(3, 32)
		Me.ProgressBar1.MarqueeAnimationSpeed = 25
		Me.ProgressBar1.Name = "ProgressBar1"
		Me.ProgressBar1.Size = New System.Drawing.Size(192, 17)
		Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.ProgressBar1.TabIndex = 1
		'
		'chkRealTime
		'
		Me.chkRealTime.AutoSize = True
		Me.chkRealTime.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkRealTime.Location = New System.Drawing.Point(303, 5)
		Me.chkRealTime.Name = "chkRealTime"
		Me.chkRealTime.Size = New System.Drawing.Size(162, 17)
		Me.chkRealTime.TabIndex = 2
		Me.chkRealTime.Text = "Real Time synchronization "
		Me.chkRealTime.UseVisualStyleBackColor = True
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.rbLinux)
		Me.GroupBox1.Controls.Add(Me.rbWindows)
		Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(16, 87)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(281, 59)
		Me.GroupBox1.TabIndex = 11
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "XBMC Source type"
		'
		'rbLinux
		'
		Me.rbLinux.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.rbLinux.Location = New System.Drawing.Point(14, 37)
		Me.rbLinux.Name = "rbLinux"
		Me.rbLinux.Size = New System.Drawing.Size(216, 19)
		Me.rbLinux.TabIndex = 1
		Me.rbLinux.Text = "Windows UNC/Linux/MacOS X"
		Me.rbLinux.UseVisualStyleBackColor = True
		'
		'rbWindows
		'
		Me.rbWindows.Checked = True
		Me.rbWindows.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.rbWindows.Location = New System.Drawing.Point(14, 15)
		Me.rbWindows.Name = "rbWindows"
		Me.rbWindows.Size = New System.Drawing.Size(216, 23)
		Me.rbWindows.TabIndex = 0
		Me.rbWindows.TabStop = True
		Me.rbWindows.Text = "Windows Drive Letter (X:\)"
		Me.rbWindows.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.pnlLoading)
		Me.Panel1.Controls.Add(Me.GroupBox1)
		Me.Panel1.Controls.Add(Me.chkRealTime)
		Me.Panel1.Controls.Add(Me.dgvSources)
		Me.Panel1.Controls.Add(Me.btnPopulate)
		Me.Panel1.Controls.Add(Me.txtName)
		Me.Panel1.Controls.Add(Me.Label16)
		Me.Panel1.Controls.Add(Me.txtPassword)
		Me.Panel1.Controls.Add(Me.txtUsername)
		Me.Panel1.Controls.Add(Me.Label13)
		Me.Panel1.Controls.Add(Me.Label14)
		Me.Panel1.Controls.Add(Me.Label7)
		Me.Panel1.Controls.Add(Me.Label6)
		Me.Panel1.Controls.Add(Me.txtPort)
		Me.Panel1.Controls.Add(Me.txtIP)
		Me.Panel1.Location = New System.Drawing.Point(3, 4)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(467, 311)
		Me.Panel1.TabIndex = 1
		'
		'dlgXBMCHost
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(473, 351)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.TableLayoutPanel1)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgXBMCHost"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "XBMC Host"
		Me.TableLayoutPanel1.ResumeLayout(False)
		CType(Me.dgvSources, System.ComponentModel.ISupportInitialize).EndInit()
		Me.pnlLoading.ResumeLayout(False)
		Me.GroupBox1.ResumeLayout(False)
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtUsername As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtPort As System.Windows.Forms.TextBox
    Friend WithEvents txtIP As System.Windows.Forms.TextBox
    Friend WithEvents btnPopulate As System.Windows.Forms.Button
    Friend WithEvents dgvSources As System.Windows.Forms.DataGridView
    Friend WithEvents pnlLoading As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents chkRealTime As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents rbLinux As System.Windows.Forms.RadioButton
    Friend WithEvents rbWindows As System.Windows.Forms.RadioButton
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewComboBoxColumn

End Class
