<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgErrorViewer
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgErrorViewer))
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.txtError = New System.Windows.Forms.TextBox()
		Me.btnCopy = New System.Windows.Forms.Button()
		Me.txtPastebinURL = New System.Windows.Forms.TextBox()
		Me.lblPastebinURL = New System.Windows.Forms.Label()
		Me.lblInfo = New System.Windows.Forms.Label()
		Me.llblURL = New System.Windows.Forms.LinkLabel()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.OK_Button.Location = New System.Drawing.Point(797, 381)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'txtError
		'
		Me.txtError.BackColor = System.Drawing.Color.White
		Me.txtError.Location = New System.Drawing.Point(6, 7)
		Me.txtError.Multiline = True
		Me.txtError.Name = "txtError"
		Me.txtError.ReadOnly = True
		Me.txtError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtError.Size = New System.Drawing.Size(858, 331)
		Me.txtError.TabIndex = 1
		'
		'btnCopy
		'
		Me.btnCopy.Location = New System.Drawing.Point(6, 381)
		Me.btnCopy.Name = "btnCopy"
		Me.btnCopy.Size = New System.Drawing.Size(150, 23)
		Me.btnCopy.TabIndex = 2
		Me.btnCopy.Text = "Copy To Clipboard"
		Me.btnCopy.UseVisualStyleBackColor = True
		Me.btnCopy.Visible = False
		'
		'txtPastebinURL
		'
		Me.txtPastebinURL.BackColor = System.Drawing.Color.White
		Me.txtPastebinURL.Location = New System.Drawing.Point(272, 381)
		Me.txtPastebinURL.Name = "txtPastebinURL"
		Me.txtPastebinURL.ReadOnly = True
		Me.txtPastebinURL.Size = New System.Drawing.Size(481, 22)
		Me.txtPastebinURL.TabIndex = 4
		Me.txtPastebinURL.Visible = False
		'
		'lblPastebinURL
		'
		Me.lblPastebinURL.AutoSize = True
		Me.lblPastebinURL.Location = New System.Drawing.Point(195, 386)
		Me.lblPastebinURL.Name = "lblPastebinURL"
		Me.lblPastebinURL.Size = New System.Drawing.Size(71, 13)
		Me.lblPastebinURL.TabIndex = 3
		Me.lblPastebinURL.Text = "Pastbin URL:"
		Me.lblPastebinURL.Visible = False
		'
		'lblInfo
		'
		Me.lblInfo.Location = New System.Drawing.Point(6, 341)
		Me.lblInfo.Name = "lblInfo"
		Me.lblInfo.Size = New System.Drawing.Size(858, 13)
		Me.lblInfo.TabIndex = 5
		Me.lblInfo.Text = "Before submitting bug reports, please verify that the bug has not already been re" & _
		  "ported. You can view a listing of all known bugs here:"
		Me.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter
		'
		'llblURL
		'
		Me.llblURL.AutoSize = True
		Me.llblURL.Location = New System.Drawing.Point(296, 354)
		Me.llblURL.Name = "llblURL"
		Me.llblURL.Size = New System.Drawing.Size(219, 13)
		Me.llblURL.TabIndex = 4
		Me.llblURL.TabStop = True
		Me.llblURL.Text = "https://sourceforge.net/apps/trac/emm-r/"
		'
		'dlgErrorViewer
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.OK_Button
		Me.ClientSize = New System.Drawing.Size(870, 409)
		Me.Controls.Add(Me.llblURL)
		Me.Controls.Add(Me.lblInfo)
		Me.Controls.Add(Me.lblPastebinURL)
		Me.Controls.Add(Me.txtPastebinURL)
		Me.Controls.Add(Me.btnCopy)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.txtError)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgErrorViewer"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Error Log Viewer"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents txtError As System.Windows.Forms.TextBox
    Friend WithEvents btnCopy As System.Windows.Forms.Button
    Friend WithEvents txtPastebinURL As System.Windows.Forms.TextBox
    Friend WithEvents lblPastebinURL As System.Windows.Forms.Label
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents llblURL As System.Windows.Forms.LinkLabel

End Class
