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

Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class dlgAddEditActor

#Region "Fields"

    Public Shared selIndex As Integer = 0

    Private eActor As MediaContainers.Person
    Private isNew As Boolean = True
    Private sHTTP As New HTTP

#End Region 'Fields

#Region "Methods"

    Public Sub SetUp()
        Me.lblName.Text = Master.eLang.GetString(154, "Actor Name:")
        Me.lblRole.Text = Master.eLang.GetString(155, "Actor Role:")
        Me.lblThumb.Text = Master.eLang.GetString(156, "Actor Thumb (URL):")
    End Sub

    Public Overloads Function ShowDialog(ByVal bNew As Boolean, Optional ByVal inActor As MediaContainers.Person = Nothing) As MediaContainers.Person
        '//
        ' Overload to pass data
        '\\

        Me.isNew = bNew

        If bNew Then
            Me.eActor = New MediaContainers.Person
        Else
            Me.eActor = inActor
        End If

        If MyBase.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Return Me.eActor
        Else
            Return Nothing
        End If
    End Function

    Private Sub btnVerify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVerify.Click
        '//
        ' Download the pic to verify the url was entered correctly
        '\\

        Try
            If Not String.IsNullOrEmpty(Me.txtThumb.Text) Then
                If StringUtils.isValidURL(Me.txtThumb.Text) Then
                    If Me.bwDownloadPic.IsBusy Then
                        Me.bwDownloadPic.CancelAsync()
                    End If

                    Me.pbActLoad.Visible = True

                    Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                    Me.bwDownloadPic.WorkerSupportsCancellation = True
                    Me.bwDownloadPic.RunWorkerAsync()
                Else
                    MsgBox(Master.eLang.GetString(159, "Specified URL is not valid."), MsgBoxStyle.Exclamation, Master.eLang.GetString(160, "Invalid URL"))
                End If
            Else
                MsgBox(Master.eLang.GetString(161, "Please enter a URL to verify."), MsgBoxStyle.Exclamation, Master.eLang.GetString(162, "No Thumb URL Specified"))
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub bwDownloadPic_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadPic.DoWork
        sHTTP.StartDownloadImage(Me.txtThumb.Text)

        While sHTTP.IsDownloading
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        e.Result = sHTTP.Image
    End Sub

    Private Sub bwDownloadPic_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadPic.RunWorkerCompleted
        '//
        ' Thread finished: display pic if it was able to get one
        '\\

        Me.pbActLoad.Visible = False

        Me.pbActors.Image = DirectCast(e.Result, Image)
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        '//
        ' Get me out of here!
        '\\

        Me.DialogResult = Windows.Forms.DialogResult.Cancel

        Me.Close()
    End Sub

    Private Sub dlgAddEditActor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '//
        ' Fill form with data if needed
        '\\

        Try
            Me.SetUp()

            If Me.isNew Then
                Me.Text = Master.eLang.GetString(157, "New Actor")
            Else
                Me.Text = Master.eLang.GetString(158, "Edit Actor")
                Me.txtName.Text = Me.eActor.Name
                Me.txtRole.Text = Me.eActor.Role
                Me.txtThumb.Text = Me.eActor.Thumb

            End If

            Me.Activate()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        '//
        ' Fill the MediaContainers.Person with the data
        '\\

        Try
            Me.eActor.Name = Me.txtName.Text
            Me.eActor.Role = Me.txtRole.Text
            Me.eActor.Thumb = Me.txtThumb.Text
            Me.DialogResult = Windows.Forms.DialogResult.OK
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Me.Close()
    End Sub

#End Region 'Methods

End Class