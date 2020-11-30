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

Public NotInheritable Class dlgAbout

#Region "Fields"

    Dim CredList As New List(Of CredLine)
    Dim PicY As Single

#End Region 'Fields

#Region "Methods"

    Private Sub dlgAbout_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
        Me.Refresh()
    End Sub

    Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim iBackground As New Bitmap(Me.picDisplay.Width, Me.picDisplay.Height)
        Dim iLogo As New Bitmap(My.Resources.Logo)
        For xPix As Integer = 0 To iLogo.Width - 1
            For yPix As Integer = 0 To iLogo.Height - 1
                Dim clr As Color = iLogo.GetPixel(xPix, yPix)
                If clr.A > 128 Then
                    clr = Color.FromArgb(128, clr.R, clr.G, clr.B)
                    iLogo.SetPixel(xPix, yPix, clr)
                End If
            Next
        Next

        Using g As Graphics = Graphics.FromImage(iBackground)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Dim DrawRect As New Rectangle(0, 0, picDisplay.ClientSize.Width, Convert.ToInt32(picDisplay.ClientSize.Height * 0.735))
            g.FillRectangle(New Drawing2D.LinearGradientBrush(DrawRect, Color.FromArgb(255, 200, 200, 255), Color.FromArgb(255, 250, 250, 250), Drawing2D.LinearGradientMode.Vertical), DrawRect)
            DrawRect = New Rectangle(0, Convert.ToInt32(picDisplay.ClientSize.Height * 0.735), picDisplay.ClientSize.Width, Convert.ToInt32(picDisplay.ClientSize.Height * 0.265))
            g.FillRectangle(New Drawing2D.LinearGradientBrush(DrawRect, Color.White, Color.FromArgb(255, 230, 230, 230), Drawing2D.LinearGradientMode.Vertical), DrawRect)
            Dim x As Integer = Convert.ToInt32((picDisplay.Width - My.Resources.Logo.Width) / 2)
            Dim y As Integer = Convert.ToInt32((picDisplay.Height - My.Resources.Logo.Height) / 2)
            g.DrawImage(iLogo, x, y, My.Resources.Logo.Width, My.Resources.Logo.Height)
            Me.picDisplay.BackgroundImage = iBackground
        End Using

        Me.Text = "About Ember Media Manager"

        ' Optimize Painting.
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.DoubleBuffer Or _
                 ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)

        CredList.Add(New CredLine With {.Text = String.Concat("Ember Media Manager v", Master.MajorVersion.ToString.Replace(",", ".")), .Font = New Font("Microsoft Sans Serif", 18, FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Format("Revision: {0}", My.Application.Info.Version.Revision), .Font = New Font("Microsoft Sans Serif", 8, FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = My.Application.Info.Description, .Font = New Font("Microsoft Sans Serif", 8, FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = My.Application.Info.Copyright, .Font = New Font("Microsoft Sans Serif", 8, FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Created By: Jason Schnitzler", .Font = New Font("Microsoft Sans Serif", 8, FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "______Currently Maintained By______", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Chris ""bodrick"" Motch"})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "______Original Project Coders______", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        'CredList.Add(New CredLine With {.Text = "_______Project Main Coders________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Jason ""nul7"" Schnitzler"})
        CredList.Add(New CredLine With {.Text = "Nuno ""Zordor"" Novais"})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        'CredList.Add(New CredLine With {.Text = "__________Project Manager_________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = "___QA/Documentation Coordinator___", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Bence ""olympia"" Nádas"})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        'CredList.Add(New CredLine With {.Text = "__________Project Coders__________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        'CredList.Add(New CredLine With {.Text = String.Empty})
        'CredList.Add(New CredLine With {.Text = String.Empty})
        'CredList.Add(New CredLine With {.Text = String.Empty})
        'CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "___________Beta Testers___________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Tristan ""sphere"" Fischer"})
        CredList.Add(New CredLine With {.Text = "Bud ""WheresWaldo"" Hammerton"})
        CredList.Add(New CredLine With {.Text = "Wayne ""nitr8"" Humphrey"})
        'CredList.Add(New CredLine With {.Text = "Roberto Lucena"})
        CredList.Add(New CredLine With {.Text = "Brenton McSweyn"})
        CredList.Add(New CredLine With {.Text = "Clay Mitchell"})
        CredList.Add(New CredLine With {.Text = "Adyn Rex"})
        CredList.Add(New CredLine With {.Text = "Chris ""NasaGeek"" Roberts"})
        CredList.Add(New CredLine With {.Text = "Tom ""themugger"" Shine"})
        CredList.Add(New CredLine With {.Text = "Alex ""spylex"" Spyrou"})
        CredList.Add(New CredLine With {.Text = "Dale ""FidoFuz"" Williams"})
        CredList.Add(New CredLine With {.Text = "Daniel Z."})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "___________Contributors___________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Darren ""RogueDazza"" Sayers"})
        CredList.Add(New CredLine With {.Text = "Jim ""FCCWizard"" Brown"})
        'CredList.Add(New CredLine With {.Text = "Jeremy ""Jem"" Worboys"})
        CredList.Add(New CredLine With {.Text = """pcjco"""})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "__________Special Thanks__________", .Font = New Font("Microsoft Sans Serif", 10, FontStyle.Underline Or FontStyle.Bold)})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Carlos ""asphinx"" Nabb - Genre Images"})
        CredList.Add(New CredLine With {.Text = "Benjamin ""ccMatrix"" Schirmer - Studio Icon Repository"})
        CredList.Add(New CredLine With {.Text = """Jezz_X"" and Wayne ""nitr8"" Humphrey - Media Flags"})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Ember Media Manager is free software:"})
        CredList.Add(New CredLine With {.Text = "you can redistribute it and/or modify"})
        CredList.Add(New CredLine With {.Text = "it under the terms of the GNU General"})
        CredList.Add(New CredLine With {.Text = "Public License as published by the Free"})
        CredList.Add(New CredLine With {.Text = "Software Foundation, either version 3"})
        CredList.Add(New CredLine With {.Text = "of the License, or (at your option) any"})
        CredList.Add(New CredLine With {.Text = "later version."})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "Ember Media Manager is distributed in"})
        CredList.Add(New CredLine With {.Text = "the hope that it will be useful, but"})
        CredList.Add(New CredLine With {.Text = "WITHOUT ANY WARRANTY; without even the"})
        CredList.Add(New CredLine With {.Text = "implied warranty of MERCHANTABILITY or"})
        CredList.Add(New CredLine With {.Text = "FITNESS FOR A PARTICULAR PURPOSE."})
        CredList.Add(New CredLine With {.Text = String.Empty})
        CredList.Add(New CredLine With {.Text = "See the GNU General Public License for more details."})

        PicY = Me.picDisplay.ClientSize.Height
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub

    Private Sub pbFFMPEG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbFFMPEG.Click
        If Master.isWindows Then
            Using nProc As New Process
                nProc.StartInfo.Arguments = "http://www.ffmpeg.org/"
                nProc.Start()
            End Using
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.ffmpeg.org/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbIMDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbIMDB.Click
        If Master.isWindows Then
            Process.Start("http://www.imdb.com/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.imdb.com/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbIMPA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbIMPA.Click
        If Master.isWindows Then
            Process.Start("http://www.impawards.com/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.impawards.com/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbMI.Click
        If Master.isWindows Then
            Process.Start("http://mediainfo.sourceforge.net")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://mediainfo.sourceforge.net"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbMPDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbMPDB.Click
        If Master.isWindows Then
            Process.Start("http://www.moviepostersdb.com/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.moviepostersdb.com/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbTMDB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbTMDB.Click
        If Master.isWindows Then
            Process.Start("http://www.themoviedb.org/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.themoviedb.org/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbXBMC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbXBMC.Click
        If Master.isWindows Then
            Process.Start("http://www.xbmc.org/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.xbmc.org/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub pbYouTube_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbYouTube.Click
        If Master.isWindows Then
            Process.Start("http://www.youtube.com/")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://www.youtube.com/"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub picDisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picDisplay.Click
        If Master.isWindows Then
            Process.Start("http://ember.purplepig.net")
        Else
            Using Explorer As New Process
                Explorer.StartInfo.FileName = "xdg-open"
                Explorer.StartInfo.Arguments = "http://ember.purplepig.net"
                Explorer.Start()
            End Using
        End If
    End Sub

    Private Sub picDisplay_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles picDisplay.Paint
        Dim CurrentX As Single, CurrentY As Single, FontMod As Single = 0

        For i As Integer = 0 To CredList.Count - 1

            CurrentY = PicY + FontMod
            FontMod += CredList(i).Font.Size + 5

            CurrentX = (Me.picDisplay.ClientSize.Width - e.Graphics.MeasureString(CredList(i).Text, CredList(i).Font).Width) / 2

            If i = CredList.Count - 1 AndAlso CurrentY < -25 Then PicY = Me.picDisplay.ClientSize.Height

            e.Graphics.DrawString(CredList(i).Text, CredList(i).Font, Brushes.Black, CurrentX, CurrentY)

        Next

        PicY -= 1

        System.Threading.Thread.Sleep(30)

        Me.picDisplay.Invalidate()
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Friend Class CredLine

#Region "Fields"

        Private _font As Font
        Private _text As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property Font() As Font
            Get
                Return _font
            End Get
            Set(ByVal value As Font)
                _font = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return _text
            End Get
            Set(ByVal value As String)
                _text = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            _text = String.Empty
            _font = New Font("Microsoft Sans Serif", 11, FontStyle.Bold)
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class