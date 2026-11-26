Imports System.Diagnostics
Imports System.Threading.Tasks

Public Class ProcessRunMethods

    Public Async Function Methods(StartInfo As ProcessStartInfo) As Task
        Process.Run("file.exe")                                                 ' Noncompliant
        Process.Run("C:\file.exe")
        Process.Run("file.exe", New String() {"arg"}, silent:=True)             ' Noncompliant
        Process.Run("C:\file.exe", New String() {"arg"}, silent:=True)
        Await Process.RunAsync("file.exe")                                      ' Noncompliant
        Await Process.RunAsync("C:\file.exe")
        Await Process.RunAsync("file.exe", New String() {"arg"})                ' Noncompliant
        Await Process.RunAsync("C:\file.exe", New String() {"arg"})
        Process.RunAndCaptureText("file.exe")                                   ' Noncompliant
        Process.RunAndCaptureText("/usr/bin/file")
        Process.RunAndCaptureText("file.exe", New String() {"arg"})             ' Noncompliant
        Process.RunAndCaptureText("/usr/bin/file", New String() {"arg"})
        Await Process.RunAndCaptureTextAsync("file.exe")                        ' Noncompliant
        Await Process.RunAndCaptureTextAsync("./file.exe")
        Await Process.RunAndCaptureTextAsync("file.exe", New String() {"arg"})  ' Noncompliant
        Await Process.RunAndCaptureTextAsync("./file.exe", New String() {"arg"})
        Process.StartAndForget("file.exe")                                      ' Noncompliant
        Process.StartAndForget("C:\file.exe")
        Process.StartAndForget("file.exe", New String() {"arg"})                ' Noncompliant
        Process.StartAndForget("C:\file.exe", New String() {"arg"})

        Process.Run(StartInfo)
        Await Process.RunAsync(StartInfo)
        Process.RunAndCaptureText(StartInfo)
        Await Process.RunAndCaptureTextAsync(StartInfo)
        Process.StartAndForget(StartInfo)
        Process.Run(New ProcessStartInfo("file.exe"))                           ' Noncompliant
        Process.Run(New ProcessStartInfo("C:\file.exe"))
    End Function

End Class
