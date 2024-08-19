<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class dlgTVChangeEp
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
		Me.lvEpisodes = New System.Windows.Forms.ListView()
		Me.colEpisode = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.colTitle = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.pbPreview = New System.Windows.Forms.PictureBox()
		Me.lblTitle = New System.Windows.Forms.Label()
		Me.txtPlot = New System.Windows.Forms.TextBox()
		CType(Me.pbPreview, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Location = New System.Drawing.Point(505, 286)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'Cancel_Button
		'
		Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Cancel_Button.Location = New System.Drawing.Point(575, 286)
		Me.Cancel_Button.Name = "Cancel_Button"
		Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
		Me.Cancel_Button.TabIndex = 1
		Me.Cancel_Button.Text = "Cancel"
		'
		'lvEpisodes
		'
		Me.lvEpisodes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colEpisode, Me.colTitle})
		Me.lvEpisodes.FullRowSelect = True
		Me.lvEpisodes.HideSelection = False
		Me.lvEpisodes.Location = New System.Drawing.Point(3, 3)
		Me.lvEpisodes.MultiSelect = False
		Me.lvEpisodes.Name = "lvEpisodes"
		Me.lvEpisodes.Size = New System.Drawing.Size(358, 278)
		Me.lvEpisodes.TabIndex = 2
		Me.lvEpisodes.UseCompatibleStateImageBehavior = False
		Me.lvEpisodes.View = System.Windows.Forms.View.Details
		'
		'colEpisode
		'
		Me.colEpisode.Text = "Episode"
		'
		'colTitle
		'
		Me.colTitle.Text = "Title"
		Me.colTitle.Width = 276
		'
		'pbPreview
		'
		Me.pbPreview.Location = New System.Drawing.Point(417, 12)
		Me.pbPreview.Name = "pbPreview"
		Me.pbPreview.Size = New System.Drawing.Size(174, 117)
		Me.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
		Me.pbPreview.TabIndex = 3
		Me.pbPreview.TabStop = False
		'
		'lblTitle
		'
		Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblTitle.Location = New System.Drawing.Point(367, 140)
		Me.lblTitle.Name = "lblTitle"
		Me.lblTitle.Size = New System.Drawing.Size(275, 21)
		Me.lblTitle.TabIndex = 3
		'
		'txtPlot
		'
		Me.txtPlot.BackColor = System.Drawing.SystemColors.Control
		Me.txtPlot.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me.txtPlot.Location = New System.Drawing.Point(366, 169)
		Me.txtPlot.Multiline = True
		Me.txtPlot.Name = "txtPlot"
		Me.txtPlot.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtPlot.Size = New System.Drawing.Size(276, 111)
		Me.txtPlot.TabIndex = 4
		'
		'dlgTVChangeEp
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.Cancel_Button
		Me.ClientSize = New System.Drawing.Size(646, 315)
		Me.ControlBox = False
		Me.Controls.Add(Me.txtPlot)
		Me.Controls.Add(Me.lblTitle)
		Me.Controls.Add(Me.pbPreview)
		Me.Controls.Add(Me.lvEpisodes)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.Cancel_Button)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgTVChangeEp"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Change Episode"
		CType(Me.pbPreview, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents lvEpisodes As System.Windows.Forms.ListView
    Friend WithEvents colEpisode As System.Windows.Forms.ColumnHeader
    Friend WithEvents colTitle As System.Windows.Forms.ColumnHeader
    Friend WithEvents pbPreview As System.Windows.Forms.PictureBox
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents txtPlot As System.Windows.Forms.TextBox

End Class
