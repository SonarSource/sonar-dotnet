<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmNotify
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmNotify))
		Me.pbIcon = New System.Windows.Forms.PictureBox()
		Me.lblTitle = New System.Windows.Forms.Label()
		Me.lblMessage = New System.Windows.Forms.Label()
		CType(Me.pbIcon, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'pbIcon
		'
		Me.pbIcon.BackColor = System.Drawing.Color.Transparent
		Me.pbIcon.Location = New System.Drawing.Point(8, 8)
		Me.pbIcon.Name = "pbIcon"
		Me.pbIcon.Size = New System.Drawing.Size(64, 64)
		Me.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbIcon.TabIndex = 0
		Me.pbIcon.TabStop = False
		'
		'lblTitle
		'
		Me.lblTitle.BackColor = System.Drawing.Color.Transparent
		Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblTitle.Location = New System.Drawing.Point(78, 8)
		Me.lblTitle.Name = "lblTitle"
		Me.lblTitle.Size = New System.Drawing.Size(230, 18)
		Me.lblTitle.TabIndex = 0
		Me.lblTitle.Text = "Title"
		Me.lblTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter
		'
		'lblMessage
		'
		Me.lblMessage.BackColor = System.Drawing.Color.Transparent
		Me.lblMessage.Location = New System.Drawing.Point(78, 31)
		Me.lblMessage.Name = "lblMessage"
		Me.lblMessage.Size = New System.Drawing.Size(230, 39)
		Me.lblMessage.TabIndex = 1
		Me.lblMessage.Text = "Message"
		Me.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter
		'
		'frmNotify
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
		Me.ClientSize = New System.Drawing.Size(315, 80)
		Me.Controls.Add(Me.lblMessage)
		Me.Controls.Add(Me.lblTitle)
		Me.Controls.Add(Me.pbIcon)
		Me.Cursor = System.Windows.Forms.Cursors.Hand
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
		Me.Name = "frmNotify"
		Me.ShowInTaskbar = False
		CType(Me.pbIcon, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pbIcon As System.Windows.Forms.PictureBox
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblMessage As System.Windows.Forms.Label

End Class
