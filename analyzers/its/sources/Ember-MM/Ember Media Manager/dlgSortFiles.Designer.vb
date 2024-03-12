<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgSortFiles
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
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.btnBrowse = New System.Windows.Forms.Button()
		Me.txtPath = New System.Windows.Forms.TextBox()
		Me.btnGo = New System.Windows.Forms.Button()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.lblStatus = New System.Windows.Forms.Label()
		Me.pbStatus = New System.Windows.Forms.ProgressBar()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.fbdBrowse = New System.Windows.Forms.FolderBrowserDialog()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.GroupBox1.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(351, 143)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(72, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Close"
		'
		'btnBrowse
		'
		Me.btnBrowse.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnBrowse.Location = New System.Drawing.Point(383, 25)
		Me.btnBrowse.Name = "btnBrowse"
		Me.btnBrowse.Size = New System.Drawing.Size(37, 23)
		Me.btnBrowse.TabIndex = 2
		Me.btnBrowse.Text = "..."
		Me.btnBrowse.UseVisualStyleBackColor = True
		'
		'txtPath
		'
		Me.txtPath.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtPath.Location = New System.Drawing.Point(9, 26)
		Me.txtPath.Name = "txtPath"
		Me.txtPath.Size = New System.Drawing.Size(365, 22)
		Me.txtPath.TabIndex = 1
		'
		'btnGo
		'
		Me.btnGo.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnGo.Location = New System.Drawing.Point(12, 143)
		Me.btnGo.Name = "btnGo"
		Me.btnGo.Size = New System.Drawing.Size(72, 24)
		Me.btnGo.TabIndex = 0
		Me.btnGo.Text = "Go"
		Me.btnGo.UseVisualStyleBackColor = True
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.lblStatus)
		Me.GroupBox1.Controls.Add(Me.pbStatus)
		Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(9, 52)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(411, 79)
		Me.GroupBox1.TabIndex = 3
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Status"
		'
		'lblStatus
		'
		Me.lblStatus.AutoSize = True
		Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblStatus.Location = New System.Drawing.Point(6, 25)
		Me.lblStatus.Name = "lblStatus"
		Me.lblStatus.Size = New System.Drawing.Size(192, 13)
		Me.lblStatus.TabIndex = 0
		Me.lblStatus.Text = "Enter Path and Press ""Go"" to Begin."
		'
		'pbStatus
		'
		Me.pbStatus.Location = New System.Drawing.Point(6, 45)
		Me.pbStatus.Name = "pbStatus"
		Me.pbStatus.Size = New System.Drawing.Size(399, 23)
		Me.pbStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbStatus.TabIndex = 1
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.Location = New System.Drawing.Point(6, 6)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(72, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Path to Sort:"
		'
		'fbdBrowse
		'
		Me.fbdBrowse.Description = "Select the folder which contains the files you wish to sort."
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.Label1)
		Me.Panel1.Controls.Add(Me.GroupBox1)
		Me.Panel1.Controls.Add(Me.txtPath)
		Me.Panel1.Controls.Add(Me.btnBrowse)
		Me.Panel1.Location = New System.Drawing.Point(3, 3)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(429, 137)
		Me.Panel1.TabIndex = 2
		'
		'dlgSortFiles
		'
		Me.AcceptButton = Me.btnGo
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(435, 169)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.btnGo)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgSortFiles"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Sort Files Into Folders"
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents txtPath As System.Windows.Forms.TextBox
    Friend WithEvents btnGo As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents pbStatus As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents fbdBrowse As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
