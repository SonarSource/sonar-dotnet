<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgManualEdit
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
		Me.components = New System.ComponentModel.Container()
		Me.RichTextBox1 = New System.Windows.Forms.RichTextBox()
		Me.mnuFormat = New System.Windows.Forms.MenuItem()
		Me.Panel1 = New System.Windows.Forms.Panel()
		Me.Panel3 = New System.Windows.Forms.Panel()
		Me.Splitter1 = New System.Windows.Forms.Splitter()
		Me.Panel2 = New System.Windows.Forms.Panel()
		Me.ListBox1 = New System.Windows.Forms.ListBox()
		Me.mnuParse = New System.Windows.Forms.MenuItem()
		Me.MenuItem19 = New System.Windows.Forms.MenuItem()
		Me.MainMenu1 = New System.Windows.Forms.MainMenu(Me.components)
		Me.mnuFile = New System.Windows.Forms.MenuItem()
		Me.mnuSave = New System.Windows.Forms.MenuItem()
		Me.MenuItem9 = New System.Windows.Forms.MenuItem()
		Me.mnuExit = New System.Windows.Forms.MenuItem()
		Me.Panel1.SuspendLayout()
		Me.Panel3.SuspendLayout()
		Me.Panel2.SuspendLayout()
		Me.SuspendLayout()
		'
		'RichTextBox1
		'
		Me.RichTextBox1.AcceptsTab = True
		Me.RichTextBox1.DetectUrls = False
		Me.RichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.RichTextBox1.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.RichTextBox1.Location = New System.Drawing.Point(0, 0)
		Me.RichTextBox1.Name = "RichTextBox1"
		Me.RichTextBox1.Size = New System.Drawing.Size(853, 343)
		Me.RichTextBox1.TabIndex = 0
		Me.RichTextBox1.Text = ""
		'
		'mnuFormat
		'
		Me.mnuFormat.Index = 1
		Me.mnuFormat.Text = "&Format / Indent"
		'
		'Panel1
		'
		Me.Panel1.Controls.Add(Me.Panel3)
		Me.Panel1.Controls.Add(Me.Splitter1)
		Me.Panel1.Controls.Add(Me.Panel2)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(853, 447)
		Me.Panel1.TabIndex = 1
		'
		'Panel3
		'
		Me.Panel3.Controls.Add(Me.RichTextBox1)
		Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
		Me.Panel3.Location = New System.Drawing.Point(0, 0)
		Me.Panel3.Name = "Panel3"
		Me.Panel3.Size = New System.Drawing.Size(853, 343)
		Me.Panel3.TabIndex = 2
		'
		'Splitter1
		'
		Me.Splitter1.BackColor = System.Drawing.Color.DimGray
		Me.Splitter1.Dock = System.Windows.Forms.DockStyle.Bottom
		Me.Splitter1.Location = New System.Drawing.Point(0, 343)
		Me.Splitter1.Name = "Splitter1"
		Me.Splitter1.Size = New System.Drawing.Size(853, 4)
		Me.Splitter1.TabIndex = 0
		Me.Splitter1.TabStop = False
		'
		'Panel2
		'
		Me.Panel2.Controls.Add(Me.ListBox1)
		Me.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom
		Me.Panel2.Location = New System.Drawing.Point(0, 347)
		Me.Panel2.Name = "Panel2"
		Me.Panel2.Size = New System.Drawing.Size(853, 100)
		Me.Panel2.TabIndex = 0
		'
		'ListBox1
		'
		Me.ListBox1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.ListBox1.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.ListBox1.HorizontalScrollbar = True
		Me.ListBox1.ItemHeight = 21
		Me.ListBox1.Location = New System.Drawing.Point(0, 0)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.Size = New System.Drawing.Size(853, 100)
		Me.ListBox1.TabIndex = 0
		'
		'mnuParse
		'
		Me.mnuParse.Index = 0
		Me.mnuParse.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftP
		Me.mnuParse.Text = "&Parse"
		'
		'MenuItem19
		'
		Me.MenuItem19.Index = 1
		Me.MenuItem19.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuParse, Me.mnuFormat})
		Me.MenuItem19.Text = "&Tools"
		'
		'MainMenu1
		'
		Me.MainMenu1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.MenuItem19})
		'
		'mnuFile
		'
		Me.mnuFile.Index = 0
		Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuSave, Me.MenuItem9, Me.mnuExit})
		Me.mnuFile.Text = "&File"
		'
		'mnuSave
		'
		Me.mnuSave.Index = 0
		Me.mnuSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS
		Me.mnuSave.Text = "&Save"
		'
		'MenuItem9
		'
		Me.MenuItem9.Index = 1
		Me.MenuItem9.Text = "-"
		'
		'mnuExit
		'
		Me.mnuExit.Index = 2
		Me.mnuExit.Shortcut = System.Windows.Forms.Shortcut.AltF4
		Me.mnuExit.Text = "E&xit"
		'
		'dlgManualEdit
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.ClientSize = New System.Drawing.Size(853, 447)
		Me.Controls.Add(Me.Panel1)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.Menu = Me.MainMenu1
		Me.MinimizeBox = False
		Me.Name = "dlgManualEdit"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Manual NFO Editor"
		Me.Panel1.ResumeLayout(False)
		Me.Panel3.ResumeLayout(False)
		Me.Panel2.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub
    Friend WithEvents RichTextBox1 As System.Windows.Forms.RichTextBox
    Friend WithEvents mnuFormat As System.Windows.Forms.MenuItem
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Splitter1 As System.Windows.Forms.Splitter
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents mnuParse As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem19 As System.Windows.Forms.MenuItem
    Friend WithEvents MainMenu1 As System.Windows.Forms.MainMenu
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuSave As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem9 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuExit As System.Windows.Forms.MenuItem

End Class
