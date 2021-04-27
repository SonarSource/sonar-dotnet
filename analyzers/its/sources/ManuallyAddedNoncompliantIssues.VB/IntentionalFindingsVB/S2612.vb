Imports System.Security.AccessControl

Public Class S2612
    Public Sub TestCases(fileSecurity As FileSecurity, directorySecurity As DirectorySecurity)
        fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow))

        fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant (S2612) {{Make sure this permission is safe.}}

        directorySecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow))

        directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant
    End Sub
End Class
