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

Imports System.Runtime.InteropServices

Public Class frmNotify

    #Region "Fields"

    Public Shared MasterIndex As Integer = 0

    Public Index As Integer = 0

    Protected AnimationTimer As New Timer()
    Protected AnimationType As AnimationTypes = AnimationTypes.Show

    Private Shared DisplayedForms As New List(Of frmNotify)

    Private _type As String

    #End Region 'Fields

    #Region "Enumerations"

    Public Enum AnimationTypes
        Show = 0
        Wait = 1
        Close = 2
    End Enum

    #End Region 'Enumerations

    #Region "Events"

    Public Event NotifierClicked(ByVal _type As String)

    Public Event NotifierClosed()

    #End Region 'Events

    #Region "Properties"

    Protected Overrides ReadOnly Property ShowWithoutActivation() As Boolean
        Get
            Return True
        End Get
    End Property

    #End Region 'Properties

    #Region "Methods"

    Public Overloads Sub Show(ByVal _type As String, ByVal _icon As Integer, ByVal _title As String, ByVal _message As String, ByVal _customicon As Image)
        Me._type = _type
        If Not IsNothing(_customicon) Then
            Me.pbIcon.Image = _customicon
        Else
            Select Case _icon
                Case 1
                    Me.pbIcon.Image = My.Resources._error
                Case 2
                    Me.pbIcon.Image = My.Resources._comment
                Case 3
                    Me.pbIcon.Image = My.Resources._movie
                Case 4
                    Me.pbIcon.Image = My.Resources._tv
                Case Else
                    Me.pbIcon.Image = My.Resources._info
            End Select
        End If
        Me.lblTitle.Text = _title
        Me.lblMessage.Text = _message

        MyBase.Show()
    End Sub

    Protected Sub OnTimer(ByVal sender As Object, ByVal e As EventArgs)
        Select Case Me.AnimationType

            Case AnimationTypes.Show

                If Me.Height < 80 Then
                    Me.SetBounds(Me.Left, Me.Top - 2, Me.Width, Me.Height + 2)

                    For Each DisplayedForm As frmNotify In frmNotify.DisplayedForms.Where(Function(s) s.Index < Me.Index)
                        DisplayedForm.Top -= 2
                    Next
                Else
                    Me.AnimationTimer.Stop()
                    Me.Height = 80
                    Me.AnimationTimer.Interval = 3000
                    Me.AnimationType = AnimationTypes.Wait
                    Me.AnimationTimer.Start()
                End If

            Case AnimationTypes.Wait

                Me.AnimationTimer.Stop()
                Me.AnimationTimer.Interval = 5
                Me.AnimationType = AnimationTypes.Close
                Me.AnimationTimer.Start()

            Case AnimationTypes.Close

                If Me.Height > 2 Then
                    Me.SetBounds(Me.Left, Me.Top + 2, Me.Width, Me.Height - 2)
                Else
                    Me.AnimationTimer.Stop()
                    Me.Close()
                End If

        End Select
    End Sub

    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function SetWindowPos(ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As UInt32) As Boolean
    End Function

    Private Sub frmNotify_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Click
        RaiseEvent NotifierClicked(Me._type)
    End Sub

    Private Sub frmNotify_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        frmNotify.DisplayedForms.Remove(Me)
        RaiseEvent NotifierClosed()
    End Sub

    Private Sub frmNotify_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetWindowPos(Me.Handle, New IntPtr(-1), Screen.PrimaryScreen.WorkingArea.Width - Me.Width - 5, Screen.PrimaryScreen.WorkingArea.Height - 5, 315, 0, &H10)

        AddHandler AnimationTimer.Tick, AddressOf OnTimer

        'only allow 6 notifications on screen at a time
        If frmNotify.DisplayedForms.Count = 6 Then
            frmNotify.DisplayedForms(0).Close()
        End If

        For Each DisplayedForm As frmNotify In frmNotify.DisplayedForms
            DisplayedForm.Top -= 5
        Next

        frmNotify.MasterIndex += 1
        Me.Index = frmNotify.MasterIndex

        frmNotify.DisplayedForms.Add(Me)
    End Sub

    Private Sub frmNotify_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.AnimationTimer.Stop()
        Me.AnimationTimer.Interval = 5
        Me.AnimationType = AnimationTypes.Show
        Me.AnimationTimer.Start()
    End Sub

    Private Sub lblMessage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblMessage.Click
        RaiseEvent NotifierClicked(Me._type)
    End Sub

    Private Sub lblTitle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblTitle.Click
        RaiseEvent NotifierClicked(Me._type)
    End Sub

    Private Sub pbIcon_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbIcon.Click
        RaiseEvent NotifierClicked(Me._type)
    End Sub

    #End Region 'Methods

End Class