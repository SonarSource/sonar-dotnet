Imports System.Drawing
Imports System.Windows.Forms

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

Public Class ImageUtils

#Region "Methods"

    Public Shared Function AddMissingStamp(ByVal oImage As Image) As Image
        Dim nImage As New Bitmap(GrayScale(oImage))

        'now overlay "missing" image
        Dim grOverlay As Graphics = Graphics.FromImage(nImage)
        Dim oWidth As Integer = If(nImage.Width >= My.Resources.missing.Width, My.Resources.missing.Width, nImage.Width)
        Dim oheight As Integer = If(nImage.Height >= My.Resources.missing.Height, My.Resources.missing.Height, nImage.Height)
        grOverlay.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        grOverlay.DrawImage(My.Resources.missing, 0, 0, oWidth, oheight)

        Return nImage
    End Function

    Public Shared Function GrayScale(ByVal oImage As Image) As Image
        Dim nImage As New Bitmap(oImage)

        'first let's convert the background to grayscale
        Dim g As Graphics = Graphics.FromImage(nImage)
        Dim cm As Imaging.ColorMatrix = New Imaging.ColorMatrix(New Single()() _
             {New Single() {0.5, 0.5, 0.5, 0, 0}, _
            New Single() {0.5, 0.5, 0.5, 0, 0}, _
            New Single() {0.5, 0.5, 0.5, 0, 0}, _
            New Single() {0, 0, 0, 1, 0}, _
            New Single() {0, 0, 0, 0, 1}})

        Dim ia As Imaging.ImageAttributes = New Imaging.ImageAttributes()
        ia.SetColorMatrix(cm)
        g.DrawImage(oImage, New Rectangle(0, 0, oImage.Width, oImage.Height), 0, 0, oImage.Width, oImage.Height, GraphicsUnit.Pixel, ia)

        Return nImage
    End Function

    Public Shared Sub DrawGradEllipse(ByRef graphics As Graphics, ByVal bounds As Rectangle, ByVal color1 As Color, ByVal color2 As Color)
        Try
            Dim rPoints() As Point = { _
            New Point(bounds.X, bounds.Y), _
            New Point(bounds.X + bounds.Width, bounds.Y), _
            New Point(bounds.X + bounds.Width, bounds.Y + bounds.Height), _
            New Point(bounds.X, bounds.Y + bounds.Height) _
        }
            Dim pgBrush As New Drawing2D.PathGradientBrush(rPoints)
            Dim gPath As New Drawing2D.GraphicsPath
            gPath.AddEllipse(bounds.X, bounds.Y, bounds.Width, bounds.Height)
            pgBrush = New Drawing2D.PathGradientBrush(gPath)
            pgBrush.CenterColor = color1
            pgBrush.SurroundColors = New Color() {color2}
            graphics.FillEllipse(pgBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub ResizeImage(ByRef _image As Image, ByVal maxWidth As Integer, ByVal maxHeight As Integer, Optional ByVal usePadding As Boolean = False, Optional ByVal PaddingARGB As Integer = -16777216)
        Try
            If Not IsNothing(_image) Then
                Dim sPropPerc As Single = 1.0 'no default scaling

                If _image.Width > _image.Height Then
                    sPropPerc = CSng(maxWidth / _image.Width)
                Else
                    sPropPerc = CSng(maxHeight / _image.Height)
                End If

                ' Get the source bitmap.
                Using bmSource As New Bitmap(_image)
                    ' Make a bitmap for the result.
                    Dim bmDest As New Bitmap( _
                    Convert.ToInt32(bmSource.Width * sPropPerc), _
                    Convert.ToInt32(bmSource.Height * sPropPerc))
                    ' Make a Graphics object for the result Bitmap.
                    Using grDest As Graphics = Graphics.FromImage(bmDest)
                        grDest.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                        ' Copy the source image into the destination bitmap.
                        grDest.DrawImage(bmSource, New Rectangle(0, 0, _
                        bmDest.Width, bmDest.Height), New Rectangle(0, 0, _
                        bmSource.Width, bmSource.Height), GraphicsUnit.Pixel)
                    End Using

                    If usePadding Then
                        Dim bgBMP As Bitmap = New Bitmap(maxWidth, maxHeight)
                        Dim grOverlay As Graphics = Graphics.FromImage(bgBMP)
                        grOverlay.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                        grOverlay.FillRectangle(New SolidBrush(Color.FromArgb(PaddingARGB)), New RectangleF(0, 0, maxWidth, maxHeight))
                        Dim iLeft As Integer = Convert.ToInt32(If(bmDest.Width = maxWidth, 0, (maxWidth - bmDest.Width) / 2))
                        Dim iTop As Integer = Convert.ToInt32(If(bmDest.Height = maxHeight, 0, (maxHeight - bmDest.Height) / 2))
                        grOverlay.DrawImage(bmDest, iLeft, iTop, bmDest.Width, bmDest.Height)
                        bmDest = bgBMP
                    End If

                    _image = bmDest

                End Using

            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub ResizePB(ByRef pbResize As PictureBox, ByRef pbCache As PictureBox, ByVal maxHeight As Integer, ByVal maxWidth As Integer)
        '//
        ' Resize the picture box based on the dimensions of the image and the dimensions
        ' of the available space... try to use the most screen real estate
        '
        ' Why not use "Zoom" for fanart background? - To keep the image at the top. Zoom centers vertically.
        '\\

        Try
            If Not IsNothing(pbCache.Image) Then
                pbResize.SizeMode = PictureBoxSizeMode.Normal
                Dim sPropPerc As Single = 1.0 'no default scaling

                pbResize.Size = New Size(maxWidth, maxHeight)

                ' Height
                If pbCache.Image.Height > pbResize.Height Then
                    ' Reduce height first
                    sPropPerc = CSng(pbResize.Height / pbCache.Image.Height)
                End If

                ' Width
                If (pbCache.Image.Width * sPropPerc) > pbResize.Width Then
                    ' Scaled width exceeds Box's width, recalculate scale_factor
                    sPropPerc = CSng(pbResize.Width / pbCache.Image.Width)
                End If

                ' Get the source bitmap.
                Dim bmSource As New Bitmap(pbCache.Image)
                ' Make a bitmap for the result.
                Dim bmDest As New Bitmap( _
                Convert.ToInt32(bmSource.Width * sPropPerc), _
                Convert.ToInt32(bmSource.Height * sPropPerc))
                ' Make a Graphics object for the result Bitmap.
                Dim grDest As Graphics = Graphics.FromImage(bmDest)
                ' Copy the source image into the destination bitmap.
                grDest.DrawImage(bmSource, 0, 0, _
                bmDest.Width + 1, _
                bmDest.Height + 1)
                ' Display the result.
                pbResize.Image = bmDest

                'tweak pb after resizing pic
                pbResize.Width = pbResize.Image.Width
                pbResize.Height = pbResize.Image.Height
                'center it

                'Clean up
                bmSource = Nothing
                bmDest = Nothing
                grDest = Nothing
            Else
                pbResize.Left = 0
                pbResize.Size = New Size(maxWidth, maxHeight)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub SetGlassOverlay(ByRef pbUnderlay As PictureBox)
        '//
        ' Put our crappy glossy overlay over the poster
        '\\

        Try
            Dim bmOverlay As New Bitmap(pbUnderlay.Image)
            Dim grOverlay As Graphics = Graphics.FromImage(bmOverlay)
            Dim bmHeight As Integer = Convert.ToInt32(pbUnderlay.Image.Height * 0.65)

            grOverlay.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            grOverlay.DrawImage(My.Resources.overlay, 0, 0, pbUnderlay.Image.Width, bmHeight)
            pbUnderlay.Image = bmOverlay

            bmOverlay = New Bitmap(pbUnderlay.Image)
            grOverlay = Graphics.FromImage(bmOverlay)

            grOverlay.DrawImage(My.Resources.overlay2, 0, 0, pbUnderlay.Image.Width, pbUnderlay.Image.Height)
            pbUnderlay.Image = bmOverlay

            grOverlay.Dispose()
            bmOverlay = Nothing
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Function SetOverlay(ByRef imgUnderlay As Image, ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal imgOverlay As Image, ByVal Location As Integer) As Image
        'locations:
        '1 = upper left
        '2 = upper right
        '3 = lower left
        '4 = lower right
        Try
            ResizeImage(imgUnderlay, iWidth, iHeight, True, Color.Transparent.ToArgb)
            Dim bmOverlay As New Bitmap(imgUnderlay)
            Dim grOverlay As Graphics = Graphics.FromImage(bmOverlay)
            Dim iLeft As Integer = 0
            Dim iTop As Integer = 0

            Select Case Location
                Case 2
                    iLeft = bmOverlay.Width - imgOverlay.Width
                    iTop = 0
                Case 3
                    iLeft = 0
                    iTop = bmOverlay.Height - imgOverlay.Height
                Case 4
                    iLeft = bmOverlay.Width - imgOverlay.Width
                    iTop = bmOverlay.Height - imgOverlay.Height
                Case Else
                    iLeft = 0
                    iTop = 0
            End Select

            grOverlay.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            grOverlay.DrawImage(imgOverlay, iLeft, iTop, imgOverlay.Width, imgOverlay.Height)
            imgUnderlay = bmOverlay

            grOverlay.Dispose()
            bmOverlay = Nothing
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return imgUnderlay
    End Function

#End Region 'Methods

End Class