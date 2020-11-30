<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated> _
Partial Class dlgExportMovies
    Inherits System.Windows.Forms.Form

    #Region "Fields"

    Friend  WithEvents btnCancel As System.Windows.Forms.Button
    Friend  WithEvents btnSource As System.Windows.Forms.Button
    Friend  WithEvents cbSearch As System.Windows.Forms.ComboBox
    Friend  WithEvents cbTemplate As System.Windows.Forms.ComboBox
    Friend  WithEvents Close_Button As System.Windows.Forms.Button
    Friend  WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend  WithEvents Label1 As System.Windows.Forms.Label
    Friend  WithEvents Label2 As System.Windows.Forms.Label
    Friend  WithEvents lblCanceling As System.Windows.Forms.Label
    Friend  WithEvents lblCompiling As System.Windows.Forms.Label
    Friend  WithEvents lblFile As System.Windows.Forms.Label
    Friend  WithEvents lblIn As System.Windows.Forms.Label
    Friend  WithEvents lstSources As System.Windows.Forms.CheckedListBox
    Friend  WithEvents pbCompile As System.Windows.Forms.ProgressBar
    Friend  WithEvents pnlBG As System.Windows.Forms.Panel
    Friend  WithEvents pnlBottomMain As System.Windows.Forms.Panel
    Friend  WithEvents pnlCancel As System.Windows.Forms.Panel
    Friend  WithEvents pnlSearch As System.Windows.Forms.Panel
    Friend  WithEvents Reset_Button As System.Windows.Forms.Button
    Friend  WithEvents Save_Button As System.Windows.Forms.Button
    Friend  WithEvents Search_Button As System.Windows.Forms.Button
    Friend  WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend  WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend  WithEvents wbMovieList As System.Windows.Forms.WebBrowser

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    #End Region 'Fields

    #Region "Methods"

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgExportMovies))
		Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
		Me.Save_Button = New System.Windows.Forms.Button()
		Me.Close_Button = New System.Windows.Forms.Button()
		Me.pnlBottomMain = New System.Windows.Forms.Panel()
		Me.pnlSearch = New System.Windows.Forms.Panel()
		Me.btnSource = New System.Windows.Forms.Button()
		Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
		Me.Reset_Button = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Search_Button = New System.Windows.Forms.Button()
		Me.lblIn = New System.Windows.Forms.Label()
		Me.cbSearch = New System.Windows.Forms.ComboBox()
		Me.txtSearch = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.cbTemplate = New System.Windows.Forms.ComboBox()
		Me.pnlCancel = New System.Windows.Forms.Panel()
		Me.btnCancel = New System.Windows.Forms.Button()
		Me.pbCompile = New System.Windows.Forms.ProgressBar()
		Me.lblCompiling = New System.Windows.Forms.Label()
		Me.lblFile = New System.Windows.Forms.Label()
		Me.lblCanceling = New System.Windows.Forms.Label()
		Me.pnlBG = New System.Windows.Forms.Panel()
		Me.wbMovieList = New System.Windows.Forms.WebBrowser()
		Me.lstSources = New System.Windows.Forms.CheckedListBox()
		Me.TableLayoutPanel1.SuspendLayout()
		Me.pnlBottomMain.SuspendLayout()
		Me.pnlSearch.SuspendLayout()
		Me.pnlCancel.SuspendLayout()
		Me.pnlBG.SuspendLayout()
		Me.SuspendLayout()
		'
		'TableLayoutPanel1
		'
		Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TableLayoutPanel1.ColumnCount = 2
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Controls.Add(Me.Save_Button, 0, 0)
		Me.TableLayoutPanel1.Controls.Add(Me.Close_Button, 1, 0)
		Me.TableLayoutPanel1.Location = New System.Drawing.Point(886, 6)
		Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
		Me.TableLayoutPanel1.RowCount = 1
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29.0!))
		Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
		Me.TableLayoutPanel1.TabIndex = 0
		'
		'Save_Button
		'
		Me.Save_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.Save_Button.Enabled = False
		Me.Save_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Save_Button.Location = New System.Drawing.Point(3, 3)
		Me.Save_Button.Name = "Save_Button"
		Me.Save_Button.Size = New System.Drawing.Size(67, 23)
		Me.Save_Button.TabIndex = 0
		Me.Save_Button.Text = "Save"
		'
		'Close_Button
		'
		Me.Close_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.Close_Button.DialogResult = DialogResult.Cancel
		Me.Close_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Close_Button.Location = New System.Drawing.Point(76, 3)
		Me.Close_Button.Name = "Close_Button"
		Me.Close_Button.Size = New System.Drawing.Size(67, 23)
		Me.Close_Button.TabIndex = 1
		Me.Close_Button.Text = "Close"
		'
		'pnlBottomMain
		'
		Me.pnlBottomMain.Controls.Add(Me.pnlSearch)
		Me.pnlBottomMain.Controls.Add(Me.Label2)
		Me.pnlBottomMain.Controls.Add(Me.cbTemplate)
		Me.pnlBottomMain.Controls.Add(Me.TableLayoutPanel1)
		Me.pnlBottomMain.Location = New System.Drawing.Point(0, 502)
		Me.pnlBottomMain.Name = "pnlBottomMain"
		Me.pnlBottomMain.Size = New System.Drawing.Size(1035, 48)
		Me.pnlBottomMain.TabIndex = 0
		'
		'pnlSearch
		'
		Me.pnlSearch.Controls.Add(Me.btnSource)
		Me.pnlSearch.Controls.Add(Me.Reset_Button)
		Me.pnlSearch.Controls.Add(Me.Label1)
		Me.pnlSearch.Controls.Add(Me.Search_Button)
		Me.pnlSearch.Controls.Add(Me.lblIn)
		Me.pnlSearch.Controls.Add(Me.cbSearch)
		Me.pnlSearch.Controls.Add(Me.txtSearch)
		Me.pnlSearch.Enabled = False
		Me.pnlSearch.Location = New System.Drawing.Point(214, 9)
		Me.pnlSearch.Name = "pnlSearch"
		Me.pnlSearch.Size = New System.Drawing.Size(489, 28)
		Me.pnlSearch.TabIndex = 3
		'
		'btnSource
		'
		Me.btnSource.ImageIndex = 0
		Me.btnSource.ImageList = Me.ImageList1
		Me.btnSource.Location = New System.Drawing.Point(196, 4)
		Me.btnSource.Name = "btnSource"
		Me.btnSource.Size = New System.Drawing.Size(19, 23)
		Me.btnSource.TabIndex = 2
		Me.btnSource.UseVisualStyleBackColor = True
		Me.btnSource.Visible = False
		'
		'ImageList1
		'
		Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
		Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
		Me.ImageList1.Images.SetKeyName(0, "asc.png")
		Me.ImageList1.Images.SetKeyName(1, "desc.png")
		'
		'Reset_Button
		'
		Me.Reset_Button.Enabled = False
		Me.Reset_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Reset_Button.Location = New System.Drawing.Point(416, 2)
		Me.Reset_Button.Name = "Reset_Button"
		Me.Reset_Button.Size = New System.Drawing.Size(67, 23)
		Me.Reset_Button.TabIndex = 6
		Me.Reset_Button.Text = "Reset"
		'
		'Label1
		'
		Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label1.Location = New System.Drawing.Point(15, 6)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(51, 14)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Filter"
		Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'Search_Button
		'
		Me.Search_Button.Enabled = False
		Me.Search_Button.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Search_Button.Location = New System.Drawing.Point(343, 2)
		Me.Search_Button.Name = "Search_Button"
		Me.Search_Button.Size = New System.Drawing.Size(67, 23)
		Me.Search_Button.TabIndex = 5
		Me.Search_Button.Text = "Apply"
		'
		'lblIn
		'
		Me.lblIn.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.lblIn.Location = New System.Drawing.Point(209, 7)
		Me.lblIn.Name = "lblIn"
		Me.lblIn.Size = New System.Drawing.Size(32, 13)
		Me.lblIn.TabIndex = 3
		Me.lblIn.Text = "in"
		Me.lblIn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'cbSearch
		'
		Me.cbSearch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbSearch.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.cbSearch.FormattingEnabled = True
		Me.cbSearch.Location = New System.Drawing.Point(243, 3)
		Me.cbSearch.Name = "cbSearch"
		Me.cbSearch.Size = New System.Drawing.Size(94, 21)
		Me.cbSearch.TabIndex = 4
		'
		'txtSearch
		'
		Me.txtSearch.Location = New System.Drawing.Point(72, 4)
		Me.txtSearch.Name = "txtSearch"
		Me.txtSearch.Size = New System.Drawing.Size(123, 22)
		Me.txtSearch.TabIndex = 1
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.Label2.Location = New System.Drawing.Point(11, 17)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(55, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Template"
		Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'cbTemplate
		'
		Me.cbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cbTemplate.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
		Me.cbTemplate.FormattingEnabled = True
		Me.cbTemplate.Location = New System.Drawing.Point(68, 13)
		Me.cbTemplate.Name = "cbTemplate"
		Me.cbTemplate.Size = New System.Drawing.Size(140, 21)
		Me.cbTemplate.TabIndex = 2
		'
		'pnlCancel
		'
		Me.pnlCancel.BackColor = System.Drawing.Color.LightGray
		Me.pnlCancel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.pnlCancel.Controls.Add(Me.btnCancel)
		Me.pnlCancel.Controls.Add(Me.pbCompile)
		Me.pnlCancel.Controls.Add(Me.lblCompiling)
		Me.pnlCancel.Controls.Add(Me.lblFile)
		Me.pnlCancel.Controls.Add(Me.lblCanceling)
		Me.pnlCancel.Location = New System.Drawing.Point(242, 12)
		Me.pnlCancel.Name = "pnlCancel"
		Me.pnlCancel.Size = New System.Drawing.Size(403, 76)
		Me.pnlCancel.TabIndex = 1
		Me.pnlCancel.Visible = False
		'
		'btnCancel
		'
		Me.btnCancel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
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
		Me.lblCompiling.Size = New System.Drawing.Size(395, 20)
		Me.lblCompiling.TabIndex = 0
		Me.lblCompiling.Text = "Compiling Movie List..."
		Me.lblCompiling.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
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
		Me.lblCanceling.Text = "Canceling Compilation..."
		Me.lblCanceling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.lblCanceling.Visible = False
		'
		'pnlBG
		'
		Me.pnlBG.AutoScroll = True
		Me.pnlBG.Controls.Add(Me.pnlCancel)
		Me.pnlBG.Controls.Add(Me.wbMovieList)
		Me.pnlBG.Dock = System.Windows.Forms.DockStyle.Fill
		Me.pnlBG.Location = New System.Drawing.Point(0, 0)
		Me.pnlBG.Name = "pnlBG"
		Me.pnlBG.Size = New System.Drawing.Size(1035, 550)
		Me.pnlBG.TabIndex = 2
		'
		'wbMovieList
		'
		Me.wbMovieList.Location = New System.Drawing.Point(0, 0)
		Me.wbMovieList.MinimumSize = New System.Drawing.Size(20, 20)
		Me.wbMovieList.Name = "wbMovieList"
		Me.wbMovieList.Size = New System.Drawing.Size(1034, 500)
		Me.wbMovieList.TabIndex = 0
		Me.wbMovieList.Visible = False
		'
		'lstSources
		'
		Me.lstSources.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lstSources.FormattingEnabled = True
		Me.lstSources.Location = New System.Drawing.Point(286, 442)
		Me.lstSources.Name = "lstSources"
		Me.lstSources.Size = New System.Drawing.Size(123, 89)
		Me.lstSources.TabIndex = 1
		Me.lstSources.Visible = False
		'
		'dlgExportMovies
		'
		Me.AcceptButton = Me.Save_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
		Me.AutoScroll = True
		Me.CancelButton = Me.Close_Button
		Me.ClientSize = New System.Drawing.Size(1035, 550)
		Me.Controls.Add(Me.lstSources)
		Me.Controls.Add(Me.pnlBottomMain)
		Me.Controls.Add(Me.pnlBG)
		Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "dlgExportMovies"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Export Movies"
		Me.TableLayoutPanel1.ResumeLayout(False)
		Me.pnlBottomMain.ResumeLayout(False)
		Me.pnlBottomMain.PerformLayout()
		Me.pnlSearch.ResumeLayout(False)
		Me.pnlSearch.PerformLayout()
		Me.pnlCancel.ResumeLayout(False)
		Me.pnlBG.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub

    #End Region 'Methods

End Class