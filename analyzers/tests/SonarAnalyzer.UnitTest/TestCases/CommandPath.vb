Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Security

Public Class Program

    Private CompliantField As String = "C:\file.exe"
    Private NoncompliantField As String = "file.exe"

    Private Field As Process = Process.Start("file.exe")                ' Noncompliant
    Public Property PropertyRW As Process = Process.Start("file.exe")   ' Noncompliant

    Public Sub Invocations(Password As SecureString)
        Dim CompliantVariable As String = "C:\file.exe"
        Dim NoncompliantVariable As String = "file.exe"
        Dim NothingVariable As String = Nothing
        Dim StartInfo As New ProcessStartInfo("bad.exe")    ' FN {{Make sure the "PATH" used To find this command includes only what you intend.}}
        'FIXME: Mark location

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
        StartInfo = New ProcessStartInfo("file.exe")                ' FN
        StartInfo = New ProcessStartInfo("file.exe", "arguments")   ' FN

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

        psi = New ProcessStartInfo("bad.exe")   ' Compliant, safe value Is assigned later
        psi.FileName = "C:\file.exe"

        psi = New ProcessStartInfo("bad.exe")   ' FN, safe value Is assigned later, but we don't track the intermediate usage
        Process.Start(psi)
        psi.FileName = "C:\file.exe"

        psi = New ProcessStartInfo("bad.exe")   ' FN, safe value Is assigned later, but we don't track the intermediate usage
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
        Process.Start("AA:/file.exe")   ' Noncompliant
        Process.Start("0:/file.exe")    ' Noncompliant
        Process.Start("Ř:/file.exe")    ' Noncompliant
        Process.Start("ř:/file.exe")    ' Noncompliant
        Process.Start("...\file.exe")   ' Noncompliant
        Process.Start("AA:\file.exe")   ' Noncompliant
        Process.Start("0:\file.exe")    ' Noncompliant
        Process.Start("Ř:\file.exe")    ' Noncompliant
        Process.Start("ř:\file.exe")    ' Noncompliant
    End Sub

End Class
