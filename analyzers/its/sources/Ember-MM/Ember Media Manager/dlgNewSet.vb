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

Public Class dlgNewSet

#Region "Methods"

    Public Overloads Function ShowDialog(Optional ByVal SetName As String = "") As String
        If Not String.IsNullOrEmpty(SetName) Then
            txtSetName.Text = SetName
            Me.Text = Master.eLang.GetString(207, "Edit Set")
        Else
            Me.Text = Master.eLang.GetString(208, "Add New Set")
        End If

        If MyBase.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Return txtSetName.Text
        Else
            Return String.Empty
        End If
    End Function

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgNewSet_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()
    End Sub

    Private Sub dlgNewSet_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Dim tAL As New List(Of String)
        tAL = Master.eSettings.Sets

        If Not tAL.Contains(txtSetName.Text) Then
            tAL.Add(txtSetName.Text)
        End If

        Master.eSettings.Save()

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SetUp()
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label1.Text = Master.eLang.GetString(206, "Set Name:")
    End Sub

#End Region 'Methods

End Class