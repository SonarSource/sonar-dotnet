Imports System
Imports System.Security
Imports System.Diagnostics
Imports P = System.Diagnostics.Process
Imports PSI = System.Diagnostics.ProcessStartInfo

Namespace Tests.Diagnostics
    Class Program
        Public Sub Foo(ByVal fileName As String, ByVal startInfo As ProcessStartInfo, ByVal process As Process)
            process.Start() ' Compliant, FileName is not passed here
            Process.Start(fileName) ' Noncompliant
            Process.Start(fileName, Nothing)  ' Noncompliant
            Process.Start(fileName, Nothing, Nothing, Nothing) ' Noncompliant
            Process.Start(fileName, Nothing, Nothing, Nothing, Nothing) ' Noncompliant

            Process.Start(startInfo) ' Compliant, the ProcessStartInfo's FileName has already been highlighted elsewhere

            startInfo.FileName = fileName ' Noncompliant
            process.StartInfo.FileName = fileName ' Noncompliant

            fileName = startInfo.FileName ' Compliant, the FileName is not set here
            fileName = process.StartInfo.FileName ' Compliant, the FileName is not set here

            Dim x = New ProcessStartInfo() ' Compliant, the FileName is set elsewhere
            x = New ProcessStartInfo(fileName) ' Noncompliant
            x = New ProcessStartInfo(fileName, Nothing) ' Noncompliant

            ' Different ways to specify the types
            System.Diagnostics.Process.Start(fileName) ' Noncompliant
            P.Start(fileName) ' Noncompliant
            x = New System.Diagnostics.ProcessStartInfo(fileName) ' Noncompliant
            x = New PSI(fileName) ' Noncompliant
        End Sub
    End Class
End Namespace
