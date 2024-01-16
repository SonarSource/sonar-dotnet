Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text

Namespace ClassLibrary1
    Public Class Class1
        Public Sub ExtractArchive(ByVal archive As ZipArchive)
            For Each entry As ZipArchiveEntry In archive.Entries
                entry.ExtractToFile("") ' Noncompliant
'               ^^^^^^^^^^^^^^^^^^^^^^^
            Next

            For i As Integer = 0 To archive.Entries.Count - 1
                archive.Entries(i).ExtractToFile("") ' Noncompliant
            Next

            archive.Entries.ToList().ForEach(Sub(e) e.ExtractToFile("")) ' Noncompliant
'                                                   ^^^^^^^^^^^^^^^^^^^

            ZipFileExtensions.ExtractToDirectory(archive, "") ' Noncompliant
            archive.ExtractToDirectory("") ' Noncompliant
        End Sub

        Public Sub ExtractEntry(ByVal entry As ZipArchiveEntry)
            Dim fullName As String
            Dim stream As Stream

            entry.ExtractToFile("") ' Noncompliant
            entry.ExtractToFile("", True) ' Noncompliant

            ZipFileExtensions.ExtractToFile(entry, "") ' Noncompliant
            ZipFileExtensions.ExtractToFile(entry, "", True) ' Noncompliant

            ZipFile.ExtractToDirectory("", "") ' Noncompliant
            ZipFile.ExtractToDirectory("", "", Encoding.Default) ' Noncompliant

            stream = entry.Open() ' Compliant, method is not tracked

            entry.Delete() ' Compliant, method is not tracked

            fullName = entry.FullName ' Compliant, properties are not tracked

            ExtractToFile(entry) ' Compliant, method is not tracked
        End Sub

        Public Sub ExtractToFile(ByVal entry As ZipArchiveEntry)
        End Sub
    End Class
End Namespace
