Imports System.Windows.Forms
Imports System
Imports System.IO
Imports System.Text.RegularExpressions
Imports EmberAPI

Public Class frmMovieExtractor
    Private PreviousFrameValue As Integer

    Event GenericEvent(ByVal mType As EmberAPI.Enums.ModuleEventType, ByRef _params As System.Collections.Generic.List(Of Object))

	Private Sub btnFrameLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFrameLoad.Click
		Try
			Using ffmpeg As New Process()

				ffmpeg.StartInfo.FileName = Functions.GetFFMpeg
				ffmpeg.StartInfo.Arguments = String.Format("-ss 0 -i ""{0}"" -an -f rawvideo -vframes 1 -s 1280x720 -vcodec mjpeg -y ""{1}""", Master.currMovie.Filename, Path.Combine(Master.TempPath, "frame.jpg"))
				ffmpeg.EnableRaisingEvents = False
				ffmpeg.StartInfo.UseShellExecute = False
				ffmpeg.StartInfo.CreateNoWindow = True
				ffmpeg.StartInfo.RedirectStandardOutput = True
				ffmpeg.StartInfo.RedirectStandardError = True
				ffmpeg.Start()
				Using d As StreamReader = ffmpeg.StandardError

					Do
						Dim s As String = d.ReadLine()
						If s.Contains("Duration: ") Then
							Dim sTime As String = Regex.Match(s, "Duration: (?<dur>.*?),").Groups("dur").ToString
							If Not sTime = "N/A" Then
								Dim ts As TimeSpan = CDate(CDate(String.Format("{0} {1}", DateTime.Today.ToString("d"), sTime))).Subtract(CDate(DateTime.Today))
								Dim intSeconds As Integer = ((ts.Hours * 60) + ts.Minutes) * 60 + ts.Seconds
								tbFrame.Maximum = intSeconds
							Else
								tbFrame.Maximum = 0
							End If
							tbFrame.Value = 0
							tbFrame.Enabled = True
						End If
					Loop While Not d.EndOfStream
				End Using
				ffmpeg.WaitForExit()
				ffmpeg.Close()
			End Using

			If tbFrame.Maximum > 0 AndAlso File.Exists(Path.Combine(Master.TempPath, "frame.jpg")) Then
				Using fsFImage As New FileStream(Path.Combine(Master.TempPath, "frame.jpg"), FileMode.Open, FileAccess.Read)
					pbFrame.Image = Image.FromStream(fsFImage)
				End Using
				btnFrameLoad.Enabled = False
				btnFrameSave.Enabled = True
			Else
				tbFrame.Maximum = 0
				tbFrame.Value = 0
				tbFrame.Enabled = False
				pbFrame.Image = Nothing
			End If
			PreviousFrameValue = 0

		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
			tbFrame.Maximum = 0
			tbFrame.Value = 0
			tbFrame.Enabled = False
			pbFrame.Image = Nothing
		End Try
	End Sub
	Private Sub tbFrame_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles tbFrame.KeyUp
		If tbFrame.Value <> PreviousFrameValue Then
			GrabTheFrame()
		End If
	End Sub

	Private Sub tbFrame_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tbFrame.MouseUp
		If tbFrame.Value <> PreviousFrameValue Then
			GrabTheFrame()
		End If
	End Sub
	Private Sub tbFrame_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbFrame.Scroll
		Try
			Dim sec2Time As New TimeSpan(0, 0, tbFrame.Value)
			lblTime.Text = String.Format("{0}:{1:00}:{2:00}", sec2Time.Hours, sec2Time.Minutes, sec2Time.Seconds)

		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
		End Try
	End Sub

	Private Sub GrabTheFrame()
		Try

			tbFrame.Enabled = False
			Dim ffmpeg As New Process()

			ffmpeg.StartInfo.FileName = Functions.GetFFMpeg
			'ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -an -f rawvideo -vframes 1 -vcodec mjpeg -y -pix_fmt ""yuvj420p"" ""{2}""", tbFrame.Value, Master.currMovie.Filename, Path.Combine(Master.TempPath, "frame.jpg"))
			ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -vframes 1 -y ""{2}""", tbFrame.Value, Master.currMovie.Filename, Path.Combine(Master.TempPath, "frame.jpg"))
			ffmpeg.EnableRaisingEvents = False
			ffmpeg.StartInfo.UseShellExecute = False
			ffmpeg.StartInfo.CreateNoWindow = True
			ffmpeg.StartInfo.RedirectStandardOutput = False
			ffmpeg.StartInfo.RedirectStandardError = False

			pnlFrameProgress.Visible = True
			btnFrameSave.Enabled = False

			ffmpeg.Start()

			ffmpeg.WaitForExit()
			ffmpeg.Close()

			If File.Exists(Path.Combine(Master.TempPath, "frame.jpg")) Then
				Using fsFImage As FileStream = New FileStream(Path.Combine(Master.TempPath, "frame.jpg"), FileMode.Open, FileAccess.Read)
					pbFrame.Image = Image.FromStream(fsFImage)
				End Using
				tbFrame.Enabled = True
				btnFrameSave.Enabled = True
				pnlFrameProgress.Visible = False
				PreviousFrameValue = tbFrame.Value
			Else
				lblTime.Text = String.Empty
				tbFrame.Maximum = 0
				tbFrame.Value = 0
				tbFrame.Enabled = False
				btnFrameSave.Enabled = False
				btnFrameLoad.Enabled = True
				pbFrame.Image = Nothing
				pnlFrameProgress.Visible = False
				PreviousFrameValue = tbFrame.Value
			End If

		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error", False)
			PreviousFrameValue = 0
			lblTime.Text = String.Empty
			tbFrame.Maximum = 0
			tbFrame.Value = 0
			tbFrame.Enabled = False
			btnFrameSave.Enabled = False
			btnFrameLoad.Enabled = True
			pbFrame.Image = Nothing
		End Try
	End Sub

	Private Sub btnFrameSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFrameSave.Click
		Try
			Dim tPath As String = Path.Combine(Master.TempPath, "extrathumbs")

			If Not Directory.Exists(tPath) Then
				Directory.CreateDirectory(tPath)
			End If

			Dim iMod As Integer = Functions.GetExtraModifier(tPath)

			Dim exImage As New Images
			exImage.ResizeExtraThumb(Path.Combine(Master.TempPath, "frame.jpg"), Path.Combine(tPath, String.Concat("thumb", (iMod + 1), ".jpg")))
			exImage.Dispose()
			exImage = Nothing

		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
		End Try

		btnFrameSave.Enabled = False
	End Sub

	Private Sub DelayTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DelayTimer.Tick
		DelayTimer.Stop()
		GrabTheFrame()
	End Sub

	Private Sub btnAutoGen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAutoGen.Click
		Try
			If Convert.ToInt32(txtThumbCount.Text) > 0 Then
				pnlFrameProgress.Visible = True
				Me.Refresh()
				ThumbGenerator.CreateRandomThumbs(Master.currMovie, Convert.ToInt32(txtThumbCount.Text), True)
				pnlFrameProgress.Visible = False
				'Me.RefreshExtraThumbs()
				RaiseEvent GenericEvent(Enums.ModuleEventType.MovieFrameExtrator, Nothing)
			End If
		Catch ex As Exception
			Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
		End Try
	End Sub

	Private Sub txtThumbCount_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtThumbCount.GotFocus
		Me.AcceptButton = Me.btnAutoGen
	End Sub
	Private Sub txtThumbCount_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtThumbCount.TextChanged
		btnAutoGen.Enabled = Not String.IsNullOrEmpty(txtThumbCount.Text)
	End Sub


	Private Sub frmMovieExtrator_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		If Master.eSettings.AutoThumbs > 0 Then
			txtThumbCount.Text = Master.eSettings.AutoThumbs.ToString
		End If

	End Sub

	Public Sub SetUp()
		Me.GroupBox1.Text = Master.eLang.GetString(1, "Auto-Generate")
		Me.Label5.Text = Master.eLang.GetString(2, "# to Create:")
		Me.btnAutoGen.Text = Master.eLang.GetString(3, "Auto-Gen")
		Me.btnFrameSave.Text = Master.eLang.GetString(4, "Save Extrathumb")
		Me.Label3.Text = Master.eLang.GetString(5, "Extracting Frame...")
		Me.btnFrameLoad.Text = Master.eLang.GetString(6, "Load Movie")

	End Sub

	Public Sub New()

		' This call is required by the designer.
		InitializeComponent()
		Me.SetUp()
		' Add any initialization after the InitializeComponent() call.

	End Sub
End Class
