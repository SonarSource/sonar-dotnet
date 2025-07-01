Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Security

Public Class Program

    Private CompliantField As String = "C:\file.exe"
    Private NoncompliantField As String = "file.exe"

    Private Field As Process = Process.Start("file.exe")                ' Noncompliant
    Public Property PropertyRW As Process = Process.Start("file.exe")   ' Noncompliant
    Public Property PropertyUnused As Process       ' For coverage

    Public Sub Invocations(Password As SecureString)
        Dim CompliantVariable As String = "C:\file.exe"
        Dim NoncompliantVariable As String = "file.exe"
        Dim NothingVariable As String = Nothing
        Dim StartInfo As New ProcessStartInfo("bad.exe")    ' Noncompliant {{Make sure the "PATH" variable only contains fixed, unwriteable directories.}}
        '                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        ' Compliant
        Process.Start(StartInfo)       ' Not tracked here, it's already raised on the constructor
        Process.Start("")
        Process.Start(NothingVariable)
        Process.Start("C:\file.exe")
        Process.Start("C:\file.exe", "arguments")
        Process.Start("C:\file.exe", "arguments", "userName", Password, "domain")
        Process.Start("C:\file.exe", "userName", Password, "domain")
        Process.Start(CompliantField)
        Process.Start(CompliantVariable)
        StartInfo = New ProcessStartInfo()
        StartInfo = New ProcessStartInfo("C:\file.exe")
        StartInfo = New ProcessStartInfo("C:\file.exe", "arguments")

        Process.Start("file.exe")                       ' Noncompliant
        Process.Start("file.exe", "arguments")          ' Noncompliant
        Process.Start("file.exe", "arguments", "userName", Password, "domain")  ' Noncompliant
        Process.Start("file.exe", "userName", Password, "domain")               ' Noncompliant
        Process.Start(NoncompliantField)                ' Noncompliant
        Process.Start(NoncompliantVariable)             ' Noncompliant
        StartInfo = New ProcessStartInfo("file.exe")                ' Noncompliant
        StartInfo = New ProcessStartInfo("file.exe", "arguments")   ' Noncompliant

        ' Reassignment
        CompliantField = NoncompliantVariable
        Process.Start(CompliantField)                   ' Noncompliant
        NoncompliantVariable = CompliantVariable
        Process.Start(NoncompliantVariable)             ' Compliant after reassignment
    End Sub

    Public Sub Properties(Arg As ProcessStartInfo)
        Arg.FileName = "C:\file.exe"
        Arg.FileName = "file.exe"               ' Noncompliant

        Dim psi As New ProcessStartInfo("C:\file.exe")
        psi.FileName = "file.exe"               ' Noncompliant

        psi = New ProcessStartInfo("bad.exe")   ' Noncompliant, later assignment is not tracked
        psi.FileName = "C:\file.exe"

        psi = New ProcessStartInfo() With {.FileName = "C:\file.exe"}
        psi = New ProcessStartInfo With {.FileName = "bad.exe"}     ' FN
        psi = New ProcessStartInfo() With {.FileName = "bad.exe"}   ' FN
        psi = New ProcessStartInfo() With {.FileName = "bad.exe"}   ' Compliant, reassigned later
        psi.FileName = "C:\file.exe"

        psi = New ProcessStartInfo() With {.FileName = "bad.exe"}   ' FN
        Process.Start(psi)
        psi.FileName = "C:\file.exe"

        psi = New ProcessStartInfo() With {.FileName = "bad.exe"}   ' FN
        Run(psi)
        psi.FileName = "C:\file.exe"
    End Sub

    Private Sub Run(Psi As ProcessStartInfo)
        Process.Start(Psi)
    End Sub

    Public Sub PathFormat()
        ' Compliant prefixes
        Process.Start("/file")
        Process.Start("/File")
        Process.Start("/dir/dir/dir/file")
        Process.Start("//////file")        ' Compliant, we don't validate the path format itself
        Process.Start("/file.exe")
        Process.Start("./file.exe")
        Process.Start("../file.exe")
        Process.Start("c:")
        Process.Start("c:/file.exe")
        Process.Start("C:/file.exe")
        Process.Start("D:/file.exe")
        Process.Start("//server/file.exe")
        Process.Start("//server/dir/file.exe")
        Process.Start("//server/c$/file.exe")
        Process.Start("//10.0.0.1/dir/file.exe")
        Process.Start("\file.exe")
        Process.Start("\\\\\\file")        ' Compliant, we don't validate the path format itself
        Process.Start(".\file.exe")
        Process.Start("..\file.exe")
        Process.Start("c:\file.exe")
        Process.Start("C:\file.exe")
        Process.Start("C:file.exe")       ' Missing "\" after drive letter: Valid relative path from current directory Of the C: drive
        Process.Start("C:Dir\file.exe")   ' Missing "\" after drive letter: Valid relative path from current directory Of the C: drive
        Process.Start("C:\dir\file.exe")
        Process.Start("D:\file.exe")
        Process.Start("z:\file.exe")
        Process.Start("Z:\file.exe")
        Process.Start("\\server\file.exe")
        Process.Start("\\server\dir\file.exe")
        Process.Start("\\server\c$\file.exe")
        Process.Start("\\10.0.0.1\dir\file.exe")
        Process.Start("http://www.microsoft.com")
        Process.Start("https://www.microsoft.com")
        Process.Start("ftp://www.microsoft.com")
        ' Compliant, custom protocols used to start an application
        Process.Start("skype:echo123?call")
        Process.Start("AA:/file.exe")
        Process.Start("Ř:/file.exe")
        Process.Start("ř:/file.exe")
        Process.Start("AA:\file.exe")
        Process.Start("Ř:\file.exe")
        Process.Start("ř:\file.exe")
        Process.Start("0:/file.exe")
        Process.Start("0:\file.exe")

        Process.Start("file")           ' Noncompliant
        Process.Start("file.exe")       ' Noncompliant
        Process.Start("File.bat")       ' Noncompliant
        Process.Start("dir/file.cmd")   ' Noncompliant
        Process.Start("-file.com")      ' Noncompliant
        Process.Start("@file.cpl")      ' Noncompliant
        Process.Start("$file.dat")      ' Noncompliant
        Process.Start(".file.txt")      ' Noncompliant
        Process.Start(".|file.fake")    ' Noncompliant
        Process.Start("~/file.exe")     ' Noncompliant
        Process.Start("...file.exe")    ' Noncompliant
        Process.Start(".../file.exe")   ' Noncompliant
        Process.Start("...\file.exe")   ' Noncompliant
    End Sub

End Class

Namespace CustomType

    Public Class ProcessStartInfo

        Public Property FileName As String

        Public Sub New(fileName As String)
        End Sub

        Public Shared Sub Usage()
            Dim psi As New ProcessStartInfo("bad.exe")  ' Compliant with this custom class
            psi.FileName = "file.exe"                   ' Compliant
        End Sub

    End Class

End Namespace
