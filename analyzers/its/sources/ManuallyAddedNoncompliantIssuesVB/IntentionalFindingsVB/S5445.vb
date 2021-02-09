Imports System.IO

Public Class S5445
    Public Sub Noncompliant()
        Dim TempPath = Path.GetTempFileName() 'Noncompliant (S5445)

        Using Writer As New StreamWriter(TempPath)
            Writer.WriteLine("content")
        End Using
    End Sub

    Public Sub Compliant()
        Dim RandomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())

        Using FileStream As new FileStream(RandomPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.DeleteOnClose)
            Using Writer As New StreamWriter(FileStream)
                Writer.WriteLine("content")
            End Using
        End Using
    End Sub
End Class
