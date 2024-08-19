Imports EmberAPI

' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Public Class dlgImgView

#Region "Fields"

    Private isFull As Boolean = False
    Private PanStartPoint As New Point

#End Region 'Fields

#Region "Methods"

    Public Overloads Function ShowDialog(ByVal iImage As Image) As Windows.Forms.DialogResult
        '//
        ' Overload to pass data
        '\\

        Me.pbCache.Image = iImage

        Return MyBase.ShowDialog()
    End Function

    Private Sub dlgImgView_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape OrElse e.KeyCode = Keys.Enter Then
            Me.Close()
        End If
    End Sub

    Private Sub dlgImgView_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '//
        ' Default to Fit method
        '\\

        Me.SetUp()

        Me.DoFit()

        Me.Activate()
    End Sub

    Private Sub DoFit()
        '//
        ' Resize the image/form to fit within the bounds of the screen
        '\\

        Try
            Me.Visible = False 'hide form until resizing is done... hides Full -> Fit position whackiness not fixable by .SuspendLayout
            Me.ResetScroll()
            Me.isFull = False
            Me.pnlBG.AutoScroll = False

            ImageUtils.ResizePB(Me.pbPicture, Me.pbCache, My.Computer.Screen.WorkingArea.Height - 60, My.Computer.Screen.WorkingArea.Width)

            Me.Width = Me.pbPicture.Width
            Me.Height = Me.pbPicture.Height + 53
            Me.Left = Convert.ToInt32((My.Computer.Screen.WorkingArea.Width - Me.Width) / 2)
            Me.Top = Convert.ToInt32((My.Computer.Screen.WorkingArea.Height - Me.Height) / 2)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Visible = True
    End Sub

    Private Sub DoFull()
        '//
        ' Set image to full size, then fill the screen as much as possible
        '\\

        Try
            Me.Visible = False 'hide form until resizing is done... hides Full -> Fit position whackiness not fixable by .SuspendLayout
            Dim screenHeight As Integer = My.Computer.Screen.WorkingArea.Height
            Dim screenWidth As Integer = My.Computer.Screen.WorkingArea.Width

            Me.ResetScroll()
            Me.isFull = True

            Me.pbPicture.Image = Me.pbCache.Image
            Me.pbPicture.SizeMode = PictureBoxSizeMode.AutoSize
            Me.pnlBG.AutoScroll = True

            'set dlg size

            If Me.pbPicture.Height >= (screenHeight - 32) Then
                Me.Height = screenHeight
            Else
                Me.Height = pbPicture.Height + 32
            End If
            Me.Top = Convert.ToInt32((screenHeight - Me.Height) / 2)

            If Me.pbPicture.Width >= (screenWidth - 25) Then
                Me.Width = screenWidth
            Else
                Me.Width = Me.pbPicture.Width + 25
            End If
            Me.Left = Convert.ToInt32((screenWidth - Me.Width) / 2)

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Visible = True
    End Sub

    Private Sub pbPicture_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbPicture.DoubleClick
        '//
        ' close on doubleclick
        '\\

        Me.Close()
    End Sub

    Private Sub pbPicture_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbPicture.MouseDown
        '//
        ' Set up for panning picture
        '\\

        If Me.isFull Then
            PanStartPoint = New Point(e.X, e.Y)
            pbPicture.Cursor = Cursors.NoMove2D
        End If
    End Sub

    Private Sub pbPicture_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbPicture.MouseMove
        '//
        ' Pan picture by dragging it around
        '\\

        Try
            If e.Button = Windows.Forms.MouseButtons.Left AndAlso pbPicture.Cursor = Cursors.NoMove2D Then

                Dim DeltaX As Integer = (PanStartPoint.X - e.X)
                Dim DeltaY As Integer = (PanStartPoint.Y - e.Y)

                Me.pnlBG.AutoScrollPosition = New Drawing.Point((DeltaX - pnlBG.AutoScrollPosition.X), (DeltaY - pnlBG.AutoScrollPosition.Y))

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub pbPicture_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbPicture.MouseUp
        '//
        ' Stop Panning
        '\\

        Me.pbPicture.Cursor = Cursors.Default
    End Sub

    Private Sub ResetScroll()
        '//
        ' Move the picture back to upper-left corner
        '\\

        Try
            Me.pnlBG.AutoScrollPosition = New Drawing.Point(0, 0)
            Me.pbPicture.Location = New Point(0, 25)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(184, "Image Viewer")
        Me.tsbFit.Text = Master.eLang.GetString(185, "Fit")
        Me.tsbFull.Text = Master.eLang.GetString(186, "Full Size")
    End Sub

    Private Sub tsbFit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbFit.Click
        '//
        ' Fit pic to screen
        '\\

        Me.DoFit()
    End Sub

    Private Sub tsbFull_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tsbFull.Click
        '//
        ' Show pic in full size
        '\\

        Me.DoFull()
    End Sub

#End Region 'Methods

End Class