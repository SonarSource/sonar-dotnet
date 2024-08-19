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
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.chkOnNewEp = New System.Windows.Forms.CheckBox()
		Me.chkOnMovieScraped = New System.Windows.Forms.CheckBox()
		Me.chkOnNewMovie = New System.Windows.Forms.CheckBox()
		Me.chkOnError = New System.Windows.Forms.CheckBox()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.chkEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.BackColor = System.Drawing.Color.White
		Me.pnlSettings.Controls.Add(Me.chkOnNewEp)
		Me.pnlSettings.Controls.Add(Me.chkOnMovieScraped)
		Me.pnlSettings.Controls.Add(Me.chkOnNewMovie)
		Me.pnlSettings.Controls.Add(Me.chkOnError)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(13, 15)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 0
		'
		'chkOnNewEp
		'
		Me.chkOnNewEp.AutoSize = True
		Me.chkOnNewEp.Location = New System.Drawing.Point(10, 146)
		Me.chkOnNewEp.Name = "chkOnNewEp"
		Me.chkOnNewEp.Size = New System.Drawing.Size(149, 17)
		Me.chkOnNewEp.TabIndex = 4
		Me.chkOnNewEp.Text = "On New Episode Added"
		Me.chkOnNewEp.UseVisualStyleBackColor = True
		'
		'chkOnMovieScraped
		'
		Me.chkOnMovieScraped.AutoSize = True
		Me.chkOnMovieScraped.Location = New System.Drawing.Point(10, 109)
		Me.chkOnMovieScraped.Name = "chkOnMovieScraped"
		Me.chkOnMovieScraped.Size = New System.Drawing.Size(120, 17)
		Me.chkOnMovieScraped.TabIndex = 3
		Me.chkOnMovieScraped.Text = "On Movie Scraped"
		Me.chkOnMovieScraped.UseVisualStyleBackColor = True
		'
		'chkOnNewMovie
		'
		Me.chkOnNewMovie.AutoSize = True
		Me.chkOnNewMovie.Location = New System.Drawing.Point(10, 86)
		Me.chkOnNewMovie.Name = "chkOnNewMovie"
		Me.chkOnNewMovie.Size = New System.Drawing.Size(139, 17)
		Me.chkOnNewMovie.TabIndex = 2
		Me.chkOnNewMovie.Text = "On New Movie Added"
		Me.chkOnNewMovie.UseVisualStyleBackColor = True
		'
		'chkOnError
		'
		Me.chkOnError.AutoSize = True
		Me.chkOnError.Location = New System.Drawing.Point(10, 45)
		Me.chkOnError.Name = "chkOnError"
		Me.chkOnError.Size = New System.Drawing.Size(70, 17)
		Me.chkOnError.TabIndex = 1
		Me.chkOnError.Text = "On Error"
		Me.chkOnError.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.chkEnabled)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(617, 25)
		Me.Panel1.TabIndex = 0
		'
		'chkEnabled
		'
		Me.chkEnabled.AutoSize = True
		Me.chkEnabled.Location = New System.Drawing.Point(10, 5)
		Me.chkEnabled.Name = "chkEnabled"
		Me.chkEnabled.Size = New System.Drawing.Size(68, 17)
		Me.chkEnabled.TabIndex = 0
		Me.chkEnabled.Text = "Enabled"
		Me.chkEnabled.UseVisualStyleBackColor = True
		'
		'frmSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(643, 356)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Name = "frmSettingsHolder"
		Me.Text = "frmSettingsHolder"
		Me.pnlSettings.ResumeLayout(False)
		Me.pnlSettings.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents chkEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents chkOnError As System.Windows.Forms.CheckBox
    Friend WithEvents chkOnMovieScraped As System.Windows.Forms.CheckBox
    Friend WithEvents chkOnNewMovie As System.Windows.Forms.CheckBox
    Friend WithEvents chkOnNewEp As System.Windows.Forms.CheckBox
End Class
