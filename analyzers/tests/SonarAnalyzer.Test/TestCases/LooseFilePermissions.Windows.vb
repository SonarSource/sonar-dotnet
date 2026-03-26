Imports System.Security.AccessControl
Imports System.Security.Principal

Public Class S2612TestCasesWindows
    Const Everyone = "Everyone"

    Public Sub UpdatePermissionsUsingAccessControl(fileSecurity as FileSecurity, directorySecurity as DirectorySecurity)
            fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Deny))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ListDirectory, AccessControlType.Deny))
            fileSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ListDirectory, AccessControlType.Allow))
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant

            fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny))
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny))
            fileSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow))
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow)) ' Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read Or FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read Or FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.SetAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, AccessControlType.Allow)) ' Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule(new NTAccount("Everyone"), FileSystemRights.FullControl, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow)) ' Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(new NTAccount(Everyone), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow)) ' Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, Nothing), FileSystemRights.Modify, AccessControlType.Allow)) ' Noncompliant

            directorySecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Allow))
            directorySecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Allow))
            directorySecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Deny))
            directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Deny))
            directorySecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow))
            directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant {{Make sure this permission is safe.}}
            directorySecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)) ' Noncompliant {{Make sure this permission is safe.}}
    End Sub

    Public Sub InVariable(fileSecurity As FileSecurity)
        Dim unsafeAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow)
'                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary    {{Make sure this permission is safe.}}
'                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{Make sure this permission is safe.}}
        fileSecurity.RemoveAccessRule(unsafeAccessRule)

        fileSecurity.AddAccessRule(unsafeAccessRule) ' Noncompliant
        fileSecurity.SetAccessRule(unsafeAccessRule) ' Noncompliant

        Dim safeAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny)
        fileSecurity.AddAccessRule(safeAccessRule)
        fileSecurity.SetAccessRule(safeAccessRule)

        Dim rule = new FileSystemAccessRule("User", FileSystemRights.Read, AccessControlType.Allow)
        rule = new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow)

        fileSecurity.AddAccessRule(rule) ' Compliant - FN, we look only at the object creation and don't track state changes.
    End Sub

    Public Sub InsideTryCatch(fileSecurity As FileSecurity)
        Dim rule = new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow)
'                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
        Try
            fileSecurity.AddAccessRule(rule) ' Noncompliant
        Catch
        End Try
    End Sub
End Class
