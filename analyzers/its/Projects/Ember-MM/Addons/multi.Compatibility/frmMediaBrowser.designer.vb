<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMediaBrowser
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
		Me.chkVideoTSParent = New System.Windows.Forms.CheckBox()
		Me.chkBackdrop = New System.Windows.Forms.CheckBox()
		Me.chkMyMovies = New System.Windows.Forms.CheckBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.chkEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.BackColor = System.Drawing.Color.White
		Me.pnlSettings.Controls.Add(Me.chkVideoTSParent)
		Me.pnlSettings.Controls.Add(Me.chkBackdrop)
		Me.pnlSettings.Controls.Add(Me.chkMyMovies)
		Me.pnlSettings.Controls.Add(Me.Label1)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(13, 15)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 0
		'
		'chkVideoTSParent
		'
		Me.chkVideoTSParent.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkVideoTSParent.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkVideoTSParent.Location = New System.Drawing.Point(13, 32)
		Me.chkVideoTSParent.Name = "chkVideoTSParent"
		Me.chkVideoTSParent.Size = New System.Drawing.Size(584, 17)
		Me.chkVideoTSParent.TabIndex = 1
		Me.chkVideoTSParent.Text = "Compatible VIDEO_TS File Placement/Naming"
		Me.chkVideoTSParent.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkVideoTSParent.UseVisualStyleBackColor = True
		'
		'chkBackdrop
		'
		Me.chkBackdrop.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkBackdrop.Location = New System.Drawing.Point(13, 55)
		Me.chkBackdrop.Name = "chkBackdrop"
		Me.chkBackdrop.Size = New System.Drawing.Size(584, 18)
		Me.chkBackdrop.TabIndex = 2
		Me.chkBackdrop.Text = "Movie Fanart as backdrop.jpg"
		Me.chkBackdrop.UseVisualStyleBackColor = True
		'
		'chkMyMovies
		'
		Me.chkMyMovies.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkMyMovies.Location = New System.Drawing.Point(13, 79)
		Me.chkMyMovies.Name = "chkMyMovies"
		Me.chkMyMovies.Size = New System.Drawing.Size(584, 18)
		Me.chkMyMovies.TabIndex = 3
		Me.chkMyMovies.Text = "Media Browser mymovies.xml"
		Me.chkMyMovies.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Label1.ForeColor = System.Drawing.Color.Red
		Me.Label1.Location = New System.Drawing.Point(224, 182)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(192, 25)
		Me.Label1.TabIndex = 4
		Me.Label1.Text = "Experimental Module"
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
		'frmMediaBrowser
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(643, 356)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Name = "frmMediaBrowser"
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
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents chkBackdrop As System.Windows.Forms.CheckBox
    Friend WithEvents chkMyMovies As System.Windows.Forms.CheckBox
    Friend WithEvents chkVideoTSParent As System.Windows.Forms.CheckBox
End Class
