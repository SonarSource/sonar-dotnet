<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgSetsManager
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgSetsManager))
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.lbMovies = New System.Windows.Forms.ListBox()
		Me.btnAdd = New System.Windows.Forms.Button()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.btnRemoveSet = New System.Windows.Forms.Button()
		Me.btnEditSet = New System.Windows.Forms.Button()
		Me.btnNewSet = New System.Windows.Forms.Button()
		Me.lbSets = New System.Windows.Forms.ListBox()
		Me.GroupBox3 = New System.Windows.Forms.GroupBox()
		Me.lblCurrentSet = New System.Windows.Forms.Label()
		Me.btnDown = New System.Windows.Forms.Button()
		Me.btnUp = New System.Windows.Forms.Button()
		Me.btnRemove = New System.Windows.Forms.Button()
		Me.lbMoviesInSet = New System.Windows.Forms.ListBox()
		Me.pnlCancel = New System.Windows.Forms.Panel()
		Me.pbCompile = New System.Windows.Forms.ProgressBar()
		Me.lblCompiling = New System.Windows.Forms.Label()
		Me.lblFile = New System.Windows.Forms.Label()
		Me.lblCanceling = New System.Windows.Forms.Label()
		Me.btnCancel = New System.Windows.Forms.Button()
		Me.pnlTop = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.pnlSaving = New System.Windows.Forms.Panel()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
		Me.GroupBox1.SuspendLayout()
		Me.GroupBox2.SuspendLayout()
		Me.GroupBox3.SuspendLayout()
		Me.pnlCancel.SuspendLayout()
		Me.pnlTop.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.pnlSaving.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.OK_Button.Location = New System.Drawing.Point(628, 455)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "Close"
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.lbMovies)
		Me.GroupBox1.Controls.Add(Me.btnAdd)
		Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GroupBox1.Location = New System.Drawing.Point(469, 69)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(226, 382)
		Me.GroupBox1.TabIndex = 4
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Movies"
		'
		'lbMovies
		'
		Me.lbMovies.Enabled = False
		Me.lbMovies.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lbMovies.FormattingEnabled = True
		Me.lbMovies.Location = New System.Drawing.Point(8, 19)
		Me.lbMovies.Name = "lbMovies"
		Me.lbMovies.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
		Me.lbMovies.Size = New System.Drawing.Size(209, 329)
		Me.lbMovies.TabIndex = 0
		'
		'btnAdd
		'
		Me.btnAdd.Enabled = False
		Me.btnAdd.Image = CType(resources.GetObject("btnAdd.Image"), System.Drawing.Image)
		Me.btnAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnAdd.Location = New System.Drawing.Point(8, 351)
		Me.btnAdd.Name = "btnAdd"
		Me.btnAdd.Size = New System.Drawing.Size(23, 23)
		Me.btnAdd.TabIndex = 1
		Me.btnAdd.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnAdd.UseVisualStyleBackColor = True
		'
		'GroupBox2
		'
		Me.GroupBox2.Controls.Add(Me.btnRemoveSet)
		Me.GroupBox2.Controls.Add(Me.btnEditSet)
		Me.GroupBox2.Controls.Add(Me.btnNewSet)
		Me.GroupBox2.Controls.Add(Me.lbSets)
		Me.GroupBox2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GroupBox2.Location = New System.Drawing.Point(5, 69)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(226, 382)
		Me.GroupBox2.TabIndex = 2
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Sets"
		'
		'btnRemoveSet
		'
		Me.btnRemoveSet.Enabled = False
		Me.btnRemoveSet.Image = CType(resources.GetObject("btnRemoveSet.Image"), System.Drawing.Image)
		Me.btnRemoveSet.Location = New System.Drawing.Point(194, 351)
		Me.btnRemoveSet.Name = "btnRemoveSet"
		Me.btnRemoveSet.Size = New System.Drawing.Size(23, 23)
		Me.btnRemoveSet.TabIndex = 3
		Me.btnRemoveSet.UseVisualStyleBackColor = True
		'
		'btnEditSet
		'
		Me.btnEditSet.Enabled = False
		Me.btnEditSet.Image = CType(resources.GetObject("btnEditSet.Image"), System.Drawing.Image)
		Me.btnEditSet.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnEditSet.Location = New System.Drawing.Point(37, 351)
		Me.btnEditSet.Name = "btnEditSet"
		Me.btnEditSet.Size = New System.Drawing.Size(23, 23)
		Me.btnEditSet.TabIndex = 2
		Me.btnEditSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnEditSet.UseVisualStyleBackColor = True
		'
		'btnNewSet
		'
		Me.btnNewSet.Enabled = False
		Me.btnNewSet.Image = CType(resources.GetObject("btnNewSet.Image"), System.Drawing.Image)
		Me.btnNewSet.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnNewSet.Location = New System.Drawing.Point(8, 351)
		Me.btnNewSet.Name = "btnNewSet"
		Me.btnNewSet.Size = New System.Drawing.Size(23, 23)
		Me.btnNewSet.TabIndex = 1
		Me.btnNewSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnNewSet.UseVisualStyleBackColor = True
		'
		'lbSets
		'
		Me.lbSets.Enabled = False
		Me.lbSets.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lbSets.FormattingEnabled = True
		Me.lbSets.Location = New System.Drawing.Point(8, 20)
		Me.lbSets.Name = "lbSets"
		Me.lbSets.Size = New System.Drawing.Size(209, 329)
		Me.lbSets.Sorted = True
		Me.lbSets.TabIndex = 0
		'
		'GroupBox3
		'
		Me.GroupBox3.Controls.Add(Me.lblCurrentSet)
		Me.GroupBox3.Controls.Add(Me.btnDown)
		Me.GroupBox3.Controls.Add(Me.btnUp)
		Me.GroupBox3.Controls.Add(Me.btnRemove)
		Me.GroupBox3.Controls.Add(Me.lbMoviesInSet)
		Me.GroupBox3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.GroupBox3.Location = New System.Drawing.Point(237, 69)
		Me.GroupBox3.Name = "GroupBox3"
		Me.GroupBox3.Size = New System.Drawing.Size(226, 382)
		Me.GroupBox3.TabIndex = 3
		Me.GroupBox3.TabStop = False
		Me.GroupBox3.Text = "Movies In Set"
		'
		'lblCurrentSet
		'
		Me.lblCurrentSet.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCurrentSet.Location = New System.Drawing.Point(6, 20)
		Me.lblCurrentSet.Name = "lblCurrentSet"
		Me.lblCurrentSet.Size = New System.Drawing.Size(214, 23)
		Me.lblCurrentSet.TabIndex = 0
		Me.lblCurrentSet.Text = "None Selected"
		Me.lblCurrentSet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'btnDown
		'
		Me.btnDown.Enabled = False
		Me.btnDown.Image = CType(resources.GetObject("btnDown.Image"), System.Drawing.Image)
		Me.btnDown.Location = New System.Drawing.Point(38, 351)
		Me.btnDown.Name = "btnDown"
		Me.btnDown.Size = New System.Drawing.Size(23, 23)
		Me.btnDown.TabIndex = 3
		Me.btnDown.UseVisualStyleBackColor = True
		'
		'btnUp
		'
		Me.btnUp.Enabled = False
		Me.btnUp.Image = CType(resources.GetObject("btnUp.Image"), System.Drawing.Image)
		Me.btnUp.Location = New System.Drawing.Point(9, 351)
		Me.btnUp.Name = "btnUp"
		Me.btnUp.Size = New System.Drawing.Size(23, 23)
		Me.btnUp.TabIndex = 2
		Me.btnUp.UseVisualStyleBackColor = True
		'
		'btnRemove
		'
		Me.btnRemove.Enabled = False
		Me.btnRemove.Image = CType(resources.GetObject("btnRemove.Image"), System.Drawing.Image)
		Me.btnRemove.Location = New System.Drawing.Point(195, 351)
		Me.btnRemove.Name = "btnRemove"
		Me.btnRemove.Size = New System.Drawing.Size(23, 23)
		Me.btnRemove.TabIndex = 4
		Me.btnRemove.UseVisualStyleBackColor = True
		'
		'lbMoviesInSet
		'
		Me.lbMoviesInSet.Enabled = False
		Me.lbMoviesInSet.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lbMoviesInSet.FormattingEnabled = True
		Me.lbMoviesInSet.Location = New System.Drawing.Point(9, 46)
		Me.lbMoviesInSet.Name = "lbMoviesInSet"
		Me.lbMoviesInSet.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
		Me.lbMoviesInSet.Size = New System.Drawing.Size(209, 303)
		Me.lbMoviesInSet.TabIndex = 1
		'
		'pnlCancel
		'
		Me.pnlCancel.BackColor = System.Drawing.Color.White
		Me.pnlCancel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlCancel.Controls.Add(Me.pnlSaving)
		Me.pnlCancel.Controls.Add(Me.pbCompile)
		Me.pnlCancel.Controls.Add(Me.lblCompiling)
		Me.pnlCancel.Controls.Add(Me.lblFile)
		Me.pnlCancel.Controls.Add(Me.lblCanceling)
		Me.pnlCancel.Controls.Add(Me.btnCancel)
		Me.pnlCancel.Location = New System.Drawing.Point(150, 209)
		Me.pnlCancel.Name = "pnlCancel"
		Me.pnlCancel.Size = New System.Drawing.Size(403, 76)
		Me.pnlCancel.TabIndex = 4
		Me.pnlCancel.Visible = False
		'
		'pbCompile
		'
		Me.pbCompile.Location = New System.Drawing.Point(8, 36)
		Me.pbCompile.Name = "pbCompile"
		Me.pbCompile.Size = New System.Drawing.Size(388, 18)
		Me.pbCompile.Style = System.Windows.Forms.ProgressBarStyle.Continuous
		Me.pbCompile.TabIndex = 3
		'
		'lblCompiling
		'
		Me.lblCompiling.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblCompiling.Location = New System.Drawing.Point(3, 11)
		Me.lblCompiling.Name = "lblCompiling"
		Me.lblCompiling.Size = New System.Drawing.Size(203, 20)
		Me.lblCompiling.TabIndex = 0
		Me.lblCompiling.Text = "Loading Movies and Sets..."
		Me.lblCompiling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCompiling.Visible = False
		'
		'lblFile
		'
		Me.lblFile.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblFile.Location = New System.Drawing.Point(3, 57)
		Me.lblFile.Name = "lblFile"
		Me.lblFile.Size = New System.Drawing.Size(395, 13)
		Me.lblFile.TabIndex = 4
		Me.lblFile.Text = "File ..."
		'
		'lblCanceling
		'
		Me.lblCanceling.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblCanceling.Location = New System.Drawing.Point(110, 12)
		Me.lblCanceling.Name = "lblCanceling"
		Me.lblCanceling.Size = New System.Drawing.Size(186, 20)
		Me.lblCanceling.TabIndex = 1
		Me.lblCanceling.Text = "Canceling Load..."
		Me.lblCanceling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCanceling.Visible = False
		'
		'btnCancel
		'
		Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.btnCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.btnCancel.Image = CType(resources.GetObject("btnCancel.Image"), System.Drawing.Image)
		Me.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnCancel.Location = New System.Drawing.Point(298, 3)
		Me.btnCancel.Name = "btnCancel"
		Me.btnCancel.Size = New System.Drawing.Size(100, 30)
		Me.btnCancel.TabIndex = 2
		Me.btnCancel.Text = "Cancel"
		Me.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		Me.btnCancel.UseVisualStyleBackColor = True
		'
		'pnlTop
		'
		Me.pnlTop.BackColor = System.Drawing.Color.SteelBlue
		Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlTop.Controls.Add(Me.Label2)
		Me.pnlTop.Controls.Add(Me.Label4)
		Me.pnlTop.Controls.Add(Me.PictureBox1)
		Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
		Me.pnlTop.Location = New System.Drawing.Point(0, 0)
		Me.pnlTop.Name = "pnlTop"
		Me.pnlTop.Size = New System.Drawing.Size(702, 64)
		Me.pnlTop.TabIndex = 1
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.BackColor = System.Drawing.Color.Transparent
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.ForeColor = System.Drawing.Color.White
		Me.Label2.Location = New System.Drawing.Point(61, 38)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(202, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Add and configure movie boxed sets."
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.BackColor = System.Drawing.Color.Transparent
		Me.Label4.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label4.ForeColor = System.Drawing.Color.White
		Me.Label4.Location = New System.Drawing.Point(58, 3)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(170, 32)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "Sets Manager"
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
		Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
		Me.PictureBox1.Location = New System.Drawing.Point(12, 7)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(36, 48)
		Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
		Me.PictureBox1.TabIndex = 0
		Me.PictureBox1.TabStop = False
		'
		'pnlSaving
		'
		Me.pnlSaving.BackColor = System.Drawing.Color.White
		Me.pnlSaving.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlSaving.Controls.Add(Me.Label3)
		Me.pnlSaving.Controls.Add(Me.ProgressBar1)
		Me.pnlSaving.Location = New System.Drawing.Point(77, 12)
		Me.pnlSaving.Name = "pnlSaving"
		Me.pnlSaving.Size = New System.Drawing.Size(252, 51)
		Me.pnlSaving.TabIndex = 5
		Me.pnlSaving.Visible = False
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label3.Location = New System.Drawing.Point(2, 7)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(51, 13)
		Me.Label3.TabIndex = 0
		Me.Label3.Text = "Saving..."
		'
		'ProgressBar1
		'
		Me.ProgressBar1.Location = New System.Drawing.Point(4, 26)
		Me.ProgressBar1.MarqueeAnimationSpeed = 25
		Me.ProgressBar1.Name = "ProgressBar1"
		Me.ProgressBar1.Size = New System.Drawing.Size(242, 16)
		Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
		Me.ProgressBar1.TabIndex = 1
		'
		'dlgSetsManager
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.CancelButton = Me.btnCancel
		Me.ClientSize = New System.Drawing.Size(702, 482)
		Me.ControlBox = False
		Me.Controls.Add(Me.pnlTop)
		Me.Controls.Add(Me.OK_Button)
		Me.Controls.Add(Me.pnlCancel)
		Me.Controls.Add(Me.GroupBox3)
		Me.Controls.Add(Me.GroupBox2)
		Me.Controls.Add(Me.GroupBox1)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgSetsManager"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Sets Manager"
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox2.ResumeLayout(False)
		Me.GroupBox3.ResumeLayout(False)
		Me.pnlCancel.ResumeLayout(False)
		Me.pnlTop.ResumeLayout(False)
		Me.pnlTop.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.pnlSaving.ResumeLayout(False)
		Me.pnlSaving.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents lbMovies As System.Windows.Forms.ListBox
    Friend WithEvents btnNewSet As System.Windows.Forms.Button
    Friend WithEvents lbSets As System.Windows.Forms.ListBox
    Friend WithEvents lbMoviesInSet As System.Windows.Forms.ListBox
    Friend WithEvents btnDown As System.Windows.Forms.Button
    Friend WithEvents btnUp As System.Windows.Forms.Button
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents btnAdd As System.Windows.Forms.Button
    Friend WithEvents pnlCancel As System.Windows.Forms.Panel
    Friend WithEvents pbCompile As System.Windows.Forms.ProgressBar
    Friend WithEvents lblCompiling As System.Windows.Forms.Label
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents lblCanceling As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnEditSet As System.Windows.Forms.Button
    Friend WithEvents btnRemoveSet As System.Windows.Forms.Button
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents lblCurrentSet As System.Windows.Forms.Label
    Friend WithEvents pnlSaving As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar

End Class
