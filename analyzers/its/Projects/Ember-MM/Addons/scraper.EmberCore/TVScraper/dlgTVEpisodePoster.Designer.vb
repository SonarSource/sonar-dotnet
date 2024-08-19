<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgTVEpisodePoster
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
		Me.Cancel_Button = New System.Windows.Forms.Button()
		Me.pbPoster = New System.Windows.Forms.PictureBox()
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Location = New System.Drawing.Point(292, 287)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Location = New System.Drawing.Point(362, 287)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'pbPoster
		'
		Me.pbPoster.BackColor = System.Drawing.Color.DimGray
		Me.pbPoster.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pbPoster.Location = New System.Drawing.Point(6, 6)
		Me.pbPoster.Name = "pbPoster"
		Me.pbPoster.Size = New System.Drawing.Size(423, 276)
		Me.pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbPoster.TabIndex = 2
		Me.pbPoster.TabStop = False
		'
		'dlgTVEpisodePoster
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(435, 315)
		Me.ControlBox = False
		Me.Controls.Add(Me.pbPoster)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgTVEpisodePoster"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Confirm Episode Poster"
		CType(Me.pbPoster, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents pbPoster As System.Windows.Forms.PictureBox

End Class
