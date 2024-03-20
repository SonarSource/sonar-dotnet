Imports System.IO
Module clsUtils
    Public Sub Log(ByVal txt As String, ByVal logpath As String)
        Dim sw As StreamWriter

        Dim logfile As String = Path.Combine(logpath, "log.txt")
        If Not File.Exists(logfile) Then
            sw = File.CreateText(logfile)
        Else
            sw = File.AppendText(logfile)
        End If
        sw.WriteLine(Now.ToShortDateString & " " & Now.ToShortTimeString & ": " & txt)
        sw.Flush()
        sw.Close()
    End Sub
End Module
