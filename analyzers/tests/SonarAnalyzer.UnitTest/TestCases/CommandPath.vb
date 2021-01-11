Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Security

Public Class Program

    Private CompliantField As String = "C:\file.exe"
    Private NoncompliantField As String = "file.exe"

    Private Field As Process = Process.Start("file.exe")                ' FN
    Public Property PropertyRW As Process = Process.Start("file.exe")   ' FN

    Public Sub Invocations(Password As SecureString, Arguments As IEnumerable(Of String))
        Dim CompliantVariable As String = "C:\file.exe"
        Dim NoncompliantVariable As String = "file.exe"
        Dim StartInfo As New ProcessStartInfo("bad.exe")    ' FN {{Make sure the "PATH" used To find this command includes only what you intend.}}
        'FIXME: Mark location

        ' Compliant
        Process.Start(StartInfo)       ' Not tracked here, it's already raised on the constructor
        Process.Start("C:\file.exe")
        Process.Start("C:\file.exe", Arguments)
        Process.Start("C:\file.exe", "arguments")
        Process.Start("C:\file.exe", "arguments", "userName", Password, "domain")
        Process.Start("C:\file.exe", "userName", Password, "domain")
        Process.Start(CompliantField)
        Process.Start(CompliantVariable)
        StartInfo = New ProcessStartInfo()
        StartInfo = New ProcessStartInfo("C:\file.exe")
        StartInfo = New ProcessStartInfo("C:\file.exe", "arguments")

        Process.Start("file.exe")                       ' FN
        Process.Start("file.exe", Arguments)            ' FN
        Process.Start("file.exe", "arguments")          ' FN
        Process.Start("file.exe", "arguments", "userName", Password, "domain")  ' FN
        Process.Start("file.exe", "userName", Password, "domain")               ' FN
        Process.Start(NoncompliantField)                ' FN
        Process.Start(NoncompliantVariable)             ' FN
        StartInfo = New ProcessStartInfo("file.exe")                ' FN
        StartInfo = New ProcessStartInfo("file.exe", "arguments")   ' FN

        ' Reassignment
        CompliantField = NoncompliantVariable
        Process.Start(CompliantField)                   ' FN
        NoncompliantVariable = CompliantField
        Process.Start(NoncompliantVariable)             ' Compliant after reassignment
    End Sub

    Public Sub Properties(Arg As ProcessStartInfo)
        Arg.FileName = "C:\file.exe"
        Arg.FileName = "file.exe"               ' FN

        Dim psi As New ProcessStartInfo("C:\file.exe")
        psi.FileName = "file.exe"               ' FN

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

        Process.Start("file")           ' FN
        Process.Start("file.exe")       ' FN
        Process.Start("File.bat")       ' FN
        Process.Start("dir/file.cmd")   ' FN
        Process.Start("-file.com")      ' FN
        Process.Start("@file.cpl")      ' FN
        Process.Start("$file.dat")      ' FN
        Process.Start(".file.txt")      ' FN
        Process.Start(".|file.fake")    ' FN
        Process.Start("~/file.exe")     ' FN
        Process.Start("...file.exe")    ' FN
        Process.Start(".../file.exe")   ' FN
        Process.Start("AA:/file.exe")   ' FN
        Process.Start("0:/file.exe")    ' FN
        Process.Start("Ř:/file.exe")    ' FN
        Process.Start("ř:/file.exe")    ' FN
        Process.Start("...\file.exe")   ' FN
        Process.Start("AA:\file.exe")   ' FN
        Process.Start("0:\file.exe")    ' FN
        Process.Start("Ř:\file.exe")    ' FN
        Process.Start("ř:\file.exe")    ' FN
    End Sub

End Class
