<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgTVSource
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
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.txtSourceName = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.txtSourcePath = New System.Windows.Forms.TextBox()
		Me.btnBrowse = New System.Windows.Forms.Button()
		Me.pbValid = New System.Windows.Forms.PictureBox()
		Me.fbdBrowse = New System.Windows.Forms.FolderBrowserDialog()
		Me.tmrWait = New System.Windows.Forms.Timer(Me.components)
		Me.tmrName = New System.Windows.Forms.Timer(Me.components)
		Me.tmrPathWait = New System.Windows.Forms.Timer(Me.components)
		Me.tmrPath = New System.Windows.Forms.Timer(Me.components)
		Me.Panel1 = New System.Windows.Forms.Panel()
		CType(Me.pbValid, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Enabled = False
		Me.OK_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.OK_Button.Location = New System.Drawing.Point(283, 109)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(356, 109)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'txtSourceName
		'
		Me.txtSourceName.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtSourceName.Location = New System.Drawing.Point(11, 25)
		Me.txtSourceName.Name = "txtSourceName"
		Me.txtSourceName.Size = New System.Drawing.Size(130, 22)
		Me.txtSourceName.TabIndex = 4
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.Location = New System.Drawing.Point(9, 10)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(79, 13)
		Me.Label1.TabIndex = 3
		Me.Label1.Text = "Source Name:"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.Location = New System.Drawing.Point(9, 55)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(72, 13)
		Me.Label2.TabIndex = 0
		Me.Label2.Text = "Source Path:"
		'
		'txtSourcePath
		'
		Me.txtSourcePath.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.txtSourcePath.Location = New System.Drawing.Point(11, 70)
		Me.txtSourcePath.Name = "txtSourcePath"
		Me.txtSourcePath.Size = New System.Drawing.Size(376, 22)
		Me.txtSourcePath.TabIndex = 1
		'
		'btnBrowse
		'
		Me.btnBrowse.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnBrowse.Location = New System.Drawing.Point(393, 69)
		Me.btnBrowse.Name = "btnBrowse"
		Me.btnBrowse.Size = New System.Drawing.Size(26, 23)
		Me.btnBrowse.TabIndex = 2
		Me.btnBrowse.Text = "..."
		Me.btnBrowse.UseVisualStyleBackColor = True
		'
		'pbValid
		'
		Me.pbValid.Image = Global.Ember_Media_Manager.My.Resources.Resources.invalid
		Me.pbValid.Location = New System.Drawing.Point(147, 27)
		Me.pbValid.Name = "pbValid"
		Me.pbValid.Size = New System.Drawing.Size(16, 16)
		Me.pbValid.TabIndex = 7
		Me.pbValid.TabStop = False
		'
		'fbdBrowse
		'
		Me.fbdBrowse.Description = "Select the parent folder for your movie folders/files."
		'
		'tmrWait
		'
		Me.tmrWait.Interval = 250
		'
		'tmrName
		'
		Me.tmrName.Interval = 250
		'
		'tmrPathWait
		'
		Me.tmrPathWait.Interval = 250
		'
		'tmrPath
		'
		Me.tmrPath.Interval = 250
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.pbValid)
		Me.Panel1.Controls.Add(Me.btnBrowse)
		Me.Panel1.Controls.Add(Me.Label2)
		Me.Panel1.Controls.Add(Me.txtSourcePath)
		Me.Panel1.Controls.Add(Me.Label1)
		Me.Panel1.Controls.Add(Me.txtSourceName)
		Me.Panel1.Location = New System.Drawing.Point(4, 4)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(427, 101)
		Me.Panel1.TabIndex = 2
		'
		'dlgTVSource
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(435, 136)
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgTVSource"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "TV Source"
		Me.TopMost = True
		CType(Me.pbValid, System.ComponentModel.ISupportInitialize).EndInit()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents txtSourceName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtSourcePath As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents pbValid As System.Windows.Forms.PictureBox
    Friend WithEvents fbdBrowse As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents tmrWait As System.Windows.Forms.Timer
    Friend WithEvents tmrName As System.Windows.Forms.Timer
    Friend WithEvents tmrPathWait As System.Windows.Forms.Timer
    Friend WithEvents tmrPath As System.Windows.Forms.Timer
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
