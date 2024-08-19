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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSettingsHolder))
		Me.ListView1 = New System.Windows.Forms.ListView()
		Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.Label3 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.TextBox2 = New System.Windows.Forms.TextBox()
		Me.Button4 = New System.Windows.Forms.Button()
		Me.btnRemoveSet = New System.Windows.Forms.Button()
		Me.btnEditSet = New System.Windows.Forms.Button()
		Me.btnNewSet = New System.Windows.Forms.Button()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.cbEnabled = New System.Windows.Forms.CheckBox()
		Me.pnlSettings = New System.Windows.Forms.Panel()
		Me.Panel1.SuspendLayout()
		Me.pnlSettings.SuspendLayout()
		Me.SuspendLayout()
		'
		'ListView1
		'
		Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
		Me.ListView1.FullRowSelect = True
		Me.ListView1.Location = New System.Drawing.Point(7, 44)
		Me.ListView1.Name = "ListView1"
		Me.ListView1.Size = New System.Drawing.Size(491, 216)
		Me.ListView1.TabIndex = 1
		Me.ListView1.UseCompatibleStateImageBehavior = False
		Me.ListView1.View = System.Windows.Forms.View.Details
		'
		'ColumnHeader1
		'
		Me.ColumnHeader1.Text = "Name"
		Me.ColumnHeader1.Width = 138
		'
		'ColumnHeader2
		'
		Me.ColumnHeader2.Text = "Path"
		Me.ColumnHeader2.Width = 329
		'
		'Label3
		'
		Me.Label3.Location = New System.Drawing.Point(10, 296)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(45, 13)
		Me.Label3.TabIndex = 5
		Me.Label3.Text = "Name"
		Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
		'
		'Label4
		'
		Me.Label4.Location = New System.Drawing.Point(175, 296)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(65, 13)
		Me.Label4.TabIndex = 7
		Me.Label4.Text = "Path"
		Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(61, 293)
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.Size = New System.Drawing.Size(108, 22)
		Me.TextBox1.TabIndex = 6
		'
		'TextBox2
		'
		Me.TextBox2.Location = New System.Drawing.Point(246, 293)
		Me.TextBox2.Name = "TextBox2"
		Me.TextBox2.Size = New System.Drawing.Size(222, 22)
		Me.TextBox2.TabIndex = 8
		'
		'Button4
		'
		Me.Button4.Location = New System.Drawing.Point(473, 295)
		Me.Button4.Margin = New System.Windows.Forms.Padding(0)
		Me.Button4.Name = "Button4"
		Me.Button4.Size = New System.Drawing.Size(24, 20)
		Me.Button4.TabIndex = 9
		Me.Button4.Text = "..."
		Me.Button4.TextAlign = System.Drawing.ContentAlignment.TopCenter
		Me.Button4.UseVisualStyleBackColor = True
		'
		'btnRemoveSet
		'
		Me.btnRemoveSet.Enabled = False
		Me.btnRemoveSet.Image = CType(resources.GetObject("btnRemoveSet.Image"), System.Drawing.Image)
		Me.btnRemoveSet.Location = New System.Drawing.Point(89, 262)
		Me.btnRemoveSet.Name = "btnRemoveSet"
		Me.btnRemoveSet.Size = New System.Drawing.Size(23, 23)
		Me.btnRemoveSet.TabIndex = 4
		Me.btnRemoveSet.UseVisualStyleBackColor = True
		'
		'btnEditSet
		'
		Me.btnEditSet.Enabled = False
		Me.btnEditSet.Image = CType(resources.GetObject("btnEditSet.Image"), System.Drawing.Image)
		Me.btnEditSet.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnEditSet.Location = New System.Drawing.Point(60, 262)
		Me.btnEditSet.Name = "btnEditSet"
		Me.btnEditSet.Size = New System.Drawing.Size(23, 23)
		Me.btnEditSet.TabIndex = 3
		Me.btnEditSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnEditSet.UseVisualStyleBackColor = True
		'
		'btnNewSet
		'
		Me.btnNewSet.Enabled = False
		Me.btnNewSet.Image = CType(resources.GetObject("btnNewSet.Image"), System.Drawing.Image)
		Me.btnNewSet.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnNewSet.Location = New System.Drawing.Point(12, 262)
		Me.btnNewSet.Name = "btnNewSet"
		Me.btnNewSet.Size = New System.Drawing.Size(23, 23)
		Me.btnNewSet.TabIndex = 2
		Me.btnNewSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnNewSet.UseVisualStyleBackColor = True
		'
		'Panel1
		'
		Me.Panel1.BackColor = System.Drawing.Color.WhiteSmoke
		Me.Panel1.Controls.Add(Me.cbEnabled)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(617, 25)
		Me.Panel1.TabIndex = 0
		'
		'cbEnabled
		'
		Me.cbEnabled.AutoSize = True
		Me.cbEnabled.Location = New System.Drawing.Point(10, 5)
		Me.cbEnabled.Name = "cbEnabled"
		Me.cbEnabled.Size = New System.Drawing.Size(68, 17)
		Me.cbEnabled.TabIndex = 0
		Me.cbEnabled.Text = "Enabled"
		Me.cbEnabled.UseVisualStyleBackColor = True
		'
		'pnlSettings
		'
		Me.pnlSettings.Controls.Add(Me.Panel1)
		Me.pnlSettings.Controls.Add(Me.ListView1)
		Me.pnlSettings.Controls.Add(Me.btnRemoveSet)
		Me.pnlSettings.Controls.Add(Me.Label3)
		Me.pnlSettings.Controls.Add(Me.btnEditSet)
		Me.pnlSettings.Controls.Add(Me.Label4)
		Me.pnlSettings.Controls.Add(Me.btnNewSet)
		Me.pnlSettings.Controls.Add(Me.TextBox1)
		Me.pnlSettings.Controls.Add(Me.Button4)
		Me.pnlSettings.Controls.Add(Me.TextBox2)
		Me.pnlSettings.Location = New System.Drawing.Point(3, 12)
		Me.pnlSettings.Name = "pnlSettings"
		Me.pnlSettings.Size = New System.Drawing.Size(617, 327)
		Me.pnlSettings.TabIndex = 0
		'
		'frmSettingsHolder
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.BackColor = System.Drawing.Color.White
		Me.ClientSize = New System.Drawing.Size(625, 342)
		Me.Controls.Add(Me.pnlSettings)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "frmSettingsHolder"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Settings for Media File Manager"
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.pnlSettings.ResumeLayout(False)
		Me.pnlSettings.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents btnRemoveSet As System.Windows.Forms.Button
    Friend WithEvents btnEditSet As System.Windows.Forms.Button
    Friend WithEvents btnNewSet As System.Windows.Forms.Button
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents pnlSettings As System.Windows.Forms.Panel

End Class
