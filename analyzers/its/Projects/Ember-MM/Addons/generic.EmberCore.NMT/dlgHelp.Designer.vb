<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgHelp
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
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.txtHelp = New System.Windows.Forms.TextBox()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.OK_Button.Location = New System.Drawing.Point(226, 394)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'txtHelp
		'
		Me.txtHelp.Location = New System.Drawing.Point(-1, 1)
		Me.txtHelp.Multiline = True
		Me.txtHelp.Name = "txtHelp"
		Me.txtHelp.ReadOnly = True
		Me.txtHelp.Size = New System.Drawing.Size(522, 387)
		Me.txtHelp.TabIndex = 1
		'
		'dlgHelp
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.CancelButton = Me.OK_Button
		Me.ClientSize = New System.Drawing.Size(520, 418)
		Me.Controls.Add(Me.txtHelp)
		Me.Controls.Add(Me.OK_Button)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgHelp"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "Help"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents txtHelp As System.Windows.Forms.TextBox

End Class
