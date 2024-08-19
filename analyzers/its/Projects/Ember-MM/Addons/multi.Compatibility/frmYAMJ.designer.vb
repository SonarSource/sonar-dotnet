<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmYAMJ
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
		Me.btnCheckAll = New System.Windows.Forms.Button()
		Me.chkYAMJCompatibleTVImages = New System.Windows.Forms.CheckBox()
		Me.gbImages = New System.Windows.Forms.GroupBox()
		Me.chkAllSeasonPoster = New System.Windows.Forms.CheckBox()
		Me.chkEpisodePoster = New System.Windows.Forms.CheckBox()
		Me.chkShowFanart = New System.Windows.Forms.CheckBox()
		Me.chkShowPoster = New System.Windows.Forms.CheckBox()
		Me.chkSeasonFanart = New System.Windows.Forms.CheckBox()
		Me.chkSeasonPoster = New System.Windows.Forms.CheckBox()
		Me.chkYAMJnfoFields = New System.Windows.Forms.CheckBox()
		Me.chkVideoTSParent = New System.Windows.Forms.CheckBox()
		Me.chkYAMJCompatibleSets = New System.Windows.Forms.CheckBox()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.chkEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings.SuspendLayout()
		Me.gbImages.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'pnlSettings
		'
		Me.pnlSettings.BackColor = System.Drawing.Color.White
		Me.pnlSettings.Controls.Add(Me.btnCheckAll)
		Me.pnlSettings.Controls.Add(Me.chkYAMJCompatibleTVImages)
		Me.pnlSettings.Controls.Add(Me.gbImages)
		Me.pnlSettings.Controls.Add(Me.chkYAMJnfoFields)
		Me.pnlSettings.Controls.Add(Me.chkVideoTSParent)
		Me.pnlSettings.Controls.Add(Me.chkYAMJCompatibleSets)
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Location = New System.Drawing.Point(13, 15)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 0
		'
		'btnCheckAll
		'
		Me.btnCheckAll.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnCheckAll.Location = New System.Drawing.Point(13, 30)
		Me.btnCheckAll.Name = "btnCheckAll"
		Me.btnCheckAll.Size = New System.Drawing.Size(93, 20)
		Me.btnCheckAll.TabIndex = 1
		Me.btnCheckAll.Text = "Check All"
		Me.btnCheckAll.UseVisualStyleBackColor = True
		'
		'chkYAMJCompatibleTVImages
		'
		Me.chkYAMJCompatibleTVImages.AutoSize = True
		Me.chkYAMJCompatibleTVImages.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkYAMJCompatibleTVImages.Location = New System.Drawing.Point(13, 125)
		Me.chkYAMJCompatibleTVImages.Name = "chkYAMJCompatibleTVImages"
		Me.chkYAMJCompatibleTVImages.Size = New System.Drawing.Size(211, 17)
		Me.chkYAMJCompatibleTVImages.TabIndex = 5
		Me.chkYAMJCompatibleTVImages.Text = "YAMJ Compatible TV Images Naming"
		Me.chkYAMJCompatibleTVImages.UseVisualStyleBackColor = True
		'
		'gbImages
		'
		Me.gbImages.Controls.Add(Me.chkAllSeasonPoster)
		Me.gbImages.Controls.Add(Me.chkEpisodePoster)
		Me.gbImages.Controls.Add(Me.chkShowFanart)
		Me.gbImages.Controls.Add(Me.chkShowPoster)
		Me.gbImages.Controls.Add(Me.chkSeasonFanart)
		Me.gbImages.Controls.Add(Me.chkSeasonPoster)
		Me.gbImages.Enabled = False
		Me.gbImages.Location = New System.Drawing.Point(6, 126)
		Me.gbImages.Name = "gbImages"
		Me.gbImages.Size = New System.Drawing.Size(594, 114)
		Me.gbImages.TabIndex = 6
		Me.gbImages.TabStop = False
		'
		'chkAllSeasonPoster
		'
		Me.chkAllSeasonPoster.AutoSize = True
		Me.chkAllSeasonPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkAllSeasonPoster.Location = New System.Drawing.Point(15, 39)
		Me.chkAllSeasonPoster.Name = "chkAllSeasonPoster"
		Me.chkAllSeasonPoster.Size = New System.Drawing.Size(234, 17)
		Me.chkAllSeasonPoster.TabIndex = 1
		Me.chkAllSeasonPoster.Text = "Show All Season as Season 1.banner.jpg"
		Me.chkAllSeasonPoster.UseVisualStyleBackColor = True
		'
		'chkEpisodePoster
		'
		Me.chkEpisodePoster.AutoSize = True
		Me.chkEpisodePoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkEpisodePoster.Location = New System.Drawing.Point(15, 85)
		Me.chkEpisodePoster.Name = "chkEpisodePoster"
		Me.chkEpisodePoster.Size = New System.Drawing.Size(246, 17)
		Me.chkEpisodePoster.TabIndex = 3
		Me.chkEpisodePoster.Text = "Episode Poster as <Show>.videoimage.jpg"
		Me.chkEpisodePoster.UseVisualStyleBackColor = True
		'
		'chkShowFanart
		'
		Me.chkShowFanart.AutoSize = True
		Me.chkShowFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkShowFanart.Location = New System.Drawing.Point(286, 20)
		Me.chkShowFanart.Name = "chkShowFanart"
		Me.chkShowFanart.Size = New System.Drawing.Size(238, 17)
		Me.chkShowFanart.TabIndex = 4
		Me.chkShowFanart.Text = "Show Poster as Set_<Show>_1.fanart.jpg"
		Me.chkShowFanart.UseVisualStyleBackColor = True
		'
		'chkShowPoster
		'
		Me.chkShowPoster.AutoSize = True
		Me.chkShowPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkShowPoster.Location = New System.Drawing.Point(15, 20)
		Me.chkShowPoster.Name = "chkShowPoster"
		Me.chkShowPoster.Size = New System.Drawing.Size(204, 17)
		Me.chkShowPoster.TabIndex = 0
		Me.chkShowPoster.Text = "Show Poster as Set_<Show>_1.jpg"
		Me.chkShowPoster.UseVisualStyleBackColor = True
		'
		'chkSeasonFanart
		'
		Me.chkSeasonFanart.AutoSize = True
		Me.chkSeasonFanart.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkSeasonFanart.Location = New System.Drawing.Point(286, 62)
		Me.chkSeasonFanart.Name = "chkSeasonFanart"
		Me.chkSeasonFanart.Size = New System.Drawing.Size(261, 17)
		Me.chkSeasonFanart.TabIndex = 5
		Me.chkSeasonFanart.Text = "Season Fanart as <Episode>SxxE01.fanart.jpg"
		Me.chkSeasonFanart.UseVisualStyleBackColor = True
		'
		'chkSeasonPoster
		'
		Me.chkSeasonPoster.AutoSize = True
		Me.chkSeasonPoster.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkSeasonPoster.Location = New System.Drawing.Point(15, 62)
		Me.chkSeasonPoster.Name = "chkSeasonPoster"
		Me.chkSeasonPoster.Size = New System.Drawing.Size(226, 17)
		Me.chkSeasonPoster.TabIndex = 2
		Me.chkSeasonPoster.Text = "Season Poster as <Episode>SxxE01.jpg"
		Me.chkSeasonPoster.UseVisualStyleBackColor = True
		'
		'chkYAMJnfoFields
		'
		Me.chkYAMJnfoFields.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkYAMJnfoFields.Location = New System.Drawing.Point(13, 102)
		Me.chkYAMJnfoFields.Name = "chkYAMJnfoFields"
		Me.chkYAMJnfoFields.Size = New System.Drawing.Size(584, 18)
		Me.chkYAMJnfoFields.TabIndex = 4
		Me.chkYAMJnfoFields.Text = "YAMJ Specific NFO fields"
		Me.chkYAMJnfoFields.UseVisualStyleBackColor = True
		'
		'chkVideoTSParent
		'
		Me.chkVideoTSParent.CheckAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkVideoTSParent.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkVideoTSParent.Location = New System.Drawing.Point(13, 58)
		Me.chkVideoTSParent.Name = "chkVideoTSParent"
		Me.chkVideoTSParent.Size = New System.Drawing.Size(584, 17)
		Me.chkVideoTSParent.TabIndex = 2
		Me.chkVideoTSParent.Text = "Compatible VIDEO_TS File Placement/Naming"
		Me.chkVideoTSParent.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.chkVideoTSParent.UseVisualStyleBackColor = True
		'
		'chkYAMJCompatibleSets
		'
		Me.chkYAMJCompatibleSets.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.chkYAMJCompatibleSets.Location = New System.Drawing.Point(13, 81)
		Me.chkYAMJCompatibleSets.Name = "chkYAMJCompatibleSets"
		Me.chkYAMJCompatibleSets.Size = New System.Drawing.Size(584, 17)
		Me.chkYAMJCompatibleSets.TabIndex = 3
		Me.chkYAMJCompatibleSets.Text = "YAMJ Compatible Movie Sets"
		Me.chkYAMJCompatibleSets.UseVisualStyleBackColor = True
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
		'frmYAMJ
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(643, 356)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Name = "frmYAMJ"
		Me.Text = "frmSettingsHolder"
		Me.pnlSettings.ResumeLayout(False)
		Me.pnlSettings.PerformLayout()
		Me.gbImages.ResumeLayout(False)
		Me.gbImages.PerformLayout()
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents chkEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents chkYAMJCompatibleSets As System.Windows.Forms.CheckBox
    Friend WithEvents chkYAMJCompatibleTVImages As System.Windows.Forms.CheckBox
    Friend WithEvents chkVideoTSParent As System.Windows.Forms.CheckBox
    Friend WithEvents btnCheckAll As System.Windows.Forms.Button
    Friend WithEvents chkYAMJnfoFields As System.Windows.Forms.CheckBox
    Friend WithEvents gbImages As System.Windows.Forms.GroupBox
    Friend WithEvents chkSeasonPoster As System.Windows.Forms.CheckBox
    Friend WithEvents chkSeasonFanart As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowFanart As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowPoster As System.Windows.Forms.CheckBox
    Friend WithEvents chkEpisodePoster As System.Windows.Forms.CheckBox
    Friend WithEvents chkAllSeasonPoster As System.Windows.Forms.CheckBox
End Class
