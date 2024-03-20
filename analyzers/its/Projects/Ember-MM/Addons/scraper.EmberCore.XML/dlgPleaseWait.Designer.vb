<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgPleaseWait
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
		Me.Label1 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label1.Location = New System.Drawing.Point(8, 4)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(265, 19)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Downloading details..."
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
		'
		'ProgressBar1
		'
		Me.ProgressBar1.Location = New System.Drawing.Point(28, 26)
		Me.ProgressBar1.Name = "ProgressBar1"
		Me.ProgressBar1.Size = New System.Drawing.Size(225, 10)
		Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.ProgressBar1.TabIndex = 1
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Cancel_Button.Location = New System.Drawing.Point(108, 51)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 0
		Me.Cancel_Button.Text = "Cancel"
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.White
		Me.Panel1.Controls.Add(Me.ProgressBar1)
		Me.Panel1.Controls.Add(Me.Label1)
		Me.Panel1.Location = New System.Drawing.Point(4, 3)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(282, 46)
		Me.Panel1.TabIndex = 1
		'
		'dlgPleaseWait
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(289, 78)
		Me.ControlBox = False
		Me.Controls.Add(Me.Panel1)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgPleaseWait"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "Loading Please Wait"
		Me.TopMost = True
		Me.Panel1.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
