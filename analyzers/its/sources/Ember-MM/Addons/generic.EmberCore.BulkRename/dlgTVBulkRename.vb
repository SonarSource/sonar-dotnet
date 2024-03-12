Imports System.Windows.Forms
Imports EmberAPI

Public Class dlgTVBulkRenamer

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgTVBulkRename_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetUp()
    End Sub
    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(51, "TV Bulk Renamer")
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.Label2.Text = Master.eLang.GetString(52, "Rename TV Shows and files")
        Me.Label4.Text = Me.Text
        Me.lblCompiling.Text = Master.eLang.GetString(54, "Compiling TV Shows List...")
        Me.lblCanceling.Text = Master.eLang.GetString(5, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Rename_Button.Text = Master.eLang.GetString(6, "Rename")
        Me.tsmLockMovie.Text = Master.eLang.GetString(24, "Lock", True)
        Me.tsmUnlockMovie.Text = Master.eLang.GetString(108, "Unlock", True)
        Me.tsmLockAll.Text = Master.eLang.GetString(169, "Lock All", True)
        Me.tsmUnlockAll.Text = Master.eLang.GetString(170, "Unlock All", True)
        Me.lblFolderPattern.Text = Master.eLang.GetString(53, "Folder Pattern")
        Me.lblFilePattern.Text = Master.eLang.GetString(8, "File Pattern")
        Me.chkRenamedOnly.Text = Master.eLang.GetString(55, "Display Only TV Shows That Will Be Renamed")

        Dim frmToolTip As New ToolTip()
        'Dim s As String = String.Format(Master.eLang.GetString(11, "$1 = First Letter of the Title{0}$A = Audio{0}$B = Base Path{0}$C = Director{0}$D = Directory{0}$E = Sort Title{0}$F = File Name{0}$G = Genre (Follow with a space, dot or hyphen to change separator){0}$I = IMDB ID{0}$L = List Title{0}$M = MPAA{0}$O = OriginalTitle{0}$R = Resolution{0}$S = Source{0}$T = Title{0}$Y = Year{0}$X. (Replace Space with .){0}{{}} = Optional{0}$?aaa?bbb? = Replace aaa with bbb{0}$- = Remove previous char if next pattern does not have a value{0}$+ = Remove next char if previous pattern does not have a value{0}$^ = Remove previous and next char if next pattern does not have a value"), vbNewLine)
        'frmToolTip.SetToolTip(Me.txtFolder, s)
        'frmToolTip.SetToolTip(Me.txtFile, s)
    End Sub
End Class
